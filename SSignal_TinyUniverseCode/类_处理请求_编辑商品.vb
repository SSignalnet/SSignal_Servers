Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 发布商品() As 类_SS包生成器
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
                Dim 字节数组(Http请求.ContentLength - 1) As Byte
                Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
                Dim SS包解读器 As New 类_SS包解读器(字节数组)
                Dim 标题 As String = Nothing
                SS包解读器.读取_有标签("标题", 标题)
                If String.IsNullOrEmpty(标题) = True Then
                    Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End If
                If 标题.Length > 最大值_常量集合.流星语标题字符数 Then
                    Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End If
                Dim 样式 As 流星语列表项样式_常量集合
                SS包解读器.读取_有标签("样式", 样式)
                Dim 价格 As Double
                SS包解读器.读取_有标签("价格", 价格)
                Dim 币种 As String = Nothing
                SS包解读器.读取_有标签("币种", 币种)
                Dim 购买链接 As String = Nothing
                SS包解读器.读取_有标签("购买链接", 购买链接)
                Dim SS包解读器2() As Object = Nothing
                Dim 图片字节数组() As Byte = Nothing
                Dim SS包() As Byte = Nothing
                Dim 商品编号 As Long
                SS包解读器2 = SS包解读器.读取_重复标签("段落")
                If SS包解读器2 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                Dim 变长文本 As New StringBuilder(最大值_常量集合.讯宝文本长度)
                Dim 文本写入器 As New StringWriter(变长文本)
                文本写入器.Write("<BUY>" & 替换XML敏感字符(购买链接) & "</BUY>")
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
                文本写入器.Close()
                商品编号 = 0
                结果 = 数据库_保存商品(样式, 标题, 价格, 币种, 文本写入器.ToString, 商品编号)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\GD"
                If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                Dim 预览图片数量, 预览图片最大数量 As Integer
                Select Case 样式
                    Case 流星语列表项样式_常量集合.一幅大图片, 流星语列表项样式_常量集合.一幅小图片
                        预览图片最大数量 = 1
                    Case 流星语列表项样式_常量集合.三幅小图片
                        预览图片最大数量 = 3
                End Select
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
                                保存图片(图片字节数组, 路径 & "\" & 商品编号 & "_" & J & "_pre.jpg", 最大值_常量集合.小宇宙小预览图片宽高)
                            Case Else
                                保存图片(图片字节数组, 路径 & "\" & 商品编号 & "_" & J & "_pre.jpg", 最大值_常量集合.小宇宙大预览图片宽高)
                        End Select
                        预览图片数量 += 1
                    End If
                    If String.Compare(扩展名, "gif", True) <> 0 Then
                        保存图片(图片字节数组, 路径 & "\" & 商品编号 & "_" & J & ".jpg")
                    Else
                        File.WriteAllBytes(路径 & "\" & 商品编号 & "_" & J & ".gif", 图片字节数组)
                    End If
                Next
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

    Private Function 数据库_保存商品(ByVal 样式 As 流星语列表项样式_常量集合, ByVal 标题 As String, ByVal 价格 As Double,
                              ByVal 币种 As String, ByVal 详情 As String, ByRef 商品编号 As Long) As 类_SS包生成器
        商品编号 = Date.UtcNow.Ticks
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 商品编号)
            列添加器.添加列_用于插入数据("样式", 样式)
            列添加器.添加列_用于插入数据("标题", 标题)
            列添加器.添加列_用于插入数据("价格", 价格)
            If String.IsNullOrEmpty(币种) = False Then
                列添加器.添加列_用于插入数据("币种", 币种)
            Else
                列添加器.添加列_用于插入数据("币种", "")
            End If
            列添加器.添加列_用于插入数据("详情", 详情)
            列添加器.添加列_用于插入数据("冒泡时间", 商品编号)
            列添加器.添加列_用于插入数据("已删除", False)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "商品", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            商品编号 += 1
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 商品移至最前() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 商品编号 As Long
        If Long.TryParse(Http请求("GoodsID"), 商品编号) = False Then Return Nothing

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
                结果 = 数据库_商品移至最前(商品编号)
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

    Private Function 数据库_商品移至最前(ByVal 商品编号 As Long) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("冒泡时间", Date.UtcNow.Ticks)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 商品编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "商品", 列添加器_新数据, 筛选器, 主键索引名)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 删除商品() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        Dim Credential As String = Http请求("Credential")
        Dim 商品编号 As Long
        If Long.TryParse(Http请求("GoodsID"), 商品编号) = False Then Return Nothing

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
                结果 = 数据库_删除商品(商品编号, 样式)
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
        Dim 路径 As String = Context.Server.MapPath("/") & "App_Data\GD\" & 商品编号
        Try
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
        Catch ex As Exception
        End Try
        Return XML成功
    End Function

    Private Function 数据库_删除商品(ByVal 商品编号 As Long, ByRef 样式 As 流星语列表项样式_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 商品编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("样式")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "商品", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                样式 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("已删除", True)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 商品编号)
            列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "商品", 列添加器_新数据, 筛选器, 主键索引名)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
