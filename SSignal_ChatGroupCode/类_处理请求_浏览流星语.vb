Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Private Structure 流星语_复合数据
        Dim 编号, 转发对象编号 As Long
        Dim 类型 As 流星语类型_常量集合
        Dim 样式 As 流星语列表项样式_常量集合
        Dim 标题, 子域名 As String
        Dim 文本库号 As Short
        Dim 文本编号, 评论数量, 点赞数量 As Long
        Dim 置顶 As Boolean
    End Structure

    Private Structure 查看流星语_复合数据
        Dim 编号, 文本编号, 评论数量, 点赞数量 As Long
        Dim 英语讯宝地址, 标题, 正文, 本国语讯宝地址 As String
        Dim 类型 As 流星语类型_常量集合
        Dim 文本库号 As Short
    End Structure

    Private Structure 评论_复合数据
        Dim 编号, 文本编号, 回复数量, 点赞数量 As Long
        Dim 文本库号 As Short
        Dim 英语讯宝地址, 本国语讯宝地址, 正文 As String
    End Structure

    Private Structure 回复_复合数据
        Dim 编号, 回复对象编号, 文本编号, 点赞数量 As Long
        Dim 文本库号 As Short
        Dim 英语讯宝地址, 本国语讯宝地址, 正文 As String
        Dim 回复对象 As 回复对象_复合数据
    End Structure

    Private Structure 回复对象_复合数据
        Dim 文本编号 As Long
        Dim 文本库号 As Short
        Dim 英语讯宝地址, 本国语讯宝地址, 正文 As String
    End Structure


    Public Function 获取流星语列表() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim GroupID As Long
        If Long.TryParse(Http请求("GroupID"), GroupID) = False Then Return Nothing
        Dim 第几页 As Long
        If Long.TryParse(Http请求("PageNumber"), 第几页) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing

        Dim 流星语() As 流星语_复合数据
        Dim 流星语数量 As Integer
        Dim 总数 As Long
        Const 每页条数 As Integer = 20
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
                ReDim 流星语(每页条数 - 1)
跳转点1:
                结果 = 数据库_获取流星语列表(GroupID, 流星语, 流星语数量, 每页条数, 第几页, 总数)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 流星语数量 = 0 Then
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
        Dim 变长文本 As New StringBuilder(300 * 流星语数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<PAGENUMBER>" & 第几页 & "</PAGENUMBER>")
        文本写入器.Write("<TOTALPAGES>" & Math.Ceiling(总数 / 每页条数) & "</TOTALPAGES>")
        文本写入器.Write("<ROLE>" & 角色 & "</ROLE>")
        If 流星语数量 > 0 Then
            Dim I As Short
            For I = 0 To 流星语数量 - 1
                文本写入器.Write("<METEORRAIN>")
                With 流星语(I)
                    文本写入器.Write("<ID>" & .编号 & "</ID>")
                    文本写入器.Write("<TYPE>" & .类型 & "</TYPE>")
                    文本写入器.Write("<STYLE>" & .样式 & "</STYLE>")
                    文本写入器.Write("<TITLE>" & 替换XML敏感字符(.标题) & "</TITLE>")
                    文本写入器.Write("<DATE>" & Date.FromBinary(.编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                    If .置顶 = True Then 文本写入器.Write("<STICKY/>")
                    Select Case .类型
                        Case 流星语类型_常量集合.图文, 流星语类型_常量集合.视频
                            文本写入器.Write("<COMMENTS>" & .评论数量 & "</COMMENTS>")
                            文本写入器.Write("<LIKES>" & .点赞数量 & "</LIKES>")
                        Case 流星语类型_常量集合.转发
                            文本写入器.Write("<DOMAIN>" & .子域名 & "</DOMAIN>")
                            文本写入器.Write("<FID>" & .转发对象编号 & "</FID>")
                        Case Else
                            Continue For
                    End Select
                End With
                文本写入器.Write("</METEORRAIN>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取流星语列表(ByVal 群编号 As Long, ByRef 流星语() As 流星语_复合数据, ByRef 流星语数量 As Integer,
                                 ByVal 每页条数 As Integer, ByVal 第几页 As Long, ByRef 总数 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"编号", "类型", "样式", "标题", "文本库号", "文本编号", "评论数量", "点赞数量", "置顶"})
            Dim 指令2 As New 类_数据库指令_请求快速获取分页数据(主数据库, "流星语", "#群编号置顶编号", 筛选器, , 列添加器, 第几页, 每页条数)
            Dim 读取器2 As 类_读取器_快速分页 = 指令2.执行()
            总数 = 读取器2.记录总数量
            While 读取器2.读取
                With 流星语(流星语数量)
                    .编号 = 读取器2(0)
                    .类型 = 读取器2(1)
                    .样式 = 读取器2(2)
                    .标题 = 读取器2(3)
                    .文本库号 = 读取器2(4)
                    .文本编号 = 读取器2(5)
                    .评论数量 = 读取器2(6)
                    .点赞数量 = 读取器2(7)
                    .置顶 = 读取器2(8)
                End With
                流星语数量 += 1
            End While
            If 流星语数量 > 0 Then
                Dim 指令 As 类_数据库指令_请求获取数据
                Dim SS包() As Byte = Nothing
                Dim I As Integer
                For I = 0 To 流星语数量 - 1
                    With 流星语(I)
                        If .类型 = 流星语类型_常量集合.转发 Then
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于获取数据("SS包")
                            指令 = New 类_数据库指令_请求获取数据(主数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                            SS包 = Nothing
                            读取器 = 指令.执行()
                            While 读取器.读取
                                SS包 = 读取器(0)
                                Exit While
                            End While
                            读取器.关闭()
                            If SS包 IsNot Nothing Then
                                Dim SS包解读器 As New 类_SS包解读器(SS包)
                                SS包解读器.读取_有标签("D", .子域名)
                                SS包解读器.读取_有标签("I", .转发对象编号)
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

    Public Function 获取流星语() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing

        Dim 流星语 As 查看流星语_复合数据
        Dim 评论() As 评论_复合数据
        Dim 评论数量 As Integer
        Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 群编号 As Long
                Dim 结果 As 类_SS包生成器 = 数据库_获取所属的群(流星语编号, 群编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return XML错误信息(结果.出错提示文本)
                End If
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, 群编号, 角色)
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
                流星语 = New 查看流星语_复合数据
                结果 = 数据库_获取流星语(流星语编号, 流星语)
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
                Const 每页条数 As Integer = 20
                ReDim 评论(每页条数 - 1)
                结果 = 数据库_获取更多评论(EnglishSSAddress, 流星语编号, 0, 评论, 评论数量, 每页条数)
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
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
        If String.IsNullOrEmpty(流星语.英语讯宝地址) = False Then
            Dim 变长文本 As New StringBuilder(1000)
            Dim 文本写入器 As New StringWriter(变长文本)
            文本写入器.Write("<SUCCEED>")
            文本写入器.Write("<TYPE>" & 流星语.类型 & "</TYPE>")
            If 流星语.类型 <> 流星语类型_常量集合.转发 Then
                文本写入器.Write("<ENGLISH2>" & 替换XML敏感字符(流星语.英语讯宝地址) & "</ENGLISH2>")
                If String.IsNullOrEmpty(流星语.本国语讯宝地址) = False Then
                    文本写入器.Write("<NATIVE2>" & 替换XML敏感字符(流星语.本国语讯宝地址) & "</NATIVE2>")
                End If
                文本写入器.Write("<TITLE>" & 替换XML敏感字符(流星语.标题) & "</TITLE>")
                If 流星语.类型 = 流星语类型_常量集合.图文 Then
                    文本写入器.Write("<BODY2>" & 流星语.正文 & "</BODY2>")
                End If
                文本写入器.Write("<COMMENTS>" & 流星语.评论数量 & "</COMMENTS>")
                文本写入器.Write("<LIKES2>" & 流星语.点赞数量 & "</LIKES2>")
                文本写入器.Write("<DATE2>" & Date.FromBinary(流星语编号).AddMinutes(时区偏移量).ToString & "</DATE2>")
                文本写入器.Write("<ROLE>" & 角色 & "</ROLE>")
                If 评论数量 > 0 Then
                    Dim I As Short
                    For I = 0 To 评论数量 - 1
                        文本写入器.Write("<COMMENT>")
                        With 评论(I)
                            文本写入器.Write("<ID>" & .编号 & "</ID>")
                            文本写入器.Write("<ENGLISH>" & .英语讯宝地址 & "</ENGLISH>")
                            If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                                文本写入器.Write("<NATIVE>" & .本国语讯宝地址 & "</NATIVE>")
                            End If
                            文本写入器.Write("<BODY>" & .正文 & "</BODY>")
                            文本写入器.Write("<REPLIES>" & .回复数量 & "</REPLIES>")
                            文本写入器.Write("<LIKES>" & .点赞数量 & "</LIKES>")
                            文本写入器.Write("<DATE>" & Date.FromBinary(.编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                        End With
                        文本写入器.Write("</COMMENT>")
                    Next
                End If
            Else
                文本写入器.Write("<BODY2>" & 流星语.正文 & "</BODY2>")
            End If
            文本写入器.Write("</SUCCEED>")
            文本写入器.Close()
            Return 文本写入器.ToString
        Else
            Return ""
        End If
    End Function

    Private Function 数据库_获取流星语(ByVal 编号 As Long, ByRef 流星语 As 查看流星语_复合数据) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"已删除", "类型", "标题", "文本库号", "文本编号", "评论数量", "点赞数量"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 已删除 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                已删除 = 读取器(0)
                If 已删除 = False Then
                    流星语.类型 = 读取器(1)
                    流星语.标题 = 读取器(2)
                    流星语.文本库号 = 读取器(3)
                    流星语.文本编号 = 读取器(4)
                    流星语.评论数量 = 读取器(5)
                    流星语.点赞数量 = 读取器(6)
                End If
                Exit While
            End While
            读取器.关闭()
            If 已删除 = True Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语.文本编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("SS包")
            指令 = New 类_数据库指令_请求获取数据(主数据库, 流星语.文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
            Dim SS包() As Byte = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                SS包 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If SS包 IsNot Nothing Then
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                SS包解读器.读取_有标签("E", 流星语.英语讯宝地址)
                SS包解读器.读取_有标签("N", 流星语.本国语讯宝地址)
                SS包解读器.读取_有标签("T", 流星语.正文)
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 获取更多评论() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        Dim 早于 As Long
        If Long.TryParse(Http请求("EarlierThan"), 早于) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing

        Dim 评论() As 评论_复合数据
        Dim 评论数量 As Integer
        Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 群编号 As Long
                Dim 结果 As 类_SS包生成器 = 数据库_获取所属的群(流星语编号, 群编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return XML错误信息(结果.出错提示文本)
                End If
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, 群编号, 角色)
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
                Const 每页条数 As Integer = 20
                ReDim 评论(每页条数 - 1)
                结果 = 数据库_获取更多评论(EnglishSSAddress, 流星语编号, 早于, 评论, 评论数量, 每页条数)
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
            Catch ex As Exception
                Return XML错误信息(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return XML失败
        End If
        Dim 变长文本 As New StringBuilder(300 * 评论数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<ROLE>" & 角色 & "</ROLE>")
        If 评论数量 > 0 Then
            Dim I As Short
            For I = 0 To 评论数量 - 1
                文本写入器.Write("<COMMENT>")
                With 评论(I)
                    文本写入器.Write("<ID>" & .编号 & "</ID>")
                    文本写入器.Write("<ENGLISH>" & .英语讯宝地址 & "</ENGLISH>")
                    If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                        文本写入器.Write("<NATIVE>" & .本国语讯宝地址 & "</NATIVE>")
                    End If
                    文本写入器.Write("<BODY>" & .正文 & "</BODY>")
                    文本写入器.Write("<REPLIES>" & .回复数量 & "</REPLIES>")
                    文本写入器.Write("<LIKES>" & .点赞数量 & "</LIKES>")
                    文本写入器.Write("<DATE>" & Date.FromBinary(.编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                End With
                文本写入器.Write("</COMMENT>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取更多评论(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 早于 As Long,
                                ByRef 评论() As 评论_复合数据, ByRef 评论数量 As Integer, ByVal 每页条数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("已删除")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 已删除 As Boolean
            Dim 流星语 As New 查看流星语_复合数据
            读取器 = 指令.执行()
            While 读取器.读取
                已删除 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If 已删除 = True Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            If 早于 > 0 Then 列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.小于, 早于)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"评论编号", "文本库号", "文本编号", "回复数量", "点赞数量"})
            指令 = New 类_数据库指令_请求获取数据(主数据库, "评论", 筛选器, 每页条数, 列添加器, 每页条数, "#是文本")
            读取器 = 指令.执行()
            While 读取器.读取
                With 评论(评论数量)
                    .编号 = 读取器(0)
                    .文本库号 = 读取器(1)
                    .文本编号 = 读取器(2)
                    .回复数量 = 读取器(3)
                    .点赞数量 = 读取器(4)
                End With
                评论数量 += 1
            End While
            读取器.关闭()
            If 评论数量 > 0 Then
                Dim SS包() As Byte = Nothing
                Dim I As Integer
                For I = 0 To 评论数量 - 1
                    With 评论(I)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于获取数据("SS包")
                        指令 = New 类_数据库指令_请求获取数据(主数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                        SS包 = Nothing
                        读取器 = 指令.执行()
                        While 读取器.读取
                            SS包 = 读取器(0)
                            Exit While
                        End While
                        读取器.关闭()
                        If SS包 IsNot Nothing Then
                            Dim SS包解读器 As New 类_SS包解读器(SS包)
                            SS包解读器.读取_有标签("E", .英语讯宝地址)
                            SS包解读器.读取_有标签("N", .本国语讯宝地址)
                            SS包解读器.读取_有标签("T", .正文)
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

    Public Function 获取更多回复() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 流星语编号 As Long
        If Long.TryParse(Http请求("MeteorRainID"), 流星语编号) = False Then Return Nothing
        Dim 评论编号 As Long
        If Long.TryParse(Http请求("CommentID"), 评论编号) = False Then Return Nothing
        Dim 早于 As Long
        If Long.TryParse(Http请求("EarlierThan"), 早于) = False Then Return Nothing
        Dim 时区偏移量 As Integer
        If Integer.TryParse(Http请求("TimezoneOffset"), 时区偏移量) = False Then Return Nothing
        If 时区偏移量 > 720 OrElse 时区偏移量 < -720 Then Return Nothing

        Dim 回复() As 回复_复合数据
        Dim 回复数量 As Integer
        Dim 评论 As New 评论_复合数据
        Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
        If 跨进程锁.WaitOne = True Then
            Try
                Dim 群编号 As Long
                Dim 结果 As 类_SS包生成器 = 数据库_获取所属的群(流星语编号, 群编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return XML错误信息(结果.出错提示文本)
                End If
                结果 = 数据库_验证连接凭据(EnglishSSAddress, Credential, 群编号, 角色)
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
                Const 每页条数 As Integer = 20
                ReDim 回复(每页条数 - 1)
                结果 = 数据库_获取更多回复(EnglishSSAddress, 流星语编号, 评论编号, 早于, 回复, 回复数量, 每页条数)
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
                If 早于 = 0 Then
                    结果 = 数据库_获取某一评论(流星语编号, 评论编号, 评论)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Select Case 结果.查询结果
                            Case 查询结果_常量集合.出错
                                Return XML错误信息(结果.出错提示文本)
                            Case Else
                                Return XML失败
                        End Select
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
        Dim 变长文本 As New StringBuilder(300 * 回复数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<ROLE>" & 角色 & "</ROLE>")
        If 早于 = 0 Then
            文本写入器.Write("<COMMENT>")
            文本写入器.Write("<ENGLISH>" & 评论.英语讯宝地址 & "</ENGLISH>")
            If String.IsNullOrEmpty(评论.本国语讯宝地址) = False Then
                文本写入器.Write("<NATIVE>" & 评论.本国语讯宝地址 & "</NATIVE>")
            End If
            文本写入器.Write("<BODY>" & 评论.正文 & "</BODY>")
            文本写入器.Write("<REPLIES>" & 评论.回复数量 & "</REPLIES>")
            文本写入器.Write("<LIKES>" & 评论.点赞数量 & "</LIKES>")
            文本写入器.Write("<DATE>" & Date.FromBinary(评论编号).AddMinutes(时区偏移量).ToString & "</DATE>")
            文本写入器.Write("</COMMENT>")
        End If
        If 回复数量 > 0 Then
            Dim I As Short
            For I = 0 To 回复数量 - 1
                文本写入器.Write("<REPLY>")
                With 回复(I)
                    文本写入器.Write("<ID>" & .编号 & "</ID>")
                    文本写入器.Write("<ENGLISH>" & .英语讯宝地址 & "</ENGLISH>")
                    If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                        文本写入器.Write("<NATIVE>" & .本国语讯宝地址 & "</NATIVE>")
                    End If
                    文本写入器.Write("<BODY>" & .正文 & "</BODY>")
                    文本写入器.Write("<LIKES>" & .点赞数量 & "</LIKES>")
                    文本写入器.Write("<DATE>" & Date.FromBinary(.编号).AddMinutes(时区偏移量).ToString & "</DATE>")
                    If .回复对象编号 > 0 Then
                        With .回复对象
                            文本写入器.Write("<TOENGLISH>" & .英语讯宝地址 & "</TOENGLISH>")
                            If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                                文本写入器.Write("<TONATIVE>" & .本国语讯宝地址 & "</TONATIVE>")
                            End If
                            文本写入器.Write("<TOBODY>" & .正文 & "</TOBODY>")
                        End With
                    End If
                End With
                文本写入器.Write("</REPLY>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取更多回复(ByVal 英语讯宝地址 As String, ByVal 流星语编号 As Long, ByVal 评论编号 As Long,
                                ByVal 早于 As Long, ByRef 回复() As 回复_复合数据, ByRef 回复数量 As Integer, ByVal 每页条数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 流星语编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("已删除")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 已删除 As Boolean
            Dim 流星语 As New 查看流星语_复合数据
            读取器 = 指令.执行()
            While 读取器.读取
                已删除 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If 已删除 = True Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            If 早于 > 0 Then 列添加器.添加列_用于筛选器("回复编号", 筛选方式_常量集合.小于, 早于)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"回复编号", "回复对象编号", "文本库号", "文本编号", "点赞数量"})
            指令 = New 类_数据库指令_请求获取数据(主数据库, "回复", 筛选器, 每页条数, 列添加器, 每页条数, "#是文本")
            读取器 = 指令.执行()
            While 读取器.读取
                With 回复(回复数量)
                    .编号 = 读取器(0)
                    .回复对象编号 = 读取器(1)
                    .文本库号 = 读取器(2)
                    .文本编号 = 读取器(3)
                    .点赞数量 = 读取器(4)
                End With
                回复数量 += 1
            End While
            读取器.关闭()
            If 回复数量 > 0 Then
                Dim SS包() As Byte = Nothing
                Dim I As Integer
                For I = 0 To 回复数量 - 1
                    With 回复(I)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于获取数据("SS包")
                        指令 = New 类_数据库指令_请求获取数据(主数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                        SS包 = Nothing
                        读取器 = 指令.执行()
                        While 读取器.读取
                            SS包 = 读取器(0)
                            Exit While
                        End While
                        读取器.关闭()
                        If SS包 IsNot Nothing Then
                            Dim SS包解读器 As New 类_SS包解读器(SS包)
                            SS包解读器.读取_有标签("E", .英语讯宝地址)
                            SS包解读器.读取_有标签("N", .本国语讯宝地址)
                            SS包解读器.读取_有标签("T", .正文)
                        End If
                    End With
                Next
                Dim J As Integer
                For I = 0 To 回复数量 - 1
                    With 回复(I)
                        If .回复对象编号 > 0 Then
                            For J = 0 To 回复数量 - 1
                                If J <> I Then
                                    If .回复对象编号 = 回复(J).编号 Then Exit For
                                End If
                            Next
                            If J < 回复数量 Then
                                With .回复对象
                                    .英语讯宝地址 = 回复(J).英语讯宝地址
                                    .本国语讯宝地址 = 回复(J).本国语讯宝地址
                                    .正文 = 回复(J).正文
                                End With
                            Else
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
                                列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
                                列添加器.添加列_用于筛选器("回复编号", 筛选方式_常量集合.等于, .回复对象编号)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号"})
                                指令 = New 类_数据库指令_请求获取数据(主数据库, "回复", 筛选器, 1, 列添加器, , 主键索引名)
                                读取器 = 指令.执行()
                                With .回复对象
                                    While 读取器.读取
                                        .文本库号 = 读取器(0)
                                        .文本编号 = 读取器(1)
                                        Exit While
                                    End While
                                    读取器.关闭()
                                    列添加器 = New 类_列添加器
                                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                                    筛选器 = New 类_筛选器
                                    筛选器.添加一组筛选条件(列添加器)
                                    列添加器 = New 类_列添加器
                                    列添加器.添加列_用于获取数据("SS包")
                                    指令 = New 类_数据库指令_请求获取数据(主数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                                    SS包 = Nothing
                                    读取器 = 指令.执行()
                                    While 读取器.读取
                                        SS包 = 读取器(0)
                                        Exit While
                                    End While
                                    读取器.关闭()
                                    If SS包 IsNot Nothing Then
                                        Dim SS包解读器 As New 类_SS包解读器(SS包)
                                        SS包解读器.读取_有标签("E", .英语讯宝地址)
                                        SS包解读器.读取_有标签("N", .本国语讯宝地址)
                                        SS包解读器.读取_有标签("T", .正文)
                                    End If
                                End With
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

    Private Function 数据库_获取某一评论(ByVal 流星语编号 As Long, ByVal 评论编号 As Long, ByRef 评论 As 评论_复合数据) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
            列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号", "回复数量", "点赞数量"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "评论", 筛选器, 1, 列添加器, , "#流星语评论")
            读取器 = 指令.执行()
            While 读取器.读取
                评论.文本库号 = 读取器(0)
                评论.文本编号 = 读取器(1)
                评论.回复数量 = 读取器(2)
                评论.点赞数量 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 评论.文本编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("SS包")
            指令 = New 类_数据库指令_请求获取数据(主数据库, 评论.文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
            Dim SS包() As Byte = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                SS包 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If SS包 IsNot Nothing Then
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                SS包解读器.读取_有标签("E", 评论.英语讯宝地址)
                SS包解读器.读取_有标签("N", 评论.本国语讯宝地址)
                SS包解读器.读取_有标签("T", 评论.正文)
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
