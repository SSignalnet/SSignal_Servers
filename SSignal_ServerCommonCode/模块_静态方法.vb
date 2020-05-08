Imports System.Net
Imports System.Web
Imports System.Threading
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Public Module 模块_静态方法

    Public Function 数据库_获取访问其它服务器的凭据(ByVal 数据库 As 类_数据库, ByVal 子域名 As String,
                                     ByRef 凭据 As String, Optional ByRef 网络地址() As Byte = Nothing) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"连接凭据_我访它", "网络地址"})
            Dim 指令 As New 类_数据库指令_请求获取数据(数据库, "服务器", 筛选器, 1, 列添加器,  , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                凭据 = 读取器(0)
                网络地址 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 添加我方访问其它服务器的凭据(ByVal 跨进程锁 As Mutex, ByVal 副数据库 As 类_数据库, ByVal 子域名 As String, ByRef 凭据 As String) As 类_SS包生成器
        Dim 网络地址 As IPAddress
        If 测试 Then
            网络地址 = New IPAddress(0)
            If IPAddress.TryParse("127.0.0.1", 网络地址) = False Then
                Return New 类_SS包生成器(查询结果_常量集合.获取A记录失败)
            End If
        Else
            Try
                Dim IP主机记录 As IPHostEntry = Dns.GetHostEntry(子域名)
                网络地址 = IP主机记录.AddressList(0)
            Catch ex As Exception
                Return New 类_SS包生成器(查询结果_常量集合.获取A记录失败)
            End Try
        End If
        If 跨进程锁.WaitOne = True Then
            Try
                Return 数据库_添加我方访问其它服务器的凭据(副数据库, 子域名, 凭据, 网络地址.GetAddressBytes)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加我方访问其它服务器的凭据(ByVal 数据库 As 类_数据库, ByVal 子域名 As String, ByRef 凭据 As String, ByVal 网络地址() As Byte) As 类_SS包生成器
        Try
            凭据 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器)
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址)
            列添加器_新数据.添加列_用于插入数据("连接凭据_我访它", 凭据)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("子域名", 子域名)
                列添加器.添加列_用于插入数据("网络地址", 网络地址)
                列添加器.添加列_用于插入数据("连接凭据_我访它", 凭据)
                列添加器.添加列_用于插入数据("更新时间_它访我", 0)
                Dim 指令2 As New 类_数据库指令_插入新数据(数据库, "服务器", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 数据库_验证其它服务器访问我方的凭据(ByVal Context As HttpContext, ByVal 数据库 As 类_数据库, ByVal 子域名 As String, ByVal 连接凭据 As String, ByRef 更新时间_它访我 As Long) As 类_SS包生成器
        If String.IsNullOrEmpty(连接凭据) = True Then Return Nothing
        If 连接凭据.Length <> 长度_常量集合.连接凭据_服务器 Then Return Nothing
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(网络地址_文本, 网络地址) = False Then Return Nothing
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"网络地址", "连接凭据_它访我", "更新时间_它访我"})
            Dim 指令 As New 类_数据库指令_请求获取数据(数据库, "服务器", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 网络地址_数据库中的() As Byte = Nothing
            Dim 连接凭据_数据库中的 As String = Nothing
            Dim 找到了 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                网络地址_数据库中的 = 读取器(0)
                连接凭据_数据库中的 = 读取器(1)
                更新时间_它访我 = 读取器(2)
                找到了 = True
                Exit While
            End While
            读取器.关闭()
            If 找到了 = False Then
                Return New 类_SS包生成器(查询结果_常量集合.需要添加连接凭据)
            End If
            If 网络地址_数据库中的 Is Nothing Then
                Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
            End If
            If 测试 = False Then
                Dim 网络地址2() As Byte = 网络地址.GetAddressBytes
                If 网络地址2.Length <> 网络地址_数据库中的.Length Then
                    Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
                End If
                Dim I As Integer
                For I = 0 To 网络地址2.Length - 1
                    If 网络地址2(I) <> 网络地址_数据库中的(I) Then Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
                Next
            End If
            If String.IsNullOrEmpty(连接凭据_数据库中的) = True Then
                Return New 类_SS包生成器(查询结果_常量集合.需要添加连接凭据)
            End If
            If String.Compare(连接凭据, 连接凭据_数据库中的) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.不正确)
            End If
            Return New 类_SS包生成器(查询结果_常量集合.凭据有效)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 数据库_添加其它服务器访问我方的凭据(ByVal Context As HttpContext, ByVal 数据库 As 类_数据库, ByVal 英语子域名 As String, ByVal 本国语子域名 As String, ByVal 凭据 As String) As 类_SS包生成器
        Dim 网络地址_文本 As String = Context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(网络地址_文本) Then
            网络地址_文本 = Context.Request.ServerVariables("REMOTE_ADDR")
        End If
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(网络地址_文本, 网络地址) = False Then Return Nothing
        Dim 需要后续指令确认 As Boolean
        If String.IsNullOrEmpty(本国语子域名) = False Then
            需要后续指令确认 = True
        End If
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址.GetAddressBytes)
            列添加器_新数据.添加列_用于插入数据("连接凭据_它访我", 凭据)
            列添加器_新数据.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 英语子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名, 需要后续指令确认)
            If 指令.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("子域名", 英语子域名)
                列添加器.添加列_用于插入数据("网络地址", 网络地址.GetAddressBytes)
                列添加器.添加列_用于插入数据("连接凭据_它访我", 凭据)
                列添加器.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                Dim 指令2 As New 类_数据库指令_插入新数据(数据库, "服务器", 列添加器, 需要后续指令确认)
                指令2.执行()
            End If
            If 需要后续指令确认 Then
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址.GetAddressBytes)
                列添加器_新数据.添加列_用于插入数据("连接凭据_它访我", 凭据)
                列添加器_新数据.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 本国语子域名)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令 = New 类_数据库指令_更新数据(数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
                If 指令.执行() = 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("子域名", 本国语子域名)
                    列添加器.添加列_用于插入数据("网络地址", 网络地址.GetAddressBytes)
                    列添加器.添加列_用于插入数据("连接凭据_它访我", 凭据)
                    列添加器.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                    Dim 指令2 As New 类_数据库指令_插入新数据(数据库, "服务器", 列添加器)
                    指令2.执行()
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 获取小宇宙文本库号(ByVal 最大字节数 As Integer) As Short
        Dim 单位长度 As Integer
        If 最大字节数 <= 100 Then
            单位长度 = 10
        ElseIf 最大字节数 <= 300 Then
            单位长度 = 20
        ElseIf 最大字节数 <= 700 Then
            单位长度 = 40
        ElseIf 最大字节数 <= 1300 Then
            单位长度 = 60
        Else
            单位长度 = 100
        End If
        Return CInt(Math.Ceiling(最大字节数 / 单位长度)) * 单位长度
    End Function

    Public Function XML错误信息(ByVal 错误信息 As String) As String
        Return "<ERROR>" & 替换XML敏感字符(错误信息) & "</ERROR>"
    End Function

End Module
