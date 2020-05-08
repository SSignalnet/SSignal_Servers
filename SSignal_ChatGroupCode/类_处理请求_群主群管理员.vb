Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB

Partial Public Class 类_处理请求

    Public Function 创建大聊天群() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        Dim English As String = Http请求("English")
        Dim Native As String = Http请求("Native")
        Dim HostName As String = Http请求("HostName")
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return Nothing
        Dim Name As String = Http请求("Name")

        If String.IsNullOrEmpty(English) Then Return Nothing
        If String.IsNullOrEmpty(域名_本国语) = False Then
            If String.IsNullOrEmpty(Native) Then Return Nothing
        Else
            If String.IsNullOrEmpty(Native) = False Then Return Nothing
        End If
        If String.IsNullOrEmpty(Name) Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 群编号 As Long
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_新大聊天群(English, Native, Name, HostName, Position, 群编号)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If 结果.查询结果 = 查询结果_常量集合.成功 Then
            结果.添加_有标签("群编号", 群编号)
        End If
        Return 结果
    End Function

    Private Function 数据库_新大聊天群(ByVal 英语用户名 As String, ByVal 本国语用户名 As String,
                                ByVal 名称 As String, ByVal 主机名 As String, ByVal 位置号 As Short, ByRef 群编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
跳转点1:
        Try
            Dim 列添加器 As 类_列添加器
            Dim 筛选器 As 类_筛选器
            If 群编号 = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("名称", 筛选方式_常量集合.等于, 名称)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群", 筛选器, 1, 列添加器, , "#名称")
                读取器 = 指令.执行()
                While 读取器.读取
                    群编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 群编号 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                    列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.等于, 群角色_常量集合.群主)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据("英语讯宝地址")
                    指令 = New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群编号角色加入时间")
                    Dim 英语讯宝地址 As String = ""
                    读取器 = 指令.执行()
                    While 读取器.读取
                        英语讯宝地址 = 读取器(0)
                        Exit While
                    End While
                    读取器.关闭()
                    If 英语讯宝地址.StartsWith(英语用户名 & 讯宝地址标识) Then
                        Return New 类_SS包生成器(查询结果_常量集合.成功)
                    Else
                        Return New 类_SS包生成器(查询结果_常量集合.群名称已存在)
                    End If
                End If
                群编号 = Date.UtcNow.Ticks
            Else
                群编号 += 1
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 群编号)
            列添加器.添加列_用于插入数据("名称", 名称)
            列添加器.添加列_用于插入数据("成员数", 1)
            列添加器.添加列_用于插入数据("停用", False)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "群", 列添加器, True)
            指令2.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语用户名 & 讯宝地址标识 & 域名_英语)
            If String.IsNullOrEmpty(域名_本国语) = False Then
                列添加器.添加列_用于插入数据("本国语讯宝地址", 本国语用户名 & 讯宝地址标识 & 域名_本国语)
            End If
            列添加器.添加列_用于插入数据("角色", 群角色_常量集合.群主)
            列添加器.添加列_用于插入数据("加入时间", 群编号)
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            指令2 = New 类_数据库指令_插入新数据(主数据库, "群成员", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            GoTo 跳转点1
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 添加成员() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim MEnglishSSAddress As String = Http请求("MEnglishSSAddress")
        Dim MNativeSSAddress As String = Http请求("MNativeSSAddress")
        Dim CanSpeak As String = Http请求("CanSpeak")
        Dim HostName As String = Http请求("HostName")
        Dim Position As Short
        If Short.TryParse(Http请求("Position"), Position) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 <> 群角色_常量集合.群主 AndAlso 角色 <> 群角色_常量集合.管理员 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Return 数据库_添加成员(GroupID, MEnglishSSAddress, MNativeSSAddress, CanSpeak, HostName, Position)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_添加成员(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String,
                                ByVal 可以发言 As String, ByVal 主机名 As String, ByVal 位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 用户数 As Integer
            Dim 结果2 As 类_SS包生成器 = 数据库_统计已有用户数(用户数)
            If 结果2.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果2
            End If
            If 用户数 >= 最大值_常量集合.大聊天群服务器承载用户数 Then
                Return New 类_SS包生成器(查询结果_常量集合.大聊天群服务器用户数已满)
            End If
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
            列添加器.添加列_用于插入数据("本国语讯宝地址", 本国语讯宝地址)
            If String.Compare(可以发言, "true", True) = 0 Then
                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.邀请加入_可以发言)
            Else
                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.邀请加入_不可发言)
            End If
            列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "群成员", 列添加器, True)
            指令2.执行()
            Dim 运算器 As New 类_运算器("成员数")
            运算器.添加运算指令(运算符_常量集合.加, 1)
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("成员数", 运算器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 删减成员() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim MEnglishSSAddress As String = Http请求("MEnglishSSAddress")

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 <> 群角色_常量集合.群主 AndAlso 角色 <> 群角色_常量集合.管理员 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.删减成员)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 10 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                Dim 本国语讯宝地址 As String = Nothing
                Dim 成员角色 As 群角色_常量集合
                结果 = 数据库_删减成员(GroupID, MEnglishSSAddress, 角色, 本国语讯宝地址, 成员角色)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(GroupID, 操作代码_常量集合.删减成员)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_删减成员(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String, ByVal 操作者角色 As 群角色_常量集合,
                              ByRef 本国语讯宝地址 As String, ByRef 角色 As 群角色_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"角色", "本国语讯宝地址"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群编号英语讯宝地址")
            角色 = 群角色_常量集合.无
            读取器 = 指令2.执行()
            While 读取器.读取
                角色 = 读取器(0)
                本国语讯宝地址 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If 角色 = 群角色_常量集合.无 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            End If
            If 操作者角色 <= 角色 Then
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令3 As New 类_数据库指令_删除数据(主数据库, "群成员", 筛选器, "#群编号英语讯宝地址", True)
            指令3.执行()
            Dim 运算器 As New 类_运算器("成员数")
            运算器.添加运算指令(运算符_常量集合.减, 1)
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("成员数", 运算器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改角色() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim MEnglishSSAddress As String = Http请求("MEnglishSSAddress")
        Dim NewRole As 群角色_常量集合
        If Byte.TryParse(Http请求("NewRole"), NewRole) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Select Case NewRole
            Case 群角色_常量集合.成员_不可发言, 群角色_常量集合.成员_可以发言, 群角色_常量集合.管理员
            Case Else : Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        End Select
        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 <> 群角色_常量集合.群主 AndAlso 角色 <> 群角色_常量集合.管理员 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.修改角色)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 10 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                Dim 本国语讯宝地址 As String = Nothing
                结果 = 数据库_修改角色(GroupID, MEnglishSSAddress, 角色, NewRole, 本国语讯宝地址)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存讯宝(GroupID, EnglishSSAddress, 讯宝指令_常量集合.某人在聊天群的角色改变, 生成文本_某人在大聊天群的角色改变的通知(MEnglishSSAddress, 本国语讯宝地址, NewRole))
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(GroupID, 操作代码_常量集合.修改角色)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_修改角色(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String, ByVal 操作者角色 As 群角色_常量集合,
                              ByVal 新角色 As 群角色_常量集合, ByRef 本国语讯宝地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"角色", "本国语讯宝地址"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群编号英语讯宝地址")
            Dim 当前角色 As 群角色_常量集合 = 群角色_常量集合.无
            读取器 = 指令2.执行()
            While 读取器.读取
                当前角色 = 读取器(0)
                本国语讯宝地址 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If 当前角色 = 新角色 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            End If
            Select Case 当前角色
                Case 群角色_常量集合.无
                    Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                Case 群角色_常量集合.邀请加入_不可发言, 群角色_常量集合.邀请加入_可以发言
                    Return New 类_SS包生成器(查询结果_常量集合.不是正式群成员)
            End Select
            If 操作者角色 <= 当前角色 Then
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
            Dim 列添加器_新数据 As 类_列添加器
            If 操作者角色 = 群角色_常量集合.群主 AndAlso 新角色 = 群角色_常量集合.群主 Then
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("角色", 群角色_常量集合.管理员)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.等于, 群角色_常量集合.群主)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令3 As New 类_数据库指令_更新数据(主数据库, "群成员", 列添加器_新数据, 筛选器, "#群编号角色加入时间", True)
                指令3.执行()
            End If
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("角色", 新角色)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群成员", 列添加器_新数据, 筛选器, "#群编号英语讯宝地址")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改群名称() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim NewName As String = Http请求("NewName")

        If String.IsNullOrEmpty(NewName) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If NewName.Length > 最大值_常量集合.群名称字符数 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 <> 群角色_常量集合.群主 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.修改群名称)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 2 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                结果 = 数据库_修改群名称(GroupID, NewName)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存讯宝(GroupID, EnglishSSAddress, 讯宝指令_常量集合.修改聊天群名称, NewName)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(GroupID, 操作代码_常量集合.修改群名称)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_修改群名称(ByVal 群编号 As Long, ByVal 新名称 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("名称", 新名称)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("名称", 筛选方式_常量集合.不等于, 新名称)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.大聊天群名称已存在)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改群图标() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If Http请求.ContentLength = 0 Then Return Nothing
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        If 字节数组.Length > 2 * 长度_常量集合.图标宽高_像素 * 长度_常量集合.图标宽高_像素 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 <> 群角色_常量集合.群主 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.修改群图标)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 2 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Dim 图标存放目录 As String = Context.Server.MapPath("/") & "icons"
        Dim 图标路径 As String = 图标存放目录 & "\" & GroupID & ".jpg"
        Dim 图标更新时间 As Long
        Try
            If Directory.Exists(图标存放目录) = False Then Directory.CreateDirectory(图标存放目录)
            File.WriteAllBytes(图标路径, 字节数组)
            Dim 文件信息 As New FileInfo(图标路径)
            图标更新时间 = 文件信息.LastWriteTimeUtc.Ticks
        Catch ex As Exception
            Throw New Exception
        End Try
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_保存讯宝(GroupID, EnglishSSAddress, 讯宝指令_常量集合.聊天群图标改变, 图标更新时间)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存操作记录(GroupID, 操作代码_常量集合.修改群图标)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果.添加_有标签("图标更新时间", 图标更新时间)
                Return 结果
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Public Function 更换群主() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim English As String = Http请求("English")

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.修改角色)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 10 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                Dim 英语讯宝地址 As String = English & 讯宝地址标识 & 域名_英语
                Dim 本国语讯宝地址 As String = Nothing
                结果 = 数据库_修改角色(GroupID, 英语讯宝地址, 群角色_常量集合.群主, 群角色_常量集合.群主, 本国语讯宝地址)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存讯宝(GroupID, 英语讯宝地址, 讯宝指令_常量集合.某人在聊天群的角色改变, 生成文本_某人在大聊天群的角色改变的通知(英语讯宝地址, 本国语讯宝地址, 群角色_常量集合.群主))
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(GroupID, 操作代码_常量集合.修改角色)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Public Function 解散大聊天群() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return New 类_SS包生成器(查询结果_常量集合.失败)
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_解散大聊天群(GroupID)
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
        Dim 图标路径 As String = Context.Server.MapPath("/") & "icons\" & GroupID & ".jpg"
        If File.Exists(图标路径) = True Then
            Try
                File.Delete(图标路径)
            Catch ex As Exception
            End Try
        End If
        Return 结果
    End Function

    Private Function 数据库_解散大聊天群(ByVal 群编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "群讯宝统计", 筛选器, 主键索引名)
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(主数据库, "群成员", 筛选器, "#群编号英语讯宝地址")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(主数据库, "群", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
