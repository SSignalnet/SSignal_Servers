Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB

Partial Public Class 类_处理请求

    Private Structure 聊天群_复合数据
        Dim 名称, 群主英语讯宝地址, 群主本国语讯宝地址 As String
        Dim 编号 As Long
        Dim 成员数 As Integer
        Dim 停用 As Boolean
    End Structure

    Private Structure 群讯宝统计_复合数据
        Dim 群编号 As Long
        Dim 群名称 As String
        Dim 今日几号 As Integer
        Dim 今日发送, 昨日发送,
                前日发送 As Short
    End Structure

    Public Function 获取服务器信息() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return ""
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return Nothing
        Select Case Http请求("InfoType")
            Case "chatgroups" : Return 获取聊天群详情()
            Case "ssnumber" : Return 获取讯宝数量()
            Case Else : Return Nothing
        End Select
    End Function

    Private Function 获取聊天群详情() As String
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.大聊天群服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("大聊天群服务器 " & 启动器.本服务器主机名 & "." & 域名_英语 & " 聊天群详情<br>")
        文本写入器.Write("====================<br>")
        Dim 聊天群(99) As 聊天群_复合数据
        Dim 聊天群数量 As Integer
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"编号", "名称", "成员数", "停用"})
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群", Nothing, , 列添加器, 100, "#成员数名称")
                    读取器 = 指令.执行()
                    While 读取器.读取
                        If 聊天群数量 = 聊天群.Length Then ReDim Preserve 聊天群(聊天群数量 * 2 - 1)
                        With 聊天群(聊天群数量)
                            .编号 = 读取器(0)
                            .名称 = 读取器(1)
                            .成员数 = 读取器(2)
                            .停用 = 读取器(3)
                        End With
                        聊天群数量 += 1
                    End While
                    读取器.关闭()
                    Dim 筛选器 As 类_筛选器
                    If 聊天群数量 > 0 Then
                        Dim I As Integer
                        For I = 0 To 聊天群数量 - 1
                            With 聊天群(I)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, .编号)
                                列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.等于, 群角色_常量集合.群主)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据(New String() {"英语讯宝地址", "本国语讯宝地址"})
                                指令 = New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器,  , "#群编号角色加入时间")
                                读取器 = 指令.执行()
                                While 读取器.读取
                                    .群主英语讯宝地址 = 读取器(0)
                                    .群主本国语讯宝地址 = 读取器(1)
                                    Exit While
                                End While
                                读取器.关闭()
                            End With
                        Next
                    End If
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
        文本写入器.Write("共有 " & 聊天群数量 & " 个聊天群<br>")
        文本写入器.Write("====================<br>")
        If 聊天群数量 > 0 Then
            Const 空格 As String = "&nbsp;&nbsp;"
            Dim I As Integer
            For I = 0 To 聊天群数量 - 1
                With 聊天群(I)
                    If .停用 = True Then
                        文本写入器.Write("[已停用] ")
                    End If
                    文本写入器.Write(.名称)
                    文本写入器.Write("：")
                    文本写入器.Write(.成员数)
                    文本写入器.Write(" 个成员<br>")
                    文本写入器.Write(空格)
                    文本写入器.Write("群主：")
                    If String.IsNullOrEmpty(.群主本国语讯宝地址) = False Then
                        文本写入器.Write(.群主本国语讯宝地址 & " / " & .群主英语讯宝地址 & "）<br>")
                    Else
                        文本写入器.Write(.群主英语讯宝地址 & "）<br>")
                    End If
                    文本写入器.Write(空格)
                    文本写入器.Write("创建时间：")
                    文本写入器.Write(Date.FromBinary(.编号).ToString)
                End With
            Next
            文本写入器.Write("====================<br>")
        End If
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 获取讯宝数量() As String
        Dim 变长文本 As New StringBuilder(50 * 最大值_常量集合.传送服务器承载用户数)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("大聊天群服务器 " & 启动器.本服务器主机名 & "." & 域名_英语 & " 讯宝数量<br>")
        文本写入器.Write("====================<br>")
        Dim 当前时间 As Date
        Dim 收发统计(99) As 群讯宝统计_复合数据
        Dim 收发统计数 As Integer
        Dim 今日发送, 昨日发送, 前日发送 As Long
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
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "群讯宝统计", Nothing,  , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        If 收发统计数 = 收发统计.Length Then ReDim Preserve 收发统计(收发统计数 * 2 - 1)
                        With 收发统计(收发统计数)
                            .群编号 = 读取器(0)
                            .今日几号 = 读取器(1)
                            .今日发送 = 读取器(2)
                            .昨日发送 = 读取器(3)
                            .前日发送 = 读取器(4)
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
                                昨日发送 += .昨日发送
                                前日发送 += .前日发送
                            ElseIf 昨日几号 = .今日几号 Then
                                结果 = 数据库_更新群讯宝统计(.群编号, 今日几号, 0, .今日发送, .昨日发送)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                昨日发送 += .今日发送
                                前日发送 += .昨日发送
                                .前日发送 = .昨日发送
                                .昨日发送 = .今日发送
                                .今日发送 = 0
                            ElseIf 前日几号 = .今日几号 Then
                                结果 = 数据库_更新群讯宝统计(.群编号, 今日几号, 0, 0, .今日发送)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                前日发送 += .今日发送
                                .前日发送 = .今日发送
                                .昨日发送 = 0
                                .今日发送 = 0
                            ElseIf .今日发送 > 0 OrElse .昨日发送 > 0 OrElse .前日发送 > 0 Then
                                结果 = 数据库_删除群讯宝统计(.群编号)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果.出错提示文本
                                End If
                                .前日发送 = 0
                                .昨日发送 = 0
                                .今日发送 = 0
                            End If
                        End With
                    Next
                    Try
                        Dim 列添加器 As 类_列添加器
                        For I = 0 To 收发统计数 - 1
                            With 收发统计(I)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .群编号)
                                Dim 筛选器 As New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据("名称")
                                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群", 筛选器, 1, 列添加器,  , 主键索引名)
                                读取器 = 指令.执行()
                                While 读取器.读取
                                    .群名称 = 读取器(0)
                                    Exit While
                                End While
                                读取器.关闭()
                            End With
                        Next
                    Catch ex As Exception
                        If 读取器 IsNot Nothing Then 读取器.关闭()
                        Return ex.Message
                    End Try
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
        文本写入器.Write("====================<br>")
        If 收发统计数 > 0 Then
            Dim I As Integer
            For I = 0 To 收发统计数 - 1
                With 收发统计(I)
                    文本写入器.Write(.群名称 & "：发送 " & .今日发送 & "/" & .昨日发送 & "/" & .前日发送 & "<br>")
                End With
            Next
            文本写入器.Write("====================<br>")
        End If
        文本写入器.Write(当前时间.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_更新今日个人讯宝统计(ByVal 英语讯宝地址 As String, ByVal 今日发送 As Short,
                                  ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "个人讯宝统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新今日群讯宝统计(ByVal 群编号 As Long, ByVal 今日发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "群讯宝统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新个人讯宝统计(ByVal 英语讯宝地址 As String, ByVal 今日几号 As Integer, ByVal 今日发送 As Short,
                                ByVal 昨日发送 As Short, ByVal 前日发送 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("昨日发送", 昨日发送)
            列添加器_新数据.添加列_用于插入数据("前日发送", 前日发送)
            列添加器_新数据.添加列_用于插入数据("今日几时", 今日几时)
            列添加器_新数据.添加列_用于插入数据("时段发送", 时段发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "个人讯宝统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_更新群讯宝统计(ByVal 群编号 As Long, ByVal 今日几号 As Integer, ByVal 今日发送 As Short,
                                ByVal 昨日发送 As Short, ByVal 前日发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("今日几号", 今日几号)
            列添加器_新数据.添加列_用于插入数据("今日发送", 今日发送)
            列添加器_新数据.添加列_用于插入数据("昨日发送", 昨日发送)
            列添加器_新数据.添加列_用于插入数据("前日发送", 前日发送)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "群讯宝统计", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加个人讯宝统计(ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String,
                                ByVal 今日几号 As Integer, ByVal 今日发送 As Short, ByVal 今日几时 As Byte, ByVal 时段发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
            If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                列添加器.添加列_用于插入数据("本国语讯宝地址", 本国语讯宝地址)
            End If
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日发送", 今日发送)
            列添加器.添加列_用于插入数据("昨日发送", 0)
            列添加器.添加列_用于插入数据("前日发送", 0)
            列添加器.添加列_用于插入数据("今日几时", 今日几时)
            列添加器.添加列_用于插入数据("时段发送", 时段发送)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "个人讯宝统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加群讯宝统计(ByVal 群编号 As Long, ByVal 今日几号 As Integer, ByVal 今日发送 As Short) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("今日几号", 今日几号)
            列添加器.添加列_用于插入数据("今日发送", 今日发送)
            列添加器.添加列_用于插入数据("昨日发送", 0)
            列添加器.添加列_用于插入数据("前日发送", 0)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "群讯宝统计", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除个人讯宝统计(ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "个人讯宝统计", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群讯宝统计(ByVal 群编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "群讯宝统计", 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
