Imports System.Text.Encoding
Imports System.Security.Cryptography
Imports System.Net
Imports System.Threading
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

#Region "登录"

    Public Function 登录() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim SSAddress As String = Http请求("SSAddress")
        Dim Password As String = Http请求("Password")
        Dim DeviceType As String = Http请求("DeviceType")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")

        SSAddress = SSAddress.ToLower
        If 是否是有效的讯宝或电子邮箱地址(SSAddress) = False Then Return Nothing
        If SSAddress.EndsWith(讯宝地址标识 & 域名_英语) = False Then
            If String.IsNullOrEmpty(域名_本国语) = False Then
                If SSAddress.EndsWith(讯宝地址标识 & 域名_本国语) = False Then
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End If
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        Dim 验证码添加时间 As Long
        If String.IsNullOrEmpty(VerificationCode) = False Then
            If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
            If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        End If
        Select Case DeviceType
            Case 设备类型_手机, 设备类型_电脑
            Case Else : Return Nothing
        End Select
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        Dim 用户信息 As 类_用户信息
        Dim 连接凭据_中心服务器 As String = Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.全部)
                Dim 段() As String = SSAddress.Split(讯宝地址标识)
                结果 = 数据库_查找用户(段(0), True, 用户信息)
                If 结果.查询结果 = 查询结果_常量集合.不正确 Then
                    If String.IsNullOrEmpty(域名_本国语) = False Then
                        If 段(0).Length > 最大值_常量集合.本国语用户名长度 Then
                            Return 结果
                        Else
                            结果 = 数据库_查找用户(段(0), False, 用户信息)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                        End If
                    Else
                        Return 结果
                    End If
                ElseIf 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(VerificationCode) Then
                    Dim 登录错误次数 As Byte
                    结果 = 数据库_获取登录错误次数(用户信息.用户编号, 登录错误次数, DeviceType)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    If 登录错误次数 >= 3 Then
                        Return 获取验证码图片2()
                    End If
                Else
                    结果 = 数据库_检验验证码(验证码添加时间, VerificationCode)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
                Dim 哈希值计算器 As New SHA1Managed
                Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(Password))
                哈希值计算器.Dispose()
                结果 = 数据库_验证密码(用户信息.用户编号, 密码哈希值, DeviceType)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_保存登录信息(获取网络地址字节数组, 用户信息.用户编号, 连接凭据_中心服务器, DeviceType)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_获取登录信息(用户信息)
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
        结果.添加_有标签("用户编号", 用户信息.用户编号)
        结果.添加_有标签("连接凭据", 连接凭据_中心服务器)
        添加用户信息(结果, 用户信息, 时区偏移量)
        Return 结果
    End Function

    Private Function 数据库_查找用户(ByVal 用户名 As String, ByVal 是英语用户名 As Boolean, ByVal 用户信息 As 类_用户信息) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 索引名称 As String
            Dim 列添加器 As New 类_列添加器
            If 是英语用户名 = True Then
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 用户名)
                索引名称 = "#英语用户名"
            Else
                列添加器.添加列_用于筛选器("本国语用户名", 筛选方式_常量集合.等于, 用户名)
                索引名称 = "#本国语用户名"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If String.IsNullOrEmpty(域名_本国语) Then
                列添加器.添加列_用于获取数据(New String() {"停用", "编号", "英语用户名", "手机号", "电子邮箱地址", "职能"})
            Else
                列添加器.添加列_用于获取数据(New String() {"停用", "编号", "英语用户名", "手机号", "电子邮箱地址", "职能", "本国语用户名"})
            End If
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 索引名称)
            Dim 停用 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                停用 = 读取器(0)
                If 停用 = False Then
                    用户信息.用户编号 = 读取器(1)
                    用户信息.英语用户名 = 读取器(2)
                    用户信息.手机号 = 读取器(3)
                    用户信息.电子邮箱地址 = 读取器(4)
                    用户信息.职能 = 读取器(5)
                    If String.IsNullOrEmpty(域名_本国语) = False Then
                        用户信息.本国语用户名 = 读取器(6)
                    End If
                End If
                Exit While
            End While
            读取器.关闭()
            If 停用 Then
                Return New 类_SS包生成器(查询结果_常量集合.账号停用)
            End If
            If 用户信息.用户编号 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.不正确)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取登录错误次数(ByVal 用户编号 As Long, ByRef 登录错误次数 As Byte, ByVal 设备类型 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If String.Compare(设备类型, 设备类型_手机) = 0 Then
                列添加器.添加列_用于获取数据("登录错误次数_手机")
            Else
                列添加器.添加列_用于获取数据("登录错误次数_电脑")
            End If
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                登录错误次数 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存登录信息(ByVal 网络地址() As Byte, ByVal 用户编号 As Long, ByRef 连接凭据 As String, ByVal 设备类型 As String) As 类_SS包生成器
        Try
            连接凭据 = 设备类型 & 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端 - 设备类型.Length)
            Dim 列添加器_新数据 As New 类_列添加器
            If String.Compare(设备类型, 设备类型_手机) = 0 Then
                列添加器_新数据.添加列_用于插入数据("连接凭据_手机", 连接凭据)
                列添加器_新数据.添加列_用于插入数据("登录时间_手机", Date.UtcNow.Ticks)
                列添加器_新数据.添加列_用于插入数据("网络地址_手机", 网络地址)
                列添加器_新数据.添加列_用于插入数据("登录错误次数_手机", 0)
            Else
                列添加器_新数据.添加列_用于插入数据("连接凭据_电脑", 连接凭据)
                列添加器_新数据.添加列_用于插入数据("登录时间_电脑", Date.UtcNow.Ticks)
                列添加器_新数据.添加列_用于插入数据("网络地址_电脑", 网络地址)
                列添加器_新数据.添加列_用于插入数据("登录错误次数_电脑", 0)
            End If
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令2.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 用户编号)
                If String.Compare(设备类型, 设备类型_手机) = 0 Then
                    列添加器.添加列_用于插入数据("连接凭据_手机", 连接凭据)
                    列添加器.添加列_用于插入数据("网络地址_手机", 网络地址)
                Else
                    列添加器.添加列_用于插入数据("连接凭据_电脑", 连接凭据)
                    列添加器.添加列_用于插入数据("网络地址_电脑", 网络地址)
                End If
                列添加器.添加列_用于插入数据("登录时间_电脑", Date.UtcNow.Ticks)
                列添加器.添加列_用于插入数据("登录时间_手机", Date.UtcNow.Ticks)
                列添加器.添加列_用于插入数据("登录错误次数_电脑", 0)
                列添加器.添加列_用于插入数据("登录错误次数_手机", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
                指令3.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 获取网络地址字节数组() As Byte()
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(网络地址_文本, 网络地址) = False Then Return Nothing
        Return 网络地址.GetAddressBytes
    End Function

    Public Function 获取密钥() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")

        If UserID < 1 Then Return Nothing
        Dim 用户信息 As 类_用户信息
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.用户名和主机名)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.英语用户名) Then Return Nothing
                子域名 = 获取服务器域名(用户信息.主机名 & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名, 服务器连接凭据)
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
        If String.IsNullOrEmpty(服务器连接凭据) Then
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名, 服务器连接凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim AES加解密模块 As New RijndaelManaged
        AES加解密模块.GenerateKey()
        AES加解密模块.GenerateIV()
        Dim 密钥创建时间 As Long = Date.UtcNow.Ticks
        Dim SS包生成器2 As New 类_SS包生成器()
        With SS包生成器2
            .添加_有标签("英语用户名", 用户信息.英语用户名)
            If String.IsNullOrEmpty(域名_本国语) = False Then
                .添加_有标签("本国语用户名", 用户信息.本国语用户名)
            End If
            .添加_有标签("对称密钥", AES加解密模块.Key)
            .添加_有标签("初始向量", AES加解密模块.IV)
            .添加_有标签("时间", 密钥创建时间)
        End With
        Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名 & "/?C=UserOnOrOff&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&UserID=" & UserID & "&Position=" & 用户信息.位置号 & "&DeviceType=" & Credential.Substring(0, 设备类型_手机.Length), SS包生成器2.生成SS包)
        If TypeOf 访问结果 Is 类_SS包生成器 Then
            Return 访问结果
        Else
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果
            End If
        End If
        结果.添加_有标签("主机名", 用户信息.主机名)
        结果.添加_有标签("位置号", 用户信息.位置号)
        结果.添加_有标签("对称密钥", AES加解密模块.Key)
        结果.添加_有标签("初始向量", AES加解密模块.IV)
        结果.添加_有标签("时间", 密钥创建时间)
        Return 结果
    End Function

    Public Function 管理员登录() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Password As String = Http请求("Password")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Password) = True Then Return Nothing
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return XML无权操作
                ElseIf 用户信息.职能.Contains(职能_管理员) = False AndAlso 用户信息.职能.Contains(职能_副管理员) = False Then
                    Return XML无权操作
                End If
                Dim 哈希值计算器 As New SHA1Managed
                Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(Password))
                哈希值计算器.Dispose()
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML不正确
                    End If
                End If
                Return "<SUCCEED><PASSCODE>" & 替换XML敏感字符(System.Convert.ToBase64String(密码哈希值)) & "</PASSCODE></SUCCEED>"
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

#End Region

#Region "注销"

    Public Function 注销() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")

        If UserID < 1 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 用户信息 As 类_用户信息
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.主机名)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.凭据无效 Then
                        Return New 类_SS包生成器(查询结果_常量集合.成功)
                    Else
                        Return 结果
                    End If
                End If
                If String.IsNullOrEmpty(用户信息.主机名) = False Then
                    子域名 = 获取服务器域名(用户信息.主机名 & "." & 域名_英语）
                    结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名, 服务器连接凭据)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(服务器连接凭据) = False Then
            Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名 & "/?C=UserOnOrOff&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&UserID=" & UserID & "&Position=" & 用户信息.位置号 & "&DeviceType=" & Credential.Substring(0, 设备类型_手机.Length))
            If TypeOf 访问结果 Is 类_SS包生成器 Then
                Return 访问结果
            Else
                Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
                If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 访问结果
                End If
            End If
        End If
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_清除连接凭据(UserID, Credential)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_清除连接凭据(ByVal 用户编号 As Long, Optional ByVal 连接凭据 As String = Nothing) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            If String.IsNullOrEmpty(连接凭据) = False Then
                If 连接凭据.StartsWith(设备类型_手机) Then
                    列添加器_新数据.添加列_用于插入数据("连接凭据_手机", Nothing)
                Else
                    列添加器_新数据.添加列_用于插入数据("连接凭据_电脑", Nothing)
                End If
            Else
                列添加器_新数据.添加列_用于插入数据("连接凭据_电脑", Nothing)
                列添加器_新数据.添加列_用于插入数据("连接凭据_手机", Nothing)
            End If
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 用户编号)
                列添加器.添加列_用于插入数据("登录时间_电脑", 0)
                列添加器.添加列_用于插入数据("登录时间_手机", 0)
                列添加器.添加列_用于插入数据("登录错误次数_电脑", 0)
                列添加器.添加列_用于插入数据("登录错误次数_手机", 0)
                Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

#End Region

#Region "注册"

    Public Function 注册() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim IDtype As String = Http请求("IDtype")
        Dim ID As String = Http请求("ID")
        Dim Password As String = Http请求("Password")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")
        Dim LanguageCode As String = Http请求("LanguageCode")

        Dim 手机号 As Long
        Select Case IDtype
            Case 身份码类型_手机号
                If Long.TryParse(ID, 手机号) = False Then Return Nothing
                If 手机号 <= 0 Then Return Nothing
            Case 身份码类型_电子邮箱地址
                ID = ID.Trim.ToLower
                If 是否是有效的讯宝或电子邮箱地址(ID) = False Then Return Nothing
            Case Else
                Return Nothing
        End Select

        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        Dim 结果 As 类_SS包生成器 = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                If File.Exists(Context.Server.MapPath("/") & "App_Data\" & 文件名_注册许可) = False Then
                    If String.Compare(ID, 管理员电子邮箱地址) <> 0 Then
                        Dim 有 As Boolean = False
                        结果 = 数据库_是否有注册许可(ID, 有)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 有 = False Then
                            Return New 类_SS包生成器(查询结果_常量集合.无注册许可)
                        End If
                    End If
                End If
                Dim 验证码 As String = Nothing
                Select Case IDtype
                    Case 身份码类型_手机号
                        Dim 编号 As Long
                        结果 = 数据库_手机号是否已绑定(手机号, 编号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 编号 > 0 Then
                            Return New 类_SS包生成器(查询结果_常量集合.手机号已绑定)
                        Else
                            结果 = 数据库_检验验证码(验证码添加时间, VerificationCode)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_添加验证码(验证码添加时间, 验证码, 手机号, True)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_记录正在注册的用户(ID, Password, 验证码添加时间, 验证码)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_保存短信(手机号, 生成验证码短信(验证码, LanguageCode))
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                        End If
                    Case 身份码类型_电子邮箱地址
                        Dim 编号 As Long
                        结果 = 数据库_电子邮箱地址是否已绑定(ID, 编号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 编号 > 0 Then
                            Return New 类_SS包生成器(查询结果_常量集合.电子邮箱地址已绑定)
                        Else
                            结果 = 数据库_检验验证码(验证码添加时间, VerificationCode)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_添加验证码(验证码添加时间, 验证码, ID)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_记录正在注册的用户(ID, Password, 验证码添加时间, 验证码)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_保存邮件(生成验证码邮件(ID, 验证码, 时区偏移量, LanguageCode))
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                        End If
                End Select
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        结果.添加_有标签("验证码添加时间", 验证码添加时间)
        Select Case IDtype
            Case 身份码类型_手机号
                短信管理器.发送短信()
            Case 身份码类型_电子邮箱地址
                邮件管理器.发送邮件()
        End Select
        Return 结果
    End Function

    Private Function 数据库_是否有注册许可(ByVal ID As String, ByRef 有 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddDays(-3).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "可注册者", 筛选器, "#时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("邮箱或手机号", 筛选方式_常量集合.等于, ID)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "可注册者", 筛选器, 1, , , 主键索引名)
            读取器 = 指令2.执行()
            While 读取器.读取
                有 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_根据本国语用户名查找用户(ByVal 本国语用户名 As String, ByRef 编号 As Long,
                                                                                 Optional ByRef 停用 As Boolean = False,
                                                                                 Optional ByRef 英语用户名 As String = Nothing,
                                                                                 Optional ByRef 主机名 As String = Nothing,
                                                                                 Optional ByRef 位置号 As Short = 0) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("本国语用户名", 筛选方式_常量集合.等于, 本国语用户名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"编号", "停用", "主机名", "位置号", "英语用户名"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , "#本国语用户名")
            读取器 = 指令.执行()
            While 读取器.读取
                编号 = 读取器(0)
                停用 = 读取器(1)
                主机名 = 读取器(2)
                位置号 = 读取器(3)
                英语用户名 = 读取器(4)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_根据英语用户名查找用户(ByVal 英语用户名 As String, ByRef 编号 As Long,
                                                                                 Optional ByRef 停用 As Boolean = False,
                                                                                 Optional ByRef 本国语用户名 As String = Nothing,
                                                                                 Optional ByRef 主机名 As String = Nothing,
                                                                                 Optional ByRef 位置号 As Short = 0) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 列名数组() As String
            If String.IsNullOrEmpty(域名_本国语) Then
                列名数组 = New String() {"编号", "停用", "主机名", "位置号"}
            Else
                列名数组 = New String() {"编号", "停用", "主机名", "位置号", "本国语用户名"}
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(列名数组)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , "#英语用户名")
            读取器 = 指令.执行()
            While 读取器.读取
                编号 = 读取器(0)
                停用 = 读取器(1)
                主机名 = 读取器(2)
                位置号 = 读取器(3)
                If String.IsNullOrEmpty(域名_本国语) = False Then
                    本国语用户名 = 读取器(4)
                End If
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_手机号是否已绑定(ByVal 手机号 As Long, ByRef 编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("手机号", 筛选方式_常量集合.等于, 手机号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , "#手机号")
            读取器 = 指令.执行()
            While 读取器.读取
                编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_电子邮箱地址是否已绑定(ByVal 电子邮箱地址 As String, ByRef 编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("电子邮箱地址", 筛选方式_常量集合.等于, 电子邮箱地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , "#电子邮箱地址")
            读取器 = 指令.执行()
            While 读取器.读取
                编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_记录正在注册的用户(ByVal 邮箱或手机号 As String, ByVal 密码 As String, ByVal 时间 As Long, ByVal 验证码 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddMinutes(-30).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "正在注册的用户", 筛选器, "#时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", 时间)
            列添加器.添加列_用于插入数据("验证码", 验证码)
            列添加器.添加列_用于插入数据("邮箱或手机号", 邮箱或手机号)
            Dim 哈希值计算器 As New SHA1Managed
            列添加器.添加列_用于插入数据("密码哈希值", 哈希值计算器.ComputeHash(UTF8.GetBytes(密码)))
            列添加器.添加列_用于插入数据("验证通过", False)
            哈希值计算器.Dispose()
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "正在注册的用户", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 生成数字的随机字符串(ByVal 长度 As Short) As String
        If 长度 < 1 Then 长度 = 1
        Dim 字节数组(长度 - 1) As Byte
        Dim I As Short
        Randomize()
        For I = 0 To 长度 - 1
            字节数组(I) = Int(10 * Rnd()) + 48
        Next
        Return ASCII.GetString(字节数组)
    End Function

    Private Function 生成验证码短信(ByVal 验证码 As String, ByVal 语种 As String) As String
        Dim 界面文字 As 类_界面文字 = 获取界面文字(语种)
        Return 界面文字.获取(1, "SSignal verification code: #%", New Object() {验证码})
    End Function

    Private Function 生成验证码邮件(ByVal 电子邮箱地址 As String, ByVal 验证码 As String,
                                                       ByVal 时区偏移量 As Integer, ByVal 语种 As String) As 类_邮件
        Dim 界面文字 As 类_界面文字 = 获取界面文字(语种)
        Dim 邮件 As New 类_邮件
        邮件.收件人 = 电子邮箱地址
        邮件.标题 = 界面文字.获取(0, "Please verify your email address")
        邮件.正文 = 界面文字.获取(1, "Verification code: #%", New Object() {验证码}) & vbCrLf &
                    界面文字.获取(2, "Sent at: #%", New Object() {Date.UtcNow.AddMinutes(时区偏移量).ToString}) & vbCrLf &
                    界面文字.获取(3, "Sent by: the system (Please don't reply this email)") & vbCrLf &
                    网站链接
        Return 邮件
    End Function

#End Region

#Region "验证"

    Public Function 验证手机号或电子邮箱地址() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim IDtype As String = Http请求("IDtype")
        Dim ID As String = Http请求("ID")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")

        Dim 手机号 As Long
        Select Case IDtype
            Case 身份码类型_手机号
                If Long.TryParse(ID, 手机号) = False Then Return Nothing
                If 手机号 <= 0 Then Return Nothing
            Case 身份码类型_电子邮箱地址
                ID = ID.Trim.ToLower
                If 是否是有效的讯宝或电子邮箱地址(ID) = False Then Return Nothing
            Case Else
                Return Nothing
        End Select
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Select Case IDtype
                    Case 身份码类型_手机号
                        结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, 手机号, True)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        结果 = 数据库_验证通过(验证码添加时间, VerificationCode, 手机号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                    Case 身份码类型_电子邮箱地址
                        结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, ID, True)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        结果 = 数据库_验证通过(验证码添加时间, VerificationCode, ID)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                    Case Else
                        Return Nothing
                End Select
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        结果.添加_有标签("英语域名", 域名_英语)
        If String.IsNullOrEmpty(域名_本国语) = False Then
            结果.添加_有标签("本国语域名", 域名_本国语)
        End If
        Return 结果
    End Function

    Private Function 数据库_验证通过(ByVal 时间 As Long, ByVal 验证码 As String, ByVal 邮箱或手机号 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("验证通过", True)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 时间)
            列添加器.添加列_用于筛选器("验证码", 筛选方式_常量集合.等于, 验证码)
            列添加器.添加列_用于筛选器("邮箱或手机号", 筛选方式_常量集合.等于, 邮箱或手机号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "正在注册的用户", 列添加器_新数据, 筛选器, "#时间验证码")
            If 指令.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

#End Region

#Region "创建新账号"

    Public Function 设置用户名() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim IDtype As String = Http请求("IDtype")
        Dim ID As String = Http请求("ID")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim Password As String = Http请求("Password")
        Dim English As String = Http请求("English")
        English = English.ToLower
        If 是否是英文用户名(English) = False Then Return Nothing
        If String.Compare(English, 保留用户名_robot, True) = 0 Then
            Return New 类_SS包生成器(查询结果_常量集合.英语用户名已注册)
        End If
        Dim Native As String = Nothing
        If String.IsNullOrEmpty(域名_本国语) = False Then
            Native = Http请求("Native")
            Native = Native.ToLower
            If 是否是中文用户名(Native) = False Then Return Nothing
            If String.Compare(Native, 保留用户名_机器人, True) = 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.本国语用户名已注册)
            End If
        End If

        Dim 手机号 As Long
        Select Case IDtype
            Case 身份码类型_手机号
                If Long.TryParse(ID, 手机号) = False Then Return Nothing
                If 手机号 <= 0 Then Return Nothing
            Case 身份码类型_电子邮箱地址
                ID = ID.Trim.ToLower
                If 是否是有效的讯宝或电子邮箱地址(ID) = False Then Return Nothing
            Case Else
                Return Nothing
        End Select
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 哈希值计算器 As New SHA1Managed
                Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(Password))
                哈希值计算器.Dispose()
                Dim 可以 As Boolean
                结果 = 数据库_是否可添加新账号(验证码添加时间, VerificationCode, ID, 密码哈希值, 可以)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 可以 = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
                Dim 编号 As Long
                结果 = 数据库_根据英语用户名查找用户(English, 编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 编号 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.英语用户名已注册)
                End If
                If String.IsNullOrEmpty(域名_本国语) = False Then
                    结果 = 数据库_根据本国语用户名查找用户(Native, 编号)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    If 编号 > 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.本国语用户名已注册)
                    End If
                End If
                Dim 主机名 As String = Nothing
                结果 = 数据库_获取可用的传送服务器(主机名)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 位置号 As Short
                If String.IsNullOrEmpty(主机名) Then
                    If String.Compare(ID, 管理员电子邮箱地址) <> 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.没有可用的传送服务器)
                    Else
                        GoTo 跳转点1
                    End If
                End If
                位置号 = -1
                结果 = 数据库_获取可用的位置号(主机名, 位置号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 位置号 < 0 Then
                    'Return New 类_SS包生成器(查询结果_常量集合.传送服务器上没有空位置)
                    '请取消注释上一行代码，直接Return


                    If String.Compare(域名_英语, "ssignal.net") <> 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.传送服务器上没有空位置)
                    Else
                        结果 = 数据库_获取可用的位置号_讯宝网络体验程序(主机名, 位置号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 位置号 < 0 Then
                            Return New 类_SS包生成器(查询结果_常量集合.传送服务器上没有空位置)
                        End If
                    End If


                End If
跳转点1:
                结果 = 数据库_添加新用户(English, Native, 密码哈希值, 主机名, 位置号, IDtype, ID)
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
        结果.添加_有标签("英语用户名", English)
        If String.IsNullOrEmpty(Native) = False Then
            结果.添加_有标签("本国语用户名", Native)
        End If
        Return 结果
    End Function

    Private Function 数据库_是否可添加新账号(ByVal 时间 As Long, ByVal 验证码 As String, ByVal 邮箱或手机号 As String,
                                     ByVal 密码哈希值() As Byte, ByRef 可以 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 时间)
            列添加器.添加列_用于筛选器("验证码", 筛选方式_常量集合.等于, 验证码)
            列添加器.添加列_用于筛选器("邮箱或手机号", 筛选方式_常量集合.等于, 邮箱或手机号)
            列添加器.添加列_用于筛选器("密码哈希值", 筛选方式_常量集合.等于, 密码哈希值)
            列添加器.添加列_用于筛选器("验证通过", 筛选方式_常量集合.等于, True)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "正在注册的用户", 筛选器, 1, , , "#时间验证码")
            读取器 = 指令.执行()
            While 读取器.读取
                可以 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取可用的传送服务器(ByRef 主机名 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("类别", 筛选方式_常量集合.等于, 服务器类别_常量集合.传送服务器)
            列添加器.添加列_用于筛选器("停用", 筛选方式_常量集合.等于, False)
            列添加器.添加列_用于筛选器("统计", 筛选方式_常量集合.小于, 最大值_常量集合.传送服务器承载用户数)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("主机名")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "服务器", 筛选器, 1, 列添加器, , "#类别时间")
            读取器 = 指令.执行()
            While 读取器.读取
                主机名 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取可用的位置号(ByVal 主机名 As String, ByRef 位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("位置号")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器,  , 列添加器, 100, "#主机名位置号")
            Dim 位置号2 As Short = -1
            Dim 位置号2_之前 As Short = -1
            读取器 = 指令.执行()
            While 读取器.读取
                位置号2 = 读取器(0)
                If 位置号2 - 位置号2_之前 > 1 Then
                    位置号 = 位置号2_之前 + 1
                    Exit While
                ElseIf 位置号2 >= 0 Then
                    位置号2_之前 = 位置号2
                End If
            End While
            If 位置号 < 0 Then
                If 位置号2 < 0 Then
                    位置号 = 0
                Else
                    If 位置号2 < 最大值_常量集合.传送服务器承载用户数 - 1 Then
                        位置号 = 位置号2 + 1
                    End If
                End If
            End If
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加新用户(ByVal 英语用户名 As String, ByVal 本国语用户名 As String, ByVal 密码哈希值() As Byte,
                               ByVal 主机名 As String, ByVal 位置号 As Short, ByVal ID类型 As String, ByVal ID As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 次数 As Integer
        Dim 编号 As Long
跳转点1:
        Try
            编号 = Date.UtcNow.Ticks
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 编号)
            列添加器.添加列_用于插入数据("停用", False)
            列添加器.添加列_用于插入数据("英语用户名", 英语用户名)
            If String.IsNullOrEmpty(域名_本国语) = False Then
                列添加器.添加列_用于插入数据("本国语用户名", 本国语用户名)
            End If
            列添加器.添加列_用于插入数据("密码哈希值", 密码哈希值)
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            Select Case ID类型
                Case 身份码类型_手机号
                    列添加器.添加列_用于插入数据("手机号", Long.Parse(ID))
                Case 身份码类型_电子邮箱地址
                    列添加器.添加列_用于插入数据("手机号", 0)
                    列添加器.添加列_用于插入数据("电子邮箱地址", ID)
                    If String.Compare(ID, 管理员电子邮箱地址) = 0 Then
                        列添加器.添加列_用于插入数据("职能", 职能_管理员)
                    End If
            End Select
            Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "用户", 列添加器)
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 编号)
            列添加器.添加列_用于插入数据("登录时间_电脑", 0)
            列添加器.添加列_用于插入数据("登录时间_手机", 0)
            Dim 网络地址() As Byte = 获取网络地址字节数组()
            列添加器.添加列_用于插入数据("网络地址_电脑", 网络地址)
            列添加器.添加列_用于插入数据("网络地址_手机", 网络地址)
            列添加器.添加列_用于插入数据("登录错误次数_电脑", 0)
            列添加器.添加列_用于插入数据("登录错误次数_手机", 0)
            指令 = New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
            指令.执行()
            If String.Compare(ID, 管理员电子邮箱地址) <> 0 Then
                Dim 运算器 As New 类_运算器("统计")
                运算器.添加运算指令(运算符_常量集合.加, 1)
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("统计", 运算器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
                If 指令2.执行 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Else
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            End If
        Catch ex As 类_值已存在
            Thread.Sleep(10)
            次数 += 1
            If 次数 < 10 Then
                GoTo 跳转点1
            Else '
                Return New 类_SS包生成器(ex.Message)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

#End Region

#Region "找回"

    Public Function 忘记密码了() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim SSAddress As String = Http请求("SSAddress")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")
        Dim LanguageCode As String = Http请求("LanguageCode")

        SSAddress = SSAddress.ToLower
        If 是否是有效的讯宝或电子邮箱地址(SSAddress) = False Then Return Nothing
        If SSAddress.EndsWith(讯宝地址标识 & 域名_英语) = False Then
            If String.IsNullOrEmpty(域名_本国语) = False Then
                If SSAddress.EndsWith(讯宝地址标识 & 域名_本国语) = False Then
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End If
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        Dim 验证码 As String = Nothing
        Dim 用户信息 As 类_用户信息
        If 跨进程锁.WaitOne = True Then
            Dim 结果 As 类_SS包生成器
            Try
                结果 = 数据库_检验验证码(验证码添加时间, VerificationCode)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.全部)
                Dim 段() As String = SSAddress.Split(讯宝地址标识)
                结果 = 数据库_获取用户手机号和邮箱地址(段(0), True, 用户信息)
                If 结果.查询结果 = 查询结果_常量集合.不正确 Then
                    If String.IsNullOrEmpty(域名_本国语) = False Then
                        结果 = 数据库_获取用户手机号和邮箱地址(段(0), False, 用户信息)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                    Else
                        Return 结果
                    End If
                ElseIf 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 用户信息.手机号 > 0 Then
                    Dim 添加了 As Boolean
                    结果 = 数据库_最近是否添加了验证码(用户信息.手机号, 添加了)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    If 添加了 = True Then
                        Return New 类_SS包生成器(查询结果_常量集合.暂停发送验证码)
                    End If
                    结果 = 数据库_添加验证码(验证码添加时间, 验证码, 用户信息.手机号, True)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    结果 = 数据库_保存短信(用户信息.手机号, 生成验证码短信(验证码, LanguageCode))
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                Else
                    Dim 添加了 As Boolean
                    结果 = 数据库_最近是否添加了验证码(用户信息.电子邮箱地址, 添加了)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    If 添加了 = True Then
                        Return New 类_SS包生成器(查询结果_常量集合.暂停发送验证码)
                    End If
                    结果 = 数据库_添加验证码(验证码添加时间, 验证码, 用户信息.电子邮箱地址)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    Dim 界面文字 As 类_界面文字 = 获取界面文字(LanguageCode)
                    Dim 邮件 As New 类_邮件
                    邮件.收件人 = 用户信息.电子邮箱地址
                    邮件.标题 = 界面文字.获取(4, "Please reset your password")
                    邮件.正文 = 界面文字.获取(1, "Verification code: #%", New Object() {验证码}) & vbCrLf &
                                           界面文字.获取(2, "Sent at: #%", New Object() {Date.UtcNow.AddMinutes(时区偏移量).ToString}) & vbCrLf &
                                           界面文字.获取(3, "Sent by: the system (Please don't reply this email)") & vbCrLf &
                                           网站链接
                    结果 = 数据库_保存邮件(邮件)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            结果.添加_有标签("验证码添加时间", 验证码添加时间)
            If 用户信息.手机号 > 0 Then
                结果.添加_有标签("发送至手机", True)
                短信管理器.发送短信()
            Else
                结果.添加_有标签("发送至手机", False)
                邮件管理器.发送邮件()
            End If
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_获取用户手机号和邮箱地址(ByVal 用户名 As String, ByVal 是英语用户名 As Boolean, ByVal 用户信息 As 类_用户信息) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 索引名称 As String
            Dim 列添加器 As New 类_列添加器
            If 是英语用户名 = True Then
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 用户名)
                索引名称 = "#英语用户名"
            Else
                列添加器.添加列_用于筛选器("本国语用户名", 筛选方式_常量集合.等于, 用户名)
                索引名称 = "#本国语用户名"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"停用", "编号", "手机号", "电子邮箱地址"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 索引名称)
            Dim 停用 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                停用 = 读取器(0)
                If 停用 = False Then
                    用户信息.用户编号 = 读取器(1)
                    用户信息.手机号 = 读取器(2)
                    用户信息.电子邮箱地址 = 读取器(3)
                End If
                Exit While
            End While
            读取器.关闭()
            If 停用 Then
                Return New 类_SS包生成器(查询结果_常量集合.账号停用)
            End If
            If 用户信息.用户编号 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.不正确)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 重设密码() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim SSAddress As String = Http请求("SSAddress")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim Password As String = Http请求("Password")

        SSAddress = SSAddress.ToLower
        If 是否是有效的讯宝或电子邮箱地址(SSAddress) = False Then Return Nothing
        If SSAddress.EndsWith(讯宝地址标识 & 域名_英语) = False Then
            If String.IsNullOrEmpty(域名_本国语) = False Then
                If SSAddress.EndsWith(讯宝地址标识 & 域名_本国语) = False Then
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End If
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        If String.IsNullOrEmpty(Password) = True Then Return Nothing
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        Dim 主机名 As String = Nothing
        Dim 位置号 As Short
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        Dim 结果 As 类_SS包生成器
        Dim 用户信息 As 类_用户信息
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.全部)
                Dim 段() As String = SSAddress.Split(讯宝地址标识)
                结果 = 数据库_获取用户手机号和邮箱地址(段(0), True, 用户信息)
                If 结果.查询结果 = 查询结果_常量集合.不正确 Then
                    If String.IsNullOrEmpty(域名_本国语) = False Then
                        结果 = 数据库_获取用户手机号和邮箱地址(段(0), False, 用户信息)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                    Else
                        Return 结果
                    End If
                ElseIf 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 用户信息.手机号 > 0 Then
                    结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, 用户信息.手机号, True)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                Else
                    结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, 用户信息.电子邮箱地址, True)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
                结果 = 数据库_获取传送服务器(用户信息.用户编号, 主机名, 位置号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(主机名) = False Then
                    子域名 = 获取服务器域名(主机名 & "." & 域名_英语)
                    结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名, 服务器连接凭据)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
                结果 = 数据库_重设密码(用户信息.用户编号, Password)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_清除连接凭据(用户信息.用户编号)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If 结果.查询结果 = 查询结果_常量集合.成功 AndAlso String.IsNullOrEmpty(主机名) = False Then
            If String.IsNullOrEmpty(服务器连接凭据) Then
                结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名, 服务器连接凭据)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            End If
            Return 访问其它服务器("https://" & 子域名 & "/?C=UserOnOrOff&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&UserID=" & 用户信息.用户编号 & "&Position=" & 位置号)
        Else
            Return 结果
        End If
    End Function

    Private Function 数据库_获取传送服务器(ByVal 用户编号 As Long, ByRef 主机名 As String, ByRef 位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"主机名", "位置号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                主机名 = 读取器(0)
                位置号 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_重设密码(ByVal 用户编号 As Long, ByVal 密码 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            Dim 哈希值计算器 As New SHA1Managed
            列添加器_新数据.添加列_用于插入数据("密码哈希值", 哈希值计算器.ComputeHash(UTF8.GetBytes(密码)))
            哈希值计算器.Dispose()
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

#End Region

#Region "修改"

    Public Function 获取账户信息() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")

        If UserID < 1 Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        Dim 用户信息 As 类_用户信息
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.全部)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                用户信息.用户编号 = UserID
                结果 = 数据库_获取登录信息(用户信息)
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
        添加用户信息(结果, 用户信息, 时区偏移量)
        Return 结果
    End Function

    Private Sub 添加用户信息(ByVal 结果 As 类_SS包生成器, ByVal 用户信息 As 类_用户信息, ByVal 时区偏移量 As Integer)
        Dim SS包生成器 As New 类_SS包生成器(, 12)
        SS包生成器.添加_有标签("英语域名", 域名_英语)
        If String.IsNullOrEmpty(域名_本国语) = False Then
            SS包生成器.添加_有标签("本国语域名", 域名_本国语)
        End If
        If String.IsNullOrEmpty(用户信息.英语用户名) = False Then
            SS包生成器.添加_有标签("英语用户名", 用户信息.英语用户名)
        End If
        If String.IsNullOrEmpty(用户信息.本国语用户名) = False Then
            SS包生成器.添加_有标签("本国语用户名", 用户信息.本国语用户名)
        End If
        If 用户信息.手机号 > 0 Then
            SS包生成器.添加_有标签("手机号", 用户信息.手机号)
        End If
        If String.IsNullOrEmpty(用户信息.电子邮箱地址) = False Then
            SS包生成器.添加_有标签("电子邮箱地址", 用户信息.电子邮箱地址)
        End If
        If String.IsNullOrEmpty(用户信息.职能) = False Then
            SS包生成器.添加_有标签("职能", 用户信息.职能)
        End If
        If 用户信息.登录时间_电脑 > 0 Then
            SS包生成器.添加_有标签("登录时间_电脑", Date.FromBinary(用户信息.登录时间_电脑).AddMinutes(时区偏移量).ToString)
        End If
        If 用户信息.登录时间_手机 > 0 Then
            SS包生成器.添加_有标签("登录时间_手机", Date.FromBinary(用户信息.登录时间_手机).AddMinutes(时区偏移量).ToString)
        End If
        If String.IsNullOrEmpty(用户信息.网络地址_电脑) = False Then
            SS包生成器.添加_有标签("网络地址_电脑", 用户信息.网络地址_电脑)
        End If
        If String.IsNullOrEmpty(用户信息.网络地址_手机) = False Then
            SS包生成器.添加_有标签("网络地址_手机", 用户信息.网络地址_手机)
        End If
        结果.添加_有标签("用户信息", SS包生成器.生成SS包)
    End Sub

    Private Function 数据库_获取登录信息(ByVal 用户信息 As 类_用户信息) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户信息.用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"登录时间_电脑", "登录时间_手机", "网络地址_电脑", "网络地址_手机"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                用户信息.登录时间_电脑 = 读取器(0)
                用户信息.登录时间_手机 = 读取器(1)
                Dim 字节数组() As Byte
                字节数组 = 读取器(2)
                If 字节数组 IsNot Nothing Then
                    Dim 网络地址 As New IPAddress(字节数组)
                    用户信息.网络地址_电脑 = 网络地址.ToString
                End If
                字节数组 = 读取器(3)
                If 字节数组 IsNot Nothing Then
                    Dim 网络地址 As New IPAddress(字节数组)
                    用户信息.网络地址_手机 = 网络地址.ToString
                End If
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改密码() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID2 As String = Http请求("UserID")
        Dim UserID As Long
        If Long.TryParse(UserID2, UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim NewPassword As String = Http请求("NewPassword")
        Dim CurrentPassword As String = Http请求("CurrentPassword")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(CurrentPassword) = True Then Return Nothing
        If CurrentPassword.Length > 最大值_常量集合.密码长度 OrElse CurrentPassword.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If String.IsNullOrEmpty(NewPassword) = True Then Return Nothing
        If NewPassword.Length > 最大值_常量集合.密码长度 OrElse NewPassword.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If String.Compare(CurrentPassword, NewPassword) = 0 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 结果 As 类_SS包生成器
            Try
                结果 = 数据库_验证用户凭据(UserID, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(UserID, 操作次数, 操作代码_常量集合.更换用户密码)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 2 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                结果 = 数据库_修改密码(UserID, CurrentPassword, NewPassword)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存操作记录(UserID, 操作代码_常量集合.更换用户密码, 获取网络地址字节数组)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_修改密码(ByVal 用户编号 As Long, ByVal 当前密码 As String, ByVal 新密码 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            Dim 哈希值计算器 As New SHA1Managed
            列添加器_新数据.添加列_用于插入数据("密码哈希值", 哈希值计算器.ComputeHash(UTF8.GetBytes(新密码)))
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("密码哈希值", 筛选方式_常量集合.等于, 哈希值计算器.ComputeHash(UTF8.GetBytes(当前密码)))
            哈希值计算器.Dispose()
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.不正确)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 新手机号() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim PhoneNumber As Long
        If Long.TryParse(Http请求("PhoneNumber"), PhoneNumber) = False Then Return Nothing
        Dim Password As String = Http请求("Password")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")
        Dim LanguageCode As String = Http请求("LanguageCode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Password) = True Then Return Nothing
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If PhoneNumber <= 0 Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 验证码 As String = Nothing
            Dim 验证码添加时间 As Long
            Dim 结果 As 类_SS包生成器
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.手机号)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 哈希值计算器 As New SHA1Managed
                Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(Password))
                哈希值计算器.Dispose()
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(UserID, 操作次数, 操作代码_常量集合.更换手机号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 1 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                Dim 编号 As Long
                结果 = 数据库_手机号是否已绑定(PhoneNumber, 编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 编号 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.手机号已绑定)
                End If
                If 用户信息.手机号 = PhoneNumber Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                结果 = 数据库_添加手机号更换记录_副数据库(用户信息.手机号, PhoneNumber)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_添加验证码(验证码添加时间, 验证码, 用户信息.手机号, True)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存短信(PhoneNumber, 生成验证码短信(验证码, LanguageCode))
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            结果.添加_有标签("验证码添加时间", 验证码添加时间)
            短信管理器.发送短信()
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加手机号更换记录_副数据库(ByVal 当前手机号 As String, ByVal 新手机号 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddMinutes(-30).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "手机号更换", 筛选器)
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("原手机号", 当前手机号)
            列添加器.添加列_用于插入数据("新手机号", 新手机号)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "手机号更换", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 验证新手机号() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID2 As String = Http请求("UserID")
        Dim UserID As Long
        If Long.TryParse(UserID2, UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim PhoneNumber As Long
        If Long.TryParse(Http请求("PhoneNumber"), PhoneNumber) = False Then Return Nothing
        Dim LanguageCode As String = Http请求("LanguageCode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        If PhoneNumber <= 0 Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.手机号)
                Dim 结果 As 类_SS包生成器
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, 用户信息.手机号, True)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_删除手机号更换记录(用户信息.手机号, PhoneNumber)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 编号 As Long
                结果 = 数据库_手机号是否已绑定(PhoneNumber, 编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 编号 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.手机号已绑定)
                End If
                结果 = 数据库_添加手机号更换记录_主数据库(用户信息.手机号, PhoneNumber)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_更换用户手机号(UserID, PhoneNumber)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(UserID, 操作代码_常量集合.更换手机号, 获取网络地址字节数组)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_删除手机号更换记录(ByVal 当前手机号 As Long, ByVal 新手机号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("原手机号", 筛选方式_常量集合.等于, 当前手机号)
            列添加器.添加列_用于筛选器("新手机号", 筛选方式_常量集合.等于, 新手机号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "手机号更换", 筛选器)
            If 指令.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加手机号更换记录_主数据库(ByVal 当前手机号 As Long, ByVal 新手机号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("原手机号", 当前手机号)
            列添加器.添加列_用于插入数据("新手机号", 新手机号)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "手机号更换", 列添加器, True)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更换用户手机号(ByVal 用户编号 As Long, ByVal 新手机号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("手机号", 新手机号)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 新电子邮箱地址() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim EmailAddress As String = Http请求("EmailAddress")
        Dim Password As String = Http请求("Password")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")
        Dim LanguageCode As String = Http请求("LanguageCode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Password) = True Then Return Nothing
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        EmailAddress = EmailAddress.Trim.ToLower
        If 是否是有效的讯宝或电子邮箱地址(EmailAddress) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 验证码 As String = Nothing
            Dim 验证码添加时间 As Long
            Dim 结果 As 类_SS包生成器
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.电子邮箱地址)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 哈希值计算器 As New SHA1Managed
                Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(Password))
                哈希值计算器.Dispose()
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(UserID, 操作次数, 操作代码_常量集合.更换电子邮箱地址)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 1 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                Dim 编号 As Long
                结果 = 数据库_电子邮箱地址是否已绑定(EmailAddress, 编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 编号 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.手机号已绑定)
                End If
                If String.Compare(用户信息.电子邮箱地址, EmailAddress) = 0 Then Return New 类_SS包生成器(查询结果_常量集合.失败)
                结果 = 数据库_添加电子邮箱地址更换记录_副数据库(用户信息.电子邮箱地址, EmailAddress)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_添加验证码(验证码添加时间, 验证码, 用户信息.电子邮箱地址)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 界面文字 As 类_界面文字 = 获取界面文字(LanguageCode)
                Dim 邮件 As New 类_邮件
                邮件.收件人 = EmailAddress
                邮件.标题 = 界面文字.获取(6, "Please verify your new email address")
                邮件.正文 = 界面文字.获取(1, "Verification code: #%", New Object() {验证码}) & vbCrLf &
                    界面文字.获取(2, "Sent at: #%", New Object() {Date.UtcNow.AddMinutes(时区偏移量).ToString}) & vbCrLf &
                    界面文字.获取(3, "Sent by: Robot (Please don't reply this email)") & vbCrLf &
                    网站链接
                结果 = 数据库_保存邮件(邮件)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            结果.添加_有标签("验证码添加时间", 验证码添加时间)
            邮件管理器.发送邮件()
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加电子邮箱地址更换记录_副数据库(ByVal 当前电子邮箱地址 As String, ByVal 新电子邮箱地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddMinutes(-30).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "电子邮箱地址更换", 筛选器)
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("原电子邮箱地址", 当前电子邮箱地址)
            列添加器.添加列_用于插入数据("新电子邮箱地址", 新电子邮箱地址)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "电子邮箱地址更换", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 验证新电子邮箱地址() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID2 As String = Http请求("UserID")
        Dim UserID As Long
        If Long.TryParse(UserID2, UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim EmailAddress As String = Http请求("EmailAddress")
        Dim LanguageCode As String = Http请求("LanguageCode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        EmailAddress = EmailAddress.Trim.ToLower
        If 是否是有效的讯宝或电子邮箱地址(EmailAddress) = False Then Return Nothing
        If LanguageCode.Length <> 长度_常量集合.语言代码 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 结果 As 类_SS包生成器
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.电子邮箱地址)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_检验验证码(验证码添加时间, VerificationCode, 用户信息.电子邮箱地址, True)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_删除电子邮箱地址更换记录(用户信息.电子邮箱地址, EmailAddress)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 编号 As Long
                结果 = 数据库_电子邮箱地址是否已绑定(EmailAddress, 编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 编号 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.手机号已绑定)
                End If
                结果 = 数据库_添加电子邮箱地址更换记录_主数据库(用户信息.电子邮箱地址, EmailAddress)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_更换用户电子邮箱地址(UserID, EmailAddress)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存操作记录(UserID, 操作代码_常量集合.更换电子邮箱地址, 获取网络地址字节数组)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.电子邮箱地址) = False Then
                    Dim 界面文字 As 类_界面文字 = 获取界面文字(LanguageCode)
                    Dim 邮件 As New 类_邮件
                    邮件.收件人 = 用户信息.电子邮箱地址
                    邮件.标题 = 界面文字.获取(4, "Email address changed")
                    邮件.正文 = 界面文字.获取(5, "Your Email address changed into #% successfully.", New Object() {EmailAddress}) & vbCrLf &
                                       网站链接
                    结果 = 数据库_保存邮件(邮件)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            邮件管理器.发送邮件()
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_删除电子邮箱地址更换记录(ByVal 当前电子邮箱地址 As String, ByVal 新电子邮箱地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("原电子邮箱地址", 筛选方式_常量集合.等于, 当前电子邮箱地址)
            列添加器.添加列_用于筛选器("新电子邮箱地址", 筛选方式_常量集合.等于, 新电子邮箱地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "电子邮箱地址更换", 筛选器)
            If 指令.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加电子邮箱地址更换记录_主数据库(ByVal 当前电子邮箱地址 As String, ByVal 新电子邮箱地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("原电子邮箱地址", 当前电子邮箱地址)
            列添加器.添加列_用于插入数据("新电子邮箱地址", 新电子邮箱地址)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "电子邮箱地址更换", 列添加器, True)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更换用户电子邮箱地址(ByVal 用户编号 As Long, ByVal 新电子邮箱地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("电子邮箱地址", 新电子邮箱地址)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function




    '下面提示体验程序的代码可删除
    Private Function 数据库_获取可用的位置号_讯宝网络体验程序(ByVal 主机名 As String, ByRef 位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            列添加器.添加列_用于筛选器("停用", 筛选方式_常量集合.等于, True)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"编号", "位置号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, 1, "#主机名位置号")
            Dim 编号 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                编号 = 读取器(0)
                位置号 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_删除数据(主数据库, "用户", 筛选器, 主键索引名)
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

#End Region

End Class
