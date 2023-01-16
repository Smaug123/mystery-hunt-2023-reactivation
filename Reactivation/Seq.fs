namespace Reactivation

[<RequireQualifiedAccess>]
module Seq =

    let minMax (s : int seq) : struct (int * int) voption =
        use e = s.GetEnumerator ()
        if not (e.MoveNext ()) then ValueNone else

        let mutable min = e.Current
        let mutable max = e.Current
        while e.MoveNext () do
            if e.Current < min then min <- e.Current
            if e.Current > max then max <- e.Current
        ValueSome (struct (min, max))

    let rangeOrZero (s : int seq) : int =
        match minMax s with
        | ValueNone -> 0
        | ValueSome (min, max) -> max - min