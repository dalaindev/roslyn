﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.IO
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.SpecialType
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.OverloadResolution
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.UnitTests.Emit
Imports Roslyn.Test.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests.Semantics

    Public Class InterpolatedStringTests
        Inherits BasicTestBase

        <Fact>
        Sub SimpleInterpolation()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim number = 8675309
        Write($"Jenny: {number}")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="Jenny: 8675309")

        End Sub

        <Fact>
        Sub InterpolationWithAlignment()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim number = 8675309
        Write($"Jenny: {number,12}")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="Jenny:      8675309")

        End Sub

        <Fact>
        Sub InterpolationWithFormat()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim number = 8675309
        Write($"Jenny: {number:###-####}")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="Jenny: 867-5309")

        End Sub

        <Fact>
        Sub InterpolationWithFormatAndAlignment()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim number = 8675309
        Write($"Jenny: {number,12:###-####}")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="Jenny:     867-5309")

        End Sub

        <Fact>
        Sub TwoInterpolations()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim Hello = "Goodbye", World = "No one"
        Write($"This is a ""{NameOf(Hello)}, {NameOf(World)}!"" program.")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="This is a ""Hello, World!"" program.")

        End Sub

        <Fact>
        Sub EscapeSequences()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim arr As Object() = {}
        Write($"Solution: {{ { If(arr.Length > 0, String.Join("", "", arr), "Ø") } }}")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="Solution: { Ø }")

        End Sub

        <Fact>
        Sub NestedInterpolations()

            Dim verifier = CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Write($"The date/time is {$"{#2014-12-18 09:00:00#:yyyy-MM-dd HH:mm:ss}"}.")
    End Sub
End Module
    </file>
</compilation>, expectedOutput:="The date/time is 2014-12-18 09:00:00.")

        End Sub

        <Fact>
        Sub MissingStringFormat()

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntimeAndReferences(
<compilation>
    <file name="a.vb">
Imports System.Console

Module Program
    Sub Main()
        Dim obj As Object = Nothing
        Write($"{obj}.")
    End Sub
End Module
    </file>
</compilation>)
            compilation.MakeMemberMissing(SpecialMember.System_String__Format)

            AssertTheseEmitDiagnostics(compilation,
<expected>
BC35000: Requested operation is not available because the runtime library function 'System.String.Format' is not defined.
        Write($"{obj}.")
              ~~~~~~~~~
</expected>)

        End Sub

        <Fact, WorkItem(1102783)>
        Sub SmartQuotes()

            CompileAndVerify(
<compilation>
    <file name="a.vb">
Module Program
    Sub Main()

        Dim arr = {
            $“1”,
            $”2“,
            $“3",
            $"4“,
            $"5”,
            $”6",
            $" ”“ ",
            $” {1:x”“y} “
        }

        System.Console.WriteLine(String.Join("", arr))

    End Sub
End Module    </file>
</compilation>, expectedOutput:="123456 ""  xy")

        End Sub

        <Fact, WorkItem(1102800)>
        Sub FullwidthDelimiters()

            ' Any combination of fullwidth and ASCII curly braces of the same direction is an escaping sequence for the corresponding ASCII curly brace.
            ' We insert that curly brace doubled and because this is the escaping sequence understood by String.Format, that will be replaced by a single brace.
            ' This is deliberate design and it aligns with existing rules for double quote escaping in strings.
            CompileAndVerify(
<compilation>
    <file name="a.vb">
Imports System.Console
Module Program
    Sub Main()
        WriteLine($"{0｝" = "0")
        WriteLine($"｛10：X}" = "A")

        WriteLine($"}}" = "}")
        WriteLine($"}｝" = "}")
        WriteLine($"｝}" = "}")
        WriteLine($"｝｝" = "}")

        WriteLine($"{{" = "{")
        WriteLine($"{｛" = "{")
        WriteLine($"｛{" = "{")
        WriteLine($"｛｛" = "{")

        WriteLine(＄＂““" = """")
    End Sub
End Module</file>
</compilation>, expectedOutput:="True
True
True
True
True
True
True
True
True
True
True")

        End Sub

    End Class

End Namespace