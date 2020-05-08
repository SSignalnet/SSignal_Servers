Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Partial Public Class 类_处理请求

    Public Function 获取报表() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim UserID As Long
        If Long.TryParse(Http请求("UserID"), UserID) = False Then Return Nothing
        Dim Credential As String = Http请求("Credential")

        If UserID < 1 Then Return Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 用户信息 As New 类_用户信息(类_用户信息.范围_常量集合.职能)
                Dim 结果 As 类_SS包生成器 = 数据库_验证用户凭据(UserID, Credential, 用户信息)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If String.IsNullOrEmpty(用户信息.职能) Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 用户信息.职能.Contains(职能_管理员) = False AndAlso 用户信息.职能.Contains(职能_副管理员) = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                Dim 内容 As String = ""
                结果 = 数据库_获取报表(内容)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                结果.添加_有标签("报表", 内容)
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

    Private Function 数据库_获取报表(ByRef 内容 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 时间 As Date = Date.UtcNow.AddHours(8 - 12)    '北京时间-12小时
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("截止时间", 筛选方式_常量集合.大于, 时间.Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("内容")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "报表", 筛选器, 1, 列添加器, , "#截止时间")
            读取器 = 指令.执行()
            While 读取器.读取
                内容 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(内容) = True Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("职能")
                指令 = New 类_数据库指令_请求获取数据(主数据库, "用户", Nothing, , 列添加器, 100)
                Dim 用户数, 管理人员数 As Long
                Dim 职能 As String
                读取器 = 指令.执行()
                While 读取器.读取
                    用户数 += 1
                    职能 = 读取器(0)
                    If String.IsNullOrEmpty(职能) = False Then
                        管理人员数 += 1
                    End If
                End While
                读取器.关闭()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"类别", "停用"})
                指令 = New 类_数据库指令_请求获取数据(主数据库, "服务器", Nothing, , 列添加器, 100)
                Dim 传送服务器数_启用的, 传送服务器数_停用的, 大聊天群服务器数_启用的, 大聊天群服务器数_停用的,
                       小宇宙中心服务器数, 小宇宙写入服务器数_启用的, 小宇宙写入服务器数_停用的,
                       小宇宙读取服务器数_启用的, 小宇宙读取服务器数_停用的, 视频通话服务器数_启用的, 视频通话服务器数_停用的 As Long
                Dim 类别 As 服务器类别_常量集合
                Dim 停用 As Boolean
                读取器 = 指令.执行()
                While 读取器.读取
                    类别 = 读取器(0)
                    停用 = 读取器(1)
                    Select Case 类别
                        Case 服务器类别_常量集合.传送服务器
                            If 停用 = False Then
                                传送服务器数_启用的 += 1
                            Else
                                传送服务器数_停用的 += 1
                            End If
                        Case 服务器类别_常量集合.大聊天群服务器
                            If 停用 = False Then
                                大聊天群服务器数_启用的 += 1
                            Else
                                大聊天群服务器数_停用的 += 1
                            End If
                        Case 服务器类别_常量集合.小宇宙中心服务器
                            小宇宙中心服务器数 += 1
                        Case 服务器类别_常量集合.小宇宙写入服务器
                            If 停用 = False Then
                                小宇宙写入服务器数_启用的 += 1
                            Else
                                小宇宙写入服务器数_停用的 += 1
                            End If
                        Case 服务器类别_常量集合.小宇宙读取服务器
                            If 停用 = False Then
                                小宇宙读取服务器数_启用的 += 1
                            Else
                                小宇宙读取服务器数_停用的 += 1
                            End If
                        Case 服务器类别_常量集合.视频通话服务器
                            If 停用 = False Then
                                视频通话服务器数_启用的 += 1
                            Else
                                视频通话服务器数_停用的 += 1
                            End If
                    End Select
                End While
                读取器.关闭()
                Const 空格 As String = "&nbsp;&nbsp;"
                时间 = Date.UtcNow.AddHours(8)
                Dim 变长文本 As New StringBuilder(2000)
                Dim 文本写入器 As New StringWriter(变长文本)
                If String.IsNullOrWhiteSpace(域名_本国语) = False Then
                    文本写入器.Write(讯宝中心服务器主机名 & "." & 域名_英语 & " / " & 域名_本国语)
                Else
                    文本写入器.Write(讯宝中心服务器主机名 & "." & 域名_英语)
                End If
                文本写入器.Write("<br>截至北京时间 " & 时间.Year & "-" & Strings.Format(时间.Month, "00") & "-" & Strings.Format(时间.Day, "00") & " " & Strings.Format(时间.Hour, "00") & ":" & Strings.Format(时间.Minute, "00") & ":" & Strings.Format(时间.Second, "00"))
                文本写入器.Write("<br>用户数：<br>" & 空格 & FormatNumber(用户数, 0, , , TriState.True) & " 位")
                文本写入器.Write("<br>管理人员数：<br>" & 空格 & FormatNumber(管理人员数, 0, , , TriState.True))
                文本写入器.Write("<br>传送服务器数：<br>" & 空格 & FormatNumber(传送服务器数_启用的, 0, , , TriState.True) & " 台运营<br>" & 空格 & FormatNumber(传送服务器数_停用的, 0, , , TriState.True) & " 台停用")
                文本写入器.Write("<br>大聊天群服务器数：<br>" & 空格 & FormatNumber(大聊天群服务器数_启用的, 0, , , TriState.True) & " 台运营<br>" & 空格 & FormatNumber(大聊天群服务器数_停用的, 0, , , TriState.True) & " 台停用")
                文本写入器.Write("<br>小宇宙中心服务器数：<br>" & 空格 & FormatNumber(小宇宙中心服务器数, 0, , , TriState.True) & " 台")
                文本写入器.Write("<br>小宇宙写入服务器数：<br>" & 空格 & FormatNumber(小宇宙写入服务器数_启用的, 0, , , TriState.True) & " 台运营<br>" & 空格 & FormatNumber(小宇宙写入服务器数_停用的, 0, , , TriState.True) & " 台停用")
                文本写入器.Write("<br>小宇宙读取服务器数：<br>" & 空格 & FormatNumber(小宇宙读取服务器数_启用的, 0, , , TriState.True) & " 台运营<br>" & 空格 & FormatNumber(小宇宙读取服务器数_停用的, 0, , , TriState.True) & " 台停用")
                文本写入器.Write("<br>视频通话服务器数：<br>" & 空格 & FormatNumber(视频通话服务器数_启用的, 0, , , TriState.True) & " 台运营<br>" & 空格 & FormatNumber(视频通话服务器数_停用的, 0, , , TriState.True) & " 台停用")
                内容 = 文本写入器.ToString
                文本写入器.Close()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("截止时间", 时间.Ticks)
                列添加器.添加列_用于插入数据("内容", 内容)
                Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "报表", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
