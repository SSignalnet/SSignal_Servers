Imports SSignal_Protocols
Imports SSignal_ServerCommonCode

Public Class TinyUniverse
    Inherits System.Web.UI.Page

    Dim 界面文字 As 类_界面文字

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim 访问路径 As String = Request.Url.ToString
        If 访问路径.StartsWith("http://") Then
            Response.Clear()
            Response.End()
            Return
        End If
        Dim 语言代码 As String = Request("LanguageCode")
        Select Case 语言代码
            Case 语言代码_中文, 语言代码_英语
            Case Else
                语言代码 = 语言代码_中文
        End Select
        If String.Compare(语言代码, 语言代码_中文) = 0 Then
            界面文字 = New 类_界面文字(语言代码)
        Else
            界面文字 = New 类_界面文字(语言代码, My.Resources.UItext)
        End If
    End Sub

    Protected Function WZ(ByVal 代码 As Integer, ByVal 现有文字 As String,
                       Optional ByVal 要插入的文本() As Object = Nothing) As String
        If 界面文字 IsNot Nothing Then
            Return 界面文字.获取(代码, 现有文字, 要插入的文本)
        Else
            Return 现有文字
        End If
    End Function

End Class