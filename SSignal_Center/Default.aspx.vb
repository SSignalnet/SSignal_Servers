Imports System.Text.Encoding
Imports SSignal_CenterCode
Imports SSignal_Protocols

Public Class IO
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim 指令 As String = Request("C")
        If String.IsNullOrEmpty(指令) = True Then Return
        Dim 访问路径 As String = Request.Url.ToString
        If 访问路径.StartsWith("http://") Then
            Response.Clear()
            Response.End()
            Return
        ElseIf 域名验证 = False Then
            If 测试 = False Then
                If 访问路径.StartsWith("https://" & 讯宝中心服务器主机名 & "." & 域名_英语 & "/") Then
                    域名验证 = True
                Else
                    Return
                End If
            Else
                If 访问路径.StartsWith("https://" & 获取服务器域名(讯宝中心服务器主机名 & "." & 域名_英语) & "/") Then
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
                Case "VerifySSAddress" : 结果 = 类.验证讯宝地址真实性
                Case "VerifySSServer" : 结果 = 类.验证我方真实性
                Case "GetVCodePicture" : 结果 = 类.获取验证码图片

                Case "AccountInfo" : 结果 = 类.获取账户信息()
                Case "Login" : 结果 = 类.登录()
                Case "GetKeyIV" : 结果 = 类.获取密钥()
                Case "CreateGroup" : 结果 = 类.创建大聊天群()
                Case "DeleteGroup" : 结果 = 类.解散大聊天群()
                Case "ChangeOwner" : 结果 = 类.更换群主()
                Case "Logout" : 结果 = 类.注销()
                Case "Register" : 结果 = 类.注册()
                Case "VerifyPhoneOrEmail" : 结果 = 类.验证手机号或电子邮箱地址()
                Case "ForgotPassword" : 结果 = 类.忘记密码了()
                Case "ResetPassword" : 结果 = 类.重设密码()
                Case "SetAccountName" : 结果 = 类.设置用户名()
                Case "ChangePassword" : 结果 = 类.修改密码()
                Case "NewEmailAddress" : 结果 = 类.新电子邮箱地址()
                Case "VerifyNewEmailAddress" : 结果 = 类.验证新电子邮箱地址()
                Case "NewPhoneNumber" : 结果 = 类.新手机号()
                Case "VerifyNewPhoneNumber" : 结果 = 类.验证新手机号()

                Case "ServerStart" : 结果 = 类.服务器启动

                Case "AdminCredential" : 结果 = 类.获取服务器连接凭据
                Case "GetServerList" : 结果 = 类.备份数据库时获取服务器列表
                Case "GetReport" : 结果 = 类.获取报表
                Case "AdminLogin" : 结果 = 类.管理员登录
                Case "ListUsers" : 结果 = 类.列出用户()
                Case "ListServers" : 结果 = 类.列出服务器()
                Case "ReviseDuty" : 结果 = 类.修改职能()
                Case "DisableEnableAccount" : 结果 = 类.停用启用账户()
                Case "GetServerInfo" : 结果 = 类.获取服务器信息()
                Case "SetGoodsEditor" : 结果 = 类.设置商品编辑者
                'Case "ReportAppErrors" : 结果 = 类.报告程序故障()
                Case "PublishAPK" : 结果 = 类.发布安卓客户端软件()
                Case "AddServer" : 结果 = 类.添加服务器账号()
                Case "ReviseServer" : 结果 = 类.修改服务器账号()
                Case "DisableEnableServer" : 结果 = 类.停用启用服务器账号()
                Case "ReviseEmailBoxPassword" : 结果 = 类.修改邮箱密码()
                Case "InitialEmailBoxPassword" : 结果 = 类.设置邮箱初始密码()
                Case "AuthorizeRegister" : 结果 = 类.添加可注册者()
                Case "UnauthorizeRegister" : 结果 = 类.移除可注册者()
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
