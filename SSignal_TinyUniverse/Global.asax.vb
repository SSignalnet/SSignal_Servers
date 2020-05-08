Imports System.Threading
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_TinyUniverseCode

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' 应用程序启动时激发
        Try
            Dim 行() As String = File.ReadAllLines(Context.Server.MapPath("/") & "App_Data\domain.txt", Encoding.UTF8)
            If 行.Length <> 2 Then Return
            If 检查英语域名(行(0)) = False Then Return
            域名_英语 = 行(0)
            If String.Compare(行(0), 行(1), True) <> 0 Then
                If 检查本国语域名(行(1), 语言代码_中文) = False Then Return
                域名_本国语 = 行(1)
            Else
                域名_本国语 = Nothing
            End If
        Catch ex As Exception
            Return
        End Try
        Dim 跨进程锁 As New Mutex(False, "Mu_SSU")
        Dim 类 As New 类_打开或创建数据库
        Dim 副数据库 As 类_数据库 = 类.打开或创建副数据库(Context)
        If 副数据库 IsNot Nothing Then
            Dim 主数据库 As 类_数据库 = 类.打开或创建主数据库(Context, 副数据库)
            If 主数据库 IsNot Nothing Then
                Application.Add("Mu_SSU", 跨进程锁)
                Application.Add("Rb_SSU", 主数据库)
                Application.Add("Nb_SSU", 副数据库)
                Dim 启动器 As New 类_启动器(跨进程锁, 副数据库)
                Application.Add("Ln_SSU", 启动器)
                启动器.启动()
            End If
        End If
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' 会话启动时激发
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' 每个请求开始时激发
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' 尝试验证用户身份时激发
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' 发生错误时激发
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' 会话结束时激发
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' 应用程序结束时激发
        Dim 主数据库 As 类_数据库 = Application.Get("Rb_SSU")
        If 主数据库 IsNot Nothing Then 主数据库.关闭()
        Dim 副数据库 As 类_数据库 = Application.Get("Nb_SSU")
        If 副数据库 IsNot Nothing Then 副数据库.关闭()
        Dim 跨进程锁 As Mutex = Application.Get("Mu_SSU")
        If 跨进程锁 IsNot Nothing Then 跨进程锁.Dispose()
    End Sub

End Class
