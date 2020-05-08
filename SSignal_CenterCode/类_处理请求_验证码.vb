Imports System.Drawing
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Partial Public Class 类_处理请求

    Public Function 获取验证码图片() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If 跨进程锁.WaitOne = True Then
            Try
                Return 获取验证码图片2()
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
    End Function

    Private Function 获取验证码图片2() As 类_SS包生成器
        Dim 数量 As Integer
        Dim 结果 As 类_SS包生成器 = 数据库_统计验证码(数量)
        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
            Return 结果
        End If
        If 数量 > 20 Then
            Return New 类_SS包生成器(查询结果_常量集合.获取验证码次数过多)
        End If
        Dim 验证码添加时间 As Long
        Dim 验证码 As String = Nothing
        结果 = 数据库_添加验证码(验证码添加时间, 验证码)
        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
            Return 结果
        End If
        Dim 图片字节数组() As Byte = 生成复杂验证码图片(验证码, False)
        结果 = New 类_SS包生成器(查询结果_常量集合.验证码)
        结果.添加_有标签("验证码添加时间", 验证码添加时间)
        结果.添加_有标签("图片", 图片字节数组)
        Return 结果
    End Function

    Private Function 生成复杂验证码图片(ByVal 验证码 As String, ByVal 黑色 As Boolean) As Byte()
        Dim 图片 As New Bitmap(200, 50)
        Dim 绘图器 As Graphics = Graphics.FromImage(图片)
        If 黑色 = False Then
            绘图器.Clear(Color.White)
        Else
            绘图器.Clear(Color.Black)
        End If
        Dim 字符() As Char = 验证码.ToCharArray
        Dim 宽度 As Single = 图片.Width / 长度_常量集合.验证码 - 5
        Dim 高度的一半 As Single = 图片.Height / 2
        Dim 高度变化范围 As Single = 图片.Height / 2
        Randomize()
        Dim 位置, 点1, 点2 As Point
        Dim 角度 As Single
        Dim 字体 As New Font("Microsoft Sans Serif", 20)
        Dim I As Integer
        For I = 0 To 字符.Length - 1
            位置 = New Point(I * 宽度 + 5 * Rnd(), CInt(高度变化范围 * Rnd()))
            If Rnd() < 0.5 Then
                角度 = 30 * Rnd()
            Else
                角度 = -30 * Rnd()
            End If
            绘图器.TranslateTransform(位置.X, 位置.Y)
            绘图器.RotateTransform(角度)
            If 黑色 = False Then
                绘图器.DrawString(字符(I), 字体, Brushes.Black, 0, 0)
            Else
                绘图器.DrawString(字符(I), 字体, Brushes.Lime, 0, 0)
            End If
            绘图器.RotateTransform(-角度)
            绘图器.TranslateTransform(-位置.X, -位置.Y)
            If Rnd() < 0.5 Then
                点1 = New Point(I * 宽度 + 5 * Rnd(), 高度的一半 + CInt(高度的一半 * Rnd()))
            Else
                点1 = New Point(I * 宽度 + 5 * Rnd(), 高度的一半 - CInt(高度的一半 * Rnd()))
            End If
            If 点1.Y < 高度的一半 Then
                点2 = New Point((I + 1) * 宽度 - 5 * Rnd(), 高度的一半 + CInt(高度的一半 * Rnd()))
            Else
                点2 = New Point((I + 1) * 宽度 - 5 * Rnd(), 高度的一半 - CInt(高度的一半 * Rnd()))
            End If
            If 黑色 = False Then
                绘图器.DrawLine(New Pen(Color.Black, 3), 点1, 点2)
            Else
                绘图器.DrawLine(New Pen(Color.Lime, 3), 点1, 点2)
            End If
        Next
        'If Rnd() < 0.5 Then
        '    点1 = New Point(5 * Rnd(), 高度的一半 + CInt(高度的一半 * Rnd()))
        'Else
        '    点1 = New Point(5 * Rnd(), 高度的一半 - CInt(高度的一半 * Rnd()))
        'End If
        'If 点1.Y < 高度的一半 Then
        '    点2 = New Point(图片.Width - 5 * Rnd(), 高度的一半 + CInt(高度的一半 * Rnd()))
        'Else
        '    点2 = New Point(图片.Width - 5 * Rnd(), 高度的一半 - CInt(高度的一半 * Rnd()))
        'End If
        '绘图器.DrawLine(笔, 点1, 点2)
        绘图器.Dispose()
        Dim 内存流 As New MemoryStream
        图片.Save(内存流, Imaging.ImageFormat.Jpeg)
        图片.Dispose()
        Dim 字节数组() As Byte = 内存流.ToArray
        内存流.Close()
        Return 字节数组
    End Function

    Private Function 数据库_统计验证码(ByRef 数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 网络地址() As Byte = 获取网络地址字节数组()
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("网络地址", 筛选方式_常量集合.等于, 网络地址)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.大于, Date.UtcNow.AddMinutes(-验证码的有效时间_分钟).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "验证码", 筛选器, , , 20, "#网络地址时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                数量 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加验证码(ByRef 时间 As Long, ByRef 验证码 As String,
                               Optional ByVal 手机号或电子邮箱地址 As String = Nothing, Optional ByVal 纯数字 As Boolean = False) As 类_SS包生成器
跳转点1:
        Try
            Call 数据库_清除验证码()
            Dim 列添加器 As New 类_列添加器
            时间 = Date.UtcNow.Ticks
            列添加器.添加列_用于插入数据("时间", 时间)
            If 纯数字 = False Then
                验证码 = 生成大写英文字母与数字的随机字符串(长度_常量集合.验证码)
            Else
                验证码 = 生成数字的随机字符串(长度_常量集合.验证码)
            End If
            列添加器.添加列_用于插入数据("随机字符串", 验证码)
            If String.IsNullOrEmpty(手机号或电子邮箱地址) = False Then
                列添加器.添加列_用于插入数据("手机号或电子邮箱地址", 手机号或电子邮箱地址)
            End If
            列添加器.添加列_用于插入数据("网络地址", 获取网络地址字节数组)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "验证码", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            GoTo 跳转点1
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 数据库_清除验证码()
        Dim 列添加器 As New 类_列添加器
        列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.小于, Date.UtcNow.AddMinutes(-验证码的有效时间_分钟).Ticks)
        Dim 筛选器 As New 类_筛选器
        筛选器.添加一组筛选条件(列添加器)
        Dim 指令 As New 类_数据库指令_删除数据(副数据库, "验证码", 筛选器, "#时间随机字符串")
        指令.执行()
    End Sub

    Private Function 数据库_检验验证码(ByVal 时间 As Long, ByVal 验证码 As String, Optional ByVal 手机号或电子邮箱地址 As String = Nothing,
                                                            Optional ByVal 是邮件或短信验证码 As Boolean = False) As 类_SS包生成器
        Try
            Call 数据库_清除验证码()
            Dim 新验证码 As String = 生成大写英文字母与数字的随机字符串(长度_常量集合.验证码)
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("随机字符串", 新验证码)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 时间)
            列添加器.添加列_用于筛选器("随机字符串", 筛选方式_常量集合.等于, 验证码)
            If String.IsNullOrEmpty(手机号或电子邮箱地址) = False Then
                列添加器.添加列_用于筛选器("手机号或电子邮箱地址", 筛选方式_常量集合.等于, 手机号或电子邮箱地址)
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "验证码", 列添加器_新数据, 筛选器, "#时间随机字符串")
            If 指令.执行() > 0 Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                If 是邮件或短信验证码 = False Then
                    Return 获取验证码图片2()
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.验证码不匹配)
                End If
            End If
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_最近是否添加了验证码(ByVal 手机号或电子邮箱地址 As String, ByRef 添加了 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("手机号或电子邮箱地址", 筛选方式_常量集合.等于, 手机号或电子邮箱地址)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.大于, Date.UtcNow.AddMinutes(-验证码的时间间隔_分钟).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "验证码", 筛选器, 1, , , "#手机邮箱时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                添加了 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
