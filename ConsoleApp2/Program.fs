namespace Solve

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
                [1..4]
                |> List.filter (fun i ->
                    start.[i - 1] <> next.[i - 1]
                )
                |> List.exactlyOne
            start, instruction
        )

    let restrict (board : Map<int * int, string>) =
        if Seq.range (board.Keys |> Seq.map snd) > 5 then false
        elif Seq.range (board.Keys |> Seq.map fst) > 7 then false
        else true

    let rec go
        (currX : int)
        (currY : int)
        (bonds : BondSet)
        (board : Map<int * int, string>)
        (instructions : (string * int) list)
        : _ list
        =
        if not (restrict board) then [] else
        match instructions with
        | [] -> [board, BondSet.directionList bonds]
        | (word, i) :: rest ->
            // Place this word.
            let newBoard =
                board
                |> Map.add (currX, currY) word

            match i with
            | 1 ->
                // horizontal, i.e. change X
                [
                    match Map.tryFind (currX + 1, currY) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX + 1, currY) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX + 1) currY bonds newBoard rest
                    | Some _ -> ()
                    match Map.tryFind (currX - 1, currY) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX - 1, currY) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX - 1) currY bonds newBoard rest
                    | Some _ -> ()
                ]
            | 2 ->
                // vertical, i.e. change Y
                [
                    match Map.tryFind (currX, currY + 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX, currY + 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go currX (currY + 1) bonds newBoard rest
                    | Some _ -> ()
                    match Map.tryFind (currX, currY - 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX, currY - 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go currX (currY - 1) bonds newBoard rest
                    | Some _ -> ()
                ]
            | 3 ->
                // Bottom left to top right
                [
                    match Map.tryFind (currX + 1, currY + 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX + 1, currY + 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX + 1) (currY + 1) bonds newBoard rest
                    | Some _ -> ()
                    match Map.tryFind (currX - 1, currY - 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX - 1, currY - 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX - 1) (currY - 1) bonds newBoard rest
                    | Some _ -> ()
                ]
            | 4 ->
                // Top left to bottom right
                [
                    match Map.tryFind (currX - 1, currY + 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX - 1, currY + 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX - 1) (currY + 1) bonds newBoard rest
                    | Some _ -> ()
                    match Map.tryFind (currX + 1, currY - 1) newBoard with
                    | None ->
                        match bonds |> BondSet.addIfOk (currX + 1, currY - 1) (currX, currY) with
                        | None -> ()
                        | Some bonds ->
                            yield! go (currX + 1) (currY - 1) bonds newBoard rest
                    | Some _ -> ()
                ]
            | _ -> failwith "bad direction"

    let print ((x, y), s) =
        printfn "%i, %i: %s" x y s

    [<EntryPoint>]
    let main _ =
        let after =
            instructions
            |> go 0 0 BondSet.empty Map.empty
            |> List.map (fun (examplePlacement, exampleBonds) ->
                let munged =
                    exampleBonds
                    |> List.choose (fun (srcX, srcY) ->
                        match examplePlacement.TryFind (srcX, srcY) with
                        | Some w ->
                            Some ((srcX, srcY), w)
                        | None ->
                            None
                    )
                munged
            )
            |> List.distinct
        printfn "Before filtering, %i options" after.Length

        let after =
            after
            |> List.sortBy (fun l ->
                let (x, y), _ = List.last l
                abs x + abs y
            )

        if after.Length = 0 then 1 else

        let l = after.[1]
        let positions = l |> List.map fst
        let minX = positions |> List.map fst |> List.min
        let maxX = positions |> List.map fst |> List.max
        let minY = positions |> List.map snd |> List.min
        let maxY = positions |> List.map snd |> List.max
        let arr = Array2D.zeroCreate (maxY - minY + 1) (maxX - minX + 1)

        let mutable i = 0
        for x, y in positions do
            if i >= instructions.Length then
                arr.[y - minY, x - minX] <- ValueSome 'M'
            else
                arr.[y - minY, x - minX] <- ValueSome words.[i].[snd instructions.[i] - 1]
            i <- i + 1

        for row in maxY .. -1 .. minY do
            for col in minX..maxX do
                match arr.[row - minY, col - minX] with
                | ValueNone -> printf "."
                | ValueSome c -> printf "O"
            printfn ""

        0