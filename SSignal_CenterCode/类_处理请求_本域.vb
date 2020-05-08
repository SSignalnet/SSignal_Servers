Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 服务器启动() As Object
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        Dim Credential As String = Http请求("Credential")
        If Credential.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        Dim Type As 服务器类别_常量集合
        If Byte.TryParse(Http请求("Type"), Type) = False Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器
        Dim 主机名 As String = Nothing
        Dim 网络地址字节数组() As Byte
        If 跨进程锁.WaitOne = True Then
            Try
                网络地址字节数组 = 获取网络地址字节数组()
                If 网络地址字节数组 Is Nothing Then
                    Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
                End If
                结果 = 数据库_获取服务器主机名(Type, 网络地址字节数组, 主机名)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Dim 子域名 As String = 获取服务器域名(主机名 & "." & 域名_英语)
        Dim 访问结果 As Object = 访问其它服务器("https://" & 子域名 & "/?C=ServerStart&Credential=" & 替换URI敏感字符(Credential))
        If TypeOf 访问结果 Is 类_SS包生成器 Then
            Return 访问结果
        Else
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then
                Return 访问结果
            End If
        End If
        Dim 用户() As 用户_复合数据 = Nothing
        Dim 用户数, I As Short
        Dim 小宇宙写入服务器主机名 As String = Nothing
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_保存服务器的凭据(子域名, 网络地址字节数组, Credential)
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                    Return 结果
                End If
                Select Case Type
                    Case 服务器类别_常量集合.传送服务器
                        ReDim 用户(最大值_常量集合.传送服务器承载用户数 - 1)
                        结果 = 数据库_传送服务器获取用户(主机名, 用户, 用户数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                        结果 = 数据库_分配小宇宙写入服务器给传送服务器(主机名, 小宇宙写入服务器主机名)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                End Select
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        结果.添加_有标签("主机名", 主机名)
        Select Case Type
            Case 服务器类别_常量集合.传送服务器
                结果.添加_有标签("小宇宙写入服务器", 小宇宙写入服务器主机名)
                If 用户数 > 0 Then
                    Dim 某个用户 As 用户_复合数据
                    For I = 0 To 用户数 - 1
                        某个用户 = 用户(I)
                        Dim SS包生成器2 As New 类_SS包生成器()
                        With SS包生成器2
                            .添加_有标签("编号", 某个用户.编号)
                            .添加_有标签("英语", 某个用户.英语用户名)
                            If String.IsNullOrEmpty(某个用户.本国语用户名) = False Then
                                .添加_有标签("本国语", 某个用户.本国语用户名)
                            End If
                            .添加_有标签("位置号", 某个用户.位置号)
                            .添加_有标签("停用", 某个用户.停用)
                        End With
                        结果.添加_有标签("用户", SS包生成器2)
                    Next
                End If
        End Select
        Return 结果
    End Function

    Private Function 数据库_获取服务器主机名(ByVal 类别 As 服务器类别_常量集合, ByVal 网络地址() As Byte, ByRef 主机名 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("类别", 筛选方式_常量集合.等于, 类别)
            列添加器.添加列_用于筛选器("网络地址", 筛选方式_常量集合.等于, 网络地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"主机名", "停用"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "服务器", 筛选器, 1, 列添加器, , "#类别网络地址")
            Dim 停用 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                主机名 = 读取器(0)
                停用 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(主机名) = False Then
                If 停用 = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.成功)
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.已停用)
                End If
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存服务器的凭据(ByVal 子域名 As String, ByVal 网络地址() As Byte, ByVal 凭据 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址)
            列添加器_新数据.添加列_用于插入数据("连接凭据_我访它", 凭据)
            列添加器_新数据.添加列_用于插入数据("连接凭据_它访我", 凭据)
            列添加器_新数据.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(副数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("子域名", 子域名)
                列添加器.添加列_用于插入数据("网络地址", 网络地址)
                列添加器.添加列_用于插入数据("连接凭据_我访它", 凭据)
                列添加器.添加列_用于插入数据("连接凭据_它访我", 凭据)
                列添加器.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "服务器", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_传送服务器获取用户(ByVal 主机名 As String, ByRef 用户() As 用户_复合数据, ByRef 用户数 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If String.IsNullOrEmpty(域名_本国语) = False Then
                列添加器.添加列_用于获取数据(New String() {"编号", "停用", "英语用户名", "位置号", "本国语用户名"})
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 最大值_常量集合.传送服务器承载用户数, 列添加器, 100, "#主机名位置号")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 用户(用户数)
                        .编号 = 读取器(0)
                        .停用 = 读取器(1)
                        .英语用户名 = 读取器(2)
                        .位置号 = 读取器(3)
                        .本国语用户名 = 读取器(4)
                    End With
                    用户数 += 1
                End While
            Else
                列添加器.添加列_用于获取数据(New String() {"编号", "停用", "英语用户名", "位置号"})
                Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "用户", 筛选器, 最大值_常量集合.传送服务器承载用户数, 列添加器, 100, "#主机名位置号")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 用户(用户数)
                        .编号 = 读取器(0)
                        .停用 = 读取器(1)
                        .英语用户名 = 读取器(2)
                        .位置号 = 读取器(3)
                    End With
                    用户数 += 1
                End While
            End If
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_分配小宇宙写入服务器给传送服务器(ByVal 传送服务器主机名 As String, ByRef 小宇宙写入服务器主机名 As String) As 类_SS包生成器


        小宇宙写入服务器主机名 = 讯宝小宇宙中心服务器主机名
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

End Class
