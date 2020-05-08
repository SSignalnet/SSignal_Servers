Imports System.Threading
Imports System.Web
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_讯宝管理器

    Public Function 收到讯宝(ByVal Context As HttpContext, ByVal 传送服务器子域名 As String, ByVal 传送服务器凭据 As String,
                         ByVal 发送者英语讯宝地址 As String, ByVal 发送序号 As Long, ByVal 群主英语讯宝地址 As String,
                         ByVal 群编号 As Byte, ByVal 接收者数组() As 接收者_复合数据, ByVal 讯宝指令 As 讯宝指令_常量集合,
                         ByVal 讯宝文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte) As Object
        Dim 段2() As String = 发送者英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Select Case 段2(1)
            Case 域名_英语, 域名_本国语
            Case Else
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.域内自定义二级讯宝指令集1, 讯宝指令_常量集合.域内自定义二级讯宝指令集2,
                            讯宝指令_常量集合.域内自定义二级讯宝指令集3, 讯宝指令_常量集合.域内自定义二级讯宝指令集4
                        Return Nothing
                End Select
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
                Dim 访问结果 As Object = 访问其它服务器(获取路径_验证服务器真实性(传送服务器子域名, 传送服务器凭据, 本服务器主机名 & "." & 域名_英语))
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
        Dim I, J As Integer
        For I = 0 To 接收者数组.Length - 1
            With 接收者数组(I)
                Dim 接收者 As 类_用户 = 用户目录(.位置号)
                Dim 段() As String = .讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                Select Case 段(1)
                    Case 域名_英语
                        If String.Compare(接收者.英语用户名, 段(0)) <> 0 Then
                            .位置号 = -1
                            J += 1
                        End If
                    Case 域名_本国语
                        If String.Compare(接收者.本国语用户名, 段(0)) <> 0 Then
                            .位置号 = -1
                            J += 1
                        End If
                    Case Else
                        .位置号 = -1
                        J += 1
                End Select
            End With
        Next
        If J > 0 Then
            If J < 接收者数组.Length Then
                Dim 接收者数组2(接收者数组.Length - J - 1) As 接收者_复合数据
                J = 0
                For I = 0 To 接收者数组.Length - 1
                    If 接收者数组(I).位置号 >= 0 Then
                        接收者数组2(J) = 接收者数组(I)
                        J += 1
                    End If
                Next
                接收者数组 = 接收者数组2
            Else
                Return New 类_SS包生成器(查询结果_常量集合.讯宝地址不存在)
            End If
        End If
        If 群编号 = 0 Then
            If 接收者数组.Length = 1 Then
                Dim 接收者 As 类_用户 = 用户目录(接收者数组(0).位置号)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 发送者 As 类_讯友 = Nothing
                        结果 = 数据库_查找讯友(接收者.用户编号, 发送者英语讯宝地址, 发送者)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 发送者 Is Nothing Then
                            If 讯宝指令 = 讯宝指令_常量集合.邀请加入小聊天群 Then
                                Return New 类_SS包生成器(查询结果_常量集合.被邀请加入小聊天群者未添加我为讯友)
                            End If
                            Select Case 讯宝指令
                                Case 讯宝指令_常量集合.发送文字, 讯宝指令_常量集合.邀请加入大聊天群
                                    Dim 段() As String = 发送者英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                                    Dim 是白域, 是黑域 As Boolean
                                    结果 = 数据库_是否白域(接收者.用户编号, 段(1), 是白域)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果
                                    End If
                                    If 是白域 = False Then
                                        结果 = 数据库_是否黑域(接收者.用户编号, 段(1), 是黑域)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                            Return 结果
                                        End If
                                    End If
                                    If 是白域 = False AndAlso 是黑域 = True Then
                                        If 讯宝指令 <> 讯宝指令_常量集合.邀请加入大聊天群 Then
                                            Return New 类_SS包生成器(查询结果_常量集合.对方未添加我为讯友)
                                        Else
                                            Return New 类_SS包生成器(查询结果_常量集合.被邀请加入大聊天群者未添加我为讯友)
                                        End If
                                    End If
                                Case Else
                                    Return New 类_SS包生成器(查询结果_常量集合.对方未添加我为讯友)
                            End Select
                        ElseIf 发送者.拉黑 = True Then
                            Return New 类_SS包生成器(查询结果_常量集合.已被对方拉黑)
                        End If
                        结果 = 数据库_统计接收次数(发送者英语讯宝地址)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        结果 = 数据库_统计个人接收次数(接收者.用户编号, 接收者数组(0).位置号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        If 接收者.网络连接器_手机 IsNot Nothing OrElse 接收者.网络连接器_电脑 IsNot Nothing Then
                            结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 发送序号, Nothing, 0, 接收者.用户编号, 接收者数组(0).位置号, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                        Else
                            结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 发送序号, Nothing, 0, 接收者.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者)
                        End If
                    Catch ex As Exception
                        Return New 类_SS包生成器(ex.Message)
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Else
                Return Nothing
            End If
        Else
            If 接收者数组.Length > 1 Then
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_统计接收次数(发送者英语讯宝地址)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Return 结果
                        End If
                        Dim 接收者 As 类_用户
                        Dim 群主 As 类_讯友
                        Dim 验证了群主 As Boolean
                        Dim SS包生成器 As New 类_SS包生成器()
                        For I = 0 To 接收者数组.Length - 1
                            With 接收者数组(I)
                                接收者 = 用户目录(.位置号)
                                群主 = Nothing
                                结果 = 数据库_查找讯友(接收者.用户编号, 群主英语讯宝地址, 群主)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Throw New Exception
                                End If
                                If 群主 Is Nothing Then
                                    Continue For
                                End If
                                If 验证了群主 = False Then
                                    Dim 段() As String = 群主.英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                                    If String.Compare(传送服务器子域名, 群主.主机名 & "." & 段(1)) <> 0 Then Return Nothing
                                    验证了群主 = True
                                End If
                                If 讯宝指令 = 讯宝指令_常量集合.修改聊天群名称 AndAlso 群编号 > 0 Then
                                    结果 = 数据库_修改群备注(接收者.用户编号, 群主英语讯宝地址, 群编号, 讯宝文本)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                End If
                                结果 = 数据库_统计个人接收次数(接收者.用户编号, .位置号)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                If 接收者.网络连接器_手机 IsNot Nothing OrElse 接收者.网络连接器_电脑 IsNot Nothing Then
                                    Dim SS包生成器2 As New 类_SS包生成器()
                                    SS包生成器2.添加_有标签("用户编号", 接收者.用户编号)
                                    SS包生成器2.添加_有标签("位置号", .位置号)
                                    SS包生成器.添加_有标签("群成员", SS包生成器2)
                                Else
                                    结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 发送序号, 群主英语讯宝地址, 群编号, 接收者.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者)
                                End If
                            End With
                        Next
                        If SS包生成器.数据量 > 0 Then
                            结果 = 数据库_存为推送的讯宝2(发送者英语讯宝地址, 0, 发送序号, 群主英语讯宝地址, 群编号, SS包生成器.生成SS包, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                        End If
                    Catch ex As Exception
                        Return New 类_SS包生成器(ex.Message)
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Else
                If String.Compare(群主英语讯宝地址, 接收者数组(0).讯宝地址) = 0 Then
                    Dim 接收者 As 类_用户 = 用户目录(接收者数组(0).位置号)
                    If 跨进程锁.WaitOne = True Then
                        Try
                            Dim 发送者 As 类_讯友 = Nothing
                            结果 = 数据库_查找讯友(接收者.用户编号, 发送者英语讯宝地址, 发送者)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            If 发送者 Is Nothing Then Return Nothing
                            结果 = 数据库_统计接收次数(发送者英语讯宝地址)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_统计个人接收次数(接收者.用户编号, 接收者数组(0).位置号)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            Dim 加入者本国语讯宝地址 As String = Nothing
                            Dim 加入者主机名 As String = Nothing
                            Dim 加入者位置号 As Short
                            Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                            结果 = 数据库_获取加入者信息(接收者.用户编号, 群编号, 发送者英语讯宝地址, 加入者本国语讯宝地址, 加入者主机名, 加入者位置号, 角色)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            If 角色 = 群角色_常量集合.无 Then
                                If 讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then
                                    Return New 类_SS包生成器(查询结果_常量集合.某人离开了小聊天群)
                                Else
                                    Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                                End If
                            End If
                            Dim 群成员() As 小聊天群成员_复合数据 = Nothing
                            Dim 群成员数, 群正式成员数 As Short
                            结果 = 数据库_发送讯宝时获取群成员(接收者.用户编号, 群编号, 群成员, 群成员数)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            Dim 发送给发送者 As Boolean
                            Dim 当前时刻 As Long = Date.UtcNow.Ticks
                            If 讯宝指令 = 讯宝指令_常量集合.获取小聊天群成员列表 Then
                                If 角色 = 群角色_常量集合.邀请加入_可以发言 Then
                                    结果 = 数据库_成为群成员(接收者.用户编号, 群编号, 发送者英语讯宝地址)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                    For I = 0 To 群成员数 - 1
                                        If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                            群成员(I).角色 = 群角色_常量集合.成员_可以发言
                                            Exit For
                                        End If
                                    Next
                                Else
                                    For I = 0 To 群成员数 - 1
                                        If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                结果 = 数据库_保存要发送的讯宝(当前时刻, 群主英语讯宝地址, 接收者.用户编号, 接收者数组(0).位置号, 接收者.讯宝群消息序号, 发送者英语讯宝地址, 群成员(I).主机名, 群成员(I).位置号, 群主英语讯宝地址, 群编号, 讯宝指令, 生成文本_获取小聊天群成员列表(群成员, 群成员数), 宽度, 高度, 秒数, 设备类型_常量集合.全部)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                If 接收者.讯宝群消息序号 > Long.MinValue Then
                                    接收者.讯宝群消息序号 -= 1
                                Else
                                    接收者.讯宝群消息序号 = -1
                                End If
                                If 角色 = 群角色_常量集合.邀请加入_可以发言 Then
                                    讯宝指令 = 讯宝指令_常量集合.某人加入聊天群
                                    With 群成员(I)
                                        讯宝文本 = 生成文本_某人加入小聊天群的通知(发送者英语讯宝地址, .本国语讯宝地址, 本服务器主机名, .位置号)
                                    End With
                                    发送给发送者 = True
                                Else
                                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                                End If
                            End If
                            For I = 0 To 群成员数 - 1
                                If 群成员(I).角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                    群正式成员数 += 1
                                End If
                            Next
                            If 群正式成员数 > 1 Then
                                For I = 0 To 群成员数 - 1
                                    If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                        If 群成员(I).角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                            Exit For
                                        Else
                                            GoTo 跳转点3
                                        End If
                                    End If
                                Next
                                If I < 群成员数 Then
                                    Select Case 讯宝指令
                                        Case 讯宝指令_常量集合.退出小聊天群
                                            结果 = 数据库_删除群成员(接收者.用户编号, 群编号, 发送者英语讯宝地址)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                            讯宝文本 = 生成文本_某人离开聊天群的通知(发送者英语讯宝地址, 群成员(I).本国语讯宝地址)
                                            发送给发送者 = True
                                    End Select
                                Else
跳转点3:
                                    If 讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then
                                        Return New 类_SS包生成器(查询结果_常量集合.某人离开了小聊天群)
                                    Else
                                        Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                                    End If
                                End If
                                Dim SS包生成器_本服务器群成员 As New 类_SS包生成器(, 群正式成员数)
                                Dim 接收者服务器(群正式成员数 - 1) As 接收者服务器_复合数据
                                Dim 接收者服务器数量 As Integer
                                Dim 接收者服务器子域名, 段() As String
                                Dim 接收者编号_本服务器 As Long
                                Dim 接收者位置号_本服务器 As Short
                                Dim 某一群成员 As 小聊天群成员_复合数据
                                Dim 标识加域名 As String = 讯宝地址标识 & 域名_英语
                                For I = 0 To 群成员数 - 1
                                    某一群成员 = 群成员(I)
                                    If 某一群成员.角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                        If String.Compare(某一群成员.英语讯宝地址, 发送者英语讯宝地址) <> 0 OrElse 发送给发送者 Then
                                            If 某一群成员.英语讯宝地址.EndsWith(标识加域名) AndAlso
                                            String.Compare(某一群成员.主机名, 本服务器主机名) = 0 Then
                                                Dim 接收者2 As 类_用户 = 用户目录(某一群成员.位置号)
                                                If 某一群成员.英语讯宝地址.StartsWith(接收者2.英语用户名 & 讯宝地址标识) Then
                                                    If 接收者.网络连接器_手机 IsNot Nothing OrElse 接收者.网络连接器_电脑 IsNot Nothing Then
                                                        Dim SS包生成器 As New 类_SS包生成器()
                                                        SS包生成器.添加_有标签("用户编号", 接收者2.用户编号)
                                                        SS包生成器.添加_有标签("位置号", 某一群成员.位置号)
                                                        SS包生成器_本服务器群成员.添加_有标签("群成员", SS包生成器)
                                                        If SS包生成器_本服务器群成员.数据量 = 1 Then
                                                            接收者编号_本服务器 = 接收者2.用户编号
                                                            接收者位置号_本服务器 = 某一群成员.位置号
                                                        End If
                                                    Else
                                                        结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 发送序号, 群主英语讯宝地址, 群编号, 接收者.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                    End If
                                                End If
                                            Else
                                                Dim SS包生成器 As New 类_SS包生成器()
                                                SS包生成器.添加_有标签("讯宝地址", 某一群成员.英语讯宝地址)
                                                SS包生成器.添加_有标签("位置号", 某一群成员.位置号)
                                                段 = 某一群成员.英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                                                接收者服务器子域名 = 某一群成员.主机名 & "." & 段(1)
                                                For J = 0 To 接收者服务器数量 - 1
                                                    If String.Compare(接收者服务器(J).子域名, 接收者服务器子域名) = 0 Then Exit For
                                                Next
                                                If J < 接收者服务器数量 Then
                                                    接收者服务器(J).SS包生成器.添加_有标签("群成员", SS包生成器)
                                                Else
                                                    With 接收者服务器(接收者服务器数量)
                                                        .子域名 = 接收者服务器子域名
                                                        .SS包生成器 = New 类_SS包生成器(, 群成员数)
                                                        .SS包生成器.添加_有标签("群成员", SS包生成器)
                                                        If .SS包生成器.数据量 = 1 Then
                                                            .讯宝地址 = 某一群成员.英语讯宝地址
                                                            .位置号 = 某一群成员.位置号
                                                            .主机名 = 某一群成员.主机名
                                                        End If
                                                    End With
                                                    接收者服务器数量 += 1
                                                End If
                                            End If
                                        End If
                                    End If
                                Next
                                If 接收者服务器数量 > 0 Then
                                    For I = 0 To 接收者服务器数量 - 1
                                        当前时刻 += 1
                                        With 接收者服务器(I)
                                            If .SS包生成器.数据量 > 1 Then
                                                结果 = 数据库_保存要发送的讯宝2(当前时刻, 发送者英语讯宝地址, 0, 0, 发送序号, .SS包生成器.生成SS包, .主机名, 群主英语讯宝地址, 群编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 设备类型_常量集合.全部)
                                            Else
                                                结果 = 数据库_保存要发送的讯宝(当前时刻, 发送者英语讯宝地址, 0, 0, 发送序号, .讯宝地址, .主机名, .位置号, 群主英语讯宝地址, 群编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 设备类型_常量集合.全部)
                                            End If
                                        End With
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                    Next
                                End If
                                Dim 数据量 As Integer = SS包生成器_本服务器群成员.数据量
                                If 数据量 > 1 Then
                                    结果 = 数据库_存为推送的讯宝2(发送者英语讯宝地址, 0, 发送序号, 群主英语讯宝地址, 群编号, SS包生成器_本服务器群成员.生成SS包, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                                ElseIf 数据量 = 1 Then
                                    结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 发送序号, 群主英语讯宝地址, 群编号, 接收者编号_本服务器, 接收者位置号_本服务器, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                                End If
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            Else
                                Return Nothing
                            End If
                        Catch ex As Exception
                            Return New 类_SS包生成器(ex.Message)
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                    End If
                Else
                    Dim 接收者 As 类_用户 = 用户目录(接收者数组(0).位置号)
                    If 跨进程锁.WaitOne = True Then
                        Try
                            Dim 群主 As 类_讯友 = Nothing
                            结果 = 数据库_查找讯友(接收者.用户编号, 群主英语讯宝地址, 群主)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            If 群主 Is Nothing Then Return Nothing
                            Dim 段() As String = 群主.英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                            If String.Compare(传送服务器子域名, 群主.主机名 & "." & 段(1)) <> 0 Then Return Nothing
                            结果 = 数据库_统计接收次数(发送者英语讯宝地址)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            结果 = 数据库_统计个人接收次数(接收者.用户编号, 接收者数组(0).位置号)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                Return 结果
                            End If
                            If 讯宝指令 = 讯宝指令_常量集合.修改聊天群名称 AndAlso 群编号 > 0 Then
                                结果 = 数据库_修改群备注(接收者.用户编号, 群主英语讯宝地址, 群编号, 讯宝文本)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                            End If
                            If 接收者.网络连接器_手机 IsNot Nothing OrElse 接收者.网络连接器_电脑 IsNot Nothing Then
                                结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 发送序号, 群主英语讯宝地址, 群编号, 接收者.用户编号, 接收者数组(0).位置号, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                            Else
                                结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 发送序号, 群主英语讯宝地址, 群编号, 接收者.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者)
                            End If
                        Catch ex As Exception
                            Return New 类_SS包生成器(ex.Message)
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                    End If
                End If
            End If
        End If
        Return 结果
    End Function

    Private Function 数据库_统计接收次数(ByVal 发送者英语讯宝地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 接收统计 As 接收统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "接收统计", 筛选器, 1, , 100, 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                接收统计.今日几号 = 读取器(1)
                接收统计.今日接收 = 读取器(2)
                接收统计.今日几时 = 读取器(3)
                接收统计.时段接收 = 读取器(4)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CByte(当前时间.Hour)
        If 接收统计.今日几号 > 0 Then
            If 今日几号 = 接收统计.今日几号 Then
                If 接收统计.今日接收 >= 最大值_常量集合.每人每天可发送讯宝数量 Then
                    Return New 类_SS包生成器(查询结果_常量集合.今日发送的讯宝数量已达上限)
                End If
                If 接收统计.时段接收 >= 最大值_常量集合.每人每小时可发送讯宝数量 Then
                    Return New 类_SS包生成器(查询结果_常量集合.本小时发送的讯宝数量已达上限)
                End If
                If 接收统计.今日几时 = 今日几时 Then
                    Return 数据库_更新接收统计(发送者英语讯宝地址, 今日几号, 接收统计.今日接收 + 1, 今日几时, 接收统计.时段接收 + 1)
                Else
                    Return 数据库_更新接收统计(发送者英语讯宝地址, 今日几号, 接收统计.今日接收 + 1, 今日几时, 1)
                End If
            Else
                Return 数据库_更新接收统计(发送者英语讯宝地址, 今日几号, 1, 今日几时, 1)
            End If
        Else
            Return 数据库_添加接收统计(发送者英语讯宝地址, 今日几号, 1, 今日几时, 1)
        End If
    End Function

    Private Function 数据库_统计个人接收次数(ByVal 接收者用户编号 As Long, ByVal 接收者位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 收发统计 As 收发统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 接收者用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "收发统计", 筛选器, 1, , , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                收发统计.今日几号 = 读取器(2)
                收发统计.今日发送 = 读取器(3)
                收发统计.今日接收 = 读取器(4)
                收发统计.昨日发送 = 读取器(5)
                收发统计.昨日接收 = 读取器(6)
                收发统计.前日发送 = 读取器(7)
                收发统计.前日接收 = 读取器(8)
                收发统计.今日几时 = 读取器(9)
                收发统计.时段发送 = 读取器(10)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CByte(当前时间.Hour)
        If 收发统计.今日几号 > 0 Then
            If 今日几号 = 收发统计.今日几号 Then
                If 收发统计.今日几时 = 今日几时 Then
                    Return 数据库_更新今日收发统计(接收者用户编号, 收发统计.今日发送, 收发统计.今日接收 + 1, 今日几时, 收发统计.时段发送)
                Else
                    Return 数据库_更新今日收发统计(接收者用户编号, 收发统计.今日发送, 收发统计.今日接收 + 1, 今日几时, 0)
                End If
            Else
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                If 昨日几号 = 收发统计.今日几号 Then
                    Return 数据库_更新收发统计(接收者用户编号, 今日几号, 0, 1, 收发统计.今日发送, 收发统计.今日接收, 收发统计.昨日发送, 收发统计.昨日接收, 今日几时, 0)
                Else
                    Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                    Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                    If 前日几号 = 收发统计.今日几号 Then
                        Return 数据库_更新收发统计(接收者用户编号, 今日几号, 0, 1, 0, 0, 收发统计.今日发送, 收发统计.今日接收, 今日几时, 0)
                    Else
                        Return 数据库_更新收发统计(接收者用户编号, 今日几号, 0, 1, 0, 0, 0, 0, 今日几时, 0)
                    End If
                End If
            End If
        Else
            Return 数据库_添加收发统计(接收者用户编号, 接收者位置号, 今日几号, 0, 1, 今日几时, 0)
        End If
    End Function

    Friend Function 数据库_存为推送的讯宝(ByVal 发送者英语地址 As String, ByVal 同服发送者编号 As Long,
                                 ByVal 发送序号 As Long, ByVal 群主英语地址 As String, ByVal 群编号 As Byte,
                                 ByVal 接收者编号 As Long, ByVal 位置号 As Short, ByVal 设备类型 As 设备类型_常量集合,
                                 ByVal 讯宝指令 As 讯宝指令_常量集合, Optional ByVal 文本 As String = Nothing,
                                 Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0,
                                 Optional ByVal 秒数 As Byte = 0) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("时间", 0)
                列添加器.添加列_用于插入数据("接收者", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("同服发送者编号", 同服发送者编号)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            If String.IsNullOrEmpty(群主英语地址) = False Then
                列添加器.添加列_用于插入数据("群主英语地址", 群主英语地址)
            End If
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("接收者编号", 接收者编号)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("设备类型", 设备类型)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝推送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_存为推送的讯宝2(ByVal 发送者英语地址 As String, ByVal 同服发送者编号 As Long,
                                 ByVal 发送序号 As Long, ByVal 群主英语地址 As String, ByVal 群编号 As Byte,
                                 ByVal 群接收者() As Byte, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String,
                                 Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0, Optional ByVal 秒数 As Byte = 0) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("时间", 0)
                列添加器.添加列_用于插入数据("接收者", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("同服发送者编号", 同服发送者编号)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            If String.IsNullOrEmpty(群主英语地址) = False Then
                列添加器.添加列_用于插入数据("群主英语地址", 群主英语地址)
            End If
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("接收者编号", 0)
            列添加器.添加列_用于插入数据("位置号", 0)
            列添加器.添加列_用于插入数据("群接收者", 群接收者)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("设备类型", 设备类型_常量集合.全部)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝推送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_存为不推送的讯宝(ByVal 时间 As Long, ByVal 发送者英语地址 As String,
                                 ByVal 发送序号 As Long, ByVal 群主英语地址 As String, ByVal 群编号 As Byte,
                                 ByVal 接收者 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String,
                                 ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 用户 As 类_用户) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("时间", 时间)
                列添加器.添加列_用于插入数据("接收者", 接收者)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", 时间)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            If String.IsNullOrEmpty(群主英语地址) = False Then
                列添加器.添加列_用于插入数据("群主英语地址", 群主英语地址)
            End If
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("接收者编号", 接收者)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            Dim 接收结果 As 讯宝接收结果_常量集合
            If 用户.讯宝序号_手机发送 > 0 Then
                If 用户.讯宝序号_电脑发送 > 0 Then
                    接收结果 = 讯宝接收结果_常量集合.待确认
                Else
                    接收结果 = 讯宝接收结果_常量集合.电脑端接收成功
                End If
            Else
                If 用户.讯宝序号_电脑发送 > 0 Then
                    接收结果 = 讯宝接收结果_常量集合.手机端接收成功
                Else
                    接收结果 = 讯宝接收结果_常量集合.待确认
                End If
            End If
            列添加器.添加列_用于插入数据("结果", 接收结果)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝不推送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 分配讯宝推送任务()
        Dim 结果 As 类_SS包生成器
        Dim I As Integer
        Do
            Try
                If 跨进程锁.WaitOne = True Then
                    Try
                        If 要推送的讯宝数量 > 0 Then
                            For I = 0 To 要推送的讯宝数量 - 1
                                If 要推送的讯宝(I).当前状态 = 讯宝推送状态_常量集合.结束 Then
                                    Exit For
                                End If
                            Next
                            If I < 要推送的讯宝数量 Then
                                Dim 要推送的讯宝2(要推送的讯宝数量 - 1) As 类_要推送的讯宝
                                Dim 要推送的讯宝数量2 As Integer = 0
                                For I = 0 To 要推送的讯宝数量 - 1
                                    With 要推送的讯宝(I)
                                        If .当前状态 = 讯宝推送状态_常量集合.结束 Then
                                            If .不删除 = False Then
                                                If .同服发送者编号 > 0 Then
                                                    Select Case .讯宝指令
                                                        Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                                            结果 = 数据库_记录带文件的讯宝(.同服发送者编号, .讯宝指令, .文本)
                                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Continue Do
                                                        Case 讯宝指令_常量集合.发送文件
                                                            Dim SS包解读器2 As New 类_SS包解读器
                                                            SS包解读器2.解读纯文本(.文本)
                                                            Dim 存储文件名 As String = ""
                                                            SS包解读器2.读取_有标签("S", 存储文件名)
                                                            结果 = 数据库_记录带文件的讯宝(.同服发送者编号, .讯宝指令, 存储文件名)
                                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Continue Do
                                                    End Select
                                                End If
                                                结果 = 数据库_删除推送成功或失败的讯宝(要推送的讯宝(I))
                                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Continue Do
                                            End If
                                        Else
                                            要推送的讯宝2(要推送的讯宝数量2) = 要推送的讯宝(I)
                                            要推送的讯宝数量2 += 1
                                        End If
                                    End With
                                Next
                                要推送的讯宝 = 要推送的讯宝2
                                要推送的讯宝数量 = 要推送的讯宝数量2
                            End If
                        End If
                        If 要推送的讯宝数量 < 读取的讯宝最大数量 Then
                            Dim 新讯宝(读取的讯宝最大数量 - 要推送的讯宝数量 - 1) As 类_要推送的讯宝
                            Dim 新讯宝数量 As Integer = 0
                            结果 = 数据库_获取要推送的新讯宝(新讯宝, 新讯宝数量)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Continue Do
                            If 新讯宝数量 > 0 Then
                                Dim 要推送的讯宝2(要推送的讯宝数量 + 新讯宝数量 - 1) As 类_要推送的讯宝
                                If 要推送的讯宝数量 > 0 Then
                                    Array.Copy(要推送的讯宝, 0, 要推送的讯宝2, 0, 要推送的讯宝数量)
                                End If
                                Array.Copy(新讯宝, 0, 要推送的讯宝2, 要推送的讯宝数量, 新讯宝数量)
                                要推送的讯宝 = 要推送的讯宝2
                                要推送的讯宝数量 += 新讯宝数量
                            End If
                        End If
                        If 要推送的讯宝数量 > 0 Then
                            Dim J As Integer
                            For I = 0 To 要推送的讯宝数量 - 1
                                With 要推送的讯宝(I)
                                    If .当前状态 = 讯宝推送状态_常量集合.等待 Then
                                        For J = 0 To 讯宝推送器.Length - 1
                                            If 讯宝推送器(J) Is Nothing Then
                                                If .讯宝指令 <= 讯宝指令_常量集合.手机和电脑同步 Then
                                                    结果 = 数据库_存为不推送的讯宝(.发送时间, .发送者英语讯宝地址, .发送序号, .群主讯宝地址, .群编号, .接收者编号, .讯宝指令, .文本, .宽度, .高度, .秒数, 用户目录(.位置号))
                                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Continue Do
                                                End If
                                                讯宝推送器(J) = New 类_讯宝推送器(要推送的讯宝(I), J, Me)
                                                讯宝推送器(J).推送_启动新线程()
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End With
                            Next
                        End If
                    Catch ex As Exception
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Continue Do
                End If
                For I = 0 To 用户目录.Length - 1
                    With 用户目录(I)
                        If .手机等待确认的发送序号 > 0 Then
                            For J = 0 To 讯宝推送器.Length - 1
                                If 讯宝推送器(J) Is Nothing Then
                                    Dim 发送确认讯宝 As New 类_要推送的讯宝
                                    发送确认讯宝.发送者英语讯宝地址 = "*"
                                    发送确认讯宝.讯宝指令 = 讯宝指令_常量集合.从客户端发送至服务器成功
                                    发送确认讯宝.设备类型 = 设备类型_常量集合.手机
                                    发送确认讯宝.发送序号 = .手机等待确认的发送序号
                                    .手机等待确认的发送序号 = 0
                                    讯宝推送器(J) = New 类_讯宝推送器(发送确认讯宝, J, Me, 用户目录(I))
                                    讯宝推送器(J).推送_启动新线程()
                                    Exit For
                                End If
                            Next
                        End If
                        If .电脑等待确认的发送序号 > 0 Then
                            For J = 0 To 讯宝推送器.Length - 1
                                If 讯宝推送器(J) Is Nothing Then
                                    Dim 发送确认讯宝 As New 类_要推送的讯宝
                                    发送确认讯宝.发送者英语讯宝地址 = "*"
                                    发送确认讯宝.讯宝指令 = 讯宝指令_常量集合.从客户端发送至服务器成功
                                    发送确认讯宝.设备类型 = 设备类型_常量集合.电脑
                                    发送确认讯宝.发送序号 = .电脑等待确认的发送序号
                                    .电脑等待确认的发送序号 = 0
                                    讯宝推送器(J) = New 类_讯宝推送器(发送确认讯宝, J, Me, 用户目录(I))
                                    讯宝推送器(J).推送_启动新线程()
                                    Exit For
                                End If
                            Next
                        End If
                    End With
                Next
                Thread.Sleep(1000)
            Catch ex As Exception
                Thread.Sleep(2000)
            End Try
        Loop Until 关闭
    End Sub

    Private Function 数据库_获取要推送的新讯宝(ByRef 要推送的新讯宝() As 类_要推送的讯宝, ByRef 新讯宝数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝推送", Nothing, 要推送的新讯宝.Length, , CByte(要推送的新讯宝.Length), "#时间")
            Dim 时间, 接收者编号 As Long
            Dim 发送者英语地址 As String
            Dim 群接收者() As Byte
            Dim 要推送的讯宝 As 类_要推送的讯宝
            读取器 = 指令.执行()
            While 读取器.读取
                时间 = 读取器(0)    '按照表列的顺序
                发送者英语地址 = 读取器(1)
                接收者编号 = 读取器(6)
                If 要推送的讯宝是否重复(时间, 发送者英语地址, 接收者编号) = True Then Continue While
                群接收者 = 读取器(8)
                If 群接收者 Is Nothing Then
                    要推送的新讯宝(新讯宝数量) = New 类_要推送的讯宝
                    With 要推送的新讯宝(新讯宝数量)
                        .发送时间 = 时间
                        .发送者英语讯宝地址 = 发送者英语地址
                        .同服发送者编号 = 读取器(2)
                        .发送序号 = 读取器(3)
                        .群主讯宝地址 = 读取器(4)
                        .群编号 = 读取器(5)
                        .接收者编号 = 接收者编号
                        .位置号 = 读取器(7)
                        .讯宝指令 = 读取器(9)
                        .文本库号 = 读取器(10)
                        .文本编号 = 读取器(11)
                        .宽度 = 读取器(12)
                        .高度 = 读取器(13)
                        .秒数 = 读取器(14)
                        .设备类型 = 读取器(15)
                    End With
                    新讯宝数量 += 1
                    If 新讯宝数量 = 要推送的新讯宝.Length Then Exit While
                Else
                    要推送的讯宝 = New 类_要推送的讯宝
                    要推送的讯宝.发送时间 = 时间
                    要推送的讯宝.发送者英语讯宝地址 = 发送者英语地址
                    要推送的讯宝.同服发送者编号 = 读取器(2)
                    要推送的讯宝.发送序号 = 读取器(3)
                    要推送的讯宝.群主讯宝地址 = 读取器(4)
                    要推送的讯宝.群编号 = 读取器(5)
                    要推送的讯宝.接收者编号 = 接收者编号
                    要推送的讯宝.位置号 = 读取器(7)
                    要推送的讯宝.讯宝指令 = 读取器(9)
                    要推送的讯宝.文本库号 = 读取器(10)
                    要推送的讯宝.文本编号 = 读取器(11)
                    要推送的讯宝.宽度 = 读取器(12)
                    要推送的讯宝.高度 = 读取器(13)
                    要推送的讯宝.秒数 = 读取器(14)
                    Dim SS包解读器 As New 类_SS包解读器(群接收者)
                    Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("群成员")
                    Dim 长度 As Integer = 新讯宝数量 + SS包解读器2.Length
                    If 长度 > 要推送的新讯宝.Length Then ReDim Preserve 要推送的新讯宝(长度 - 1)
                    Dim SS包解读器3 As 类_SS包解读器
                    长度 = SS包解读器2.Length - 1
                    Dim I As Integer
                    For I = 0 To 长度
                        SS包解读器3 = SS包解读器2(I)
                        要推送的新讯宝(新讯宝数量) = New 类_要推送的讯宝
                        With 要推送的新讯宝(新讯宝数量)
                            .发送时间 = 要推送的讯宝.发送时间
                            .发送者英语讯宝地址 = 要推送的讯宝.发送者英语讯宝地址
                            .同服发送者编号 = 要推送的讯宝.同服发送者编号
                            .发送序号 = 要推送的讯宝.发送序号
                            .群主讯宝地址 = 要推送的讯宝.群主讯宝地址
                            .群编号 = 要推送的讯宝.群编号
                            SS包解读器3.读取_有标签("用户编号", .接收者编号)
                            SS包解读器3.读取_有标签("位置号", .位置号)
                            .讯宝指令 = 要推送的讯宝.讯宝指令
                            .文本库号 = 要推送的讯宝.文本库号
                            .文本编号 = 要推送的讯宝.文本编号
                            .宽度 = 要推送的讯宝.宽度
                            .高度 = 要推送的讯宝.高度
                            .秒数 = 要推送的讯宝.秒数
                            If I <> 长度 Then .不删除 = True
                            .设备类型 = 设备类型_常量集合.全部
                            If I = 0 Then .文本一样 = True
                        End With
                        新讯宝数量 += 1
                    Next
                    If 新讯宝数量 = 要推送的新讯宝.Length Then Exit While
                End If
            End While
            读取器.关闭()
            If 新讯宝数量 > 0 Then
                Dim 列添加器 As 类_列添加器
                Dim 筛选器 As 类_筛选器
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                Dim I, J As Integer
                For I = 0 To 新讯宝数量 - 1
                    With 要推送的新讯宝(I)
                        If .文本库号 > 0 AndAlso String.IsNullOrEmpty(.文本) Then
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于获取数据("文本")
                            指令 = New 类_数据库指令_请求获取数据(副数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                            读取器 = 指令.执行()
                            While 读取器.读取
                                .文本 = 读取器(0)
                                Exit While
                            End While
                            读取器.关闭()
                            If .文本一样 Then
                                If I < 新讯宝数量 - 1 Then
                                    文本库号 = .文本库号
                                    文本编号 = .文本编号
                                    For J = I + 1 To 新讯宝数量 - 1
                                        If 要推送的新讯宝(J).文本库号 = 文本库号 AndAlso 要推送的新讯宝(J).文本编号 = 文本编号 Then
                                            要推送的新讯宝(J).文本 = .文本
                                        Else
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 要推送的讯宝是否重复(ByVal 时间 As Long, ByVal 发送者英语地址 As String, ByVal 接收者 As Long) As Boolean
        Dim J As Integer
        For J = 要推送的讯宝数量 - 1 To 0 Step -1
            With 要推送的讯宝(J)
                If .发送时间 = 时间 Then
                    If String.Compare(.发送者英语讯宝地址, 发送者英语地址) = 0 Then
                        If .接收者编号 = 接收者 Then
                            Return True
                        End If
                    End If
                End If
            End With
        Next
        Return False
    End Function

    Private Function 数据库_删除推送成功或失败的讯宝(ByVal 讯宝 As 类_要推送的讯宝) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 讯宝.发送者英语讯宝地址)
            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 讯宝.发送序号)
            列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 讯宝.接收者编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "讯宝推送", 筛选器, "#发送者发送序号接收者")
            If 指令.执行() > 0 Then
跳转点1:
                If 讯宝.文本库号 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 讯宝.文本编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    指令 = New 类_数据库指令_删除数据(副数据库, 讯宝.文本库号 & "库", 筛选器, 主键索引名)
                    指令.执行()
                End If
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 讯宝.发送者英语讯宝地址)
                列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 讯宝.发送序号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令 = New 类_数据库指令_删除数据(副数据库, "讯宝推送", 筛选器, "#发送者发送序号接收者")
                指令.执行()
                GoTo 跳转点1
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
