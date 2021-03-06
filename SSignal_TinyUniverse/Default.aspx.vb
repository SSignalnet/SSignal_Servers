﻿Imports System.Text.Encoding
Imports SSignal_TinyUniverseCode
Imports SSignal_Protocols

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
            If 测试 = False Then
                If 访问路径.StartsWith("https://" & 讯宝小宇宙中心服务器主机名 & "." & 域名_英语 & "/") Then
                    域名验证 = True
                Else
                    Return
                End If
            Else
                If 访问路径.StartsWith("https://" & 获取服务器域名(讯宝小宇宙中心服务器主机名 & "." & 域名_英语) & "/") Then
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
                Case "ListMeteorRains" : 结果 = 类.获取流星语列表
                Case "GetMeteorRain" : 结果 = 类.获取流星语
                Case "MoreComments" : 结果 = 类.获取更多评论
                Case "MoreReplies" : 结果 = 类.获取更多回复
                Case "ListGoods" : 结果 = 类.获取商品列表 : 允许跨域请求 = True
                Case "ViewGoods" : 结果 = 类.查看商品 : 允许跨域请求 = True
                Case "SyncSSpalList" : 结果 = 类.同步讯友录
                Case "PostMeteorRain" : 结果 = 类.发布流星语
                Case "PostComment" : 结果 = 类.评论流星语
                Case "PostReply" : 结果 = 类.回复评论
                Case "GetAServerForRead" : 结果 = 类.分配数据读取服务器
                Case "PostGoods" : 结果 = 类.发布商品
                Case "Sticky" : 结果 = 类.设为置顶
                Case "CancelSticky" : 结果 = 类.取消置顶
                Case "MoveToFront" : 结果 = 类.商品移至最前
                Case "ChangePermission" : 结果 = 类.更改访问权限
                Case "DeleteMeteorRain" : 结果 = 类.删除流星语
                Case "DeleteComment" : 结果 = 类.删除评论
                Case "DeleteReply" : 结果 = 类.删除回复
                Case "DeleteGoods" : 结果 = 类.删除商品
                Case "GetCredential" : 结果 = 类.获取连接凭据
                Case "VerifySSServer" : 结果 = 类.验证我方真实性
                Case "ServerStart" : 结果 = 类.验证启动
                Case "AdminCredential" : 结果 = 类.获取管理员连接凭据
                Case "GetServerInfo" : 结果 = 类.获取服务器信息
                Case "SetGoodsEditor" : 结果 = 类.设置商品编辑者
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
