Imports System.Threading
Imports SSignalDB

Public Class 类_短信管理器
    Implements IDisposable

    Dim 跨进程锁 As Mutex
    Dim 副数据库 As 类_数据库

    Dim 发送中的短信 As 类_短信

    Dim 线程 As Thread
    Dim 取消 As Boolean

    Public Sub New(ByVal 跨进程锁1 As Mutex, ByVal 副数据库1 As 类_数据库)
        跨进程锁 = 跨进程锁1
        副数据库 = 副数据库1
    End Sub

    Public Sub 发送短信()
        If 发送中的短信 IsNot Nothing Then Return
        If 跨进程锁.WaitOne = True Then
            If 发送中的短信 IsNot Nothing Then Return
            Dim 读取器 As 类_读取器_外部 = Nothing
            Try
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"时间", "手机号", "内容"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "短信发送", Nothing, 1, 列添加器, , 主键索引名)
                Dim 短信 As New 类_短信
                读取器 = 指令.执行()
                While 读取器.读取
                    短信.时间 = 读取器(0)
                    短信.手机号 = 读取器(1)
                    短信.内容 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                If 短信.时间 > 0 Then
                    Call 发送短信_启动新线程(短信)
                End If
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        End If
    End Sub

    Friend Sub 发送短信_启动新线程(ByVal 短信 As 类_短信)
        发送中的短信 = 短信
        线程 = New Thread(New ThreadStart(AddressOf 发送短信2))
        线程.Start()
    End Sub

    Private Sub 发送短信2()
        Dim 发送成功 As Boolean
        Dim 重试次数 As Short = 2




        If 发送成功 = True Then
            Call 删除短信(发送中的短信.时间)
        Else
            Thread.Sleep(60000)
        End If
        发送中的短信 = Nothing
        Call 发送短信()
    End Sub

    Private Sub 删除短信(ByVal 时间戳 As Long)
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于等于, 时间戳)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "短信发送", 筛选器, 主键索引名)
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
