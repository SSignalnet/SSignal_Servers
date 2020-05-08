Imports SSignal_Protocols
Imports SSignalDB

Partial Public Class 类_讯宝管理器

    Private Function 数据库_发送讯宝时获取群成员(ByVal 用户编号 As Long, ByVal 群编号 As Byte,
                                    ByRef 群成员() As 小聊天群成员_复合数据, ByRef 群成员数 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语讯宝地址", "本国语讯宝地址", "主机名", "位置号", "角色"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "小聊天群成员", 筛选器,  , 列添加器, 最大值_常量集合.小聊天群成员数量, "#群主编号角色加入时间")
            ReDim 群成员(最大值_常量集合.小聊天群成员数量 - 1)
            读取器 = 指令.执行()
            While 读取器.读取
                If 群成员数 = 群成员.Length Then ReDim Preserve 群成员(群成员数 * 2 - 1)
                With 群成员(群成员数)
                    .英语讯宝地址 = 读取器(0)
                    .本国语讯宝地址 = 读取器(1)
                    .主机名 = 读取器(2)
                    .位置号 = 读取器(3)
                    .角色 = 读取器(4)
                End With
                群成员数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_分配位置(ByVal 群主 As 类_用户, ByVal 群主位置号 As Short, ByVal 备注 As String, ByRef 群编号 As Byte) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 群主.用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("群编号")
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "创建的小聊天群", 筛选器,  , 列添加器, 最大值_常量集合.每个用户可创建的小聊天群数量, "#用户群编号")
            Dim 位置号(最大值_常量集合.每个用户可创建的小聊天群数量 - 1) As Byte
            Dim 位置号数量 As Short
            读取器 = 指令2.执行()
            While 读取器.读取
                If 位置号数量 = 位置号.Length Then ReDim Preserve 位置号(位置号数量 * 2 - 1)
                位置号(位置号数量) = 读取器(0)
                位置号数量 += 1
            End While
            读取器.关闭()
            If 位置号数量 < 最大值_常量集合.每个用户可创建的小聊天群数量 Then
                群编号 = 1
                If 位置号数量 > 0 Then
                    Dim I As Integer
跳转点1:
                    For I = 0 To 位置号数量 - 1
                        If 位置号(I) = 群编号 Then
                            群编号 += 1
                            GoTo 跳转点1
                        End If
                    Next
                End If
                Dim 当前时刻 As Long = Date.UtcNow.Ticks
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 群主.用户编号)
                列添加器.添加列_用于插入数据("位置号", 群主位置号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("创建时间", 当前时刻)
                Dim 指令 As New 类_数据库指令_插入新数据(主数据库, "创建的小聊天群", 列添加器, True)
                指令.执行()
                Dim 群主地址 As String = 群主.英语用户名 & 讯宝地址标识 & 域名_英语
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("群主", 群主.用户编号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("英语讯宝地址", 群主地址)
                If String.IsNullOrEmpty(域名_本国语) = False Then
                    列添加器.添加列_用于插入数据("本国语讯宝地址", 群主.本国语用户名 & 讯宝地址标识 & 域名_本国语)
                End If
                列添加器.添加列_用于插入数据("主机名", 本服务器主机名)
                列添加器.添加列_用于插入数据("位置号", 群主位置号)
                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.群主)
                列添加器.添加列_用于插入数据("加入时间", 当前时刻)
                指令 = New 类_数据库指令_插入新数据(主数据库, "小聊天群成员", 列添加器, True)
                指令.执行()
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 群主.用户编号)
                列添加器.添加列_用于插入数据("位置号", 群主位置号)
                列添加器.添加列_用于插入数据("群主地址", 群主地址)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("群备注", 备注)
                列添加器.添加列_用于插入数据("加入时间", 当前时刻)
                指令 = New 类_数据库指令_插入新数据(主数据库, "加入的小聊天群", 列添加器)
                指令.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加邀请(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 讯友 As 类_讯友) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 讯友.英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("角色")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "小聊天群成员", 筛选器, 1, 列添加器, , "#群主编号英语讯宝地址")
            Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
            读取器 = 指令.执行()
            While 读取器.读取
                角色 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If 角色 >= 群角色_常量集合.成员_可以发言 Then Return New 类_SS包生成器(查询结果_常量集合.失败)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_请求获取数据(主数据库, "小聊天群成员", 筛选器,  , , 最大值_常量集合.小聊天群成员数量, "#群主编号角色加入时间")
            Dim 群成员数量 As Short
            读取器 = 指令.执行()
            While 读取器.读取
                群成员数量 += 1
            End While
            读取器.关闭()
            If 群成员数量 >= 最大值_常量集合.小聊天群成员数量 Then Return New 类_SS包生成器(查询结果_常量集合.出错)
            If 角色 = 群角色_常量集合.无 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("群主", 用户编号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                列添加器.添加列_用于插入数据("英语讯宝地址", 讯友.英语讯宝地址)
                If String.IsNullOrEmpty(讯友.本国语讯宝地址) = False Then
                    列添加器.添加列_用于插入数据("本国语讯宝地址", 讯友.本国语讯宝地址)
                End If
                列添加器.添加列_用于插入数据("主机名", 讯友.主机名)
                列添加器.添加列_用于插入数据("位置号", 讯友.位置号)
                列添加器.添加列_用于插入数据("角色", 群角色_常量集合.邀请加入_可以发言)
                列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
                Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "小聊天群成员", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入者信息(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 加入者 As String,
                              ByRef 本国语讯宝地址 As String, ByRef 主机名 As String, ByRef 位置号 As Short, ByRef 角色 As 群角色_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 加入者)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"本国语讯宝地址", "主机名", "位置号", "角色"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "小聊天群成员", 筛选器, 1, 列添加器, , "#群主编号英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                本国语讯宝地址 = 读取器(0)
                主机名 = 读取器(1)
                位置号 = 读取器(2)
                角色 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_是否加入了群(ByVal 用户编号 As Long, ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte, ByRef 加入了 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的小聊天群", 筛选器, 1, , , "#群主编号用户")
            读取器 = 指令.执行()
            While 读取器.读取
                加入了 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取加入的群数量(ByVal 用户编号 As Long, ByRef 加入的群数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "加入的小聊天群", 筛选器,  , , 最大值_常量集合.每个用户可加入的小聊天群数量, "#用户加入时间")
            读取器 = 指令.执行()
            While 读取器.读取
                加入的群数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_加入群(ByVal 用户编号 As Long, ByVal 位置号 As Short, ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte, ByVal 群备注 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("位置号", 位置号)
            列添加器.添加列_用于插入数据("群主地址", 群主英语讯宝地址)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("群备注", 群备注)
            列添加器.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "加入的小聊天群", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_成为群成员(ByVal 群主 As Long, ByVal 群编号 As Byte, ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("角色", 群角色_常量集合.成员_可以发言)
            列添加器_新数据.添加列_用于插入数据("加入时间", Date.UtcNow.Ticks)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 群主)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("角色", 筛选方式_常量集合.等于, 群角色_常量集合.邀请加入_可以发言)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "小聊天群成员", 列添加器_新数据, 筛选器, "#群主编号英语讯宝地址")
            If 指令2.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群成员(ByVal 群主 As Long, ByVal 群编号 As Byte, ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 群主)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令3 As New 类_数据库指令_删除数据(主数据库, "小聊天群成员", 筛选器, "#群主编号英语讯宝地址")
            指令3.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除群(ByVal 用户编号 As Long, ByVal 群编号 As Byte, ByVal 群主地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令3 As New 类_数据库指令_删除数据(主数据库, "创建的小聊天群", 筛选器, "#用户群编号")
            指令3.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群主", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令3 = New 类_数据库指令_删除数据(主数据库, "小聊天群成员", 筛选器, "#群主编号英语讯宝地址")
            指令3.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "加入的小聊天群", 筛选器, "#群主编号用户")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_修改群备注(ByVal 用户编号 As Long, ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte, ByVal 群备注 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("群备注", 群备注)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("群主地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("群备注", 筛选方式_常量集合.不等于, 群备注,  , False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "加入的小聊天群", 列添加器_新数据, 筛选器, "#群主编号用户")
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
