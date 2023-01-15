namespace Solve.Test

open FsUnitTyped
open NUnit.Framework
open Solve
open FsCheck

[<TestFixture>]
module TestSeq =

    [<Test>]
    let ``Seq.range works`` () : unit =
        Seq.range Seq.empty |> shouldEqual 0

        let prop (i : int) (s : int list) =
            let s = i :: s
            Seq.range s
            |> (=) (Seq.max s - Seq.min s)

        prop
        |> Check.QuickThrowOnFailure