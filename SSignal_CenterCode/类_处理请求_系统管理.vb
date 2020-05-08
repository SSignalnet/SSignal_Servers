Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Encoding
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 获取服务器连接凭据() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Domain As String = Http请求("Domain")

        If UserID < 1 Then Return Nothing
        Dim 访问本域服务器的凭据 As String = Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, Domain, 访问本域服务器的凭据)
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
        If String.IsNullOrEmpty(访问本域服务器的凭据) Then
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, Domain, 访问本域服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Return 访问其它服务器("https://" & Domain & "/?C=AdminCredential&Credential=" & 替换URI敏感字符(访问本域服务器的凭据))
    End Function

    Public Function 备份数据库时获取服务器列表() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")

        If UserID < 1 Then Return Nothing
        Dim 服务器主机名() As String
        Dim 服务器数量 As Integer
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                ReDim 服务器主机名(99)
                结果 = 数据库_备份数据库时获取服务器列表(服务器主机名, 服务器数量)
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
        If 服务器数量 > 0 Then
            Dim SS包生成器 As New 类_SS包生成器(, 服务器数量)
            Dim I As Integer
            For I = 0 To 服务器数量 - 1
                SS包生成器.添加_有标签("主机名", 服务器主机名(I))
            Next
            结果.添加_有标签("服务器", SS包生成器)
        End If
        Return 结果
    End Function

    Private Function 数据库_备份数据库时获取服务器列表(ByRef 服务器主机名() As String, ByRef 服务器数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据("主机名")
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "服务器", Nothing, , 列添加器, 100, 主键索引名)
            Dim 主机名 As String
            读取器 = 指令2.执行()
            While 读取器.读取
                主机名 = 读取器(0)
                If 主机名.StartsWith(讯宝中心服务器主机名) OrElse String.Compare(主机名, 讯宝小宇宙中心服务器主机名) = 0 OrElse 主机名.StartsWith(讯宝大聊天群服务器主机名前缀) Then
                    If 服务器主机名.Length = 服务器数量 Then ReDim Preserve 服务器主机名(服务器数量 * 2 - 1)
                    服务器主机名(服务器数量) = 主机名
                    服务器数量 += 1
                End If
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 列出服务器() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Type As 服务器类别_常量集合
        If Byte.TryParse(Http请求("Type"), Type) = False Then Return Nothing
        Dim OrderBy As String = Http请求("OrderBy")
        Dim HostName As String = Http请求("HostName")
        Dim PageNumber As String = Http请求("PageNumber")
        Dim Passcode As String = Http请求("Passcode")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(HostName) = False Then
            If HostName.Length > 最大值_常量集合.主机名字符数 Then Return Nothing
            HostName = HostName.ToLower
        End If
        Dim 第几页 As Long
        If Long.TryParse(PageNumber, 第几页) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 服务器() As 服务器_复合数据
        Dim 服务器数量 As Short
        Dim 总数 As Long
        Const 每页条数 As Integer = 20
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
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return XML错误信息(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML不正确
                    End If
                End If
                If 第几页 < 1 Then 第几页 = 1
                ReDim 服务器(每页条数 - 1)
跳转点1:
                结果 = 数据库_获取服务器列表(服务器, 服务器数量, Type, OrderBy, HostName, 每页条数, 第几页, 总数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 服务器数量 = 0 Then
                    If 总数 > 0 Then
                        第几页 = Int(总数 / 每页条数)
                        If 总数 Mod 每页条数 <> 0 Then 第几页 += 1
                        GoTo 跳转点1
                    End If
                End If
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
        Dim 变长文本 As New StringBuilder(300 * 服务器数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<PAGENUMBER>" & 第几页 & "</PAGENUMBER>")
        文本写入器.Write("<TOTALPAGES>" & Math.Ceiling(总数 / 每页条数) & "</TOTALPAGES>")
        文本写入器.Write("<TYPE>" & Type & "</TYPE>")
        文本写入器.Write("<ORDERBY>" & OrderBy & "</ORDERBY>")
        文本写入器.Write("<HOSTNAME>" & HostName & "</HOSTNAME>")
        文本写入器.Write("<DOMAIN>" & 域名_英语 & "</DOMAIN>")
        If 服务器数量 > 0 Then
            Dim 网络地址 As IPAddress
            Dim I, J As Short
            For I = 0 To 服务器数量 - 1
                文本写入器.Write("<SERVER>")
                With 服务器(I)
                    文本写入器.Write("<NAME>" & .主机名 & "</NAME>")
                    网络地址 = New IPAddress(.网络地址)
                    文本写入器.Write("<IP>" & 网络地址.ToString & "</IP>")
                    文本写入器.Write("<DATE>" & Date.FromBinary(.时间).AddMinutes(时区偏移量).ToString & "</DATE>")
                    If .停用 = True Then 文本写入器.Write(XML已停用)
                    If .故障信息数量 > 0 Then
                        For J = 0 To .故障信息数量 - 1
                            文本写入器.Write("<ERROR" & J + 1 & ">" & 替换XML敏感字符("[" & Date.FromBinary(.时间_故障信息(J)).AddMinutes(时区偏移量).ToString & "] " & .故障信息(J)) & "</ERROR" & J + 1 & ">")
                        Next
                    End If
                    文本写入器.Write("<STATISTICS>" & .统计 & "</STATISTICS>")
                End With
                文本写入器.Write("</SERVER>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取服务器列表(ByRef 服务器() As 服务器_复合数据, ByRef 服务器数量 As Short,
                                        ByVal 服务器类别 As 服务器类别_常量集合, ByRef 排序方式 As String, ByVal 主机名 As String,
                                       ByVal 每页条数 As Integer, ByVal 第几页 As Long, ByRef 总数 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 索引名称 As String = Nothing
            Dim 反向 As Boolean
            Select Case 排序方式
                Case "time_j"
                    索引名称 = "#类别时间"
                    反向 = True
                Case "time_s"
                    索引名称 = "#类别时间"
                Case "name_s"
                    索引名称 = "#类别主机名"
                Case "name_j"
                    索引名称 = "#类别主机名"
                    反向 = True
                Case Else
                    索引名称 = "#类别时间"
                    排序方式 = "time_j"
                    反向 = True
            End Select
            Dim 筛选器 As 类_筛选器 = Nothing
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("类别", 筛选方式_常量集合.等于, 服务器类别)
            If String.IsNullOrEmpty(主机名) = False Then
                列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.包含, 主机名, 字符串包含方式_常量集合.任意位置有)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"主机名", "网络地址", "停用", "时间", "统计"})
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "服务器", 筛选器, , 列添加器, 100, 索引名称, 反向, , True)
                Dim 起始处 As Long = (第几页 - 1) * 每页条数
                总数 = 0
                服务器数量 = 0
                读取器 = 指令.执行()
                While 读取器.读取
                    总数 += 1
                    If 服务器数量 < 每页条数 Then
                        If 总数 > 起始处 Then
                            With 服务器(服务器数量)
                                .主机名 = 读取器(0)
                                .网络地址 = 读取器(1)
                                .停用 = 读取器(2)
                                .时间 = 读取器(3)
                                .统计 = 读取器(4)
                            End With
                            服务器数量 += 1
                        End If
                    End If
                End While
                读取器.关闭()
            Else
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"主机名", "网络地址", "停用", "时间", "统计"})
                Dim 指令2 As New 类_数据库指令_请求快速获取分页数据(主数据库, "服务器", 索引名称, 筛选器, 反向, 列添加器, 第几页, 每页条数)
                Dim 读取器2 As 类_读取器_快速分页 = 指令2.执行()
                总数 = 读取器2.记录总数量
                While 读取器2.读取
                    With 服务器(服务器数量)
                        .主机名 = 读取器2(0)
                        .网络地址 = 读取器2(1)
                        .停用 = 读取器2(2)
                        .时间 = 读取器2(3)
                        .统计 = 读取器2(4)
                    End With
                    服务器数量 += 1
                End While
            End If
            If 服务器数量 > 0 Then
                Const 数量上限 As Byte = 3
                Dim I As Integer
                For I = 0 To 服务器数量 - 1
                    With 服务器(I)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, .主机名)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于获取数据(New String() {"故障信息", "时间"})
                        Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "服务器故障", 筛选器, 数量上限, 列添加器, 数量上限, "#主机名时间")
                        ReDim .故障信息(数量上限 - 1)
                        ReDim .时间_故障信息(数量上限 - 1)
                        读取器 = 指令.执行()
                        While 读取器.读取
                            .故障信息(.故障信息数量) = 读取器(0)
                            .时间_故障信息(.故障信息数量) = 读取器(1)
                            .故障信息数量 += 1
                        End While
                        读取器.关闭()
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 获取服务器信息() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim HostName As String = Http请求("HostName")
        Dim InfoType As String = Http请求("InfoType")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
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
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return XML错误信息(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                子域名 = 获取服务器域名(HostName & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名, 服务器连接凭据)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return XML错误信息(结果.出错提示文本)
                End If
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
        If String.IsNullOrEmpty(服务器连接凭据) = False Then
            Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名 & "/?C=GetServerInfo&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&InfoType=" & InfoType)
            If TypeOf 访问结果 Is 类_SS包生成器 Then
                Dim 结果 As 类_SS包生成器 = 访问结果
                If 结果.查询结果 = 查询结果_常量集合.出错 Then
                    Return XML错误信息(结果.出错提示文本)
                Else
                    Return XML失败
                End If
            Else
                Return "<SUCCEED>" & 替换XML敏感字符(UTF8.GetString(CType(访问结果, Byte()))) & "</SUCCEED>"
            End If
        Else
            Return XML失败
        End If
    End Function

    Public Function 添加服务器账号() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Name As String = Http请求("Name")
        Dim IP As String = Http请求("IP")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Name) = True Then Return Nothing
        If Name.Length > 最大值_常量集合.主机名字符数 Then Return Nothing
        Name = Name.Trim.ToLower
        If String.Compare(Name, 讯宝中心服务器主机名) = 0 Then Return Nothing
        Dim 类别 As 服务器类别_常量集合
        If Name.StartsWith(讯宝中心服务器主机名) Then
            类别 = 服务器类别_常量集合.传送服务器
        ElseIf Name.StartsWith(讯宝大聊天群服务器主机名前缀) Then
            类别 = 服务器类别_常量集合.大聊天群服务器
        ElseIf String.Compare(Name, 讯宝小宇宙中心服务器主机名) = 0 Then
            类别 = 服务器类别_常量集合.小宇宙中心服务器
        ElseIf Name.StartsWith(讯宝小宇宙写入服务器主机名前缀) Then
            类别 = 服务器类别_常量集合.小宇宙写入服务器
        ElseIf Name.StartsWith(讯宝小宇宙读取服务器主机名前缀) Then
            类别 = 服务器类别_常量集合.小宇宙读取服务器
        ElseIf Name.StartsWith(讯宝视频通话服务器主机名前缀) Then
            类别 = 服务器类别_常量集合.视频通话服务器
        Else
            Return Nothing
        End If
        IP = IP.Trim
        If String.Compare(IP, "0.0.0.0") = 0 Then Return Nothing
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(IP, 网络地址) = False Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能和主机名)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_添加服务器账号(Name, 类别, 网络地址.GetAddressBytes)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.主机名) Then
                    结果 = 数据库_给管理员账号分配传送服务器(UserID, Name)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    结果.添加_有标签("主机名", Name)
                    Return 结果
                Else
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
    End Function

    Private Function 数据库_添加服务器账号(ByVal 主机名 As String, ByVal 类别 As 服务器类别_常量集合, ByVal 网络地址() As Byte) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("类别", 类别)
            列添加器.添加列_用于插入数据("网络地址", 网络地址)
            列添加器.添加列_用于插入数据("停用", False)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("统计", 0)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "服务器", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器("主机名或网络地址已存在")
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_给管理员账号分配传送服务器(ByVal 用户编号 As Long, ByVal 主机名 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("主机名", 主机名)
            列添加器_新数据.添加列_用于插入数据("位置号", 0)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行() > 0 Then
                Dim 运算器 As New 类_运算器("统计")
                运算器.添加运算指令(运算符_常量集合.加, 1)
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("统计", 运算器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
                If 指令2.执行 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改服务器账号() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Name As String = Http请求("Name")
        Dim IP As String = Http请求("IP")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If Name.Length > 最大值_常量集合.主机名字符数 Then Return Nothing
        Dim 网络地址 As New IPAddress(0)
        If IPAddress.TryParse(IP, 网络地址) = False Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return XML无权操作
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return XML无权操作
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return XML错误信息(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.Compare(IP, "0.0.0.0") <> 0 Then
                    结果 = 数据库_修改服务器账号(Name, 网络地址.GetAddressBytes)
                Else
                    结果 = 数据库_删除服务器账号(Name)
                End If
                Select Case 结果.查询结果
                    Case 查询结果_常量集合.成功
                        Return XML成功
                    Case 查询结果_常量集合.出错
                        Return XML错误信息(结果.出错提示文本)
                    Case Else
                        Return XML失败
                End Select
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_修改服务器账号(ByVal 主机名 As String, ByVal 网络地址() As Byte) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As 类_值已存在
            Return New 类_SS包生成器("新网络地址已存在")
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除服务器账号(ByVal 主机名 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "服务器", 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "服务器故障", 筛选器, "#主机名时间")
                指令2.执行()
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 停用启用服务器账号() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Name As String = Http请求("Name")
        Dim Decision As String = Http请求("Decision")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If Name.Length > 最大值_常量集合.主机名字符数 Then Return Nothing
        Name = Name.ToLower
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return XML无权操作
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return XML无权操作
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return XML错误信息(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                Dim 停用 As Boolean
                If String.Compare(Decision, "disable") = 0 Then 停用 = True
                结果 = 数据库_停用启用服务器账号(Name, 停用)
                Select Case 结果.查询结果
                    Case 查询结果_常量集合.成功
                        Return XML成功
                    Case 查询结果_常量集合.出错
                        Return XML错误信息(结果.出错提示文本)
                    Case Else
                        Return XML失败
                End Select
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_停用启用服务器账号(ByVal 主机名 As String, ByVal 停用 As Boolean) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("停用", 停用)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 发布安卓客户端软件() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EmailAddress As String = Http请求("EmailAddress")
        Dim Password As String = Http请求("Password")
        Dim VCodeCreatedOn As String = Http请求("VCodeCreatedOn")
        Dim VerificationCode As String = Http请求("VerificationCode")
        Dim VersionCode As String = Http请求("VersionCode")
        Dim VersionName As String = Http请求("VersionName")

        If 是否是有效的讯宝或电子邮箱地址(EmailAddress) = False Then Return Nothing
        If Password.Length > 最大值_常量集合.密码长度 OrElse Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If String.IsNullOrEmpty(VerificationCode) = True Then Return Nothing
        If VerificationCode.Length <> 长度_常量集合.验证码 Then Return Nothing
        Dim 验证码添加时间 As Long
        If Long.TryParse(VCodeCreatedOn, 验证码添加时间) = False Then Return Nothing
        Dim 新版本号 As Integer
        If Integer.TryParse(VersionCode, 新版本号) = False Then Return Nothing
        If 新版本号 < 1 Then Return Nothing
        If String.IsNullOrEmpty(VersionName) = True Then Return Nothing

        Dim APK(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(APK, 0, APK.Length)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_检验验证码(验证码添加时间, VerificationCode)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 职能 As String = ""
                结果 = 数据库_验证电子邮箱和密码(EmailAddress, Password, 职能)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Dim 目录 As String = Context.Server.MapPath("/") & "download"
        If Directory.Exists(目录) = False Then Directory.CreateDirectory(目录)
        Dim 路径_版本信息 As String = 目录 & "\SSignal_android.txt"
        If File.Exists(路径_版本信息) Then
            Dim 文件内容 As String
            Try
                文件内容 = File.ReadAllText(路径_版本信息, Encoding.UTF8)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            End Try
            Dim I As Integer = 文件内容.IndexOf("[")
            If I >= 0 Then
                Dim J As Integer = 文件内容.IndexOf("]")
                If J > I + 1 Then
                    Dim 旧版本号 As Integer
                    If Integer.TryParse(文件内容.Substring(I + 1, J - I - 1), 旧版本号) Then
                        If 新版本号 <= 旧版本号 Then
                            Return New 类_SS包生成器(查询结果_常量集合.不是新版本)
                        End If
                    End If
                End If
            End If
        End If
        Dim 文本 As String = "[" & VersionCode & "]{" & VersionName & "}(" & Date.UtcNow.ToString & ")"
        Try
            File.WriteAllBytes(目录 & "\SSignal.apk", APK)
            File.WriteAllText(路径_版本信息, 文本, Encoding.UTF8)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
        结果.添加_有标签("新版本信息", 文本)
        Return 结果
    End Function

    Private Function 数据库_验证电子邮箱和密码(ByVal 电子邮箱地址 As String, ByVal 密码 As String, ByRef 职能 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("电子邮箱地址", 筛选方式_常量集合.等于, 电子邮箱地址)
            Dim 哈希值计算器 As New SHA1Managed
            Dim 密码哈希值() As Byte = 哈希值计算器.ComputeHash(UTF8.GetBytes(密码))
            列添加器.添加列_用于筛选器("密码哈希值", 筛选方式_常量集合.等于, 密码哈希值)
            哈希值计算器.Dispose()
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("职能")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , "#电子邮箱地址")
            读取器 = 指令.执行()
            While 读取器.读取
                职能 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改邮箱密码() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Password As String = Http请求("Password")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Password) = True Then Return Nothing
        If Password.Length < 12 OrElse Password.Length > 20 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 结果 As 类_SS包生成器
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                结果 = 数据库_添加邮箱密码(Password)
                If 结果.查询结果 = 查询结果_常量集合.成功 Then
                    Dim 邮件 As New 类_邮件
                    邮件.收件人 = 管理员电子邮箱地址
                    邮件.标题 = "邮箱新密码保存成功"
                    邮件.正文 = "邮箱新密码保存成功，发送邮件成功。" & vbCrLf &
                                   "注意：此邮件是系统自动发送的，请勿回复。"
                    结果 = 数据库_保存邮件(邮件)
                End If
            Catch ex As Exception
                结果 = New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            If 结果.查询结果 = 查询结果_常量集合.成功 Then
                邮件管理器.发送邮件()
            End If
            Return 结果
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加邮箱密码(ByVal 密码 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器_新数据.添加列_用于插入数据("文本", 密码)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("类型", 筛选方式_常量集合.等于, 系统任务类型_常量集合.邮箱密码)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "系统任务", 列添加器_新数据, 筛选器)
            If 指令.执行 = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("类型", 系统任务类型_常量集合.邮箱密码)
                列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                列添加器.添加列_用于插入数据("文本", 密码)
                Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "系统任务", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 设置邮箱初始密码() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return "数据库未就绪。"
        Dim Password As String = Http请求("Password")
        If Password.Length < 最小值_常量集合.密码长度 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Dim 结果 As 类_SS包生成器
            Try
                Dim 用户编号 As Long
                结果 = 数据库_检查电子邮箱是否已注册(管理员电子邮箱地址, 用户编号)
                If 结果.查询结果 = 查询结果_常量集合.成功 Then
                    If 用户编号 = 0 Then
                        Dim 目录路径 As String = Context.Server.MapPath("/") & "App_Data\"
                        If File.Exists(目录路径 & "PASSWORDSAVED.txt") = False Then
                            结果 = 数据库_添加邮箱密码(Password)
                            If 结果.查询结果 = 查询结果_常量集合.成功 Then
                                File.WriteAllText(目录路径 & "PASSWORDSAVED.txt", Date.Now.ToString, Encoding.UTF8)
                                Dim 邮件 As New 类_邮件
                                邮件.收件人 = 管理员电子邮箱地址
                                邮件.标题 = "邮箱初始密码保存成功"
                                邮件.正文 = "邮箱初始密码保存成功，发送邮件成功。" & vbCrLf &
                                                  "注意：此邮件是系统自动发送的，请勿回复。"
                                结果 = 数据库_保存邮件(邮件)
                            End If
                        Else
                            Return "无权操作。"
                        End If
                    Else
                        Return "管理员账号已存在。"
                    End If
                End If
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
            If 结果.查询结果 = 查询结果_常量集合.成功 Then
                邮件管理器.发送邮件()
            End If
            Return "成功。"
        Else
            Return Nothing
        End If
    End Function

    Private Function 数据库_检查电子邮箱是否已注册(ByVal 电子邮箱地址 As String, ByRef 编号 As Long) As 类_SS包生成器
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

    '    Public Function 报告程序故障() As 类_SS包生成器
    '        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
    '        Dim UserID As Long
    '        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
    '        Dim Credential As String = Http请求("Credential")

    '        If Http请求.ContentLength = 0 Then Return Nothing
    '        Dim 字节数组(Http请求.ContentLength - 1) As Byte
    '        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)

    '        Dim ErrorMessage As String = UTF8.GetString(字节数组)
    '        If String.IsNullOrEmpty(ErrorMessage) = True Then Return Nothing

    '        Dim 结果 As 类_SS包生成器
    '        Dim 用户信息 As 类_用户信息 = Nothing
    '        If 跨进程锁.WaitOne = True Then
    '            Try
    '                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.电子邮箱地址)
    '                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
    '                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
    '                    Return 结果
    '                End If
    '            Catch ex As Exception
    '                Return New 类_SS包生成器(ex.Message)
    '            Finally
    '                跨进程锁.ReleaseMutex()
    '            End Try
    '        Else
    '            Return New 类_SS包生成器(查询结果_常量集合.失败)
    '        End If
    '        If 结果.查询结果 = 查询结果_常量集合.成功 Then
    '            ErrorMessage = 用户信息.电子邮箱地址 & vbCrLf & ErrorMessage
    '            Dim 文件信息 As New FileInfo(Context.Server.MapPath("/") & "App_Data\Error_" & UserID & ".txt")
    '            If 文件信息.Exists Then
    '                If DateDiff(DateInterval.Hour, 文件信息.LastWriteTimeUtc, Date.UtcNow) < 24 Then
    '                    Return New 类_SS包生成器(查询结果_常量集合.失败)
    '                Else
    '                    Dim 文件内容 As String = File.ReadAllText(文件信息.FullName, Encoding.UTF8)
    '                    If String.Compare(文件内容, ErrorMessage) = 0 Then
    '                        Return New 类_SS包生成器(查询结果_常量集合.成功)
    '                    Else
    '                        GoTo 行1
    '                    End If
    '                End If
    '            Else
    '行1:
    '                Try
    '                    File.WriteAllText(文件信息.FullName, ErrorMessage, Encoding.UTF8)
    '                Catch ex As Exception
    '                    Return New 类_SS包生成器(ex.Message)
    '                End Try
    '                Dim 邮件 As New 类_邮件
    '                邮件.收件人 = 管理员电子邮箱地址
    '                邮件.标题 = "SSignal：程序故障"
    '                邮件.正文 = ErrorMessage
    '                If 跨进程锁.WaitOne = True Then
    '                    Try
    '                        结果 = 数据库_保存邮件(邮件)
    '                    Catch ex As Exception
    '                        Return New 类_SS包生成器(ex.Message)
    '                    Finally
    '                        跨进程锁.ReleaseMutex()
    '                    End Try
    '                    If 结果.查询结果 = 查询结果_常量集合.成功 Then
    '                        邮件管理器.发送邮件()
    '                    End If
    '                Else
    '                    Return New 类_SS包生成器(查询结果_常量集合.失败)
    '                End If
    '            End If
    '        End If
    '        Return 结果
    '    End Function

    Public Function 添加可注册者() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim ID As String = Http请求("ID")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(ID) = True Then Return Nothing
        If ID.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.Compare(ID, "*") <> 0 Then
                    Return 数据库_添加可注册者(ID)
                Else
                    Dim 文件信息 As New FileInfo(Context.Server.MapPath("/") & "App_Data\" & 文件名_注册许可)
                    If 文件信息.Exists = False Then
                        Dim 文件流 As FileStream = 文件信息.Create()
                        文件流.Close()
                    End If
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加可注册者(ByVal ID As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddDays(-3).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "可注册者", 筛选器, "#时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("邮箱或手机号", ID)
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "可注册者", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 移除可注册者() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim ID As String = Http请求("ID")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(ID) = True Then Return Nothing
        If ID.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.Compare(ID, "*") <> 0 Then
                    Return 数据库_移除可注册者(ID)
                Else
                    Dim 文件信息 As New FileInfo(Context.Server.MapPath("/") & "App_Data\" & 文件名_注册许可)
                    If 文件信息.Exists = True Then
                        文件信息.Delete()
                    End If
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_移除可注册者(ByVal ID As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("邮箱或手机号", 筛选方式_常量集合.等于, ID)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "可注册者", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 设置商品编辑者() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(EnglishSSAddress) = True Then Return Nothing
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 密码哈希值() As Byte
                Try
                    密码哈希值 = System.Convert.FromBase64String(Passcode)
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                End Try
                结果 = 数据库_验证密码(UserID, 密码哈希值)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                子域名 = 获取服务器域名(讯宝小宇宙中心服务器主机名 & "." & 域名_英语)
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
        Return 访问其它服务器("https://" & 子域名 & "/?C=SetGoodsEditor&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&EnglishSSAddress=" & 替换URI敏感字符(EnglishSSAddress))
    End Function

End Class
