Imports System.Threading
Imports System.Net
Imports System.Net.Mail
Imports System.Text.Encoding
Imports SSignalDB
Imports SSignal_ServerCommonCode

Public Class 类_邮件管理器
    Implements IDisposable

    Dim 跨进程锁 As Mutex
    Dim 副数据库 As 类_数据库

    Dim 发送中的邮件 As 类_邮件

    Dim 线程 As Thread
    Dim 取消 As Boolean
    Dim 密码 As String

    Public Sub New(ByVal 跨进程锁1 As Mutex, ByVal 副数据库1 As 类_数据库)
        跨进程锁 = 跨进程锁1
        副数据库 = 副数据库1
    End Sub

    Public Sub 发送邮件()
        If 发送中的邮件 IsNot Nothing Then Return
        If 跨进程锁.WaitOne = True Then
            If 发送中的邮件 IsNot Nothing Then Return
            Dim 读取器 As 类_读取器_外部 = Nothing
            Try
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"时间", "收件人", "标题", "正文"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "邮件发送", Nothing, 1, 列添加器, , 主键索引名)
                Dim 邮件 As New 类_邮件
                读取器 = 指令.执行()
                While 读取器.读取
                    邮件.时间 = 读取器(0)
                    邮件.收件人 = 读取器(1)
                    邮件.标题 = 读取器(2)
                    邮件.正文 = 读取器(3)
                    Exit While
                End While
                读取器.关闭()
                If 邮件.时间 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("类型", 筛选方式_常量集合.等于, 系统任务类型_常量集合.邮箱密码)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据("文本")
                    指令 = New 类_数据库指令_请求获取数据(副数据库, "系统任务", 筛选器, 1, 列添加器)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        密码 = 读取器(0)
                        Exit While
                    End While
                    读取器.关闭()
                    If String.IsNullOrEmpty(密码) = False Then
                        Call 发送邮件_启动新线程(邮件)
                    End If
                End If
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        End If
    End Sub

    Friend Sub 发送邮件_启动新线程(ByVal 邮件 As 类_邮件)
        发送中的邮件 = 邮件
        线程 = New Thread(New ThreadStart(AddressOf 发送邮件2))
        线程.Start()
    End Sub

    Private Sub 发送邮件2()
        Dim 邮箱地址 As String = "noreply@" & 域名_英语
        Dim 发送成功 As Boolean
        Dim 重试次数 As Short = 2
        Dim 电子邮件 As New MailMessage()
        电子邮件.From = New MailAddress(邮箱地址)
        电子邮件.To.Add(New MailAddress(发送中的邮件.收件人))
        电子邮件.SubjectEncoding = UTF8
        电子邮件.BodyEncoding = UTF8
        电子邮件.Subject = 发送中的邮件.标题
        电子邮件.Priority = MailPriority.High
        电子邮件.Body = 发送中的邮件.正文
Line1:
        Try
            Dim SMTP客户端 As New SmtpClient()
            SMTP客户端.Host = "smtp.ym.163.com"
            SMTP客户端.Port = 25
            SMTP客户端.UseDefaultCredentials = True
            SMTP客户端.Credentials = New NetworkCredential(邮箱地址, 密码)
            SMTP客户端.Timeout = 10000
            SMTP客户端.Send(电子邮件)
            电子邮件.Dispose()
            发送成功 = True
        Catch ex As Exception
            If 重试次数 > 0 Then
                重试次数 -= 1
                GoTo Line1
            End If
        End Try
        电子邮件.Dispose()
        If 发送成功 = True Then
            Call 删除邮件(发送中的邮件.时间)
        Else
            Thread.Sleep(60000)
        End If
        发送中的邮件 = Nothing
        Call 发送邮件()
    End Sub

    Private Sub 删除邮件(ByVal 时间戳 As Long)
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于等于, 时间戳)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "邮件发送", 筛选器, 主键索引名)
                指令2.执行()
            Catch ex As Exception
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                取消 = True
                If 线程 IsNot Nothing Then
                    Try
                        线程.Abort()
                        线程 = Nothing
                    Catch ex As Exception
                    End Try
                End If
            End If

            ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
            ' TODO:  将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
