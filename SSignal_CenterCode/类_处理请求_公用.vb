Imports System.IO
Imports System.Text.Encoding
Imports System.Drawing
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_ServerCommonCode
Imports SSignal_GlobalCommonCode

Partial Public Class 类_处理请求

    Private Function 数据库_验证用户凭据(ByVal 用户编号 As Long, ByVal 连接凭据 As String, Optional ByVal 用户信息 As 类_用户信息 = Nothing,
                                Optional ByVal 更新凭据 As Boolean = False, Optional ByRef 新连接凭据 As String = Nothing) As 类_SS包生成器
        If String.IsNullOrEmpty(连接凭据) = True Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        If 连接凭据.Length <> 长度_常量集合.连接凭据_客户端 Then Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 停用 As Boolean
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If 用户信息 Is Nothing Then
                列添加器.添加列_用于获取数据("停用")
            Else
                Select Case 用户信息.范围
                    Case 类_用户信息.范围_常量集合.主机名
                        列添加器.添加列_用于获取数据(New String() {"停用", "主机名", "位置号"})
                    Case 类_用户信息.范围_常量集合.用户名和主机名
                        If String.IsNullOrEmpty(域名_本国语) Then
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名", "主机名", "位置号"})
                        Else
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名", "主机名", "位置号", "本国语用户名"})
                        End If
                    Case 类_用户信息.范围_常量集合.手机号
                        列添加器.添加列_用于获取数据(New String() {"停用", "手机号"})
                    Case 类_用户信息.范围_常量集合.电子邮箱地址
                        列添加器.添加列_用于获取数据(New String() {"停用", "电子邮箱地址"})
                    Case 类_用户信息.范围_常量集合.职能
                        列添加器.添加列_用于获取数据(New String() {"停用", "职能"})
                    Case 类_用户信息.范围_常量集合.用户名
                        If String.IsNullOrEmpty(域名_本国语) Then
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名"})
                        Else
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名", "本国语用户名"})
                        End If
                    Case 类_用户信息.范围_常量集合.职能和主机名
                        列添加器.添加列_用于获取数据(New String() {"停用", "职能", "主机名"})
                    Case Else
                        If String.IsNullOrEmpty(域名_本国语) Then
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名", "主机名", "位置号", "手机号", "电子邮箱地址", "职能"})
                        Else
                            列添加器.添加列_用于获取数据(New String() {"停用", "英语用户名", "主机名", "位置号", "手机号", "电子邮箱地址", "职能", "本国语用户名"})
                        End If
                End Select
            End If
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                停用 = 读取器(0)
                If 停用 = False AndAlso 用户信息 IsNot Nothing Then
                    Select Case 用户信息.范围
                        Case 类_用户信息.范围_常量集合.主机名
                            用户信息.主机名 = 读取器(1)
                            用户信息.位置号 = 读取器(2)
                        Case 类_用户信息.范围_常量集合.用户名和主机名
                            用户信息.英语用户名 = 读取器(1)
                            用户信息.主机名 = 读取器(2)
                            用户信息.位置号 = 读取器(3)
                            If String.IsNullOrEmpty(域名_本国语) = False Then
                                用户信息.本国语用户名 = 读取器(4)
                            End If
                        Case 类_用户信息.范围_常量集合.手机号
                            用户信息.手机号 = 读取器(1)
                        Case 类_用户信息.范围_常量集合.电子邮箱地址
                            用户信息.电子邮箱地址 = 读取器(1)
                            If String.IsNullOrEmpty(用户信息.电子邮箱地址) Then
                                用户信息.电子邮箱地址 = ""
                            End If
                        Case 类_用户信息.范围_常量集合.职能
                            用户信息.职能 = 读取器(1)
                        Case 类_用户信息.范围_常量集合.用户名
                            用户信息.英语用户名 = 读取器(1)
                            If String.IsNullOrEmpty(域名_本国语) = False Then
                                用户信息.本国语用户名 = 读取器(2)
                            End If
                        Case 类_用户信息.范围_常量集合.职能和主机名
                            用户信息.职能 = 读取器(1)
                            用户信息.主机名 = 读取器(2)
                        Case Else
                            用户信息.英语用户名 = 读取器(1)
                            用户信息.主机名 = 读取器(2)
                            用户信息.位置号 = 读取器(3)
                            用户信息.手机号 = 读取器(4)
                            用户信息.电子邮箱地址 = 读取器(5)
                            If String.IsNullOrEmpty(用户信息.电子邮箱地址) Then
                                用户信息.电子邮箱地址 = ""
                            End If
                            用户信息.职能 = 读取器(6)
                            If String.IsNullOrEmpty(域名_本国语) = False Then
                                用户信息.本国语用户名 = 读取器(7)
                            End If
                    End Select
                End If
                Exit While
            End While
            读取器.关闭()
            If 停用 = True Then
                Return New 类_SS包生成器(查询结果_常量集合.账号停用)
            End If
            Dim 连接凭据_数据库中的 As String = ""
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If 连接凭据.StartsWith(设备类型_手机) Then
                列添加器.添加列_用于获取数据(New String() {"登录时间_手机", "连接凭据_手机"})
            Else
                列添加器.添加列_用于获取数据(New String() {"登录时间_电脑", "连接凭据_电脑"})
            End If
            指令 = New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            Dim 登录时间 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                登录时间 = 读取器(0)
                连接凭据_数据库中的 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If String.Compare(连接凭据, 连接凭据_数据库中的) <> 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
            End If
            Dim 登录时间2 As Date = Date.FromBinary(登录时间)
            If DateDiff(DateInterval.Day, 登录时间2, Date.UtcNow) > 30 Then
                Return New 类_SS包生成器(查询结果_常量集合.凭据无效)
            End If
            If 更新凭据 = True AndAlso DateDiff(DateInterval.Hour, 登录时间2, Date.UtcNow) > 24 Then
                Dim 列添加器_新数据 As New 类_列添加器
                If 连接凭据.StartsWith(设备类型_手机) Then
                    连接凭据 = 设备类型_手机 & 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端 - 设备类型_手机.Length)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_手机", 连接凭据)
                    列添加器_新数据.添加列_用于插入数据("登录时间_手机", Date.UtcNow.Ticks)
                    列添加器_新数据.添加列_用于插入数据("网络地址_手机", 获取网络地址字节数组)
                Else
                    连接凭据 = 设备类型_电脑 & 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_客户端 - 设备类型_电脑.Length)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_电脑", 连接凭据)
                    列添加器_新数据.添加列_用于插入数据("登录时间_电脑", Date.UtcNow.Ticks)
                    列添加器_新数据.添加列_用于插入数据("网络地址_电脑", 获取网络地址字节数组)
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
                If 指令2.执行 > 0 Then
                    新连接凭据 = 连接凭据
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.凭据有效)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_验证密码(ByVal 用户编号 As Long, ByVal 密码哈希值() As Byte, Optional ByVal 设备类型 As String = Nothing) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 密码哈希值2() As Byte = Nothing
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("密码哈希值")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                密码哈希值2 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If 密码哈希值2 IsNot Nothing Then
                If 密码哈希值.Length = 密码哈希值2.Length Then
                    Dim 登录错误次数 As Byte
                    If String.IsNullOrEmpty(设备类型) = False Then
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        If String.Compare(设备类型, 设备类型_手机) = 0 Then
                            列添加器.添加列_用于获取数据("登录错误次数_手机")
                        Else
                            列添加器.添加列_用于获取数据("登录错误次数_电脑")
                        End If
                        指令 = New 类_数据库指令_请求获取数据(副数据库, "用户", 筛选器, 1, 列添加器, , 主键索引名)
                        读取器 = 指令.执行()
                        While 读取器.读取
                            登录错误次数 = 读取器(0)
                            Exit While
                        End While
                        读取器.关闭()
                    End If
                    Dim I As Integer
                    For I = 0 To 密码哈希值2.Length - 1
                        If 密码哈希值2(I) <> 密码哈希值(I) Then Exit For
                    Next
                    If I < 密码哈希值2.Length Then
                        登录错误次数 += 1
                        If 登录错误次数 < 20 AndAlso String.IsNullOrEmpty(设备类型) = False Then
                            Call 数据库_更新登录错误次数(用户编号, 登录错误次数, 设备类型)
                        End If
                        Return New 类_SS包生成器(查询结果_常量集合.不正确)
                    Else
                        If 登录错误次数 > 0 AndAlso String.IsNullOrEmpty(设备类型) = False Then
                            Call 数据库_更新登录错误次数(用户编号, 0, 设备类型)
                        End If
                        Return New 类_SS包生成器(查询结果_常量集合.凭据有效)
                    End If
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.不正确)
                End If
            Else
                Return New 类_SS包生成器(查询结果_常量集合.不正确)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 数据库_更新登录错误次数(ByVal 用户编号 As Long, ByVal 登录错误次数 As Byte, ByVal 设备类型 As String)
        Dim 列添加器_新数据 As New 类_列添加器
        If String.Compare(设备类型, 设备类型_手机) = 0 Then
            列添加器_新数据.添加列_用于插入数据("登录错误次数_手机", 登录错误次数)
        Else
            列添加器_新数据.添加列_用于插入数据("登录错误次数_电脑", 登录错误次数)
        End If
        Dim 列添加器 As New 类_列添加器
        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 用户编号)
        Dim 筛选器 As New 类_筛选器
        筛选器.添加一组筛选条件(列添加器)
        Dim 指令 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器, 主键索引名)
        If 指令.执行() = 0 Then
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("编号", 用户编号)
            If String.Compare(设备类型, 设备类型_手机) = 0 Then
                列添加器.添加列_用于插入数据("登录时间_手机", 0)
                列添加器.添加列_用于插入数据("登录错误次数_手机", 登录错误次数)
            Else
                列添加器.添加列_用于插入数据("登录时间_电脑", 0)
                列添加器.添加列_用于插入数据("登录错误次数_电脑", 登录错误次数)
            End If
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
            指令2.执行()
        End If
    End Sub

    Private Function 数据库_获取最近操作次数(ByVal 用户编号 As Long, ByRef 操作次数 As Integer, ByVal 操作代码 As 操作代码_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("操作时间", 筛选方式_常量集合.大于, Date.UtcNow.AddMinutes(-最近操作次数统计时间_分钟).Ticks)
            If 操作代码 > 操作代码_常量集合.无 Then
                列添加器.添加列_用于筛选器("操作代码", 筛选方式_常量集合.等于, 操作代码)
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "操作记录", 筛选器, , , 10, "#用户编号操作时间")
            读取器 = 指令.执行()
            While 读取器.读取
                操作次数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存操作记录(ByVal 用户编号 As Long, ByVal 操作代码 As 操作代码_常量集合,
                                        ByVal 网络地址() As Byte) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("操作时间", 筛选方式_常量集合.小于, Date.UtcNow.AddHours(-24).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "操作记录", 筛选器, "#操作时间")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("操作代码", 操作代码)
            列添加器.添加列_用于插入数据("网络地址", 网络地址)
            列添加器.添加列_用于插入数据("操作时间", Date.UtcNow.Ticks)
            Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "操作记录", 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 获取图片(ByVal 字节数组() As Byte, Optional ByVal 起始位置 As Integer = 0) As Image
        If 字节数组 Is Nothing Then Return Nothing
        Dim 图片 As Image
        Dim 文件流 As MemoryStream = Nothing
        Try
            If 起始位置 > 0 Then
                文件流 = New MemoryStream(字节数组, 起始位置, 字节数组.Length - 起始位置)
            Else
                文件流 = New MemoryStream(字节数组)
            End If
            图片 = Image.FromStream(文件流)
            文件流.Close()
            Return 图片
        Catch ex As Exception
            If 文件流 IsNot Nothing Then 文件流.Close()
            Return Nothing
        End Try
    End Function

    Private Function 生成大写英文字母与数字的随机字符串(ByVal 长度 As Short) As String
        If 测试 = False Then
            If 长度 < 1 Then 长度 = 1
            Dim 字节数组(长度 - 1) As Byte
            Dim I As Short
            Randomize()
            For I = 0 To 长度 - 1
                If Int(36 * Rnd()) < 10 Then
                    字节数组(I) = Int(10 * Rnd()) + 48
                Else
                    字节数组(I) = Int(26 * Rnd()) + 65
                End If
            Next
            Return ASCII.GetString(字节数组)
        Else
            Dim 字节 As Byte = CByte(AscW("s"))
            Dim 字节数组2(长度 - 1) As Byte
            For I = 0 To 字节数组2.Length - 1
                字节数组2(I) = 字节
            Next
            Return ASCII.GetString(字节数组2)
        End If
    End Function

    Public Function 获取界面文字(ByVal 网页语种 As String) As 类_界面文字
        If String.Compare(网页语种, 语言代码_英语) = 0 Then
            Return New 类_界面文字(网页语种)
        Else
            Return New 类_界面文字(网页语种, My.Resources.UItext)
        End If
    End Function

End Class
