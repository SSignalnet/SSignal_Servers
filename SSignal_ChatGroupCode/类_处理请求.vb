Imports System.Web
Imports System.Net
Imports System.Threading
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Public Class 类_处理请求

    Dim 应用程序 As HttpApplicationState
    Dim 跨进程锁 As Mutex
    Dim 主数据库, 副数据库 As 类_数据库
    Dim 启动器 As 类_启动器
    Dim Context As HttpContext
    Dim Http请求 As HttpRequest

    Public Sub New(ByVal 应用程序1 As HttpApplicationState, ByVal Context1 As HttpContext, ByVal Http请求1 As HttpRequest)
        应用程序 = 应用程序1
        If 应用程序 IsNot Nothing Then
            跨进程锁 = 应用程序.Get("Mu_SSG")
            主数据库 = 应用程序.Get("Rb_SSG")
            副数据库 = 应用程序.Get("Nb_SSG")
            启动器 = 应用程序.Get("Ln_SSG")
        End If
        Context = Context1
        Http请求 = Http请求1
    End Sub

    Private Function 数据库_验证连接凭据(ByVal 英语讯宝地址 As String, ByVal 连接凭据 As String, ByVal 群编号 As Long,
                                                                ByRef 角色 As 群角色_常量集合, Optional ByRef 本国语讯宝地址 As String = Nothing,
                                                                Optional ByRef 主机名 As String = Nothing) As 类_SS包生成器
        If String.IsNullOrEmpty(连接凭据) = True Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        If 连接凭据.Length <> 长度_常量集合.连接凭据_客户端 Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If 连接凭据.StartsWith(设备类型_手机) Then
                列添加器.添加列_用于获取数据("连接凭据_手机")
            Else
                列添加器.添加列_用于获取数据("连接凭据_电脑")
            End If
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 连接凭据_数据库中的 As String = ""
            读取器 = 指令.执行()
            While 读取器.读取
                连接凭据_数据库中的 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If String.Compare(连接凭据, 连接凭据_数据库中的) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"角色", "本国语讯宝地址", "主机名"})
            指令 = New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群编号英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                角色 = 读取器(0)
                本国语讯宝地址 = 读取器(1)
                主机名 = 读取器(2)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.凭据有效)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 获取连接凭据() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If String.IsNullOrEmpty(启动器.本服务器主机名) = True Then
            If DateDiff(DateInterval.Minute, Date.FromBinary(启动器.启动时间), Date.UtcNow) >= 2 Then 启动器.启动()
            Return New 类_SS包生成器(查询结果_常量集合.服务器未就绪)
        End If
        Dim 传送服务器凭据 As String = Http请求("Credential")
        Dim 传送服务器子域名 As String = Http请求("Domain")
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim DeviceType As String = Http请求("DeviceType")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        If String.IsNullOrEmpty(传送服务器子域名) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器子域名.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.IsNullOrEmpty(传送服务器凭据) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器凭据.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 是否是有效的讯宝或电子邮箱地址(EnglishSSAddress) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 段() As String = EnglishSSAddress.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Dim 点加域名 As String = "." & 段(1)
        Dim I As Integer = 传送服务器子域名.IndexOf(点加域名)
        If I <> 传送服务器子域名.Length - 点加域名.Length Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器子域名.Substring(0, I).Contains(".") Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Select Case DeviceType
            Case 设备类型_手机, 设备类型_电脑
            Case Else : Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End Select

        Dim 结果 As 类_SS包生成器 = Nothing
        Dim 更新时间 As Long
        Dim 传送服务器子域名2 As String = 获取服务器域名(传送服务器子域名)
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_验证其它服务器访问我方的凭据(Context, 副数据库, 传送服务器子域名2, 传送服务器凭据, 更新时间)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Select Case 结果.查询结果
            Case 查询结果_常量集合.凭据有效
            Case 查询结果_常量集合.需要添加连接凭据
跳转点1:
                Dim 访问结果 As Object = 访问其它服务器(获取路径_验证服务器真实性(传送服务器子域名, 传送服务器凭据, 启动器.本服务器主机名 & "." & 域名_英语))
                If TypeOf 访问结果 Is 类_SS包生成器 Then
                    Return 访问结果
                Else
                    Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
                    If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 访问结果
                    End If
                End If
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_添加其它服务器访问我方的凭据(Context, 副数据库, 传送服务器子域名2, Nothing, 传送服务器凭据)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                    Catch ex As Exception
                        Return New 类_SS包生成器(ex.Message)
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Case 查询结果_常量集合.不正确, 查询结果_常量集合.未知IP地址
                If 更新时间 > 0 Then
                    If DateDiff(DateInterval.Minute, Date.FromBinary(更新时间), Date.UtcNow) < 5 Then
                        Return 结果
                    End If
                End If
                GoTo 跳转点1
            Case Else : Return 结果
        End Select
        Dim 图标更新时间 As Long
        Dim 文件信息 As New FileInfo(Context.Server.MapPath("/") & "icons\" & GroupID & ".jpg")
        If 文件信息.Exists Then 图标更新时间 = 文件信息.LastWriteTimeUtc.Ticks
        If 跨进程锁.WaitOne = True Then
            Try
                Return 数据库_获取连接凭据(EnglishSSAddress, 传送服务器子域名, DeviceType, GroupID, 图标更新时间)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_获取连接凭据(ByVal 英语讯宝地址 As String, ByVal 传送服务器子域名 As String,
                                                                ByVal 设备类型 As String, ByVal 群编号 As Long, ByVal 图标更新时间 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"角色", "主机名", "位置号", "本国语讯宝地址"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器,  , "#群编号英语讯宝地址")
            Dim 角色 As 群角色_常量集合
            Dim 主机名 As String = ""
            Dim 位置号 As Short
            Dim 本国语讯宝地址 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                角色 = 读取器(0)
                主机名 = 读取器(1)
                位置号 = 读取器(2)
                本国语讯宝地址 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            If 角色 = 群角色_常量集合.无 Then
                Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
            End If
            If 传送服务器子域名.StartsWith(主机名 & ".") = False Then
                Return Nothing
            End If
            If 角色 = 群角色_常量集合.邀请加入_可以发言 OrElse 角色 = 群角色_常量集合.邀请加入_不可发言 Then
                Dim 用户数 As Integer
                Dim 结果2 As 类_SS包生成器 = 数据库_统计已有用户数(用户数)
                If 结果2.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果2
                End If
                If 用户数 >= 最大值_常量集合.大聊天群服务器承载用户数 Then
                    Return New 类_SS包生成器(查询结果_常量集合.大聊天群服务器用户数已满)
                End If
                Dim 群数 As Integer
                结果2 = 数据库_统计已已加入的群数(英语讯宝地址, 群数)
                If 结果2.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果2
                End If
                If 群数 >= 最大值_常量集合.每个用户可加入的大聊天群数量 Then
                    Return New 类_SS包生成器(查询结果_常量集合.加入的大聊天群数量已达上限)
                End If
                If 角色 = 群角色_常量集合.邀请加入_可以发言 Then
                    角色 = 群角色_常量集合.成员_可以发言
                Else
                    角色 = 群角色_常量集合.成员_不可发言
                End If
                Dim 列添加器_新数据2 As New 类_列添加器
                列添加器_新数据2.添加列_用于插入数据("角色", 角色)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("角色", 角色)
                Dim 指令3 As New 类_数据库指令_更新数据(主数据库, "群成员", 列添加器_新数据2, 筛选器, "#群编号英语讯宝地址")
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("名称")
            指令 = New 类_数据库指令_请求获取数据(主数据库, "群", 筛选器, 1, 列添加器,  , 主键索引名)
            Dim 群名称 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                群名称 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If String.Compare(设备类型, 设备类型_手机) = 0 Then
                列添加器.添加列_用于获取数据(New String() {"连接凭据_手机", "访问时间_手机"})
            Else
                列添加器.添加列_用于获取数据(New String() {"连接凭据_电脑", "访问时间_电脑"})
            End If
            指令 = New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器,  , 主键索引名)
            Dim 连接凭据 As String = Nothing
            Dim 访问时间 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                连接凭据 = 读取器(0)
                访问时间 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(连接凭据) = False Then
                If DateDiff(DateInterval.Hour, Date.FromBinary(访问时间), Date.UtcNow) < 24 Then
                    GoTo 跳转点1
                End If
            End If
            连接凭据 = 设备类型 & 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端 - 设备类型.Length)
            Dim 列添加器_新数据 As New 类_列添加器
            If String.Compare(设备类型, 设备类型_手机) = 0 Then
                列添加器_新数据.添加列_用于插入数据("连接凭据_手机", 连接凭据)
                列添加器_新数据.添加列_用于插入数据("访问时间_手机", Date.UtcNow.Ticks)
                列添加器_新数据.添加列_用于插入数据("网络地址_手机", 获取网络地址字节数组)
            Else
                列添加器_新数据.添加列_用于插入数据("连接凭据_电脑", 连接凭据)
                列添加器_新数据.添加列_用于插入数据("访问时间_电脑", Date.UtcNow.Ticks)
                列添加器_新数据.添加列_用于插入数据("网络地址_电脑", 获取网络地址字节数组)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令2.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                If String.Compare(设备类型, 设备类型_手机) = 0 Then
                    列添加器.添加列_用于插入数据("连接凭据_手机", 连接凭据)
                    列添加器.添加列_用于插入数据("网络地址_手机", 获取网络地址字节数组)
                Else
                    列添加器.添加列_用于插入数据("连接凭据_电脑", 连接凭据)
                    列添加器.添加列_用于插入数据("网络地址_电脑", 获取网络地址字节数组)
                End If
                列添加器.添加列_用于插入数据("访问时间_电脑", Date.UtcNow.Ticks)
                列添加器.添加列_用于插入数据("访问时间_手机", Date.UtcNow.Ticks)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
                指令3.执行()
            End If
跳转点1:
            Dim 结果 As New 类_SS包生成器(查询结果_常量集合.成功)
            Call 添加数据_获取大聊天群服务器连接凭据(结果, 群名称, 图标更新时间, 连接凭据, 角色, 域名_本国语)
            Return 结果
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_统计已有用户数(ByRef 用户数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.大于等于, 群角色_常量集合.成员_不可发言)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("英语讯宝地址")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", Nothing, , 列添加器, 100, "#英语讯宝地址群编号")
            Dim 英语讯宝地址 As String = Nothing
            Dim 英语讯宝地址2 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                英语讯宝地址2 = 读取器(0)
                If String.Compare(英语讯宝地址2, 英语讯宝地址) <> 0 Then
                    英语讯宝地址 = 英语讯宝地址2
                    用户数 += 1
                End If
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_统计已已加入的群数(ByVal 英语讯宝地址 As String, ByRef 群数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", Nothing, , , 100, "#英语讯宝地址群编号")
            读取器 = 指令.执行()
            While 读取器.读取
                群数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取最近操作次数(ByVal 群编号 As Long, ByRef 操作次数 As Integer, ByVal 操作代码 As 操作代码_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("操作时间", 筛选方式_常量集合.大于, Date.UtcNow.AddMinutes(-最近操作次数统计时间_分钟).Ticks)
            If 操作代码 > 操作代码_常量集合.无 Then
                列添加器.添加列_用于筛选器("操作代码", 筛选方式_常量集合.等于, 操作代码)
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "操作记录", 筛选器, , , 10, "#群编号操作时间")
            读取器 = 指令.执行()
            While 读取器.读取
                操作次数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存操作记录(ByVal 群编号 As Long, ByVal 操作代码 As 操作代码_常量集合) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("操作时间", 筛选方式_常量集合.小于, Date.UtcNow.AddHours(-24).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "操作记录", 筛选器, "#操作时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("操作代码", 操作代码)
            列添加器.添加列_用于插入数据("操作时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "操作记录", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 验证我方真实性() As 类_SS包生成器
        Dim Credential As String = Http请求("Credential")
        Dim Domain_Ask As String = Http请求("Domain_Ask")
        Dim Domain_English As String = Http请求("Domain_English")
        Dim Domain_Native As String = Http请求("Domain_Native")

        If String.IsNullOrEmpty(Credential) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If Credential.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.IsNullOrEmpty(Domain_Ask) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If Domain_Ask.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.IsNullOrEmpty(Domain_English) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Domain_English = Domain_English.Trim.ToLower
        If Domain_English.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.Compare(Domain_English, 启动器.本服务器主机名 & "." & 域名_英语) <> 0 Then
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(Domain_Native) = False Then
            Domain_Native = Domain_Native.Trim.ToLower
            If Domain_Native.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            If String.Compare(Domain_Native, 启动器.本服务器主机名 & "." & 域名_本国语) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        End If
        Dim 我方访问其它服务器的凭据 As String = Nothing
        Dim 网络地址_数据库中() As Byte = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_获取访问其它服务器的凭据(副数据库, 获取服务器域名(Domain_Ask), 我方访问其它服务器的凭据, 网络地址_数据库中)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If 网络地址_数据库中 Is Nothing Then
            Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
        End If
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(网络地址_文本, 网络地址) = False Then
            Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
        End If
        If 测试 = False Then
            Dim 网络地址2() As Byte = 网络地址.GetAddressBytes
            If 网络地址_数据库中.Length <> 网络地址2.Length Then
                Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
            End If
            Dim I As Integer
            For I = 0 To 网络地址2.Length - 1
                If 网络地址_数据库中(I) <> 网络地址2(I) Then Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
            Next
        End If
        If String.Compare(我方访问其它服务器的凭据, Credential) <> 0 Then
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

    Public Function 获取聊天数据的文件路径() As String
        Dim GroupID As String = Http请求("GroupID")
        If String.IsNullOrEmpty(GroupID) Then Return Nothing
        Dim FileName As String = Http请求("FileName")
        If String.IsNullOrEmpty(FileName) Then Return Nothing
        Return Context.Server.MapPath("/") & "App_Data\SS\" & GroupID & "\" & FileName
    End Function

    Public Function 获取小宇宙数据的文件路径() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return Nothing
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As String = Http请求("GroupID")
        If String.IsNullOrEmpty(GroupID) Then Return Nothing
        Dim FileName As String = Http请求("FileName")
        If String.IsNullOrEmpty(FileName) Then Return Nothing

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then Return Nothing
            Catch ex As Exception
                Return Nothing
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return Nothing
        End If
        Return Context.Server.MapPath("/") & "App_Data\MR\" & GroupID & "\" & FileName
    End Function

    Public Function 验证启动() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim Credential As String = Http请求("Credential")
        If Credential.Length <> 长度_常量集合.连接凭据_服务器 Then Return Nothing
        Dim 网络地址_文本 As String = 获取网络地址文本()
        If String.IsNullOrEmpty(网络地址_文本) Then Return Nothing
        If 启动器.验证启动(网络地址_文本, Credential) = True Then
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Public Function 获取管理员连接凭据() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        启动器.连接凭据_管理员 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端)
        Dim SS包生成器 As New 类_SS包生成器(查询结果_常量集合.成功)
        SS包生成器.添加_有标签("凭据", 启动器.连接凭据_管理员)
        Return SS包生成器
    End Function

    Private Function 获取网络地址文本() As String
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Return 网络地址_文本
    End Function

    Private Function 获取网络地址字节数组() As Byte()
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(获取网络地址文本, 网络地址) = False Then Return Nothing
        Return 网络地址.GetAddressBytes
    End Function

    Public Function 分配数据读取服务器() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        Dim 子域名_小宇宙中心服务器 As String = Nothing
        Dim 访问服务器的凭据 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 < 群角色_常量集合.成员_不可发言 Then
                    Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                End If
                子域名_小宇宙中心服务器 = 获取服务器域名(讯宝小宇宙中心服务器主机名 & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_小宇宙中心服务器, 访问服务器的凭据)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(访问服务器的凭据) Then
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_小宇宙中心服务器, 访问服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Return 访问其它服务器("https://" & 子域名_小宇宙中心服务器 & "/?C=GetAServerForRead&Credential=" & 替换URI敏感字符(访问服务器的凭据) & "&Domain=" & 启动器.本服务器主机名 & "." & 域名_英语 & "&EnglishSSAddress=" & 替换URI敏感字符(EnglishSSAddress))
    End Function

End Class
