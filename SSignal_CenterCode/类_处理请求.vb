Imports System.Web
Imports System.Threading
Imports SSignalDB

Public Class 类_处理请求

    Dim 应用程序 As HttpApplicationState
    Dim 跨进程锁 As Mutex
    Dim 主数据库, 副数据库 As 类_数据库
    Dim 短信管理器 As 类_短信管理器
    Dim 邮件管理器 As 类_邮件管理器
    Dim Context As HttpContext
    Dim Http请求 As HttpRequest

    Public Sub New(ByVal 应用程序1 As HttpApplicationState, ByVal Context1 As HttpContext, ByVal Http请求1 As HttpRequest)
        应用程序 = 应用程序1
        If 应用程序 IsNot Nothing Then
            跨进程锁 = 应用程序.Get("Mu_SSC")
            主数据库 = 应用程序.Get("Rb_SSC")
            副数据库 = 应用程序.Get("Nb_SSC")
            短信管理器 = 应用程序.Get("Sm_SSC")
            邮件管理器 = 应用程序.Get("Em_SSC")
        End If
        Context = Context1
        Http请求 = Http请求1
    End Sub

End Class
