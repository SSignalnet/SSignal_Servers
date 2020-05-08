Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Private Structure 大聊天群_复合数据
        Dim 主机名, 群名称 As String
        Dim 群编号 As Long
    End Structure

    Public Function 创建大聊天群() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim Name As String = Http请求("Name")
        Dim Number As String = Http请求("Number")
        Dim 估计成员数 As Integer
        If Integer.TryParse(Number, 估计成员数) = False Then Return Nothing

        If UserID < 1 Then Return Nothing
        If String.IsNullOrEmpty(Name) Then Return Nothing
        If 估计成员数 <= 最大值_常量集合.小聊天群成员数量 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 用户信息 As 类_用户信息
        Dim 大聊天群() As 大聊天群_复合数据
        Dim 大聊天群数量 As Integer
        Dim SS包生成器 As 类_SS包生成器
        Dim 主机名 As String = Nothing
        Dim 子域名_大聊天群服务器 As String = Nothing
        Dim 访问本域服务器的凭据 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.用户名和主机名)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                ReDim 大聊天群(最大值_常量集合.每个用户可拥有的大聊天群数量 - 1)
                结果 = 数据库_获取拥有的大聊天群(UserID, 大聊天群, 大聊天群数量)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 大聊天群数量 >= 最大值_常量集合.每个用户可拥有的大聊天群数量 Then
                    结果 = New 类_SS包生成器(查询结果_常量集合.拥有的大聊天群数量已达上限)
                    Dim I As Integer
                    For I = 0 To 大聊天群数量 - 1
                        SS包生成器 = New 类_SS包生成器()
                        With 大聊天群(I)
                            SS包生成器.添加_有标签("主机名", .主机名)
                            SS包生成器.添加_有标签("群编号", .群编号)
                            SS包生成器.添加_有标签("群名称", .群名称)
                        End With
                        结果.添加_有标签("群", SS包生成器)
                    Next
                    Return 结果
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(UserID, 操作次数, 操作代码_常量集合.创建大聊天群)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 1 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                If 估计成员数 <= 最大值_常量集合.大聊天群服务器承载用户数 Then
                    结果 = 数据库_分配大聊天群服务器(估计成员数, 主机名)
                Else
                    结果 = 数据库_分配超级大聊天群服务器(估计成员数, 主机名)
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                子域名_大聊天群服务器 = 获取服务器域名(主机名 & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
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
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim 群编号 As Long
        Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名_大聊天群服务器 & "/?C=CreateGroup&Credential=" & 替换URI敏感字符(访问本域服务器的凭据) & "&English=" & 用户信息.英语用户名 & "&Native=" & 用户信息.本国语用户名 & "&HostName=" & 用户信息.主机名 & "&Position=" & 用户信息.位置号 & "&Name=" & 替换URI敏感字符(Name))
        If TypeOf 访问结果 Is 类_SS包生成器 Then
            Return 访问结果
        Else
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果
            End If
            SS包解读器.读取_有标签("群编号", 群编号)
            If 群编号 <= 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        End If
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_记录拥有的大聊天群(UserID, 主机名, 群编号, Name)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_保存操作记录(UserID, 操作代码_常量集合.创建大聊天群, 获取网络地址字节数组)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
            Return 结果
        End If
        If 大聊天群数量 > 0 Then
            Dim I As Integer
            For I = 0 To 大聊天群数量 - 1
                SS包生成器 = New 类_SS包生成器()
                With 大聊天群(I)
                    SS包生成器.添加_有标签("主机名", .主机名)
                    SS包生成器.添加_有标签("群编号", .群编号)
                    SS包生成器.添加_有标签("群名称", .群名称)
                End With
                结果.添加_有标签("群", SS包生成器)
            Next
        End If
        SS包生成器 = New 类_SS包生成器()
        SS包生成器.添加_有标签("主机名", 主机名)
        SS包生成器.添加_有标签("群编号", 群编号)
        SS包生成器.添加_有标签("群名称", Name)
        结果.添加_有标签("群", SS包生成器)
        Return 结果
    End Function

    Private Function 数据库_获取拥有的大聊天群(ByVal 用户编号 As Long, ByRef 大聊天群() As 大聊天群_复合数据, ByRef 大聊天群数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"主机名", "群编号", "群名称"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群主", 筛选器, , 列添加器, 10, "#用户主机名群")
            读取器 = 指令.执行()
            While 读取器.读取
                With 大聊天群(大聊天群数量)
                    .主机名 = 读取器(0)
                    .群编号 = 读取器(1)
                    .群名称 = 读取器(2)
                End With
                大聊天群数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_分配大聊天群服务器(ByRef 估计成员数 As Integer, ByRef 主机名 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("类别", 筛选方式_常量集合.等于, 服务器类别_常量集合.大聊天群服务器)
            列添加器.添加列_用于筛选器("停用", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"统计", "主机名"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "服务器", 筛选器,  , 列添加器, 100, "#类别统计")
            Dim 统计 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                统计 = 读取器(0)
                If 最大值_常量集合.大聊天群服务器承载用户数 - 统计 >= 估计成员数 Then
                    主机名 = 读取器(1)
                    Exit While
                End If
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(主机名) = False Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.没有可用的大聊天群服务器)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_分配超级大聊天群服务器(ByRef 估计成员数 As Integer, ByRef 主机名 As String) As 类_SS包生成器
        Return New 类_SS包生成器(查询结果_常量集合.没有可用的大聊天群服务器)
    End Function

    Private Function 数据库_记录拥有的大聊天群(ByVal 用户编号 As Long, ByVal 主机名 As String, ByVal 群编号 As Long, ByVal 群名称 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("主机名", 主机名)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("群名称", 群名称)
            Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "群主", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 更换群主() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim HostName As String = Http请求("HostName")
        Dim GroupID As String = Http请求("GroupID")
        Dim 群编号 As Long
        If Long.TryParse(GroupID, 群编号) = False Then Return Nothing
        Dim English As String = Http请求("English")

        If UserID < 1 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 用户信息 As 类_用户信息
        Dim 子域名_大聊天群服务器 As String = Nothing
        Dim 访问本域服务器的凭据 As String = Nothing
        Dim 新群主编号 As Long
        If 跨进程锁.WaitOne = True Then
            Try
                用户信息 = New 类_用户信息(类_用户信息.范围_常量集合.用户名)
                结果 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.Compare(用户信息.英语用户名, English) = 0 Then
                    Return Nothing
                End If
                结果 = 数据库_查找拥有的大聊天群(UserID, HostName, 群编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果 = 数据库_根据英语用户名查找用户(English, 新群主编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 新群主编号 = 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.用户不存在)
                End If
                Dim 数量 As Integer
                结果 = 数据库_统计拥有的大聊天群(新群主编号, 数量)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 数量 >= 最大值_常量集合.每个用户可拥有的大聊天群数量 Then
                    Return New 类_SS包生成器(查询结果_常量集合.拥有的大聊天群数量已达上限)
                End If
                子域名_大聊天群服务器 = 获取服务器域名(HostName & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
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
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名_大聊天群服务器 & "/?C=ChangeOwner&Credential=" & 替换URI敏感字符(访问本域服务器的凭据) & "&GroupID=" & GroupID & "&English=" & English)
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
                Return 数据库_更换群主(UserID, HostName, 群编号, 新群主编号)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_统计拥有的大聊天群(ByVal 用户编号 As Long, ByRef 数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群主", 筛选器, , , 10, "#用户主机名群")
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

    Private Function 数据库_更换群主(ByVal 用户编号 As Long, ByVal 主机名 As String, ByVal 群编号 As Long, ByVal 新群主编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("用户编号", 新群主编号)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群主", 列添加器_新数据, 筛选器, "#用户主机名群")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 解散大聊天群() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim HostName As String = Http请求("HostName")
        Dim GroupID As String = Http请求("GroupID")
        Dim 群编号 As Long
        If Long.TryParse(GroupID, 群编号) = False Then Return Nothing

        If UserID < 1 Then Return Nothing
        Dim 结果 As 类_SS包生成器
        Dim 子域名_大聊天群服务器 As String = Nothing
        Dim 访问本域服务器的凭据 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_验证用户凭据(UserID, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_查找拥有的大聊天群(UserID, HostName, 群编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                子域名_大聊天群服务器 = 获取服务器域名(HostName & "." & 域名_英语)
                结果 = 数据库_获取访问其它服务器的凭据(副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
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
            结果 = 添加我方访问其它服务器的凭据(跨进程锁, 副数据库, 子域名_大聊天群服务器, 访问本域服务器的凭据)
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                Return 结果
            End If
        End If
        Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名_大聊天群服务器 & "/?C=DeleteGroup&Credential=" & 替换URI敏感字符(访问本域服务器的凭据) & "&GroupID=" & GroupID)
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
                Return 数据库_删除拥有的大聊天群(UserID, HostName, 群编号)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_查找拥有的大聊天群(ByVal 用户编号 As Long, ByVal 主机名 As String, ByVal 群编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群主", 筛选器, 1, , , "#用户主机名群")
            Dim 找到了 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                找到了 = True
                Exit While
            End While
            读取器.关闭()
            If 找到了 = True Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除拥有的大聊天群(ByVal 用户编号 As Long, ByVal 主机名 As String, ByVal 群编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "群主", 筛选器, "#用户主机名群")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
