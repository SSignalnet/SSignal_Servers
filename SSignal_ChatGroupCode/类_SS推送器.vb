Imports System.Threading
Imports SSignal_GlobalCommonCode

Friend Class 类_SS推送器
    Implements IDisposable

    Dim SS管理器 As 类_SS管理器
    Dim 索引号 As Integer
    Dim 要推送的SS As 类_要推送的SS
    Dim 线程 As Thread

    Friend Sub New(ByVal 要推送的SS1 As 类_要推送的SS, ByVal 索引号1 As Integer, ByVal SS管理器1 As 类_SS管理器)
        要推送的SS = 要推送的SS1
        索引号 = 索引号1
        SS管理器 = SS管理器1
    End Sub

    Friend Sub 推送_启动新线程()
        要推送的SS.当前状态 = SS推送状态_常量集合.推送中
        线程 = New Thread(New ThreadStart(AddressOf 推送))
        线程.Start()
    End Sub

    Private Sub 推送()
        Try
            Dim SS包生成器 As New 类_SS包生成器(True)
            SS包生成器.添加_带标签("群编号", 要推送的SS.群编号)
            SS包生成器.添加_带标签("发送者", 要推送的SS.发送者英语地址)
            SS包生成器.添加_带标签("发送时间", 要推送的SS.发送时间)
            SS包生成器.添加_带标签("类型", 要推送的SS.类型)
            SS包生成器.添加_带标签("发送序号", 要推送的SS.发送序号)
            SS包生成器.添加_带标签("文本", 要推送的SS.文本)
            Select Case 要推送的SS.类型
                Case SS类型_常量集合.图片, SS类型_常量集合.短视频
                    SS包生成器.添加_带标签("宽度", 要推送的SS.宽度)
                    SS包生成器.添加_带标签("高度", 要推送的SS.高度)
            End Select
            Select Case 要推送的SS.类型
                Case SS类型_常量集合.语音, SS类型_常量集合.短视频
                    SS包生成器.添加_带标签("秒数", 要推送的SS.秒数)
            End Select
            With SS管理器.群目录(要推送的SS.群编号 - 1)
                Dim I As Integer
                If 要推送的SS.类型 < SS类型_常量集合.视频通话 Then
                    For I = 0 To .成员数 - 1
                        With .成员(I).用户
                            If .网络连接器_手机 IsNot Nothing AndAlso .手机连接步骤未完成 = False Then
                                Try
                                    If SS包生成器.发送SS包(.网络连接器_手机, .AES加密器_手机) = False Then
                                        .网络连接器_手机.Close()
                                        .网络连接器_手机 = Nothing
                                    End If
                                Catch ex As Exception
                                    .网络连接器_手机.Close()
                                    .网络连接器_手机 = Nothing
                                End Try
                            End If
                            If .网络连接器_电脑 IsNot Nothing AndAlso .电脑连接步骤未完成 = False Then
                                Try
                                    If SS包生成器.发送SS包(.网络连接器_电脑, .AES加密器_电脑) = False Then
                                        .网络连接器_电脑.Close()
                                        .网络连接器_电脑 = Nothing
                                    End If
                                Catch ex As Exception
                                    .网络连接器_电脑.Close()
                                    .网络连接器_电脑 = Nothing
                                End Try
                            End If
                        End With
                    Next
                Else
                    Dim 发送者英语地址 As String = 要推送的SS.发送者英语地址
                    For I = 0 To .成员数 - 1
                        If String.Compare(发送者英语地址, .成员(I).用户.英语SS地址) = 0 Then
                            Exit For
                        End If
                    Next
                    If I < .成员数 Then
                        With .成员(I).用户
                            If .网络连接器_手机 IsNot Nothing AndAlso .手机连接步骤未完成 = False Then
                                Try
                                    If SS包生成器.发送SS包(.网络连接器_手机, .AES加密器_手机) = False Then
                                        .网络连接器_手机.Close()
                                        .网络连接器_手机 = Nothing
                                    End If
                                Catch ex As Exception
                                    .网络连接器_手机.Close()
                                    .网络连接器_手机 = Nothing
                                End Try
                            End If
                            If .网络连接器_电脑 IsNot Nothing AndAlso .电脑连接步骤未完成 = False Then
                                Try
                                    If SS包生成器.发送SS包(.网络连接器_电脑, .AES加密器_电脑) = False Then
                                        .网络连接器_电脑.Close()
                                        .网络连接器_电脑 = Nothing
                                    End If
                                Catch ex As Exception
                                    .网络连接器_电脑.Close()
                                    .网络连接器_电脑 = Nothing
                                End Try
                            End If
                        End With
                    End If
                End If
            End With
        Catch ex As Exception
        Finally
            要推送的SS.当前状态 = SS推送状态_常量集合.结束
            SS管理器.SS推送器(索引号) = Nothing
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
                If SS管理器.SS推送器 IsNot Nothing Then
                    SS管理器.SS推送器(索引号) = Nothing
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
