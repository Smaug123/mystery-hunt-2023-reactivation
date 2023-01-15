namespace Solve

[<RequireQualifiedAccess>]
module Seq =

    let range (s : int seq) : int =
        use e = s.GetEnumerator ()
        if not (e.MoveNext ()) then 0 else

        let mutable min = e.Current
        let mutable max = e.Current
        while e.MoveNext () do
            if e.Current < min then min <- e.Current
            if e.Current > max then max <- e.Current
        max - min

