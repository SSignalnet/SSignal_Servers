Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Web
Imports System.Text
Imports System.Timers
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Public Class 类_SS管理器
    Implements IDisposable

#Region "定义和声明"

    Private Structure 群成员_复合数据
        Dim 英语SS地址, 本国语SS地址 As String
        Dim 角色 As 群角色_常量集合
    End Structure

    Private Structure 要清除的SS_复合数据
        Dim 发送者编号 As Long
        Dim 类型 As SS类型_常量集合
        Dim 文本 As String
    End Structure

    Const 读取的SS最大数量 As Integer = 10
    Const 推送器最大数量 As Short = 10

    Dim 凭据_中心服务器, 网络地址_中心服务器 As String
    Public 本服务器主机名 As String
    Friend 数据存放路径 As String
    Dim 启动时间 As Long

    Friend 跨进程锁 As Mutex
    Friend 主数据库, 副数据库 As 类_数据库
    Dim 线程_侦听, 线程_分配SS推送任务, 线程_清除带文件SS As Thread

    Public 关闭 As Boolean
    Dim 网络连接器_侦听 As Socket

    Friend 群目录() As 类_群
    Friend 用户目录() As 类_用户
    Friend 用户数量 As Integer

    Dim 要推送的SS() As 类_要推送的SS
    Dim 要推送的SS数量 As Integer

    Friend SS推送器() As 类_SS推送器

    Public 连接凭据_管理员 As String

    WithEvents 定时器 As Timers.Timer

#End Region

    Public Sub New(ByVal Context As HttpContext, ByVal 跨进程锁1 As Mutex, ByVal 主数据库1 As 类_数据库, ByVal 副数据库1 As 类_数据库)
        数据存放路径 = Context.Server.MapPath("/") & "App_Data\"
        跨进程锁 = 跨进程锁1
        主数据库 = 主数据库1
        副数据库 = 副数据库1
    End Sub

    Public Function 验证中心服务器(ByVal 网络地址 As String, ByVal 服务器凭据 As String) As Boolean
        If String.Compare(网络地址, 网络地址_中心服务器) <> 0 Then Return False
        If String.Compare(服务器凭据, 凭据_中心服务器) <> 0 Then Return False
        Return True
    End Function

    Public Sub 启动()
        启动时间 = Date.UtcNow.Ticks
        凭据_中心服务器 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器)
        Dim 线程 As New Thread(New ThreadStart(AddressOf 启动2))
        线程.Start()
    End Sub

    Private Sub 启动2()
        Dim 中心服务器域名 As String = 获取服务器域名(SS中心服务器主机名 & "." & 域名_英语)
        Dim 访问结果 As Object = 访问其它服务器("https://" & 中心服务器域名 & "/IO.aspx?C=ServerStart&Credential=" & 替换URI敏感字符(凭据_中心服务器) & "&Type=" & 服务器类别_常量集合.大聊天群服务器, , 30000)
        If TypeOf 访问结果 Is 类_SS包生成器 Then Return
        Try
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then Return
            SS包解读器.读取_带标签("主机名", 本服务器主机名, Nothing)
            If String.IsNullOrEmpty(本服务器主机名) Then Return
            If 本服务器主机名.Length > 最大值_常量集合.主机名字符数 Then Return
            Dim SS包解读器2() As Object = SS包解读器.读取_带重复标签("大聊天群")
            If SS包解读器2 IsNot Nothing Then
                Dim 群目录2(SS包解读器2.Length - 1) As 类_群
                Dim I As Integer
                For I = 0 To 群目录2.Length - 1
                    群目录2(I) = New 类_群()
                Next
                Dim 某一群 As 类_群
                Dim 编号 As Short
                Dim SS包解读器3 As 类_SS包解读器 = Nothing
                For I = 0 To SS包解读器2.Length - 1
                    某一群 = New 类_群
                    With CType(SS包解读器2(I), 类_SS包解读器)
                        .读取_带标签("编号", 编号, 0)
                        If 编号 <= 0 Then Return
                        .读取_带标签("名称", 某一群.名称)
                        .读取_带标签("英语", 某一群.群主英语SS地址)
                        .读取_带标签("本国语", 某一群.群主本国语SS地址)
                        .读取_带标签("查封", 某一群.查封)
                    End With
                    群目录2(编号 - 1) = 某一群
                Next
                Dim 用户目录2(最大值_常量集合.大聊天群服务器承载用户数 - 1) As 类_用户
                Dim 用户数量2 As Integer
                Dim 结果 As 类_SS包生成器 = 数据库_获取用户目录(用户目录2, 用户数量2)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return
                End If
                For I = 0 To 群目录2.Length - 1
                    结果 = 数据库_获取群成员(群目录2(I), I + 1, 用户目录2, 用户数量2)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return
                    End If
                Next
                群目录 = 群目录2
                用户目录 = 用户目录2
                用户数量 = 用户数量2
            Else
                群目录 = Nothing
            End If
            If 线程_侦听 Is Nothing Then
                线程_侦听 = New Thread(New ThreadStart(AddressOf 侦听))
                线程_侦听.Start()
            End If
            If 线程_分配SS推送任务 Is Nothing Then
                ReDim SS推送器(推送器最大数量 - 1)
                线程_分配SS推送任务 = New Thread(New ThreadStart(AddressOf 分配SS推送任务))
                线程_分配SS推送任务.Start()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Function 验证启动(ByVal 网络地址 As String, ByVal 服务器凭据 As String) As Boolean
        If 启动时间 <= 0 Then Return False
        If String.Compare(服务器凭据, 凭据_中心服务器) <> 0 Then Return False
        If DateDiff(DateInterval.Second, Date.FromBinary(启动时间), Date.UtcNow) > 30 Then Return False
        网络地址_中心服务器 = 网络地址
        启动时间 = 0
        Return True
    End Function

    Private Function 数据库_获取用户目录(ByRef 用户目录2() As 类_用户, ByRef 用户数量2 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", Nothing,  , , 100, 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                用户目录2(用户数量2) = New 类_用户
                With 用户目录2(用户数量2)
                    .英语SS地址 = 读取器(0)
                    .本国语SS地址 = 读取器(1)
                    .主机名 = 读取器(2)
                    .位置号 = 读取器(3)
                End With
                用户数量2 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取群成员(ByVal 某一群 As 类_群, ByVal 群编号 As Short, ByVal 用户目录2() As 类_用户, ByVal 用户数量2 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语SS地址", "角色"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器,  , 列添加器, 100, "#群编号英语SS地址")
            Dim 成员(499), 某一成员 As 类_群成员
            Dim 成员数, I As Integer
            Dim 英语SS地址 As String
            读取器 = 指令.执行()
            While 读取器.读取
                If 成员数 = 成员.Length Then ReDim Preserve 成员(成员数 * 2 - 1)
                英语SS地址 = 读取器(0)
                For I = 0 To 用户数量2 - 1
                    If String.Compare(英语SS地址, 用户目录2(I).英语SS地址) = 0 Then Exit For
                Next
                If I < 用户数量2 Then
                    某一成员 = New 类_群成员
                    某一成员.角色 = 读取器(1)
                    某一成员.用户 = 用户目录2(I)
                    成员(成员数) = 某一成员
                    成员数 += 1
                End If
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 获取群成员列表() As 类_SS包生成器
        'If SS类型 = SS类型_常量集合.获取群成员列表 Then
        '    If 角色 = 群角色_常量集合.邀请加入 Then
        '        结果 = 数据库_成为群成员(群编号, 发送者.英语SS地址)
        '        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
        '        群成员.角色 = 群角色_常量集合.普通成员
        '        Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
        '        Select Case 设备类型
        '            Case 设备类型_常量集合.手机 : If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
        '            Case 设备类型_常量集合.电脑 : If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
        '            Case Else : Throw New Exception
        '        End Select
        '        If 同步设备类型 <> 设备类型_常量集合.全部 Then
        '            SS包生成器 = New 类_SS包生成器(True)
        '            SS包生成器.添加_带标签("事件", 同步事件_常量集合.加入聊天群)
        '            SS包生成器.添加_带标签("主机名", 本服务器主机名)
        '            SS包生成器.添加_带标签("域名", 域名_英语)
        '            SS包生成器.添加_带标签("群编号", 群编号)
        '            SS包生成器.添加_带标签("群名称", 群.名称)
        '            结果 = 数据库_存为推送的SS(发送者.英语SS地址, 0, 0, SS类型_常量集合.手机和电脑同步, SS包生成器.生成纯文本, , , , 同步设备类型, 位置号_用户目录)
        '            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
        '        End If
        '    End If
        '    Dim SS包生成器2 As New 类_SS包生成器(True)
        '    For I = 0 To 群.成员数 - 1
        '        With 群.成员(I)
        '            Dim SS包生成器3 As New 类_SS包生成器(True, , SS包编码_常量集合.UTF8)
        '            With .用户
        '                SS包生成器3.添加_带标签("E", .英语SS地址)
        '                If String.IsNullOrEmpty(.本国语SS地址) = False Then
        '                    SS包生成器3.添加_带标签("N", .本国语SS地址)
        '                End If
        '            End With
        '            SS包生成器3.添加_带标签("R", .角色)
        '            SS包生成器2.添加_带标签("M", SS包生成器3)
        '        End With
        '    Next
        '    结果 = 数据库_存为推送的SS(接收者英语SS地址, 0, 本次发送序号, 接收者英语SS地址, 群编号, 发送者.用户编号, 位置号, 设备类型, SS类型_常量集合.获取群成员列表, SS包生成器2.生成纯文本)
        '    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
        '    If 角色 = 群角色_常量集合.邀请加入 Then
        '        SS类型 = SS类型_常量集合.某人加入小聊天群
        '        Dim SS包生成器3 As New 类_SS包生成器(True)
        '        SS包生成器3.添加_带标签("ENGLISH", 发送者英语SS地址)
        '        If String.IsNullOrEmpty(发送者.本国语用户名) = False Then
        '            SS包生成器3.添加_带标签("NATIVE", 发送者.本国语用户名 & SS地址标识 & 域名_本国语)
        '        End If
        '        SS包生成器3.添加_带标签("HOST", 本服务器主机名)
        '        SS包生成器3.添加_带标签("POSITION", 位置号)
        '        SS文本 = SS包生成器3.生成纯文本
        '        发送给发送者 = True
        '    Else
        '        Return
        '    End If
        'End If
    End Function


    Private Sub 侦听()
        Try
            Dim 地址和端口 As New IPEndPoint(IPAddress.Any, 获取大聊天群服务器侦听端口)
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
            Dim 位置号_用户目录 As Short
            SS包解读器.读取(位置号_用户目录)
            If 位置号_用户目录 < 0 OrElse 位置号_用户目录 >= 用户目录.Length Then Throw New Exception
            SS包解读器.读取字节(设备类型)
            发送者 = 用户目录(位置号_用户目录)
            Dim 字节数组() As Byte = Nothing
            Select Case 设备类型
                Case 设备类型_常量集合.手机 : SS包解读器.读取(字节数组, 长度信息字节数_常量集合.两字节, 发送者.AES解密器_手机)
                Case 设备类型_常量集合.电脑 : SS包解读器.读取(字节数组, 长度信息字节数_常量集合.两字节, 发送者.AES解密器_电脑)
                Case Else : Throw New Exception
            End Select
            SS包解读器 = New 类_SS包解读器(字节数组)
            Dim 英语SS地址 As String = Nothing
            SS包解读器.读取_带标签("英语SS地址", 英语SS地址)
            If String.Compare(英语SS地址, 发送者.英语SS地址) <> 0 Then Throw New Exception
            Dim 本国语SS地址 As String = Nothing
            SS包解读器.读取_带标签("本国语SS地址", 本国语SS地址)
            If String.Compare(本国语SS地址, 发送者.本国语SS地址) <> 0 Then Throw New Exception
            Dim 验证码 As String = Nothing
            SS包解读器.读取_带标签("验证码", 验证码)
            If String.IsNullOrEmpty(验证码) Then Throw New Exception
            If 验证码.Length <> 长度_常量集合.验证码 Then Throw New Exception
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
            Dim 不推送的SS(99) As 类_要推送的SS
            Dim 不推送的SS数量, 不推送的SS数量2 As Integer
            Dim 结果 As 类_SS包生成器 = Nothing
            Dim I As Integer
            If 跨进程锁.WaitOne = True Then
                Try
                    '结果 = 数据库_获取不推送的SS(用户编号, 设备类型, 不推送的SS, 不推送的SS数量)
                    'If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Throw New Exception
            End If
            Dim SS包生成器 As New 类_SS包生成器(True)
            SS包生成器.添加_带标签("验证码", 验证码)
            If 不推送的SS数量 > 0 Then
                Dim SS包生成器2 As New 类_SS包生成器(True)
                For I = 0 To 不推送的SS数量 - 1
                    Dim SS包生成器3 As New 类_SS包生成器(True)
                    With 不推送的SS(I)
                        If .类型 >= SS类型_常量集合.手机和电脑同步 Then
                            不推送的SS数量2 += 1
                            Continue For
                        End If
                        SS包生成器3.添加_带标签("发送者", .发送者英语地址)
                        SS包生成器3.添加_带标签("发送时间", .发送时间)
                        SS包生成器3.添加_带标签("类型", .类型)
                        SS包生成器3.添加_带标签("发送序号", .发送序号)
                        If .群编号 > 0 Then
                            SS包生成器3.添加_带标签("群编号", .群编号)
                            SS包生成器3.添加_带标签("群主", .群主SS地址)
                        End If
                        SS包生成器3.添加_带标签("文本", .文本)
                        Select Case .类型
                            Case SS类型_常量集合.图片, SS类型_常量集合.短视频
                                SS包生成器3.添加_带标签("宽度", .宽度)
                                SS包生成器3.添加_带标签("高度", .高度)
                        End Select
                        Select Case .类型
                            Case SS类型_常量集合.语音, SS类型_常量集合.短视频
                                SS包生成器3.添加_带标签("秒数", .秒数)
                        End Select
                    End With
                    SS包生成器2.添加_带标签("SS", SS包生成器3)
                Next
                If SS包生成器2.数据量 > 0 Then
                    SS包生成器.添加_带标签("新SS", SS包生成器2)
                End If
            End If
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 发送者.SS序号_手机发送 = 0 Then 发送者.SS序号_手机发送 = Date.UtcNow.Ticks
                    If 发送者.SS群消息序号 = 0 Then 发送者.SS群消息序号 = -发送者.SS序号_手机发送
                    SS包生成器.添加_带标签("发送序号", 发送者.SS序号_手机发送)
                    SS包生成器.添加_带标签("在线", CBool(IIf(发送者.网络连接器_电脑 IsNot Nothing, True, False)))
                    If SS包生成器.发送SS包(网络连接器, 发送者.AES加密器_手机) = False Then Throw New Exception
                Case 设备类型_常量集合.电脑
                    If 发送者.SS序号_电脑发送 = 0 Then 发送者.SS序号_电脑发送 = Date.UtcNow.Ticks
                    If 发送者.SS群消息序号 = 0 Then 发送者.SS群消息序号 = -发送者.SS序号_电脑发送
                    SS包生成器.添加_带标签("发送序号", 发送者.SS序号_电脑发送)
                    SS包生成器.添加_带标签("在线", CBool(IIf(发送者.网络连接器_手机 IsNot Nothing, True, False)))
                    If SS包生成器.发送SS包(网络连接器, 发送者.AES加密器_电脑) = False Then Throw New Exception
            End Select
            Select Case 设备类型
                Case 设备类型_常量集合.手机 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_手机)
                Case 设备类型_常量集合.电脑 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_电脑)
            End Select
            Dim 数量 As Integer
            SS包解读器.读取_带标签("SS数量", 数量, 0)
            If 数量 <> 不推送的SS数量 - 不推送的SS数量2 Then Throw New Exception
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
                            SS包生成器 = New 类_SS包生成器(True)
                            SS包生成器.添加_带标签("事件", 同步事件_常量集合.手机上线)
                            If 跨进程锁.WaitOne = True Then
                                Try
                                    Call 数据库_存为推送的SS(发送者.英语SS地址, 0, 0, SS类型_常量集合.手机和电脑同步, SS包生成器.生成纯文本, , , , 设备类型_常量集合.电脑, 位置号_用户目录)
                                Catch ex As Exception
                                    Throw ex
                                Finally
                                    跨进程锁.ReleaseMutex()
                                End Try
                            End If
                        End If
                    End If
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
                            SS包生成器 = New 类_SS包生成器(True)
                            SS包生成器.添加_带标签("事件", 同步事件_常量集合.电脑上线)
                            If 跨进程锁.WaitOne = True Then
                                Try
                                    Call 数据库_存为推送的SS(发送者.英语SS地址, 0, 0, SS类型_常量集合.手机和电脑同步, SS包生成器.生成纯文本, , , , 设备类型_常量集合.手机, 位置号_用户目录)
                                Catch ex As Exception
                                    Throw ex
                                Finally
                                    跨进程锁.ReleaseMutex()
                                End Try
                            End If
                        End If
                    End If
                    发送者.电脑连接步骤未完成 = False
            End Select
            网络连接器.ReceiveTimeout = 0
            Do
                Select Case 设备类型
                    Case 设备类型_常量集合.手机 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_手机)
                    Case 设备类型_常量集合.电脑 : SS包解读器 = New 类_SS包解读器(网络连接器, 发送者.AES解密器_电脑)
                    Case Else : Throw New Exception
                End Select
                发送SS(SS包解读器, 发送者, 位置号_用户目录, 设备类型)
            Loop Until 关闭
        Catch ex As Exception
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


    Private Sub 发送SS(ByVal SS包解读器 As 类_SS包解读器, ByVal 发送者 As 类_用户, ByVal 位置号_用户目录 As Short, ByVal 设备类型 As 设备类型_常量集合)
        Dim 结果 As 类_SS包生成器 = Nothing
        Dim 本次发送序号 As Long
        Dim SS类型 As SS类型_常量集合
        Dim SS文本 As String = Nothing
        Dim SS文件数据() As Byte = Nothing
        Dim 宽度, 高度, 群编号, 位置号_群成员目录 As Short
        Dim 秒数 As Byte
        SS包解读器.读取_带标签("序号", 本次发送序号, 0)
        Select Case 设备类型
            Case 设备类型_常量集合.手机
                If 发送者.SS序号_手机发送 < Long.MaxValue Then
                    If 本次发送序号 <> 发送者.SS序号_手机发送 + 1 Then
                        If 本次发送序号 = 发送者.SS序号_手机发送 Then
                            Return
                        Else
                            Throw New Exception
                        End If
                    End If
                Else
                    If 本次发送序号 <> 1 Then
                        If 本次发送序号 = 发送者.SS序号_手机发送 Then
                            Return
                        Else
                            Throw New Exception
                        End If
                    End If
                End If
                发送者.SS序号_手机发送 = 本次发送序号
            Case 设备类型_常量集合.电脑
                If 发送者.SS序号_电脑发送 < Long.MaxValue Then
                    If 本次发送序号 <> 发送者.SS序号_电脑发送 + 1 Then
                        If 本次发送序号 = 发送者.SS序号_电脑发送 Then
                            Return
                        Else
                            Throw New Exception
                        End If
                    End If
                Else
                    If 本次发送序号 <> 1 Then
                        If 本次发送序号 = 发送者.SS序号_电脑发送 Then
                            Return
                        Else
                            Throw New Exception
                        End If
                    End If
                End If
                发送者.SS序号_电脑发送 = 本次发送序号
        End Select
        SS包解读器.读取_带标签("类型", SS类型, SS类型_常量集合.无)
        If SS类型 = SS类型_常量集合.无 Then Return
        SS包解读器.读取_带标签("群编号", 群编号, 0)
        If 群编号 <= 0 OrElse 群编号 > 群目录.Length Then Return
        SS包解读器.读取_带标签("位置号", 位置号_群成员目录, 0)
        If 位置号_群成员目录 < 0 Then Return
        Select Case SS类型
            Case SS类型_常量集合.文字
                SS包解读器.读取_带标签("文本", SS文本, Nothing)
                If String.IsNullOrEmpty(SS文本) Then Throw New Exception
            Case SS类型_常量集合.语音
                SS包解读器.读取_带标签("文本", SS文本, Nothing)
                If String.IsNullOrEmpty(SS文本) Then Throw New Exception
                SS文本 = 生成大写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器) & 特征字符_下划线 & 本次发送序号 & "." & SS文本
                SS包解读器.读取_带标签("秒数", 秒数, 0)
                SS包解读器.读取_带标签("文件", SS文件数据, Nothing)
                If SS文件数据 Is Nothing Then Throw New Exception
            Case SS类型_常量集合.图片, SS类型_常量集合.短视频
                SS包解读器.读取_带标签("文本", SS文本, Nothing)
                If String.IsNullOrEmpty(SS文本) Then Throw New Exception
                SS文本 = 生成大写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器) & 特征字符_下划线 & 本次发送序号 & "." & SS文本
                SS包解读器.读取_带标签("宽度", 宽度, 0)
                If 宽度 < 1 OrElse 宽度 > 最大值_常量集合.SS预览图片宽高_像素 Then Throw New Exception
                SS包解读器.读取_带标签("高度", 高度, 0)
                If 高度 < 1 OrElse 高度 > 最大值_常量集合.SS预览图片宽高_像素 Then Throw New Exception
                SS包解读器.读取_带标签("文件", SS文件数据, Nothing)
                If SS文件数据 Is Nothing Then Throw New Exception
                If SS类型 = SS类型_常量集合.短视频 Then SS包解读器.读取_带标签("秒数", 秒数, 0)
            Case SS类型_常量集合.文件
                SS包解读器.读取_带标签("文本", SS文本, Nothing)
                If String.IsNullOrEmpty(SS文本) Then Throw New Exception
                SS文本 = 生成大写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器) & 特征字符_下划线 & 本次发送序号 & 特征字符_下划线 & SS文本
                SS包解读器.读取_带标签("文件", SS文件数据, Nothing)
                If SS文件数据 Is Nothing Then Throw New Exception
            Case SS类型_常量集合.撤回
                SS包解读器.读取_带标签("文本", SS文本, Nothing)
                Dim 发送序号_撤回的SS As Long
                If Long.TryParse(SS文本, 发送序号_撤回的SS) = False Then Throw New Exception
                If 发送序号_撤回的SS = 本次发送序号 Then Throw New Exception
            Case Else : Throw New Exception
        End Select
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_统计发送次数(发送者.英语SS地址, 发送者.本国语SS地址)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Select Case 结果.查询结果
                        Case 查询结果_常量集合.本小时发送的SS数量已达上限
                            结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型_常量集合.本小时发送的SS数量已达上限, , , , , 设备类型, 位置号_用户目录)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            Return
                        Case 查询结果_常量集合.本日发送的SS数量已达上限
                            结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型_常量集合.本日发送的SS数量已达上限, , , , , 设备类型, 位置号_用户目录)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            Return
                        Case 查询结果_常量集合.失败 : Return
                        Case Else : Throw New Exception
                    End Select
                End If
                Dim 群 As 类_群 = 群目录(群编号 - 1)
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                Dim 群成员 As 类_群成员 = Nothing
                If 位置号_群成员目录 < 群.成员数 Then
                    群成员 = 群.成员(位置号_群成员目录)
                    If 群成员.用户.Equals(发送者) Then
                        角色 = 群成员.角色
                    End If
                End If
                If 角色 = 群角色_常量集合.无 Then
                    结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型_常量集合.不是群成员, , , , , 设备类型, 位置号_用户目录)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    Return
                End If
                Dim 群正式成员数 As Short
                For I = 0 To 群.成员数 - 1
                    If 群.成员(I).角色 <> 群角色_常量集合.邀请加入 Then
                        群正式成员数 += 1
                    End If
                Next
                If 群正式成员数 > 最大值_常量集合.小聊天群成员数量 Then
                    结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型, SS文本, 宽度, 高度, 秒数, , , 位置号_群成员目录)
                Else
                    结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型_常量集合.群里没有成员, , , , , 设备类型, 位置号_用户目录)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    Return
                End If
            Catch ex As Exception
                Throw ex
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Throw New Exception
        End If
        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
        If SS文件数据 IsNot Nothing Then
            If 结果.查询结果 = 查询结果_常量集合.成功 Then
                Dim 路径 As String
                路径 = 数据存放路径 & "SS\" & 群编号
                If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                路径 &= "\" & SS文本
                File.WriteAllBytes(路径, SS文件数据)
                If SS类型 = SS类型_常量集合.图片 Then 生成预览图片(路径)
            End If
        End If
        If SS类型 < SS类型_常量集合.视频通话 Then
            Dim 另一设备的类型 As 设备类型_常量集合
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 发送者.网络连接器_电脑 Is Nothing Then Return
                    另一设备的类型 = 设备类型_常量集合.电脑
                Case 设备类型_常量集合.电脑
                    If 发送者.网络连接器_手机 Is Nothing Then Return
                    另一设备的类型 = 设备类型_常量集合.手机
                Case Else : Return
            End Select
            If 跨进程锁.WaitOne = True Then
                Try
                    结果 = 数据库_存为推送的SS(发送者.英语SS地址, 本次发送序号, 群编号, SS类型, SS文本, 宽度, 高度, 秒数, 另一设备的类型, 位置号_用户目录)
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
            End If
        End If
    End Sub

    Private Function 数据库_统计发送次数(ByVal 英语SS地址 As String, ByVal 本国语SS地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 收发统计 As 个人SS统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "个人SS统计", 筛选器, 1, , 100, 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                收发统计.今日几号 = 读取器(2)
                收发统计.今日发送 = 读取器(3)
                收发统计.昨日发送 = 读取器(4)
                收发统计.前日发送 = 读取器(5)
                收发统计.今日几时 = 读取器(6)
                收发统计.时段发送 = 读取器(7)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CBool(当前时间.Hour)
        If 收发统计.今日几号 > 0 Then
            If 今日几号 = 收发统计.今日几号 Then
                If 收发统计.今日发送 < 最大值_常量集合.每天可发送SS数量 Then
                    If 收发统计.时段发送 >= 最大值_常量集合.每小时可发送SS数量 Then
                        If 收发统计.时段发送 < 最大值_常量集合.每小时可发送SS数量 + 3 Then
                            Dim 结果 As 类_SS包生成器 = 数据库_更新今日个人收发统计(英语SS地址, 收发统计.今日发送, 今日几时, 收发统计.时段发送 + 1)
                            Select Case 结果.查询结果
                                Case 查询结果_常量集合.成功, 查询结果_常量集合.失败
                                    Return New 类_SS包生成器(查询结果_常量集合.本小时发送的SS数量已达上限)
                                Case Else
                                    Return 结果
                            End Select
                        Else
                            Return New 类_SS包生成器(查询结果_常量集合.本小时发送的SS数量已达上限)
                        End If
                    End If
                    If 收发统计.今日几时 = 今日几时 Then
                        Return 数据库_更新今日个人收发统计(英语SS地址, 收发统计.今日发送 + 1, 今日几时, 收发统计.时段发送 + 1)
                    Else
                        Return 数据库_更新今日个人收发统计(英语SS地址, 收发统计.今日发送 + 1, 今日几时, 1)
                    End If
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.本日发送的SS数量已达上限)
                End If
            Else
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                If 昨日几号 = 收发统计.今日几号 Then
                    Return 数据库_更新个人收发统计(英语SS地址, 今日几号, 1, 收发统计.今日发送, 收发统计.昨日发送, 今日几时, 1)
                Else
                    Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                    Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                    If 前日几号 = 收发统计.今日几号 Then
                        Return 数据库_更新个人收发统计(英语SS地址, 今日几号, 1, 0, 收发统计.今日发送, 今日几时, 1)
                    Else
                        Return 数据库_更新个人收发统计(英语SS地址, 今日几号, 1, 0, 0, 今日几时, 1)
                    End If
                End If
            End If
        Else
            Return 数据库_添加个人收发统计(英语SS地址, 本国语SS地址, 今日几号, 1, 今日几时, 1)
        End If
    End Function

    Private Sub 生成预览图片(ByVal 原图路径 As String)
        Dim 原图 As Bitmap = Nothing
        Dim 预览图片 As Bitmap = Nothing
        Try
            原图 = New Bitmap(原图路径)
            If 原图.Width > 最大值_常量集合.SS预览图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.SS预览图片宽高_像素 Then
                Dim 缩小比例 As Double
                If 原图.Height > 原图.Width Then
                    缩小比例 = 最大值_常量集合.SS预览图片宽高_像素 / 原图.Height
                Else
                    缩小比例 = 最大值_常量集合.SS预览图片宽高_像素 / 原图.Width
                End If
                预览图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
            Else
                预览图片 = New Bitmap(原图.Width, 原图.Height)
            End If
            Dim 绘图器 As Graphics = Graphics.FromImage(预览图片)
            绘图器.FillRectangle(New SolidBrush(Color.White), 0, 0, 预览图片.Width, 预览图片.Height)
            绘图器.DrawImage(原图, 0, 0, 预览图片.Width, 预览图片.Height)
            原图.Dispose()
            绘图器.Dispose()
            预览图片.Save(原图路径 & ".jpg", Imaging.ImageFormat.Jpeg)
            预览图片.Dispose()
        Catch ex As Exception
            If 原图 IsNot Nothing Then 原图.Dispose()
            If 预览图片 IsNot Nothing Then 预览图片.Dispose()
            Throw ex
        End Try
    End Sub

    Private Function 数据库_存为推送的SS(ByVal 发送者英语地址 As String, ByVal 发送序号 As Long, ByVal 群编号 As Byte,
                                 ByVal SS类型 As SS类型_常量集合, Optional ByVal 文本 As String = Nothing,
                                 Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0, Optional ByVal 秒数 As Byte = 0,
                                 Optional ByVal 设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部,
                                 Optional ByVal 位置号_用户目录 As Short = -1, Optional ByVal 位置号_成员目录 As Short = -1) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            列添加器.添加列_用于插入数据("类型", SS类型)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("设备类型", 设备类型)
            列添加器.添加列_用于插入数据("位置号_成员目录", 位置号_成员目录)
            列添加器.添加列_用于插入数据("位置号_用户目录", 位置号_用户目录)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "SS", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 分配SS推送任务()
        Dim I As Integer
        Do
            Try
                If 要推送的SS数量 > 0 Then
                    For I = 0 To 要推送的SS数量 - 1
                        If 要推送的SS(I).当前状态 = SS推送状态_常量集合.结束 Then
                            Exit For
                        End If
                    Next
                    If I < 要推送的SS数量 Then
                        If 跨进程锁.WaitOne = True Then
                            Try
                                Dim 要推送的SS2(要推送的SS数量 - 1) As 类_要推送的SS
                                Dim 要推送的SS数量2 As Integer = 0
                                Dim 结果 As 类_SS包生成器
                                For I = 0 To 要推送的SS数量 - 1
                                    With 要推送的SS(I)
                                        If .当前状态 = SS推送状态_常量集合.结束 Then

                                        Else
                                            要推送的SS2(要推送的SS数量2) = 要推送的SS(I)
                                            要推送的SS数量2 += 1
                                        End If
                                    End With
                                Next
                                要推送的SS = 要推送的SS2
                                要推送的SS数量 = 要推送的SS数量2
                            Catch ex As Exception
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        Else
                            Continue Do
                        End If
                    End If
                End If
                If 要推送的SS数量 < 读取的SS最大数量 Then
                    Dim 新SS(读取的SS最大数量 - 要推送的SS数量 - 1) As 类_要推送的SS
                    Dim 新SS数量 As Integer = 0
                    If 跨进程锁.WaitOne = True Then
                        Try
                            Dim 结果 As 类_SS包生成器 = 数据库_获取要推送的新SS(新SS, 新SS数量)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Continue Do
                            End If
                        Catch ex As Exception
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Continue Do
                    End If
                    If 新SS数量 > 0 Then
                        Dim 要推送的SS2(要推送的SS数量 + 新SS数量 - 1) As 类_要推送的SS
                        If 要推送的SS数量 > 0 Then
                            Array.Copy(要推送的SS, 0, 要推送的SS2, 0, 要推送的SS数量)
                        End If
                        Array.Copy(新SS, 0, 要推送的SS2, 要推送的SS数量, 新SS数量)
                        要推送的SS = 要推送的SS2
                        要推送的SS数量 += 新SS数量
                    End If
                End If
                If 要推送的SS数量 > 0 Then
                    Dim J As Integer
                    For I = 0 To 要推送的SS数量 - 1
                        If 要推送的SS(I).当前状态 = SS推送状态_常量集合.等待 Then
                            For J = 0 To SS推送器.Length - 1
                                If SS推送器(J) Is Nothing Then
                                    SS推送器(J) = New 类_SS推送器(要推送的SS(I), J, Me)
                                    SS推送器(J).推送_启动新线程()
                                    Exit For
                                End If
                            Next
                        End If
                    Next
                End If
                Thread.Sleep(1000)
            Catch ex As Exception
                Thread.Sleep(2000)
            End Try
        Loop Until 关闭
    End Sub

    Private Function 数据库_获取要推送的新SS(ByRef 要推送的新SS() As 类_要推送的SS, ByRef 新SS数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "SS", Nothing, 要推送的新SS.Length, , CByte(要推送的新SS.Length), "#群编号时间")
            Dim 时间, 接收者编号 As Long
            Dim 发送者英语地址 As String
            Dim 群接收者() As Byte
            Dim 要推送的SS As 类_要推送的SS
            读取器 = 指令.执行()
            While 读取器.读取
                时间 = 读取器(0)    '按照表列的顺序
                发送者英语地址 = 读取器(1)
                接收者编号 = 读取器(6)
                If 要推送的SS是否重复(时间, 发送者英语地址, 接收者编号) = True Then Continue While
                群接收者 = 读取器(8)
                要推送的新SS(新SS数量) = New 类_要推送的SS
                With 要推送的新SS(新SS数量)
                    .发送时间 = 时间
                    .发送者英语地址 = 发送者英语地址
                    .发送序号 = 读取器(3)
                    .群编号 = 读取器(5)
                    .类型 = 读取器(9)
                    .文本库号 = 读取器(10)
                    .文本编号 = 读取器(11)
                    .宽度 = 读取器(12)
                    .高度 = 读取器(13)
                    .秒数 = 读取器(14)
                    .设备类型 = 读取器(15)
                End With
                新SS数量 += 1
                If 新SS数量 = 要推送的新SS.Length Then Exit While
            End While
            读取器.关闭()
            If 新SS数量 > 0 Then
                Dim 列添加器 As 类_列添加器
                Dim 筛选器 As 类_筛选器
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                Dim I, J As Integer
                For I = 0 To 新SS数量 - 1
                    With 要推送的新SS(I)
                        If .文本库号 > 0 AndAlso String.IsNullOrEmpty(.文本) Then
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
                            If .文本一样 Then
                                If I < 新SS数量 - 1 Then
                                    文本库号 = .文本库号
                                    文本编号 = .文本编号
                                    For J = I + 1 To 新SS数量 - 1
                                        If 要推送的新SS(J).文本库号 = 文本库号 AndAlso 要推送的新SS(J).文本编号 = 文本编号 Then
                                            要推送的新SS(J).文本 = .文本
                                        Else
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
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

    Private Function 要推送的SS是否重复(ByVal 群编号 As Short, ByVal 时间 As Long, ByVal 发送者英语地址 As String) As Boolean
        Dim J As Integer
        For J = 要推送的SS数量 - 1 To 0 Step -1
            With 要推送的SS(J)
                If .群编号 = 群编号 Then
                    If .发送时间 = 时间 Then
                        If String.Compare(.发送者英语地址, 发送者英语地址) = 0 Then
                            Return True
                        End If
                    End If
                End If
            End With
        Next
        Return False
    End Function

    Private Function 数据库_删除推送成功或失败的SS(ByVal SS As 类_要推送的SS) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, SS.发送者英语地址)
            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, SS.发送序号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "SS推送", 筛选器, "#发送者发送序号")
            指令.执行()
            If SS.文本库号 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, SS.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令 = New 类_数据库指令_删除数据(副数据库, SS.文本库号 & "库", 筛选器, 主键索引名)
                指令.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function



    'Private Function 数据库_发送SS时获取群成员(ByVal 用户编号 As Long, ByVal 群编号 As Byte,
    '                                ByRef 群成员() As 群成员_复合数据, ByRef 群成员数 As Short) As 类_SS包生成器
    '    Dim 读取器 As 类_读取器_外部 = Nothing
    '    Try
    '        Dim 列添加器 As New 类_列添加器
    '        列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
    '        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
    '        Dim 筛选器 As New 类_筛选器
    '        筛选器.添加一组筛选条件(列添加器)
    '        列添加器 = New 类_列添加器
    '        列添加器.添加列_用于获取数据(New String() {"英语SS地址", "本国语SS地址", "主机名", "位置号", "角色"})
    '        Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器,  , 列添加器, 最大值_常量集合.小聊天群成员数量, "#群主编号角色加入时间")
    '        ReDim 群成员(最大值_常量集合.小聊天群成员数量 - 1)
    '        读取器 = 指令.执行()
    '        While 读取器.读取
    '            If 群成员数 = 群成员.Length Then ReDim Preserve 群成员(群成员数 * 2 - 1)
    '            With 群成员(群成员数)
    '                .英语SS地址 = 读取器(0)
    '                .本国语SS地址 = 读取器(1)
    '                .主机名 = 读取器(2)
    '                .位置号 = 读取器(3)
    '                .角色 = 读取器(4)
    '            End With
    '            群成员数 += 1
    '        End While
    '        读取器.关闭()
    '        Return New 类_SS包生成器(查询结果_常量集合.成功)
    '    Catch ex As Exception
    '        If 读取器 IsNot Nothing Then 读取器.关闭()
    '        Return New 类_SS包生成器(ex.Message)
    '    End Try
    'End Function

    '    Private Function 数据库_分配位置(ByVal 群主 As 类_群成员, ByVal 群主位置号 As Short, ByVal 备注 As String, ByRef 群编号 As Byte) As 类_SS包生成器
    '        Dim 读取器 As 类_读取器_外部 = Nothing
    '        Try
    '            Dim 列添加器 As New 类_列添加器
    '            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 群主.用户编号)
    '            Dim 筛选器 As New 类_筛选器
    '            筛选器.添加一组筛选条件(列添加器)
    '            列添加器 = New 类_列添加器
    '            列添加器.添加列_用于获取数据("群编号")
    '            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "创建的群", 筛选器,  , 列添加器, 最大值_常量集合.每个用户可创建的小聊天群数量, "#用户群编号")
    '            Dim 位置号(最大值_常量集合.每个用户可创建的小聊天群数量 - 1) As Byte
    '            Dim 位置号数量 As Short
    '            读取器 = 指令2.执行()
    '            While 读取器.读取
    '                If 位置号数量 = 位置号.Length Then ReDim Preserve 位置号(位置号数量 * 2 - 1)
    '                位置号(位置号数量) = 读取器(0)
    '                位置号数量 += 1
    '            End While
    '            读取器.关闭()
    '            If 位置号数量 < 最大值_常量集合.每个用户可创建的小聊天群数量 Then
    '                群编号 = 1
    '                If 位置号数量 > 0 Then
    '                    Dim I As Integer
    '跳转点1:
    '                    For I = 0 To 位置号数量 - 1
    '                        If 位置号(I) = 群编号 Then
    '                            群编号 += 1
    '                            GoTo 跳转点1
    '                        End If
    '                    Next
    '                End If
    '                Dim 当前时刻 As Long = Date.UtcNow.Ticks
    '                列添加器 = New 类_列添加器
    '                列添加器.添加列_用于插入数据("用户编号", 群主.用户编号)
    '                列添加器.添加列_用于插入数据("位置号", 群主位置号)
    '                列添加器.添加列_用于插入数据("群编号", 群编号)
    '                列添加器.添加列_用于插入数据("创建时间", 当前时刻)
    '                Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "创建的群", 列添加器, True)
    '                指令.执行()
    '                Dim 群主地址 As String = 群主.英语用户名 & SS地址标识 & 域名_英语
    '                列添加器 = New 类_列添加器
    '                列添加器.添加列_用于插入数据("群主", 群主.用户编号)
    '                列添加器.添加列_用于插入数据("群编号", 群编号)
    '                列添加器.添加列_用于插入数据("英语SS地址", 群主地址)
    '                If String.IsNullOrEmpty(域名_本国语) = False Then
    '                    列添加器.添加列_用于插入数据("本国语SS地址", 群主.本国语用户名 & SS地址标识 & 域名_本国语)
    '                End If
    '                列添加器.添加列_用于插入数据("主机名", 本服务器主机名)
    '                列添加器.添加列_用于插入数据("位置号", 群主位置号)
    '                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.群主)
    '                列添加器.添加列_用于插入数据("加入时间", 当前时刻)
    '                指令 = New 类_数据库指令_插入新数据(主数据库, "群成员", 列添加器, True)
    '                指令.执行()
    '                列添加器 = New 类_列添加器
    '                列添加器.添加列_用于插入数据("用户编号", 群主.用户编号)
    '                列添加器.添加列_用于插入数据("位置号", 群主位置号)
    '                列添加器.添加列_用于插入数据("群主地址", 群主地址)
    '                列添加器.添加列_用于插入数据("群编号", 群编号)
    '                列添加器.添加列_用于插入数据("群备注", 备注)
    '                列添加器.添加列_用于插入数据("加入时间", 当前时刻)
    '                指令 = New 类_数据库指令_插入新数据(主数据库, "加入的群", 列添加器)
    '                指令.执行()
    '            End If
    '            Return New 类_SS包生成器(查询结果_常量集合.成功)
    '        Catch ex As Exception
    '            If 读取器 IsNot Nothing Then 读取器.关闭()
    '            Return New 类_SS包生成器(ex.Message)
    '        End Try
    '    End Function

    Private Function 数据库_添加邀请(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 英语SS地址 As String, ByVal 本国语SS地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("角色")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群主编号英语SS地址")
            Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
            读取器 = 指令.执行()
            While 读取器.读取
                角色 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If 角色 >= 群角色_常量集合.普通成员 Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器,  , , 最大值_常量集合.小聊天群成员数量, "#群主编号角色加入时间")
            Dim 群成员数量 As Short
            读取器 = 指令.执行()
            While 读取器.读取
                群成员数量 += 1
            End While
            读取器.关闭()
            If 群成员数量 >= 最大值_常量集合.小聊天群成员数量 Then Return New 类_SS包生成器(查询结果_常量集合.出错)
            If 角色 = 群角色_常量集合.无 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("群主", 用户编号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("英语SS地址", 英语SS地址)
                If String.IsNullOrEmpty(本国语SS地址) = False Then
                    列添加器.添加列_用于插入数据("本国语SS地址", 本国语SS地址)
                End If
                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.邀请加入)
                列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
                Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "群成员", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入者信息(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 加入者 As String,
                              ByRef 本国语SS地址 As String, ByRef 主机名 As String, ByRef 位置号 As Short, ByRef 角色 As 群角色_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 加入者)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"本国语SS地址", "主机名", "位置号", "角色"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群主编号英语SS地址")
            读取器 = 指令.执行()
            While 读取器.读取
                本国语SS地址 = 读取器(0)
                主机名 = 读取器(1)
                位置号 = 读取器(2)
                角色 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_是否加入了群(ByVal 用户编号 As Long, ByVal 群主英语SS地址 As String, ByVal 群编号 As Byte, ByRef 加入了 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主英语SS地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的群", 筛选器, 1, , , "#群主编号用户")
            读取器 = 指令.执行()
            While 读取器.读取
                加入了 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入的群数量(ByVal 用户编号 As Long, ByRef 加入的群数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的群", 筛选器,  , , 最大值_常量集合.每个用户可加入的小聊天群数量, "#用户加入时间")
            读取器 = 指令.执行()
            While 读取器.读取
                加入的群数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_加入群(ByVal 用户编号 As Long, ByVal 位置号 As Short, ByVal 群主英语SS地址 As String, ByVal 群编号 As Byte, ByVal 群备注 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            列添加器.添加列_用于插入数据("群主地址", 群主英语SS地址)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("群备注", 群备注)
            列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "加入的群", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_成为群成员(ByVal 群编号 As Byte, ByVal 英语SS地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("角色", 群角色_常量集合.普通成员)
            列添加器_新数据.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.等于, 群角色_常量集合.邀请加入)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "群成员", 列添加器_新数据, 筛选器, "#群编号英语SS地址")
            If 指令2.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群成员(ByVal 群主 As Long, ByVal 群编号 As Byte, ByVal 英语SS地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 群主)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令3 As New 类_数据库指令_删除数据(主数据库, "群成员", 筛选器, "#群主编号英语SS地址")
            指令3.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 群主地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令3 As New 类_数据库指令_删除数据(主数据库, "创建的群", 筛选器, "#用户群编号")
            指令3.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令3 = New 类_数据库指令_删除数据(主数据库, "群成员", 筛选器, "#群主编号英语SS地址")
            指令3.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "加入的群", 筛选器, "#群主编号用户")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_修改群备注(ByVal 用户编号 As Long, ByVal 群主英语SS地址 As String, ByVal 群编号 As Byte, ByVal 群备注 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("群备注", 群备注)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主英语SS地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("群备注", 筛选方式_常量集合.不等于, 群备注,  , False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "加入的群", 列添加器_新数据, 筛选器, "#群主编号用户")
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function


    Private Sub 定时器_Elapsed(sender As Object, e As ElapsedEventArgs) Handles 定时器.Elapsed
        If 用户目录 IsNot Nothing Then
            Dim 在线用户(用户目录.Length - 1) As Short
            Dim 某一用户 As 类_用户
            Dim I, 在线用户数 As Integer
            For I = 0 To 用户目录.Length - 1
                If 用户目录(I) IsNot Nothing Then
                    某一用户 = 用户目录(I)
                    If 某一用户.网络连接器_手机 IsNot Nothing OrElse 某一用户.网络连接器_电脑 IsNot Nothing Then
                        在线用户(在线用户数) = I
                        在线用户数 += 1
                    End If
                End If
            Next
            If 在线用户数 > 0 Then
                If 在线用户数 > 1 Then
                    Randomize()
                    I = Int(Rnd() * 在线用户数)
                Else
                    I = 0
                End If
                某一用户 = 用户目录(在线用户(I))
                If 某一用户 IsNot Nothing Then
                    If 某一用户.网络连接器_手机 IsNot Nothing Then
                        If 跨进程锁.WaitOne = True Then
                            Try
                                数据库_存为推送的SS(本服务器主机名 & "." & 域名_英语, 0, 0, SS类型_常量集合.用http访问我, , , , , 设备类型_常量集合.手机, 在线用户(I))
                            Catch ex As Exception
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        End If
                    ElseIf 某一用户.网络连接器_电脑 IsNot Nothing Then
                        If 跨进程锁.WaitOne = True Then
                            Try
                                数据库_存为推送的SS(本服务器主机名 & "." & 域名_英语, 0, 0, SS类型_常量集合.用http访问我, , , , , 设备类型_常量集合.电脑, 在线用户(I))
                            Catch ex As Exception
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        End If
                    End If
                End If
                Return
            End If
        End If
        定时器.Stop()
        定时器 = Nothing
    End Sub

    Public Sub 重新计时()
        If 定时器 IsNot Nothing Then
            定时器.Stop()
            定时器.Start()
        End If
    End Sub

    Public Function 获取统计数据() As String
        Dim 变长文本 As New StringBuilder(300)
        Dim 文本写入器 As New StringWriter(变长文本)
        Dim 在线用户总数, 手机在线用户数, 电脑在线用户数, 手机和电脑同时在线用户数 As Integer
        If 用户目录 IsNot Nothing Then
            Dim 某一用户 As 类_用户
            Dim I As Integer
            For I = 0 To 用户目录.Length - 1
                某一用户 = 用户目录(I)
                If 某一用户 IsNot Nothing Then
                    If 某一用户.网络连接器_手机 IsNot Nothing Then
                        在线用户总数 += 1
                        If 某一用户.网络连接器_电脑 IsNot Nothing Then
                            手机和电脑同时在线用户数 += 1
                        Else
                            手机在线用户数 += 1
                        End If
                    ElseIf 某一用户.网络连接器_电脑 IsNot Nothing Then
                        在线用户总数 += 1
                        电脑在线用户数 += 1
                    End If
                End If
            Next
        End If
        文本写入器.Write("大聊天群服务器 " & 本服务器主机名 & "." & 域名_英语 & " 统计数据<br>")
        文本写入器.Write("====================<br>")
        If 群目录 IsNot Nothing Then
            文本写入器.Write("大聊天群数量：" & 群目录.Length & " 个；<br>")
        Else
            文本写入器.Write("大聊天群数量：" & 0 & " 个；<br>")
        End If
        文本写入器.Write("当前在线用户总数：" & 在线用户总数 & " 人，其中：<br>")
        文本写入器.Write(手机在线用户数 & " 人手机在线；<br>")
        文本写入器.Write(电脑在线用户数 & " 人电脑在线；<br>")
        文本写入器.Write(手机和电脑同时在线用户数 & " 人手机和电脑同时在线。<br>")
        文本写入器.Write("====================<br>")
        Dim 今日发送, 昨日发送, 前日发送 As Integer
        Dim 当前时间 As Date
        If 跨进程锁.WaitOne = True Then
            Try
                当前时间 = Date.Now
                Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                Dim 收发统计(最大值_常量集合.传送服务器承载用户数 - 1) As 群SS统计_复合数据
                Dim 收发统计数 As Integer
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "群SS统计", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        With 收发统计(收发统计数)
                            .群编号 = 读取器(0)
                            .今日几号 = 读取器(1)
                            .今日发送 = 读取器(2)
                            .昨日发送 = 读取器(3)
                            .前日发送 = 读取器(4)
                        End With
                        收发统计数 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                If 收发统计数 > 0 Then
                    Dim 结果 As 类_SS包生成器
                    Dim I As Integer
                    For I = 0 To 收发统计数 - 1
                        With 收发统计(I)
                            If 今日几号 = .今日几号 Then
                                今日发送 += .今日发送
                                昨日发送 += .昨日发送
                                前日发送 += .前日发送
                            ElseIf 昨日几号 = .今日几号 Then
                                结果 = 数据库_更新群收发统计(.群编号, 今日几号, 0, .今日发送, .昨日发送)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                昨日发送 += .今日发送
                                前日发送 += .昨日发送
                            ElseIf 前日几号 = .今日几号 Then
                                结果 = 数据库_更新群收发统计(.群编号, 今日几号, 0, 0, .今日发送)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                前日发送 += .今日发送
                            ElseIf .今日发送 > 0 OrElse .昨日发送 > 0 OrElse .前日发送 > 0 Then
                                结果 = 数据库_删除群收发统计(.群编号)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return ""
        End If
        文本写入器.Write("今日发送SS：" & 今日发送 & " 条；<br>")
        文本写入器.Write("昨日发送SS：" & 昨日发送 & " 条；<br>")
        文本写入器.Write("前日发送SS：" & 前日发送 & " 条；<br>")
        文本写入器.Write("====================<br>")
        文本写入器.Write(当前时间.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取在线用户详情() As String
        If 用户目录 Is Nothing Then Return ""
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.大聊天群服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("大聊天群服务器 " & 本服务器主机名 & "." & 域名_英语 & " 在线用户详情<br>")
        文本写入器.Write("====================<br>")
        Dim 某一用户 As 类_用户
        Dim I As Integer
        For I = 0 To 用户目录.Length - 1
            某一用户 = 用户目录(I)
            If 某一用户 IsNot Nothing Then
                If 某一用户.网络连接器_手机 IsNot Nothing OrElse 某一用户.网络连接器_电脑 IsNot Nothing Then
                    If String.IsNullOrEmpty(某一用户.本国语SS地址) = False Then
                        文本写入器.Write(某一用户.本国语SS地址 & " / " & 某一用户.英语SS地址 & "：")
                    Else
                        文本写入器.Write(某一用户.英语SS地址 & "：")
                    End If
                    If 某一用户.网络连接器_手机 IsNot Nothing Then
                        If 某一用户.网络连接器_电脑 IsNot Nothing Then
                            文本写入器.Write("手机和电脑")
                        Else
                            文本写入器.Write("手机")
                        End If
                    Else
                        文本写入器.Write("电脑")
                    End If
                    文本写入器.Write("<br>")
                End If
            End If
        Next
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取聊天群详情() As String
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.大聊天群服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("大聊天群服务器 " & 本服务器主机名 & "." & 域名_英语 & " 聊天群详情<br>")
        文本写入器.Write("====================<br>")
        If 群目录 IsNot Nothing Then
            For I = 0 To 群目录.Length - 1
                With 群目录(I)
                    文本写入器.Write(.名称)
                    文本写入器.Write(" 群：")
                    文本写入器.Write(.成员数)
                    文本写入器.Write(" 个成员（群主 ")
                    If String.IsNullOrEmpty(.群主本国语SS地址) = False Then
                        文本写入器.Write(.群主本国语SS地址 & " / " & .群主英语SS地址 & "）<br>")
                    Else
                        文本写入器.Write(.群主英语SS地址 & "）<br>")
                    End If
                End With
            Next
        End If
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取SS数量() As String
        If 用户目录 Is Nothing Then Return ""
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.传送服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("大聊天群服务器 " & 本服务器主机名 & "." & 域名_英语 & " SS数量<br>")
        文本写入器.Write("====================<br>")
        Dim 当前时间 As Date
        Dim 收发统计(最大值_常量集合.传送服务器承载用户数 - 1) As 个人SS统计_复合数据
        Dim 收发统计数 As Integer
        If 跨进程锁.WaitOne = True Then
            Try
                当前时间 = Date.Now
                Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "个人SS统计", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        With 收发统计(收发统计数)
                            .英语SS地址 = 读取器(0)
                            .本国语SS地址 = 读取器(1)
                            .今日几号 = 读取器(2)
                            .今日发送 = 读取器(3)
                            .昨日发送 = 读取器(4)
                            .前日发送 = 读取器(5)
                            .今日几时 = 读取器(6)
                            .时段发送 = 读取器(7)
                        End With
                        收发统计数 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                If 收发统计数 > 0 Then
                    Dim 结果 As 类_SS包生成器
                    Dim I As Integer
                    For I = 0 To 收发统计数 - 1
                        With 收发统计(I)
                            If 今日几号 <> .今日几号 Then
                                If 昨日几号 = .今日几号 Then
                                    结果 = 数据库_更新个人收发统计(.英语SS地址, 今日几号, 0, .今日发送, .昨日发送, 0, 0)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = .昨日发送
                                    .昨日发送 = .今日发送
                                    .今日发送 = 0
                                ElseIf 前日几号 = .今日几号 Then
                                    结果 = 数据库_更新个人收发统计(.英语SS地址, 今日几号, 0, 0, .今日发送, 0, 0)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = .今日发送
                                    .昨日发送 = 0
                                    .今日发送 = 0
                                ElseIf .今日发送 > 0 OrElse .昨日发送 > 0 OrElse .前日发送 > 0 OrElse .今日几时 > 0 OrElse .时段发送 > 0 Then
                                    结果 = 数据库_删除个人收发统计(.英语SS地址)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = 0
                                    .昨日发送 = 0
                                    .今日发送 = 0
                                End If
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return ""
        End If
        If 收发统计数 > 0 Then
            Dim I As Integer
            For I = 0 To 收发统计数 - 1
                With 收发统计(I)
                    If String.IsNullOrEmpty(.本国语SS地址) = False Then
                        文本写入器.Write(.本国语SS地址 & " / " & .英语SS地址 & "：")
                    Else
                        文本写入器.Write(.英语SS地址 & "：")
                    End If
                    文本写入器.Write("发送 " & .今日发送 & "/" & .昨日发送 & "/" & .前日发送 & "<br>")
                End With
            Next
        End If
        文本写入器.Write("====================<br>")
        文本写入器.Write(当前时间.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_更新今日个人收发统计(ByVal 英语SS地址 As String, ByVal 今日发送 As Short,
                                  ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "个人SS统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新今日群收发统计(ByVal 群编号 As Short, ByVal 今日发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "群SS统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新个人收发统计(ByVal 英语SS地址 As String, ByVal 今日几号 As Integer, ByVal 今日发送 As Short,
                                ByVal 昨日发送 As Short, ByVal 前日发送 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("昨日发送", 昨日发送)
            列添加器_新数据.添加列_用于插入数据("前日发送", 前日发送)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "个人SS统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新群收发统计(ByVal 群编号 As Short, ByVal 今日几号 As Integer, ByVal 今日发送 As Short,
                                ByVal 昨日发送 As Short, ByVal 前日发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("昨日发送", 昨日发送)
            列添加器_新数据.添加列_用于插入数据("前日发送", 前日发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "群SS统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加个人收发统计(ByVal 英语SS地址 As String, ByVal 本国语SS地址 As String,
                                ByVal 今日几号 As Integer, ByVal 今日发送 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("英语SS地址", 英语SS地址)
            列添加器.添加列_用于插入数据("本国语SS地址", 本国语SS地址)
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日发送", 今日发送)
            列添加器.添加列_用于插入数据("昨日发送", 0)
            列添加器.添加列_用于插入数据("前日发送", 0)
            列添加器.添加列_用于插入数据("今日几时", 今日几时)
            列添加器.添加列_用于插入数据("时段发送", 时段发送)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "个人SS统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加群收发统计(ByVal 群编号 As Short, ByVal 今日几号 As Integer, ByVal 今日发送 As Short)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日发送", 今日发送)
            列添加器.添加列_用于插入数据("昨日发送", 0)
            列添加器.添加列_用于插入数据("前日发送", 0)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "群SS统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除个人收发统计(ByVal 英语SS地址 As String)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语SS地址", 筛选方式_常量集合.等于, 英语SS地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "个人SS统计", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群收发统计(ByVal 群编号 As Long)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "群SS统计", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                关闭 = True
                If 网络连接器_侦听 IsNot Nothing Then
                    Try
                        网络连接器_侦听.Close()
                    Catch ex As Exception
                    End Try
                    网络连接器_侦听 = Nothing
                End If
                If 线程_侦听 IsNot Nothing Then
                    Try
                        线程_侦听.Abort()
                    Catch ex As Exception
                    End Try
                    线程_侦听 = Nothing
                End If
                If 线程_分配SS推送任务 IsNot Nothing Then
                    Try
                        线程_分配SS推送任务.Abort()
                    Catch ex As Exception
                    End Try
                    线程_分配SS推送任务 = Nothing
                End If
                If 线程_清除带文件SS IsNot Nothing Then
                    Try
                        线程_清除带文件SS.Abort()
                    Catch ex As Exception
                    End Try
                    线程_清除带文件SS = Nothing
                End If
                If SS推送器 IsNot Nothing Then
                    Dim I As Integer
                    For I = 0 To SS推送器.Length - 1
                        If SS推送器(I) IsNot Nothing Then
                            SS推送器(I).Dispose()
                        End If
                    Next
                End If
            End If
        End If
        Me.disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

#End Region

End Class
