Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Web
Imports System.Timers
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Public Class 类_讯宝管理器
    Implements IDisposable

#Region "定义和声明"

    Private Structure 要清除的讯宝_复合数据
        Dim 发送者编号 As Long
        Dim 讯宝指令 As 讯宝指令_常量集合
        Dim 文本 As String
    End Structure

    Private Structure 接收者服务器_复合数据
        Dim 子域名, 讯宝地址, 主机名 As String
        Dim SS包生成器 As 类_SS包生成器
        Dim 位置号 As Short
    End Structure

    Private Structure 讯友录变动_复合数据
        Dim 用户编号, 时间 As Long
        Dim 变动 As 讯友录变动_常量集合
        Dim 英语讯宝地址, 本国语讯宝地址, 标签一, 标签二, 原标签名, 新标签名 As String
        Dim 用户位置号 As Short
        Dim 拉黑 As Boolean
    End Structure

    Const 读取的讯宝最大数量 As Integer = 10
    Const 推送器最大数量 As Short = 10

    Dim 凭据_中心服务器, 网络地址_中心服务器 As String
    Public 本服务器主机名, 小宇宙写入服务器主机名 As String
    Friend 数据存放路径 As String
    Dim 头像存放目录 As String
    Dim 启动时间 As Long

    Friend 跨进程锁 As Mutex
    Friend 主数据库, 副数据库 As 类_数据库
    Dim 线程_侦听, 线程_分配讯宝发送任务, 线程_分配讯宝推送任务, 线程_清除讯宝, 线程_同步讯友录 As Thread

    Public 关闭 As Boolean
    Dim 网络连接器_侦听 As Socket

    Friend 用户目录() As 类_用户

    Dim 要发送的讯宝() As 类_要发送的讯宝
    Dim 要推送的讯宝() As 类_要推送的讯宝
    Dim 要发送的讯宝数量, 要推送的讯宝数量 As Integer

    Dim 讯宝域发送器() As 类_讯宝域发送器
    Dim 讯宝域数量 As Short
    Friend 讯宝推送器() As 类_讯宝推送器

    Public 连接凭据_管理员 As String

    WithEvents 定时器 As Timers.Timer

#End Region

    Public Sub New(ByVal Context As HttpContext, ByVal 跨进程锁1 As Mutex, ByVal 主数据库1 As 类_数据库, ByVal 副数据库1 As 类_数据库)
        数据存放路径 = Context.Server.MapPath("/") & "App_Data\"
        头像存放目录 = Context.Server.MapPath("/") & "icons"
        跨进程锁 = 跨进程锁1
        主数据库 = 主数据库1
        副数据库 = 副数据库1
    End Sub

    Public Function 验证中心服务器(ByVal 网络地址 As String, ByVal 服务器凭据 As String) As Boolean
        If String.Compare(网络地址, 网络地址_中心服务器) <> 0 Then Return False
        If String.Compare(服务器凭据, 凭据_中心服务器) <> 0 Then Return False
        Return True
    End Function

    Public Function 查找用户编号(ByVal 英语讯宝地址 As String, ByVal 位置号 As Short) As Long
        If 用户目录 Is Nothing Then Return 0
        If 位置号 >= 用户目录.Length Then Return 0
        Dim 某一用户 As 类_用户 = 用户目录(位置号)
        If 英语讯宝地址.StartsWith(某一用户.英语用户名 & 讯宝地址标识) Then
            Return 某一用户.用户编号
        Else
            Return 0
        End If
    End Function

    Public Sub 启动()
        启动时间 = Date.UtcNow.Ticks
        凭据_中心服务器 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器)
        Dim 线程 As New Thread(New ThreadStart(AddressOf 启动2))
        线程.Start()
    End Sub

    Private Sub 启动2()
        Dim 中心服务器子域名 As String = 获取服务器域名(讯宝中心服务器主机名 & "." & 域名_英语)
        Dim 访问结果 As Object = 访问其它服务器("https://" & 中心服务器子域名 & "/?C=ServerStart&Credential=" & 替换URI敏感字符(凭据_中心服务器) & "&Type=" & 服务器类别_常量集合.传送服务器, , 30000)
        If TypeOf 访问结果 Is 类_SS包生成器 Then Return
        Try
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then Return
            If 跨进程锁.WaitOne = True Then
                Try
                    Dim 网络地址 As New IPAddress(0)
                    If IPAddress.TryParse(网络地址_中心服务器, 网络地址) = False Then Return
                    Dim 网络地址字节数组() As Byte = 网络地址.GetAddressBytes
                    Dim 列添加器_新数据 As New 类_列添加器
                    列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址字节数组)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_我访它", 凭据_中心服务器)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_它访我", 凭据_中心服务器)
                    列添加器_新数据.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 中心服务器子域名)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令 As New 类_数据库指令_更新数据(副数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
                    If 指令.执行() = 0 Then
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于插入数据("子域名", 中心服务器子域名)
                        列添加器.添加列_用于插入数据("网络地址", 网络地址字节数组)
                        列添加器.添加列_用于插入数据("连接凭据_我访它", 凭据_中心服务器)
                        列添加器.添加列_用于插入数据("连接凭据_它访我", 凭据_中心服务器)
                        列添加器.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                        Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "服务器", 列添加器)
                        指令2.执行()
                    End If
                Catch ex As Exception
                    Return
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Return
            End If
            SS包解读器.读取_有标签("主机名", 本服务器主机名, Nothing)
            If String.IsNullOrEmpty(本服务器主机名) Then Return
            If 本服务器主机名.Length > 最大值_常量集合.主机名字符数 Then Return
            SS包解读器.读取_有标签("小宇宙写入服务器", 小宇宙写入服务器主机名, Nothing)
            If String.IsNullOrEmpty(小宇宙写入服务器主机名) Then Return
            If 小宇宙写入服务器主机名.Length > 最大值_常量集合.主机名字符数 Then Return
            Dim 用户目录2(最大值_常量集合.传送服务器承载用户数 - 1) As 类_用户
            Dim I As Integer
            For I = 0 To 用户目录2.Length - 1
                用户目录2(I) = New 类_用户()
            Next
            Dim 非英语 As Boolean
            If String.IsNullOrEmpty(域名_本国语) = False Then
                非英语 = True
            End If
            Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("用户")
            If SS包解读器2 IsNot Nothing Then
                Dim 位置号 As Short
                Dim 某一用户 As 类_用户
                Dim SS包解读器3 As 类_SS包解读器 = Nothing
                For I = 0 To SS包解读器2.Length - 1
                    某一用户 = New 类_用户
                    With CType(SS包解读器2(I), 类_SS包解读器)
                        .读取_有标签("编号", 某一用户.用户编号)
                        If 某一用户.用户编号 = 0 Then Return
                        .读取_有标签("英语", 某一用户.英语用户名)
                        If String.IsNullOrEmpty(某一用户.英语用户名) Then Return
                        If 非英语 Then
                            .读取_有标签("本国语", 某一用户.本国语用户名)
                        End If
                        .读取_有标签("位置号", 位置号, 0)
                        If 位置号 < 0 Then Return
                        .读取_有标签("停用", 某一用户.停用)
                    End With
                    用户目录2(位置号) = 某一用户
                Next
            End If
            用户目录 = 用户目录2
            If 线程_侦听 Is Nothing Then
                线程_侦听 = New Thread(New ThreadStart(AddressOf 侦听))
                线程_侦听.Start()
            End If
            If 线程_分配讯宝推送任务 Is Nothing Then
                ReDim 讯宝推送器(推送器最大数量 - 1)
                线程_分配讯宝推送任务 = New Thread(New ThreadStart(AddressOf 分配讯宝推送任务))
                线程_分配讯宝推送任务.Start()
            End If
            If 线程_分配讯宝发送任务 Is Nothing Then
                线程_分配讯宝发送任务 = New Thread(New ThreadStart(AddressOf 分配讯宝发送任务))
                线程_分配讯宝发送任务.Start()
            End If
            If 线程_清除讯宝 Is Nothing Then
                线程_清除讯宝 = New Thread(New ThreadStart(AddressOf 清除讯宝))
                线程_清除讯宝.Start()
            End If
            If 线程_同步讯友录 Is Nothing Then
                线程_同步讯友录 = New Thread(New ThreadStart(AddressOf 同步讯友录))
                线程_同步讯友录.Start()
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

    Public Function 用户上线或离线(ByVal 用户编号 As Long, ByVal 位置号 As Short, ByVal 字节数组() As Byte, ByVal 设备类型 As String) As 类_SS包生成器
        If 用户目录 Is Nothing Then
            If DateDiff(DateInterval.Minute, Date.FromBinary(启动时间), Date.UtcNow) >= 2 Then 启动()
            Return New 类_SS包生成器(查询结果_常量集合.服务器未就绪)
        End If
        If 位置号 >= 用户目录.Length Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        Dim 某一用户 As 类_用户 = 用户目录(位置号)
        If 某一用户 Is Nothing Then
            If DateDiff(DateInterval.Minute, Date.FromBinary(启动时间), Date.UtcNow) >= 2 Then 启动()
            Return New 类_SS包生成器(查询结果_常量集合.服务器未就绪)
        End If
        If 某一用户.用户编号 > 0 Then
            If 某一用户.用户编号 <> 用户编号 Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(设备类型) = False Then
            If 字节数组 IsNot Nothing Then
                If 某一用户.用户编号 = 0 Then 某一用户.用户编号 = 用户编号
                Dim 对称密钥() As Byte = Nothing
                Dim 初始向量() As Byte = Nothing
                Dim 密钥创建时间 As Long
                Try
                    Dim SS包解读器2 As New 类_SS包解读器(字节数组)
                    With SS包解读器2
                        .读取_有标签("英语用户名", 某一用户.英语用户名)
                        If String.IsNullOrEmpty(某一用户.英语用户名) Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                        If String.IsNullOrEmpty(域名_本国语) = False Then
                            .读取_有标签("本国语用户名", 某一用户.本国语用户名)
                            If String.IsNullOrEmpty(某一用户.本国语用户名) Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                        End If
                        .读取_有标签("对称密钥", 对称密钥)
                        If 对称密钥 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                        .读取_有标签("初始向量", 初始向量)
                        If 初始向量 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                        .读取_有标签("时间", 密钥创建时间)
                    End With
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                End Try
                Select Case 设备类型
                    Case 设备类型_手机
                        If 某一用户.网络连接器_手机 IsNot Nothing Then
                            Try
                                某一用户.网络连接器_手机.Close()
                            Catch ex As Exception
                            End Try
                            某一用户.网络连接器_手机 = Nothing
                        End If
                        Dim AES加解密模块 As New RijndaelManaged
                        AES加解密模块.Key = 对称密钥
                        AES加解密模块.IV = 初始向量
                        某一用户.AES加解密模块_手机 = AES加解密模块
                        某一用户.AES解密器_手机 = AES加解密模块.CreateDecryptor
                        某一用户.AES加密器_手机 = AES加解密模块.CreateEncryptor
                        某一用户.AES密钥创建时间_手机 = 密钥创建时间
                    Case 设备类型_电脑
                        If 某一用户.网络连接器_电脑 IsNot Nothing Then
                            Try
                                某一用户.网络连接器_电脑.Close()
                            Catch ex As Exception
                            End Try
                            某一用户.网络连接器_电脑 = Nothing
                        End If
                        Dim AES加解密模块 As New RijndaelManaged
                        AES加解密模块.Key = 对称密钥
                        AES加解密模块.IV = 初始向量
                        某一用户.AES加解密模块_电脑 = AES加解密模块
                        某一用户.AES解密器_电脑 = AES加解密模块.CreateDecryptor
                        某一用户.AES加密器_电脑 = AES加解密模块.CreateEncryptor
                        某一用户.AES密钥创建时间_电脑 = 密钥创建时间
                    Case Else
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                End Select
                If 定时器 Is Nothing Then
                    定时器 = New Timers.Timer
                    定时器.Interval = 120000
                    定时器.Start()
                End If
            Else
                Select Case 设备类型
                    Case 设备类型_手机
                        释放手机连接(某一用户)
                    Case 设备类型_电脑
                        释放电脑连接(某一用户)
                    Case Else
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                End Select
            End If
        Else
            释放手机连接(某一用户)
            释放电脑连接(某一用户)
        End If
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

    Private Sub 释放手机连接(ByVal 某一用户 As 类_用户)
        If 某一用户.网络连接器_手机 IsNot Nothing Then
            Try
                某一用户.网络连接器_手机.Close()
            Catch ex As Exception
            End Try
            某一用户.网络连接器_手机 = Nothing
        End If
        某一用户.讯宝序号_手机发送 = 0
        If 某一用户.AES解密器_手机 IsNot Nothing Then
            某一用户.AES解密器_手机.Dispose()
            某一用户.AES解密器_手机 = Nothing
        End If
        If 某一用户.AES加密器_手机 IsNot Nothing Then
            某一用户.AES加密器_手机.Dispose()
            某一用户.AES加密器_手机 = Nothing
        End If
        If 某一用户.AES加解密模块_手机 IsNot Nothing Then
            某一用户.AES加解密模块_手机.Dispose()
            某一用户.AES加解密模块_手机 = Nothing
        End If
    End Sub

    Private Sub 释放电脑连接(ByVal 某一用户 As 类_用户)
        If 某一用户.网络连接器_电脑 IsNot Nothing Then
            Try
                某一用户.网络连接器_电脑.Close()
            Catch ex As Exception
            End Try
            某一用户.网络连接器_电脑 = Nothing
        End If
        某一用户.讯宝序号_电脑发送 = 0
        If 某一用户.AES解密器_电脑 IsNot Nothing Then
            某一用户.AES解密器_电脑.Dispose()
            某一用户.AES解密器_电脑 = Nothing
        End If
        If 某一用户.AES加密器_电脑 IsNot Nothing Then
            某一用户.AES加密器_电脑.Dispose()
            某一用户.AES加密器_电脑 = Nothing
        End If
        If 某一用户.AES加解密模块_电脑 IsNot Nothing Then
            某一用户.AES加解密模块_电脑.Dispose()
            某一用户.AES加解密模块_电脑 = Nothing
        End If
    End Sub

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
                                数据库_存为推送的讯宝(本服务器主机名 & "." & 域名_英语, 0, 0, Nothing, 0, 某一用户.用户编号, I, 设备类型_常量集合.手机, 讯宝指令_常量集合.用http访问我)
                            Catch ex As Exception
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        End If
                    ElseIf 某一用户.网络连接器_电脑 IsNot Nothing Then
                        If 跨进程锁.WaitOne = True Then
                            Try
                                数据库_存为推送的讯宝(本服务器主机名 & "." & 域名_英语, 0, 0, Nothing, 0, 某一用户.用户编号, I, 设备类型_常量集合.电脑, 讯宝指令_常量集合.用http访问我)
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


    Private Function 数据库_记录带文件的讯宝(ByVal 发送者编号 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("发送者编号", 发送者编号)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本", 文本)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "带文件讯宝", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 清除讯宝()
        Try
            Do
                Dim 需要清除的讯宝(499) As 要清除的讯宝_复合数据
                Dim 需要清除的讯宝数量 As Integer = 0
                Dim 时间截点 As Long
                Dim 结果 As 类_SS包生成器
                If 跨进程锁.WaitOne = True Then
                    Try
                        时间截点 = Date.UtcNow.AddDays(-最大值_常量集合.讯宝文件保留天数).Ticks
                        结果 = 数据库_获取需要清除的带文件讯宝(需要清除的讯宝, 需要清除的讯宝数量, 时间截点)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return
                    Catch ex As Exception
                        Return
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                End If
                If 需要清除的讯宝数量 > 0 Then
                    For I = 0 To 需要清除的讯宝数量 - 1
                        With 需要清除的讯宝(I)
                            Select Case .讯宝指令
                                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                    Dim 路径 As String = 数据存放路径 & "SS\" & .发送者编号 & "\" & .文本
                                    If File.Exists(路径) Then
                                        Try
                                            File.Delete(路径)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                    路径 &= ".jpg"
                                    If File.Exists(路径) Then
                                        Try
                                            File.Delete(路径)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送文件
                                    Dim 路径 As String = 数据存放路径 & "SS\" & .发送者编号 & "\" & .文本
                                    If File.Exists(路径) Then
                                        Try
                                            File.Delete(路径)
                                        Catch ex As Exception
                                        End Try
                                    End If
                            End Select
                        End With
                    Next
                End If
                If 跨进程锁.WaitOne = True Then
                    Try
                        If 需要清除的讯宝数量 > 0 Then
                            结果 = 数据库_清除带文件的讯宝(时间截点)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return
                            End If
                        End If
                        结果 = 数据库_清除不推送的讯宝(时间截点)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return
                        End If
                    Catch ex As Exception
                        Return
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                End If
                Thread.Sleep(300000)
            Loop Until 关闭
        Catch ex As Exception
        End Try
    End Sub

    Private Function 数据库_获取需要清除的带文件讯宝(ByRef 需要清除的讯宝() As 要清除的讯宝_复合数据, ByRef 需要清除的讯宝数量 As Integer, ByVal 时间截点 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间截点)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"发送者编号", "指令", "文本"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "带文件讯宝", 筛选器,  , 列添加器, 100, "#时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                If 需要清除的讯宝数量 = 需要清除的讯宝.Length Then ReDim Preserve 需要清除的讯宝(需要清除的讯宝数量 * 2 - 1)
                With 需要清除的讯宝(需要清除的讯宝数量)
                    .发送者编号 = 读取器(0)
                    .讯宝指令 = 读取器(1)
                    .文本 = 读取器(2)
                End With
                需要清除的讯宝数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_清除带文件的讯宝(ByVal 时间截点 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间截点)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "带文件讯宝", 筛选器, "#时间")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_清除不推送的讯宝(ByVal 时间截点 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间截点)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "讯宝不推送", 筛选器, "#时间")
            指令.执行()
            Dim I, 最大字数, 最大字数2 As Integer
            For I = 1 To 最大值_常量集合.讯宝文本长度
                最大字数 = 获取文本库号(I)
                If 最大字数 <> 最大字数2 Then
                    最大字数2 = 最大字数
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间截点)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    指令 = New 类_数据库指令_删除数据(副数据库, 最大字数 & "库", 筛选器, "#时间")
                    指令.执行()
                End If
            Next
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function


    Private Sub 同步讯友录()
        Dim 子域名_小宇宙服务器 As String = 获取服务器域名(小宇宙写入服务器主机名 & "." & 域名_英语)
        Dim 访问本域服务器的凭据 As String = Nothing
        Try
            Do
                Dim 讯友录变动() As 讯友录变动_复合数据 = Nothing
                Dim 讯友录变动数量 As Integer = 0
                Dim 结果 As 类_SS包生成器
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_获取讯友录变动记录(讯友录变动, 讯友录变动数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                        If 讯友录变动数量 > 0 Then
                            Dim I As Integer
                            For I = 0 To 讯友录变动数量 - 1
                                Select Case 讯友录变动(I).变动
                                    Case 讯友录变动_常量集合.添加, 讯友录变动_常量集合.修改拉黑
                                        结果 = 数据库_获取讯友(讯友录变动(I))
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                End Select
                            Next
                            If String.IsNullOrEmpty(访问本域服务器的凭据) = True Then
                                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_小宇宙服务器, 访问本域服务器的凭据)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            End If
                        End If
                    Catch ex As Exception
                        Throw New Exception
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                End If
                If 讯友录变动数量 > 0 Then
                    If String.IsNullOrEmpty(访问本域服务器的凭据) Then
                        结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_小宇宙服务器, 访问本域服务器的凭据)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                    End If
                    Dim SS包生成器 As New 类_SS包生成器
                    Dim 某一用户 As 类_用户
                    Dim I As Integer
                    For I = 0 To 讯友录变动数量 - 1
                        Dim SS包生成器2 As New 类_SS包生成器
                        With 讯友录变动(I)
                            某一用户 = 用户目录(.用户位置号)
                            If 某一用户.用户编号 <> .用户编号 Then Continue For
                            SS包生成器2.添加_有标签("用户名", 某一用户.英语用户名)
                            SS包生成器2.添加_有标签("类型", .变动)
                            Select Case .变动
                                Case 讯友录变动_常量集合.添加, 讯友录变动_常量集合.修改拉黑
                                    SS包生成器2.添加_有标签("英语", .英语讯宝地址)
                                    If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                                        SS包生成器2.添加_有标签("本国语", .本国语讯宝地址)
                                    End If
                                    If String.IsNullOrEmpty(.标签一) = False Then
                                        SS包生成器2.添加_有标签("标签一", .标签一)
                                    End If
                                    If String.IsNullOrEmpty(.标签二) = False Then
                                        SS包生成器2.添加_有标签("标签二", .标签二)
                                    End If
                                    SS包生成器2.添加_有标签("拉黑", .拉黑)
                                Case 讯友录变动_常量集合.删除
                                    SS包生成器2.添加_有标签("英语", .英语讯宝地址)
                                Case 讯友录变动_常量集合.重命名标签
                                    If String.Compare(.原标签名, .新标签名) = 0 Then Continue For
                                    SS包生成器2.添加_有标签("原标签名", .原标签名)
                                    SS包生成器2.添加_有标签("新标签名", .新标签名)
                            End Select
                        End With
                        SS包生成器.添加_有标签("变动", SS包生成器2)
                    Next
                    Dim 连接凭据 As String = Nothing
                    Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名_小宇宙服务器 & "/?C=SyncSSpalList&Credential=" & 替换URI敏感字符(访问本域服务器的凭据) & "&HostName=" & 本服务器主机名, SS包生成器.生成SS包)
                    If TypeOf 访问结果 Is 类_SS包生成器 Then
                        GoTo 跳转点1
                    Else
                        Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
                        If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then GoTo 跳转点1
                    End If
                    If 跨进程锁.WaitOne = True Then
                        Try
                            结果 = 数据库_清除讯友录变动记录(讯友录变动(讯友录变动数量 - 1).时间)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                        Catch ex As Exception
                            Throw New Exception
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    End If
                End If
跳转点1:
                Thread.Sleep(60000)
            Loop Until 关闭
        Catch ex As Exception
        End Try
    End Sub

    Private Function 数据库_获取讯友录变动记录(ByRef 讯友录变动() As 讯友录变动_复合数据, ByRef 讯友录变动数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            ReDim 讯友录变动(199)
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "讯友录变动", Nothing, , , 100, "#时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                If 讯友录变动数量 = 讯友录变动.Length Then ReDim Preserve 讯友录变动(讯友录变动数量 * 2 - 1)
                With 讯友录变动(讯友录变动数量)
                    .用户编号 = 读取器(0)
                    .用户位置号 = 读取器(1)
                    .变动 = 读取器(2)
                    .英语讯宝地址 = 读取器(3)
                    If .变动 = 讯友录变动_常量集合.重命名标签 Then
                        .原标签名 = 读取器(4)
                        .新标签名 = 读取器(5)
                    End If
                    .时间 = 读取器(6)
                    If 讯友录变动数量 >= 100 Then
                        If .时间 <> 讯友录变动(讯友录变动数量 - 1).时间 Then
                            Exit While
                        End If
                    End If
                End With
                讯友录变动数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取讯友(ByRef 讯友录变动 As 讯友录变动_复合数据) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 讯友录变动.用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 讯友录变动.英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"本国语讯宝地址", "标签一", "标签二", "拉黑"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器, , "#用户英语讯宝地址")
            读取器 = 指令2.执行()
            While 读取器.读取
                讯友录变动.本国语讯宝地址 = 读取器(0)
                讯友录变动.标签一 = 读取器(1)
                讯友录变动.标签二 = 读取器(2)
                讯友录变动.拉黑 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_清除讯友录变动记录(ByVal 时间截点 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于等于, 时间截点)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "讯友录变动", 筛选器, "#时间")
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
                If 线程_分配讯宝发送任务 IsNot Nothing Then
                    Try
                        线程_分配讯宝发送任务.Abort()
                    Catch ex As Exception
                    End Try
                    线程_分配讯宝发送任务 = Nothing
                End If
                If 线程_分配讯宝推送任务 IsNot Nothing Then
                    Try
                        线程_分配讯宝推送任务.Abort()
                    Catch ex As Exception
                    End Try
                    线程_分配讯宝推送任务 = Nothing
                End If
                If 线程_清除讯宝 IsNot Nothing Then
                    Try
                        线程_清除讯宝.Abort()
                    Catch ex As Exception
                    End Try
                    线程_清除讯宝 = Nothing
                End If
                If 线程_同步讯友录 IsNot Nothing Then
                    Try
                        线程_同步讯友录.Abort()
                    Catch ex As Exception
                    End Try
                    线程_同步讯友录 = Nothing
                End If
                If 讯宝推送器 IsNot Nothing Then
                    Dim I As Integer
                    For I = 0 To 讯宝推送器.Length - 1
                        If 讯宝推送器(I) IsNot Nothing Then
                            讯宝推送器(I).Dispose()
                        End If
                    Next
                End If
                If 讯宝域发送器 IsNot Nothing AndAlso 讯宝域数量 > 0 Then
                    Dim I As Integer
                    For I = 0 To 讯宝域数量 - 1
                        With 讯宝域发送器(I)
                            If .发送器1 IsNot Nothing Then .发送器1.Dispose()
                            If .发送器2 IsNot Nothing Then .发送器2.Dispose()
                        End With
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
