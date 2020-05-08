Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

#Region "定义和声明"

    Private Structure 个人发送统计_复合数据
        Dim 今日几号 As Integer
        Dim 今日几时 As Byte
        Dim 今日发送, 昨日发送, 前日发送, 时段发送 As Short
    End Structure

    Private Structure 群发送统计_复合数据
        Dim 今日几号, 今日发送, 昨日发送, 前日发送 As Integer
    End Structure

    Private Structure 需清除的讯宝_复合数据
        Dim 群编号 As Long
        Dim 时间 As Long
        Dim 讯宝指令 As 讯宝指令_常量集合
        Dim 文本 As String
        Dim 文本库号 As Short
        Dim 文本编号 As Long
    End Structure

#End Region

    Public Function 检查是否有新讯宝() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        If Http请求.ContentLength = 0 Then Return Nothing

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                结果 = 数据库_清除讯宝()
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 字节数组(Http请求.ContentLength - 1) As Byte
                Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
                Dim SS包解读器 As New 类_SS包解读器(字节数组)
                Dim SS包解读器3() As Object = SS包解读器.读取_重复标签("GP")
                If SS包解读器3 IsNot Nothing Then
                    Dim 群编号, 晚于 As Long
                    Dim 加入的群(SS包解读器3.Length - 1) As 加入的大聊天群_复合数据
                    Dim 角色 As 群角色_常量集合
                    Dim I As Integer
                    For I = 0 To SS包解读器3.Length - 1
                        With CType(SS包解读器3(I), 类_SS包解读器)
                            .读取_有标签("GI", 群编号, 0)
                            .读取_有标签("LT", 晚于, 0)
                        End With
                        If 群编号 > 0 Then
                            With 加入的群(I)
                                角色 = 群角色_常量集合.无
                                结果 = 数据库_获取角色(群编号, EnglishSSAddress, 角色)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                    Return 结果
                                End If
                                If 角色 >= 群角色_常量集合.成员_不可发言 Then
                                    .群编号 = 群编号
                                    结果 = 数据库_获取新讯宝数量(群编号, 晚于, .新讯宝数量)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                        Return 结果
                                    End If
                                End If
                            End With
                        End If
                    Next
                    Dim 新讯宝数量 As Integer
                    For I = 0 To 加入的群.Length - 1
                        新讯宝数量 += 加入的群(I).新讯宝数量
                    Next
                    If 新讯宝数量 > 0 Then
                        结果 = New 类_SS包生成器(查询结果_常量集合.成功, 加入的群.Length)
                        Call 添加数据_大聊天群返回新讯宝数量(结果, 加入的群)
                    End If
                End If
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

    Private Function 数据库_验证连接凭据(ByVal 英语讯宝地址 As String, ByVal 连接凭据 As String) As 类_SS包生成器
        If String.IsNullOrEmpty(连接凭据) = True Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        If 连接凭据.Length <> 长度_常量集合.连接凭据_客户端 Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If 连接凭据.StartsWith(设备类型_手机) Then
                列添加器.添加列_用于获取数据("连接凭据_手机")
            Else
                列添加器.添加列_用于获取数据("连接凭据_电脑")
            End If
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 连接凭据_数据库中的 As String = ""
            读取器 = 指令.执行()
            While 读取器.读取
                连接凭据_数据库中的 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If String.Compare(连接凭据, 连接凭据_数据库中的) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
            End If
            Return New 类_SS包生成器(查询结果_常量集合.凭据有效)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取新讯宝数量(ByVal 群编号 As Long, ByVal 晚于 As Long, ByRef 新讯宝数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            If 晚于 < 2 Then 晚于 = Date.UtcNow.AddMinutes(-1).Ticks
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.大于, 晚于)
            列添加器.添加列_用于筛选器("指令", 筛选方式_常量集合.大于, 讯宝指令_常量集合.撤回)
            列添加器.添加列_用于筛选器("指令", 筛选方式_常量集合.小于, 讯宝指令_常量集合.视频通话请求)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝", 筛选器, 100, , 100, "#群编号时间")
            读取器 = 指令.执行()
            While 读取器.读取
                新讯宝数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 发送或接收() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If Http请求.ContentLength = 0 Then Return Nothing

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                Dim 本国语讯宝地址 As String = Nothing
                Dim 主机名 As String = Nothing
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色, 本国语讯宝地址, 主机名)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 < 群角色_常量集合.成员_不可发言 Then
                    Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                End If
                结果 = 数据库_清除讯宝()
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 字节数组(Http请求.ContentLength - 1) As Byte
                Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
                Dim SS包解读器 As New 类_SS包解读器(字节数组)
                Dim SS包解读器2 As 类_SS包解读器 = Nothing
                SS包解读器.读取_有标签("POST", SS包解读器2)
                If SS包解读器2 IsNot Nothing Then
                    If 角色 = 群角色_常量集合.成员_不可发言 Then
                        Return New 类_SS包生成器(查询结果_常量集合.不可发言)
                    End If
                    Dim 讯宝指令 As 讯宝指令_常量集合
                    SS包解读器2.读取_有标签("CM", 讯宝指令)
                    If 讯宝指令 >= 讯宝指令_常量集合.域内自定义二级讯宝指令集1 Then
                        Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End If
                    Dim 讯宝文本 As String = Nothing
                    SS包解读器2.读取_有标签("TX", 讯宝文本)
                    If String.IsNullOrEmpty(讯宝文本) = False Then
                        If 讯宝文本.Length > 最大值_常量集合.讯宝文本长度 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End If
                    Dim 宽度, 高度 As Short
                    Dim 秒数 As Byte
                    If 讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                        SS包解读器2.读取_有标签("SC", 秒数)
                        If 秒数 < 1 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End If
                    Select Case 讯宝指令
                        Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                            SS包解读器2.读取_有标签("WD", 宽度)
                            If 宽度 < 1 OrElse 宽度 > 最大值_常量集合.讯宝图片宽高_像素 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                            SS包解读器2.读取_有标签("HT", 高度)
                            If 高度 < 1 OrElse 高度 > 最大值_常量集合.讯宝图片宽高_像素 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End Select
                    Dim 视频预览图片数据() As Byte = Nothing
                    If 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                        SS包解读器2.读取_有标签("PV", 视频预览图片数据)
                        If 视频预览图片数据 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End If
                    Dim 讯宝文件数据() As Byte = Nothing
                    Select Case 讯宝指令
                        Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                            讯宝文本 = 生成文件名_发送语音图片短视频时(Date.UtcNow.Ticks, 讯宝文本)
                            SS包解读器2.读取_有标签("FD", 讯宝文件数据)
                            If 讯宝文件数据 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        Case 讯宝指令_常量集合.发送文件
                            讯宝文本 = 生成文件名_发送文件时(Date.UtcNow.Ticks, 讯宝文本)
                            SS包解读器2.读取_有标签("FD", 讯宝文件数据)
                            If 讯宝文件数据 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                    End Select
                    结果 = 数据库_统计个人发送次数(EnglishSSAddress, 本国语讯宝地址)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    结果 = 数据库_统计群发送次数(GroupID)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                    If 讯宝文件数据 IsNot Nothing Then
                        Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\SS\" & GroupID
                        If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                        If 讯宝指令 = 讯宝指令_常量集合.发送文件 Then
                            Dim SS包解读器3 As New 类_SS包解读器
                            SS包解读器3.解读纯文本(讯宝文本)
                            Dim 存储文件名 As String = ""
                            SS包解读器3.读取_有标签("S", 存储文件名)
                            路径 &= "\" & 存储文件名
                        Else
                            路径 &= "\" & 讯宝文本
                        End If
                        File.WriteAllBytes(路径, 讯宝文件数据)
                        If 讯宝指令 = 讯宝指令_常量集合.发送图片 Then
                            生成预览图片(路径)
                        ElseIf 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                            File.WriteAllBytes(路径 & ".jpg", 视频预览图片数据)
                        End If
                    End If
                    If 讯宝指令 = 讯宝指令_常量集合.撤回 Then
                        Dim 发送时间 As Long = Long.Parse(讯宝文本)
                        If 发送时间 <= Date.FromBinary(发送时间).AddSeconds(-(最大值_常量集合.讯宝可撤回的时限_秒 * 5)).Ticks Then Return Nothing
                        结果 = 数据库_删除讯宝(GroupID, EnglishSSAddress, 发送时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            If 结果.查询结果 = 查询结果_常量集合.失败 Then
                                Return New 类_SS包生成器(查询结果_常量集合.成功)
                            Else
                                Return 结果
                            End If
                        End If
                    End If
                    结果 = 数据库_保存讯宝(GroupID, EnglishSSAddress, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 主机名)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Return 结果
                    End If
                End If
                SS包解读器.读取_有标签("GET", SS包解读器2, Nothing)
                If SS包解读器2 IsNot Nothing Then
                    Dim SS包解读器3() As Object = SS包解读器2.读取_重复标签("GP")
                    If SS包解读器3 IsNot Nothing Then
                        Dim 群编号, 晚于 As Long
                        Dim 加入的群(SS包解读器3.Length - 1) As 加入的大聊天群_复合数据
                        Dim I As Integer
                        For I = 0 To SS包解读器3.Length - 1
                            With CType(SS包解读器3(I), 类_SS包解读器)
                                .读取_有标签("GI", 群编号, 0)
                                .读取_有标签("LT", 晚于, 0)
                            End With
                            If 群编号 > 0 Then
                                With 加入的群(I)
                                    If 群编号 = GroupID Then
                                        .群编号 = 群编号
                                        结果 = 数据库_获取新讯宝(群编号, 晚于, .新讯宝, .新讯宝数量)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                            Return 结果
                                        End If
                                    Else
                                        角色 = 群角色_常量集合.无
                                        结果 = 数据库_获取角色(群编号, EnglishSSAddress, 角色)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                            Return 结果
                                        End If
                                        If 角色 >= 群角色_常量集合.成员_不可发言 Then
                                            .群编号 = 群编号
                                            结果 = 数据库_获取新讯宝(群编号, 晚于, .新讯宝, .新讯宝数量)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                                                Return 结果
                                            End If
                                        End If
                                    End If
                                End With
                            End If
                        Next
                        Dim 新讯宝数量 As Integer
                        For I = 0 To 加入的群.Length - 1
                            新讯宝数量 += 加入的群(I).新讯宝数量
                        Next
                        If 新讯宝数量 > 0 Then
                            结果 = New 类_SS包生成器(查询结果_常量集合.成功, 加入的群.Length)
                            Call 添加数据_大聊天群返回新讯宝(结果, 加入的群)
                        End If
                    End If
                End If
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

    Private Function 数据库_统计个人发送次数(ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 发送统计 As 个人发送统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "个人讯宝统计", 筛选器, 1, , , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                发送统计.今日几号 = 读取器(2)
                发送统计.今日发送 = 读取器(3)
                发送统计.昨日发送 = 读取器(4)
                发送统计.前日发送 = 读取器(5)
                发送统计.今日几时 = 读取器(6)
                发送统计.时段发送 = 读取器(7)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CBool(当前时间.Hour)
        If 发送统计.今日几号 > 0 Then
            If 今日几号 = 发送统计.今日几号 Then
                If 发送统计.今日发送 < 最大值_常量集合.每人每天可发送讯宝数量_大聊天群 Then
                    If 发送统计.时段发送 >= 最大值_常量集合.每人每小时可发送讯宝数量_大聊天群 Then
                        Return New 类_SS包生成器(查询结果_常量集合.本小时发送的讯宝数量已达上限)
                    End If
                    If 发送统计.今日几时 = 今日几时 Then
                        Return 数据库_更新今日个人讯宝统计(英语讯宝地址, 发送统计.今日发送 + 1, 今日几时, 发送统计.时段发送 + 1)
                    Else
                        Return 数据库_更新今日个人讯宝统计(英语讯宝地址, 发送统计.今日发送 + 1, 今日几时, 1)
                    End If
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.今日发送的讯宝数量已达上限)
                End If
            Else
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                If 昨日几号 = 发送统计.今日几号 Then
                    Return 数据库_更新个人讯宝统计(英语讯宝地址, 今日几号, 1, 发送统计.今日发送, 发送统计.昨日发送, 今日几时, 1)
                Else
                    Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                    Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                    If 前日几号 = 发送统计.今日几号 Then
                        Return 数据库_更新个人讯宝统计(英语讯宝地址, 今日几号, 1, 0, 发送统计.今日发送, 今日几时, 1)
                    Else
                        Return 数据库_更新个人讯宝统计(英语讯宝地址, 今日几号, 1, 0, 0, 今日几时, 1)
                    End If
                End If
            End If
        Else
            Return 数据库_添加个人讯宝统计(英语讯宝地址, 本国语讯宝地址, 今日几号, 1, 今日几时, 1)
        End If
    End Function

    Private Function 数据库_统计群发送次数(ByVal 群编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 发送统计 As 群发送统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "群讯宝统计", 筛选器, 1, , , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                发送统计.今日几号 = 读取器(1)
                发送统计.今日发送 = 读取器(2)
                发送统计.昨日发送 = 读取器(3)
                发送统计.前日发送 = 读取器(4)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CBool(当前时间.Hour)
        If 发送统计.今日几号 > 0 Then
            If 今日几号 = 发送统计.今日几号 Then
                Return 数据库_更新今日群讯宝统计(群编号, 发送统计.今日发送 + 1)
            Else
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                If 昨日几号 = 发送统计.今日几号 Then
                    Return 数据库_更新群讯宝统计(群编号, 今日几号, 1, 发送统计.今日发送, 发送统计.昨日发送)
                Else
                    Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                    Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                    If 前日几号 = 发送统计.今日几号 Then
                        Return 数据库_更新群讯宝统计(群编号, 今日几号, 1, 0, 发送统计.今日发送)
                    Else
                        Return 数据库_更新群讯宝统计(群编号, 今日几号, 1, 0, 0)
                    End If
                End If
            End If
        Else
            Return 数据库_添加群讯宝统计(群编号, 今日几号, 1)
        End If
    End Function

    Private Function 数据库_清除讯宝() As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 需清除的讯宝(99) As 需清除的讯宝_复合数据
        Dim 需清除的讯宝数量 As Integer
        Dim 时间 As Long = Date.UtcNow.AddDays(-3).Ticks
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"群编号", "时间", "指令", "文本库号", "文本编号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝", 筛选器, , 列添加器, 100, "#时间")
            读取器 = 指令.执行()
            While 读取器.读取
                If 需清除的讯宝数量 = 需清除的讯宝.Length Then ReDim Preserve 需清除的讯宝(需清除的讯宝数量 * 2 - 1)
                With 需清除的讯宝(需清除的讯宝数量)
                    .群编号 = 读取器(0)
                    .时间 = 读取器(1)
                    .讯宝指令 = 读取器(2)
                    .文本库号 = 读取器(3)
                    .文本编号 = 读取器(4)
                End With
                需清除的讯宝数量 += 1
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, 时间)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "讯宝", 筛选器, "#时间")
            指令2.执行()
            If 需清除的讯宝数量 > 0 Then
                Dim I, J, K As Integer
                K = 需清除的讯宝数量 - 1
                For I = 0 To K
                    With 需清除的讯宝(I)
                        If .文本库号 > 0 Then
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, .时间)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            指令2 = New 类_数据库指令_删除数据(副数据库, .文本库号 & "库", 筛选器, "#时间")
                            指令2.执行()
                            For J = I + 1 To K
                                If .文本库号 = 需清除的讯宝(J).文本库号 Then
                                    需清除的讯宝(J).文本库号 = 0
                                End If
                            Next
                        End If
                    End With
                Next
                Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\SS\"
                Dim 路径2 As String
                For I = 0 To 需清除的讯宝数量 - 1
                    With 需清除的讯宝(I)
                        Select Case .讯宝指令
                            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                路径2 = 路径 & .群编号 & "\" & .文本
                                If File.Exists(路径2) Then
                                    Try
                                        File.Delete(路径2)
                                    Catch ex As Exception
                                    End Try
                                End If
                                路径2 &= ".jpg"
                                If File.Exists(路径2) Then
                                    Try
                                        File.Delete(路径2)
                                    Catch ex As Exception
                                    End Try
                                End If
                            Case 讯宝指令_常量集合.发送语音
                                路径2 = 路径 & .群编号 & "\" & .文本
                                If File.Exists(路径2) Then
                                    Try
                                        File.Delete(路径2)
                                    Catch ex As Exception
                                    End Try
                                End If
                            Case 讯宝指令_常量集合.发送文件
                                Dim SS包解读器2 As New 类_SS包解读器
                                SS包解读器2.解读纯文本(.文本)
                                Dim 存储文件名 As String = ""
                                SS包解读器2.读取_有标签("S", 存储文件名)
                                路径2 = 路径 & .群编号 & "\" & 存储文件名
                                If File.Exists(路径2) Then
                                    Try
                                        File.Delete(路径2)
                                    Catch ex As Exception
                                    End Try
                                End If
                        End Select
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存讯宝(ByVal 群编号 As Long, ByVal 发送者英语地址 As String, ByVal 讯宝指令 As 讯宝指令_常量集合,
                              ByVal 文本 As String, Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0,
                              Optional ByVal 秒数 As Byte = 0, Optional ByVal 主机名 As String = Nothing) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 时间 As Long = Date.UtcNow.Ticks
        Dim 文本库号 As Short
        Dim 文本编号 As Long
跳转点1:
        Try
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
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("时间", 时间)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            If String.IsNullOrEmpty(主机名) = False Then
                列添加器.添加列_用于插入数据("主机名", 主机名)
            End If
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            If 读取器 IsNot Nothing Then 读取器.关闭()
            时间 += 1
            GoTo 跳转点1
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除讯宝(ByVal 群编号 As Long, ByVal 发送者英语地址 As String, ByVal 发送时间 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 发送时间)
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 发送者英语地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝", 筛选器, 1, 列添加器, , "#群编号时间")
            Dim 需清除的讯宝 As 需清除的讯宝_复合数据
            需清除的讯宝.讯宝指令 = 讯宝指令_常量集合.无
            读取器 = 指令.执行()
            While 读取器.读取
                需清除的讯宝.讯宝指令 = 读取器(0)
                需清除的讯宝.文本库号 = 读取器(1)
                需清除的讯宝.文本编号 = 读取器(2)
                Exit While
            End While
            读取器.关闭()
            If 需清除的讯宝.讯宝指令 = 讯宝指令_常量集合.无 Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
            Dim 文本 As String = Nothing
            If 需清除的讯宝.文本库号 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 需清除的讯宝.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("文本")
                指令 = New 类_数据库指令_请求获取数据(副数据库, 需清除的讯宝.文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                读取器 = 指令.执行()
                While 读取器.读取
                    文本 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 需清除的讯宝.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令3 As New 类_数据库指令_删除数据(副数据库, 需清除的讯宝.文本库号 & "库", 筛选器, 主键索引名)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 发送时间)
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 发送者英语地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "讯宝", 筛选器, "#群编号时间")
            指令2.执行()
            Select Case 需清除的讯宝.讯宝指令
                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                    Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\SS\"
                    Dim 路径2 As String
                    路径2 = 路径 & 群编号 & "\" & 文本
                    If File.Exists(路径2) Then
                        Try
                            File.Delete(路径2)
                        Catch ex As Exception
                        End Try
                    End If
                    路径2 &= ".jpg"
                    If File.Exists(路径2) Then
                        Try
                            File.Delete(路径2)
                        Catch ex As Exception
                        End Try
                    End If
                Case 讯宝指令_常量集合.发送语音
                    Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\SS\"
                    Dim 路径2 As String
                    路径2 = 路径 & 群编号 & "\" & 文本
                    If File.Exists(路径2) Then
                        Try
                            File.Delete(路径2)
                        Catch ex As Exception
                        End Try
                    End If
                Case 讯宝指令_常量集合.发送文件
                    Dim SS包解读器2 As New 类_SS包解读器
                    SS包解读器2.解读纯文本(文本)
                    Dim 存储文件名 As String = ""
                    SS包解读器2.读取_有标签("S", 存储文件名)
                    Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\SS\"
                    Dim 路径2 As String
                    路径2 = 路径 & 群编号 & "\" & 存储文件名
                    If File.Exists(路径2) Then
                        Try
                            File.Delete(路径2)
                        Catch ex As Exception
                        End Try
                    End If
            End Select
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取角色(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String, ByRef 角色 As 群角色_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("角色")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "群成员", 筛选器, 1, 列添加器, , "#群编号英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                角色 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取新讯宝(ByVal 群编号 As Long, ByVal 晚于 As Long, ByRef 新讯宝() As 大聊天群新讯宝_复合数据, ByRef 新讯宝数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            If 晚于 < 2 Then 晚于 = Date.UtcNow.AddMinutes(-1).Ticks
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.大于, 晚于)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝", 筛选器, 50, , 50, "#群编号时间")
            ReDim 新讯宝(499)
            读取器 = 指令.执行()
            While 读取器.读取
                If 新讯宝数量 = 新讯宝.Length Then ReDim Preserve 新讯宝(新讯宝数量 * 2 - 1)
                With 新讯宝(新讯宝数量)
                    .群编号 = 群编号
                    .时间 = 读取器(1)
                    .发送者英语地址 = 读取器(2)
                    .主机名 = 读取器(3)
                    .讯宝指令 = 读取器(4)
                    .文本库号 = 读取器(5)
                    .文本编号 = 读取器(6)
                    .宽度 = 读取器(7)
                    .高度 = 读取器(8)
                    .秒数 = 读取器(9)
                End With
                新讯宝数量 += 1
            End While
            读取器.关闭()
            If 新讯宝数量 > 0 Then
                Dim I As Integer
                For I = 0 To 新讯宝数量 - 1
                    With 新讯宝(I)
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
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function


    Public Function 获取成员列表() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return Nothing
        Dim 第几页 As Long
        If Long.TryParse(Http请求("PageNumber"), 第几页) = False Then Return Nothing

        Dim 群成员() As 大聊天群成员_复合数据
        Dim 群成员数量 As Integer
        Dim 总数 As Long
        Const 每页条数 As Integer = 50
        Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If 角色 < 群角色_常量集合.成员_不可发言 Then
                    Return XML无权操作
                End If
                If 第几页 < 1 Then 第几页 = 1
                ReDim 群成员(每页条数 - 1)
跳转点1:
                结果 = 数据库_获取群成员列表(GroupID, 群成员, 群成员数量, 每页条数, 第几页, 总数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 群成员数量 = 0 Then
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
        Dim 变长文本 As New StringBuilder(300 * 群成员数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<PAGENUMBER>" & 第几页 & "</PAGENUMBER>")
        文本写入器.Write("<TOTALPAGES>" & Math.Ceiling(总数 / 每页条数) & "</TOTALPAGES>")
        If 群成员数量 > 0 Then
            Dim I As Short
            For I = 0 To 群成员数量 - 1
                文本写入器.Write("<MEMBER>")
                With 群成员(I)
                    文本写入器.Write("<ENGLISH>" & .英语讯宝地址 & "</ENGLISH>")
                    If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                        文本写入器.Write("<NATIVE>" & .本国语讯宝地址 & "</NATIVE>")
                    End If
                    If String.IsNullOrEmpty(.昵称) = False Then
                        文本写入器.Write("<NAME>" & 替换XML敏感字符(.昵称) & "</NAME>")
                    End If
                    文本写入器.Write("<ROLE>" & .角色 & "</ROLE>")
                    文本写入器.Write("<ICON>" & 替换XML敏感字符(获取讯友头像路径(.英语讯宝地址, .主机名)) & "</ICON>")
                End With
                文本写入器.Write("</MEMBER>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取群成员列表(ByVal 群编号 As Long, ByRef 群成员() As 大聊天群成员_复合数据, ByRef 群成员数 As Integer,
                                 ByVal 每页条数 As Integer, ByVal 第几页 As Long, ByRef 总数 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语讯宝地址", "本国语讯宝地址", "角色", "昵称", "主机名"})
            Dim 指令2 As New 类_数据库指令_请求快速获取分页数据(主数据库, "群成员", "#群编号角色加入时间", 筛选器, , 列添加器, 第几页, 每页条数)
            Dim 读取器2 As 类_读取器_快速分页 = 指令2.执行()
            总数 = 读取器2.记录总数量
            While 读取器2.读取
                With 群成员(群成员数)
                    .英语讯宝地址 = 读取器2(0)
                    .本国语讯宝地址 = 读取器2(1)
                    .角色 = 读取器2(2)
                    .昵称 = 读取器2(3)
                    .主机名 = 读取器2(4)
                End With
                群成员数 += 1
            End While
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 修改昵称() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim Nickname As String = Http请求("Nickname")

        If String.IsNullOrEmpty(Nickname) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If Nickname.Length > 最大值_常量集合.讯友备注字符数 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 < 群角色_常量集合.成员_不可发言 Then
                    Return New 类_SS包生成器(查询结果_常量集合.不是群成员)
                End If
                Dim 操作次数 As Integer
                结果 = 数据库_获取最近操作次数(GroupID, 操作次数, 操作代码_常量集合.修改昵称)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 操作次数 >= 1 Then
                    Return New 类_SS包生成器(查询结果_常量集合.稍后重试)
                End If
                结果 = 数据库_修改昵称(GroupID, EnglishSSAddress, Nickname)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Return 数据库_保存操作记录(GroupID, 操作代码_常量集合.修改昵称)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_修改昵称(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String, ByVal 昵称 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("昵称", 昵称)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("昵称", 筛选方式_常量集合.不等于, 昵称)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "群成员", 列添加器_新数据, 筛选器, "#群编号英语讯宝地址")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 退出大聊天群() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                Dim 本国语讯宝地址 As String = Nothing
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, GroupID, 角色, 本国语讯宝地址)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                If 角色 = 群角色_常量集合.无 Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                End If
                Return 数据库_退出大聊天群(GroupID, EnglishSSAddress)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_退出大聊天群(ByVal 群编号 As Long, ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
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
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
