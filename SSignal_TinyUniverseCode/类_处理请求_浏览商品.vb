Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Private Structure 商品_复合数据
        Dim 编号 As Long
        Dim 样式 As 流星语列表项样式_常量集合
        Dim 标题, 币种 As String
        Dim 价格 As Double
    End Structure

    Private Structure 查看商品_复合数据
        Dim 标题, 币种, 详情 As String
        Dim 价格 As Double
    End Structure

    Public Function 获取商品列表() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 第几页 As Long
        If Long.TryParse(Http请求("PageNumber"), 第几页) = False Then Return Nothing

        Dim 商品() As 商品_复合数据
        Dim 商品数量 As Integer
        Dim 总数 As Long
        Const 每页条数 As Integer = 20
        Dim 是作者 As Boolean
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
                If 第几页 < 1 Then 第几页 = 1
                ReDim 商品(每页条数 - 1)
跳转点1:
                结果 = 数据库_获取商品列表(商品, 商品数量, 每页条数, 第几页, 总数, 是作者)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    If 结果.查询结果 = 查询结果_常量集合.出错 Then
                        Return XML错误信息(结果.出错提示文本)
                    Else
                        Return XML失败
                    End If
                End If
                If 商品数量 = 0 Then
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
        Dim 变长文本 As New StringBuilder(300 * 商品数量)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<PAGENUMBER>" & 第几页 & "</PAGENUMBER>")
        文本写入器.Write("<TOTALPAGES>" & Math.Ceiling(总数 / 每页条数) & "</TOTALPAGES>")
        If 商品数量 > 0 Then
            Dim I As Short
            For I = 0 To 商品数量 - 1
                文本写入器.Write("<GOODS>")
                With 商品(I)
                    文本写入器.Write("<ID>" & .编号 & "</ID>")
                    文本写入器.Write("<STYLE>" & .样式 & "</STYLE>")
                    文本写入器.Write("<TITLE>" & 替换XML敏感字符(.标题) & "</TITLE>")
                    文本写入器.Write("<PRICE>" & .价格 & "</PRICE>")
                    文本写入器.Write("<CURRENCY>" & 替换XML敏感字符(.币种) & "</CURRENCY>")
                End With
                文本写入器.Write("</GOODS>")
            Next
        End If
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取商品列表(ByRef 商品() As 商品_复合数据, ByRef 商品数量 As Integer, ByVal 每页条数 As Integer,
                                ByVal 第几页 As Long, ByRef 总数 As Long, ByRef 是作者 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"编号", "样式", "标题", "价格", "币种"})
            Dim 指令2 As New 类_数据库指令_请求快速获取分页数据(主数据库, "商品", "#冒泡时间", Nothing, , 列添加器, 第几页, 每页条数)
            Dim 读取器2 As 类_读取器_快速分页 = 指令2.执行()
            总数 = 读取器2.记录总数量
            While 读取器2.读取
                With 商品(商品数量)
                    .编号 = 读取器2(0)
                    .样式 = 读取器2(1)
                    .标题 = 读取器2(2)
                    .价格 = 读取器2(3)
                    .币种 = 读取器2(4)
                End With
                商品数量 += 1
            End While
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Public Function 查看商品() As String
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return XML数据库未就绪
        Dim EnglishSSAddress As String = Http请求("EnglishSSAddress")
        If EnglishSSAddress.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return Nothing
        Dim Credential As String = Http请求("Credential")
        Dim 商品编号 As Long
        If Long.TryParse(Http请求("GoodsID"), 商品编号) = False Then Return Nothing

        Dim 商品 As 查看商品_复合数据
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
                商品 = New 查看商品_复合数据
                结果 = 数据库_获取商品(商品编号, 商品)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Select Case 结果.查询结果
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
        Dim 变长文本 As New StringBuilder(1000)
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write("<SUCCEED>")
        文本写入器.Write("<TITLE>" & 替换XML敏感字符(商品.标题) & "</TITLE>")
        文本写入器.Write("<PRICE>" & 商品.价格 & "</PRICE>")
        文本写入器.Write("<CURRENCY>" & 替换XML敏感字符(商品.币种) & "</CURRENCY>")
        文本写入器.Write("<BODY>" & 商品.详情 & "</BODY>")
        文本写入器.Write("</SUCCEED>")
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Private Function 数据库_获取商品(ByVal 编号 As Long, ByRef 商品 As 查看商品_复合数据) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"已删除", "标题", "价格", "币种", "详情"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "商品", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 已删除 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                已删除 = 读取器(0)
                If 已删除 = False Then
                    商品.标题 = 读取器(1)
                    商品.价格 = 读取器(2)
                    商品.币种 = 读取器(3)
                    商品.详情 = 读取器(4)
                End If
                Exit While
            End While
            读取器.关闭()
            If 已删除 = True Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
