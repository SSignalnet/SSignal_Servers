Imports SSignal_Protocols
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 验证讯宝地址真实性() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim Credential As String = Http请求("Credential")
        Dim Domain_English As String = Http请求("Domain_English")
        Dim Domain_Native As String = Http请求("Domain_Native")
        Dim SSAddress As String = Http请求("SSAddress")

        If Credential.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Domain_English = Domain_English.Trim.ToLower
        If Domain_English.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.Compare(Domain_English, 讯宝中心服务器主机名 & "." & 域名_英语) = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 英语子域名_外域服务器 As String = 获取服务器域名(Domain_English)
        Dim 本国语子域名_外域服务器 As String
        If String.IsNullOrEmpty(Domain_Native) = False Then
            Domain_Native = Domain_Native.Trim.ToLower
            If Domain_Native.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            If String.Compare(Domain_Native, 讯宝中心服务器主机名 & "." & 域名_本国语) = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            本国语子域名_外域服务器 = 获取服务器域名(Domain_Native)
        Else
            本国语子域名_外域服务器 = Nothing
        End If
        SSAddress = SSAddress.Trim.ToLower
        If 是否是有效的讯宝或电子邮箱地址(SSAddress) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 段() As String = SSAddress.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        If 段.Length <> 2 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim 英语用户名 As String = Nothing
        Dim 本国语用户名 As String = Nothing
        Select Case 段(1)
            Case 域名_英语 : 英语用户名 = 段(0)
            Case 域名_本国语 : 本国语用户名 = 段(0)
            Case Else : Return New 类_SS包生成器(查询结果_常量集合.讯宝地址不存在)
        End Select
        Dim 结果 As 类_SS包生成器
        Dim 更新时间 As Long
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_验证其它服务器访问我方的凭据(Context, 副数据库, 英语子域名_外域服务器, Credential, 更新时间)
                If 结果.查询结果 = 查询结果_常量集合.凭据有效 Then
                    Return 查找用户(英语用户名, 本国语用户名)
                End If
                If String.IsNullOrEmpty(Domain_Native) = False Then
                    结果 = 数据库_验证其它服务器访问我方的凭据(Context, 副数据库, 本国语子域名_外域服务器, Credential, 更新时间)
                    If 结果.查询结果 = 查询结果_常量集合.凭据有效 Then
                        Return 查找用户(英语用户名, 本国语用户名)
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
        Select Case 结果.查询结果
            Case 查询结果_常量集合.需要添加连接凭据
            Case 查询结果_常量集合.不正确, 查询结果_常量集合.未知IP地址
                If 更新时间 > 0 Then
                    If DateDiff(DateInterval.Minute, Date.FromBinary(更新时间), Date.UtcNow) < 5 Then
                        Return 结果
                    End If
                End If
            Case Else : Return 结果
        End Select
        Dim 访问结果 As Object = 访问其它服务器(获取路径_验证服务器真实性(Domain_English, Credential, 讯宝中心服务器主机名 & "." & 段(1), Domain_Native))
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
                结果 = 数据库_添加其它服务器访问我方的凭据(Context, 副数据库, 英语子域名_外域服务器, 本国语子域名_外域服务器, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                Return 查找用户(英语用户名, 本国语用户名)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 查找用户(ByVal 英语用户名 As String, ByVal 本国语用户名 As String) As 类_SS包生成器
        Dim 用户编号 As Long
        Dim 主机名 As String = Nothing
        Dim 位置号 As Short
        Dim 停用 As Boolean
        Dim 结果 As 类_SS包生成器
        If String.IsNullOrEmpty(英语用户名) = False Then
            结果 = 数据库_根据英语用户名查找用户(英语用户名, 用户编号, 停用, 本国语用户名, 主机名, 位置号)
        Else
            结果 = 数据库_根据本国语用户名查找用户(本国语用户名, 用户编号, 停用, 英语用户名, 主机名, 位置号)
        End If
        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
        If 用户编号 = 0 OrElse 停用 = True Then
            Return New 类_SS包生成器(查询结果_常量集合.讯宝地址不存在)
        End If
        If String.IsNullOrEmpty(主机名) Then
            Return New 类_SS包生成器(查询结果_常量集合.讯宝地址不存在)
        End If
        Call 添加数据_验证讯宝地址真实性(结果, 英语用户名, 本国语用户名, 主机名, 位置号, 域名_英语, 域名_本国语)
        Return 结果
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
        If String.Compare(Domain_English, 讯宝中心服务器主机名 & "." & 域名_英语) <> 0 Then
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If String.IsNullOrEmpty(Domain_Native) = False Then
            Domain_Native = Domain_Native.Trim.ToLower
            If Domain_Native.Length > 最大值_常量集合.子域名长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
            If String.Compare(Domain_Native, 讯宝中心服务器主机名 & "." & 域名_本国语) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        End If
        Dim 我方访问外域的凭据 As String = Nothing
        Dim 网络地址_数据库中() As Byte = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_获取访问其它服务器的凭据(副数据库, 获取服务器域名(Domain_Ask), 我方访问外域的凭据, 网络地址_数据库中)
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
        Dim 网络地址字节数组() As Byte = 获取网络地址字节数组()
        If 网络地址字节数组 Is Nothing Then
            Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
        End If
        If 测试 = False Then
            If 网络地址_数据库中.Length <> 网络地址字节数组.Length Then
                Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
            End If
            Dim I As Integer
            For I = 0 To 网络地址字节数组.Length - 1
                If 网络地址_数据库中(I) <> 网络地址字节数组(I) Then Return New 类_SS包生成器(查询结果_常量集合.未知IP地址)
            Next
        End If
        If String.Compare(我方访问外域的凭据, Credential) <> 0 Then
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

End Class
