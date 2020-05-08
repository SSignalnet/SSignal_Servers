Imports System.Threading
Imports SSignal_Protocols

Friend Class 类_讯宝发送器
    Implements IDisposable

    Friend 讯宝域发送器 As 类_讯宝域发送器
    Friend 要发送的讯宝 As 类_要发送的讯宝
    Dim 线程 As Thread

    Friend Sub New(ByVal 讯宝域发送器1 As 类_讯宝域发送器)
        讯宝域发送器 = 讯宝域发送器1
        要发送的讯宝 = New 类_要发送的讯宝
        要发送的讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束
    End Sub

    Friend Sub 发送_启动新线程(ByVal 要发送的讯宝1 As 类_要发送的讯宝)
        要发送的讯宝 = 要发送的讯宝1
        要发送的讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.发送中
        要发送的讯宝.子域名_发送服务器 = 讯宝域发送器.讯宝管理器.本服务器主机名 & "." & 域名_英语
        要发送的讯宝.发送凭据 = 讯宝域发送器.访问其它服务器的凭据
        要发送的讯宝.子域名_接收服务器 = 讯宝域发送器.子域名_目标服务器
        线程 = New Thread(New ParameterizedThreadStart(AddressOf 传送服务器发送讯宝))
        线程.Start(要发送的讯宝)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                If 线程 IsNot Nothing Then
                    Try
                        线程.Abort()
                        线程 = Nothing
                    Catch ex As Exception
                    End Try
                End If
                If Me.Equals(讯宝域发送器.发送器1) Then
                    讯宝域发送器.发送器1 = Nothing
                ElseIf Me.Equals(讯宝域发送器.发送器2) Then
                    讯宝域发送器.发送器2 = Nothing
                End If
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

End Class
