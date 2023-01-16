namespace Reactivation

open System.Collections.Immutable

type BondSet = private | BondSet of ((int * int) * (int * int)) ImmutableHashSet

[<RequireQualifiedAccess>]
module BondSet =
    let sort a b = if a < b then (a, b) else (b, a)

    let addIfOk ((sourceX, sourceY) as source) ((destX, destY) as dest) (BondSet bonds) : BondSet option =
        let distance = abs (sourceX - destX) + abs (sourceY - destY)

        if distance = 2 then
            // Check the other
            if sourceX < destX && sourceY < destY then
                if bonds.Contains (sort (sourceX + 1, sourceY) (sourceX, sourceY + 1)) then
                    None
                else
                    bonds.Add (sort source dest) |> BondSet |> Some
            elif sourceX > destX && sourceY > destY then
                if bonds.Contains (sort (sourceX - 1, sourceY) (sourceX, sourceY - 1)) then
                    None
                else
                    bonds.Add (sort source dest) |> BondSet |> Some
            elif sourceX < destX then
                if bonds.Contains (sort (sourceX, sourceY - 1) (sourceX + 1, sourceY)) then
                    None
                else
                    bonds.Add (sort source dest) |> BondSet |> Some
            else if bonds.Contains (sort (sourceX - 1, sourceY) (sourceX, sourceY + 1)) then
                None
            else
                bonds.Add (sort source dest) |> BondSet |> Some

        else
            if distance <> 1 then
                failwith "bad assumption"

            bonds.Add (sort source dest) |> BondSet |> Some

    let empty = BondSet ImmutableHashSet.Empty

    let directionList (BondSet s) =
        let rec go (acc : _ list) (start : int * int) (s : ImmutableHashSet<_>) =
            if s.IsEmpty then
                List.rev (start :: acc)
            else
                let next, toRem =
                    s
                    |> Seq.choose (fun (p1, p2) ->
                        if p1 = start then Some (p2, (p1, p2))
                        elif p2 = start then Some (p1, (p1, p2))
                        else None
                    )
                    |> Seq.exactlyOne

                go (start :: acc) next (s.Remove toRem)

        go [] (0, 0) s
