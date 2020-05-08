Imports SSignal_Protocols

Public Class 类_界面文字

#Region "定义和声明"

    Const 替换标识 As String = "#%"
    Const 单复数标识_前面 As String = "$%"
    Const 单复数标识_后面 As String = "/%"
    '举例：There is$% are$% #% apple/% apples/% on the table.

    Dim 分段符_替换标识() As String = New String() {替换标识}
    Dim 分段符_单复数标识_前面() As String = New String() {单复数标识_前面}
    Dim 分段符_单复数标识_后面() As String = New String() {单复数标识_后面}

    Dim 文字() As String
    Public 文字数量 As Integer
    Dim 是英语 As Boolean

#End Region

    Public Sub New(ByVal 网页语种 As String, Optional ByVal 界面文字 As String = Nothing)
        文字数量 = 0
        文字 = Nothing
        If String.Compare(网页语种, 语言代码_英语) = 0 Then
            是英语 = True
            Return
        End If
        If String.IsNullOrEmpty(界面文字) Then Return
        Dim 行() As String = 界面文字.Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
        If 行.Length = 0 Then Return
        Dim I, J As Integer
        Dim 某一行 As String
        Dim 找到了 As Boolean
        ReDim 文字(99)
        For I = 0 To 行.Length - 1
            某一行 = 行(I).TrimStart
            If 某一行.StartsWith("[") Then
                某一行 = 某一行.TrimEnd
                If 某一行.EndsWith("]") Then
                    If 某一行.Length > 2 Then
                        If String.Compare(网页语种, 某一行.Substring(1, 某一行.Length - 2)) = 0 Then
                            找到了 = True
                        ElseIf 找到了 = True Then
                            Exit For
                        End If
                    End If
                End If
            ElseIf 找到了 = True AndAlso 某一行.StartsWith("<") Then
                J = 某一行.IndexOf(">")
                If J > 1 Then
                    J += 1
                    If Val(某一行.Substring(1, J - 2)) <> 文字数量 Then
                        文字数量 = 0
                        文字 = Nothing
                        Return
                    End If
                    If 某一行.Length > J Then
                        文字(文字数量) = 某一行.Substring(J, 某一行.Length - J)
                    End If
                    文字数量 += 1
                End If
            End If
        Next
        If 文字数量 > 0 Then If 文字数量 < 文字.Length Then ReDim Preserve 文字(文字数量 - 1)
    End Sub

    Public Function 获取(ByVal 索引 As Integer, ByVal 现有文字 As String, Optional ByVal 要插入的文本() As Object = Nothing) As String
        If 是英语 = False Then
            If 文字数量 > 0 Then
                If 索引 >= 0 AndAlso 索引 < 文字数量 Then
                    If 要插入的文本 Is Nothing Then
                        Return 文字(索引)
                    Else
                        Return 插入文本(文字(索引), 要插入的文本)
                    End If
                End If
            End If
        End If
        If 要插入的文本 Is Nothing Then
            Return 现有文字
        Else
            Return 插入文本(现有文字, 要插入的文本)
        End If
    End Function

    Public Function JS获取(ByVal 索引 As Integer) As String
        Dim 文字1 As String = 文字(索引)
        If String.IsNullOrEmpty(文字1) = False Then
            If 文字1.Contains(ChrW(34)) = False Then
                Return ChrW(34) & 文字1 & ChrW(34)
            ElseIf 文字1.Contains("'") = False Then
                Return "'" & 文字1 & "'"
            Else
                Return "''"
            End If
        Else
            Return "''"
        End If
    End Function

    Private Function 插入文本(ByVal 原文本 As String, ByVal 要插入的文本() As Object) As String
        Dim 段() As String = 原文本.Split(分段符_替换标识, StringSplitOptions.None)
        If 段.Length > 1 Then
            Dim I, K As Integer
            Dim 替换后的文本 As String = ""
            For I = 0 To 段.Length - 1
                If I = 0 Then
                    If I < 要插入的文本.Length Then
                        If 段(I).IndexOf(单复数标识_前面) < 0 Then
                            替换后的文本 = 段(I)
                        Else
                            Dim 段1() As String = 段(I).Split(分段符_单复数标识_前面, StringSplitOptions.RemoveEmptyEntries)
                            If 段1.Length > 1 Then
                                K = 0
                                If 段1.Length > 2 Then
                                    For K = 0 To 段1.Length - 3
                                        替换后的文本 &= 段1(K)
                                    Next
                                End If
                                Select Case Val(要插入的文本(I))
                                    Case 1, 0 : 替换后的文本 &= 段1(K)
                                    Case Else : 替换后的文本 &= 段1(K + 1)
                                End Select
                            Else
                                替换后的文本 = 段(I)
                            End If
                        End If
                    Else
                        替换后的文本 = 段(I)
                    End If
                Else
                    K = I - 1
                    If K < 要插入的文本.Length Then
                        If 段(I).IndexOf(单复数标识_后面) > 0 Then
                            Dim 段1() As String = 段(I).Split(分段符_单复数标识_后面, StringSplitOptions.RemoveEmptyEntries)
                            If 段1.Length > 1 Then
                                Select Case Val(要插入的文本(K))
                                    Case 1, 0 : 替换后的文本 &= 要插入的文本(K) & 段1(0)
                                    Case Else : 替换后的文本 &= 要插入的文本(K) & 段1(1)
                                End Select
                                If 段1.Length > 2 Then
                                    For K = 2 To 段1.Length - 1
                                        替换后的文本 &= 段1(K)
                                    Next
                                End If
                            Else
                                GoTo Line1
                            End If
                        Else
                            If I < 要插入的文本.Length Then
                                If 段(I).IndexOf(单复数标识_前面) >= 0 Then
                                    Dim 段1() As String = 段(I).Split(分段符_单复数标识_前面, StringSplitOptions.RemoveEmptyEntries)
                                    If 段1.Length > 1 Then
                                        If 段1.Length > 2 Then
                                            For K = 0 To 段1.Length - 3
                                                替换后的文本 &= 段1(K)
                                            Next
                                        End If
                                        Select Case Val(要插入的文本(I))
                                            Case 1, 0 : 替换后的文本 &= 段1(0)
                                            Case Else : 替换后的文本 &= 段1(1)
                                        End Select
                                    Else
                                        GoTo Line1
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            Else
Line1:
                                替换后的文本 &= 要插入的文本(K) & 段(I)
                            End If
                        End If
                    Else
                        替换后的文本 &= 替换标识 & 段(I)
                    End If
                End If
            Next
            Return 替换后的文本
        Else
            Return 原文本
        End If
    End Function

End Class
