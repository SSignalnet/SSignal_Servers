Imports System.Net
Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 列出用户() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Range As String = Http请求("Range")
        Dim ID As String = Http请求("ID")
        Dim OrderBy As String = Http请求("OrderBy")
        Dim PageNumber As String = Http请求("PageNumber")
        Dim Passcode As String = Http请求("Passcode")
        Dim TimezoneOffset As String = Http请求("TimezoneOffset")

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Passcode) = True Then Return Nothing
        Dim 身份码类型 As String = Nothing
        If String.Compare(Range, "someone") = 0 Then
            If ID.Contains("@") Then
                If 是否是有效的讯宝或电子邮箱地址(ID) = False Then
                    Return Nothing
                Else
                    身份码类型 = 身份码类型_电子邮箱地址
                End If
            ElseIf IsNumeric(ID) = True Then
                If ID.Length > 最大值_常量集合.手机号字符数 Then
                    Return Nothing
                End If
                身份码类型 = 身份码类型_手机号
            ElseIf 是否是中文用户名(ID) = True Then
                身份码类型 = 身份码类型_本国语用户名
            ElseIf 是否是英文用户名(ID) = True Then
                身份码类型 = 身份码类型_英语用户名
            Else
                Return Nothing
            End If
        End If
        Dim 第几页 As Long
        If Long.TryParse(PageNumber, 第几页) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(TimezoneOffset, 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 用户列表() As 用户2_复合数据
        Dim 用户数 As Short
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
                ReDim 用户列表(每页条数 - 1)
Line1:
                结果 = 数据库_获取用户列表(用户列表, 用户数, Range, ID, 身份码类型, OrderBy, 每页条数, 第几页, 总数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 用户数 = 0 Then
                    If 总数 > 0 Then
                        第几页 = Int(总数 / 每页条数)
                        If 总数 Mod 每页条数 <> 0 Then 第几页 += 1
                        GoTo Line1
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
        Dim 变长文本 As New StringBuilder(300 * 用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<PAGENUMBER>" & 第几页 & "</PAGENUMBER>")
        文本写入器.Write("<TOTALPAGES>" & Math.Ceiling(总数 / 每页条数) & "</TOTALPAGES>")
        文本写入器.Write("<RANGE>" & Range & "</RANGE>")
        文本写入器.Write("<SEARCHID>" & 替换XML敏感字符(ID) & "</SEARCHID>")
        文本写入器.Write("<ORDERBY>" & OrderBy & "</ORDERBY>")
        If 用户数 > 0 Then
            Dim 网络地址 As IPAddress
            Dim I As Short
            For I = 0 To 用户数 - 1
                文本写入器.Write("<USER>")
                With 用户列表(I)
                    文本写入器.Write("<ID>" & .编号 & "</ID>")
                    If String.IsNullOrEmpty(.英语用户名) = False Then
                        文本写入器.Write("<ENGLISH>" & .英语用户名 & "</ENGLISH>")
                    End If
                    If String.IsNullOrEmpty(.本国语用户名) = False Then
                        文本写入器.Write("<NATIVE>" & .本国语用户名 & "</NATIVE>")
                    End If
                    If .停用 = True Then 文本写入器.Write(XML已停用)
                    If .手机号 > 0 Then
                        文本写入器.Write("<PHONE>" & .手机号 & "</PHONE>")
                    End If
                    If String.IsNullOrEmpty(.电子邮箱地址) = False Then
                        文本写入器.Write("<EMAIL>" & 替换XML敏感字符(.电子邮箱地址) & "</EMAIL>")
                    End If
                    If String.IsNullOrEmpty(.职能) = False Then
                        文本写入器.Write("<DUTY>" & .职能 & "</DUTY>")
                    End If
                    If String.IsNullOrEmpty(.主机名) = False Then
                        文本写入器.Write("<HOSTNAME>" & .主机名 & "</HOSTNAME>")
                    End If
                    If .登录时间_电脑 > 0 Then
                        文本写入器.Write("<LOGINDATE_PC>" & Date.FromBinary(.登录时间_电脑).AddMinutes(时区偏移量).ToString & "</LOGINDATE_PC>")
                    End If
                    If .登录时间_手机 > 0 Then
                        文本写入器.Write("<LOGINDATE_MP>" & Date.FromBinary(.登录时间_手机).AddMinutes(时区偏移量).ToString & "</LOGINDATE_MP>")
                    End If
                    If .网络地址_电脑 IsNot Nothing Then
                        网络地址 = New IPAddress(.网络地址_电脑)
                        文本写入器.Write("<IP_PC>" & 网络地址.ToString & "</IP_PC>")
                    End If
                    If .网络地址_手机 IsNot Nothing Then
                        网络地址 = New IPAddress(.网络地址_手机)
                        文本写入器.Write("<IP_MP>" & 网络地址.ToString & "</IP_MP>")
                    End If
                End With
                文本写入器.Write("</USER>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取用户列表(ByRef 用户列表() As 用户2_复合数据, ByRef 用户数 As Short,
                                       ByRef 范围 As String, ByVal 身份码 As String, ByVal 身份码类型 As String,
                                       ByRef 排序方式 As String, ByVal 每页条数 As Integer,
                                       ByVal 第几页 As Long, ByRef 总数 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As 类_列添加器
            Dim 筛选器 As 类_筛选器
            If String.Compare(范围, "someone") = 0 Then
                Dim 索引名称 As String = Nothing
                列添加器 = New 类_列添加器
                Select Case 身份码类型
                    Case 身份码类型_手机号
                        列添加器.添加列_用于筛选器("手机号", 筛选方式_常量集合.等于, Long.Parse(身份码))
                        索引名称 = "#手机号"
                    Case 身份码类型_英语用户名
                        列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 身份码)
                        索引名称 = "#英语用户名"
                    Case 身份码类型_电子邮箱地址
                        列添加器.添加列_用于筛选器("电子邮箱地址", 筛选方式_常量集合.等于, 身份码)
                        索引名称 = "#电子邮箱地址"
                    Case Else
                        If String.IsNullOrEmpty(域名_本国语) Then
                            Return New 类_SS包生成器(查询结果_常量集合.失败)
                        Else
                            If String.Compare(身份码类型, 身份码类型_本国语用户名) = 0 Then
                                列添加器.添加列_用于筛选器("本国语用户名", 筛选方式_常量集合.等于, 身份码)
                                索引名称 = "#本国语用户名"
                            End If
                        End If
                End Select
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                If String.IsNullOrEmpty(域名_本国语) Then
                    列添加器.添加列_用于获取数据(New String() {"编号", "停用", "主机名", "手机号", "电子邮箱地址", "职能", "英语用户名"})
                Else
                    列添加器.添加列_用于获取数据(New String() {"编号", "停用", "主机名", "手机号", "电子邮箱地址", "职能", "英语用户名", "本国语用户名"})
                End If
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, , 列添加器, 每页条数, 索引名称)
                Dim 起始处 As Long = (第几页 - 1) * 每页条数
                总数 = 0
                用户数 = 0
                读取器 = 指令.执行()
                While 读取器.读取
                    总数 += 1
                    If 用户数 < 每页条数 Then
                        If 总数 > 起始处 Then
                            With 用户列表(用户数)
                                .编号 = 读取器(0)
                                .停用 = 读取器(1)
                                .主机名 = 读取器(2)
                                .手机号 = 读取器(3)
                                .电子邮箱地址 = 读取器(4)
                                .职能 = 读取器(5)
                                .英语用户名 = 读取器(6)
                                If String.IsNullOrEmpty(域名_本国语) = False Then
                                    .本国语用户名 = 读取器(7)
                                End If
                            End With
                            用户数 += 1
                        End If
                    End If
                End While
                读取器.关闭()
            Else
                Dim 索引名称 As String
                Dim 反向 As Boolean
                Select Case 范围
                    Case "disabled"
                        索引名称 = "#编号_已停用"
                        Select Case 排序方式
                            Case "bh_s" : 反向 = True
                            Case Else : 排序方式 = "bh_j"
                        End Select
                    Case "hasduty"
                        索引名称 = "#职能编号"
                        Select Case 排序方式
                            Case "bh_s" : 反向 = True
                            Case Else : 排序方式 = "bh_j"
                        End Select
                    Case Else
                        范围 = "all"
                        索引名称 = 主键索引名
                        Select Case 排序方式
                            Case "bh_s"
                            Case Else
                                反向 = True
                                排序方式 = "bh_j"
                        End Select
                End Select
                列添加器 = New 类_列添加器
                If String.IsNullOrEmpty(域名_本国语) Then
                    列添加器.添加列_用于获取数据(New String() {"编号", "停用", "主机名", "手机号", "电子邮箱地址", "职能", "英语用户名"})
                Else
                    列添加器.添加列_用于获取数据(New String() {"编号", "停用", "主机名", "手机号", "电子邮箱地址", "职能", "英语用户名", "本国语用户名"})
                End If
                Dim 指令2 As New 类_数据库指令_请求快速获取分页数据(主数据库, "用户", 索引名称, Nothing, 反向, 列添加器, 第几页, 每页条数)
                用户数 = 0
                Dim 读取器2 As 类_读取器_快速分页 = 指令2.执行()
                总数 = 读取器2.记录总数量
                If String.IsNullOrEmpty(域名_本国语) Then
                    While 读取器2.读取
                        With 用户列表(用户数)
                            .编号 = 读取器2(0)
                            .停用 = 读取器2(1)
                            .主机名 = 读取器2(2)
                            .手机号 = 读取器2(3)
                            .电子邮箱地址 = 读取器2(4)
                            .职能 = 读取器2(5)
                            .英语用户名 = 读取器2(6)
                        End With
                        用户数 += 1
                    End While
                Else
                    While 读取器2.读取
                        With 用户列表(用户数)
                            .编号 = 读取器2(0)
                            .停用 = 读取器2(1)
                            .主机名 = 读取器2(2)
                            .手机号 = 读取器2(3)
                            .电子邮箱地址 = 读取器2(4)
                            .职能 = 读取器2(5)
                            .英语用户名 = 读取器2(6)
                            .本国语用户名 = 读取器2(7)
                        End With
                        用户数 += 1
                    End While
                End If
            End If
            Dim I As Integer
            For I = 0 To 用户数 - 1
                With 用户列表(I)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"登录时间_电脑", "登录时间_手机", "网络地址_电脑", "网络地址_手机"})
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        .登录时间_电脑 = 读取器(0)
                        .登录时间_手机 = 读取器(1)
                        .网络地址_电脑 = 读取器(2)
                        .网络地址_手机 = 读取器(3)
                        Exit While
                    End While
                    读取器.关闭()
                End With
            Next
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改职能() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 用户编号 As Long
        If Long.TryParse(Http请求("Who"), 用户编号) = False Then Return Nothing
        Dim Duty As String = Http请求("Duty")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If 用户编号 <= 0 Then Return Nothing
        If String.IsNullOrEmpty(Duty) = False Then
            If Duty.Length > 最大值_常量集合.职能字符串长度 Then Return Nothing
        End If
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
                        Return XML不正确
                    End If
                End If
                结果 = 数据库_修改职能(用户编号, Duty)
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

    Private Function 数据库_修改职能(ByVal 用户编号 As Long, ByVal 新职能 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 原职能 As String = ""
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("职能")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                原职能 = 读取器(0)
                If 原职能 Is Nothing Then 原职能 = ""
                Exit While
            End While
            读取器.关闭()
            If String.Compare(原职能, 新职能) <> 0 Then
                If 原职能.Contains(职能_管理员) Then
                    If 新职能.Contains(职能_管理员) = False Then
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                    End If
                End If
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("职能", IIf(String.IsNullOrEmpty(新职能), Nothing, 新职能))
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
                If 指令2.执行 > 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Else
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 停用启用账户() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 用户编号 As Long
        If Long.TryParse(Http请求("Who"), 用户编号) = False Then Return Nothing
        If 用户编号 = UserID Then Return "<CANNOTDISABLE/>"
        Dim Disable_Enable As String = Http请求("Disable_Enable")
        Dim Passcode As String = Http请求("Passcode")

        If UserID < 1 Then Return Nothing
        If 用户编号 <= 0 Then Return Nothing
        Dim 服务器连接凭据 As String = Nothing
        Dim 子域名 As String = Nothing
        Dim 位置号 As Short
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
                        Return XML不正确
                    End If
                End If
                Dim 停用 As Boolean
                If String.Compare(Disable_Enable, "disable") = 0 Then 停用 = True
                If 停用 = False Then
                    结果 = 数据库_停用启用账号(用户编号, False)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        If 结果.查询结果 = 查询结果_常量集合.出错 Then
                            Return XML错误信息(结果.出错提示文本)
                        Else
                            Return XML失败
                        End If
                    End If
                    Return XML成功
                End If
                Dim 主机名 As String = Nothing
                结果 = 数据库_获取传送服务器(用户编号, 主机名, 位置号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return XML错误信息(结果.出错提示文本)
                End If
                If String.IsNullOrEmpty(主机名) = False Then
                    子域名 = 获取服务器域名(主机名 & "." & 域名_英语）
                    结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名, 服务器连接凭据)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return XML错误信息(结果.出错提示文本)
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
        If String.IsNullOrEmpty(服务器连接凭据) = False Then
            Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名 & "/?C=UserOnOrOff&Credential=" & 替换URI敏感字符(服务器连接凭据) & "&UserID=" & 用户编号 & "&Position=" & 位置号)
            If TypeOf 访问结果 Is 类_SS包生成器 Then
                Dim 结果 As 类_SS包生成器 = 访问结果
                If 结果.查询结果 = 查询结果_常量集合.出错 Then
                    Return XML错误信息(结果.出错提示文本)
                Else
                    Return XML失败
                End If
            Else
                Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
                If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                    If SS包解读器.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(SS包解读器.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
            End If
        End If
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_停用启用账号(用户编号, True)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
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
        Return XML成功
    End Function

    Private Function 数据库_停用启用账号(ByVal 用户编号 As Long, ByVal 停用 As Boolean) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("停用", 停用)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("停用", 筛选方式_常量集合.不等于, 停用)
            If 停用 = True Then
                列添加器.添加列_用于筛选器("职能", 筛选方式_常量集合.为空, Nothing)
            End If
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

End Class
