Imports System.IO
Imports System.Drawing
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Private Structure 回复_常量集合
        Dim 英语讯宝地址, 本国语讯宝地址, 正文 As String
        Dim 文本库号 As Short
        Dim 文本编号 As Long
    End Structure

    Public Function 发布流星语() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        If Http请求.ContentLength = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If EnglishSSAddress.EndsWith(讯宝地址标识 & 域名_英语) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    Return 结果
                End If
                Dim 次数 As Integer
                结果 = 数据库_获取最近发布次数(EnglishSSAddress, 次数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                If 次数 > 最大值_常量集合.小宇宙每日发布流星语次数 Then
                    Return New 类_SS包生成器(查询结果_常量集合.今日发布流星语的次数已达上限)
                End If
                Dim 字节数组(Http请求.ContentLength - 1) As Byte
                Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
                Dim SS包解读器 As New 类_SS包解读器(字节数组)
                Dim 流星语类型 As 流星语类型_常量集合
                SS包解读器.读取_有标签("类型", 流星语类型)
                Dim 标题 As String = Nothing
                SS包解读器.读取_有标签("标题", 标题)
                If String.IsNullOrEmpty(标题) = True Then
                    Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End If
                If 标题.Length > 最大值_常量集合.流星语标题字符数 Then
                    Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End If
                Dim 访问权限 As 流星语访问权限_常量集合
                SS包解读器.读取_有标签("访问权限", 访问权限)
                Dim 讯友标签 As String = Nothing
                If 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                    SS包解读器.读取_有标签("讯友标签", 讯友标签)
                End If
                Dim 样式 As 流星语列表项样式_常量集合
                SS包解读器.读取_有标签("样式", 样式)
                Dim SS包解读器2() As Object = Nothing
                Dim 图片字节数组() As Byte = Nothing
                Dim 视频字节数组() As Byte = Nothing
                Dim SS包() As Byte = Nothing
                Dim 流星语编号 As Long
                Select Case 流星语类型
                    Case 流星语类型_常量集合.图文
                        SS包解读器2 = SS包解读器.读取_重复标签("段落")
                        If SS包解读器2 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        Dim 变长文本 As New StringBuilder(最大值_常量集合.讯宝文本长度)
                        Dim 文本写入器 As New StringWriter(变长文本)
                        Dim 是图片 As Boolean
                        Dim 文本 As String = Nothing
                        Dim I As Integer
                        For I = 0 To SS包解读器2.Length - 1
                            With CType(SS包解读器2(I), 类_SS包解读器)
                                .读取_有标签("是图片", 是图片, False)
                                If 是图片 = False Then
                                    .读取_有标签("文本", 文本, Nothing)
                                    If String.IsNullOrEmpty(文本) = False Then
                                        文本写入器.Write("<P>" & 替换XML敏感字符(文本) & "</P>")
                                    End If
                                Else
                                    .读取_有标签("扩展名", 文本, Nothing)
                                    If String.IsNullOrEmpty(文本) = False Then
                                        If String.Compare(文本, "gif", True) <> 0 Then 文本 = "jpg"
                                        文本写入器.Write("<IMG>" & 替换XML敏感字符(文本) & "</IMG>")
                                    End If
                                End If
                            End With
                        Next
                        If 变长文本.Length = 0 Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                        Dim SS包生成器 As New 类_SS包生成器
                        文本写入器.Close()
                        SS包生成器.添加_有标签("T", 文本写入器.ToString)    '不可添加更多内容，否则有可能无法写入SSignalDB数据库
                        SS包 = SS包生成器.生成SS包
                        If SS包.Length > 8000 Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                    Case 流星语类型_常量集合.视频
                        Select Case 样式
                            Case 流星语列表项样式_常量集合.一幅大图片, 流星语列表项样式_常量集合.一幅小图片
                            Case Else : Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End Select
                        SS包解读器.读取_有标签("视频数据", 视频字节数组)
                        If 视频字节数组 Is Nothing Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                        SS包解读器.读取_有标签("预览图片", 图片字节数组)
                        If 图片字节数组 Is Nothing Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                    Case 流星语类型_常量集合.转发
                        Dim 子域名 As String = Nothing
                        SS包解读器.读取_有标签("子域名", 子域名)
                        If String.IsNullOrEmpty(子域名) = True Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                        If 子域名.Length > 最大值_常量集合.子域名长度 Then
                            Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                        End If
                        SS包解读器.读取_有标签("流星语编号", 流星语编号)
                        Dim SS包生成器 As New 类_SS包生成器
                        SS包生成器.添加_有标签("D", 子域名)
                        SS包生成器.添加_有标签("I", 流星语编号)
                        SS包 = SS包生成器.生成SS包
                    Case Else
                        Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End Select
                Dim 段() As String = EnglishSSAddress.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                流星语编号 = 0
                结果 = 数据库_保存流星语(段(0), 流星语类型, 样式, 标题, SS包, 访问权限, 讯友标签, 流星语编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Select Case 流星语类型
                    Case 流星语类型_常量集合.图文
                        If SS包解读器2 IsNot Nothing Then
                            Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\MR\" & 段(0)
                            If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                            Dim I, 预览图片数量, 预览图片最大数量 As Integer
                            Select Case 样式
                                Case 流星语列表项样式_常量集合.一幅大图片, 流星语列表项样式_常量集合.一幅小图片
                                    预览图片最大数量 = 1
                                Case 流星语列表项样式_常量集合.三幅小图片
                                    预览图片最大数量 = 3
                            End Select
                            Dim 是图片 As Boolean
                            Dim 扩展名 As String = Nothing
                            Dim J As Integer
                            For I = 0 To SS包解读器2.Length - 1
                                SS包解读器 = SS包解读器2(I)
                                SS包解读器.读取_有标签("是图片", 是图片, False)
                                If 是图片 = False Then Continue For
                                SS包解读器.读取_有标签("扩展名", 扩展名, Nothing)
                                If String.IsNullOrEmpty(扩展名) = True Then Continue For
                                SS包解读器.读取_有标签("图片数据", 图片字节数组, Nothing)
                                If 图片字节数组 Is Nothing Then Continue For
                                J += 1
                                If 预览图片数量 < 预览图片最大数量 Then
                                    Select Case 样式
                                        Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.三幅小图片
                                            保存图片(图片字节数组, 路径 & "\" & 流星语编号 & "_" & J & "_pre.jpg", 最大值_常量集合.小宇宙小预览图片宽高)
                                        Case Else
                                            保存图片(图片字节数组, 路径 & "\" & 流星语编号 & "_" & J & "_pre.jpg", 最大值_常量集合.小宇宙大预览图片宽高)
                                    End Select
                                    预览图片数量 += 1
                                End If
                                If String.Compare(扩展名, "gif", True) <> 0 Then
                                    保存图片(图片字节数组, 路径 & "\" & 流星语编号 & "_" & J & ".jpg")
                                Else
                                    File.WriteAllBytes(路径 & "\" & 流星语编号 & "_" & J & ".gif", 图片字节数组)
                                End If
                            Next
                        End If
                    Case 流星语类型_常量集合.视频
                        Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\MR\" & 段(0)
                        If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                        If 样式 = 流星语列表项样式_常量集合.一幅小图片 Then
                            保存图片(图片字节数组, 路径 & "\" & 流星语编号 & ".jpg", 最大值_常量集合.小宇宙小预览图片宽高)
                        Else
                            保存图片(图片字节数组, 路径 & "\" & 流星语编号 & ".jpg", 最大值_常量集合.小宇宙大预览图片宽高)
                        End If
                        File.WriteAllBytes(路径 & "\" & 流星语编号 & ".mp4", 视频字节数组)
                End Select
                Return 数据库_保存操作记录(EnglishSSAddress, 操作代码_常量集合.发布, 流星语编号, 0, 0)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 数据库_获取最近发布次数(ByVal 英语讯宝地址 As String, ByRef 次数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("操作代码", 筛选方式_常量集合.等于, 操作代码_常量集合.发布)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "操作记录", 筛选器, , 列添加器, 100, "#地址时间")
            读取器 = 指令.执行()
            While 读取器.读取
                次数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存流星语(ByVal 英语用户名 As String, ByVal 类型 As 流星语类型_常量集合, ByVal 样式 As 流星语列表项样式_常量集合,
                               ByVal 标题 As String, ByVal SS包() As Byte, ByVal 访问权限 As 流星语访问权限_常量集合, ByVal 讯友标签 As String,
                               ByRef 流星语编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 文本库号 As Short
        Dim 文本编号 As Long
        If SS包 IsNot Nothing Then
            Try
                文本库号 = 获取文本库号(SS包.Length)
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("SS包", SS包)
                Dim 指令3 As New 类_数据库指令_插入新数据(主数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
                Return New 类_SS包生成器(ex.Message)
            End Try
        End If
        流星语编号 = Date.UtcNow.Ticks
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 流星语编号)
            列添加器.添加列_用于插入数据("英语用户名", 英语用户名)
            列添加器.添加列_用于插入数据("类型", 类型)
            列添加器.添加列_用于插入数据("样式", 样式)
            列添加器.添加列_用于插入数据("标题", 标题)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("评论数量", 0)
            列添加器.添加列_用于插入数据("点赞数量", 0)
            列添加器.添加列_用于插入数据("访问权限", 访问权限)
            If String.IsNullOrEmpty(讯友标签) = False Then
                列添加器.添加列_用于插入数据("讯友标签", 讯友标签)
            End If
            列添加器.添加列_用于插入数据("置顶", False)
            列添加器.添加列_用于插入数据("已删除", False)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "流星语", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            流星语编号 += 1
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 评论流星语() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        Dim Text As String = Http请求("Text")
        Dim 时区偏移量 As Integer
        If String.IsNullOrEmpty(Text) = False Then
            If Text.Length > 最大值_常量集合.流星语评论和回复的文字长度 Then Return Nothing
            If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
            If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        End If

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.IsNullOrEmpty(Text) Then
                    Dim 已点赞 As Boolean
                    结果 = 数据库_是否已点赞(EnglishSSAddress, 流星语编号, 0, 0, 已点赞)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        If 结果.查询结果 = 查询结果_常量集合.出错 Then
                            Return XML错误信息(结果.出错提示文本)
                        Else
                            Return XML失败
                        End If
                    End If
                    If 已点赞 = True Then
                        Return XML成功
                    End If
                End If
                Dim 次数 As Integer
                结果 = 数据库_获取最近评论或回复次数(EnglishSSAddress, 次数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 次数 > 最大值_常量集合.小宇宙每日评论或回复次数 Then
                    Return XML已达上限
                End If
                Dim 评论编号 As Long
                结果 = 数据库_评论流星语(流星语编号, EnglishSSAddress, Text, 评论编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Select Case 结果.查询结果
                        Case 查询结果_常量集合.无权操作
                            Return XML无权操作
                        Case 查询结果_常量集合.出错
                            Return XML错误信息(结果.出错提示文本)
                        Case Else
                            Return XML失败
                    End Select
                End If
                If String.IsNullOrEmpty(Text) Then
                    结果 = 数据库_保存操作记录(EnglishSSAddress, 操作代码_常量集合.点赞, 流星语编号, 0, 0)
                Else
                    结果 = 数据库_保存操作记录(EnglishSSAddress, 操作代码_常量集合.评论, 流星语编号, 0, 0)
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Dim 变长文本 As New StringBuilder(100)
                Dim 文本写入器 As New StringWriter(变长文本)
                文本写入器.Write("<SUCCEED>")
                文本写入器.Write("<ID>" & 评论编号 & "</ID>")
                文本写入器.Write("<DATE>" & Date.FromBinary(评论编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                文本写入器.Write("</SUCCEED>")
                文本写入器.Close()
                Return 文本写入器.ToString
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_是否已点赞(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 评论编号 As Long,
                               ByVal 回复对象编号 As Long, ByRef 已点赞 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            列添加器.添加列_用于筛选器("回复对象编号", 筛选方式_常量集合.等于, 回复对象编号)
            列添加器.添加列_用于筛选器("操作代码", 筛选方式_常量集合.等于, 操作代码_常量集合.点赞)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "操作记录", 筛选器, 1, 列添加器, , "#地址操作")
            读取器 = 指令.执行()
            While 读取器.读取
                已点赞 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取最近评论或回复次数(ByVal 英语讯宝地址 As String, ByRef 次数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("操作代码", 筛选方式_常量集合.不等于, 操作代码_常量集合.发布)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "操作记录", 筛选器, , 列添加器, 100, "#地址时间")
            读取器 = 指令.执行()
            While 读取器.读取
                次数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_评论流星语(ByVal 流星语编号 As Long, ByVal 英语讯宝地址 As String, ByVal 文本 As String, ByRef 评论编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 文本库号 As Short
        Dim 文本编号 As Long
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语用户名", "访问权限", "讯友标签", "已删除"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 英语用户名 As String = Nothing
            Dim 访问权限 As 流星语访问权限_常量集合 = 流星语访问权限_常量集合.无
            Dim 讯友标签 As String = Nothing
            Dim 已删除 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                英语用户名 = 读取器(0)
                访问权限 = 读取器(1)
                If 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                    讯友标签 = 读取器(2)
                End If
                已删除 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            If 访问权限 = 流星语访问权限_常量集合.无 OrElse 已删除 = True Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
            Dim SS包生成器 As 类_SS包生成器 = Nothing
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) = False OrElse 英语讯宝地址.StartsWith(英语用户名 & 讯宝地址标识) = False Then
                If 访问权限 = 流星语访问权限_常量集合.只有我 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"本国语讯宝地址", "标签一", "标签二"})
                指令 = New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器, , "#用户名讯友")
                Dim 找到了 As Boolean
                Dim 本国语讯宝地址 As String = Nothing
                Dim 标签一 As String = Nothing
                Dim 标签二 As String = Nothing
                读取器 = 指令.执行()
                While 读取器.读取
                    找到了 = True
                    本国语讯宝地址 = 读取器(0)
                    标签一 = 读取器(1)
                    标签二 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                If 找到了 = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                    If String.Compare(讯友标签, 标签一) <> 0 AndAlso String.Compare(讯友标签, 标签二) <> 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                    End If
                End If
                SS包生成器 = New 类_SS包生成器
                SS包生成器.添加_有标签("E", 英语讯宝地址)
                SS包生成器.添加_有标签("N", 本国语讯宝地址)
                If String.IsNullOrEmpty(文本) = False Then
                    SS包生成器.添加_有标签("T", 文本)
                End If
            ElseIf String.IsNullOrEmpty(文本) = False Then
                SS包生成器 = New 类_SS包生成器
                SS包生成器.添加_有标签("T", 文本)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            End If
            If SS包生成器 IsNot Nothing Then
                Dim SS包() As Byte = SS包生成器.生成SS包
                文本库号 = 获取文本库号(SS包.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("SS包", SS包)
                Dim 指令3 As New 类_数据库指令_插入新数据(主数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        评论编号 = Date.UtcNow.Ticks
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("流星语编号", 流星语编号)
            列添加器.添加列_用于插入数据("评论编号", 评论编号)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("回复数量", 0)
            If String.IsNullOrEmpty(文本) = False Then
                列添加器.添加列_用于插入数据("点赞数量", 0)
            Else
                列添加器.添加列_用于插入数据("点赞数量", -1)
            End If
            列添加器.添加列_用于插入数据("已删除", False)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "评论", 列添加器, True)
            指令2.执行()
            Dim 列添加器_新数据 As New 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                Dim 运算器 As New 类_运算器("评论数量")
                运算器.添加运算指令(运算符_常量集合.加, 1)
                列添加器_新数据.添加列_用于插入数据("评论数量", 运算器)
            Else
                Dim 运算器 As New 类_运算器("点赞数量")
                运算器.添加运算指令(运算符_常量集合.加, 1)
                列添加器_新数据.添加列_用于插入数据("点赞数量", 运算器)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            评论编号 += 1
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存操作记录(ByVal 英语讯宝地址 As String, ByVal 操作代码 As 操作代码_常量集合, ByVal 流星语编号 As Long, ByVal 评论编号 As Long, ByVal 回复对象编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("操作时间", 筛选方式_常量集合.小于, Date.UtcNow.AddHours(-24).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "操作记录", 筛选器, "#操作时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
            列添加器.添加列_用于插入数据("操作代码", 操作代码)
            列添加器.添加列_用于插入数据("操作时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("流星语编号", 流星语编号)
            列添加器.添加列_用于插入数据("评论编号", 评论编号)
            列添加器.添加列_用于插入数据("回复对象编号", 回复对象编号)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "操作记录", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 回复评论() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号, 评论编号, 回复对象编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        If Long.TryParse(Http请求("CommentID"), 评论编号) = False Then Return Nothing
        Dim ReplyID As String = Http请求("ReplyID")
        If String.IsNullOrEmpty(ReplyID) = False Then
            If Long.TryParse(ReplyID, 回复对象编号) = False Then Return Nothing
        End If
        Dim Text As String = Http请求("Text")
        Dim 时区偏移量 As Integer
        If String.IsNullOrEmpty(Text) = False Then
            If Text.Length > 最大值_常量集合.流星语评论和回复的文字长度 Then Return Nothing
            If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
            If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing
        End If

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                If String.IsNullOrEmpty(Text) Then
                    Dim 已点赞 As Boolean
                    结果 = 数据库_是否已点赞(EnglishSSAddress, 流星语编号, 评论编号, 回复对象编号, 已点赞)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        If 结果.查询结果 = 查询结果_常量集合.出错 Then
                            Return XML错误信息(结果.出错提示文本)
                        Else
                            Return XML失败
                        End If
                    End If
                    If 已点赞 = True Then
                        Return XML成功
                    End If
                End If
                Dim 次数 As Integer
                结果 = 数据库_获取最近评论或回复次数(EnglishSSAddress, 次数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 次数 > 最大值_常量集合.小宇宙每日评论或回复次数 Then
                    Return XML已达上限
                End If
                Dim 回复编号 As Long
                Dim 回复 As New 回复_常量集合
                结果 = 数据库_回复评论(流星语编号, 评论编号, 回复对象编号, EnglishSSAddress, Text, 回复编号, 回复)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Select Case 结果.查询结果
                        Case 查询结果_常量集合.无权操作
                            Return XML无权操作
                        Case 查询结果_常量集合.出错
                            Return XML错误信息(结果.出错提示文本)
                        Case Else
                            Return XML失败
                    End Select
                End If
                If String.IsNullOrEmpty(Text) Then
                    结果 = 数据库_保存操作记录(EnglishSSAddress, 操作代码_常量集合.点赞, 流星语编号, 评论编号, 回复对象编号)
                Else
                    结果 = 数据库_保存操作记录(EnglishSSAddress, 操作代码_常量集合.回复, 流星语编号, 评论编号, 回复对象编号)
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Dim 变长文本 As New StringBuilder(300)
                Dim 文本写入器 As New StringWriter(变长文本)
                文本写入器.Write("<SUCCEED>")
                文本写入器.Write("<ID>" & 回复编号 & "</ID>")
                文本写入器.Write("<DATE>" & Date.FromBinary(回复编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                If 回复对象编号 > 0 Then
                    文本写入器.Write("<TOENGLISH>" & 回复.英语讯宝地址 & "</TOENGLISH>")
                    If String.IsNullOrEmpty(回复.本国语讯宝地址) = False Then
                        文本写入器.Write("<TONATIVE>" & 回复.本国语讯宝地址 & "</TONATIVE>")
                    End If
                    文本写入器.Write("<TOBODY>" & 替换XML敏感字符(回复.正文) & "</TOBODY>")
                End If
                文本写入器.Write("</SUCCEED>")
                文本写入器.Close()
                Return 文本写入器.ToString
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_回复评论(ByVal 流星语编号 As Long, ByVal 评论编号 As Long, ByVal 回复对象编号 As Long,
                              ByVal 英语讯宝地址 As String, ByVal 文本 As String, ByRef 回复编号 As Long, ByRef 回复 As 回复_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 文本库号 As Short
        Dim 文本编号 As Long
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语用户名", "访问权限", "讯友标签", "已删除"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 英语用户名 As String = Nothing
            Dim 访问权限 As 流星语访问权限_常量集合 = 流星语访问权限_常量集合.无
            Dim 讯友标签 As String = Nothing
            Dim 已删除 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                英语用户名 = 读取器(0)
                访问权限 = 读取器(1)
                If 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                    讯友标签 = 读取器(2)
                End If
                已删除 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            If 访问权限 = 流星语访问权限_常量集合.无 OrElse 已删除 = True Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"点赞数量", "已删除"})
            指令 = New 类_数据库指令_请求获取数据(主数据库, "评论", 筛选器, 1, 列添加器, , "#流星语评论")
            Dim 找到了 As Boolean
            Dim 点赞数量 As Long
            已删除 = False
            读取器 = 指令.执行()
            While 读取器.读取
                找到了 = True
                点赞数量 = 读取器(0)
                已删除 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If 找到了 = False OrElse 已删除 = True OrElse 点赞数量 < 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
            If 回复对象编号 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
                列添加器.添加列_用于筛选器("回复编号", 筛选方式_常量集合.等于, 回复对象编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号", "点赞数量"})
                指令 = New 类_数据库指令_请求获取数据(主数据库, "回复", 筛选器, 1, 列添加器, , "#流星语评论回复")
                找到了 = False
                点赞数量 = 0
                读取器 = 指令.执行()
                While 读取器.读取
                    找到了 = True
                    回复.文本库号 = 读取器(0)
                    回复.文本编号 = 读取器(1)
                    点赞数量 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                If 找到了 = False OrElse 点赞数量 < 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 回复.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("SS包")
                指令 = New 类_数据库指令_请求获取数据(主数据库, 回复.文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                Dim SS包() As Byte = Nothing
                读取器 = 指令.执行()
                While 读取器.读取
                    SS包 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If SS包 IsNot Nothing Then
                    Dim SS包解读器 As New 类_SS包解读器(SS包)
                    SS包解读器.读取_有标签("E", 回复.英语讯宝地址)
                    SS包解读器.读取_有标签("N", 回复.本国语讯宝地址)
                    SS包解读器.读取_有标签("T", 回复.正文)
                End If
            End If
            Dim SS包生成器 As 类_SS包生成器 = Nothing
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) = False OrElse 英语讯宝地址.StartsWith(英语用户名 & 讯宝地址标识) = False Then
                If 访问权限 = 流星语访问权限_常量集合.只有我 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"本国语讯宝地址", "标签一", "标签二"})
                指令 = New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器, , "#用户名讯友")
                找到了 = False
                Dim 本国语讯宝地址 As String = Nothing
                Dim 标签一 As String = Nothing
                Dim 标签二 As String = Nothing
                读取器 = 指令.执行()
                While 读取器.读取
                    找到了 = True
                    本国语讯宝地址 = 读取器(0)
                    标签一 = 读取器(1)
                    标签二 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                If 找到了 = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                ElseIf 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                    If String.Compare(讯友标签, 标签一) <> 0 AndAlso String.Compare(讯友标签, 标签二) <> 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                    End If
                End If
                SS包生成器 = New 类_SS包生成器
                SS包生成器.添加_有标签("E", 英语讯宝地址)
                SS包生成器.添加_有标签("N", 本国语讯宝地址)
                If String.IsNullOrEmpty(文本) = False Then
                    SS包生成器.添加_有标签("T", 文本)
                End If
            ElseIf String.IsNullOrEmpty(文本) = False Then
                SS包生成器 = New 类_SS包生成器
                SS包生成器.添加_有标签("T", 文本)
            End If
            If SS包生成器 IsNot Nothing Then
                Dim SS包() As Byte = SS包生成器.生成SS包
                文本库号 = 获取文本库号(SS包.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("SS包", SS包)
                Dim 指令3 As New 类_数据库指令_插入新数据(主数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        回复编号 = Date.UtcNow.Ticks
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("流星语编号", 流星语编号)
            列添加器.添加列_用于插入数据("评论编号", 评论编号)
            列添加器.添加列_用于插入数据("回复编号", 回复编号)
            列添加器.添加列_用于插入数据("回复对象编号", 回复对象编号)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            If String.IsNullOrEmpty(文本) = False Then
                列添加器.添加列_用于插入数据("点赞数量", 0)
            Else
                列添加器.添加列_用于插入数据("点赞数量", -1)
            End If
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "回复", 列添加器, True)
            指令2.执行()
            Dim 列添加器_新数据 As New 类_列添加器
            Dim 运算器 As New 类_运算器("回复数量")
            运算器.添加运算指令(运算符_常量集合.加, 1)
            列添加器_新数据.添加列_用于插入数据("回复数量", 运算器)
            If String.IsNullOrEmpty(文本) = True Then
                运算器 = New 类_运算器("点赞数量")
                运算器.添加运算指令(运算符_常量集合.加, 1)
                列添加器_新数据.添加列_用于插入数据("点赞数量", 运算器)
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "评论", 列添加器_新数据, 筛选器, "#流星语评论")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            回复编号 += 1
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 设为置顶() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_设为置顶(EnglishSSAddress, 流星语编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Return XML成功
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_设为置顶(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                Dim 段() As String = 英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                列添加器.添加列_用于筛选器("置顶", 筛选方式_常量集合.大于, 0)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, , 列添加器, 10, "#用户名置顶编号")
                Dim 编号(2) As Long
                Dim 编号数量 As Integer
                读取器 = 指令.执行()
                While 读取器.读取
                    If 编号.Length = 编号数量 Then ReDim Preserve 编号(编号数量 * 2 - 1)
                    编号(编号数量) = 读取器(0)
                    编号数量 += 1
                End While
                读取器.关闭()
                Const 最多置顶数量 As Integer = 3
                Dim 置顶 As Byte
                Dim 列添加器_新数据 As 类_列添加器
                Dim 指令2 As 类_数据库指令_更新数据
                If 编号数量 > 0 Then
                    If 编号(0) = 流星语编号 Then
                        Return New 类_SS包生成器(查询结果_常量集合.成功)
                    End If
                    If 编号数量 < 最多置顶数量 Then
                        置顶 = 编号数量 + 1
                    Else
                        置顶 = 最多置顶数量
                    End If
                    Dim 置顶2 As Byte = 置顶
                    Dim I As Integer
                    For I = 0 To 编号数量 - 1
                        If 编号(I) <> 流星语编号 Then
                            If 置顶2 > 0 Then 置顶2 -= 1
                            列添加器_新数据 = New 类_列添加器
                            列添加器_新数据.添加列_用于插入数据("置顶", 置顶2)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            指令2 = New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                            指令2.执行()
                        End If
                    Next
                Else
                    置顶 = 1
                End If
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("置顶", 置顶)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令2 = New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                指令2.执行()
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 取消置顶() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_取消置顶(EnglishSSAddress, 流星语编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Return XML成功
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_取消置顶(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                Dim 段() As String = 英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                列添加器.添加列_用于筛选器("置顶", 筛选方式_常量集合.大于, 0)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, , 列添加器, 10, "#用户名置顶编号")
                Dim 编号(2) As Long
                Dim 编号数量 As Integer
                读取器 = 指令.执行()
                While 读取器.读取
                    If 编号.Length = 编号数量 Then ReDim Preserve 编号(编号数量 * 2 - 1)
                    编号(编号数量) = 读取器(0)
                    编号数量 += 1
                End While
                读取器.关闭()
                Dim 列添加器_新数据 As 类_列添加器
                Dim 指令2 As 类_数据库指令_更新数据
                If 编号数量 > 0 Then
                    Dim 置顶 As Byte
                    Dim I As Integer
                    For I = 编号数量 - 1 To 0 Step -1
                        If 编号(I) <> 流星语编号 Then
                            置顶 += 1
                            列添加器_新数据 = New 类_列添加器
                            列添加器_新数据.添加列_用于插入数据("置顶", 置顶)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号(I))
                            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                            列添加器.添加列_用于筛选器("置顶", 筛选方式_常量集合.不等于, 置顶)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            指令2 = New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                            指令2.执行()
                        Else
                            列添加器_新数据 = New 类_列添加器
                            列添加器_新数据.添加列_用于插入数据("置顶", 0)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            指令2 = New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                            指令2.执行()
                        End If
                    Next
                End If
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 更改访问权限() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        Dim 访问权限 As 流星语访问权限_常量集合
        If Byte.TryParse(Http请求("Permission"), 访问权限) = False Then Return Nothing
        Dim Tag As String = Nothing
        Select Case 访问权限
            Case 流星语访问权限_常量集合.任何人, 流星语访问权限_常量集合.全部讯友, 流星语访问权限_常量集合.只有我
            Case 流星语访问权限_常量集合.某标签讯友
                Tag = Http请求("Tag")
                If String.IsNullOrEmpty(Tag) = True Then Return XML失败
            Case Else : Return Nothing
        End Select
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_更改访问权限(EnglishSSAddress, 流星语编号, 访问权限, Tag)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Return XML成功
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_更改访问权限(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 访问权限 As 流星语访问权限_常量集合,
                                ByVal 讯友标签 As String) As 类_SS包生成器
        Try
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                Dim 段() As String = 英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("访问权限", 访问权限)
                列添加器_新数据.添加列_用于插入数据("讯友标签", 讯友标签)
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                指令2.执行()
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 删除流星语() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing

        Dim 类型 As 流星语类型_常量集合
        Dim 样式 As 流星语列表项样式_常量集合
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_删除流星语(EnglishSSAddress, 流星语编号, 类型, 样式)
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
        Dim 段() As String = EnglishSSAddress.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\MR\" & 段(0) & "\" & 流星语编号
        Select Case 类型
            Case 流星语类型_常量集合.图文
                Dim 路径2 As String
                Dim I As Integer
                For I = 1 To 100
                    路径2 = 路径 & "_" & I & ".jpg"
                    If File.Exists(路径2) Then
                        Try
                            File.Delete(路径2)
                        Catch ex As Exception
                        End Try
                    Else
                        路径2 = 路径 & "_" & I & ".gif"
                        If File.Exists(路径2) Then
                            Try
                                File.Delete(路径2)
                            Catch ex As Exception
                            End Try
                        Else
                            Exit For
                        End If
                    End If
                Next
                Select Case 样式
                    Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.一幅大图片
                        路径2 = 路径 & "_1_pre" & ".jpg"
                        If File.Exists(路径2) Then
                            Try
                                File.Delete(路径2)
                            Catch ex As Exception
                            End Try
                        End If
                    Case 流星语列表项样式_常量集合.三幅小图片
                        For I = 1 To 3
                            路径2 = 路径 & "_" & I & "_pre" & ".jpg"
                            If File.Exists(路径2) Then
                                Try
                                    File.Delete(路径2)
                                Catch ex As Exception
                                End Try
                            End If
                        Next
                End Select
            Case 流星语类型_常量集合.视频
                Dim 路径2 As String = 路径 & ".jpg"
                If File.Exists(路径2) Then
                    Try
                        File.Delete(路径2)
                    Catch ex As Exception
                    End Try
                End If
                路径2 = 路径 & ".mp4"
                If File.Exists(路径2) Then
                    Try
                        File.Delete(路径2)
                    Catch ex As Exception
                    End Try
                End If
        End Select
        Return XML成功
    End Function

    Private Function 数据库_删除流星语(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByRef 类型 As 流星语类型_常量集合, ByRef 样式 As 流星语列表项样式_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                Dim 段() As String = 英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"类型", "样式"})
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
                读取器 = 指令.执行()
                While 读取器.读取
                    类型 = 读取器(0)
                    样式 = 读取器(1)
                    Exit While
                End While
                读取器.关闭()
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("已删除", True)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 段(0))
                列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
                指令2.执行()
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.无权操作)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 删除评论() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号, 评论编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        If Long.TryParse(Http请求("CommentID"), 评论编号) = False Then Return Nothing

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_删除评论(EnglishSSAddress, 流星语编号, 评论编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Return XML成功
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_删除评论(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 评论编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As 类_列添加器
            Dim 筛选器 As 类_筛选器
            If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("英语用户名")
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 英语用户名 As String = Nothing
                读取器 = 指令.执行()
                While 读取器.读取
                    英语用户名 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 英语讯宝地址.StartsWith(英语用户名 & 讯宝地址标识) = False Then GoTo 跳转点1
            Else
跳转点1:
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号"})
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "评论", 筛选器, 1, 列添加器, , "#流星语评论")
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                读取器 = 指令.执行()
                While 读取器.读取
                    文本库号 = 读取器(0)
                    文本编号 = 读取器(1)
                    Exit While
                End While
                读取器.关闭()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("SS包")
                指令 = New 类_数据库指令_请求获取数据(主数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                Dim SS包() As Byte = Nothing
                读取器 = 指令.执行()
                While 读取器.读取
                    SS包 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                Dim 发布者英语讯宝地址 As String = Nothing
                SS包解读器.读取_有标签("E", 发布者英语讯宝地址)
                If String.Compare(发布者英语讯宝地址, 英语讯宝地址) <> 0 Then
                    Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                End If
            End If
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("已删除", True)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "评论", 列添加器_新数据, 筛选器, "#流星语评论", True)
            指令2.执行()
            列添加器_新数据 = New 类_列添加器
            Dim 运算器 As New 类_运算器("评论数量")
            运算器.添加运算指令(运算符_常量集合.减, 1)
            列添加器_新数据.添加列_用于插入数据("评论数量", 运算器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令2 = New 类_数据库指令_更新数据(主数据库, "流星语", 列添加器_新数据, 筛选器, 主键索引名)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 删除回复() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号, 评论编号, 回复编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        If Long.TryParse(Http请求("CommentID"), 评论编号) = False Then Return Nothing
        If Long.TryParse(Http请求("ReplyID"), 回复编号) = False Then Return Nothing

        If 跨进程锁.WaitOne = True Then
            Try
                Dim 结果 As 类_SS包生成器 = 数据库_验证连接凭据(EnglishSSAddress, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.凭据有效 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML凭据无效
                    End If
                End If
                结果 = 数据库_删除回复(EnglishSSAddress, 流星语编号, 评论编号, 回复编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                Return XML成功
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
    End Function

    Private Function 数据库_删除回复(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 评论编号 As Long, ByVal 回复编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            列添加器.添加列_用于筛选器("回复编号", 筛选方式_常量集合.等于, 回复编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "回复", 筛选器, 1, 列添加器, , "#流星语评论回复")
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                文本库号 = 读取器(0)
                文本编号 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If 文本库号 > 0 Then
                If 英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据("英语用户名")
                    指令 = New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
                    Dim 英语用户名 As String = Nothing
                    读取器 = 指令.执行()
                    While 读取器.读取
                        英语用户名 = 读取器(0)
                        Exit While
                    End While
                    读取器.关闭()
                    If 英语讯宝地址.StartsWith(英语用户名 & 讯宝地址标识) = False Then GoTo 跳转点1
                Else
跳转点1:
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据("SS包")
                    指令 = New 类_数据库指令_请求获取数据(主数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                    Dim SS包() As Byte = Nothing
                    读取器 = 指令.执行()
                    While 读取器.读取
                        SS包 = 读取器(0)
                        Exit While
                    End While
                    读取器.关闭()
                    Dim SS包解读器 As New 类_SS包解读器(SS包)
                    Dim 发布者英语讯宝地址 As String = Nothing
                    SS包解读器.读取_有标签("E", 发布者英语讯宝地址)
                    If String.Compare(发布者英语讯宝地址, 英语讯宝地址) <> 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.无权操作)
                    End If
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_删除数据(主数据库, 文本库号 & "库", 筛选器, 主键索引名, True)
                指令2.执行()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
                列添加器.添加列_用于筛选器("回复编号", 筛选方式_常量集合.等于, 回复编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令2 = New 类_数据库指令_删除数据(主数据库, "回复", 筛选器, "#流星语评论回复", True)
                指令2.执行()
                Dim 列添加器_新数据 As New 类_列添加器
                Dim 运算器 As New 类_运算器("回复数量")
                运算器.添加运算指令(运算符_常量集合.减, 1)
                列添加器_新数据.添加列_用于插入数据("回复数量", 运算器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
                列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令3 As New 类_数据库指令_更新数据(主数据库, "评论", 列添加器_新数据, 筛选器, "#流星语评论")
                指令3.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Sub 保存图片(ByVal 图片字节数组() As Byte, ByVal 保存路径 As String, Optional ByVal 宽高最大值 As Integer = 0)
        Dim 原图 As Bitmap = Nothing
        Dim 预览图片 As Bitmap = Nothing
        Dim 内存流 As MemoryStream = Nothing
        Try
            内存流 = New MemoryStream(图片字节数组)
            原图 = Bitmap.FromStream(内存流)
            内存流.Close()
            内存流 = Nothing
            If 宽高最大值 > 0 Then
                If 原图.Width > 宽高最大值 OrElse 原图.Height > 宽高最大值 Then
                    Dim 缩小比例 As Double
                    If 原图.Height > 原图.Width Then
                        缩小比例 = 宽高最大值 / 原图.Height
                    Else
                        缩小比例 = 宽高最大值 / 原图.Width
                    End If
                    预览图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
                Else
                    预览图片 = New Bitmap(原图.Width, 原图.Height)
                End If
            Else
                预览图片 = New Bitmap(原图.Width, 原图.Height)
            End If
            Dim 绘图器 As Graphics = Graphics.FromImage(预览图片)
            绘图器.FillRectangle(New SolidBrush(Color.White), 0, 0, 预览图片.Width, 预览图片.Height)
            绘图器.DrawImage(原图, 0, 0, 预览图片.Width, 预览图片.Height)
            原图.Dispose()
            绘图器.Dispose()
            预览图片.Save(保存路径, Imaging.ImageFormat.Jpeg)
            预览图片.Dispose()
        Catch ex As Exception
            If 内存流 IsNot Nothing Then 内存流.Close()
            If 原图 IsNot Nothing Then 原图.Dispose()
            If 预览图片 IsNot Nothing Then 预览图片.Dispose()
            Throw ex
        End Try
    End Sub

End Class
