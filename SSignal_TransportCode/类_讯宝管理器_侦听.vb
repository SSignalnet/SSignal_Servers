Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text.Encoding
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Partial Public Class 类_讯宝管理器

    Private Structure 加入的小聊天群_复合数据
        Dim 群主英语讯宝地址, 群备注 As String
        Dim 群编号 As Byte
        Dim 移除 As Boolean
    End Structure

    Private Structure 加入的大聊天群_复合数据
        Dim 主机名, 英语域名, 本国语域名, 群名称 As String
        Dim 群编号 As Long
    End Structure

    Private Structure 讯友_复合数据
        Dim 英语讯宝地址, 本国语讯宝地址, 备注, 标签一, 标签二, 主机名 As String
        Dim 位置号 As Short
        Dim 拉黑 As Boolean
    End Structure

    Private Sub 侦听()
        Try
            Dim 地址和端口 As New IPEndPoint(IPAddress.Any, 获取传送服务器侦听端口(域名_英语))
            网络连接器_侦听 = New Socket(地址和端口.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            网络连接器_侦听.Bind(地址和端口)
            网络连接器_侦听.Listen(最大值_常量集合.传送服务器承载用户数 / 5)
            Dim 网络连接器 As Socket
            Dim 错误信息 As String = ""
            Do
                Try
                    网络连接器 = 网络连接器_侦听.Accept
                    If 关闭 = False Then
                        Dim 线程 As New Thread(New ParameterizedThreadStart(AddressOf 新连接))
                        线程.Start(网络连接器)
                    End If
                Catch ex As Exception
                End Try
            Loop Until 关闭 = True
        Catch ex As Exception
        End Try
    End Sub

    Private Sub 新连接(ByVal 网络连接器1 As Object)
        Dim 网络连接器 As Socket = CType(网络连接器1, Socket)
        Dim 设备类型 As 设备类型_常量集合
        Dim 发送者 As 类_用户 = Nothing
        Try
            网络连接器.ReceiveTimeout = 收发时限
            网络连接器.SendTimeout = 收发时限
            Dim SS包解读器 As New 类_SS包解读器(网络连接器, , 500)
            Dim 位置号 As Short
            SS包解读器.读取(位置号)
            If 位置号 < 0 OrElse 位置号 >= 最大值_常量集合.传送服务器承载用户数 Then Throw New Exception
            SS包解读器.读取字节(设备类型)
            Dim 字节数组() As Byte = Nothing
            发送者 = 用户目录(位置号)
            If 发送者.停用 = True Then Exit Try
            Dim 密钥创建时间 As Long
            SS包解读器.读取(密钥创建时间)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 密钥创建时间 = 发送者.AES密钥创建时间_手机 Then
                        网络连接器.Send(ASCII.GetBytes("Y"))
                    Else
                        网络连接器.Send(ASCII.GetBytes("N"))
                        Thread.Sleep(1000)
                        Exit Try
                    End If
                    SS包解读器.读取(字节数组, 长度信息字节数_常量集合.两字节, 发送者.AES解密器_手机)
                Case 设备类型_常量集合.电脑
                    If 密钥创建时间 = 发送者.AES密钥创建时间_电脑 Then
                        网络连接器.Send(ASCII.GetBytes("Y"))
                    Else
                        网络连接器.Send(ASCII.GetBytes("N"))
                        Thread.Sleep(1000)
                        Exit Try
                    End If
                    SS包解读器.读取(字节数组, 长度信息字节数_常量集合.两字节, 发送者.AES解密器_电脑)
                Case Else : Throw New Exception
            End Select
            SS包解读器 = New 类_SS包解读器(字节数组)
            Dim 用户编号 As Long
            SS包解读器.读取_有标签("用户编号", 用户编号)
            If 用户编号 <> 发送者.用户编号 OrElse 用户编号 <= 0 Then Throw New Exception
            Dim 验证码 As String = Nothing
            SS包解读器.读取_有标签("验证码", 验证码)
            If String.IsNullOrEmpty(验证码) Then Throw New Exception
            If 验证码.Length <> 长度_常量集合.验证码 Then Throw New Exception
            Dim 讯友录更新时间_客户端 As Long
            SS包解读器.读取_有标签("讯友录更新时间", 讯友录更新时间_客户端)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    发送者.手机连接步骤未完成 = True
                    If 发送者.网络连接器_手机 IsNot Nothing Then 发送者.网络连接器_手机.Close()
                    发送者.网络连接器_手机 = 网络连接器
                Case 设备类型_常量集合.电脑
                    发送者.电脑连接步骤未完成 = True
                    If 发送者.网络连接器_电脑 IsNot Nothing Then 发送者.网络连接器_电脑.Close()
                    发送者.网络连接器_电脑 = 网络连接器
            End Select
            Dim 不推送的讯宝(99) As 类_要推送的讯宝
            Dim 不推送的讯宝数量, 不推送的讯宝数量2 As Integer
            Dim 讯友(最大值_常量集合.讯友数量 / 10 - 1) As 讯友_复合数据
            Dim 加入的小聊天群(最大值_常量集合.每个用户可加入的小聊天群数量) As 加入的小聊天群_复合数据
            Dim 加入的大聊天群(最大值_常量集合.每个用户可加入的大聊天群数量) As 加入的大聊天群_复合数据
            Dim 讯友数量, 加入的小聊天群数量, 加入的大聊天群数量 As Short
            Dim 黑域(99), 白域(99) As 域名_复合数据
            Dim 黑域数量, 白域数量 As Integer
            Dim 结果 As 类_SS包生成器 = Nothing
            Dim 讯友录更新时间_服务器 As Long
            Dim I As Integer
            Dim 发送者英语讯宝地址 As String = 发送者.英语用户名 & 讯宝地址标识 & 域名_英语
            If 跨进程锁.WaitOne = True Then
                Try
                    结果 = 数据库_获取不推送的讯宝(用户编号, 设备类型, 不推送的讯宝, 不推送的讯宝数量)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    结果 = 数据库_获取讯友录更新时间(用户编号, 讯友录更新时间_服务器)
                    If 讯友录更新时间_客户端 <> 讯友录更新时间_服务器 Then
                        结果 = 数据库_获取讯友录(用户编号, 讯友, 讯友数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                        If 加入的小聊天群数量 > 0 Then
                            If 讯友数量 > 0 Then
                                Dim 群主讯宝地址 As String
                                Dim J As Integer
                                For I = 0 To 加入的小聊天群数量 - 1
                                    群主讯宝地址 = 加入的小聊天群(I).群主英语讯宝地址
                                    If String.Compare(群主讯宝地址, 发送者英语讯宝地址) <> 0 Then
                                        For J = 0 To 讯友数量 - 1
                                            If String.Compare(群主讯宝地址, 讯友(J).英语讯宝地址) = 0 Then Exit For
                                        Next
                                        If J = 讯友数量 Then 加入的小聊天群(I).移除 = True
                                    End If
                                Next
                                For I = 0 To 加入的小聊天群数量 - 1
                                    If 加入的小聊天群(I).移除 = False Then Exit For
                                Next
                                If I < 加入的小聊天群数量 Then
                                    For I = 0 To 加入的小聊天群数量 - 1
                                        If 加入的小聊天群(I).移除 Then
                                            With 加入的小聊天群(I)
                                                结果 = 数据库_移除加入的群(用户编号, .群主英语讯宝地址, .群编号)
                                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                            End With
                                        End If
                                    Next
                                Else
                                    结果 = 数据库_移除加入的群(用户编号)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                    加入的小聊天群数量 = 0
                                End If
                            Else
                                For I = 0 To 加入的小聊天群数量 - 1
                                    If String.Compare(加入的小聊天群(I).群主英语讯宝地址, 发送者英语讯宝地址) <> 0 Then
                                        With 加入的小聊天群(I)
                                            结果 = 数据库_移除加入的群(用户编号, .群主英语讯宝地址, .群编号)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                        End With
                                    End If
                                Next
                            End If
                        End If
                        结果 = 数据库_获取白域(用户编号, 白域, 白域数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                        结果 = 数据库_获取黑域(用户编号, 黑域, 黑域数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    End If
                    结果 = 数据库_获取加入的小聊天群(用户编号, 加入的小聊天群, 加入的小聊天群数量)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    结果 = 数据库_获取加入的大聊天群(用户编号, 加入的大聊天群, 加入的大聊天群数量)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Throw New Exception
            End If
            Dim SS包生成器 As New 类_SS包生成器()
            SS包生成器.添加_有标签("验证码", 验证码)
            SS包生成器.添加_有标签("讯友录更新时间", 讯友录更新时间_服务器)
            Dim 头像路径 As String = 头像存放目录 & "\" & 发送者.英语用户名 & ".jpg"
            Dim 文件信息 As New FileInfo(头像路径)
            If 文件信息.Exists Then 发送者.头像更新时间 = 文件信息.LastWriteTimeUtc.Ticks
            SS包生成器.添加_有标签("头像更新时间", 发送者.头像更新时间)
            If 讯友录更新时间_客户端 <> 讯友录更新时间_服务器 Then
                If 讯友数量 > 0 Then
                    Dim SS包生成器2 As New 类_SS包生成器()
                    For I = 0 To 讯友数量 - 1
                        Dim SS包生成器3 As New 类_SS包生成器()
                        With 讯友(I)
                            SS包生成器3.添加_有标签("英语讯宝地址", .英语讯宝地址)
                            If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                                SS包生成器3.添加_有标签("本国语讯宝地址", .本国语讯宝地址)
                            End If
                            SS包生成器3.添加_有标签("备注", .备注)
                            If String.IsNullOrEmpty(.标签一) = False Then
                                SS包生成器3.添加_有标签("标签一", .标签一)
                            End If
                            If String.IsNullOrEmpty(.标签二) = False Then
                                SS包生成器3.添加_有标签("标签二", .标签二)
                            End If
                            SS包生成器3.添加_有标签("主机名", .主机名)
                            SS包生成器3.添加_有标签("拉黑", .拉黑)
                            SS包生成器3.添加_有标签("位置号", .位置号)
                        End With
                        SS包生成器2.添加_有标签("讯友", SS包生成器3)
                    Next
                    SS包生成器.添加_有标签("讯友录", SS包生成器2)
                End If
                If 白域数量 > 0 Then
                    Dim SS包生成器2 As New 类_SS包生成器()
                    For I = 0 To 白域数量 - 1
                        Dim SS包生成器3 As New 类_SS包生成器()
                        With 白域(I)
                            SS包生成器3.添加_有标签("英语", .英语)
                            If String.IsNullOrEmpty(.本国语) = False Then
                                SS包生成器3.添加_有标签("本国语", .本国语)
                            End If
                        End With
                        SS包生成器2.添加_有标签("域名", SS包生成器3)
                    Next
                    SS包生成器.添加_有标签("白域", SS包生成器2)
                End If
                If 黑域数量 > 0 Then
                    Dim SS包生成器2 As New 类_SS包生成器()
                    For I = 0 To 黑域数量 - 1
                        Dim SS包生成器3 As New 类_SS包生成器()
                        With 黑域(I)
                            SS包生成器3.添加_有标签("英语", .英语)
                            If String.IsNullOrEmpty(.本国语) = False Then
                                SS包生成器3.添加_有标签("本国语", .本国语)
                            End If
                        End With
                        SS包生成器2.添加_有标签("域名", SS包生成器3)
                    Next
                    SS包生成器.添加_有标签("黑域", SS包生成器2)
                End If
            End If
            If 加入的小聊天群数量 > 0 Then
                Dim SS包生成器2 As New 类_SS包生成器()
                For I = 0 To 加入的小聊天群数量 - 1
                    Dim SS包生成器3 As New 类_SS包生成器()
                    With 加入的小聊天群(I)
                        If .移除 = False Then
                            SS包生成器3.添加_有标签("群主", .群主英语讯宝地址)
                            SS包生成器3.添加_有标签("群编号", .群编号)
                            SS包生成器3.添加_有标签("群备注", .群备注)
                        End If
                    End With
                    SS包生成器2.添加_有标签("群", SS包生成器3)
                Next
                SS包生成器.添加_有标签("小聊天群", SS包生成器2)
            End If
            If 加入的大聊天群数量 > 0 Then
                Dim SS包生成器2 As New 类_SS包生成器()
                For I = 0 To 加入的大聊天群数量 - 1
                    Dim SS包生成器3 As New 类_SS包生成器()
                    With 加入的大聊天群(I)
                        SS包生成器3.添加_有标签("主机名", .主机名)
                        SS包生成器3.添加_有标签("英语域名", .英语域名)
                        If String.IsNullOrEmpty(.本国语域名) = False Then
                            SS包生成器3.添加_有标签("本国语域名", .本国语域名)
                        End If
                        SS包生成器3.添加_有标签("群编号", .群编号)
                        SS包生成器3.添加_有标签("群名称", .群名称)
                    End With
                    SS包生成器2.添加_有标签("群", SS包生成器3)
                Next
                SS包生成器.添加_有标签("大聊天群", SS包生成器2)
            End If
            If 不推送的讯宝数量 > 0 Then
                Dim SS包生成器2 As New 类_SS包生成器()
                For I = 0 To 不推送的讯宝数量 - 1
                    Dim SS包生成器3 As New 类_SS包生成器()
                    With 不推送的讯宝(I)
                        If .讯宝指令 >= 讯宝指令_常量集合.手机和电脑同步 Then
                            不推送的讯宝数量2 += 1
                            Continue For
                        End If
                        SS包生成器3.添加_有标签("发送者", .发送者英语讯宝地址)
                        SS包生成器3.添加_有标签("发送时间", .发送时间)
                        SS包生成器3.添加_有标签("指令", .讯宝指令)
                        SS包生成器3.添加_有标签("发送序号", .发送序号)
                        If .群编号 > 0 Then
                            SS包生成器3.添加_有标签("群编号", .群编号)
                            SS包生成器3.添加_有标签("群主", .群主讯宝地址)
                        End If
                        SS包生成器3.添加_有标签("文本", .文本)
                        Select Case .讯宝指令
                            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                SS包生成器3.添加_有标签("宽度", .宽度)
                                SS包生成器3.添加_有标签("高度", .高度)
                        End Select
                        If .讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                            SS包生成器3.添加_有标签("秒数", .秒数)
                        End If
                    End With
                    SS包生成器2.添加_有标签("讯宝", SS包生成器3)
                Next
                If SS包生成器2.数据量 > 0 Then
                    SS包生成器.添加_有标签("新讯宝", SS包生成器2)
                End If
            End If
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 发送者.讯宝序号_手机发送 = 0 Then 发送者.讯宝序号_手机发送 = Date.UtcNow.Ticks
                    If 发送者.讯宝群消息序号 = 0 Then 发送者.讯宝群消息序号 = -发送者.讯宝序号_手机发送
                    SS包生成器.添加_有标签("发送序号", 发送者.讯宝序号_手机发送)
                    SS包生成器.添加_有标签("在线", CBool(IIf(发送者.网络连接器_电脑 IsNot Nothing, True, False)))
                    If SS包生成器.发送SS包(网络连接器, 发送者.AES加密器_手机) = False Then Throw New Exception
                Case 设备类型_常量集合.电脑
                    If 发送者.讯宝序号_电脑发送 = 0 Then 发送者.讯宝序号_电脑发送 = Date.UtcNow.Ticks
                    If 发送者.讯宝群消息序号 = 0 Then 发送者.讯宝群消息序号 = -发送者.讯宝序号_电脑发送
                    SS包生成器.添加_有标签("发送序号", 发送者.讯宝序号_电脑发送)
                    SS包生成器.添加_有标签("在线", CBool(IIf(发送者.网络连接器_手机 IsNot Nothing, True, False)))
                    If SS包生成器.发送SS包(网络连接器, 发送者.AES加密器_电脑) = False Then Throw New Exception
            End Select
            Select Case 设备类型
                Case 设备类型_常量集合.手机 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_手机)
                Case 设备类型_常量集合.电脑 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_电脑)
            End Select
            Dim 数量 As Integer
            If 讯友录更新时间_客户端 <> 讯友录更新时间_服务器 Then
                SS包解读器.读取_有标签("讯友数量", 数量, 0)
                If 讯友数量 > 0 Then
                    If 数量 <> 讯友数量 Then Throw New Exception
                Else
                    If 数量 <> 0 Then Throw New Exception
                End If
                SS包解读器.读取_有标签("小聊天群数量", 数量, 0)
                If 数量 <> 加入的小聊天群数量 Then Throw New Exception
                SS包解读器.读取_有标签("大聊天群数量", 数量, 0)
                If 数量 <> 加入的大聊天群数量 Then Throw New Exception
            End If
            SS包解读器.读取_有标签("讯宝数量", 数量, 0)
            If 数量 <> 不推送的讯宝数量 - 不推送的讯宝数量2 Then Throw New Exception
            If 不推送的讯宝数量 > 0 Then
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_清除不推送的讯宝(用户编号, 设备类型, 不推送的讯宝(不推送的讯宝数量 - 1).发送时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                End If
            End If
            Dim 新网络地址() As Byte = CType(网络连接器.RemoteEndPoint, IPEndPoint).Address.GetAddressBytes
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 发送者.网络连接器_电脑 IsNot Nothing Then
                        Dim 通知 As Boolean = True
                        If 发送者.网络地址_手机 IsNot Nothing Then
                            Dim 旧网络地址() As Byte = 发送者.网络地址_手机
                            If 新网络地址.Length = 旧网络地址.Length Then
                                For I = 0 To 新网络地址.Length - 1
                                    If 新网络地址(I) <> 旧网络地址(I) Then Exit For
                                Next
                                If I = 新网络地址.Length Then 通知 = False
                            End If
                        End If
                        If 通知 = True Then
                            SS包生成器 = New 类_SS包生成器()
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.手机上线)
                            If 跨进程锁.WaitOne = True Then
                                Try
                                    Call 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 0, Nothing, 0, 用户编号, 位置号, 设备类型_常量集合.电脑, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                                Catch ex As Exception
                                    Throw ex
                                Finally
                                    跨进程锁.ReleaseMutex()
                                End Try
                            End If
                        End If
                    End If
                    发送者.网络地址_手机 = 新网络地址
                    发送者.手机连接步骤未完成 = False
                Case 设备类型_常量集合.电脑
                    If 发送者.网络连接器_手机 IsNot Nothing Then
                        Dim 通知 As Boolean = True
                        If 发送者.网络地址_电脑 IsNot Nothing Then
                            Dim 旧网络地址() As Byte = 发送者.网络地址_电脑
                            If 新网络地址.Length = 旧网络地址.Length Then
                                For I = 0 To 新网络地址.Length - 1
                                    If 新网络地址(I) <> 旧网络地址(I) Then Exit For
                                Next
                                If I = 新网络地址.Length Then 通知 = False
                            End If
                        End If
                        If 通知 = True Then
                            SS包生成器 = New 类_SS包生成器()
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.电脑上线)
                            If 跨进程锁.WaitOne = True Then
                                Try
                                    Call 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 0, Nothing, 0, 用户编号, 位置号, 设备类型_常量集合.手机, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                                Catch ex As Exception
                                    Throw ex
                                Finally
                                    跨进程锁.ReleaseMutex()
                                End Try
                            End If
                        End If
                    End If
                    发送者.网络地址_电脑 = 新网络地址
                    发送者.电脑连接步骤未完成 = False
            End Select
            网络连接器.ReceiveTimeout = 0
            Do
                Select Case 设备类型
                    Case 设备类型_常量集合.手机 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_手机)
                    Case 设备类型_常量集合.电脑 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_电脑)
                    Case Else : Throw New Exception
                End Select
                发送讯宝(SS包解读器, 发送者, 位置号, 设备类型)
            Loop Until 关闭
        Catch ex As Exception
            'Try
            '    File.WriteAllText(数据存放路径 & Date.Now.Ticks & ".txt", ex.Message & vbCrLf & ex.StackTrace)
            'Catch ex2 As Exception
            'End Try
        End Try
        Try
            If 发送者 IsNot Nothing Then
                Select Case 设备类型
                    Case 设备类型_常量集合.手机 : 发送者.手机连接步骤未完成 = False
                    Case 设备类型_常量集合.电脑 : 发送者.电脑连接步骤未完成 = False
                End Select
            End If
            If 网络连接器.Connected = True Then
                网络连接器.Shutdown(SocketShutdown.Both)
                网络连接器.Disconnect(False)
            End If
            网络连接器.Close()
            Thread.CurrentThread.Abort()
        Catch ex As ThreadAbortException
        Catch ex As Exception
        End Try
    End Sub

    Private Function 数据库_获取不推送的讯宝(ByVal 接收者编号 As Long, ByVal 设备类型 As 设备类型_常量集合,
                                  ByRef 不推送的新讯宝() As 类_要推送的讯宝, ByRef 新讯宝数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 接收者编号)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.不等于, 讯宝接收结果_常量集合.手机端接收成功)
                Case 设备类型_常量集合.电脑
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.不等于, 讯宝接收结果_常量集合.电脑端接收成功)
            End Select
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝不推送", 筛选器,  , , 100, "#接收者时间")
            读取器 = 指令.执行()
            While 读取器.读取
                If 新讯宝数量 = 不推送的新讯宝.Length Then ReDim Preserve 不推送的新讯宝(新讯宝数量 * 2 - 1)
                不推送的新讯宝(新讯宝数量) = New 类_要推送的讯宝
                With 不推送的新讯宝(新讯宝数量)
                    .发送时间 = 读取器(0)    '按照表列的顺序
                    .发送者英语讯宝地址 = 读取器(1)
                    .发送序号 = 读取器(2)
                    .群主讯宝地址 = 读取器(3)
                    .群编号 = 读取器(4)
                    .讯宝指令 = 读取器(6)
                    .文本库号 = 读取器(7)
                    .文本编号 = 读取器(8)
                    .宽度 = 读取器(9)
                    .高度 = 读取器(10)
                    .秒数 = 读取器(11)
                End With
                新讯宝数量 += 1
            End While
            读取器.关闭()
            If 新讯宝数量 > 0 Then
                Dim I As Integer
                For I = 0 To 新讯宝数量 - 1
                    With 不推送的新讯宝(I)
                        If .文本库号 > 0 Then
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于获取数据("文本")
                            指令 = New 类_数据库指令_请求获取数据(副数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                            读取器 = 指令.执行()
                            While 读取器.读取
                                .文本 = 读取器(0)
                                Exit While
                            End While
                            读取器.关闭()
                        End If
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取讯友录更新时间(ByVal 用户编号 As Long, ByRef 讯友录更新时间 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("更新时间")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录版本", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                讯友录更新时间 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取讯友录(ByVal 用户编号 As Long, ByRef 讯友() As 讯友_复合数据, ByRef 讯友数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语讯宝地址", "本国语讯宝地址", "备注", "标签一", "标签二", "主机名", "位置号", "拉黑"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器,  , 列添加器, 100, "#用户英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                If 讯友.Length = 讯友数量 Then ReDim Preserve 讯友(讯友数量 * 2 - 1)
                With 讯友(讯友数量)
                    .英语讯宝地址 = 读取器(0)
                    .本国语讯宝地址 = 读取器(1)
                    .备注 = 读取器(2)
                    .标签一 = 读取器(3)
                    .标签二 = 读取器(4)
                    .主机名 = 读取器(5)
                    .位置号 = 读取器(6)
                    .拉黑 = 读取器(7)
                End With
                讯友数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入的小聊天群(ByVal 用户编号 As Long, ByRef 加入的群() As 加入的小聊天群_复合数据, ByRef 加入的群数 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"群主地址", "群编号", "群备注"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的小聊天群", 筛选器,  , 列添加器, 最大值_常量集合.每个用户可加入的小聊天群数量, "#用户加入时间")
            读取器 = 指令.执行()
            While 读取器.读取
                If 加入的群数 = 加入的群.Length Then ReDim Preserve 加入的群(加入的群数 * 2 - 1)
                With 加入的群(加入的群数)
                    .群主英语讯宝地址 = 读取器(0)
                    .群编号 = 读取器(1)
                    .群备注 = 读取器(2)
                End With
                加入的群数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入的大聊天群(ByVal 用户编号 As Long, ByRef 加入的群() As 加入的大聊天群_复合数据, ByRef 加入的群数 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"主机名", "英语域名", "本国语域名", "群编号", "群名称"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的大聊天群", 筛选器,  , 列添加器, 最大值_常量集合.每个用户可加入的大聊天群数量, "#用户加入时间")
            读取器 = 指令.执行()
            While 读取器.读取
                If 加入的群数 = 加入的群.Length Then ReDim Preserve 加入的群(加入的群数 * 2 - 1)
                With 加入的群(加入的群数)
                    .主机名 = 读取器(0)
                    .英语域名 = 读取器(1)
                    .本国语域名 = 读取器(2)
                    .群编号 = 读取器(3)
                    .群名称 = 读取器(4)
                End With
                加入的群数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取白域(ByVal 用户编号 As Long, ByRef 白域() As 域名_复合数据, ByRef 白域数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语域名", "本国语域名"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "白域", 筛选器,  , 列添加器, 100, "#用户编号英语域名")
            读取器 = 指令.执行()
            While 读取器.读取
                If 白域数量 = 白域.Length Then ReDim Preserve 白域(白域数量 * 2 - 1)
                With 白域(白域数量)
                    .英语 = 读取器(0)
                    .本国语 = 读取器(1)
                End With
                白域数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取黑域(ByVal 用户编号 As Long, ByRef 黑域() As 域名_复合数据, ByRef 黑域数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语域名", "本国语域名"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "黑域", 筛选器,  , 列添加器, 100, "#用户编号英语域名")
            读取器 = 指令.执行()
            While 读取器.读取
                If 黑域数量 = 黑域.Length Then ReDim Preserve 黑域(黑域数量 * 2 - 1)
                With 黑域(黑域数量)
                    .英语 = 读取器(0)
                    .本国语 = 读取器(1)
                End With
                黑域数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_移除加入的群(ByVal 用户编号 As Long, Optional ByVal 群主地址 As String = Nothing, Optional ByVal 群编号 As Byte = 0)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 索引名称 As String
            If String.IsNullOrEmpty(群主地址) = False Then
                列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主地址)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                索引名称 = "#群主编号用户"
            Else
                索引名称 = "#用户加入时间"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "加入的小聊天群", 筛选器, 索引名称)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_清除不推送的讯宝(ByVal 接收者编号 As Long, ByVal 设备类型 As 设备类型_常量集合, ByVal 时间截点 As Long)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 接收者编号)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于等于, 时间截点)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.电脑端接收成功)
                Case 设备类型_常量集合.电脑
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.手机端接收成功)
            End Select
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "讯宝不推送", 筛选器, "#接收者时间")
            指令.执行()
            Dim 列添加器_新数据 As New 类_列添加器
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    列添加器_新数据.添加列_用于插入数据("结果", 讯宝接收结果_常量集合.手机端接收成功)
                Case 设备类型_常量集合.电脑
                    列添加器_新数据.添加列_用于插入数据("结果", 讯宝接收结果_常量集合.电脑端接收成功)
            End Select
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 接收者编号)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于等于, 时间截点)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.待确认)
                Case 设备类型_常量集合.电脑
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.待确认)
            End Select
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_更新 As New 类_数据库指令_更新数据(副数据库, "讯宝不推送", 列添加器_新数据, 筛选器, "#接收者时间")
            指令_更新.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
