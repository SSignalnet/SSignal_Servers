Imports System.Threading
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Partial Public Class 类_处理请求

    Private Function 数据库_保存邮件(ByVal 邮件 As 类_邮件) As 类_SS包生成器
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("收件人", 邮件.收件人)
            列添加器.添加列_用于插入数据("标题", 邮件.标题)
            列添加器.添加列_用于插入数据("正文", 邮件.正文)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "邮件发送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Thread.Sleep(10)
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
