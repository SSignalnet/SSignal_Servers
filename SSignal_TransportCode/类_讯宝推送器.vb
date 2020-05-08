Imports System.Threading
Imports SSignal_Protocols

Friend Class 类_讯宝推送器
    Implements IDisposable

    Dim 讯宝管理器 As 类_讯宝管理器
    Dim 索引号 As Integer
    Dim 要推送的讯宝 As 类_要推送的讯宝
    Dim 当前用户 As 类_用户
    Dim 线程 As Thread

    Friend Sub New(ByVal 要推送的讯宝1 As 类_要推送的讯宝, ByVal 索引号1 As Integer, ByVal 讯宝管理器1 As 类_讯宝管理器,
                   Optional ByVal 当前用户1 As 类_用户 = Nothing)
        要推送的讯宝 = 要推送的讯宝1
        索引号 = 索引号1
        讯宝管理器 = 讯宝管理器1
        当前用户 = 当前用户1
    End Sub

    Friend Sub 推送_启动新线程()
        If 当前用户 Is Nothing Then
            当前用户 = 讯宝管理器.用户目录(要推送的讯宝.位置号)
            If 当前用户.用户编号 = 要推送的讯宝.接收者编号 Then
跳转点1:
                要推送的讯宝.当前状态 = 讯宝推送状态_常量集合.推送中
                线程 = New Thread(New ThreadStart(AddressOf 推送))
                线程.Start()
            Else
                要推送的讯宝.当前状态 = 讯宝推送状态_常量集合.结束
                讯宝管理器.讯宝推送器(索引号) = Nothing
            End If
        Else
            GoTo 跳转点1
        End If
    End Sub

    Private Sub 推送()
        Try
            Dim SS包生成器 As New 类_SS包生成器()
            SS包生成器.添加_有标签("指令", 要推送的讯宝.讯宝指令)
            SS包生成器.添加_有标签("发送者", 要推送的讯宝.发送者英语讯宝地址)
            SS包生成器.添加_有标签("发送时间", 要推送的讯宝.发送时间)
            If 要推送的讯宝.群编号 > 0 Then
                SS包生成器.添加_有标签("群编号", 要推送的讯宝.群编号)
            End If
            If String.IsNullOrEmpty(要推送的讯宝.群主讯宝地址) = False Then
                SS包生成器.添加_有标签("群主", 要推送的讯宝.群主讯宝地址)
            End If
            SS包生成器.添加_有标签("发送序号", 要推送的讯宝.发送序号)
            If 要推送的讯宝.讯宝指令 < 讯宝指令_常量集合.手机和电脑同步 Then
                SS包生成器.添加_有标签("文本", 要推送的讯宝.文本)
                Select Case 要推送的讯宝.讯宝指令
                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                        SS包生成器.添加_有标签("宽度", 要推送的讯宝.宽度)
                        SS包生成器.添加_有标签("高度", 要推送的讯宝.高度)
                End Select
                If 要推送的讯宝.讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                    SS包生成器.添加_有标签("秒数", 要推送的讯宝.秒数)
                End If
跳转点1:
                If 要推送的讯宝.设备类型 <> 设备类型_常量集合.电脑 Then
                    If 当前用户.网络连接器_手机 IsNot Nothing Then
                        Dim 次数 As Integer
                        While 当前用户.手机连接步骤未完成 AndAlso 次数 < 20
                            Thread.Sleep(1000)
                            次数 += 1
                        End While
                        Try
                            If SS包生成器.发送SS包(当前用户.网络连接器_手机, 当前用户.AES加密器_手机) = False Then
                                当前用户.网络连接器_手机.Close()
                                当前用户.网络连接器_手机 = Nothing
                            End If
                        Catch ex As Exception
                            当前用户.网络连接器_手机.Close()
                            当前用户.网络连接器_手机 = Nothing
                        End Try
                    End If
                End If
                If 要推送的讯宝.设备类型 <> 设备类型_常量集合.手机 Then
                    If 当前用户.网络连接器_电脑 IsNot Nothing Then
                        Dim 次数 As Integer
                        While 当前用户.电脑连接步骤未完成 AndAlso 次数 < 20
                            Thread.Sleep(1000)
                            次数 += 1
                        End While
                        Try
                            If SS包生成器.发送SS包(当前用户.网络连接器_电脑, 当前用户.AES加密器_电脑) = False Then
                                当前用户.网络连接器_电脑.Close()
                                当前用户.网络连接器_电脑 = Nothing
                            End If
                        Catch ex As Exception
                            当前用户.网络连接器_电脑.Close()
                            当前用户.网络连接器_电脑 = Nothing
                        End Try
                    End If
                End If
            Else
                Select Case 要推送的讯宝.讯宝指令
                    Case 讯宝指令_常量集合.手机和电脑同步, 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友
                        SS包生成器.添加_有标签("文本", 要推送的讯宝.文本)
                End Select
                Select Case 要推送的讯宝.设备类型
                    Case 设备类型_常量集合.手机
                        Dim 次数 As Integer
                        While 当前用户.手机连接步骤未完成 AndAlso 次数 < 20
                            Thread.Sleep(1000)
                            次数 += 1
                        End While
                        If SS包生成器.发送SS包(当前用户.网络连接器_手机, 当前用户.AES加密器_手机) = False Then
                            当前用户.网络连接器_手机.Close()
                            当前用户.网络连接器_手机 = Nothing
                        End If
                    Case 设备类型_常量集合.电脑
                        Dim 次数 As Integer
                        While 当前用户.电脑连接步骤未完成 AndAlso 次数 < 20
                            Thread.Sleep(1000)
                            次数 += 1
                        End While
                        If SS包生成器.发送SS包(当前用户.网络连接器_电脑, 当前用户.AES加密器_电脑) = False Then
                            当前用户.网络连接器_电脑.Close()
                            当前用户.网络连接器_电脑 = Nothing
                        End If
                    Case Else
                        GoTo 跳转点1
                End Select
            End If
        Catch ex As Exception
        Finally
            要推送的讯宝.当前状态 = 讯宝推送状态_常量集合.结束
            讯宝管理器.讯宝推送器(索引号) = Nothing
        End Try
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
                If 讯宝管理器.讯宝推送器 IsNot Nothing Then
                    讯宝管理器.讯宝推送器(索引号) = Nothing
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
