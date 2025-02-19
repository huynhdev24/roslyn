﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.RemoveUnnecessaryCast

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.RemoveUnnecessaryCast
    <Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryCast)>
    Partial Public Class RemoveUnnecessaryCastTests
        Inherits AbstractVisualBasicDiagnosticProviderBasedUserDiagnosticTest

        Friend Overrides Function CreateDiagnosticProviderAndFixer(workspace As Workspace) As (DiagnosticAnalyzer, CodeFixProvider)
            Return (New VisualBasicRemoveUnnecessaryCastDiagnosticAnalyzer(),
                    New VisualBasicRemoveUnnecessaryCastCodeFixProvider())
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545979")>
        Public Async Function TestDontRemoveCastToErrorType() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Dim x = [|CType(0, ErrorType)|]
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact>
        Public Async Function TestDontRemoveCastSimpleArgument1() As Task
            Dim markup =
<File>
Option Strict On
Imports System.Drawing
Module M
    Sub Main()
       ' test PredefinedCastExpressionSyntax and WalkDownParentheses
        Dim x As New Point([|CInt((System.Math.Floor(1.1)))|], 1)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact>
        Public Async Function TestDontRemoveCastSimpleArgument2() As Task
            Dim markup =
<File>
Option Strict On    
Imports System.Drawing
Module M
    Sub Main()
      ' test CastExpressionSyntax
       Dim y As New Point([|CType(1.1, Integer)|], 1)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545148")>
        Public Async Function TestParenthesizeToKeepParseTheSame1() As Task
            Dim markup =
<File>
Imports System.Collections
Imports System.Linq

Module Program
    Sub Main
        Dim a = CType([|CObj(From x In "" Select x)|], IEnumerable)
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System.Collections
Imports System.Linq

Module Program
    Sub Main
        Dim a = CType((From x In "" Select x), IEnumerable)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530762")>
        Public Async Function TestParenthesizeToKeepParseTheSame2() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim x = 0 &lt; [|CInt(&lt;x/&gt;.GetHashCode)|]
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        Dim x = 0 &lt; (&lt;x/&gt;.GetHashCode)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530762")>
        Public Async Function TestParenthesizeToKeepParseTheSame3() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim x = 0 &lt; [|CInt(&lt;x/&gt;.GetHashCode)|]
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        Dim x = 0 &lt; (&lt;x/&gt;.GetHashCode)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545149")>
        Public Async Function TestInsertCallKeywordIfNecessary1() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        [|CInt(1)|].ToString
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        Call 1.ToString
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545150")>
        Public Async Function TestInsertCallKeywordIfNecessary2() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        [|CStr(Mid())|].GetType
    End Sub
    Function Mid() As String
    End Function
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        [Mid]().GetType
    End Sub
    Function Mid() As String
    End Function
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsInlineTemporary)>
        <WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545229")>
        Public Async Function TestInsertCallKeywordIfNecessary3() As Task
            Dim code =
<File>
Imports System
Class C1
    Sub M()
#If True Then
        [|CInt(1)|].ToString()
#End If
    End Sub
End Class
</File>

            Dim expected =
<File>
Imports System
Class C1
    Sub M()
#If True Then
        Call 1.ToString()
#End If
    End Sub
End Class
</File>

            Await TestAsync(code, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545528")>
        <WorkItem(16488, "DevDiv_Projects/Roslyn")>
        Public Async Function TestAddExplicitArgumentListIfNecessary1() As Task
            Dim markup =
<File>
Imports System
Module Program
    Sub Main()
        Dim x As Action = Sub() Console.WriteLine("Hello")
        [|CType(x, Action)|] : Console.WriteLine()
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module Program
    Sub Main()
        Dim x As Action = Sub() Console.WriteLine("Hello")
        x() : Console.WriteLine()
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545134")>
        Public Async Function TestDontRemoveConversionFromNullableLongToIComparable() As Task
            Dim markup =
<File>
Option Strict On

Class M
    Sub Main()
        Dim y As System.IComparable(Of Long) = [|CType(1, Long?)|]
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545151")>
        Public Async Function TestDontRemoveArrayLiteralConversion() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim x As Object = [|CType({1}, Long())|]
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545152")>
        Public Async Function TestDontRemoveAddressOfCastToDelegate() As Task
            Dim markup =
<File>
Imports System

Module Program
    Sub Main()
        Dim x As Object = [|CType(AddressOf Console.WriteLine, Action)|]
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545311")>
        Public Async Function TestRemoveUnneededCastInLambda1() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim f As Func(Of Long) = Function() [|CLng(5)|]
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        Dim f As Func(Of Long) = Function() 5
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545311")>
        Public Async Function TestRemoveUnneededCastInLambda2() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim f As Func(Of Long) = Function()
                                     Return [|CLng(5)|]
                                 End Function
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        Dim f As Func(Of Long) = Function()
                                     Return 5
                                 End Function
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545311")>
        Public Async Function TestRemoveUnneededCastInLambda3() As Task
            Dim markup =
<File>
Imports System
Module Module1
    Sub Main()
        Dim lambda As Func(Of Action(Of Integer, Long)) = Function()
                                                              Return [|CType(Sub(x As Integer, y As Long)
                                                                           End Sub, Action(Of Integer, Long))|]
                                                          End Function
    End Sub
End Module

</File>

            Dim expected =
<File>
Imports System
Module Module1
    Sub Main()
        Dim lambda As Func(Of Action(Of Integer, Long)) = Function()
                                                              Return Sub(x As Integer, y As Long)
                                                                     End Sub
                                                          End Function
    End Sub
End Module

</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545311")>
        Public Async Function TestRemoveUnneededCastInFunctionStatement() As Task
            Dim markup =
<File>
Module Program
    Function M() As Long
        Return [|CLng(5)|]
    End Function
End Module
</File>

            Dim expected =
<File>
Module Program
    Function M() As Long
        Return 5
    End Function
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545311")>
        Public Async Function TestRemoveUnneededCastInFunctionVariableAssignment() As Task
            Dim markup =
<File>
Module Program
    Function M() As Long
        M = [|CLng(5)|]
    End Function
End Module
</File>

            Dim expected =
<File>
Module Program
    Function M() As Long
        M = 5
    End Function
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545312")>
        Public Async Function TestRemoveUnneededCastInBinaryExpression() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        Dim m As Integer = 3
        Dim n? As Integer = 2
        Dim comparer = [|CType(m, Integer?)|] > n
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        Dim m As Integer = 3
        Dim n? As Integer = 2
        Dim comparer = m > n
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545423")>
        Public Async Function TestRemoveUnneededCastInsideCaseLabel() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case [|CType(5, Long)|]
        End Select
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case 5
        End Select
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545421")>
        Public Async Function TestRemoveUnneededCastInOptionalParameterValue() As Task
            Dim markup =
<File>
Module Program
    Function test(Optional ByVal x? As Integer = [|CType(Nothing, Object)|]) As Boolean
        Return x.HasValue
    End Function
End Module
</File>

            Dim expected =
<File>
Module Program
    Function test(Optional ByVal x? As Integer = Nothing) As Boolean
        Return x.HasValue
    End Function
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545579")>
        Public Async Function TestRemoveUnneededCastInRangeCaseClause1() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case CType(5, Long)
            Case [|CType(1, Long)|] To CType(5, Long)
        End Select
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case CType(5, Long)
            Case 1 To CType(5, Long)
        End Select
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545579")>
        Public Async Function TestRemoveUnneededCastInRangeCaseClause2() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case CType(5, Long)
            Case CType(1, Long) To [|CType(5, Long)|]
        End Select
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        Select Case 5L
            Case CType(5, Long)
            Case CType(1, Long) To 5
        End Select
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545580")>
        Public Async Function TestRemoveUnneededCastForLoop1() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        For i As Long = [|CLng(0)|] To CLng(4) Step CLng(5)
        Next
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        For i As Long = 0 To CLng(4) Step CLng(5)
        Next
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545580")>
        Public Async Function TestRemoveUnneededCastForLoop2() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        For i As Long = CLng(0) To [|CLng(4)|] Step CLng(5)
        Next
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        For i As Long = CLng(0) To 4 Step CLng(5)
        Next
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545580")>
        Public Async Function TestRemoveUnneededCastForLoop3() As Task
            Dim markup =
<File>
Module Module1
    Sub Main()
        For i As Long = CLng(0) To CLng(4) Step [|CLng(5)|]
        Next
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Module1
    Sub Main()
        For i As Long = CLng(0) To CLng(4) Step 5
        Next
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545599")>
        Public Async Function TestDontRemoveNeededCastWithUserDefinedConversionsAndOptionStrictOff() As Task
            Dim markup =
<File>
Option Strict Off

Public Class X
    Sub Goo()
        Dim x As New X()
        Dim y As Integer = [|CDbl(x)|]
    End Sub

    Public Shared Widening Operator CType(ByVal x As X) As Double
    End Operator
    Public Shared Widening Operator CType(ByVal x As X) As Single?
    End Operator
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529535")>
        Public Async Function TestDontRemoveNeededCastWhenResultIsAmbiguous() As Task
            Dim markup =
<File>
Option Strict On
Interface IEnumerable(Of Out Tout)
End Interface
Class A : End Class
Class B
    Inherits A
End Class
Class ControlList
    Implements IEnumerable(Of A)
    Implements IEnumerable(Of B)
End Class

Module VarianceExample
    Sub Main()
        Dim _ctrlList As IEnumerable(Of A) = [|CType(New ControlList, IEnumerable(Of A))|]
    End Sub
End Module
</File>

            Dim expected =
<File>
Option Strict On
Interface IEnumerable(Of Out Tout)
End Interface
Class A : End Class
Class B
    Inherits A
End Class
Class ControlList
    Implements IEnumerable(Of A)
    Implements IEnumerable(Of B)
End Class

Module VarianceExample
    Sub Main()
        Dim _ctrlList As IEnumerable(Of A) = New ControlList
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545261")>
        Public Async Function TestRemoveUnnecessaryCastToNothingInArrayInitializer() As Task
            Dim markup =
<File>
Module Program
    Sub Main(args As String())
        Dim NothingArray = {([|CType(Nothing, Object)|])}
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main(args As String())
        Dim NothingArray = {(Nothing)}
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545526")>
        Public Async Function TestDontRemoveCastThatResultsInDifferentStringRepresentations() As Task
            Dim markup =
<File>
Option Strict Off

Module M
    Sub Main()
        Goo([|CType(1000000000000000, Double)|]) ' Prints 1E+15
        Goo(1000000000000000) ' Prints 1000000000000000
    End Sub
    Sub Goo(x As String)
        Console.WriteLine(x)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545631")>
        Public Async Function TestDontRemoveCastThatChangesArrayLiteralTypeAndBreaksOverloadResolution() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = {[|CLng(Nothing)|]}
        Goo(a)
    End Sub

    Sub Goo(a() As Long)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545456")>
        Public Async Function TestRemoveCastInAttribute() As Task
            Dim markup =
<File>
Imports System
Class GooAttribute
    Inherits Attribute

    Sub New(o As Object)
    End Sub

End Class

&lt;Goo([|CObj(1)|])&gt;
Class C
End Class
</File>

            Dim expected =
<File>
Imports System
Class GooAttribute
    Inherits Attribute

    Sub New(o As Object)
    End Sub

End Class

&lt;Goo(1)&gt;
Class C
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545701")>
        Public Async Function TestAddParenthesesIfCopyBackAffected1() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Dim x = 1
        Goo([|CInt(x)|])
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Sub Main()
        Dim x = 1
        Goo((x))
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545701")>
        Public Async Function TestAddParenthesesIfCopyBackAffected2() As Task
            Dim markup =
<File>
Module M
    Private x As Integer = 1
    Sub Main()
        Goo([|CInt(x)|])
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Private x As Integer = 1
    Sub Main()
        Goo((x))
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545701")>
        Public Async Function TestAddParenthesesIfCopyBackAffected3() As Task
            Dim markup =
<File>
Module M
    Private Property x As Integer = 1
    Sub Main()
        Goo([|CInt(x)|])
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Private Property x As Integer = 1
    Sub Main()
        Goo((x))
        Console.WriteLine(x)
    End Sub
    Sub Goo(ByRef x As Integer)
        x = 2
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestDontRemoveNecessaryCastPassedToParamArray1() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Goo([|CObj(Nothing)|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestDontRemoveNecessaryCastPassedToParamArray2() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Goo([|CStr(Nothing)|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestRemoveUnnecessaryCastPassedToParamArray1() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Goo([|CObj(New Object)|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Sub Main()
        Goo(New Object)
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestRemoveUnnecessaryCastPassedToParamArray2() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Goo([|CStr("")|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Sub Main()
        Goo("")
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.Length)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestRemoveUnnecessaryCastPassedToParamArray3() As Task
            Dim markup =
<File>
Imports System
Module M
    Sub Main()
        Goo([|DirectCast(New Exception, Object)|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module M
    Sub Main()
        Goo(New Exception)
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestRemoveUnnecessaryCastPassedToParamArray4() As Task
            Dim markup =
<File>
Imports System
Module M
    Sub Main()
        Goo([|DirectCast(Nothing, Object())|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module M
    Sub Main()
        Goo(Nothing)
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545971")>
        Public Async Function TestRemoveUnnecessaryCastPassedToParamArray5() As Task
            Dim markup =
<File>
Imports System
Module M
    Sub Main()
        Goo([|DirectCast(Nothing, String())|])
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module M
    Sub Main()
        Goo(Nothing)
    End Sub
    Sub Goo(ParamArray x As Object())
        Console.WriteLine(x.GetType)
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact>
        Public Async Function TestRemoveUnnecessaryCastToArrayLiteral1() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Dim i = [|CType({1, 2, 3}, Integer())|]
    End Sub
End Module
</File>

            Dim expected =
<File>
Module M
    Sub Main()
        Dim i = {1, 2, 3}
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact>
        Public Async Function TestDontRemoveNecessaryCastToArrayLiteral2() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = {[|CLng(Nothing)|]}
        Goo(a)
    End Sub

    Sub Goo(a() As Long)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact>
        Public Async Function TestDontRemoveNecessaryCastToArrayLiteral() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Dim i = [|CType({1, 2, 3}, Long())|]
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545972")>
        Public Async Function TestRemoveUnnecessaryCastInBinaryIf1() As Task
            Dim markup =
<File>
Class Test
    Public Shared Sub Main()
        Dim a1 As Long = If((0 = 0), [|CType(1, Long)|], CType(2, Long))
    End Sub
End Class
</File>

            Dim expected =
<File>
Class Test
    Public Shared Sub Main()
        Dim a1 As Long = If((0 = 0), 1, CType(2, Long))
    End Sub
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545972")>
        Public Async Function TestRemoveUnnecessaryCastInBinaryIf2() As Task
            Dim markup =
<File>
Class Test
    Public Shared Sub Main()
        Dim a1 As Long = If((0 = 0), CType(1, Long), [|CType(2, Long)|])
    End Sub
End Class
</File>

            Dim expected =
<File>
Class Test
    Public Shared Sub Main()
        Dim a1 As Long = If((0 = 0), CType(1, Long), 2)
    End Sub
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545974")>
        Public Async Function TestRemoveUnnecessaryCastInObjectCreationExpression() As Task
            Dim markup =
<File>
Imports System
Module M
    Sub Main()
        Dim t1 As Type = [|CType(New ArgumentException(), Exception)|].GetType()
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module M
    Sub Main()
        Dim t1 As Type = New ArgumentException().GetType()
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545973")>
        Public Async Function TestRemoveUnnecessaryCastInSelectCase() As Task
            Dim markup =
<File>
Imports System
Module Module1
    Sub Main()
        Select Case [|CType(2, Integer)|]
            Case 2 To CType(5, Object)
                Console.WriteLine("true")
        End Select
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System
Module Module1
    Sub Main()
        Select Case 2
            Case 2 To CType(5, Object)
                Console.WriteLine("true")
        End Select
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545526")>
        Public Async Function TestDontRemoveCastToDoubleInOptionStrictOff() As Task
            Dim markup =
<File>
Option Strict Off

Module M
    Sub Main()
        Goo([|CType(1000000000000000, Double)|]) ' Prints 1E+15
        Goo(1000000000000000) ' Prints 1000000000000000
    End Sub
    Sub Goo(x As String)
        Console.WriteLine(x)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545828")>
        Public Async Function TestDontRemoveCStrInCharToStringToObjectChain() As Task
            Dim markup =
<File>
Imports System
Module Program
    Sub Main()
        Dim x As Object = [|CStr(" "c)|]
        Console.WriteLine(x.GetType())
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545808")>
        Public Async Function TestDontRemoveNecessaryCastWithMultipleUserDefinedConversionsAndOptionStrictOff() As Task
            Dim markup =
<File>
Option Strict Off

Public Class X
    Shared Sub Main()
        Dim x As New X()
        Dim y As Integer = [|CDbl(x)|]
        Console.WriteLine(y)
    End Sub

    Public Shared Widening Operator CType(ByVal x As X) As Double
        Return 1
    End Operator
    Public Shared Widening Operator CType(ByVal x As X) As Single
        Return 2
    End Operator
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545998")>
        Public Async Function TestDontRemoveCastWhichWouldChangeAttributeOverloadResolution() As Task
            Dim markup =
<File>
Imports System

&lt;A({[|CLng(0)|]})&gt;
Class A
    Inherits Attribute

    Sub New(x As Integer())
    End Sub

    Sub New(x As Long())
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact>
        Public Async Function TestDontMoveTrailingComment() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        With ""
            Dim y = [|CInt(1 + 2)|] ' Blah
        End With
    End Sub
End Module
</File>

            Dim expected =
<File>
Module Program
    Sub Main()
        With ""
            Dim y = 1 + 2 ' Blah
        End With
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryStringPredefinedCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CStr(a).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryWideningPredefinedCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CLng(a).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryNarrowingPredefinedCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CByte(a).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryStringCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CType(a, String).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryWideningCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CType(a, Long).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryNarrowingCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CType(a, Byte).GetType()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryUserDefinedCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CType(a, WeirdInt).GetType()|]
    End Sub

    Public Class WeirdInt
        Private val As Integer
        Public Sub New(ByVal b As Integer)
            Me.val = b
        End Sub
        Public Shared Widening Operator CType(ByVal d As WeirdInt) As Integer
            Return d.val
        End Operator
        Public Shared Narrowing Operator CType(ByVal i As Integer) As WeirdInt
            Return New WeirdInt(i)
        End Operator
    End Class
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestRemoveUnnecessaryCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CType(a, Object).GetType()|]
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = a.GetType()
    End Sub
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestRemoveUnnecessaryInheritedCTypeCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a As BaseClass
        a = New DerivedClass()
        Dim b = [|CType(a, BaseClass).GetType()|]
    End Sub

    Public Class BaseClass
    End Class

    Public Class DerivedClass : Inherits BaseClass
    End Class
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a As BaseClass
        a = New DerivedClass()
        Dim b = a.GetType()
    End Sub

    Public Class BaseClass
    End Class

    Public Class DerivedClass : Inherits BaseClass
    End Class
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestRemoveUnnecessaryDirectCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|DirectCast(a, Object).GetType()|]
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = a.GetType()
    End Sub
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestRemoveUnnecessaryTryCastWithGetType() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|TryCast(a, Object).GetType()|]
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = a.GetType()
    End Sub
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/32399")>
        Public Async Function TestDontRemoveNecessaryPredefinedCastWithToString() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a = 2
        Dim b = [|CStr(a).ToString()|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestDontRemoveNecessaryPredefinedCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as Object
        With [|CStr(a)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestDontRemoveNecessaryDirectCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as Object
        With [|DirectCast(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestDontRemoveNecessaryCTypeCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as Object
        With [|CType(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestDontRemoveNecessaryTryCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as Object
        With [|TryCast(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestRemoveUnnecessaryPredefinedCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as String
        With [|CStr(a)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a as String
        With a
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestRemoveUnnecessaryDirectCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as String
        With [|DirectCast(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a as String
        With a
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestRemoveUnnecessaryCTypeCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as String
        With [|CType(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a as String
        With a
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/30617")>
        Public Async Function TestRemoveUnnecessaryTryCastInWithStatement() As Task
            Dim markup =
<File>
Module Program
    Sub Main()
        Dim a as String
        With [|TryCast(a, String)|]
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>
            Dim expected =
<File>
Module Program
    Sub Main()
        Dim a as String
        With a
            Dim x as Integer = .Length
        End With
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact>
        Public Async Function TestRemoveCastInFieldInitializer() As Task
            Dim markup =
<File>
Imports System.Collections.Generic
Class B
    Dim list = [|CObj(GetList())|]

    Private Shared Function GetList() As List(Of String)
        Return New List(Of String) From {"abc", "def", "ghi"}
    End Function
End Class
</File>

            Dim expected =
<File>
Imports System.Collections.Generic
Class B
    Dim list = GetList()

    Private Shared Function GetList() As List(Of String)
        Return New List(Of String) From {"abc", "def", "ghi"}
    End Function
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact>
        Public Async Function TestDontDuplicateTrivia() As Task
            Dim markup =
<File>
Imports System
Module M
    Sub Main()
        [|CType(x(), Action)|] ' Remove redundant cast
    End Sub
    Function x() As Action
        Return Sub() Console.WriteLine(1)
    End Function
End Module
</File>

            Dim expected =
<File>
Imports System
Module M
    Sub Main()
        x()() ' Remove redundant cast
    End Sub
    Function x() As Action
        Return Sub() Console.WriteLine(1)
    End Function
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/531479")>
        Public Async Function TestEscapeNextStatementIfNeeded() As Task
            Dim markup =
<File>
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main()
        Dim y = [|CType(From z In "" Distinct, IEnumerable(Of Char))|]
        Take()
    End Sub

    Sub Take()
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main()
        Dim y = From z In "" Distinct
        [Take]()
    End Sub

    Sub Take()
    End Sub
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/607749")>
        Public Async Function TestBugfix_607749() As Task
            Dim markup =
<File>
Imports System
Interface I
    Property A As Action
End Interface

Class C
    Implements I
    Property A As Action = [|CType(Sub() If True Then, Action)|] Implements I.A
End Class
</File>

            Dim expected =
<File>
Imports System
Interface I
    Property A As Action
End Interface

Class C
    Implements I
    Property A As Action = (Sub() If True Then) Implements I.A
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/609477")>
        Public Async Function TestBugfix_609477() As Task
            Dim markup =
<File>
Imports System
Module Program
    Sub Main()
        If True Then : Dim x As Action = [|CType(Sub() If True Then, Action)|] : Else : Return : End If
    End Sub
End Module

</File>

            Dim expected =
<File>
Imports System
Module Program
    Sub Main()
        If True Then : Dim x As Action = (Sub() If True Then) : Else : Return : End If
    End Sub
End Module

</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/552813")>
        Public Async Function TestDontRemoveCastWhileNarrowingWithOptionOn() As Task
            Dim markup =
<File>
Option Strict On
Module Program
    Public Function IsFailFastSuppressed() As Boolean
        Dim value = New Object()
        Return value IsNot Nothing AndAlso [|DirectCast(value, Boolean)|]
    End Function
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/577929")>
        Public Async Function TestDontRemoveCastWhileDefaultingNullables() As Task
            Dim markup =
<File>
Module M
    Sub Main()
        Dim x? As Date = [|CDate(Nothing)|]
        Console.WriteLine(x)
    End Sub
End Module
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact>
        Public Async Function TestRemoveCastAroundAction() As Task
            Dim markup =
<File>
Imports System
Module Program
    Sub Main()
        Dim x As Action = Sub() Console.WriteLine("Hello")
        [|CType(x, Action)|] : Console.WriteLine()
    End Sub
End Module

</File>

            Dim expected =
<File>
Imports System
Module Program
    Sub Main()
        Dim x As Action = Sub() Console.WriteLine("Hello")
        x() : Console.WriteLine()
    End Sub
End Module

</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/578016")>
        Public Async Function TestDontRemoveCStr() As Task
            Dim markup =
<File>Option Strict On

Module M
    Sub Main()
        Goo()
    End Sub
    Sub Goo(Optional x As Object = [|CStr|](Chr(1)))
        Console.WriteLine(x.GetType())
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530105")>
        Public Async Function TestDontRemoveNumericCast() As Task
            Dim markup =
<File>
Interface I
    [|Sub Goo(Optional x As Object = CByte(1))|]
End Interface
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530104")>
        Public Async Function TestDontRemoveCTypeFromNumberToEnum() As Task
            Dim markup =
<File>
Option Strict On

Interface I
    [|Sub Goo(Optional x As DayOfWeek = CType(-1, DayOfWeek))|]
End Interface
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530077")>
        Public Async Function TestDontRemoveCastForLambdaToDelegateConversionWithOptionStrictOn() As Task
            Dim markup =
 <File>
Option Strict On
Imports System
Module Program
    Sub Main(args As String())
        Dim x = 1
        Dim y As Func(Of Integer) = Function()
                                        Return [|CType(x.ToString(), Integer)|]
                                    End Function
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529966")>
        Public Async Function TestDontRemoveForNarrowingConversionFromObjectWithOptionStrictOnInsideQueryExpression() As Task
            Dim markup =
<File>
Option Strict On
Imports System
Imports System.Linq
Module Program
    Sub Main(args As String())
        Dim o3 As Object = ""hi""
        Dim col = {o3, o3}
        Dim q3 = From i As String In [|CType(col, String())|]
    End Sub
End Module
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530650")>
        Public Async Function TestRemoveUnnecessaryCastFromLambdaToDelegateParenthesizeLambda() As Task
            Dim markup =
<File>
Imports System

Module M
    Sub Main()
        [|CType(Sub() Return, Action)|] : Return
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System

Module M
    Sub Main()
        Call (Sub() Return) : Return
    End Sub
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/707189")>
        Public Async Function TestRemoveUnnecessaryCastFromInvocationStatement() As Task
            Dim markup =
<File>
Imports System
Imports System.Collections.Generic
Module Module1
    Sub Main()
        [|DirectCast(GetEnumerator(), IDisposable).Dispose()|]
    End Sub
    Function GetEnumerator() As List(Of Integer).Enumerator
        Return Nothing
    End Function
End Module
</File>

            Dim expected =
<File>
Imports System
Imports System.Collections.Generic
Module Module1
    Sub Main()
        GetEnumerator().Dispose()
    End Sub
    Function GetEnumerator() As List(Of Integer).Enumerator
        Return Nothing
    End Function
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/707189")>
        Public Async Function DoNotRemoveUnnecessaryCastFromInvocationStatement2() As Task
            Dim markup =
<File>
Interface I1
Sub Goo()
End Interface
Class M
    Implements I1
    Shared Sub Main()
        [|CType(New M(), I1).Goo()|]
    End Sub

    Public Sub Goo() Implements I1.Goo
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768895")>
        Public Async Function TestRemoveUnnecessaryCastInTernary() As Task
            Dim markup =
<File>
Class Program
    Private Shared Sub Main(args As String())
        Dim x As Object = Nothing
        Dim y As Integer = If([|CBool(x)|], 1, 0)
    End Sub
End Class
</File>

            Dim expected =
<File>
Class Program
    Private Shared Sub Main(args As String())
        Dim x As Object = Nothing
        Dim y As Integer = If(x, 1, 0)
    End Sub
End Class
</File>
            Await TestAsync(markup, expected)
        End Function

        <WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/770187")>
        <WpfFact(Skip:="770187")>
        Public Async Function TestDontRemoveNecessaryCastInSelectCaseExpression() As Task
            ' Cast removal invokes a different user defined operator, hence the cast is necessary.

            Dim markup =
<File>
    <![CDATA[
Namespace ConsoleApplication23
    Class Program
        Public Shared Sub Main(args As String())
            Dim goo As Integer = 0
            Select Case [|CType(0, Short)|]
                Case New A
                    Return
            End Select
        End Sub
    End Class

    Class A
        Public Shared Operator =(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short =")
            Return 0
        End Operator

        Public Shared Operator <>(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short <>")
            Return 0
        End Operator

        Public Shared Operator =(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer =")
            Throw New NotImplementedException
        End Operator

        Public Shared Operator <>(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer <>")
            Throw New NotImplementedException
        End Operator
    End Class
End Namespace]]>
</File>
            Await TestMissingAsync(markup)
        End Function

        <WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/770187")>
        <WpfFact(Skip:="770187")>
        Public Async Function TestDontRemoveNecessaryCastInSelectCaseExpression2() As Task
            ' Cast removal invokes a different user defined operator, hence the cast is necessary.

            Dim markup =
<File>
    <![CDATA[
Namespace ConsoleApplication23
    Class Program
        Public Shared Sub Main(args As String())
            Dim goo As Integer = 0
            Select Case [|CType(0, Short)|]
                Case < New A
                    Return
            End Select
        End Sub
    End Class

    Class A
        Public Shared Operator <(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short <")
            Return 0
        End Operator

        Public Shared Operator >(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short >")
            Return 0
        End Operator

        Public Shared Operator <(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer <")
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer >")
            Throw New NotImplementedException
        End Operator
    End Class
End Namespace]]>
</File>
            Await TestMissingAsync(markup)
        End Function

        <WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/770187")>
        <WpfFact(Skip:="770187")>
        Public Async Function TestDontRemoveNecessaryCastInSelectCaseExpression3() As Task
            ' Cast removal invokes a different user defined operator, hence the cast is necessary.

            Dim markup =
<File>
    <![CDATA[
Namespace ConsoleApplication23
    Class Program
        Public Shared Sub Main(args As String())
            Dim goo As Integer = 0
            Select Case [|CType(0, Short)|]
                Case New A To New A
                    Return
            End Select
        End Sub
    End Class

    Class A
        Public Shared Operator <=(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short <=")
            Return 0
        End Operator

        Public Shared Operator >=(ByVal p1 As Short, ByVal p2 As A) As Boolean
            Console.WriteLine("Short >=")
            Return 0
        End Operator

        Public Shared Operator <=(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer <=")
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >=(ByVal p1 As Integer, ByVal p2 As A) As Boolean
            Console.WriteLine("Integer >=")
            Throw New NotImplementedException
        End Operator
    End Class
End Namespace]]>
</File>
            Await TestMissingAsync(markup)
        End Function

#Region "Interface Casts"

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545889")>
        Public Async Function TestDontRemoveCastToInterfaceForUnsealedType() As Task
            Dim markup =
<File>
Imports System

Class X
    Implements IDisposable
    Private Shared Sub Main()
        Dim x As X = New Y()
        [|DirectCast(x, IDisposable)|].Dispose()
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Console.WriteLine("X.Dispose")
    End Sub
End Class

Class Y
    Inherits X
    Implements IDisposable
    Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
        Console.WriteLine("Y.Dispose")
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545890")>
        Public Async Function DoNotRemoveCastToInterfaceForSealedType1() As Task
            Dim markup =
<File>
Imports System

Interface I
    Sub Goo(Optional x As Integer = 0)
End Interface

NotInheritable Class C
    Implements I
    Public Sub Goo(Optional x As Integer = 0) Implements I.Goo
        Console.WriteLine(x)
    End Sub

    Private Shared Sub Main()
        [|DirectCast(New C(), I)|].Goo()
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545890")>
        Public Async Function DoNotRemoveCastToInterfaceForSealedType2() As Task
            Dim markup =
<File>
Imports System

Interface I
    ReadOnly Property Goo() As String
End Interface

NotInheritable Class C
    Implements I
    Public ReadOnly Property Goo() As String Implements I.Goo
        Get
            Return "Nikov Rules"
        End Get
    End Property

    Private Shared Sub Main()
        Console.WriteLine([|DirectCast(New C(), I)|].Goo)
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545890")>
        Public Async Function DoNotRemoveCastToInterfaceForSealedType3() As Task
            Dim markup =
<File>
Imports System

Interface I
    ReadOnly Property Goo() As String
End Interface

NotInheritable Class C
    Implements I
    Public Shared ReadOnly Property Instance() As C
        Get
            Return New C()
        End Get
    End Property

    Public ReadOnly Property Goo() As String Implements I.Goo
        Get
            Return "Nikov Rules"
        End Get
    End Property

    Private Shared Sub Main()
        Console.WriteLine([|DirectCast(Instance, I)|].Goo)
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545890")>
        Public Async Function TestDontRemoveCastToInterfaceForSealedType4() As Task
            ' Note: The cast below can't be removed (even though C is sealed)
            ' because the unspecified optional parameter default values differ.

            Dim markup =
<File>
Imports System

Interface I
    Sub Goo(Optional x As Integer = 0)
End Interface

NotInheritable Class C
    Implements I
    Public Sub Goo(Optional x As Integer = 1) Implements I.Goo
        Console.WriteLine(x)
    End Sub

    Private Shared Sub Main()
        [|DirectCast(New C(), I)|].Goo()
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545890")>
        Public Async Function TestDontRemoveCastToInterfaceForSealedType5() As Task
            ' Note: The cast below cannot be removed (even though C is sealed)
            ' because default values differ for optional parameters and
            ' hence the method is not considered an implementation.

            Dim markup =
<File>
Imports System

Interface I
    Sub Goo(Optional x As Integer = 0)
End Interface

NotInheritable Class C
    Implements I
    Public Sub Goo(Optional x As Integer = 1) Implements I.Goo
        Console.WriteLine(x)
    End Sub

    Private Shared Sub Main()
        [|DirectCast(New C(), I)|].Goo(2)
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545888")>
        Public Async Function TestDontRemoveCastToInterfaceForSealedType6() As Task
            ' Note: The cast below can't be removed (even though C is sealed)
            ' because the specified named arguments refer to parameters that
            ' appear at different positions in the member signatures.

            Dim markup =
<File>
Imports System

Interface I
    Sub Goo(Optional x As Integer = 0, Optional y As Integer = 0)
End Interface

NotInheritable Class C
    Implements I
    Public Sub Goo(Optional y As Integer = 0, Optional x As Integer = 0) Implements I.Goo
        Console.WriteLine(x)
    End Sub

    Private Shared Sub Main()
        [|DirectCast(New C(), I)|].Goo(x:=1)
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545888")>
        Public Async Function DoNotRemoveCastToInterfaceForSealedType7() As Task
            Dim markup =
<File>
Imports System

Interface I
    Function Goo(Optional x As Integer = 0, Optional y As Integer = 0) As Integer
End Interface

NotInheritable Class C
    Implements I
    Public Function Goo(Optional x As Integer = 0, Optional y As Integer = 0) As Integer Implements I.Goo
        Return x * 2
    End Function

    Private Shared Sub Main()
        Console.WriteLine([|DirectCast(New C(), I)|].Goo(x:=1))
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545888")>
        Public Async Function TestDontRemoveCastToInterfaceForSealedType9() As Task
            ' Note: The cast below can't be removed (even though C is sealed)
            ' because it would result in binding to a Dispose method that doesn't
            ' implement IDisposable.Dispose().

            Dim markup =
<File>
Imports System
Imports System.IO

NotInheritable Class C
    Inherits MemoryStream
    Private Shared Sub Main()
        Dim s As New C()
        [|DirectCast(s, IDisposable)|].Dispose()
    End Sub

    Public Shadows Sub Dispose()
        Console.WriteLine("new Dispose()")
    End Sub
End Class
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545887")>
        Public Async Function TestDontRemoveCastToInterfaceForStruct1() As Task
            ' Note: The cast below can't be removed because the cast boxes 's' and
            ' unboxing would change program behavior.

            Dim markup =
<File>
Imports System

Interface IIncrementable
    ReadOnly Property Value() As Integer
    Sub Increment()
End Interface

Structure S
    Implements IIncrementable
    Public Property Value() As Integer Implements IIncrementable.Value
        Get
            Return m_Value
        End Get
        Private Set
            m_Value = Value
        End Set
    End Property
    Private m_Value As Integer
    Public Sub Increment() Implements IIncrementable.Increment
        Value += 1
    End Sub

    Private Shared Sub Main()
        Dim s = New S()
        [|DirectCast(s, IIncrementable)|].Increment()
        Console.WriteLine(s.Value)
    End Sub
End Structure
</File>

            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545834"), WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530073")>
        Public Async Function TestRemoveCastToInterfaceForStruct2() As Task
            ' Note: The cast below can be removed because we are sure to have
            ' a fresh copy of the struct from the GetEnumerator() method.

            Dim markup =
<File>
Imports System
Imports System.Collections.Generic

Class Program
    Private Shared Sub Main()
        Call [|DirectCast(GetEnumerator(), IDisposable)|].Dispose()
    End Sub

    Private Shared Function GetEnumerator() As List(Of Integer).Enumerator
        Dim x = New List(Of Integer)() From {1, 2, 3}
        Return x.GetEnumerator()
    End Function
End Class
</File>

            Dim expected =
<File>
Imports System
Imports System.Collections.Generic

Class Program
    Private Shared Sub Main()
        Call GetEnumerator().Dispose()
    End Sub

    Private Shared Function GetEnumerator() As List(Of Integer).Enumerator
        Dim x = New List(Of Integer)() From {1, 2, 3}
        Return x.GetEnumerator()
    End Function
End Class
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544655")>
        Public Async Function TestRemoveCastToICloneableForDelegate() As Task
            ' Note: The cast below can be removed because delegates are implicitly sealed.

            Dim markup =
<File>
Imports System

Class C
    Private Shared Sub Main()
        Dim a As Action = Sub()
                          End Sub
        Dim c = [|DirectCast(a, ICloneable)|].Clone()
    End Sub
End Class
</File>

            Dim expected =
<File>
Imports System

Class C
    Private Shared Sub Main()
        Dim a As Action = Sub()
                          End Sub
        Dim c = a.Clone()
    End Sub
End Class
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545926")>
        Public Async Function TestRemoveCastToICloneableForArray() As Task
            ' Note: The cast below can be removed because arrays are implicitly sealed.

            Dim markup =
<File>
Imports System

Class C
    Private Shared Sub Main()
        Dim a = New Integer() {1, 2, 3}
        Dim c = [|DirectCast(a, ICloneable)|].Clone()
    End Sub
End Class
</File>

            Dim expected =
<File>
Imports System

Class C
    Private Shared Sub Main()
        Dim a = New Integer() {1, 2, 3}
        Dim c = a.Clone()
    End Sub
End Class
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529937")>
        Public Async Function TestRemoveCastToICloneableForArray2() As Task
            ' Note: The cast below can be removed because arrays are implicitly sealed.

            Dim markup =
<File>
Imports System

Module module1
    Sub Main()
        Dim c = [|DirectCast({1}, ICloneable)|].Clone
    End Sub
End Module
</File>

            Dim expected =
<File>
Imports System

Module module1
    Sub Main()
        Dim c = {1}.Clone
    End Sub
End Module
</File>
            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529897")>
        Public Async Function TestRemoveCastToIConvertibleForEnum() As Task
            ' Note: The cast below can be removed because enums are implicitly sealed.

            Dim markup =
<File>
Imports System

Class Program
    Private Shared Sub Main()
        Dim e As [Enum] = DayOfWeek.Monday
        Dim y = [|DirectCast(e, IConvertible)|].GetTypeCode()
    End Sub
End Class
</File>

            Dim expected =
<File>
Imports System

Class Program
    Private Shared Sub Main()
        Dim e As [Enum] = DayOfWeek.Monday
        Dim y = e.GetTypeCode()
    End Sub
End Class
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/844482")>
        <WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1031406")>
        Public Async Function TestDoNotRemoveCastFromDerivedToBaseWithImplicitReference() As Task
            ' Cast removal changes the runtime behavior of the program.
            Dim markup =
<File>
Module Program
    Sub Main(args As String())
        Dim x As C = new C
        Dim y As C = [|DirectCast(x, D)|]
    End Sub
End Module

Class C
End Class

Class D
    Inherits C
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/995908")>
        Public Async Function TestRemoveCastIntroducesDuplicateAnnotations() As Task
            Dim markup =
<File>
    <![CDATA[
Imports System.Runtime.CompilerServices
Imports N

Interface INamedTypeSymbol
    Inherits INamespaceOrTypeSymbol
End Interface

Interface INamespaceOrTypeSymbol
End Interface

Namespace N
    Friend Module INamespaceOrTypeSymbolExtensions
        <Extension>
        Public Sub ExtensionMethod(symbol As INamespaceOrTypeSymbol)
        End Sub
    End Module
End Namespace

Module Program
    Sub Main(args As String())
        Dim symbol As INamedTypeSymbol = Nothing
        [|DirectCast(symbol, INamespaceOrTypeSymbol).ExtensionMethod()|]
    End Sub
End Module
]]>
</File>

            Dim expected =
<File>
    <![CDATA[
Imports System.Runtime.CompilerServices
Imports N

Interface INamedTypeSymbol
    Inherits INamespaceOrTypeSymbol
End Interface

Interface INamespaceOrTypeSymbol
End Interface

Namespace N
    Friend Module INamespaceOrTypeSymbolExtensions
        <Extension>
        Public Sub ExtensionMethod(symbol As INamespaceOrTypeSymbol)
        End Sub
    End Module
End Namespace

Module Program
    Sub Main(args As String())
        Dim symbol As INamedTypeSymbol = Nothing
        symbol.ExtensionMethod()
    End Sub
End Module
]]>
</File>
            Await TestAsync(markup, expected)
        End Function

#End Region

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/739")>
        Public Async Function TestRemoveAroundArrayLiteralInInterpolation1() As Task
            Dim markup =
<File>
Module M
    Dim x = $"{ [|CObj({})|] }" ' Remove unnecessary cast
End Module
</File>

            Dim expected =
<File>
Module M
    Dim x = $"{ {} }" ' Remove unnecessary cast
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/739")>
        Public Async Function TestRemoveAroundArrayLiteralInInterpolation2() As Task
            Dim markup =
<File>
Module M
    Dim x = $"{[|CObj({})|] }" ' Remove unnecessary cast
End Module
</File>

            Dim expected =
<File>
Module M
    Dim x = $"{({}) }" ' Remove unnecessary cast
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/739")>
        Public Async Function TestRemoveAroundArrayLiteralInInterpolation3() As Task
            Dim markup =
<File>
Module M
    Dim x = $"{ [|CObj({})|]}" ' Remove unnecessary cast
End Module
</File>

            Dim expected =
<File>
Module M
    Dim x = $"{ {}}" ' Remove unnecessary cast
End Module
</File>

            Await TestAsync(markup, expected)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/2761")>
        Public Async Function TestDontRemoveCastFromBaseToDerivedWithNarrowingReference() As Task
            Dim markup =
<File>
Module Module1
    Private Function NewMethod(base As Base) As Base
        Return If([|TryCast(base, Derived1)|], New Derived1())
    End Function
End Module

Class Base
End Class

Class Derived1 : Inherits Base
End Class

Class Derived2 : Inherits Base
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/3254")>
        Public Async Function TestDontRemoveCastToTypeParameterWithExceptionConstraint() As Task
            Dim markup =
<File>
Imports System

Class Program
    Private Shared Sub RequiresCondition(Of TException As Exception)(condition As Boolean, messageOnFalseCondition As String)
        If Not condition Then
            Throw [|DirectCast(Activator.CreateInstance(GetType(TException), messageOnFalseCondition), TException)|]
        End If
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/3254")>
        Public Async Function TestDontRemoveCastToTypeParameterWithExceptionSubTypeConstraint() As Task
            Dim markup =
<File>
Imports System

Class Program
    Private Shared Sub RequiresCondition(Of TException As ArgumentException)(condition As Boolean, messageOnFalseCondition As String)
        If Not condition Then
            Throw [|DirectCast(Activator.CreateInstance(GetType(TException), messageOnFalseCondition), TException)|]
        End If
    End Sub
End Class
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/3163")>
        Public Async Function TestDoNotRemoveCastInUserDefinedNarrowingConversionStrictOn() As Task
            Dim markup =
<File>
Option Strict On

Module Module1

    Sub Main()
        Dim red = ColorF.FromArgb(255, 255, 0, 0)
        Dim c As Color = [|CType(red, Color)|]
    End Sub

End Module

Public Structure ColorF
    Public A, R, G, B As Single
    Public Shared Function FromArgb(a As Double, r As Double, g As Double, b As Double) As ColorF
        Return New ColorF With {.A = CSng(a), .R = CSng(r), .G = CSng(g), .B = CSng(b)}
    End Function
    Public Shared Widening Operator CType(x As Color) As ColorF
        Return ColorF.FromArgb(x.A / 255, x.R / 255, x.G / 255, x.B / 255)
    End Operator
    Public Shared Narrowing Operator CType(x As ColorF) As Color
        Return Color.FromArgb(CByte(x.A * 255), CByte(x.R * 255), CByte(x.G * 255), CByte(x.B * 255))
    End Operator
End Structure

Public Structure Color
    Public A, R, G, B As Byte
    Public Shared Function FromArgb(a As Byte, r As Byte, g As Byte, b As Byte) As Color
        Return New Color With {.A = a, .R = r, .G = g, .B = b}
    End Function
End Structure
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/3163")>
        Public Async Function TestDoNotRemoveCastInUserDefinedNarrowingConversionStrictOff() As Task
            Dim markup =
<File>
Option Strict Off

Module Module1

    Sub Main()
        Dim red = ColorF.FromArgb(255, 255, 0, 0)
        Dim c As Color = [|CType(red, Color)|]
    End Sub

End Module

Public Structure ColorF
    Public A, R, G, B As Single
    Public Shared Function FromArgb(a As Double, r As Double, g As Double, b As Double) As ColorF
        Return New ColorF With {.A = CSng(a), .R = CSng(r), .G = CSng(g), .B = CSng(b)}
    End Function
    Public Shared Widening Operator CType(x As Color) As ColorF
        Return ColorF.FromArgb(x.A / 255, x.R / 255, x.G / 255, x.B / 255)
    End Operator
    Public Shared Narrowing Operator CType(x As ColorF) As Color
        Return Color.FromArgb(CByte(x.A * 255), CByte(x.R * 255), CByte(x.G * 255), CByte(x.B * 255))
    End Operator
End Structure

Public Structure Color
    Public A, R, G, B As Byte
    Public Shared Function FromArgb(a As Byte, r As Byte, g As Byte, b As Byte) As Color
        Return New Color With {.A = a, .R = r, .G = g, .B = b}
    End Function
End Structure
</File>
            Await TestMissingAsync(markup)
        End Function

        <Fact, WorkItem("https://github.com/dotnet/roslyn/issues/11008#issuecomment-230786838")>
        Public Async Function DontOfferToRemoveCastWhenAccessingHiddenProperty() As Task
            Await TestMissingInRegularAndScriptAsync(
<Code>
Imports System.Collections.Generic
Class Fruit
    Public Property Properties As IDictionary(Of String, Object)
End Class
Class Apple
    Inherits Fruit
    Public Shadows Property Properties As IDictionary(Of String, Object)
End Class
Class Tester
    Public Sub Test()
        Dim a = New Apple()
        [|CType(a, Fruit)|].Properties(""Color"") = ""Red""
    End Sub
End Class
</Code>.Value)
        End Function
    End Class
End Namespace
