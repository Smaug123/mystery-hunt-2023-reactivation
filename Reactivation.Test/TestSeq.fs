namespace Reactivation.Test

open FsUnitTyped
open NUnit.Framework
open Reactivation
open FsCheck

[<TestFixture>]
module TestSeq =

    [<Test>]
    let ``Seq.range works`` () : unit =
        Seq.rangeOrZero Seq.empty |> shouldEqual 0

        let prop (i : int) (s : int list) =
            let s = i :: s
            Seq.rangeOrZero s
            |> (=) (Seq.max s - Seq.min s)

        prop
        |> Check.QuickThrowOnFailure