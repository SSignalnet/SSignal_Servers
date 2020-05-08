Imports System.IO
Imports System.Text
Imports SSignalDB

Partial Public Class 类_处理请求

    Private Structure 域_复合数据
        Dim 英语域名, 本国语域名 As String
        Dim 访问者数量 As Long
    End Structure

    Public Function 获取服务器信息() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return ""
        If 启动器.验证中心服务器(获取网络地址文本(), Http请求("Credential")) = False Then Return Nothing
        Select Case Http请求("InfoType")
            Case "visitors" : Return 获取访问者统计数据()
            Case "goods" : Return 获取商品统计数据()
            Case "meterorains" : Return 获取流星语统计数据()
            Case Else : Return Nothing
        End Select
    End Function

    Private Function 获取访问者统计数据() As String
        Dim 变长文本 As New StringBuilder(5000)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("小宇宙中心服务器 " & 启动器.本服务器主机名 & "." & 域名_英语 & " 访问者统计<br>")
        文本写入器.Write("====================<br>")
        Dim 域(99) As 域_复合数据
        Dim 域数量, 访问者数量, I As Long
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("访问时间", 筛选方式_常量集合.大于, Date.UtcNow.AddHours(-24).Ticks)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"英语域名", "本国语域名"})
                    Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, , 列添加器, 100, "#访问时间")
                    Dim 英语域名, 本国语域名 As String
                    读取器 = 指令.执行()
                    While 读取器.读取
                        英语域名 = 读取器(0)
                        本国语域名 = 读取器(1)
                        For I = 0 To 域数量 - 1
                            If String.Compare(英语域名, 域(I).英语域名) = 0 Then Exit For
                        Next
                        If I < 域数量 Then
                            域(I).访问者数量 += 1
                        Else
                            If 域数量 = 域.Length Then ReDim Preserve 域(域数量 * 2 - 1)
                            With 域(I)
                                .英语域名 = 英语域名
                                .本国语域名 = 本国语域名
                                .访问者数量 = 1
                            End With
                            域数量 += 1
                        End If
                        访问者数量 += 1
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
        文本写入器.Write("最近24小时内共有 " & 访问者数量 & " 位访问者<br>")
        If 域数量 > 0 Then
            For I = 0 To 域数量 - 1
                With 域(I)
                    文本写入器.Write(.访问者数量 & " 位访问者来自 " & .英语域名 & IIf(String.IsNullOrEmpty(.本国语域名), "", "/" & .本国语域名) & " <br>")
                End With
            Next
        End If
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 获取商品统计数据() As String
        Dim 变长文本 As New StringBuilder(100)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("小宇宙中心服务器 " & 启动器.本服务器主机名 & "." & 域名_英语 & " 商品统计<br>")
        文本写入器.Write("====================<br>")
        Dim 商品数量 As Integer
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "商品", 筛选器, , , 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        商品数量 += 1
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
        文本写入器.Write("共有 " & 商品数量 & " 个商品<br>")
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 获取流星语统计数据() As String
        Dim 变长文本 As New StringBuilder(100)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("小宇宙中心服务器 " & 启动器.本服务器主机名 & "." & 域名_英语 & " 流星语统计<br>")
        文本写入器.Write("====================<br>")
        Dim 流星语数量, 评论数量, 点赞数量 As Long
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 读取器 As 类_读取器_外部 = Nothing
                Try
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"评论数量", "点赞数量"})
                    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, , 列添加器, 100)
                    读取器 = 指令.执行()
                    While 读取器.读取
                        评论数量 += 读取器(0)
                        点赞数量 += 读取器(1)
                        流星语数量 += 1
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
        文本写入器.Write("共有 " & 流星语数量 & " 个流星语<br>")
        文本写入器.Write("共有 " & 评论数量 & " 条评论<br>")
        文本写入器.Write("共有 " & 点赞数量 & " 个点赞<br>")
        文本写入器.Write("====================<br>")
        文本写入器.Write(Date.Now.ToString)
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

End Class
