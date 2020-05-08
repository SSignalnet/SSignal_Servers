Imports SSignal_Protocols
Imports SSignal_ServerCommonCode
Imports SSignal_TinyUniverseCode

Public Class MyTinyUniverse
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

    Protected Function OtherJS() As String
        Return "var Domain_Read = '" & 分配数据读取服务器() & "'; var Domain_Write = '" & 分配数据写入服务器() & "'; var EnglishDomain = '" & 域名_英语 & "';"
    End Function

    Private Function 分配数据读取服务器() As String
        Dim 英语用户名 As String = Request("EnglishUsername")

        Return 获取服务器域名(讯宝小宇宙中心服务器主机名 & "." & 域名_英语)     '根据访问者英语讯宝地址分配读取服务器的代码请自己写
    End Function

    Private Function 分配数据写入服务器() As String
        Dim 英语用户名 As String = Request("EnglishUsername")

        Return 获取服务器域名(讯宝小宇宙中心服务器主机名 & "." & 域名_英语)    '根据英语用户名分配写入服务器的代码请自己写
    End Function

End Class