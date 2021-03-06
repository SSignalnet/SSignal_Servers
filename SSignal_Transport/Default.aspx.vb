﻿Imports System.Text.Encoding
Imports SSignal_Protocols
Imports SSignal_TransportCode

Public Class IO
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim 指令 As String = Request("C")
        If String.IsNullOrEmpty(指令) = True Then Return
        Dim 访问路径 As String = Request.Url.ToString
        If 访问路径.StartsWith("http://") Then
            Response.Clear()
            Response.End()
            Return
        ElseIf 域名验证 = False Then
            Dim 段() As String = 访问路径.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
            If 测试 = False Then
                If 段(1).EndsWith("." & 域名_英语) Then
                    域名验证 = True
                Else
                    Return
                End If
            Else
                If 段(1).EndsWith(获取服务器域名(讯宝中心服务器主机名 & "cnbj01." & 域名_英语)) Then
                    域名验证 = True
                Else
                    Return
                End If
            End If
        End If
        Dim 结果 As Object = Nothing
        Dim 允许跨域请求 As Boolean
        Try
            Dim 类 As New 类_处理请求(Application, Context, Request)
            Select Case 指令
                Case "SendSS" : 结果 = 类.收到讯宝
                Case "EnterTinyUniverse" : 结果 = 类.进入小宇宙
                Case "VerifySSServer" : 结果 = 类.验证我方真实性
                Case "AddContact" : 结果 = 类.添加讯友
                Case "JoinLargeGroup" : 结果 = 类.加入大聊天群
                Case "UserOnOrOff" : 结果 = 类.用户上线或离线
                Case "ServerStart" : 结果 = 类.验证启动
                Case "AdminCredential" : 结果 = 类.获取管理员连接凭据
                Case "GetServerInfo" : 结果 = 类.获取服务器信息
            End Select
        Catch ex As Exception
            结果 = New 类_SS包生成器(ex.Message)
        End Try
        Response.Clear()
        If 结果 IsNot Nothing Then
            If 允许跨域请求 Then Response.AddHeader("Access-Control-Allow-Origin", "*")
            Dim 字节数组() As Byte = Nothing
            If TypeOf 结果 Is 类_SS包生成器 Then
                字节数组 = 结果.生成SS包
                If 字节数组 IsNot Nothing Then
                    Response.AddHeader("content-length", 字节数组.Length)
                Else
                    Response.AddHeader("content-length", 0)
                End If
                Response.ContentType = "application/octet-stream"
            ElseIf TypeOf 结果 Is Byte() Then
                字节数组 = 结果
                Response.AddHeader("content-length", 字节数组.Length)
                Response.ContentType = "application/octet-stream"
            ElseIf TypeOf 结果 Is String Then
                If String.IsNullOrEmpty(结果) = False Then
                    字节数组 = UTF8.GetBytes(结果)
                    Response.AddHeader("content-length", 字节数组.Length)
                Else
                    Response.AddHeader("content-length", 0)
                End If
                Response.ContentType = "text/xml"
            End If
            If 字节数组 IsNot Nothing Then
                Response.OutputStream.Write(字节数组, 0, 字节数组.Length)
            End If
        End If
        Response.End()
    End Sub

End Class
