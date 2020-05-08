Imports System.Threading
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_ServerCommonCode

Public Class 类_启动器

#Region "定义和声明"

    Dim 凭据_中心服务器, 网络地址_中心服务器 As String
    Public 本服务器主机名 As String
    Friend 启动时间 As Long

    Dim 跨进程锁 As Mutex
    Dim 副数据库 As 类_数据库

    Public 连接凭据_管理员 As String

#End Region

    Public Sub New(ByVal 跨进程锁1 As Mutex, ByVal 副数据库1 As 类_数据库)
        跨进程锁 = 跨进程锁1
        副数据库 = 副数据库1
    End Sub

    Public Function 验证中心服务器(ByVal 网络地址 As String, ByVal 服务器凭据 As String) As Boolean
        If String.Compare(网络地址, 网络地址_中心服务器) <> 0 Then Return False
        If String.Compare(服务器凭据, 凭据_中心服务器) <> 0 Then Return False
        Return True
    End Function

    Public Sub 启动()
        启动时间 = Date.UtcNow.Ticks
        凭据_中心服务器 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器)
        Dim 线程 As New Thread(New ThreadStart(AddressOf 启动2))
        线程.Start()
    End Sub

    Private Sub 启动2()
        Dim 中心服务器子域名 As String = 获取服务器域名(讯宝中心服务器主机名 & "." & 域名_英语)
        Dim 访问结果 As Object = 访问其它服务器("https://" & 中心服务器子域名 & "/?C=ServerStart&Credential=" & 替换URI敏感字符(凭据_中心服务器) & "&Type=" & 服务器类别_常量集合.小宇宙中心服务器, , 30000)
        If TypeOf 访问结果 Is 类_SS包生成器 Then Return
        Try
            Dim SS包解读器 As New 类_SS包解读器(CType(访问结果, Byte()))
            If SS包解读器.查询结果 <> 查询结果_常量集合.成功 Then Return
            If 跨进程锁.WaitOne = True Then
                Try
                    Dim 网络地址 As New IPAddress(0)
                    If IPAddress.TryParse(网络地址_中心服务器, 网络地址) = False Then Return
                    Dim 网络地址字节数组() As Byte = 网络地址.GetAddressBytes
                    Dim 列添加器_新数据 As New 类_列添加器
                    列添加器_新数据.添加列_用于插入数据("网络地址", 网络地址字节数组)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_我访它", 凭据_中心服务器)
                    列添加器_新数据.添加列_用于插入数据("连接凭据_它访我", 凭据_中心服务器)
                    列添加器_新数据.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 中心服务器子域名)
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令 As New 类_数据库指令_更新数据(副数据库, "服务器", 列添加器_新数据, 筛选器, 主键索引名)
                    If 指令.执行() = 0 Then
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于插入数据("子域名", 中心服务器子域名)
                        列添加器.添加列_用于插入数据("网络地址", 网络地址字节数组)
                        列添加器.添加列_用于插入数据("连接凭据_我访它", 凭据_中心服务器)
                        列添加器.添加列_用于插入数据("连接凭据_它访我", 凭据_中心服务器)
                        列添加器.添加列_用于插入数据("更新时间_它访我", Date.UtcNow.Ticks)
                        Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "服务器", 列添加器)
                        指令2.执行()
                    End If
                Catch ex As Exception
                    Return
                Finally
                    跨进程锁.ReleaseMutex()
                End Try
            Else
                Return
            End If
            SS包解读器.读取_有标签("主机名", 本服务器主机名, Nothing)
        Catch ex As Exception
        End Try
    End Sub

    Public Function 验证启动(ByVal 网络地址 As String, ByVal 服务器凭据 As String) As Boolean
        If 启动时间 <= 0 Then Return False
        If String.Compare(服务器凭据, 凭据_中心服务器) <> 0 Then Return False
        If DateDiff(DateInterval.Second, Date.FromBinary(启动时间), Date.UtcNow) > 30 Then Return False
        网络地址_中心服务器 = 网络地址
        启动时间 = 0
        Return True
    End Function


    'Private Sub 清除已删除的()
    '    Dim 列添加器 As New 类_列添加器
    '    列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
    '    列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
    '    Dim 筛选器 As New 类_筛选器
    '    筛选器.添加一组筛选条件(列添加器)
    '    列添加器 = New 类_列添加器
    '    列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号"})
    '    Dim 指令 As New 类_数据库指令_请求获取数据(主数据库, "回复", 筛选器, , 列添加器, 100, "#流星语评论回复")
    '    Dim 文本位置(999) As 文本位置_复合数据
    '    Dim 文本位置数量 As Integer
    '    读取器 = 指令.执行()
    '    While 读取器.读取
    '        If 文本位置.Length = 文本位置数量 Then ReDim Preserve 文本位置(文本位置数量 * 2 - 1)
    '        With 文本位置(文本位置数量)
    '            .文本库号 = 读取器(0)
    '            .文本编号 = 读取器(1)
    '        End With
    '        文本位置数量 += 1
    '    End While
    '    读取器.关闭()
    '    If 文本位置数量 > 0 Then
    '        Dim I As Integer
    '        For I = 0 To 文本位置数量 - 1
    '            With 文本位置(I)
    '                列添加器 = New 类_列添加器
    '                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
    '                筛选器 = New 类_筛选器
    '                筛选器.添加一组筛选条件(列添加器)
    '                Dim 指令3 As New 类_数据库指令_删除数据(主数据库, .文本库号 & "库", 筛选器, 主键索引名, True)
    '                指令3.执行()
    '            End With
    '        Next
    '    End If
    '    列添加器 = New 类_列添加器
    '    列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
    '    列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
    '    筛选器 = New 类_筛选器
    '    筛选器.添加一组筛选条件(列添加器)
    '    Dim 指令2 As New 类_数据库指令_删除数据(主数据库, "回复", 筛选器, "#流星语评论回复")
    '    指令2.执行()
    '    列添加器 = New 类_列添加器
    '    列添加器.添加列_用于筛选器("流星语编号", 筛选方式_常量集合.等于, 流星语编号)
    '    列添加器.添加列_用于筛选器("评论编号", 筛选方式_常量集合.等于, 评论编号)
    '    筛选器 = New 类_筛选器
    '    筛选器.添加一组筛选条件(列添加器)
    '    列添加器 = New 类_列添加器
    '    列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号"})
    '    指令 = New 类_数据库指令_请求获取数据(主数据库, "评论", 筛选器, 1, 列添加器, , "#流星语评论")
    '    Dim 文本库号 As Short
    '    Dim 文本编号 As Long
    '    读取器 = 指令.执行()
    '    While 读取器.读取
    '        文本库号 = 读取器(0)
    '        文本编号 = 读取器(1)
    '        Exit While
    '    End While
    '    读取器.关闭()
    '    列添加器 = New 类_列添加器
    '    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
    '    筛选器 = New 类_筛选器
    '    筛选器.添加一组筛选条件(列添加器)
    '    指令2 = New 类_数据库指令_删除数据(主数据库, 文本库号 & "库", 筛选器, 主键索引名, True)
    '    指令2.执行()

    'End Sub

End Class
