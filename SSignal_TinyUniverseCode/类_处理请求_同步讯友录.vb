Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_处理请求

    Public Function 同步讯友录() As 类_SS包生成器
        If 主数据库 Is Nothing OrElse 副数据库 Is Nothing Then Return New 类_SS包生成器(查询结果_常量集合.数据库未就绪)
        If String.IsNullOrEmpty(启动器.本服务器主机名) = True Then
            If DateDiff(DateInterval.Minute, Date.FromBinary(启动器.启动时间), Date.UtcNow) >= 2 Then 启动器.启动()
            Return New 类_SS包生成器(查询结果_常量集合.服务器未就绪)
        End If
        Dim 传送服务器凭据 As String = Http请求("Credential")
        Dim 传送服务器主机名 As String = Http请求("HostName")
        If Http请求.ContentLength = 0 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        If String.IsNullOrEmpty(传送服务器主机名) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器主机名.Length > 最大值_常量集合.主机名字符数 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If String.IsNullOrEmpty(传送服务器凭据) Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)
        If 传送服务器凭据.Length <> 长度_常量集合.连接凭据_服务器 Then Return New 类_SS包生成器(查询结果_常量集合.HTTP数据错误)

        Dim 结果 As 类_SS包生成器 = Nothing
        Dim 更新时间 As Long
        Dim 传送服务器子域名2 As String = 获取服务器域名(传送服务器主机名 & "." & 域名_英语)
        If 跨进程锁.WaitOne = True Then
            Try
                结果 = 数据库_验证其它服务器访问我方的凭据(Context, 副数据库, 传送服务器子域名2, 传送服务器凭据, 更新时间)
            Catch ex As Exception
                Return New 类_SS包生成器(ex.Message)
            Finally
                跨进程锁.ReleaseMutex()
            End Try
        Else
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        End If
        Select Case 结果.查询结果
            Case 查询结果_常量集合.凭据有效
            Case 查询结果_常量集合.需要添加连接凭据
跳转点1:
                Dim 访问结果 As Object = 访问其它服务器(获取路径_验证服务器真实性(传送服务器主机名 & "." & 域名_英语, 传送服务器凭据, 启动器.本服务器主机名 & "." & 域名_英语))
                If TypeOf 访问结果 Is 类_SS包生成器 Then
                    Return 访问结果
                Else
                    Dim SS包解读器3 As New 类_SS包解读器(CType(访问结果, Byte()))
                    If SS包解读器3.查询结果 <> 查询结果_常量集合.成功 Then Return 访问结果
                End If
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_添加其它服务器访问我方的凭据(Context, 副数据库, 传送服务器子域名2, Nothing, 传送服务器凭据)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                    Catch ex As Exception
                        Return New 类_SS包生成器(ex.Message)
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            Case 查询结果_常量集合.不正确, 查询结果_常量集合.未知IP地址
                If 更新时间 > 0 Then
                    If DateDiff(DateInterval.Minute, Date.FromBinary(更新时间), Date.UtcNow) < 5 Then
                        Return 结果
                    End If
                End If
                GoTo 跳转点1
            Case Else : Return 结果
        End Select
        Dim 字节数组(Http请求.ContentLength - 1) As Byte
        Http请求.InputStream.Read(字节数组, 0, 字节数组.Length)
        Dim SS包解读器 As New 类_SS包解读器(字节数组)
        Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("变动")
        If SS包解读器2 IsNot Nothing Then
            If 跨进程锁.WaitOne = True Then
                Try
                    Dim 英语用户名 As String = Nothing
                    Dim 英语用户名2 As String = Nothing
                    Dim 传送服务器主机名_数据库中 As String = Nothing
                    Dim 传送服务器主机名2 As String = Nothing
                    Dim 英语讯宝地址 As String = Nothing
                    Dim 本国语讯宝地址 As String = Nothing
                    Dim 原标签名 As String = Nothing
                    Dim 新标签名 As String = Nothing
                    Dim 拉黑 As Boolean
                    Dim 变动 As 讯友录变动_常量集合
                    Dim I As Integer
                    For I = 0 To SS包解读器2.Length - 1
                        With (CType(SS包解读器2(I), 类_SS包解读器))
                            .读取_有标签("用户名", 英语用户名, Nothing)
                            If String.IsNullOrEmpty(英语用户名) Then Continue For
                            If String.Compare(英语用户名, 英语用户名2) <> 0 Then
                                结果 = 数据库_获取传送服务器(英语用户名, 传送服务器主机名_数据库中)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                                If String.IsNullOrEmpty(传送服务器主机名_数据库中) = False Then
                                    If String.Compare(传送服务器主机名_数据库中, 传送服务器主机名) <> 0 Then
                                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                                    End If
                                End If
                                英语用户名2 = 英语用户名
                            End If
                            .读取_有标签("类型", 变动, 讯友录变动_常量集合.无)
                            Select Case 变动
                                Case 讯友录变动_常量集合.添加, 讯友录变动_常量集合.修改拉黑
                                    .读取_有标签("英语", 英语讯宝地址, Nothing)
                                    If String.IsNullOrEmpty(英语讯宝地址) Then Continue For
                                    .读取_有标签("本国语", 本国语讯宝地址, Nothing)
                                    .读取_有标签("标签一", 原标签名, Nothing)
                                    .读取_有标签("标签二", 新标签名, Nothing)
                                    .读取_有标签("拉黑", 拉黑, False)
                                    If String.IsNullOrEmpty(传送服务器主机名_数据库中) Then
                                        传送服务器主机名2 = 传送服务器主机名
                                    Else
                                        传送服务器主机名2 = Nothing
                                    End If
                                    结果 = 数据库_添加或修改讯友(英语用户名, 英语讯宝地址, 本国语讯宝地址, 原标签名, 新标签名, 拉黑, 传送服务器主机名2)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                                Case 讯友录变动_常量集合.删除
                                    .读取_有标签("英语", 英语讯宝地址, Nothing)
                                    If String.IsNullOrEmpty(英语讯宝地址) Then Continue For
                                    结果 = 数据库_删除讯友(英语用户名, 英语讯宝地址)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                                Case 讯友录变动_常量集合.重命名标签
                                    .读取_有标签("原标签名", 原标签名, Nothing)
                                    If String.IsNullOrEmpty(原标签名) Then Continue For
                                    .读取_有标签("新标签名", 新标签名, Nothing)
                                    If String.IsNullOrEmpty(新标签名) Then Continue For
                                    结果 = 数据库_讯友标签重命名(英语用户名, 原标签名, 新标签名)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Return 结果
                            End Select
                        End With
                    Next
                Catch ex As Exception
                    Return New 类_SS包生成器(ex.Message)
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        End If
        Return New 类_SS包生成器(查询结果_常量集合.成功)
    End Function

    Private Function 数据库_获取传送服务器(ByVal 英语用户名 As String, ByRef 传送服务器主机名 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("主机名")
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "传送服务器", 筛选器, 1, 列添加器,  , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                传送服务器主机名 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加或修改讯友(ByVal 英语用户名 As String, ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String,
                                 ByVal 标签一 As String, ByVal 标签二 As String, ByVal 拉黑 As Boolean, ByVal 传送服务器主机名 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签一", 标签一)
            列添加器_新数据.添加列_用于插入数据("标签二", 标签二)
            列添加器_新数据.添加列_用于插入数据("拉黑", 拉黑)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户名讯友")
            If 指令2.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("英语用户名", 英语用户名)
                列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                    列添加器.添加列_用于插入数据("本国语讯宝地址", 本国语讯宝地址)
                End If
                If String.IsNullOrEmpty(标签一) = False Then
                    列添加器.添加列_用于插入数据("标签一", 标签一)
                End If
                If String.IsNullOrEmpty(标签二) = False Then
                    列添加器.添加列_用于插入数据("标签二", 标签二)
                End If
                列添加器.添加列_用于插入数据("拉黑", 拉黑)
                Dim 指令3 As New 类_数据库指令_插入新数据(主数据库, "讯友录", 列添加器)
                指令3.执行()
                If String.IsNullOrEmpty(传送服务器主机名) = False Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("英语用户名", 英语用户名)
                    列添加器.添加列_用于插入数据("主机名", 传送服务器主机名)
                    指令3 = New 类_数据库指令_插入新数据(副数据库, "传送服务器", 列添加器)
                    指令3.执行()
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除讯友(ByVal 英语用户名 As String, ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_删除 As New 类_数据库指令_删除数据(主数据库, "讯友录", 筛选器, "#用户名讯友")
            指令_删除.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_讯友标签重命名(ByVal 英语用户名 As String, ByVal 旧标签名称 As String, ByVal 新标签名称 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签一", 新标签名称)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            列添加器.添加列_用于筛选器("标签一", 筛选方式_常量集合.等于, 旧标签名称)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签一")
            指令.执行()
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签二", 新标签名称)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            列添加器.添加列_用于筛选器("标签二", 筛选方式_常量集合.等于, 旧标签名称)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签二")
            指令.执行()
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签二", Nothing)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("英语用户名", 筛选方式_常量集合.等于, 英语用户名)
            列添加器.添加列_用于筛选器("标签二", 筛选方式_常量集合.等于, New 类_列_表成员("标签一"))
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签二")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
