namespace Reactivation

open System.Collections.Generic
open System.Collections.Immutable

type Instruction =
    | Horizontal = 1
    | Vertical = 2
    | UpAndRight = 3
    | DownAndRight = 4

module Program =

    let words =
        [
            "LEAD"
            "READ"
            "BEAD"
            "BEAR"
            "BEAT"
            "BOAT"
            "BOOT"
            "BOLT"
            "COLT"
            "CULT"
            "CURT"
            "CART"
            "CARE"
            "DARE"
            "DANE"
            "DONE"
            "DONG"
            "DING"
            "MING"
            "MINX"
            "MIND"
            "MEND"
            "MELD"
            "MEAD"
        ]

    let instructions =
        words @ [ List.head words ]
        |> List.pairwise
        |> List.map (fun (start, next) ->
            let instruction =
                [ 1..4 ]
                |> List.filter (fun i -> start.[i - 1] <> next.[i - 1])
                |> List.exactlyOne
                |> enum<Instruction>

            start, instruction
        )

    let restrict (board : IReadOnlyDictionary<int * int, string>) =
        if Seq.rangeOrZero (board.Keys |> Seq.map snd) > 5 then
            false
        elif Seq.rangeOrZero (board.Keys |> Seq.map fst) > 7 then
            false
        else
            true

    let rec go
        (currX : int)
        (currY : int)
        (bonds : BondSet)
        (board : ImmutableDictionary<int * int, string>)
        (instructions : (string * Instruction) list)
        : _ list
        =
        if not (restrict board) then
            []
        else
            match instructions with
            | [] -> [ board, BondSet.directionList bonds ]
            | (word, i) :: rest ->
                // Place this word.
                let newBoard = board.Add ((currX, currY), word)

                match i with
                | Instruction.Horizontal ->
                    // horizontal, i.e. change X
                    [
                        if not (newBoard.ContainsKey (currX + 1, currY)) then
                            match bonds |> BondSet.addIfOk (currX + 1, currY) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX + 1) currY bonds newBoard rest

                        if not (newBoard.ContainsKey (currX - 1, currY)) then
                            match bonds |> BondSet.addIfOk (currX - 1, currY) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX - 1) currY bonds newBoard rest
                    ]
                | Instruction.Vertical ->
                    // vertical, i.e. change Y
                    [
                        if not (newBoard.ContainsKey (currX, currY + 1)) then
                            match bonds |> BondSet.addIfOk (currX, currY + 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go currX (currY + 1) bonds newBoard rest
                        if not (newBoard.ContainsKey (currX, currY - 1)) then
                            match bonds |> BondSet.addIfOk (currX, currY - 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go currX (currY - 1) bonds newBoard rest
                    ]
                | Instruction.UpAndRight ->
                    [
                        if not (newBoard.ContainsKey (currX + 1, currY + 1)) then
                            match bonds |> BondSet.addIfOk (currX + 1, currY + 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX + 1) (currY + 1) bonds newBoard rest
                        if not (newBoard.ContainsKey (currX - 1, currY - 1)) then
                            match bonds |> BondSet.addIfOk (currX - 1, currY - 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX - 1) (currY - 1) bonds newBoard rest
                    ]
                | Instruction.DownAndRight ->
                    [
                        if not (newBoard.ContainsKey (currX - 1, currY + 1)) then
                            match bonds |> BondSet.addIfOk (currX - 1, currY + 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX - 1) (currY + 1) bonds newBoard rest
                        if not (newBoard.ContainsKey (currX + 1, currY - 1)) then
                            match bonds |> BondSet.addIfOk (currX + 1, currY - 1) (currX, currY) with
                            | None -> ()
                            | Some bonds -> yield! go (currX + 1) (currY - 1) bonds newBoard rest
                    ]
                | _ -> failwith "bad direction"

    let print ((x, y), s) = printfn "%i, %i: %s" x y s

    [<EntryPoint>]
    let main _ =
        let sw = System.Diagnostics.Stopwatch.StartNew ()

        let after =
            instructions
            |> go 0 0 BondSet.empty ImmutableDictionary.Empty
            |> List.map (fun (examplePlacement, exampleBonds) ->
                let munged =
                    exampleBonds
                    |> List.choose (fun (srcX, srcY) ->
                        match examplePlacement.TryGetValue ((srcX, srcY)) with
                        | true, w -> Some ((srcX, srcY), w)
                        | false, _ -> None
                    )

                munged
            )

        let positions =
            after
            |> List.minBy (fun l ->
                let (x, y), _ = List.last l
                abs x + abs y
            )
            |> List.map fst

        sw.Stop ()
        printfn "%i" sw.ElapsedMilliseconds

        let struct (minX, maxX) = positions |> Seq.map fst |> Seq.minMax |> ValueOption.get
        let struct (minY, maxY) = positions |> Seq.map snd |> Seq.minMax |> ValueOption.get
        let arr = Array2D.zeroCreate (maxY - minY + 1) (maxX - minX + 1)

        let mutable i = 0

        for x, y in positions do
            arr.[y - minY, x - minX] <- words.[i].[int (snd instructions.[i]) - 1]
            i <- i + 1

        for row in maxY .. -1 .. minY do
            for col in minX..maxX do
                match arr.[row - minY, col - minX] with
                | '\000' -> printf "."
                | _ -> printf "O"

            printfn ""

        0
