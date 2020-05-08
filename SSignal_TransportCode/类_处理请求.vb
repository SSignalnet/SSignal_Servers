Imports System.Web
Imports System.Net
Imports System.Threading
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Public Class 类_处理请求

    Dim 应用程序 As HttpApplicationState
    Dim 跨进程锁 As Mutex
    Dim 主数据库, 副数据库 As 类_数据库
    Dim 讯宝管理器 As 类_讯宝管理器
    Dim Context As HttpContext
    Dim Http请求 As HttpRequest

    Public Sub New(ByVal 应用程序1 As HttpApplicationState, ByVal Context1 As HttpContext, ByVal Http请求1 As HttpRequest)
        应用程序 = 应用程序1
        If 应用程序 IsNot Nothing Then
            跨进程锁 = 应用程序.Get("Mu_SST")
            主数据库 = 应用程序.Get("Rb_SST")
            副数据库 = 应用程序.Get("Nb_SST")
            讯宝管理器 = 应用程序.Get("Ss_SST")
            讯宝管理器.重新计时()
        End If
        Context = Context1
        Http请求 = Http请求1
    End Sub

    Public Function 收到讯宝() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If Http请求.ContentLength = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Dim SS包解读器 As New 类_SS包解读器(字节数组)
        Dim 传送服务器子域名 As String = Nothing
        SS包解读器.读取_有标签("DM", 传送服务器子域名)
        If String.IsNullOrEmpty(传送服务器子域名) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器子域名.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 传送服务器凭据 As String = Nothing
        SS包解读器.读取_有标签("CR", 传送服务器凭据)
        If String.IsNullOrEmpty(传送服务器凭据) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器凭据.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 发送者英语讯宝地址 As String = Nothing
        SS包解读器.读取_有标签("FR", 发送者英语讯宝地址)
        If 是否是有效的讯宝或电子邮箱地址(发送者英语讯宝地址) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 发送序号 As Long
        SS包解读器.读取_有标签("ID", 发送序号)
        Dim SS包解读器2 As 类_SS包解读器 = Nothing
        Dim 群编号 As Byte
        SS包解读器.读取_有标签("GI", 群编号)
        Dim 群主英语讯宝地址 As String = Nothing
        If 群编号 > 0 Then
            SS包解读器.读取_有标签("GO", 群主英语讯宝地址)
            If 是否是有效的讯宝或电子邮箱地址(群主英语讯宝地址) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End If
        SS包解读器.读取_有标签("TO", SS包解读器2)
        If SS包解读器2 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim SS包解读器3() As Object = SS包解读器2.读取_重复标签("TA")
        If SS包解读器3 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 群编号 = 0 Then If SS包解读器3.Length <> 1 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 接收者数组(SS包解读器3.Length - 1) As 接收者_复合数据
        Dim SS包解读器4 As 类_SS包解读器 = Nothing
        Dim I As Integer
        For I = 0 To SS包解读器3.Length - 1
            SS包解读器4 = SS包解读器3(I)
            With 接收者数组(I)
                SS包解读器4.读取_有标签("AD", .讯宝地址)
                If 是否是有效的讯宝或电子邮箱地址(.讯宝地址) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                If 群编号 = 0 AndAlso String.Compare(.讯宝地址, 发送者英语讯宝地址, True) = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                SS包解读器4.读取_有标签("PO", .位置号)
                If .位置号 < 0 OrElse .位置号 >= 最大值_常量集合.传送服务器承载用户数 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            End With
        Next
        Dim 讯宝指令 As 讯宝指令_常量集合
        SS包解读器.读取_有标签("CM", 讯宝指令)
        Dim 文本 As String = Nothing
        SS包解读器.读取_有标签("TX", 文本)
        If String.IsNullOrEmpty(文本) = False Then
            If 文本.Length > 最大值_常量集合.讯宝文本长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End If
        Dim 宽度, 高度 As Short
        Dim 秒数 As Byte
        Select Case 讯宝指令
            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                SS包解读器.读取_有标签("WD", 宽度)
                If 宽度 < 1 OrElse 宽度 > 最大值_常量集合.讯宝图片宽高_像素 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                SS包解读器.读取_有标签("HT", 高度)
                If 高度 < 1 OrElse 高度 > 最大值_常量集合.讯宝图片宽高_像素 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End Select
        If 讯宝指令 = 讯宝指令_常量集合.发送语音 Then
            SS包解读器.读取_有标签("SC", 秒数)
            If 秒数 < 1 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End If
        Return 讯宝管理器.收到讯宝(Context, 传送服务器子域名, 传送服务器凭据, 发送者英语讯宝地址, 发送序号, 群主英语讯宝地址, 群编号, 接收者数组, 讯宝指令, 文本, 宽度, 高度, 秒数)
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
        If String.Compare(Domain_English, 讯宝管理器.本服务器主机名 & "." & 域名_英语) <> 0 Then
            Return New 类_SS包生成器_失败("1")
        End If
        If String.IsNullOrEmpty(Domain_Native) = False Then
            Domain_Native = Domain_Native.Trim.ToLower
            If Domain_Native.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            If String.Compare(Domain_Native, 讯宝管理器.本服务器主机名 & "." & 域名_本国语) <> 0 Then
                Return New 类_SS包生成器_失败("2")
            End If
        End If
        Dim 我方访问其它服务器的凭据 As String = Nothing
        Dim 网络地址_数据库中() As Byte = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_获取访问其它服务器的凭据(副数据库, 获取服务器域名(Domain_Ask), 我方访问其它服务器的凭据, 网络地址_数据库中)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
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
            Return New 类_SS包生成器_失败("3")
        End If
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

    Public Function 获取文件路径() As String
        Dim 位置号 As Short
        If Short.TryParse(Http请求("Position"), 位置号) = False Then Return Nothing
        If 位置号 < 0 Then Return Nothing
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If String.IsNullOrEmpty(EnglishSSAddress) Then Return Nothing
        If EnglishSSAddress.EndsWith(讯宝地址标识 & 域名_英语) = False Then Return Nothing
        Dim FileName As String = Http请求("FileName")
        If String.IsNullOrEmpty(FileName) Then Return Nothing
        Dim 用户编号 As Long = 讯宝管理器.查找用户编号(EnglishSSAddress, 位置号)
        If 用户编号 < 1 Then Return Nothing
        Return Context.Server.MapPath("/") & "App_Data\SS\" & 用户编号 & "\" & FileName
    End Function

    Public Function 添加讯友() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 讯宝管理器.用户目录 Is Nothing Then Return Nothing
        If Http请求.ContentLength = 0 Then Return Nothing
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return Nothing
        If Position < 0 OrElse Position >= 最大值_常量集合.传送服务器承载用户数 Then Return Nothing
        Dim DeviceType As String = Http请求("DeviceType")
        Dim 发送者 As 类_用户 = 讯宝管理器.用户目录(Position)
        If 发送者.停用 = True Then Return Nothing
        If UserID <> 发送者.用户编号 OrElse UserID <= 0 Then Return Nothing
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Dim SS包解读器 As 类_SS包解读器
        Dim 发送序号 As Long
        Dim 讯宝地址 As String = Nothing
        Dim 备注 As String = Nothing
        Dim 拉黑 As Boolean
        Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
        Try
            Select Case DeviceType
                Case 设备类型_手机 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_手机)
                Case 设备类型_电脑 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_电脑)
                Case Else : Return Nothing
            End Select
            SS包解读器.读取_有标签("发送序号", 发送序号)
            SS包解读器.读取_有标签("讯宝地址", 讯宝地址)
            SS包解读器.读取_有标签("备注", 备注)
            SS包解读器.读取_有标签("拉黑", 拉黑)
            If String.IsNullOrEmpty(讯宝地址) Then Return Nothing
            讯宝地址 = 讯宝地址.Trim.ToLower
            If 是否是有效的讯宝或电子邮箱地址(讯宝地址) = False Then Return Nothing
            If String.Compare(讯宝地址, 发送者.英语用户名 & 讯宝地址标识 & 域名_英语) = 0 OrElse String.Compare(讯宝地址, 发送者.本国语用户名 & 讯宝地址标识 & 域名_本国语) = 0 Then
                Return Nothing
            End If
            Dim 发送序号2 As Long
            Select Case DeviceType
                Case 设备类型_手机
                    发送序号2 = 发送者.讯宝序号_手机发送
                    If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
                Case 设备类型_电脑
                    发送序号2 = 发送者.讯宝序号_电脑发送
                    If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
            End Select
            If Math.Abs(发送序号 - 发送序号2) > 3 Then Return New 类_SS包生成器(查询结果_常量集合.发送序号不一致)
        Catch ex As Exception
            Return Nothing
        End Try
        Dim 段() As String = 讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Select Case 段(1)
            Case 域名_英语
                If String.Compare(段(0), 保留用户名_robot, True) = 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
            Case 域名_本国语
                If String.Compare(段(0), 保留用户名_机器人, True) = 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
        End Select
        Dim 子域名_中心服务器 As String = Nothing
        Dim 访问服务器的凭据 As String = Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 找到了 As Boolean
                Select Case 段(1)
                    Case 域名_英语
                        结果 = 数据库_查找讯友_添加时(发送者.用户编号, 讯宝地址, 找到了)
                    Case 域名_本国语
                        结果 = 数据库_查找讯友_添加时(发送者.用户编号, 讯宝地址, 找到了, False)
                    Case Else
                        结果 = 数据库_查找讯友_添加时(发送者.用户编号, 讯宝地址, 找到了)
                        If 找到了 = True Then Return New 类_SS包生成器(查询结果_常量集合.成功)
                        结果 = 数据库_查找讯友_添加时(发送者.用户编号, 讯宝地址, 找到了, False)
                End Select
                If 找到了 = True Then Return New 类_SS包生成器(查询结果_常量集合.成功)
                Dim 数量 As Integer
                结果 = 数据库_统计讯友(UserID, 数量)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                If 数量 >= 最大值_常量集合.讯友数量 Then
                    Return New 类_SS包生成器(查询结果_常量集合.讯友录满了)
                End If
                子域名_中心服务器 = 获取服务器域名(讯宝中心服务器主机名 & "." & 段(1))
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_中心服务器, 访问服务器的凭据)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(访问服务器的凭据) Then
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_中心服务器, 访问服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
        End If
        Dim 访问结果2 As Object = 访问其它服务器(获取路径_验证讯宝地址真实性(子域名_中心服务器, 访问服务器的凭据, 讯宝地址, 讯宝管理器.本服务器主机名, 域名_英语, 域名_本国语))
        If TypeOf 访问结果2 Is 类_SS包生成器 Then
            Return 访问结果2
        Else
            Dim SS包解读器2 As New 类_SS包解读器(CType(访问结果2, Byte()))
            If SS包解读器2.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果2
            End If
            Dim 英语讯宝地址 As String = Nothing
            SS包解读器2.读取_有标签("E", 英语讯宝地址)
            Dim 主机名 As String = Nothing
            SS包解读器2.读取_有标签("H", 主机名)
            Dim 位置号 As Short
            SS包解读器2.读取_有标签("P", 位置号)
            Dim 本国语讯宝地址 As String = Nothing
            SS包解读器2.读取_有标签("N", 本国语讯宝地址)
            If 跨进程锁.WaitOne = True Then
                Try
                    结果 = 数据库_添加新讯友(发送者.用户编号, Position, 英语讯宝地址, 本国语讯宝地址, 备注, 主机名, 位置号, 拉黑)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                    Dim 更新时间 As Long
                    结果 = 讯宝管理器.数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                    If 同步设备类型 <> 设备类型_常量集合.全部 Then
                        Dim SS包生成器 As New 类_SS包生成器()
                        SS包生成器.添加_有标签("事件", 同步事件_常量集合.添加讯友)
                        SS包生成器.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        SS包生成器.添加_有标签("备注", 备注)
                        SS包生成器.添加_有标签("主机名", 主机名)
                        SS包生成器.添加_有标签("位置号", 位置号)
                        If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                            SS包生成器.添加_有标签("本国语讯宝地址", 本国语讯宝地址)
                        End If
                        SS包生成器.添加_有标签("拉黑", 拉黑)
                        SS包生成器.添加_有标签("时间", 更新时间)
                        结果 = 讯宝管理器.数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, Position, 同步设备类型, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                    End If
                    If 结果.查询结果 = 查询结果_常量集合.成功 Then
                        结果.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        结果.添加_有标签("备注", 备注)
                        结果.添加_有标签("主机名", 主机名)
                        结果.添加_有标签("位置号", 位置号)
                        If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                            结果.添加_有标签("本国语讯宝地址", 本国语讯宝地址)
                        End If
                        结果.添加_有标签("拉黑", 拉黑)
                        结果.添加_有标签("时间", 更新时间)
                    End If
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
            Return 结果
        End If
    End Function

    Private Function 数据库_查找讯友_添加时(ByVal 用户编号 As Long, ByVal 讯宝地址 As String, ByRef 找到了 As Boolean, Optional ByVal 是英语地址 As Boolean = True) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 索引名称 As String
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            If 是英语地址 Then
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 讯宝地址)
                索引名称 = "#用户英语讯宝地址"
            Else
                列添加器.添加列_用于筛选器("本国语讯宝地址", 筛选方式_常量集合.等于, 讯宝地址)
                索引名称 = "#用户本国语讯宝地址"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, , , 索引名称)
            读取器 = 指令.执行()
            While 读取器.读取
                找到了 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_统计讯友(ByVal 用户编号 As Long, ByRef 数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("用户编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器,  , 列添加器, 100, "#用户英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加新讯友(ByVal 用户编号 As Long, ByVal 用户位置号 As Short, ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String,
                               ByVal 备注 As String, ByVal 主机名 As String, ByVal 位置号 As Short, ByVal 拉黑 As Boolean) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
            If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                列添加器.添加列_用于插入数据("本国语讯宝地址", 本国语讯宝地址)
            End If
            If String.IsNullOrEmpty(备注) = False Then
                列添加器.添加列_用于插入数据("备注", 备注)
            End If
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            列添加器.添加列_用于插入数据("拉黑", 拉黑)
            Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "讯友录", 列添加器, True)
            指令.执行()
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("变动", 讯友录变动_常量集合.添加)
            列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_更新 As New 类_数据库指令_更新数据(主数据库, "讯友录变动", 列添加器_新数据, 筛选器, "#用户地址")
            If 指令_更新.执行 = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 用户编号)
                列添加器.添加列_用于插入数据("位置号", 用户位置号)
                列添加器.添加列_用于插入数据("变动", 讯友录变动_常量集合.添加)
                列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                Dim 指令_插入 As New 类_数据库指令_插入新数据(主数据库, "讯友录变动", 列添加器)
                指令_插入.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 进入小宇宙() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 讯宝管理器.用户目录 Is Nothing Then Return Nothing
        If Http请求.ContentLength = 0 Then Return Nothing
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return Nothing
        If Position < 0 OrElse Position >= 最大值_常量集合.传送服务器承载用户数 Then Return Nothing
        Dim DeviceType As String = Http请求("DeviceType")
        Dim 发送者 As 类_用户 = 讯宝管理器.用户目录(Position)
        If 发送者.停用 = True Then Return Nothing
        If UserID <> 发送者.用户编号 OrElse UserID <= 0 Then Return Nothing
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Dim SS包解读器 As 类_SS包解读器
        Dim 发送序号 As Long
        Dim 子域名 As String = Nothing
        Try
            Select Case DeviceType
                Case 设备类型_手机 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_手机)
                Case 设备类型_电脑 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_电脑)
                Case Else : Return Nothing
            End Select
            SS包解读器.读取_有标签("发送序号", 发送序号)
            SS包解读器.读取_有标签("子域名", 子域名)
            If String.IsNullOrEmpty(子域名) Then Return Nothing
            Dim 发送序号2 As Long
            Select Case DeviceType
                Case 设备类型_手机
                    发送序号2 = 发送者.讯宝序号_手机发送
                Case 设备类型_电脑
                    发送序号2 = 发送者.讯宝序号_电脑发送
            End Select
            If Math.Abs(发送序号 - 发送序号2) > 3 Then Return New 类_SS包生成器(查询结果_常量集合.发送序号不一致)
            子域名 = 子域名.ToLower
        Catch ex As Exception
            Return Nothing
        End Try
        Dim 结果 As 类_SS包生成器
        Dim 子域名_小宇宙服务器 As String = Nothing
        Dim 访问服务器的凭据 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                子域名_小宇宙服务器 = 获取服务器域名(子域名)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_小宇宙服务器, 访问服务器的凭据)
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
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_小宇宙服务器, 访问服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim 连接凭据 As String = Nothing
        Dim 是商品编辑 As Boolean
        Dim 访问结果 As Object = 访问其它服务器(获取路径_获取小宇宙服务器连接凭据(子域名_小宇宙服务器, 访问服务器的凭据, 讯宝管理器.本服务器主机名, 发送者.英语用户名, DeviceType, 域名_英语))
        If TypeOf 访问结果 Is 类_SS包生成器 Then
            Return 访问结果
        Else
            SS包解读器 = New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果
            End If
            SS包解读器.读取_有标签("C", 连接凭据)
            SS包解读器.读取_有标签("G", 是商品编辑)
        End If
        结果.添加_有标签("子域名", 子域名)
        结果.添加_有标签("连接凭据", 连接凭据)
        结果.添加_有标签("是商品编辑", 是商品编辑)
        Return 结果
    End Function

    Public Function 加入大聊天群() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 讯宝管理器.用户目录 Is Nothing Then Return Nothing
        If Http请求.ContentLength = 0 Then Return Nothing
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return Nothing
        If Position < 0 OrElse Position >= 最大值_常量集合.传送服务器承载用户数 Then Return Nothing
        Dim DeviceType As String = Http请求("DeviceType")
        Dim 发送者 As 类_用户 = 讯宝管理器.用户目录(Position)
        If 发送者.停用 = True Then Return Nothing
        If UserID <> 发送者.用户编号 OrElse UserID <= 0 Then Return Nothing
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Dim SS包解读器 As 类_SS包解读器
        Dim 发送序号 As Long
        Dim 子域名 As String = Nothing
        Dim 群编号 As Long
        Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
        Try
            Select Case DeviceType
                Case 设备类型_手机 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_手机)
                Case 设备类型_电脑 : SS包解读器 = New 类_SS包解读器(字节数组, 发送者.AES解密器_电脑)
                Case Else : Return Nothing
            End Select
            SS包解读器.读取_有标签("发送序号", 发送序号)
            SS包解读器.读取_有标签("子域名", 子域名)
            SS包解读器.读取_有标签("群编号", 群编号)
            If String.IsNullOrEmpty(子域名) Then Return Nothing
            If 群编号 < 1 Then Return Nothing
            Dim 发送序号2 As Long
            Select Case DeviceType
                Case 设备类型_手机
                    发送序号2 = 发送者.讯宝序号_手机发送
                    If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
                Case 设备类型_电脑
                    发送序号2 = 发送者.讯宝序号_电脑发送
                    If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
            End Select
            If Math.Abs(发送序号 - 发送序号2) > 3 Then Return New 类_SS包生成器(查询结果_常量集合.发送序号不一致)
            子域名 = 子域名.ToLower
        Catch ex As Exception
            Return Nothing
        End Try
        Dim 结果 As 类_SS包生成器
        Dim 子域名_大聊天群服务器 As String = Nothing
        Dim 访问服务器的凭据 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                子域名_大聊天群服务器 = 获取服务器域名(子域名)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_大聊天群服务器, 访问服务器的凭据)
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
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_大聊天群服务器, 访问服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim 群名称 As String = Nothing
        Dim 图标更新时间 As Long
        Dim 连接凭据 As String = Nothing
        Dim 角色 As 群角色_常量集合
        Dim 本国语域名 As String = Nothing
        Dim 访问结果 As Object = 访问其它服务器(获取路径_获取大聊天群服务器连接凭据(子域名_大聊天群服务器, 访问服务器的凭据, 讯宝管理器.本服务器主机名, 发送者.英语用户名, DeviceType, 群编号, 域名_英语))
        If TypeOf 访问结果 Is 类_SS包生成器 Then
            Return 访问结果
        Else
            SS包解读器 = New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果
            End If
            SS包解读器.读取_有标签("N", 群名称)
            SS包解读器.读取_有标签("V", 图标更新时间)
            SS包解读器.读取_有标签("C", 连接凭据)
            SS包解读器.读取_有标签("R", 角色)
            SS包解读器.读取_有标签("D", 本国语域名)
        End If
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_加入大聊天群(UserID, Position, 子域名, 本国语域名, 群编号, 群名称, 发送者, 同步设备类型)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If 结果.查询结果 = 查询结果_常量集合.成功 Then
            结果.添加_有标签("子域名", 子域名)
            结果.添加_有标签("群编号", 群编号)
            结果.添加_有标签("群名称", 群名称)
            结果.添加_有标签("图标更新时间", 图标更新时间)
            结果.添加_有标签("连接凭据", 连接凭据)
            结果.添加_有标签("角色", 角色)
            结果.添加_有标签("本国语域名", 本国语域名)
        End If
        Return 结果
    End Function

    Private Function 数据库_加入大聊天群(ByVal 用户编号 As Long, ByVal 位置号 As Short, ByVal 子域名 As String, ByVal 本国语域名 As String,
                                ByVal 群编号 As Long, ByVal 群名称 As String, ByVal 发送者 As 类_用户, ByVal 同步设备类型 As 设备类型_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim I As Integer = 子域名.IndexOf(".")
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 子域名.Substring(I + 1))
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 子域名.Substring(0, I))
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("群名称")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的大聊天群", 筛选器, 1, 列添加器,  , "#用户域名主机名群")
            Dim 群名称2 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                群名称2 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(群名称2) = False Then
                If String.Compare(群名称2, 群名称) <> 0 Then
                    Dim 列添加器_新数据 As New 类_列添加器
                    列添加器_新数据.添加列_用于插入数据("群名称", 群名称)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
                    列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 子域名.Substring(I + 1))
                    列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 子域名.Substring(0, I))
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "加入的大聊天群", 列添加器_新数据, 筛选器, "#用户域名主机名群")
                    指令2.执行()
                End If
            Else
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 用户编号)
                列添加器.添加列_用于插入数据("位置号", 位置号)
                列添加器.添加列_用于插入数据("主机名", 子域名.Substring(0, I))
                列添加器.添加列_用于插入数据("英语域名", 子域名.Substring(I + 1))
                If String.IsNullOrEmpty(本国语域名) = False Then
                    列添加器.添加列_用于插入数据("本国语域名", 本国语域名)
                End If
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("群名称", 群名称)
                列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
                Dim 指令3 As New 类_数据库指令_插入新数据(主数据库, "加入的大聊天群", 列添加器)
                指令3.执行()
                If 同步设备类型 <> 设备类型_常量集合.全部 Then
                    Dim SS包生成器 As New 类_SS包生成器()
                    SS包生成器.添加_有标签("事件", 同步事件_常量集合.加入大聊天群)
                    SS包生成器.添加_有标签("主机名", 子域名.Substring(0, I))
                    SS包生成器.添加_有标签("英语域名", 子域名.Substring(I + 1))
                    If String.IsNullOrEmpty(本国语域名) = False Then
                        SS包生成器.添加_有标签("本国语域名", 本国语域名)
                    End If
                    SS包生成器.添加_有标签("群编号", 群编号)
                    SS包生成器.添加_有标签("群名称", 群名称)
                    Dim 结果 As 类_SS包生成器 = 讯宝管理器.数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 用户编号, 位置号, 同步设备类型, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 用户上线或离线() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 讯宝管理器.验证中心服务器(获取网络地址(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return Nothing
        Dim DeviceType As String = Http请求("DeviceType")
        If UserID < 1 OrElse Position < 0 Then Return Nothing
        Dim 字节数组() As Byte
        If Http请求.ContentLength > 0 Then
            ReDim 字节数组(Http请求.ContentLength - 1)
            Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Else
            字节数组 = Nothing
        End If
        Return 讯宝管理器.用户上线或离线(UserID, Position, 字节数组, DeviceType)
    End Function

    Public Function 验证启动() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim Credential As String = Http请求("Credential")
        If Credential.Length <> 长度_常量集合.连接凭据_服务器 Then Return Nothing
        Dim 网络地址_文本 As String = 获取网络地址()
        If String.IsNullOrEmpty(网络地址_文本) Then Return Nothing
        If 讯宝管理器.验证启动(网络地址_文本, Credential) = True Then
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Public Function 获取管理员连接凭据() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 讯宝管理器.验证中心服务器(获取网络地址(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        讯宝管理器.连接凭据_管理员 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端)
        Dim SS包生成器 As New 类_SS包生成器(查询结果_常量集合.成功)
        SS包生成器.添加_有标签("凭据", 讯宝管理器.连接凭据_管理员)
        Return SS包生成器
    End Function

    Public Function 获取服务器信息() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return ""
        If 讯宝管理器.验证中心服务器(获取网络地址(), Http请求("Credential")) = False Then Return Nothing
        Select Case Http请求("InfoType")
            Case "statistics" : Return 讯宝管理器.获取统计数据
            Case "users" : Return 讯宝管理器.获取在线用户详情
            Case "chatgroups" : Return 讯宝管理器.获取聊天群详情
            Case "ssnumber" : Return 讯宝管理器.获取讯宝数量
            Case Else : Return Nothing
        End Select
    End Function

    Private Function 获取网络地址() As String
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Return 网络地址_文本
    End Function

End Class
