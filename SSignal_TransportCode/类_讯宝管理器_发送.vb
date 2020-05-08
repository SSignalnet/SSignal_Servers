Imports System.IO
Imports System.Threading
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Partial Public Class 类_讯宝管理器

    Private Sub 发送讯宝(ByVal SS包解读器 As 类_SS包解读器, ByVal 发送者 As 类_用户, ByVal 位置号 As Short, ByVal 设备类型 As 设备类型_常量集合)
        Dim SS包生成器 As 类_SS包生成器
        Dim 结果 As 类_SS包生成器 = Nothing
        Dim 本次发送序号 As Long
        Dim 讯宝指令, 讯宝无法发送的原因 As 讯宝指令_常量集合
        Dim 接收者英语讯宝地址 As String = Nothing
        Dim 讯宝文本 As String = Nothing
        Dim 发送者的讯友 As 类_讯友 = Nothing
        Dim 接收者的讯友 As 类_讯友 = Nothing
        Dim 讯宝文件数据() As Byte = Nothing
        Dim 宽度, 高度 As Short
        Dim 秒数, 群编号 As Byte
        Dim 接收者在本服务器上, 发送给自己创建的群 As Boolean
        Dim 接收者 As 类_用户 = Nothing

        'Dim 流写入器 As StreamWriter = Nothing
        'SS包解读器.读取_有标签("指令", 讯宝指令, 讯宝指令_常量集合.无)
        'If 讯宝指令 = 讯宝指令_常量集合.发送文字 Then
        '    流写入器 = File.AppendText(数据存放路径 & Date.Now.Ticks & ".txt")
        '    SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
        '    SS包解读器.读取_有标签("序号", 本次发送序号, 0)
        '    流写入器.WriteLine(讯宝指令 & "_" & 讯宝文本 & " (" & 本次发送序号 & ")")
        '    流写入器.Flush()
        '    流写入器.Close()
        'End If

        Dim 发送者英语讯宝地址 As String = 发送者.英语用户名 & 讯宝地址标识 & 域名_英语
        SS包解读器.读取_有标签("序号", 本次发送序号, 0)
        If 本次发送序号 = 0 Then Return
        Select Case 设备类型
            Case 设备类型_常量集合.手机
                If 本次发送序号 <= 发送者.讯宝序号_手机发送 Then
                    Throw New Exception("1")
                End If
                发送者.讯宝序号_手机发送 = 本次发送序号
            Case 设备类型_常量集合.电脑
                If 本次发送序号 <= 发送者.讯宝序号_电脑发送 Then
                    Throw New Exception("3")
                End If
                发送者.讯宝序号_电脑发送 = 本次发送序号
        End Select
        SS包解读器.读取_有标签("指令", 讯宝指令, 讯宝指令_常量集合.无)
        If 讯宝指令 = 讯宝指令_常量集合.无 Then GoTo 跳转点5
        Select Case 讯宝指令
            Case 讯宝指令_常量集合.确认收到
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("5")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 发送者英语地址 As String = Nothing
                SS包解读器2.读取_有标签("发送者", 发送者英语地址)
                Dim 发送序号 As Long
                SS包解读器2.读取_有标签("发送序号", 发送序号)
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_确认收到(发送者.用户编号, 设备类型, 发送者英语地址, 发送序号)
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("6")
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("7")
                GoTo 跳转点5
            Case 讯宝指令_常量集合.删除讯友
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("8")
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("9")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("10")
                            GoTo 跳转点5
                        End If
                        结果 = 数据库_删除讯友(发送者.用户编号, 位置号, 讯宝文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("11")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("12")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        SS包生成器2.添加_有标签("事件", 同步事件_常量集合.删除讯友)
                        SS包生成器2.添加_有标签("英语讯宝地址", 讯宝文本)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("13")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("14")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.给讯友添加标签
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("15")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 英语讯宝地址 As String = Nothing
                SS包解读器2.读取_有标签("英语讯宝地址", 英语讯宝地址)
                Dim 标签名称 As String = Nothing
                SS包解读器2.读取_有标签("标签名称", 标签名称)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("16")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("17")
                            GoTo 跳转点5
                        End If
                        Dim 目前标签一 As String = Nothing
                        Dim 目前标签二 As String = Nothing
                        结果 = 数据库_获取讯友标签(发送者.用户编号, 英语讯宝地址, 目前标签一, 目前标签二)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("18")
                        If String.IsNullOrEmpty(目前标签一) = False AndAlso String.IsNullOrEmpty(目前标签二) = False Then GoTo 跳转点5
                        If (String.Compare(标签名称, 目前标签一, True) = 0 OrElse String.Compare(标签名称, 目前标签二, True) = 0) Then GoTo 跳转点5
                        Dim 讯友数量 As Integer
                        结果 = 数据库_获取某标签讯友数量(发送者.用户编号, 标签名称, 讯友数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("19")
                        If 讯友数量 >= 最大值_常量集合.每个标签讯友数量 Then GoTo 跳转点5
                        If String.IsNullOrEmpty(目前标签一) = False Then
                            结果 = 数据库_修改讯友标签(发送者.用户编号, 位置号, 英语讯宝地址, 目前标签一, 标签名称)
                        Else
                            结果 = 数据库_修改讯友标签(发送者.用户编号, 位置号, 英语讯宝地址, 标签名称, 目前标签二)
                        End If
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("20")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("21")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        SS包生成器2.添加_有标签("事件", 同步事件_常量集合.讯友添加标签)
                        SS包生成器2.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        SS包生成器2.添加_有标签("标签名称", 标签名称)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("22")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("23")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.移除讯友标签
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("24")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 英语讯宝地址 As String = Nothing
                SS包解读器2.读取_有标签("英语讯宝地址", 英语讯宝地址)
                Dim 标签名称 As String = Nothing
                SS包解读器2.读取_有标签("标签名称", 标签名称)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("25")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("26")
                            GoTo 跳转点5
                        End If
                        Dim 目前标签一 As String = Nothing
                        Dim 目前标签二 As String = Nothing
                        结果 = 数据库_获取讯友标签(发送者.用户编号, 英语讯宝地址, 目前标签一, 目前标签二)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("27")
                        If String.Compare(目前标签一, 标签名称, True) = 0 Then
                            结果 = 数据库_修改讯友标签(发送者.用户编号, 位置号, 英语讯宝地址, Nothing, 目前标签二)
                        ElseIf String.Compare(目前标签二, 标签名称, True) = 0 Then
                            结果 = 数据库_修改讯友标签(发送者.用户编号, 位置号, 英语讯宝地址, 目前标签一, Nothing)
                        End If
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("28")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("29")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        SS包生成器2.添加_有标签("事件", 同步事件_常量集合.讯友移除标签)
                        SS包生成器2.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        SS包生成器2.添加_有标签("标签名称", 标签名称)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("30")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("31")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.修改讯友备注
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("32")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 英语讯宝地址 As String = Nothing
                SS包解读器2.读取_有标签("英语讯宝地址", 英语讯宝地址)
                Dim 备注 As String = Nothing
                SS包解读器2.读取_有标签("备注", 备注)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("33")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("34")
                            GoTo 跳转点5
                        End If
                        结果 = 数据库_修改讯友备注(发送者.用户编号, 英语讯宝地址, 备注)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("35")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("36")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        SS包生成器2.添加_有标签("事件", 同步事件_常量集合.修改讯友备注)
                        SS包生成器2.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        SS包生成器2.添加_有标签("备注", 备注)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("37")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("38")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.拉黑取消拉黑讯友
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("39")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 英语讯宝地址 As String = Nothing
                SS包解读器2.读取_有标签("英语讯宝地址", 英语讯宝地址)
                Dim 拉黑 As Boolean
                SS包解读器2.读取_有标签("拉黑", 拉黑)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("40")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("41")
                            GoTo 跳转点5
                        End If
                        结果 = 数据库_拉黑讯友或取消(发送者.用户编号, 位置号, 英语讯宝地址, 拉黑)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("42")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("43")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        If 拉黑 Then
                            SS包生成器2.添加_有标签("事件", 同步事件_常量集合.拉黑讯友)
                        Else
                            SS包生成器2.添加_有标签("事件", 同步事件_常量集合.取消拉黑讯友)
                        End If
                        SS包生成器2.添加_有标签("英语讯宝地址", 英语讯宝地址)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("44")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("45")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.重命名讯友标签
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("46")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 原标签名称 As String = Nothing
                SS包解读器2.读取_有标签("原标签名称", 原标签名称)
                Dim 新标签名称 As String = Nothing
                SS包解读器2.读取_有标签("新标签名称", 新标签名称)
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 次数 As Integer
                        结果 = 数据库_获取讯友录变动次数(发送者.用户编号, 次数)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("47")
                        If 次数 >= 6 Then
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.对讯友录的编辑过于频繁)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("48")
                            GoTo 跳转点5
                        End If
                        Dim 旧标签名讯友数量 As Integer
                        结果 = 数据库_获取某标签讯友数量(发送者.用户编号, 原标签名称, 旧标签名讯友数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("49")
                        Dim 新标签名讯友数量 As Integer
                        结果 = 数据库_获取某标签讯友数量(发送者.用户编号, 新标签名称, 新标签名讯友数量)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("50")
                        If 旧标签名讯友数量 + 新标签名讯友数量 > 最大值_常量集合.每个标签讯友数量 Then
                            GoTo 跳转点5
                        End If
                        结果 = 数据库_讯友标签重命名(发送者.用户编号, 位置号, 原标签名称, 新标签名称)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("51")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("52")
                        Dim SS包生成器2 As New 类_SS包生成器()
                        SS包生成器2.添加_有标签("事件", 同步事件_常量集合.重命名标签)
                        SS包生成器2.添加_有标签("旧名称", 原标签名称)
                        SS包生成器2.添加_有标签("新名称", 新标签名称)
                        SS包生成器2.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("53")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("54")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.添加黑域, 讯宝指令_常量集合.添加白域
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("55")
                If 跨进程锁.WaitOne = True Then
                    Try
                        Dim 本国语域名 As String = Nothing
                        If 讯宝指令 = 讯宝指令_常量集合.添加黑域 Then
                            If String.Compare(讯宝文本, 黑域_全部) = 0 Then
                                GoTo 跳转点4
                            End If
                        End If
                        结果 = 数据库_查找某个域的讯友(发送者.用户编号, 讯宝文本, 本国语域名)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("56")
跳转点4:
                        结果 = 数据库_添加黑白域(发送者.用户编号, 讯宝文本, 本国语域名, 讯宝指令)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("57")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("58")
                        SS包生成器 = New 类_SS包生成器()
                        If 讯宝指令 = 讯宝指令_常量集合.添加黑域 Then
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.添加黑域)
                        Else
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.添加白域)
                        End If
                        SS包生成器.添加_有标签("英语域名", 讯宝文本)
                        If String.IsNullOrEmpty(本国语域名) = False Then
                            SS包生成器.添加_有标签("本国语域名", 本国语域名)
                        End If
                        SS包生成器.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("59")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("60")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.移除黑域, 讯宝指令_常量集合.移除白域
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("61")
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_移除黑白域(发送者.用户编号, 讯宝文本, 讯宝指令)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("62")
                        Dim 更新时间 As Long
                        结果 = 数据库_更新讯友录更新时间(发送者.用户编号, 更新时间)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("63")
                        SS包生成器 = New 类_SS包生成器()
                        If 讯宝指令 = 讯宝指令_常量集合.移除黑域 Then
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.移除黑域)
                        Else
                            SS包生成器.添加_有标签("事件", 同步事件_常量集合.移除白域)
                        End If
                        SS包生成器.添加_有标签("英语域名", 讯宝文本)
                        SS包生成器.添加_有标签("时间", 更新时间)
                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("64")
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("65")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.修改图标
                SS包解读器.读取_有标签("文件", 讯宝文件数据, Nothing)
                If 讯宝文件数据 Is Nothing Then Throw New Exception("66")
                If 讯宝文件数据.Length > 2 * 长度_常量集合.图标宽高_像素 * 长度_常量集合.图标宽高_像素 Then Throw New Exception("67")
                Dim 头像路径 As String = 头像存放目录 & "\" & 发送者.英语用户名 & ".jpg"
                Try
                    If Directory.Exists(头像存放目录) = False Then Directory.CreateDirectory(头像存放目录)
                    File.WriteAllBytes(头像路径, 讯宝文件数据)
                Catch ex As Exception
                    Throw ex
                End Try
                Dim 文件信息 As New FileInfo(头像路径)
                发送者.头像更新时间 = 文件信息.LastWriteTimeUtc.Ticks
                讯宝文本 = 发送者.头像更新时间
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 本次发送序号, Nothing, 0, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("68")
                Else
                    Throw New Exception("69")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.创建小聊天群
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("70")
                If 跨进程锁.WaitOne = True Then
                    Try
                        群编号 = 0
                        结果 = 数据库_分配位置(发送者, 位置号, 讯宝文本, 群编号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("71")
                        If 群编号 > 0 Then
                            结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 本次发送序号, 发送者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("72")
                        End If
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("73")
                End If
                GoTo 跳转点5
            Case 讯宝指令_常量集合.退出大聊天群
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("74")
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 英语域名 As String = Nothing
                SS包解读器2.读取_有标签("英语域名", 英语域名)
                Dim 主机名 As String = Nothing
                SS包解读器2.读取_有标签("主机名", 主机名)
                Dim 群编号2 As Long
                SS包解读器2.读取_有标签("群编号", 群编号2)
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_退出大聊天群(发送者.用户编号, 英语域名, 主机名, 群编号2)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("75")
                        Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
                        Select Case 设备类型
                            Case 设备类型_常量集合.手机
                                If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
                            Case 设备类型_常量集合.电脑
                                If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
                        End Select
                        If 同步设备类型 <> 设备类型_常量集合.全部 Then
                            Dim SS包生成器2 As New 类_SS包生成器()
                            SS包生成器2.添加_有标签("事件", 同步事件_常量集合.退出大聊天群)
                            SS包生成器2.添加_有标签("英语域名", 英语域名)
                            SS包生成器2.添加_有标签("主机名", 主机名)
                            SS包生成器2.添加_有标签("群编号", 群编号2)
                            结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 同步设备类型, 讯宝指令_常量集合.手机和电脑同步, SS包生成器2.生成纯文本)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("76")
                        End If
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("77")
                End If
                GoTo 跳转点5
        End Select
        SS包解读器.读取_有标签("地址", 接收者英语讯宝地址, Nothing)
        接收者英语讯宝地址 = 接收者英语讯宝地址.Trim.ToLower
        If 是否是有效的讯宝或电子邮箱地址(接收者英语讯宝地址) = False Then Throw New Exception("78")
        Dim 段2() As String = 接收者英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Select Case 段2(1)
            Case 域名_英语, 域名_本国语
            Case Else
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.域内自定义二级讯宝指令集1, 讯宝指令_常量集合.域内自定义二级讯宝指令集2,
                             讯宝指令_常量集合.域内自定义二级讯宝指令集3, 讯宝指令_常量集合.域内自定义二级讯宝指令集4
                        Throw New Exception("79")
                End Select
        End Select
        SS包解读器.读取_有标签("群编号", 群编号, 0)
        If 群编号 > 0 AndAlso String.Compare(接收者英语讯宝地址, 发送者英语讯宝地址) = 0 Then
            发送给自己创建的群 = True
            If 讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then GoTo 跳转点5
        Else
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.删减聊天群成员, 讯宝指令_常量集合.解散小聊天群, 讯宝指令_常量集合.修改聊天群名称
                    GoTo 跳转点5
            End Select
            发送给自己创建的群 = False
            If 跨进程锁.WaitOne = True Then
                Try
                    结果 = 数据库_查找讯友(发送者.用户编号, 接收者英语讯宝地址, 发送者的讯友)
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Throw New Exception("80")
            End If
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("81")
            If 发送者的讯友 Is Nothing Then GoTo 跳转点5
            If 发送者的讯友.拉黑 Then GoTo 跳转点5
            If 接收者英语讯宝地址.EndsWith(讯宝地址标识 & 域名_英语) AndAlso String.Compare(发送者的讯友.主机名, 本服务器主机名) = 0 Then
                If 发送者的讯友.位置号 < 0 OrElse 发送者的讯友.位置号 >= 用户目录.Length Then GoTo 跳转点5
                接收者 = 用户目录(发送者的讯友.位置号)
                If 接收者英语讯宝地址.StartsWith(接收者.英语用户名 & 讯宝地址标识) Then
                    If 跨进程锁.WaitOne = True Then
                        Try
                            结果 = 数据库_查找讯友(接收者.用户编号, 发送者英语讯宝地址, 接收者的讯友)
                        Catch ex As Exception
                            Throw ex
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Throw New Exception("82")
                    End If
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("83")
                    If 接收者的讯友 Is Nothing Then
                        If 群编号 = 0 Then
                            Select Case 讯宝指令
                                Case 讯宝指令_常量集合.邀请加入小聊天群
                                    讯宝无法发送的原因 = 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友
                                    SS包解读器.读取_有标签("文本", 讯宝文本)
                                    Dim SS包解读器2 As New 类_SS包解读器()
                                    SS包解读器2.解读纯文本(讯宝文本)
                                    SS包解读器2.读取_有标签("I", 群编号)
                                    If 跨进程锁.WaitOne = True Then
                                        Try
                                            结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 发送者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝无法发送的原因)
                                        Catch ex As Exception
                                            Throw ex
                                        Finally
                                            跨进程锁.ReleaseMutex()
                                        End Try
                                    Else
                                        Throw New Exception("84")
                                    End If
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("85")
                                    GoTo 跳转点5
                                Case 讯宝指令_常量集合.邀请加入大聊天群
                                    Dim 是白域, 是黑域 As Boolean
                                    If 跨进程锁.WaitOne = True Then
                                        Try
                                            结果 = 数据库_是否白域(接收者.用户编号, 域名_英语, 是白域)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("86")
                                            If 是白域 = False Then
                                                结果 = 数据库_是否黑域(接收者.用户编号, 域名_英语, 是黑域)
                                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("87")
                                            End If
                                            If 是白域 = False AndAlso 是黑域 = True Then
                                                讯宝无法发送的原因 = 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友
                                                SS包解读器.读取_有标签("文本", 讯宝文本)
                                                结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 发送者英语讯宝地址, 0, 发送者.用户编号, 位置号, 设备类型, 讯宝无法发送的原因, 讯宝文本)
                                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("88")
                                                GoTo 跳转点5
                                            End If
                                        Catch ex As Exception
                                            Throw ex
                                        Finally
                                            跨进程锁.ReleaseMutex()
                                        End Try
                                    Else
                                        Throw New Exception("89")
                                    End If
                                Case 讯宝指令_常量集合.发送文字
                                    Dim 是白域, 是黑域 As Boolean
                                    If 跨进程锁.WaitOne = True Then
                                        Try
                                            结果 = 数据库_是否白域(接收者.用户编号, 域名_英语, 是白域)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("90")
                                            If 是白域 = False Then
                                                结果 = 数据库_是否黑域(接收者.用户编号, 域名_英语, 是黑域)
                                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("91")
                                            End If
                                        Catch ex As Exception
                                            Throw ex
                                        Finally
                                            跨进程锁.ReleaseMutex()
                                        End Try
                                    Else
                                        Throw New Exception("92")
                                    End If
                                    If 是白域 = False AndAlso 是黑域 = True Then
                                        讯宝无法发送的原因 = 讯宝指令_常量集合.对方未添加我为讯友
                                        GoTo 跳转点1
                                    End If
                                Case Else
                                    讯宝无法发送的原因 = 讯宝指令_常量集合.对方未添加我为讯友
                                    GoTo 跳转点1
                            End Select
                        Else
                            讯宝无法发送的原因 = 讯宝指令_常量集合.对方未添加我为讯友
                            GoTo 跳转点1
                        End If
                    ElseIf 接收者的讯友.拉黑 = True Then
                        讯宝无法发送的原因 = 讯宝指令_常量集合.对方把我拉黑了
                        GoTo 跳转点1
                    End If
                    接收者在本服务器上 = True
                Else
                    讯宝无法发送的原因 = 讯宝指令_常量集合.讯宝地址不存在
                    GoTo 跳转点1
                End If
            Else
                接收者在本服务器上 = False
            End If
        End If
        Dim 视频预览图片数据() As Byte = Nothing
        Select Case 讯宝指令
            Case 讯宝指令_常量集合.发送文字
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("93")
            Case 讯宝指令_常量集合.发送语音
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("94")
                讯宝文本 = 生成文件名_发送语音图片短视频时(本次发送序号, 讯宝文本)
                SS包解读器.读取_有标签("秒数", 秒数, 0)
                SS包解读器.读取_有标签("文件", 讯宝文件数据, Nothing)
                If 讯宝文件数据 Is Nothing Then Throw New Exception("95")
            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("96")
                讯宝文本 = 生成文件名_发送语音图片短视频时(本次发送序号, 讯宝文本)
                If 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                    SS包解读器.读取_有标签("预览", 视频预览图片数据, Nothing)
                    If 视频预览图片数据 Is Nothing Then Throw New Exception("97")
                End If
                SS包解读器.读取_有标签("宽度", 宽度, 0)
                If 宽度 < 1 OrElse 宽度 > 最大值_常量集合.讯宝预览图片宽高_像素 Then Throw New Exception("98")
                SS包解读器.读取_有标签("高度", 高度, 0)
                If 高度 < 1 OrElse 高度 > 最大值_常量集合.讯宝预览图片宽高_像素 Then Throw New Exception("99")
                SS包解读器.读取_有标签("文件", 讯宝文件数据, Nothing)
                If 讯宝文件数据 Is Nothing Then Throw New Exception("100")
            Case 讯宝指令_常量集合.发送文件
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("101")
                讯宝文本 = 生成文件名_发送文件时(本次发送序号, 讯宝文本)
                SS包解读器.读取_有标签("文件", 讯宝文件数据, Nothing)
                If 讯宝文件数据 Is Nothing Then Throw New Exception("102")
            Case 讯宝指令_常量集合.撤回
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                Dim 发送序号_撤回的讯宝 As Long
                If Long.TryParse(讯宝文本, 发送序号_撤回的讯宝) = False Then Throw New Exception("103")
                If 发送序号_撤回的讯宝 = 本次发送序号 Then Throw New Exception("104")
            Case 讯宝指令_常量集合.邀请加入小聊天群
                SS包解读器.读取_有标签("文本", 讯宝文本)
                Dim SS包解读器2 As New 类_SS包解读器()
                SS包解读器2.解读纯文本(讯宝文本)
                Dim 群编号2 As Byte
                SS包解读器2.读取_有标签("I", 群编号2)
                If 群编号2 = 0 Then Throw New Exception("105")
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_添加邀请(发送者.用户编号, 群编号2, 发送者的讯友)
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("106")
                End If
                Select Case 结果.查询结果
                    Case 查询结果_常量集合.成功
                    Case 查询结果_常量集合.失败
                        讯宝无法发送的原因 = 讯宝指令_常量集合.已是群成员
                        GoTo 跳转点2
                    Case 查询结果_常量集合.出错
                        讯宝无法发送的原因 = 讯宝指令_常量集合.群成员数量已达上限
跳转点2:
                        If 跨进程锁.WaitOne = True Then
                            Try
                                结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 发送者英语讯宝地址, 群编号2, 发送者.用户编号, 位置号, 设备类型, 讯宝无法发送的原因)
                            Catch ex As Exception
                                Throw ex
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        Else
                            Throw New Exception("107")
                        End If
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("108")
                        GoTo 跳转点5
                    Case Else : Throw New Exception("109")
                End Select
            Case 讯宝指令_常量集合.某人加入聊天群, 讯宝指令_常量集合.邀请加入大聊天群
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("110")
            Case 讯宝指令_常量集合.退出小聊天群
                If 群编号 > 0 Then
                    If 跨进程锁.WaitOne = True Then
                        Try
                            结果 = 数据库_移除加入的群(发送者.用户编号, 接收者英语讯宝地址, 群编号)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("111")
                        Catch ex As Exception
                            Throw ex
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Throw New Exception("112")
                    End If
                Else
                    Throw New Exception("113")
                End If
            Case 讯宝指令_常量集合.删减聊天群成员
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("114")
                If String.Compare(讯宝文本, 发送者英语讯宝地址) = 0 Then Throw New Exception("115")
            Case 讯宝指令_常量集合.修改聊天群名称
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                If String.IsNullOrEmpty(讯宝文本) Then Throw New Exception("116")
                If 群编号 > 0 Then
                    If 跨进程锁.WaitOne = True Then
                        Try
                            结果 = 数据库_修改群备注(发送者.用户编号, 接收者英语讯宝地址, 群编号, 讯宝文本)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("117")
                            Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
                            Select Case 设备类型
                                Case 设备类型_常量集合.手机 : If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
                                Case 设备类型_常量集合.电脑 : If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
                                Case Else : Throw New Exception("118")
                            End Select
                            If 同步设备类型 <> 设备类型_常量集合.全部 Then
                                SS包生成器 = New 类_SS包生成器()
                                SS包生成器.添加_有标签("事件", 同步事件_常量集合.修改群名称)
                                SS包生成器.添加_有标签("群编号", 群编号)
                                SS包生成器.添加_有标签("群备注", 讯宝文本)
                                结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 同步设备类型, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("119")
                            End If
                        Catch ex As Exception
                            Throw ex
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Throw New Exception("120")
                    End If
                Else
                    Throw New Exception("121")
                End If
            Case 讯宝指令_常量集合.获取小聊天群成员列表
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
            Case 讯宝指令_常量集合.解散小聊天群
            Case Else : Throw New Exception("122")
        End Select
        If 接收者在本服务器上 OrElse 发送给自己创建的群 Then
            If 讯宝无法发送的原因 = 讯宝指令_常量集合.无 Then
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_统计发送次数(发送者.用户编号, 位置号)
                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                            Select Case 结果.查询结果
                                Case 查询结果_常量集合.本小时发送的讯宝数量已达上限
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.本小时发送的讯宝数量已达上限)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("123")
                                    GoTo 跳转点5
                                Case 查询结果_常量集合.今日发送的讯宝数量已达上限
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.今日发送的讯宝数量已达上限)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("124")
                                    GoTo 跳转点5
                                Case 查询结果_常量集合.失败 : GoTo 跳转点5
                                Case Else : Throw New Exception("125")
                            End Select
                        End If
                        If 群编号 = 0 Then
                            结果 = 数据库_统计个人接收次数(接收者.用户编号, 发送者的讯友.位置号)
                            If 结果.查询结果 = 查询结果_常量集合.出错 Then Throw New Exception("126")
                            If 接收者.网络连接器_手机 IsNot Nothing OrElse 接收者.网络连接器_电脑 IsNot Nothing Then
                                结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 发送者.用户编号, 本次发送序号, Nothing, 0, 接收者.用户编号, 发送者的讯友.位置号, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                            Else
                                Select Case 讯宝指令
                                    Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                        结果 = 数据库_记录带文件的讯宝(发送者.用户编号, 讯宝指令, 讯宝文本)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("127")
                                    Case 讯宝指令_常量集合.发送文件
                                        Dim SS包解读器2 As New 类_SS包解读器
                                        SS包解读器2.解读纯文本(讯宝文本)
                                        Dim 存储文件名 As String = ""
                                        SS包解读器2.读取_有标签("S", 存储文件名)
                                        结果 = 数据库_记录带文件的讯宝(发送者.用户编号, 讯宝指令, 存储文件名)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("128")
                                End Select
                                结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 本次发送序号, Nothing, 0, 接收者.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者)
                            End If
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("129")
                        Else
                            Dim 加入者本国语讯宝地址 As String = Nothing
                            Dim 加入者主机名 As String = Nothing
                            Dim 加入者位置号 As Short
                            Dim 角色 As 群角色_常量集合 = 群角色_常量集合.无
                            If 发送给自己创建的群 Then
                                结果 = 数据库_获取加入者信息(发送者.用户编号, 群编号, 发送者英语讯宝地址, 加入者本国语讯宝地址, 加入者主机名, 加入者位置号, 角色)
                            Else
                                结果 = 数据库_获取加入者信息(接收者.用户编号, 群编号, 发送者英语讯宝地址, 加入者本国语讯宝地址, 加入者主机名, 加入者位置号, 角色)
                            End If
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("130")
                            If 角色 = 群角色_常量集合.无 Then
                                If 讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令, 生成文本_某人离开聊天群的通知(发送者英语讯宝地址))
                                Else
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.不是群成员)
                                End If
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("131")
                                GoTo 跳转点5
                            End If
                            Dim 群成员() As 小聊天群成员_复合数据 = Nothing
                            Dim 群成员数, 群正式成员数 As Short
                            If 发送给自己创建的群 Then
                                结果 = 数据库_发送讯宝时获取群成员(发送者.用户编号, 群编号, 群成员, 群成员数)
                            Else
                                结果 = 数据库_发送讯宝时获取群成员(接收者.用户编号, 群编号, 群成员, 群成员数)
                            End If
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("132")
                            Dim 群主讯宝地址 As String = Nothing
                            Dim I As Integer
                            For I = 0 To 群成员数 - 1
                                If 群成员(I).角色 = 群角色_常量集合.群主 Then
                                    群主讯宝地址 = 群成员(I).英语讯宝地址
                                    Exit For
                                End If
                            Next
                            If String.Compare(群主讯宝地址, 接收者英语讯宝地址) <> 0 Then GoTo 跳转点5
                            Dim 发送给发送者 As Boolean
                            If 讯宝指令 = 讯宝指令_常量集合.获取小聊天群成员列表 Then
                                If 角色 = 群角色_常量集合.邀请加入_可以发言 Then
                                    Dim 加入了 As Boolean
                                    结果 = 数据库_是否加入了群(发送者.用户编号, 群主讯宝地址, 群编号, 加入了)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("133")
                                    If 加入了 = False Then
                                        Dim 加入的群数 As Integer
                                        结果 = 数据库_获取加入的群数量(发送者.用户编号, 加入的群数)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("134")
                                        If 加入的群数 >= 最大值_常量集合.每个用户可加入的小聊天群数量 Then
                                            结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.加入的群数量已达上限)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("135")
                                            GoTo 跳转点5
                                        End If
                                        结果 = 数据库_加入群(发送者.用户编号, 位置号, 群主讯宝地址, 群编号, 讯宝文本)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("136")
                                    End If
                                    结果 = 数据库_成为群成员(接收者.用户编号, 群编号, 发送者英语讯宝地址)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("137")
                                    For I = 0 To 群成员数 - 1
                                        If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                            群成员(I).角色 = 群角色_常量集合.成员_可以发言
                                            Exit For
                                        End If
                                    Next
                                    Dim 同步设备类型 As 设备类型_常量集合 = 设备类型_常量集合.全部
                                    Select Case 设备类型
                                        Case 设备类型_常量集合.手机 : If 发送者.网络连接器_电脑 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.电脑
                                        Case 设备类型_常量集合.电脑 : If 发送者.网络连接器_手机 IsNot Nothing Then 同步设备类型 = 设备类型_常量集合.手机
                                        Case Else : Throw New Exception("138")
                                    End Select
                                    If 同步设备类型 <> 设备类型_常量集合.全部 Then
                                        SS包生成器 = New 类_SS包生成器()
                                        SS包生成器.添加_有标签("事件", 同步事件_常量集合.加入小聊天群)
                                        SS包生成器.添加_有标签("群主讯宝地址", 群主讯宝地址)
                                        SS包生成器.添加_有标签("群编号", 群编号)
                                        SS包生成器.添加_有标签("群备注", 讯宝文本)
                                        结果 = 数据库_存为推送的讯宝(发送者.英语用户名 & 讯宝地址标识 & 域名_英语, 0, 0, Nothing, 0, 发送者.用户编号, 位置号, 同步设备类型, 讯宝指令_常量集合.手机和电脑同步, SS包生成器.生成纯文本)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("139")
                                    End If
                                End If
                                结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.获取小聊天群成员列表, 生成文本_获取小聊天群成员列表(群成员, 群成员数))
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("140")
                                If 角色 = 群角色_常量集合.邀请加入_可以发言 Then
                                    讯宝指令 = 讯宝指令_常量集合.某人加入聊天群
                                    讯宝文本 = 生成文本_某人加入小聊天群的通知(发送者英语讯宝地址, IIf(String.IsNullOrEmpty(发送者.本国语用户名), Nothing, 发送者.本国语用户名 & 讯宝地址标识 & 域名_本国语), 本服务器主机名, 位置号)
                                    发送给发送者 = True
                                Else
                                    GoTo 跳转点5
                                End If
                            End If
                            For I = 0 To 群成员数 - 1
                                If 群成员(I).角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                    群正式成员数 += 1
                                End If
                            Next
                            If 群正式成员数 > 1 OrElse 讯宝指令 = 讯宝指令_常量集合.删减聊天群成员 Then
                                If 讯宝指令 = 讯宝指令_常量集合.解散小聊天群 Then
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.群里还有成员)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("141")
                                    GoTo 跳转点5
                                End If
                                Dim 发送者本国语讯宝地址 As String
                                Dim 发送者主机名 As String
                                For I = 0 To 群成员数 - 1
                                    If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                        If 群成员(I).角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                            Exit For
                                        Else
                                            GoTo 跳转点3
                                        End If
                                    End If
                                Next
                                If I < 群成员数 Then
                                    发送者本国语讯宝地址 = 群成员(I).本国语讯宝地址
                                    发送者主机名 = 群成员(I).主机名
                                    Select Case 讯宝指令
                                        Case 讯宝指令_常量集合.退出小聊天群
                                            结果 = 数据库_删除群成员(接收者.用户编号, 群编号, 发送者英语讯宝地址)
                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("142")
                                            讯宝文本 = 生成文本_某人离开聊天群的通知(发送者英语讯宝地址, 发送者本国语讯宝地址)
                                            发送给发送者 = True
                                        Case 讯宝指令_常量集合.删减聊天群成员
                                            For I = 0 To 群成员数 - 1
                                                If String.Compare(群成员(I).英语讯宝地址, 讯宝文本) = 0 Then
                                                    Exit For
                                                End If
                                            Next
                                            If I < 群成员数 Then
                                                With 群成员(I)
                                                    结果 = 数据库_删除群成员(发送者.用户编号, 群编号, .英语讯宝地址)
                                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("143")
                                                    讯宝文本 = 生成文本_某人离开聊天群的通知(.英语讯宝地址, .本国语讯宝地址)
                                                    If .角色 <> 群角色_常量集合.邀请加入_可以发言 Then
                                                        发送给发送者 = True
                                                    Else
                                                        结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.删减聊天群成员, 讯宝文本)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("144")
                                                        GoTo 跳转点5
                                                    End If
                                                End With
                                            Else
                                                GoTo 跳转点5
                                            End If
                                    End Select
                                Else
跳转点3:
                                    If 讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then
                                        结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令, 生成文本_某人离开聊天群的通知(发送者英语讯宝地址))
                                    Else
                                        结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.不是群成员)
                                    End If
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("145")
                                    GoTo 跳转点5
                                End If
                                Dim SS包生成器_本服务器群成员 As New 类_SS包生成器(, 群正式成员数)
                                Dim 接收者服务器(群正式成员数 - 1) As 接收者服务器_复合数据
                                Dim 接收者服务器数量, J As Integer
                                Dim 接收者服务器子域名, 段() As String
                                Dim 接收者编号_本服务器 As Long
                                Dim 接收者位置号_本服务器 As Short
                                Dim 某一群成员 As 小聊天群成员_复合数据
                                Dim 标识加域名 As String = 讯宝地址标识 & 域名_英语
                                For I = 0 To 群成员数 - 1
                                    某一群成员 = 群成员(I)
                                    If 某一群成员.角色 >= 群角色_常量集合.成员_可以发言 Then
                                        If String.Compare(某一群成员.英语讯宝地址, 发送者英语讯宝地址) <> 0 OrElse 发送给发送者 = True Then
                                            If 某一群成员.英语讯宝地址.EndsWith(标识加域名) AndAlso String.Compare(某一群成员.主机名, 本服务器主机名) = 0 Then
                                                Dim 接收者2 As 类_用户 = 用户目录(某一群成员.位置号)
                                                If 某一群成员.英语讯宝地址.StartsWith(接收者2.英语用户名 & 讯宝地址标识) Then
                                                    If 讯宝指令 = 讯宝指令_常量集合.修改聊天群名称 Then
                                                        结果 = 数据库_修改群备注(接收者2.用户编号, 群主讯宝地址, 群编号, 讯宝文本)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("146")
                                                    End If
                                                    结果 = 数据库_统计个人接收次数(接收者2.用户编号, 某一群成员.位置号)
                                                    If 结果.查询结果 = 查询结果_常量集合.出错 Then Throw New Exception("147")
                                                    If 接收者2.网络连接器_手机 IsNot Nothing OrElse 接收者2.网络连接器_电脑 IsNot Nothing Then
                                                        SS包生成器 = New 类_SS包生成器()
                                                        SS包生成器.添加_有标签("用户编号", 接收者2.用户编号)
                                                        SS包生成器.添加_有标签("位置号", 某一群成员.位置号)
                                                        SS包生成器_本服务器群成员.添加_有标签("群成员", SS包生成器)
                                                        If SS包生成器_本服务器群成员.数据量 = 1 Then
                                                            接收者编号_本服务器 = 接收者2.用户编号
                                                            接收者位置号_本服务器 = 某一群成员.位置号
                                                        End If
                                                    Else
                                                        结果 = 数据库_存为不推送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 本次发送序号, 接收者英语讯宝地址, 群编号, 接收者2.用户编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 接收者2)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("148")
                                                    End If
                                                End If
                                            Else
                                                SS包生成器 = New 类_SS包生成器()
                                                SS包生成器.添加_有标签("讯宝地址", 某一群成员.英语讯宝地址)
                                                SS包生成器.添加_有标签("位置号", 某一群成员.位置号)
                                                段 = 某一群成员.英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                                                接收者服务器子域名 = 某一群成员.主机名 & "." & 段(1)
                                                For J = 0 To 接收者服务器数量 - 1
                                                    If String.Compare(接收者服务器(J).子域名, 接收者服务器子域名) = 0 Then Exit For
                                                Next
                                                If J < 接收者服务器数量 Then
                                                    接收者服务器(J).SS包生成器.添加_有标签("群成员", SS包生成器)
                                                Else
                                                    With 接收者服务器(接收者服务器数量)
                                                        .子域名 = 接收者服务器子域名
                                                        .SS包生成器 = New 类_SS包生成器(, 群成员数)
                                                        .SS包生成器.添加_有标签("群成员", SS包生成器)
                                                        If .SS包生成器.数据量 = 1 Then
                                                            .讯宝地址 = 某一群成员.英语讯宝地址
                                                            .位置号 = 某一群成员.位置号
                                                            .主机名 = 某一群成员.主机名
                                                        End If
                                                    End With
                                                    接收者服务器数量 += 1
                                                End If
                                            End If
                                        End If
                                    End If
                                Next
                                If 接收者服务器数量 > 0 Then
                                    Dim 当前时刻 As Long = Date.UtcNow.Ticks
                                    For I = 0 To 接收者服务器数量 - 1
                                        With 接收者服务器(I)
                                            If .SS包生成器.数据量 > 1 Then
                                                结果 = 数据库_保存要发送的讯宝2(当前时刻, 发送者英语讯宝地址, 发送者.用户编号, 位置号, 本次发送序号, .SS包生成器.生成SS包, .主机名, 接收者英语讯宝地址, 群编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 设备类型)
                                            Else
                                                结果 = 数据库_保存要发送的讯宝(当前时刻, 发送者英语讯宝地址, 发送者.用户编号, 位置号, 本次发送序号, .讯宝地址, .主机名, .位置号, 接收者英语讯宝地址, 群编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 设备类型)
                                            End If
                                        End With
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("149")
                                        当前时刻 += 1
                                    Next
                                End If
                                Dim 数据量 As Integer = SS包生成器_本服务器群成员.数据量
                                If 数据量 > 1 Then
                                    结果 = 数据库_存为推送的讯宝2(发送者英语讯宝地址, 发送者.用户编号, 本次发送序号, 接收者英语讯宝地址, 群编号, SS包生成器_本服务器群成员.生成SS包, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                                ElseIf 数据量 = 1 Then
                                    结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 发送者.用户编号, 本次发送序号, 接收者英语讯宝地址, 群编号, 接收者编号_本服务器, 接收者位置号_本服务器, 设备类型_常量集合.全部, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                                ElseIf 接收者服务器数量 = 0 Then
                                    Select Case 讯宝指令
                                        Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                            结果 = 数据库_记录带文件的讯宝(发送者.用户编号, 讯宝指令, 讯宝文本)
                                        Case 讯宝指令_常量集合.发送文件
                                            Dim SS包解读器2 As New 类_SS包解读器
                                            SS包解读器2.解读纯文本(讯宝文本)
                                            Dim 存储文件名 As String = ""
                                            SS包解读器2.读取_有标签("S", 存储文件名)
                                            结果 = 数据库_记录带文件的讯宝(发送者.用户编号, 讯宝指令, 存储文件名)
                                    End Select
                                End If
                            Else
                                If 讯宝指令 = 讯宝指令_常量集合.解散小聊天群 Then
                                    结果 = 数据库_删除群(发送者.用户编号, 群编号, 接收者英语讯宝地址)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("150")
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型_常量集合.全部, 讯宝指令_常量集合.解散小聊天群)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("151")
                                ElseIf 讯宝指令 <> 讯宝指令_常量集合.修改聊天群名称 Then
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.群里没有成员)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("152")
                                End If
                                GoTo 跳转点5
                            End If
                        End If
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("153")
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("154")
                If 讯宝文件数据 IsNot Nothing Then
                    Dim 路径 As String
                    路径 = 数据存放路径 & "SS\" & 发送者.用户编号
                    If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                    If 讯宝指令 = 讯宝指令_常量集合.发送文件 Then
                        Dim SS包解读器2 As New 类_SS包解读器
                        SS包解读器2.解读纯文本(讯宝文本)
                        Dim 存储文件名 As String = ""
                        SS包解读器2.读取_有标签("S", 存储文件名)
                        路径 &= "\" & 存储文件名
                    Else
                        路径 &= "\" & 讯宝文本
                    End If
                    File.WriteAllBytes(路径, 讯宝文件数据)
                    If 讯宝指令 = 讯宝指令_常量集合.发送图片 Then
                        生成预览图片(路径)
                    ElseIf 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                        File.WriteAllBytes(路径 & ".jpg", 视频预览图片数据)
                    End If
                End If
            Else
跳转点1:
                If 讯宝指令 = 讯宝指令_常量集合.撤回 Then GoTo 跳转点5
                If 跨进程锁.WaitOne = True Then
                    Try
                        结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, IIf(群编号 > 0, 接收者英语讯宝地址, Nothing), 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝无法发送的原因)
                    Catch ex As Exception
                        Throw ex
                    Finally
                        跨进程锁.ReleaseMutex()
                    End Try
                Else
                    Throw New Exception("155")
                End If
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("156")
            End If
        Else
            If 跨进程锁.WaitOne = True Then
                Try
                    If 讯宝指令 = 讯宝指令_常量集合.获取小聊天群成员列表 Then
                        If String.IsNullOrEmpty(讯宝文本) = False Then
                            Dim 加入了 As Boolean
                            结果 = 数据库_是否加入了群(发送者.用户编号, 接收者英语讯宝地址, 群编号, 加入了)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("157")
                            If 加入了 = False Then
                                Dim 加入的群数 As Integer
                                结果 = 数据库_获取加入的群数量(发送者.用户编号, 加入的群数)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("158")
                                If 加入的群数 >= 最大值_常量集合.每个用户可加入的小聊天群数量 Then
                                    结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.加入的群数量已达上限)
                                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("159")
                                    GoTo 跳转点5
                                End If
                                结果 = 数据库_加入群(发送者.用户编号, 位置号, 接收者英语讯宝地址, 群编号, 讯宝文本)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("160")
                            End If
                        End If
                    End If
                    结果 = 数据库_统计发送次数(发送者.用户编号, 位置号)
                    If 结果.查询结果 <> 查询结果_常量集合.成功 Then
                        Select Case 结果.查询结果
                            Case 查询结果_常量集合.本小时发送的讯宝数量已达上限
                                结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.本小时发送的讯宝数量已达上限)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("161")
                                GoTo 跳转点5
                            Case 查询结果_常量集合.今日发送的讯宝数量已达上限
                                结果 = 数据库_存为推送的讯宝(接收者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 设备类型, 讯宝指令_常量集合.今日发送的讯宝数量已达上限)
                                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("162")
                                GoTo 跳转点5
                            Case 查询结果_常量集合.失败 : GoTo 跳转点5
                            Case Else : Throw New Exception("163")
                        End Select
                    End If
                    结果 = 数据库_保存要发送的讯宝(Date.UtcNow.Ticks, 发送者英语讯宝地址, 发送者.用户编号, 位置号, 本次发送序号, 接收者英语讯宝地址, 发送者的讯友.主机名, 发送者的讯友.位置号, IIf(群编号 > 0, 接收者英语讯宝地址, Nothing), 群编号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 设备类型)
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Throw New Exception("164")
            End If
            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("165")
            If 讯宝文件数据 IsNot Nothing Then
                Dim 路径 As String
                路径 = 数据存放路径 & "SS\" & 发送者.用户编号
                If Directory.Exists(路径) = False Then Directory.CreateDirectory(路径)
                If 讯宝指令 = 讯宝指令_常量集合.发送文件 Then
                    Dim SS包解读器2 As New 类_SS包解读器
                    SS包解读器2.解读纯文本(讯宝文本)
                    Dim 存储文件名 As String = ""
                    SS包解读器2.读取_有标签("S", 存储文件名)
                    路径 &= "\" & 存储文件名
                Else
                    路径 &= "\" & 讯宝文本
                End If
                File.WriteAllBytes(路径, 讯宝文件数据)
                If 讯宝指令 = 讯宝指令_常量集合.发送图片 Then
                    生成预览图片(路径)
                ElseIf 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                    File.WriteAllBytes(路径 & ".jpg", 视频预览图片数据)
                End If
            End If
        End If
        If 讯宝指令 < 讯宝指令_常量集合.视频通话请求 Then
            Dim 另一设备的类型 As 设备类型_常量集合
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    If 发送者.网络连接器_电脑 Is Nothing Then GoTo 跳转点5
                    另一设备的类型 = 设备类型_常量集合.电脑
                Case 设备类型_常量集合.电脑
                    If 发送者.网络连接器_手机 Is Nothing Then GoTo 跳转点5
                    另一设备的类型 = 设备类型_常量集合.手机
                Case Else : GoTo 跳转点5
            End Select
            If 跨进程锁.WaitOne = True Then
                Try
                    结果 = 数据库_存为推送的讯宝(发送者英语讯宝地址, 0, 本次发送序号, 接收者英语讯宝地址, 群编号, 发送者.用户编号, 位置号, 另一设备的类型, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数)
                Catch ex As Exception
                    Throw ex
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
                If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception("166")
            End If
        End If
跳转点5:
        Select Case 设备类型
            Case 设备类型_常量集合.手机
                发送者.手机等待确认的发送序号 = 本次发送序号
            Case 设备类型_常量集合.电脑
                发送者.电脑等待确认的发送序号 = 本次发送序号
        End Select
    End Sub

    Private Function 数据库_确认收到(ByVal 接收者编号 As Long, ByVal 设备类型 As 设备类型_常量集合, ByVal 发送者英语地址 As String, ByVal 发送序号 As Long)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 发送者英语地址)
            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
            列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 接收者编号)
            Select Case 设备类型
                Case 设备类型_常量集合.手机
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.电脑端接收成功)
                Case 设备类型_常量集合.电脑
                    列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.手机端接收成功)
            End Select
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "讯宝不推送", 筛选器, "#发送者发送序号接收者")
            If 指令.执行() = 0 Then
                Dim 列添加器_新数据 As New 类_列添加器
                Select Case 设备类型
                    Case 设备类型_常量集合.手机
                        列添加器_新数据.添加列_用于插入数据("结果", 讯宝接收结果_常量集合.手机端接收成功)
                    Case 设备类型_常量集合.电脑
                        列添加器_新数据.添加列_用于插入数据("结果", 讯宝接收结果_常量集合.电脑端接收成功)
                End Select
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 发送者英语地址)
                列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                列添加器.添加列_用于筛选器("接收者编号", 筛选方式_常量集合.等于, 接收者编号)
                Select Case 设备类型
                    Case 设备类型_常量集合.手机
                        列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.待确认)
                    Case 设备类型_常量集合.电脑
                        列添加器.添加列_用于筛选器("结果", 筛选方式_常量集合.等于, 讯宝接收结果_常量集合.待确认)
                End Select
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令_更新 As New 类_数据库指令_更新数据(副数据库, "讯宝不推送", 列添加器_新数据, 筛选器, "#发送者发送序号接收者")
                指令_更新.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_查找讯友(ByVal 用户编号 As Long, ByVal 英语讯宝地址 As String, Optional ByRef 讯友 As 类_讯友 = Nothing) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"英语讯宝地址", "本国语讯宝地址", "主机名", "位置号", "拉黑"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器, , "#用户英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                讯友 = New 类_讯友
                讯友.英语讯宝地址 = 读取器(0)
                讯友.本国语讯宝地址 = 读取器(1)
                讯友.主机名 = 读取器(2)
                讯友.位置号 = 读取器(3)
                讯友.拉黑 = 读取器(4)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_统计发送次数(ByVal 用户编号 As Long, ByVal 位置号 As Short) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 收发统计 As 收发统计_复合数据
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "收发统计", 筛选器, 1, , , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                收发统计.今日几号 = 读取器(2)
                收发统计.今日发送 = 读取器(3)
                收发统计.今日接收 = 读取器(4)
                收发统计.昨日发送 = 读取器(5)
                收发统计.昨日接收 = 读取器(6)
                收发统计.前日发送 = 读取器(7)
                收发统计.前日接收 = 读取器(8)
                收发统计.今日几时 = 读取器(9)
                收发统计.时段发送 = 读取器(10)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
        Dim 当前时间 As Date = Date.Now
        Dim 今日几号 As Integer = Integer.Parse(当前时间.Year & Format(当前时间.DayOfYear, "000"))
        Dim 今日几时 As Byte = CByte(当前时间.Hour)
        If 收发统计.今日几号 > 0 Then
            If 今日几号 = 收发统计.今日几号 Then
                If 收发统计.今日发送 < 最大值_常量集合.每人每天可发送讯宝数量 Then
                    If 收发统计.时段发送 >= 最大值_常量集合.每人每小时可发送讯宝数量 Then
                        If 收发统计.今日几时 = 今日几时 Then
                            Return New 类_SS包生成器(查询结果_常量集合.本小时发送的讯宝数量已达上限)
                        Else
                            Return 数据库_更新今日收发统计(用户编号, 收发统计.今日发送 + 1, 收发统计.今日接收, 今日几时, 1)
                        End If
                    End If
                    If 收发统计.今日几时 = 今日几时 Then
                        Return 数据库_更新今日收发统计(用户编号, 收发统计.今日发送 + 1, 收发统计.今日接收, 今日几时, 收发统计.时段发送 + 1)
                    Else
                        Return 数据库_更新今日收发统计(用户编号, 收发统计.今日发送 + 1, 收发统计.今日接收, 今日几时, 1)
                    End If
                Else
                    Return New 类_SS包生成器(查询结果_常量集合.今日发送的讯宝数量已达上限)
                End If
            Else
                Dim 昨日时间 As Date = 当前时间.AddDays(-1)
                Dim 昨日几号 As Integer = Integer.Parse(昨日时间.Year & Format(昨日时间.DayOfYear, "000"))
                If 昨日几号 = 收发统计.今日几号 Then
                    Return 数据库_更新收发统计(用户编号, 今日几号, 1, 0, 收发统计.今日发送, 收发统计.今日接收, 收发统计.昨日发送, 收发统计.昨日接收, 今日几时, 1)
                Else
                    Dim 前日时间 As Date = 昨日时间.AddDays(-1)
                    Dim 前日几号 As Integer = Integer.Parse(前日时间.Year & Format(前日时间.DayOfYear, "000"))
                    If 前日几号 = 收发统计.今日几号 Then
                        Return 数据库_更新收发统计(用户编号, 今日几号, 1, 0, 0, 0, 收发统计.今日发送, 收发统计.今日接收, 今日几时, 1)
                    Else
                        Return 数据库_更新收发统计(用户编号, 今日几号, 1, 0, 0, 0, 0, 0, 今日几时, 1)
                    End If
                End If
            End If
        Else
            Return 数据库_添加收发统计(用户编号, 位置号, 今日几号, 1, 0, 今日几时, 1)
        End If
    End Function

    Private Function 数据库_保存要发送的讯宝(ByVal 时间 As Long, ByVal 发送者英语地址 As String, ByVal 发送者编号 As Long,
                                  ByVal 发送者位置号 As Short, ByVal 发送序号 As Long, ByVal 接收者英语地址 As String,
                                  ByVal 接收者主机名 As String, ByVal 接收者位置号 As Short, ByVal 群主英语地址 As String,
                                  ByVal 群编号 As Byte, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String,
                                  ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 设备类型 As 设备类型_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("时间", 0)
                列添加器.添加列_用于插入数据("接收者", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", 时间)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("发送者编号", 发送者编号)
            列添加器.添加列_用于插入数据("发送者位置号", 发送者位置号)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            If String.IsNullOrEmpty(接收者英语地址) = False Then
                列添加器.添加列_用于插入数据("接收者英语地址", 接收者英语地址)
            End If
            列添加器.添加列_用于插入数据("接收者主机名", 接收者主机名)
            列添加器.添加列_用于插入数据("接收者位置号", 接收者位置号)
            If String.IsNullOrEmpty(群主英语地址) = False Then
                列添加器.添加列_用于插入数据("群主英语地址", 群主英语地址)
            End If
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("设备类型", 设备类型)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝发送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_保存要发送的讯宝2(ByVal 时间 As Long, ByVal 发送者英语地址 As String, ByVal 发送者编号 As Long,
                                   ByVal 发送者位置号 As Short, ByVal 发送序号 As Long, ByVal 群接收者() As Byte,
                                   ByVal 接收者主机名 As String, ByVal 群主英语地址 As String, ByVal 群编号 As Byte,
                                   ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String, ByVal 宽度 As Short,
                                   ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 设备类型 As 设备类型_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("时间", 0)
                列添加器.添加列_用于插入数据("接收者", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("时间", 时间)
            列添加器.添加列_用于插入数据("发送者英语地址", 发送者英语地址)
            列添加器.添加列_用于插入数据("发送者编号", 发送者编号)
            列添加器.添加列_用于插入数据("发送者位置号", 发送者位置号)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            列添加器.添加列_用于插入数据("接收者主机名", 接收者主机名)
            列添加器.添加列_用于插入数据("接收者位置号", 0)
            列添加器.添加列_用于插入数据("群接收者", 群接收者)
            列添加器.添加列_用于插入数据("群主英语地址", 群主英语地址)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("设备类型", 设备类型)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "讯宝发送", 列添加器)
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Sub 分配讯宝发送任务()
        Do
            Try
                Dim I As Integer
                If 要发送的讯宝数量 > 0 Then
                    For I = 0 To 要发送的讯宝数量 - 1
                        If 要发送的讯宝(I).当前状态 = 类_要发送的讯宝.状态_常量集合.结束 Then
                            Exit For
                        End If
                    Next
                    If I < 要发送的讯宝数量 Then
                        If 跨进程锁.WaitOne = True Then
                            Try
                                Dim 结果 As 类_SS包生成器
                                Dim 讯宝 As 类_要发送的讯宝
                                For I = 0 To 要发送的讯宝数量 - 1
                                    讯宝 = 要发送的讯宝(I)
                                    If 讯宝.当前状态 = 类_要发送的讯宝.状态_常量集合.结束 Then
                                        If 讯宝.群编号 = 0 Then
                                            If 讯宝.发送失败的原因 <> 讯宝指令_常量集合.无 Then
                                                Dim 某一用户 As 类_用户 = 用户目录(讯宝.发送者位置号)
                                                If 某一用户.用户编号 = 讯宝.发送者编号 Then
                                                    Select Case 讯宝.发送失败的原因
                                                        Case 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友
                                                            Dim 群编号 As Byte = 0
                                                            Try
                                                                Dim SS包解读器 As New 类_SS包解读器()
                                                                SS包解读器.解读纯文本(讯宝.文本)
                                                                SS包解读器.读取_有标签("I", 群编号, 0)
                                                            Catch ex As Exception
                                                            End Try
                                                            结果 = 数据库_存为推送的讯宝(讯宝.接收者(0).讯宝地址, 0, 讯宝.序号, 讯宝.发送者英语地址, 群编号, 讯宝.发送者编号, 讯宝.发送者位置号, 讯宝.设备类型, 讯宝.发送失败的原因)
                                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                        Case 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友
                                                            结果 = 数据库_存为推送的讯宝(讯宝.接收者(0).讯宝地址, 0, 讯宝.序号, 讯宝.发送者英语地址, 0, 讯宝.发送者编号, 讯宝.发送者位置号, 讯宝.设备类型, 讯宝.发送失败的原因, 讯宝.文本)
                                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                        Case Else
                                                            Dim 讯宝文本 As String
                                                            If 讯宝.发送失败的原因 = 讯宝指令_常量集合.退出小聊天群 Then
                                                                讯宝文本 = 生成文本_某人离开聊天群的通知(讯宝.发送者英语地址)
                                                            Else
                                                                讯宝文本 = Nothing
                                                            End If
                                                            结果 = 数据库_存为推送的讯宝(讯宝.接收者(0).讯宝地址, 0, 讯宝.序号, Nothing, 0, 讯宝.发送者编号, 讯宝.发送者位置号, 讯宝.设备类型, 讯宝.发送失败的原因, 讯宝文本)
                                                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                            Select Case 讯宝.讯宝指令
                                                                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                                                    Dim 路径 As String = 数据存放路径 & "SS\" & 讯宝.发送者编号 & "\" & 讯宝.文本
                                                                    If File.Exists(路径) Then
                                                                        Try
                                                                            File.Delete(路径)
                                                                        Catch ex As Exception
                                                                        End Try
                                                                    End If
                                                                    路径 &= ".jpg"
                                                                    If File.Exists(路径) Then
                                                                        Try
                                                                            File.Delete(路径)
                                                                        Catch ex As Exception
                                                                        End Try
                                                                    End If
                                                                Case 讯宝指令_常量集合.发送语音
                                                                    Dim 路径 As String = 数据存放路径 & "SS\" & 讯宝.发送者编号 & "\" & 讯宝.文本
                                                                    If File.Exists(路径) Then
                                                                        Try
                                                                            File.Delete(路径)
                                                                        Catch ex As Exception
                                                                        End Try
                                                                    End If
                                                                Case 讯宝指令_常量集合.发送文件
                                                                    Dim SS包解读器2 As New 类_SS包解读器
                                                                    SS包解读器2.解读纯文本(讯宝文本)
                                                                    Dim 存储文件名 As String = ""
                                                                    SS包解读器2.读取_有标签("S", 存储文件名)
                                                                    Dim 路径 As String = 数据存放路径 & "SS\" & 讯宝.发送者编号 & "\" & 存储文件名
                                                                    If File.Exists(路径) Then
                                                                        Try
                                                                            File.Delete(路径)
                                                                        Catch ex As Exception
                                                                        End Try
                                                                    End If
                                                            End Select
                                                    End Select
                                                End If
                                            Else
跳转点1:
                                                Select Case 讯宝.讯宝指令
                                                    Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                                        结果 = 数据库_记录带文件的讯宝(讯宝.发送者编号, 讯宝.讯宝指令, 讯宝.文本)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                    Case 讯宝指令_常量集合.发送文件
                                                        Dim SS包解读器2 As New 类_SS包解读器
                                                        SS包解读器2.解读纯文本(讯宝.文本)
                                                        Dim 存储文件名 As String = ""
                                                        SS包解读器2.读取_有标签("S", 存储文件名)
                                                        结果 = 数据库_记录带文件的讯宝(讯宝.发送者编号, 讯宝.讯宝指令, 存储文件名)
                                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                                End Select
                                            End If
                                        ElseIf 讯宝.发送者编号 > 0 Then
                                            If 讯宝.发送失败的原因 <> 讯宝指令_常量集合.无 Then
                                                If 讯宝.讯宝指令 = 讯宝指令_常量集合.退出小聊天群 Then
                                                    结果 = 数据库_存为推送的讯宝(讯宝.群主英语地址, 0, 讯宝.序号, 讯宝.群主英语地址, 讯宝.群编号, 讯宝.发送者编号, 讯宝.发送者位置号, 设备类型_常量集合.全部, 讯宝.讯宝指令, 生成文本_某人离开聊天群的通知(讯宝.发送者英语地址))
                                                Else
                                                    结果 = 数据库_存为推送的讯宝(讯宝.群主英语地址, 0, 讯宝.序号, 讯宝.群主英语地址, 讯宝.群编号, 讯宝.发送者编号, 讯宝.发送者位置号, 讯宝.设备类型, 讯宝指令_常量集合.不是群成员)
                                                End If
                                            End If
                                            GoTo 跳转点1
                                        End If
                                        结果 = 数据库_删除发送成功或失败的讯宝(讯宝)
                                        If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                                    End If
                                Next
                            Catch ex As Exception
                                Return
                            Finally
                                跨进程锁.ReleaseMutex()
                            End Try
                        Else
                            Thread.Sleep(1000)
                            Continue Do
                        End If
                        Dim 要发送的讯宝2(要发送的讯宝数量 - 1) As 类_要发送的讯宝
                        Dim 要发送的讯宝数量2 As Integer = 0
                        For I = 0 To 要发送的讯宝数量 - 1
                            If 要发送的讯宝(I).当前状态 <> 类_要发送的讯宝.状态_常量集合.结束 Then
                                要发送的讯宝2(要发送的讯宝数量2) = 要发送的讯宝(I)
                                要发送的讯宝数量2 += 1
                            End If
                        Next
                        要发送的讯宝 = 要发送的讯宝2
                        要发送的讯宝数量 = 要发送的讯宝数量2
                    End If
                End If
                If 要发送的讯宝数量 < 读取的讯宝最大数量 Then
                    Dim 新讯宝(读取的讯宝最大数量 - 要发送的讯宝数量 - 1) As 类_要发送的讯宝
                    Dim 新讯宝数量 As Integer = 0
                    If 跨进程锁.WaitOne = True Then
                        Try
                            Dim 结果 As 类_SS包生成器 = 数据库_获取要发送的新讯宝(新讯宝, 新讯宝数量)
                            If 结果.查询结果 <> 查询结果_常量集合.成功 Then Throw New Exception
                        Catch ex As Exception
                            Return
                        Finally
                            跨进程锁.ReleaseMutex()
                        End Try
                    Else
                        Continue Do
                    End If
                    If 新讯宝数量 > 0 Then
                        Dim 要发送的讯宝2(读取的讯宝最大数量 - 1) As 类_要发送的讯宝
                        If 要发送的讯宝数量 > 0 Then
                            Array.Copy(要发送的讯宝, 0, 要发送的讯宝2, 0, 要发送的讯宝数量)
                        End If
                        Array.Copy(新讯宝, 0, 要发送的讯宝2, 要发送的讯宝数量, 新讯宝数量)
                        要发送的讯宝 = 要发送的讯宝2
                        要发送的讯宝数量 = 要发送的讯宝数量 + 新讯宝数量
                    End If
                End If
                If 要发送的讯宝数量 > 0 Then
                    Dim J As Integer
                    For I = 0 To 要发送的讯宝数量 - 1
                        With 要发送的讯宝(I)
                            If .当前状态 = 类_要发送的讯宝.状态_常量集合.等待 Then
                                Dim 段() As String = .接收者(0).讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                                Dim 域名_目标服务器 As String = 获取服务器域名(.接收者主机名 & "." & 段(1))
                                If 讯宝域数量 > 0 Then
                                    For J = 0 To 讯宝域数量 - 1
                                        If String.Compare(讯宝域发送器(J).子域名_目标服务器, 域名_目标服务器) = 0 Then
                                            Exit For
                                        End If
                                    Next
                                    If J < 讯宝域数量 Then
                                        讯宝域发送器(J).发送(要发送的讯宝(I))
                                    Else
                                        If 讯宝域数量 = 讯宝域发送器.Length Then ReDim Preserve 讯宝域发送器(讯宝域数量 + 9)
                                        讯宝域发送器(讯宝域数量) = New 类_讯宝域发送器(域名_目标服务器, Me)
                                        讯宝域发送器(讯宝域数量).发送(要发送的讯宝(I))
                                        讯宝域数量 += 1
                                    End If
                                    If J > 0 Then
                                        Dim 某一讯宝域发送器 As 类_讯宝域发送器 = 讯宝域发送器(J)
                                        For J = J To 1 Step -1
                                            讯宝域发送器(J) = 讯宝域发送器(J - 1)
                                        Next
                                        讯宝域发送器(0) = 某一讯宝域发送器
                                    End If
                                Else
                                    ReDim 讯宝域发送器(9)
                                    讯宝域发送器(0) = New 类_讯宝域发送器(域名_目标服务器, Me)
                                    讯宝域发送器(0).发送(要发送的讯宝(I))
                                    讯宝域数量 = 1
                                End If
                            End If
                        End With
                    Next
                End If
                Thread.Sleep(1000)
            Catch ex As Exception
                Thread.Sleep(2000)
            End Try
        Loop Until 关闭
    End Sub

    Private Function 数据库_获取要发送的新讯宝(ByRef 要发送的新讯宝() As 类_要发送的讯宝, ByRef 新讯宝数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "讯宝发送", Nothing, 要发送的新讯宝.Length, , CByte(要发送的新讯宝.Length), "#时间")
            Dim 文本 As String = ""
            Dim 时间, 发送者编号, 序号 As Long
            Dim 群接收者() As Byte
            读取器 = 指令.执行()
            While 读取器.读取
                时间 = 读取器(0)   '按照表列的顺序
                发送者编号 = 读取器(2)
                序号 = 读取器(4)
                If 要发送的讯宝是否重复(时间, 发送者编号, 序号) = True Then Continue While
                要发送的新讯宝(新讯宝数量) = New 类_要发送的讯宝
                With 要发送的新讯宝(新讯宝数量)
                    .时间 = 时间
                    .发送者英语地址 = 读取器(1)
                    .发送者编号 = 发送者编号
                    .发送者位置号 = 读取器(3)
                    .序号 = 序号
                    群接收者 = 读取器(8)
                    If 群接收者 Is Nothing Then
                        ReDim .接收者(0)
                        With .接收者(0)
                            .讯宝地址 = 读取器(5)
                            .位置号 = 读取器(7)
                        End With
                    Else
                        Dim SS包解读器 As New 类_SS包解读器(群接收者)
                        Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("群成员")
                        ReDim .接收者(SS包解读器2.Length - 1)
                        Dim SS包解读器3 As 类_SS包解读器
                        Dim I As Integer
                        For I = 0 To SS包解读器2.Length - 1
                            SS包解读器3 = SS包解读器2(I)
                            With .接收者(I)
                                SS包解读器3.读取_有标签("讯宝地址", .讯宝地址)
                                SS包解读器3.读取_有标签("位置号", .位置号)
                            End With
                        Next
                    End If
                    .接收者主机名 = 读取器(6)
                    .群主英语地址 = 读取器(9)
                    .群编号 = 读取器(10)
                    .讯宝指令 = 读取器(11)
                    .文本库号 = 读取器(12)
                    .文本编号 = 读取器(13)
                    .宽度 = 读取器(14)
                    .高度 = 读取器(15)
                    .秒数 = 读取器(16)
                    .设备类型 = 读取器(17)
                End With
                新讯宝数量 += 1
                If 新讯宝数量 = 要发送的新讯宝.Length Then Exit While
            End While
            读取器.关闭()
            If 新讯宝数量 > 0 Then
                Dim 列添加器 As 类_列添加器
                Dim 筛选器 As 类_筛选器
                Dim I As Integer
                For I = 0 To 新讯宝数量 - 1
                    With 要发送的新讯宝(I)
                        If .文本库号 > 0 Then
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于获取数据("文本")
                            指令 = New 类_数据库指令_请求获取数据(副数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                            读取器 = 指令.执行()
                            While 读取器.读取
                                .文本 = 读取器(0)
                                Exit While
                            End While
                            读取器.关闭()
                        End If
                    End With
                Next
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 要发送的讯宝是否重复(ByVal 时间 As Long, ByVal 发送者编号 As Long, ByVal 序号 As Long) As Boolean
        Dim J As Integer
        For J = 要发送的讯宝数量 - 1 To 0 Step -1
            With 要发送的讯宝(J)
                If .时间 = 时间 Then
                    If .发送者编号 = 发送者编号 Then
                        If .序号 = 序号 Then
                            Return True
                        End If
                    End If
                End If
            End With
        Next
        Return False
    End Function

    Private Function 数据库_删除发送成功或失败的讯宝(ByVal 讯宝 As 类_要发送的讯宝) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("发送者英语地址", 筛选方式_常量集合.等于, 讯宝.发送者英语地址)
            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 讯宝.序号)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.等于, 讯宝.时间)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "讯宝发送", 筛选器, "#发送者发送序号时间")
            指令.执行()
            If 讯宝.文本库号 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 讯宝.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令 = New 类_数据库指令_删除数据(副数据库, 讯宝.文本库号 & "库", 筛选器, 主键索引名)
                指令.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_查找某个域的讯友(ByVal 用户编号 As Long, ByVal 英语域名 As String, ByRef 本国语域名 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.包含, 讯宝地址标识 & 英语域名, 字符串包含方式_常量集合.结尾有)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("本国语讯宝地址")
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器, , "#用户英语讯宝地址")
            Dim 找到了 As Boolean
            Dim 本国语讯宝地址 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                找到了 = True
                本国语讯宝地址 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                Dim 段() As String = 本国语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                本国语域名 = 段(1)
            End If
            If 找到了 = True Then
                Return New 类_SS包生成器(查询结果_常量集合.成功)
            Else
                Return New 类_SS包生成器(查询结果_常量集合.失败)
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_删除讯友(ByVal 用户编号 As Long, ByVal 用户位置号 As Short, ByVal 英语讯宝地址 As String) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_删除 As New 类_数据库指令_删除数据(主数据库, "讯友录", 筛选器, "#用户英语讯宝地址", True)
            If 指令_删除.执行() > 0 Then
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("变动", 讯友录变动_常量集合.删除)
                列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令_更新 As New 类_数据库指令_更新数据(主数据库, "讯友录变动", 列添加器_新数据, 筛选器, "#用户地址")
                If 指令_更新.执行 = 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("用户编号", 用户编号)
                    列添加器.添加列_用于插入数据("位置号", 用户位置号)
                    列添加器.添加列_用于插入数据("变动", 讯友录变动_常量集合.删除)
                    列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                    列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                    Dim 指令_插入 As New 类_数据库指令_插入新数据(主数据库, "讯友录变动", 列添加器)
                    指令_插入.执行()
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取讯友标签(ByVal 用户编号 As Long, ByVal 英语讯宝地址 As String, ByRef 标签一 As String, ByRef 标签二 As String) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"标签一", "标签二"})
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器, 1, 列添加器,  , "#用户英语讯宝地址")
            读取器 = 指令.执行()
            While 读取器.读取
                标签一 = 读取器(0)
                标签二 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取某标签讯友数量(ByVal 用户编号 As Long, ByVal 标签名称 As String, ByRef 数量 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("标签一", 筛选方式_常量集合.等于, 标签名称)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器,  ,  , 最大值_常量集合.每个标签讯友数量, "#用户标签一")
            读取器 = 指令.执行()
            While 读取器.读取
                数量 += 1
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("标签二", 筛选方式_常量集合.等于, 标签名称)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_请求获取数据(主数据库, "讯友录", 筛选器,  ,  , 最大值_常量集合.每个标签讯友数量, "#用户标签二")
            读取器 = 指令.执行()
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

    Private Function 数据库_修改讯友标签(ByVal 用户编号 As Long, ByVal 用户位置号 As Short, ByVal 英语讯宝地址 As String, ByVal 标签一 As String,
                                ByVal 标签二 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签一", 标签一)
            列添加器_新数据.添加列_用于插入数据("标签二", 标签二)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_更新 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户英语讯宝地址", True)
            If 指令_更新.执行() > 0 Then
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("变动", 讯友录变动_常量集合.修改拉黑)
                列添加器_新数据.添加列_用于插入数据("原标签名", 标签一)
                列添加器_新数据.添加列_用于插入数据("新标签名", 标签二)
                列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                指令_更新 = New 类_数据库指令_更新数据(主数据库, "讯友录变动", 列添加器_新数据, 筛选器, "#用户地址")
                If 指令_更新.执行() = 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("用户编号", 用户编号)
                    列添加器.添加列_用于插入数据("位置号", 用户位置号)
                    列添加器.添加列_用于插入数据("变动", 讯友录变动_常量集合.修改拉黑)
                    列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                    列添加器.添加列_用于插入数据("原标签名", 标签一)
                    列添加器.添加列_用于插入数据("新标签名", 标签二)
                    列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                    Dim 指令_插入 As New 类_数据库指令_插入新数据(主数据库, "讯友录变动", 列添加器)
                    指令_插入.执行()
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_讯友标签重命名(ByVal 用户编号 As Long, ByVal 用户位置号 As Short, ByVal 旧标签名称 As String, ByVal 新标签名称 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签一", 新标签名称)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("标签一", 筛选方式_常量集合.等于, 旧标签名称)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签一")
            指令.执行()
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签二", 新标签名称)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("标签二", 筛选方式_常量集合.等于, 旧标签名称)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签二")
            指令.执行()
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("标签二", Nothing)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("标签二", 筛选方式_常量集合.等于, New 类_列_表成员("标签一"))
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户标签二")
            指令.执行()
            列添加器_新数据 = New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("新标签名", 新标签名称)
            列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("新标签名", 筛选方式_常量集合.等于, 旧标签名称)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令_更新 As New 类_数据库指令_更新数据(主数据库, "讯友录变动", 列添加器_新数据, 筛选器, "#用户时间")
            If 指令_更新.执行 = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 用户编号)
                列添加器.添加列_用于插入数据("位置号", 用户位置号)
                列添加器.添加列_用于插入数据("变动", 讯友录变动_常量集合.重命名标签)
                列添加器.添加列_用于插入数据("原标签名", 旧标签名称)
                列添加器.添加列_用于插入数据("新标签名", 新标签名称)
                列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                Dim 指令_插入 As New 类_数据库指令_插入新数据(主数据库, "讯友录变动", 列添加器)
                指令_插入.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_修改讯友备注(ByVal 用户编号 As Long, ByVal 英语讯宝地址 As String, ByVal 备注 As String) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("备注", 备注)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            If String.IsNullOrEmpty(备注) = False Then
                列添加器.添加列_用于筛选器("备注", 筛选方式_常量集合.不等于, 备注)
            Else
                列添加器.添加列_用于筛选器("备注", 筛选方式_常量集合.不为空, Nothing)
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户英语讯宝地址")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Friend Function 数据库_更新讯友录更新时间(ByVal 用户编号 As Long, ByRef 更新时间 As Long) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            更新时间 = Date.UtcNow.Ticks
            列添加器_新数据.添加列_用于插入数据("更新时间", 更新时间)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "讯友录版本", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行() = 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("用户编号", 用户编号)
                列添加器.添加列_用于插入数据("更新时间", 更新时间)
                Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, "讯友录版本", 列添加器)
                指令2.执行()
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_拉黑讯友或取消(ByVal 用户编号 As Long, ByVal 用户位置号 As Short, ByVal 英语讯宝地址 As String, ByVal 拉黑 As Boolean) As 类_SS包生成器
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("拉黑", 拉黑)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("拉黑", 筛选方式_常量集合.不等于, 拉黑)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(主数据库, "讯友录", 列添加器_新数据, 筛选器, "#用户英语讯宝地址", True)
            If 指令.执行() > 0 Then
                列添加器_新数据 = New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("变动", 讯友录变动_常量集合.修改拉黑)
                列添加器_新数据.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
                列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令_更新 As New 类_数据库指令_更新数据(主数据库, "讯友录变动", 列添加器_新数据, 筛选器, "#用户地址")
                If 指令_更新.执行 = 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("用户编号", 用户编号)
                    列添加器.添加列_用于插入数据("位置号", 用户位置号)
                    列添加器.添加列_用于插入数据("变动", 讯友录变动_常量集合.修改拉黑)
                    列添加器.添加列_用于插入数据("英语讯宝地址", 英语讯宝地址)
                    列添加器.添加列_用于插入数据("时间", Date.UtcNow.Ticks)
                    Dim 指令_插入 As New 类_数据库指令_插入新数据(主数据库, "讯友录变动", 列添加器)
                    指令_插入.执行()
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_获取讯友录变动次数(ByVal 用户编号 As Long, ByRef 次数 As Integer) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("变动", 筛选方式_常量集合.不等于, 讯友录变动_常量集合.添加)
            列添加器.添加列_用于筛选器("时间", 筛选方式_常量集合.大于等于, Date.UtcNow.AddMinutes(-1).Ticks)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(主数据库, "讯友录变动", 筛选器, , , 100, "#用户时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                次数 += 1
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_添加黑白域(ByVal 用户编号 As Long, ByVal 英语域名 As String, ByVal 本国语域名 As String,
                               ByVal 讯宝指令 As 讯宝指令_常量集合) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 黑域_全部)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "黑域", 筛选器, 1, , , "#用户编号英语域名")
            Dim 所有域 As Boolean
            读取器 = 指令.执行()
            While 读取器.读取
                所有域 = True
                Exit While
            End While
            读取器.关闭()
            If 讯宝指令 = 讯宝指令_常量集合.添加黑域 Then
                If 所有域 = True Then
                    If String.Compare(英语域名, 黑域_全部) = 0 Then
                        Return New 类_SS包生成器(查询结果_常量集合.成功)
                    Else
                        Return New 类_SS包生成器(查询结果_常量集合.失败)
                    End If
                End If
            Else
                If 所有域 = False Then
                    Return New 类_SS包生成器(查询结果_常量集合.失败)
                End If
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("用户编号", 用户编号)
            列添加器.添加列_用于插入数据("英语域名", 英语域名)
            If String.IsNullOrEmpty(本国语域名) = False Then
                列添加器.添加列_用于插入数据("本国语域名", 本国语域名)
            End If
            Dim 指令2 As New 类_数据库指令_插入新数据(主数据库, IIf(讯宝指令 = 讯宝指令_常量集合.添加黑域, "黑域", "白域"), 列添加器)
            指令2.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As 类_值已存在
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_移除黑白域(ByVal 用户编号 As Long, ByVal 英语域名 As String, ByVal 讯宝指令 As 讯宝指令_常量集合) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 英语域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, IIf(讯宝指令 = 讯宝指令_常量集合.移除黑域, "黑域", "白域"), 筛选器, "#用户编号英语域名")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_是否白域(ByVal 用户编号 As Long, ByVal 域名 As String, ByRef 是白域 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "白域", 筛选器, 1, , , "#用户编号英语域名")
            读取器 = 指令.执行()
            While 读取器.读取
                是白域 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_是否黑域(ByVal 用户编号 As Long, ByVal 域名 As String, ByRef 是黑域 As Boolean) As 类_SS包生成器
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 筛选器 As New 类_筛选器
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 域名)
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 黑域_全部)
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "黑域", 筛选器, 1, , , "#用户编号英语域名")
            读取器 = 指令.执行()
            While 读取器.读取
                是黑域 = True
                Exit While
            End While
            读取器.关闭()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

    Private Function 数据库_退出大聊天群(ByVal 用户编号 As Long, ByVal 英语域名 As String, ByVal 主机名 As String, ByVal 群编号 As Long) As 类_SS包生成器
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("用户编号", 筛选方式_常量集合.等于, 用户编号)
            列添加器.添加列_用于筛选器("英语域名", 筛选方式_常量集合.等于, 英语域名)
            列添加器.添加列_用于筛选器("主机名", 筛选方式_常量集合.等于, 主机名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(主数据库, "加入的大聊天群", 筛选器, "#用户域名主机名群")
            指令.执行()
            Return New 类_SS包生成器(查询结果_常量集合.成功)
        Catch ex As Exception
            Return New 类_SS包生成器(ex.Message)
        End Try
    End Function

End Class
