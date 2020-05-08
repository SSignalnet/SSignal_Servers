Imports System.IO
Imports System.Text
Imports System.Text.Encoding

Public Module 模块_静态方法

    Public Function 获取文本库号(ByVal 字数 As Integer) As Short
        Dim 单位长度 As Integer
        If 字数 <= 100 Then
            单位长度 = 10
        ElseIf 字数 <= 300 Then
            单位长度 = 20
        ElseIf 字数 <= 700 Then
            单位长度 = 40
        ElseIf 字数 <= 1300 Then
            单位长度 = 60
        Else
            单位长度 = 100
        End If
        Return CInt(Math.Ceiling(字数 / 单位长度)) * 单位长度
    End Function

    Public Function 生成大小写英文字母与数字的随机字符串(ByVal 长度 As Short) As String
        If 长度 < 1 Then 长度 = 1
        Dim 字节数组(长度 - 1) As Byte
        Dim I, J As Short
        Randomize()
        For I = 0 To 长度 - 1
            J = Int(62 * Rnd())
            If J < 10 Then
                字节数组(I) = Int(10 * Rnd()) + 48
            ElseIf J < 36 Then
                字节数组(I) = Int(26 * Rnd()) + 65
            Else
                字节数组(I) = Int(26 * Rnd()) + 97
            End If
        Next
        Return ASCII.GetString(字节数组)
    End Function

    Public Function 替换XML敏感字符(ByVal 文本 As String) As String
        If String.IsNullOrEmpty(文本) = False Then
            Dim 字符数组() As Char = 文本.ToCharArray
            Dim 变长文本 As New StringBuilder(文本.Length * 2)
            Dim 文本写入器 As New StringWriter(变长文本)
            Dim I As Integer
            For I = 0 To 字符数组.Length - 1
                Select Case 字符数组(I)
                    Case "<" : 文本写入器.Write("&lt;")
                    Case ">" : 文本写入器.Write("&gt;")
                    Case "&" : 文本写入器.Write("&amp;")
                    Case ChrW(39) : 文本写入器.Write("&apos;")
                    Case ChrW(34) : 文本写入器.Write("&quot;")
                    Case Else : 文本写入器.Write(字符数组(I))
                End Select
            Next
            文本写入器.Close()
            Return 文本写入器.ToString
        Else
            Return ""
        End If
    End Function

End Module
