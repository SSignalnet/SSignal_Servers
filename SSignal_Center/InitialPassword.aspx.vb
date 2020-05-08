Imports System.IO
Imports SSignal_Protocols
Imports SSignal_CenterCode

Public Class InitialPassword
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim 访问路径 As String = Request.Url.ToString
        If 访问路径.StartsWith("http://") Then
            Dim 段() As String = 访问路径.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
            Dim HTTPS路径 As String = "https://" & 替换端口(段(1), 域名_英语) & Request.ApplicationPath
            If HTTPS路径.EndsWith("/") Then HTTPS路径 = HTTPS路径.Substring(0, HTTPS路径.Length - 1)
            Session("HttpsPath") = HTTPS路径
            Response.Redirect(HTTPS路径 & "/" & 段(2))
            Return
        End If
    End Sub

    Protected Function Language() As String
        Return Request.ServerVariables("HTTP_ACCEPT_LANGUAGE")
    End Function

    Protected Function EmailAddress() As String
        Dim 行() As String = File.ReadAllLines(Context.Server.MapPath("/") & "App_Data\domain.txt", Encoding.UTF8)
        If 行.Length <> 2 Then Return ""
        If 检查英语域名(行(0)) = False Then Return ""
        Return "noreply@" & 行(0)
    End Function

End Class