Imports System.Threading
Imports SSignal_Protocols
Imports SSignalDB

Partial Public Class 类_处理请求

    Private Function 数据库_保存短信(ByVal 手机号 As Long, ByVal 内容 As String) As 类_SS包生成器
跳转点1:
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器.添加列_用于插入数据("手机号", 手机号)
            列添加器.添加列_用于插入数据("内容", 内容)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "短信发送", 列添加器)
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
