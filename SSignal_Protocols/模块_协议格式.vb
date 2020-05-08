Imports System.IO
Imports System.Net

Public Module 模块_协议格式

    Public Structure 接收者_复合数据
        Dim 讯宝地址 As String
        Dim 位置号 As Short
    End Structure

    Public Structure 小聊天群成员_复合数据
        Dim 英语讯宝地址, 本国语讯宝地址, 主机名 As String
        Dim 位置号 As Short
        Dim 角色 As 群角色_常量集合
    End Structure

    Public Structure 大聊天群成员_复合数据
        Dim 英语讯宝地址, 本国语讯宝地址, 昵称, 主机名 As String
        Dim 角色 As 群角色_常量集合
    End Structure

    Public Structure 大聊天群新讯宝_复合数据
        Dim 群编号, 时间 As Long
        Dim 发送者英语地址, 主机名 As String
        Dim 讯宝指令 As 讯宝指令_常量集合
        Dim 文本 As String
        Dim 文本库号, 宽度, 高度 As Short
        Dim 文本编号 As Long
        Dim 秒数 As Byte
    End Structure

    Public Structure 加入的大聊天群_复合数据
        Dim 群编号 As Long
        Dim 新讯宝() As 大聊天群新讯宝_复合数据
        Dim 新讯宝数量 As Integer
    End Structure


    Public Sub 传送服务器发送讯宝(ByVal 参数 As Object)
        Dim 讯宝 As 类_要发送的讯宝 = 参数
        Dim 重试次数 As Integer
        Dim 字节数组() As Byte
        Try
            Dim SS包生成器 As New 类_SS包生成器()
            SS包生成器.添加_有标签("DM", 讯宝.子域名_发送服务器)   'Domain
            SS包生成器.添加_有标签("CR", 讯宝.发送凭据)   'Credential
            SS包生成器.添加_有标签("FR", 讯宝.发送者英语地址)   'From
            SS包生成器.添加_有标签("ID", 讯宝.序号)   'Index
            If 讯宝.群编号 > 0 Then
                SS包生成器.添加_有标签("GI", 讯宝.群编号)   'GroupIndex
                SS包生成器.添加_有标签("GO", 讯宝.群主英语地址)   'GroupOwner
            End If
            Dim 接收者() As 接收者_复合数据 = 讯宝.接收者
            Dim I As Integer
            Dim SS包生成器2 As New 类_SS包生成器()
            For I = 0 To 接收者.Length - 1
                Dim SS包生成器3 As New 类_SS包生成器()
                With 接收者(I)
                    SS包生成器3.添加_有标签("AD", .讯宝地址)   'Address
                    SS包生成器3.添加_有标签("PO", .位置号)   'Position
                End With
                SS包生成器2.添加_有标签("TA", SS包生成器3)   'Target
            Next
            SS包生成器.添加_有标签("TO", SS包生成器2)   'To
            SS包生成器.添加_有标签("CM", 讯宝.讯宝指令)   'Command
            SS包生成器.添加_有标签("TX", 讯宝.文本)  'Text
            Select Case 讯宝.讯宝指令
                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                    SS包生成器.添加_有标签("WD", 讯宝.宽度)   'Width
                    SS包生成器.添加_有标签("HT", 讯宝.高度)   'Height
            End Select
            If 讯宝.讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                SS包生成器.添加_有标签("SC", 讯宝.秒数)    'Seconds
            End If
            字节数组 = SS包生成器.生成SS包
        Catch ex As Exception
            讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束
            Return
        End Try
        Dim 收到的字节数组() As Byte
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create("https://" & 讯宝.子域名_接收服务器 & "/?C=SendSS")
            HTTP网络请求.Method = "POST"
            HTTP网络请求.Timeout = 10000
            HTTP网络请求.ContentType = "application/octet-stream"
            HTTP网络请求.ContentLength = 字节数组.Length
            Dim 流 As Stream = HTTP网络请求.GetRequestStream
            流.Write(字节数组, 0, 字节数组.Length)
            流.Close()
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    ReDim 收到的字节数组(HTTP网络回应.ContentLength - 1)
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
继续:
                    收到的字节数 = 输入流.Read(收到的字节数组, 收到的总字节数, 收到的字节数组.Length - 收到的总字节数)
                    If 收到的字节数 > 0 Then
                        收到的总字节数 += 收到的字节数
                        If 收到的总字节数 < 收到的字节数组.Length Then
                            GoTo 继续
                        End If
                    End If
                End If
            End Using
            If 收到的字节数组 IsNot Nothing Then
                If 收到的总字节数 = 收到的字节数组.Length Then
                    Dim SS包解读器 As New 类_SS包解读器(收到的字节数组)
                    Select Case SS包解读器.查询结果
                        Case 查询结果_常量集合.成功
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.无
                        Case 查询结果_常量集合.对方未添加我为讯友
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.对方未添加我为讯友
                        Case 查询结果_常量集合.已被对方拉黑
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.对方把我拉黑了
                        Case 查询结果_常量集合.讯宝地址不存在
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.讯宝地址不存在
                        Case 查询结果_常量集合.被邀请加入小聊天群者未添加我为讯友
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友
                        Case 查询结果_常量集合.被邀请加入大聊天群者未添加我为讯友
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友
                        Case 查询结果_常量集合.某人离开了小聊天群
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.退出小聊天群
                        Case 查询结果_常量集合.不是群成员
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.不是群成员
                        Case 查询结果_常量集合.本小时发送的讯宝数量已达上限
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.本小时发送的讯宝数量已达上限
                        Case 查询结果_常量集合.今日发送的讯宝数量已达上限
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.今日发送的讯宝数量已达上限
                        Case 查询结果_常量集合.HTTP数据错误
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.HTTP数据错误
                        Case Else
                            讯宝.发送失败的原因 = 讯宝指令_常量集合.目标服务器程序出错
                    End Select
                Else
                    讯宝.发送失败的原因 = 讯宝指令_常量集合.数据传送失败
                End If
            Else
                讯宝.发送失败的原因 = 讯宝指令_常量集合.数据传送失败
            End If
        Catch ex As Exception
            If 重试次数 < 3 Then
                重试次数 += 1
                GoTo 重试
            End If
            讯宝.发送失败的原因 = 讯宝指令_常量集合.数据传送失败
        End Try
        讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束
    End Sub

    Public Sub 添加数据_检查大聊天群新讯宝数量(ByVal SS包生成器 As 类_SS包生成器, ByVal 子域名 As String, ByVal 加入的大聊天群() As 类_聊天群_大)
        Dim I As Integer
        For I = 0 To 加入的大聊天群.Length - 1
            If String.Compare(加入的大聊天群(I).子域名, 子域名) = 0 Then
                Dim SS包生成器3 As New 类_SS包生成器()
                SS包生成器3.添加_有标签("GI", 加入的大聊天群(I).编号)    'GroupIndex
                SS包生成器3.添加_有标签("LT", 加入的大聊天群(I).最新讯宝的发送时间)   'LaterThan
                SS包生成器.添加_有标签("GP", SS包生成器3)   'Group
            End If
        Next
    End Sub

    Public Sub 添加数据_大聊天群返回新讯宝数量(ByVal 结果 As 类_SS包生成器, ByVal 加入的群() As 加入的大聊天群_复合数据)
        Dim I As Integer
        For I = 0 To 加入的群.Length - 1
            With 加入的群(I)
                If .新讯宝数量 > 0 Then
                    Dim SS包生成器 As New 类_SS包生成器()
                    SS包生成器.添加_有标签("GI", .群编号)    'GroupIndex
                    SS包生成器.添加_有标签("SN", .新讯宝数量)    'SSNumber
                    结果.添加_有标签("GP", SS包生成器)   'Group
                End If
            End With
        Next
    End Sub

    Public Sub 添加数据_大聊天群发送或接收讯宝(ByVal SS包生成器 As 类_SS包生成器, ByVal 讯宝指令 As 讯宝指令_常量集合,
                             ByVal 文字 As String, ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 文件数据() As Byte,
                             ByVal 子域名 As String, ByVal 加入的大聊天群() As 类_聊天群_大, ByVal 视频预览图片数据() As Byte)
        Dim SS包生成器2 As 类_SS包生成器
        If 讯宝指令 <> 讯宝指令_常量集合.无 Then
            SS包生成器2 = New 类_SS包生成器
            SS包生成器2 = New 类_SS包生成器
            SS包生成器2.添加_有标签("CM", 讯宝指令)   'Command
            SS包生成器2.添加_有标签("TX", 文字)   'Text
            If 讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                SS包生成器2.添加_有标签("SC", 秒数)    'Seconds
            End If
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                    SS包生成器2.添加_有标签("WD", 宽度)    'Width
                    SS包生成器2.添加_有标签("HT", 高度)    'Height
            End Select
            If 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                SS包生成器2.添加_有标签("PV", 视频预览图片数据)    'Preview
            End If
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频, 讯宝指令_常量集合.发送文件
                    SS包生成器2.添加_有标签("FD", 文件数据)    'FileData
            End Select
            SS包生成器.添加_有标签("POST", SS包生成器2)
        End If
        SS包生成器2 = New 类_SS包生成器()
        Dim I As Integer
        For I = 0 To 加入的大聊天群.Length - 1
            If String.Compare(加入的大聊天群(I).子域名, 子域名) = 0 Then
                Dim SS包生成器3 As New 类_SS包生成器()
                SS包生成器3.添加_有标签("GI", 加入的大聊天群(I).编号)    'GroupIndex
                SS包生成器3.添加_有标签("LT", 加入的大聊天群(I).最新讯宝的发送时间)   'LaterThan
                SS包生成器2.添加_有标签("GP", SS包生成器3)   'Group
            End If
        Next
        If SS包生成器2.数据量 > 0 Then
            SS包生成器.添加_有标签("GET", SS包生成器2)
        End If
    End Sub

    Public Sub 添加数据_大聊天群返回新讯宝(ByVal 结果 As 类_SS包生成器, ByVal 加入的群() As 加入的大聊天群_复合数据)
        Dim I, J As Integer
        For I = 0 To 加入的群.Length - 1
            With 加入的群(I)
                If .新讯宝数量 > 0 Then
                    Dim SS包生成器 As New 类_SS包生成器()
                    SS包生成器.添加_有标签("GI", .群编号)    'GroupIndex
                    For J = 0 To .新讯宝数量 - 1
                        Dim SS包生成器2 As New 类_SS包生成器()
                        With .新讯宝(J)
                            SS包生成器2.添加_有标签("DT", .时间)   'Date
                            SS包生成器2.添加_有标签("FR", .发送者英语地址)   'From
                            If String.IsNullOrEmpty(.主机名) = False Then
                                SS包生成器2.添加_有标签("HN", .主机名)   'HostName
                            End If
                            SS包生成器2.添加_有标签("CM", .讯宝指令)   'Command
                            SS包生成器2.添加_有标签("TX", .文本)   'Text
                            If .宽度 > 0 Then
                                SS包生成器2.添加_有标签("WD", .宽度)    'Width
                            End If
                            If .高度 > 0 Then
                                SS包生成器2.添加_有标签("HT", .高度)    'Height
                            End If
                            If .秒数 > 0 Then
                                SS包生成器2.添加_有标签("SC", .秒数)    'Seconds
                            End If
                        End With
                        SS包生成器.添加_有标签("SS", SS包生成器2)
                    Next
                    结果.添加_有标签("GP", SS包生成器)   'Group
                End If
            End With
        Next
    End Sub

    Public Sub 添加数据_验证讯宝地址真实性(ByVal 结果 As 类_SS包生成器, ByVal 英语用户名 As String, ByVal 本国语用户名 As String, ByVal 主机名 As String, ByVal 位置号 As Short, ByVal 域名_英语 As String, ByVal 域名_本国语 As String)
        结果.添加_有标签("E", 英语用户名 & 讯宝地址标识 & 域名_英语)   'EnglishSSAddress
        结果.添加_有标签("H", 主机名)   'HostName
        结果.添加_有标签("P", 位置号)   'Position
        If String.IsNullOrEmpty(本国语用户名) = False Then
            结果.添加_有标签("N", 本国语用户名 & 讯宝地址标识 & 域名_本国语)   'NativeSSAddress
        End If
    End Sub

    Public Sub 添加数据_获取大聊天群服务器连接凭据(ByVal 结果 As 类_SS包生成器, ByVal 群名称 As String, ByVal 图标更新时间 As Long,
                                  ByVal 连接凭据 As String, ByVal 角色 As 群角色_常量集合, ByVal 域名_本国语 As String)
        结果.添加_有标签("N", 群名称)   'Name
        结果.添加_有标签("V", 图标更新时间)   'IconVersion
        结果.添加_有标签("C", 连接凭据)   'Credential
        结果.添加_有标签("R", 角色)   'Role
        If String.IsNullOrEmpty(域名_本国语) = False Then
            结果.添加_有标签("D", 域名_本国语)   'Domain
        End If
    End Sub

    Public Sub 添加数据_获取小宇宙服务器连接凭据(ByVal 结果 As 类_SS包生成器, ByVal 连接凭据 As String, ByVal 是商品编辑 As Boolean)
        结果.添加_有标签("C", 连接凭据)   'Credential
        结果.添加_有标签("G", 是商品编辑)   'IsGoodEditor
    End Sub

    Public Function 生成文本_邀请加入小聊天群(ByVal 群编号 As Byte, ByVal 群备注 As String) As String
        Dim SS包生成器 As New 类_SS包生成器()
        SS包生成器.添加_有标签("I", 群编号)   'Index
        SS包生成器.添加_有标签("N", 群备注)   'Name
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成文本_邀请加入大聊天群(ByVal 子域名 As String, ByVal 群编号 As Long, ByVal 群名称 As String) As String
        Dim SS包生成器 As New 类_SS包生成器()
        SS包生成器.添加_有标签("D", 子域名)   'Domain
        SS包生成器.添加_有标签("I", 群编号)   'Index
        SS包生成器.添加_有标签("N", 群名称)   'Name
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成文本_获取小聊天群成员列表(ByVal 群成员() As 小聊天群成员_复合数据, ByVal 群成员数 As Short) As String
        Dim SS包生成器 As New 类_SS包生成器()
        For I = 0 To 群成员数 - 1
            With 群成员(I)
                Dim SS包生成器2 As New 类_SS包生成器(, , SS包编码_常量集合.UTF8)
                SS包生成器2.添加_有标签("E", .英语讯宝地址)   'EnglishSSAddress
                SS包生成器2.添加_有标签("H", .主机名)   'HostName
                SS包生成器2.添加_有标签("P", .位置号)   'Position
                SS包生成器2.添加_有标签("R", .角色)   'Role
                If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                    SS包生成器2.添加_有标签("N", .本国语讯宝地址)   'NativeSSAddress
                End If
                SS包生成器.添加_有标签("M", SS包生成器2)   'Member
            End With
        Next
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成文本_某人加入小聊天群的通知(ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String, ByVal 主机名 As String,
                                    ByVal 位置号 As Short) As String
        Dim SS包生成器 As New 类_SS包生成器
        SS包生成器.添加_有标签("E", 英语讯宝地址)   'EnglishSSAddress
        If String.IsNullOrEmpty(本国语讯宝地址) = False Then
            SS包生成器.添加_有标签("N", 本国语讯宝地址)   'NativeSSAddress
        End If
        SS包生成器.添加_有标签("H", 主机名)   'HostName
        SS包生成器.添加_有标签("P", 位置号)   'Position
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成文本_某人在大聊天群的角色改变的通知(ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String, ByVal 角色 As 群角色_常量集合) As String
        Dim SS包生成器 As New 类_SS包生成器
        SS包生成器.添加_有标签("E", 英语讯宝地址)   'EnglishSSAddress
        If String.IsNullOrEmpty(本国语讯宝地址) = False Then
            SS包生成器.添加_有标签("N", 本国语讯宝地址)   'NativeSSAddress
        End If
        SS包生成器.添加_有标签("R", 角色)   'Role
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成文本_某人离开聊天群的通知(ByVal 英语讯宝地址 As String, Optional ByVal 本国语讯宝地址 As String = Nothing) As String
        Dim SS包生成器 As New 类_SS包生成器
        SS包生成器.添加_有标签("E", 英语讯宝地址)   'EnglishSSAddress
        If String.IsNullOrEmpty(本国语讯宝地址) = False Then
            SS包生成器.添加_有标签("N", 本国语讯宝地址)   'NativeSSAddress
        End If
        Return SS包生成器.生成纯文本
    End Function

End Module
