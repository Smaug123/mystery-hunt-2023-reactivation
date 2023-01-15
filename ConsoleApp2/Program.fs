namespace Solve

module Program =

    let words =
        [
            "LEAD"
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
            "CARD"
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
        //if not (restrict board) then [] else
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
        printfn "Before filtering, %i options" after.Length
        after.[7000] |> List.iter print
        let filtered =
            after
            |> List.filter (fun positions ->
                let (endX, endY) = fst (List.last positions)
                endY = 0 && (abs endX = 1)
            )
        printfn "%i total options" filtered.Length
        0