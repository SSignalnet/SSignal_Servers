Imports SSignal_Protocols
Imports SSignal_ServerCommonCode

Friend Class 类_讯宝域发送器

    Friend 子域名_目标服务器 As String
    Friend 讯宝管理器 As 类_讯宝管理器
    Friend 发送器1, 发送器2 As 类_讯宝发送器
    Friend 访问其它服务器的凭据 As String

    Friend Sub New(ByVal 子域名_目标服务器1 As String, ByVal 讯宝管理器1 As 类_讯宝管理器)
        子域名_目标服务器 = 子域名_目标服务器1
        讯宝管理器 = 讯宝管理器1
        发送器1 = New 类_讯宝发送器(Me)
        发送器2 = New 类_讯宝发送器(Me)
    End Sub

    Friend Sub 发送(ByVal 讯宝 As 类_要发送的讯宝)
        If String.IsNullOrEmpty(访问其它服务器的凭据) Then
            If 讯宝管理器.跨进程锁.WaitOne = True Then
                Try
                    Dim 结果 As 类_SS包生成器 = 数据库_获取访问其它服务器的凭据(讯宝管理器.副数据库, 子域名_目标服务器, 访问其它服务器的凭据)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        GoTo 跳转点1
                    End If
                Catch ex As Exception
                    GoTo 跳转点1
                Finally
                    讯宝管理器.跨进程锁.ReleaseMutex()
                End Try
            Else
跳转点1:
                讯宝.发送失败的原因 = 讯宝指令_常量集合.我方服务器程序出错
                讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束
                Return
            End If
            If String.IsNullOrEmpty(访问其它服务器的凭据) Then
                Dim 结果 As 类_SS包生成器 = 添加我方访问其它服务器的凭据(讯宝管理器.跨进程锁, 讯宝管理器.副数据库, 子域名_目标服务器, 访问其它服务器的凭据)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    GoTo 跳转点1
                End If
            End If
        End If
        If 发送器1.要发送的讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束 Then
            发送器1.发送_启动新线程(讯宝)
        ElseIf 发送器2.要发送的讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束 Then
            发送器2.发送_启动新线程(讯宝)
        End If
    End Sub

End Class
