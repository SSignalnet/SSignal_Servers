Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB

Partial Public Class 类_讯宝管理器

    Public Function 获取统计数据() As String
        Dim 变长文本 As New StringBuilder(300)
        Dim 文本写入器 As New StringWriter(变长文本)
        Dim 在线用户总数, 手机在线用户数, 电脑在线用户数, 手机和电脑同时在线用户数 As Integer
        If 用户目录 IsNot Nothing Then
            Dim 某一用户 As 类_用户
            Dim I As Integer
            For I = 0 To 用户目录.Length - 1
                某一用户 = 用户目录(I)
                If 某一用户 IsNot Nothing Then
                    If 某一用户.网络连接器_手机 IsNot Nothing Then
                        在线用户总数 += 1
                        If 某一用户.网络连接器_电脑 IsNot Nothing Then
                            手机和电脑同时在线用户数 += 1
                        Else
                            手机在线用户数 += 1
                        End If
                    ElseIf 某一用户.网络连接器_电脑 IsNot Nothing Then
                        在线用户总数 += 1
                        电脑在线用户数 += 1
                    End If
                End If
            Next
        End If
        文本写入器.Write("传送服务器 " & 本服务器主机名 & "." & 域名_英语 & " 统计数据<br>")
        文本写入器.Write("====================<br>")
        文本写入器.Write("当前在线用户总数：" & 在线用户总数 & " 人，其中：<br>")
        文本写入器.Write(手机在线用户数 & " 人手机在线；<br>")
        文本写入器.Write(电脑在线用户数 & " 人电脑在线；<br>")
        文本写入器.Write(手机和电脑同时在线用户数 & " 人手机和电脑同时在线。<br>")
        文本写入器.Write("====================<br>")
        Dim 今日发送, 今日接收, 昨日发送, 昨日接收, 前日发送, 前日接收 As Integer
        Dim 当前时间 As Date
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 创建的群数量 As Integer
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "创建的小聊天群", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        创建的群数量 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                文本写入器.Write("创建的小聊天群数量：" & 创建的群数量 & " 个；<br>")
                Dim 加入的群数量, 加入的外域群数量 As Integer
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于获取数据("群主地址")
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的小聊天群", Nothing,  , 列添加器, 100)
                    Dim 标识加域名 As String = 讯宝地址标识 & 域名_英语
                    读取器 = 指令.执行()
                    While 读取器.读取
                        加入的群数量 += 1
                        If CStr(读取器(0)).EndsWith(标识加域名) = False Then
                            加入的外域群数量 += 1
                        End If
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                文本写入器.Write("加入的小聊天群数量：" & 加入的群数量 & " 个，其中外域群 " & 加入的外域群数量 & " 个。<br>")
                文本写入器.Write("====================<br>")
                当前时间 = Date.Now
                Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                Dim 收发统计(最大值_常量集合.传送服务器承载用户数 - 1) As 收发统计_复合数据
                Dim 收发统计数 As Integer
                Try
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "收发统计", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        With 收发统计(收发统计数)
                            .用户编号 = 读取器(0)
                            .今日几号 = 读取器(2)
                            .今日发送 = 读取器(3)
                            .今日接收 = 读取器(4)
                            .昨日发送 = 读取器(5)
                            .昨日接收 = 读取器(6)
                            .前日发送 = 读取器(7)
                            .前日接收 = 读取器(8)
                            .今日几时 = 读取器(9)
                            .时段发送 = 读取器(10)
                        End With
                        收发统计数 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                If 收发统计数 > 0 Then
                    Dim 结果 As 类_SS包生成器
                    Dim I As Integer
                    For I = 0 To 收发统计数 - 1
                        With 收发统计(I)
                            If 今日几号 = .今日几号 Then
                                今日发送 += .今日发送
                                今日接收 += .今日接收
                                昨日发送 += .昨日发送
                                昨日接收 += .昨日接收
                                前日发送 += .前日发送
                                前日接收 += .前日接收
                            ElseIf 昨日几号 = .今日几号 Then
                                结果 = 数据库_更新收发统计(.用户编号, 今日几号, 0, 0, .今日发送, .今日接收, .昨日发送, .昨日接收, 0, 0)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                昨日发送 += .今日发送
                                昨日接收 += .今日接收
                                前日发送 += .昨日发送
                                前日接收 += .昨日接收
                            ElseIf 前日几号 = .今日几号 Then
                                结果 = 数据库_更新收发统计(.用户编号, 今日几号, 0, 0, 0, 0, .今日发送, .今日接收, 0, 0)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                前日发送 += .今日发送
                                前日接收 += .今日接收
                            ElseIf .今日发送 > 0 OrElse .今日接收 > 0 OrElse .昨日发送 > 0 OrElse .昨日接收 > 0 OrElse .前日发送 > 0 OrElse .前日接收 > 0 OrElse .今日几时 > 0 OrElse .时段发送 > 0 Then
                                结果 = 数据库_删除收发统计(.用户编号)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return ""
        End If
        文本写入器.Write("今日发送讯宝：" & 今日发送 & " 条；<br>")
        文本写入器.Write("昨日发送讯宝：" & 昨日发送 & " 条；<br>")
        文本写入器.Write("前日发送讯宝：" & 前日发送 & " 条；<br>")
        文本写入器.Write("今日接收讯宝：" & 今日接收 & " 条；<br>")
        文本写入器.Write("昨日接收讯宝：" & 昨日接收 & " 条；<br>")
        文本写入器.Write("前日接收讯宝：" & 前日接收 & " 条。<br>")
        文本写入器.Write("====================<br>")
        文本写入器.Write(当前时间.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取在线用户详情() As String
        If 用户目录 Is Nothing Then Return ""
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.传送服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("传送服务器 " & 本服务器主机名 & "." & 域名_英语 & " 在线用户详情<br>")
        文本写入器.Write("====================<br>")
        Dim 某一用户 As 类_用户
        Dim I As Integer
        For I = 0 To 用户目录.Length - 1
            某一用户 = 用户目录(I)
            If 某一用户 IsNot Nothing Then
                If 某一用户.网络连接器_手机 IsNot Nothing OrElse 某一用户.网络连接器_电脑 IsNot Nothing Then
                    If String.IsNullOrEmpty(某一用户.本国语用户名) = False Then
                        文本写入器.Write(某一用户.本国语用户名 & " / " & 某一用户.英语用户名 & "：")
                    Else
                        文本写入器.Write(某一用户.英语用户名 & "：")
                    End If
                    If 某一用户.网络连接器_手机 IsNot Nothing Then
                        If 某一用户.网络连接器_电脑 IsNot Nothing Then
                            文本写入器.Write("手机和电脑")
                        Else
                            文本写入器.Write("手机")
                        End If
                    Else
                        文本写入器.Write("电脑")
                    End If
                    文本写入器.Write("<br>")
                End If
            End If
        Next
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取聊天群详情() As String
        If 用户目录 Is Nothing Then Return ""
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.传送服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("传送服务器 " & 本服务器主机名 & "." & 域名_英语 & " 聊天群详情<br>")
        文本写入器.Write("====================<br>")
        Dim 创建的群(最大值_常量集合.传送服务器承载用户数 * 最大值_常量集合.每个用户可创建的小聊天群数量 - 1) As 创建的群_复合数据
        Dim 创建的群数量 As Integer
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"用户编号", "位置号"})
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "创建的小聊天群", Nothing,  , , 100, "#创建时间")
                    读取器 = 指令.执行()
                    While 读取器.读取
                        With 创建的群(创建的群数量)
                            .用户编号 = 读取器(0)
                            .位置号 = 读取器(1)
                        End With
                        创建的群数量 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return ""
        End If
        If 创建的群数量 > 0 Then
            Dim 某一创建的群 As 创建的群_复合数据
            Dim I, J As Integer
            For I = 创建的群数量 - 1 To 0 Step -1
                某一创建的群 = 创建的群(I)
                For J = 0 To I - 1
                    If 创建的群(J).用户编号 = 某一创建的群.用户编号 Then
                        创建的群(J).数量 += 1
                        创建的群(I).用户编号 = 0
                        Exit For
                    End If
                Next
                If J = I Then
                    创建的群(I).数量 += 1
                End If
            Next
            Dim 某一用户 As 类_用户
            For I = 0 To 创建的群数量 - 1
                With 创建的群(I)
                    If .用户编号 > 0 Then
                        某一用户 = 用户目录(.位置号)
                        If 某一用户 IsNot Nothing Then
                            If 某一用户.用户编号 = .用户编号 Then
                                If String.IsNullOrEmpty(某一用户.本国语用户名) = False Then
                                    文本写入器.Write(某一用户.本国语用户名 & " / " & 某一用户.英语用户名 & "：")
                                Else
                                    文本写入器.Write(某一用户.英语用户名 & "：")
                                End If
                                文本写入器.Write("创建了 " & .数量 & " 个小聊天群<br>")
                            End If
                        End If
                    End If
                End With
            Next
            文本写入器.Write("====================<br>")
        End If
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 获取讯宝数量() As String
        If 用户目录 Is Nothing Then Return ""
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.传送服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("传送服务器 " & 本服务器主机名 & "." & 域名_英语 & " 讯宝数量<br>")
        文本写入器.Write("====================<br>")
        Dim 当前时间 As Date
        Dim 收发统计(最大值_常量集合.传送服务器承载用户数 - 1) As 收发统计_复合数据
        Dim 收发统计数 As Integer
        If 跨进程锁.WaitOne = True Then
            Try
                当前时间 = Date.Now
                Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "收发统计", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        With 收发统计(收发统计数)
                            .用户编号 = 读取器(0)
                            .位置号 = 读取器(1)
                            .今日几号 = 读取器(2)
                            .今日发送 = 读取器(3)
                            .今日接收 = 读取器(4)
                            .昨日发送 = 读取器(5)
                            .昨日接收 = 读取器(6)
                            .前日发送 = 读取器(7)
                            .前日接收 = 读取器(8)
                            .今日几时 = 读取器(9)
                            .时段发送 = 读取器(10)
                        End With
                        收发统计数 += 1
                    End While
                    读取器.关闭()
                Catch ex As Exception
                    If 读取器 IsNot Nothing Then 读取器.关闭()
                    Return ex.Message
                End Try
                If 收发统计数 > 0 Then
                    Dim 结果 As 类_SS包生成器
                    Dim I As Integer
                    For I = 0 To 收发统计数 - 1
                        With 收发统计(I)
                            If 今日几号 <> .今日几号 Then
                                If 昨日几号 = .今日几号 Then
                                    结果 = 数据库_更新收发统计(.用户编号, 今日几号, 0, 0, .今日发送, .今日接收, .昨日发送, .昨日接收, 0, 0)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = .昨日发送
                                    .前日接收 = .昨日接收
                                    .昨日发送 = .今日发送
                                    .昨日接收 = .今日接收
                                    .今日发送 = 0
                                    .今日接收 = 0
                                ElseIf 前日几号 = .今日几号 Then
                                    结果 = 数据库_更新收发统计(.用户编号, 今日几号, 0, 0, 0, 0, .今日发送, .今日接收, 0, 0)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = .今日发送
                                    .前日接收 = .今日接收
                                    .昨日发送 = 0
                                    .昨日接收 = 0
                                    .今日发送 = 0
                                    .今日接收 = 0
                                ElseIf .今日发送 > 0 OrElse .今日接收 > 0 OrElse .昨日发送 > 0 OrElse .昨日接收 > 0 OrElse .前日发送 > 0 OrElse .前日接收 > 0 OrElse .今日几时 > 0 OrElse .时段发送 > 0 Then
                                    结果 = 数据库_删除收发统计(.用户编号)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果.出错提示文本
                                    End If
                                    .前日发送 = 0
                                    .前日接收 = 0
                                    .昨日发送 = 0
                                    .昨日接收 = 0
                                    .今日发送 = 0
                                    .今日接收 = 0
                                End If
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                Return ex.Message
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return ""
        End If
        If 收发统计数 > 0 Then
            Dim 某一用户 As 类_用户
            Dim I As Integer
            For I = 0 To 收发统计数 - 1
                With 收发统计(I)
                    某一用户 = 用户目录(.位置号)
                    If 某一用户 IsNot Nothing Then
                        If 某一用户.用户编号 = .用户编号 Then
                            If String.IsNullOrEmpty(某一用户.本国语用户名) = False Then
                                文本写入器.Write(某一用户.本国语用户名 & " / " & 某一用户.英语用户名 & "：")
                            Else
                                文本写入器.Write(某一用户.英语用户名 & "：")
                            End If
                            文本写入器.Write("发送 " & .今日发送 & "/" & .昨日发送 & "/" & .前日发送 & "，接收 " & .今日接收 & "/" & .昨日接收 & "/" & .前日接收 & "<br>")
                        End If
                    End If
                End With
            Next
            文本写入器.Write("====================<br>")
        End If
        文本写入器.Write(当前时间.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_更新今日收发统计(ByVal 用户编号 As Long, ByVal 今日发送 As Short, ByVal 今日接收 As Short,
                                  ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("今日接收", 今日接收)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "收发统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新收发统计(ByVal 用户编号 As Long, ByVal 今日几号 As Integer,
                                ByVal 今日发送 As Short, ByVal 今日接收 As Short, ByVal 昨日发送 As Short, ByVal 昨日接收 As Short,
                                ByVal 前日发送 As Short, ByVal 前日接收 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("今日接收", 今日接收)
            列添加器_新数据.添加列_用于插入数据("昨日发送", 昨日发送)
            列添加器_新数据.添加列_用于插入数据("昨日接收", 昨日接收)
            列添加器_新数据.添加列_用于插入数据("前日发送", 前日发送)
            列添加器_新数据.添加列_用于插入数据("前日接收", 前日接收)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "收发统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加收发统计(ByVal 用户编号 As Long, ByVal 位置号 As Short, ByVal 今日几号 As Integer,
                                ByVal 今日发送 As Short, ByVal 今日接收 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日发送", 今日发送)
            列添加器.添加列_用于插入数据("今日接收", 今日接收)
            列添加器.添加列_用于插入数据("昨日发送", 0)
            列添加器.添加列_用于插入数据("昨日接收", 0)
            列添加器.添加列_用于插入数据("前日发送", 0)
            列添加器.添加列_用于插入数据("前日接收", 0)
            列添加器.添加列_用于插入数据("今日几时", 今日几时)
            列添加器.添加列_用于插入数据("时段发送", 时段发送)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "收发统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除收发统计(ByVal 用户编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "收发统计", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新接收统计(ByVal 英语讯宝地址 As String, ByVal 今日几号 As Integer,
                               ByVal 今日接收 As Short, ByVal 今日几时 As Byte, ByVal 时段接收 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日接收", 今日接收)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段接收", 时段接收)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "接收统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加接收统计(ByVal 英语讯宝地址 As String, ByVal 今日几号 As Integer,
                               ByVal 今日接收 As Short, ByVal 今日几时 As Byte, ByVal 时段接收 As Short) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日接收", 今日接收)
            列添加器.添加列_用于插入数据("今日几时", 今日几时)
            列添加器.添加列_用于插入数据("时段接收", 时段接收)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "接收统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
