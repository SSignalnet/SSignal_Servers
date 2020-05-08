﻿Imports System.Web
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_ServerCommonCode

Public Class 类_打开或创建数据库

    Const 常用数据缓存区大小_兆 As Integer = 100

    Public Function 打开或创建主数据库(ByVal Context As HttpContext, ByVal 副数据库 As 类_数据库) As 类_数据库
        Dim 目录路径 As String = Context.Server.MapPath("/") & "App_Data"
        If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
        Dim 文件名 As String = "TinyUniverseData_RequireBackup"
        Dim 文件路径 As String = 目录路径 & "\" & 文件名 & 数据库文件扩展名
        Const 在副数据库独一无二的名称 As String = "主数据库"
        Dim 数据库 As 类_数据库 = Nothing
        Dim 数据库类型 As 数据库类型_常量集合 = 数据库类型_常量集合.长期_需备份
        Const 新页缓存文件长度限制_兆 As Integer = 10    '此处不要太大
        If File.Exists(文件路径) = True Then
            数据库 = New 类_数据库(文件路径, 数据库类型, , 副数据库, 在副数据库独一无二的名称)
            Try
                数据库.打开(常用数据缓存区大小_兆, 类_数据库.新页缓存文件的处理方式_常量集合.读取, 新页缓存文件长度限制_兆)
            Catch ex As Exception
                数据库.关闭()
                数据库 = Nothing
            End Try
        Else
            Try
                文件路径 = 创建数据库文件(目录路径, 文件名, 数据库类型, True)
                数据库 = New 类_数据库(文件路径, 数据库类型, , 副数据库, 在副数据库独一无二的名称)
                数据库.打开(常用数据缓存区大小_兆, 类_数据库.新页缓存文件的处理方式_常量集合.不读取, 新页缓存文件长度限制_兆)
            Catch ex As Exception
                If 数据库 IsNot Nothing Then
                    数据库.关闭()
                    数据库 = Nothing
                End If
            End Try
        End If
        If 数据库 IsNot Nothing Then
            Try
                Call 创建表_商品(数据库)
                Call 创建表_讯友录(数据库)
                Call 创建表_流星语(数据库)
                Call 创建表_评论(数据库)
                Call 创建表_回复(数据库)
                Dim I, 最大字节数, 最大字节数2 As Integer
                For I = 30 To 8000  '8000是SSignalDB数据库binary类型数据最大长度
                    最大字节数 = 获取小宇宙文本库号(I)
                    If 最大字节数 <> 最大字节数2 Then
                        最大字节数2 = 最大字节数
                        Call 创建表_小宇宙文本库(数据库, 最大字节数)
                    End If
                Next
            Catch ex As Exception
                数据库.关闭()
                数据库 = Nothing
            End Try
        End If
        Return 数据库
    End Function

    Private Sub 创建表_讯友录(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "讯友录"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("英语用户名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.英语用户名长度)
            列添加器.添加列_用于创建表("英语讯宝地址", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.讯宝和电子邮箱地址长度)
            列添加器.添加列_用于创建表("本国语讯宝地址", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.讯宝和电子邮箱地址长度, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("标签一", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.讯友标签字符数, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("标签二", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.讯友标签字符数, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("拉黑", 数据类型_常量集合.bit_位)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#用户名讯友", True)
            索引.添加排序列("英语用户名")
            索引.添加排序列("英语讯宝地址")
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#用户标签一")
            索引.添加排序列("英语用户名")
            索引.添加排序列("标签一")
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("标签一", 筛选方式_常量集合.不为空, Nothing)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#用户标签二")
            索引.添加排序列("英语用户名")
            索引.添加排序列("标签二")
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("标签二", 筛选方式_常量集合.不为空, Nothing)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_商品(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "商品"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("编号", 数据类型_常量集合.bigint_长整数, 值许可_常量集合.是主键_升序)
            列添加器.添加列_用于创建表("样式", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("标题", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.流星语标题字符数)
            列添加器.添加列_用于创建表("价格", 数据类型_常量集合.double_双精度浮点数)
            列添加器.添加列_用于创建表("币种", 数据类型_常量集合.nvarchar_双字节变长短文本, 3)
            列添加器.添加列_用于创建表("详情", 数据类型_常量集合.ntext_双字节长文本)
            列添加器.添加列_用于创建表("冒泡时间", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("已删除", 数据类型_常量集合.bit_位)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#冒泡时间")
            索引.添加排序列("冒泡时间", True)
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引.文字列_保存在索引中用于快速查询文字 = "标题"
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#已删除的")
            索引.添加排序列("编号")
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, True)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_流星语(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "流星语"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("编号", 数据类型_常量集合.bigint_长整数, 值许可_常量集合.是主键_升序)
            列添加器.添加列_用于创建表("英语用户名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.英语用户名长度)
            列添加器.添加列_用于创建表("类型", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("样式", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("标题", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.流星语标题字符数)
            列添加器.添加列_用于创建表("文本库号", 数据类型_常量集合.smallint_短整数)
            列添加器.添加列_用于创建表("文本编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("评论数量", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("点赞数量", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("访问权限", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("讯友标签", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.讯友标签字符数, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("置顶", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("已删除", 数据类型_常量集合.bit_位)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#用户名置顶编号")
            索引.添加排序列("英语用户名")
            索引.添加排序列("置顶", True)
            索引.添加排序列("编号", True)
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引.文字列_保存在索引中用于快速查询文字 = "标题"
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#用户名置顶编号任何人")
            索引.添加排序列("英语用户名")
            索引.添加排序列("置顶", True)
            索引.添加排序列("编号", True)
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("访问权限", 筛选方式_常量集合.等于, 流星语访问权限_常量集合.任何人)
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#已删除的")
            索引.添加排序列("编号")
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, True)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_评论(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "评论"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("流星语编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("评论编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("文本库号", 数据类型_常量集合.smallint_短整数)
            列添加器.添加列_用于创建表("文本编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("回复数量", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("点赞数量", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("已删除", 数据类型_常量集合.bit_位)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#流星语评论", True)
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#是文本")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号", True)
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("点赞数量", 筛选方式_常量集合.大于等于, 0)
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#是点赞")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号", True)
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("点赞数量", 筛选方式_常量集合.小于, 0)
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#已删除的")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, True)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_回复(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "回复"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("流星语编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("评论编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("回复编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("回复对象编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("文本库号", 数据类型_常量集合.smallint_短整数)
            列添加器.添加列_用于创建表("文本编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("点赞数量", 数据类型_常量集合.bigint_长整数)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#流星语评论回复", True)
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            索引.添加排序列("回复编号")
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#是文本")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            索引.添加排序列("回复编号", True)
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("点赞数量", 筛选方式_常量集合.大于等于, 0)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#是点赞")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            索引.添加排序列("回复编号", True)
            列添加器2 = New 类_列添加器
            列添加器2.添加列_用于筛选器("点赞数量", 筛选方式_常量集合.小于, 0)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_小宇宙文本库(ByVal 数据库 As 类_数据库, ByVal 最大字节数 As Short)
        Dim 表名 As String = 最大字节数 & "库"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("编号", 数据类型_常量集合.bigint_长整数, 值许可_常量集合.是主键_升序)
            列添加器.添加列_用于创建表("SS包", 数据类型_常量集合.binary_字节数组, 最大字节数)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器)
            指令.执行()
        End If
    End Sub


    Public Function 打开或创建副数据库(ByVal Context As HttpContext) As 类_数据库
        Dim 目录路径 As String = Context.Server.MapPath("/") & "App_Data"
        If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
        Dim 文件名 As String = "TinyUniverseData_NoBackup"
        Dim 文件路径 As String = 目录路径 & "\" & 文件名 & 数据库文件扩展名
        Dim 数据库 As 类_数据库 = Nothing
        Dim 数据库类型 As 数据库类型_常量集合 = 数据库类型_常量集合.长期_无需备份
        Const 新页缓存文件长度限制_兆 As Integer = 100
        If File.Exists(文件路径) = True Then
            数据库 = New 类_数据库(文件路径, 数据库类型)
            Try
                数据库.打开(常用数据缓存区大小_兆, 类_数据库.新页缓存文件的处理方式_常量集合.读取, 新页缓存文件长度限制_兆)
            Catch ex As Exception
                数据库.关闭()
                数据库 = Nothing
            End Try
        Else
            Try
                文件路径 = 创建数据库文件(目录路径, 文件名, 数据库类型, True)
                数据库 = New 类_数据库(文件路径, 数据库类型)
                数据库.打开(常用数据缓存区大小_兆, 类_数据库.新页缓存文件的处理方式_常量集合.不读取, 新页缓存文件长度限制_兆)
            Catch ex As Exception
                If 数据库 IsNot Nothing Then
                    数据库.关闭()
                    数据库 = Nothing
                End If
            End Try
        End If
        If 数据库 IsNot Nothing Then
            Try
                Dim 类 As New 类_创建共有的表
                Call 类.创建表_服务器(数据库)
                Call 类.创建表_系统任务(数据库)
                Call 创建表_用户(数据库)
                Call 创建表_操作记录(数据库)
                Call 创建表_传送服务器(数据库)
            Catch ex As Exception
                数据库.关闭()
                数据库 = Nothing
            End Try
        End If
        Return 数据库
    End Function

    Private Sub 创建表_用户(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "用户"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("英语讯宝地址", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.讯宝和电子邮箱地址长度, 值许可_常量集合.是主键_升序)
            列添加器.添加列_用于创建表("本国语讯宝地址", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.讯宝和电子邮箱地址长度, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("主机名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.主机名字符数)
            列添加器.添加列_用于创建表("英语域名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.域名长度)
            列添加器.添加列_用于创建表("本国语域名", 数据类型_常量集合.nvarchar_双字节变长短文本, 最大值_常量集合.域名长度, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("连接凭据_电脑", 数据类型_常量集合.char_单字节定长短文本, 长度_常量集合.连接凭据_客户端, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("连接凭据_手机", 数据类型_常量集合.char_单字节定长短文本, 长度_常量集合.连接凭据_客户端, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("网络地址_电脑", 数据类型_常量集合.binary_字节数组, 长度_常量集合.网络地址, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("网络地址_手机", 数据类型_常量集合.binary_字节数组, 长度_常量集合.网络地址, 值许可_常量集合.可为空)
            列添加器.添加列_用于创建表("访问时间", 数据类型_常量集合.bigint_长整数)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#访问时间")
            索引.添加排序列("访问时间", True)
            索引.添加排序列("英语讯宝地址")
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#英语域名")
            索引.添加排序列("英语域名")
            索引.添加排序列("访问时间", True)
            索引.添加排序列("英语讯宝地址")
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#本国语域名")
            索引.添加排序列("本国语域名")
            索引.添加排序列("访问时间", True)
            索引.添加排序列("本国语讯宝地址")
            Dim 列添加器2 As New 类_列添加器
            列添加器2.添加列_用于筛选器("本国语域名", 筛选方式_常量集合.不为空, Nothing)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器2)
            索引.添加筛选器(筛选器)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_操作记录(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "操作记录"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("英语讯宝地址", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.讯宝和电子邮箱地址长度)
            列添加器.添加列_用于创建表("操作代码", 数据类型_常量集合.tinyint_字节)
            列添加器.添加列_用于创建表("操作时间", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("流星语编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("评论编号", 数据类型_常量集合.bigint_长整数)
            列添加器.添加列_用于创建表("回复对象编号", 数据类型_常量集合.bigint_长整数)

            Dim 索引添加器 As New 类_索引添加器

            Dim 索引 As New 类_索引("#操作时间")
            索引.添加排序列("操作时间", True)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#地址时间")
            索引.添加排序列("英语讯宝地址")
            索引.添加排序列("操作时间", True)
            索引添加器.添加索引(索引)

            索引 = New 类_索引("#地址操作")
            索引.添加排序列("英语讯宝地址")
            索引.添加排序列("流星语编号")
            索引.添加排序列("评论编号")
            索引.添加排序列("回复对象编号")
            索引.添加排序列("操作时间", True)
            索引添加器.添加索引(索引)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器, 索引添加器)
            指令.执行()
        End If
    End Sub

    Private Sub 创建表_传送服务器(ByVal 数据库 As 类_数据库)
        Const 表名 As String = "传送服务器"
        Dim 指令2 As New 类_数据库指令_查找表(数据库, 表名)
        If 指令2.执行 Is Nothing Then
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于创建表("英语用户名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.英语用户名长度, 值许可_常量集合.是主键_升序)
            列添加器.添加列_用于创建表("主机名", 数据类型_常量集合.varchar_单字节变长短文本, 最大值_常量集合.主机名字符数)

            Dim 指令 As New 类_数据库指令_创建表(数据库, 表名, 列添加器)
            指令.执行()
        End If
    End Sub

End Class
