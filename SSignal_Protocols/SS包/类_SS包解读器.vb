Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Security.Cryptography

Public Class 类_SS包解读器

    Protected 要解读的SS包() As Byte
    Dim 有标签数据() As SS包数据_复合数据
    Dim 数据数量, 已读取字节数, 起始索引 As Integer
    Protected 无标签 As Boolean
    Dim 需反转 As Boolean
    Dim 编码 As Encoding

    Public Sub New()

    End Sub

    Public Sub New(ByVal 网络连接器 As Socket, Optional ByVal AES解密器 As ICryptoTransform = Nothing, Optional ByVal 最大长度 As Integer = 0)
        Dim 字节数组() As Byte = 接收指定长度的数据(网络连接器, 5)
        If 字节数组 Is Nothing Then
            Throw New 异常_接收SS包失败("接收SS包失败。")
        End If
        Dim 长度 As Integer
        Select Case ChrW(字节数组(0))
            Case SS包高低位标识_L
                长度 = BitConverter.ToInt32(字节数组, 1)
            Case SS包高低位标识_B
                长度 = BitConverter.ToInt32(反转(字节数组, 1, 4), 0)
            Case Else
                Throw New Exception("无法判断是高位在前还是低位在前。")
        End Select
        If 长度 < SS包标识_有标签.Length Then
            Throw New Exception("未接收到SS包。")
        End If
        If 最大长度 > 0 Then
            If 长度 > 最大长度 Then
                Throw New Exception("数据长度大于指定的最大长度。")
            End If
        End If
        字节数组 = 接收指定长度的数据(网络连接器, 长度)
        If 字节数组 Is Nothing Then
            Throw New 异常_接收SS包失败("接收SS包失败。")
        End If
        解读(字节数组, AES解密器)
    End Sub

    Public Sub New(ByVal 要解读的SS包1() As Byte, Optional ByVal AES解密器 As ICryptoTransform = Nothing)
        解读(要解读的SS包1, AES解密器)
    End Sub

    Public Sub New(ByVal SS包生成器_有标签 As 类_SS包生成器)
        If SS包生成器_有标签.无标签 = True Then Throw New Exception("这是无标签SS包生成器。")
        有标签数据 = SS包生成器_有标签.SS包数据
        数据数量 = SS包生成器_有标签.SS包数据数量
    End Sub

    Private Sub 解读(ByVal 要解读的SS包1() As Byte, ByVal AES解密器 As ICryptoTransform)
        If AES解密器 Is Nothing Then
            要解读的SS包 = 要解读的SS包1
        Else
            要解读的SS包 = AES解密(要解读的SS包1, AES解密器)
        End If
        Dim SS包编码 As SS包编码_常量集合
        Select Case ASCII.GetString(要解读的SS包, 0, SS包标识_有标签.Length)
            Case SS包标识_无标签
                已读取字节数 = SS包标识_有标签.Length
                Select Case ChrW(要解读的SS包(已读取字节数))
                    Case SS包高低位标识_L
                    Case SS包高低位标识_B
                        需反转 = True
                    Case Else
                        Throw New Exception("无法判断是高位在前还是低位在前。")
                End Select
                已读取字节数 += 1
                SS包编码 = 要解读的SS包(已读取字节数)
                已读取字节数 += 1
                Dim 总长度 As Integer
                If Not 需反转 Then
                    总长度 = BitConverter.ToInt32(要解读的SS包, 已读取字节数)
                Else
                    总长度 = BitConverter.ToInt32(反转(要解读的SS包, 已读取字节数, 4), 0)
                End If
                If 总长度 <= 0 Then
                    Throw New Exception("数据损坏。")
                End If
                已读取字节数 += 4
                If 已读取字节数 + 总长度 <> 要解读的SS包.Length Then
                    Throw New Exception("数据损坏。")
                End If
                选择编码(SS包编码)
                无标签 = True
            Case SS包标识_有标签
                已读取字节数 = SS包标识_有标签.Length
                Select Case ChrW(要解读的SS包(已读取字节数))
                    Case SS包高低位标识_L
                    Case SS包高低位标识_B
                        需反转 = True
                    Case Else
                        Throw New Exception("无法判断是高位在前还是低位在前。")
                End Select
                已读取字节数 += 1
                SS包编码 = 要解读的SS包(已读取字节数)
                已读取字节数 += 1
                If Not 需反转 Then
                    数据数量 = BitConverter.ToInt32(要解读的SS包, 已读取字节数)
                Else
                    数据数量 = BitConverter.ToInt32(反转(要解读的SS包, 已读取字节数, 4), 0)
                End If
                If 数据数量 <= 0 Then
                    Throw New Exception("数据损坏。")
                End If
                已读取字节数 += 4
                选择编码(SS包编码)
                无标签 = True
                ReDim 有标签数据(数据数量 - 1)
                Dim I As Integer
                While 未读数据长度 > 0
                    With 有标签数据(I)
                        Call 读取(.标签)
                        Call 读取字节(.类型)
                        Select Case .类型
                            Case SS包数据类型_常量集合.字符串
                                .长度信息字节数 = 长度信息字节数_常量集合.四字节
                                Dim 字符串 As String = Nothing
                                Call 读取(字符串, .长度信息字节数)
                                .数据 = 字符串
                            Case SS包数据类型_常量集合.有符号长整数
                                Dim 有符号长整数 As Long
                                Call 读取(有符号长整数)
                                .数据 = 有符号长整数
                            Case SS包数据类型_常量集合.有符号整数
                                Dim 有符号整数 As Integer
                                Call 读取(有符号整数)
                                .数据 = 有符号整数
                            Case SS包数据类型_常量集合.有符号短整数
                                Dim 有符号短整数 As Short
                                Call 读取(有符号短整数)
                                .数据 = 有符号短整数
                            Case SS包数据类型_常量集合.真假值
                                Dim 真假值 As Boolean
                                Call 读取(真假值)
                                .数据 = 真假值
                            Case SS包数据类型_常量集合.字节
                                Dim 字节 As Byte
                                Call 读取(字节)
                                .数据 = 字节
                            Case SS包数据类型_常量集合.子SS包
                                .长度信息字节数 = 长度信息字节数_常量集合.四字节
                                Dim 字节数组() As Byte = Nothing
                                Call 读取(字节数组, .长度信息字节数)
                                If 字节数组 IsNot Nothing Then
                                    .数据 = New 类_SS包解读器(字节数组)
                                End If
                            Case SS包数据类型_常量集合.字节数组
                                .长度信息字节数 = 长度信息字节数_常量集合.四字节
                                Dim 字节数组() As Byte = Nothing
                                Call 读取(字节数组, .长度信息字节数)
                                .数据 = 字节数组
                            Case SS包数据类型_常量集合.单精度浮点数
                                Dim 单精度浮点数 As Single
                                Call 读取(单精度浮点数)
                                .数据 = 单精度浮点数
                            Case SS包数据类型_常量集合.双精度浮点数
                                Dim 双精度浮点数 As Double
                                Call 读取(双精度浮点数)
                                .数据 = 双精度浮点数
                        End Select
                    End With
                    I += 1
                    If I >= 数据数量 Then Exit While
                End While
                无标签 = False
                If 数据数量 <> I Then
                    数据数量 = I
                    Throw New Exception("数据损坏。")
                End If
            Case Else
                Throw New Exception("这不是SS包。")
        End Select
    End Sub

    Private Sub 选择编码(ByVal SS包编码 As SS包编码_常量集合)
        Select Case SS包编码
            Case SS包编码_常量集合.Unicode_LittleEndian : 编码 = Unicode
            Case SS包编码_常量集合.Unicode_BigEndian : 编码 = BigEndianUnicode
            Case SS包编码_常量集合.UTF8 : 编码 = UTF8
            Case SS包编码_常量集合.ASCII : 编码 = ASCII
            Case SS包编码_常量集合.UTF32 : 编码 = UTF32
            Case SS包编码_常量集合.UTF7 : 编码 = UTF7
            Case Else : 编码 = UTF8
        End Select
    End Sub

    Public Sub 解读纯文本(ByVal 文本 As String)
        If String.IsNullOrEmpty(文本) Then Return
        Dim 行() As String = 文本.Split(New String() {vbLf}, StringSplitOptions.RemoveEmptyEntries)
        If 行.Length > 1 Then
            If String.Compare(行(0), SS包标识_纯文本) <> 0 Then
                Throw New Exception("找不到SS包标识：" & SS包标识_纯文本)
            End If
            解读纯文本(行, 1)
        End If
    End Sub

    Public Function 解读纯文本(ByVal 行() As String, ByVal 起始行 As Integer, Optional ByVal 层级 As Integer = 0) As Integer
        ReDim 有标签数据(19)
        Dim 某一行, 标签 As String
        Dim 类型 As SS包数据类型_常量集合
        Dim 数据 As Object
        Dim 空格字符串 As String = Nothing
        If 层级 > 0 Then
            空格字符串 = Space(层级)
        ElseIf 层级 < 0 Then
            层级 = 0
        End If
        Dim I, J, K As Integer
        For I = 起始行 To 行.Length - 1
跳转就2:
            某一行 = 行(I)
            If 层级 > 0 Then
                If 某一行.StartsWith(空格字符串) = False Then Return I
                某一行 = 某一行.Substring(空格字符串.Length)
            End If
            If 某一行.StartsWith(" ") Then GoTo 跳转点1
            K = 某一行.IndexOf(":", 1)
            If K < 0 Then GoTo 跳转点1
            Select Case 某一行.Substring(0, K)
                Case "S" : 类型 = SS包数据类型_常量集合.字符串
                Case "8" : 类型 = SS包数据类型_常量集合.有符号长整数
                Case "4" : 类型 = SS包数据类型_常量集合.有符号整数
                Case "2" : 类型 = SS包数据类型_常量集合.有符号短整数
                Case "SS" : 类型 = SS包数据类型_常量集合.子SS包
                Case "BT" : 类型 = SS包数据类型_常量集合.字节数组
                Case "BL" : 类型 = SS包数据类型_常量集合.真假值
                Case "1" : 类型 = SS包数据类型_常量集合.字节
                Case "4F" : 类型 = SS包数据类型_常量集合.单精度浮点数
                Case "8F" : 类型 = SS包数据类型_常量集合.双精度浮点数
                Case Else : GoTo 跳转点1
            End Select
            J = 某一行.IndexOf("=", K + 2)
            If J < 0 Then GoTo 跳转点1
            K += 1
            标签 = 某一行.Substring(K, J - K)
            If 类型 <> SS包数据类型_常量集合.子SS包 Then
                If 类型 <> SS包数据类型_常量集合.字符串 Then
                    If 类型 <> SS包数据类型_常量集合.字节数组 Then
                        If J = 某一行.Length - 1 Then GoTo 跳转点1
                        Select Case 类型
                            Case SS包数据类型_常量集合.有符号长整数
                                Dim 有符号长整数 As Long
                                If Long.TryParse(某一行.Substring(J + 1), 有符号长整数) = False Then GoTo 跳转点1
                                数据 = 有符号长整数
                            Case SS包数据类型_常量集合.有符号整数
                                Dim 有符号整数 As Integer
                                If Integer.TryParse(某一行.Substring(J + 1), 有符号整数) = False Then GoTo 跳转点1
                                数据 = 有符号整数
                            Case SS包数据类型_常量集合.有符号短整数
                                Dim 有符号短整数 As Short
                                If Short.TryParse(某一行.Substring(J + 1), 有符号短整数) = False Then GoTo 跳转点1
                                数据 = 有符号短整数
                            Case SS包数据类型_常量集合.真假值
                                If String.Compare(某一行.Substring(J + 1), "true", True) = 0 Then
                                    数据 = True
                                Else
                                    数据 = False
                                End If
                            Case SS包数据类型_常量集合.字节
                                Dim 字节 As Byte
                                If Byte.TryParse(某一行.Substring(J + 1), 字节) = False Then GoTo 跳转点1
                                数据 = 字节
                            Case SS包数据类型_常量集合.单精度浮点数
                                Dim 单精度浮点数 As Single
                                If Single.TryParse(某一行.Substring(J + 1), 单精度浮点数) = False Then GoTo 跳转点1
                                数据 = 单精度浮点数
                            Case SS包数据类型_常量集合.双精度浮点数
                                Dim 双精度浮点数 As Double
                                If Double.TryParse(某一行.Substring(J + 1), 双精度浮点数) = False Then GoTo 跳转点1
                                数据 = 双精度浮点数
                            Case Else
                                GoTo 跳转点1
                        End Select
                    Else
                        If J < 某一行.Length - 1 Then
                            数据 = System.Convert.FromBase64String(某一行.Substring(J + 1))
                        Else
                            数据 = Nothing
                        End If
                    End If
                Else
                    If J < 某一行.Length - 1 Then
                        数据 = 某一行.Substring(J + 1).Replace("&;", vbCrLf).Replace("&amp;", "&")
                    Else
                        数据 = ""
                    End If
                End If
                添加有标签数据(标签, 类型, 数据)
            Else
                Dim SS包解读器 As New 类_SS包解读器
                J = SS包解读器.解读纯文本(行, I + 1, 层级 + 1)
                添加有标签数据(标签, 类型, SS包解读器)
                I = J
                If J < 行.Length Then
                    GoTo 跳转就2
                Else
                    Exit For
                End If
            End If
        Next
        无标签 = False
        Return I
跳转点1：
        Throw New Exception("数据有错误，在第" & I + 1 & "行")
    End Function

    Private Sub 添加有标签数据(ByVal 标签 As String, ByVal 类型 As SS包数据类型_常量集合, ByVal 数据 As Object)
        If 有标签数据.Length = 数据数量 Then ReDim Preserve 有标签数据(数据数量 * 2 - 1)
        With 有标签数据(数据数量)
            .标签 = 标签
            .类型 = 类型
            .数据 = 数据
        End With
        数据数量 += 1
    End Sub

    Public Function 读取_指定长度(ByVal 长度 As Integer) As Byte()
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        If 长度 < 1 Then
            Return Nothing
        Else
            If 已读取字节数 + 长度 > 要解读的SS包.Length Then
                Throw New Exception("数据长度不足")
            Else
                Dim 字节数组(长度 - 1) As Byte
                Array.Copy(要解读的SS包, 已读取字节数, 字节数组, 0, 长度)
                已读取字节数 += 长度
                Return 字节数组
            End If
        End If
    End Function

    Public Function 读取_根据长度信息(ByVal 长度信息字节数 As 长度信息字节数_常量集合) As Byte()
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        If 长度信息字节数 < 长度信息字节数_常量集合.两字节 Then
            Throw New Exception("长度信息字节数不合法")
        Else
            If 已读取字节数 + 长度信息字节数 > 要解读的SS包.Length Then
                Throw New Exception("数据长度不足")
            Else
                Select Case 长度信息字节数
                    Case 长度信息字节数_常量集合.两字节
                        Dim 长度 As Integer
                        If Not 需反转 Then
                            长度 = BitConverter.ToInt16(要解读的SS包, 已读取字节数)
                        Else
                            长度 = BitConverter.ToInt16(反转(要解读的SS包, 已读取字节数, 2), 0)
                        End If
                        已读取字节数 += 长度信息字节数
                        Return 读取_指定长度(长度)
                    Case 长度信息字节数_常量集合.四字节
                        Dim 长度 As Integer
                        If Not 需反转 Then
                            长度 = BitConverter.ToInt32(要解读的SS包, 已读取字节数)
                        Else
                            长度 = BitConverter.ToInt32(反转(要解读的SS包, 已读取字节数, 4), 0)
                        End If
                        已读取字节数 += 长度信息字节数
                        Return 读取_指定长度(长度)
                    Case Else
                        Throw New Exception("长度信息字节数不合法")
                End Select
            End If
        End If
    End Function

    Public Sub 读取(ByRef 赋值对象 As Boolean)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(1)
        If 字节数组(0) = 0 Then
            赋值对象 = False
        Else
            赋值对象 = True
        End If
    End Sub

    Public Sub 读取字节(ByRef 赋值对象 As Object)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(1)
        赋值对象 = 字节数组(0)
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Byte)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(1)
        赋值对象 = 字节数组(0)
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Short)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(2)
        If Not 需反转 Then
            赋值对象 = BitConverter.ToInt16(字节数组, 0)
        Else
            赋值对象 = BitConverter.ToInt16(反转(字节数组), 0)
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Integer)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(4)
        If Not 需反转 Then
            赋值对象 = BitConverter.ToInt32(字节数组, 0)
        Else
            赋值对象 = BitConverter.ToInt32(反转(字节数组), 0)
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Long)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(8)
        If Not 需反转 Then
            赋值对象 = BitConverter.ToInt64(字节数组, 0)
        Else
            赋值对象 = BitConverter.ToInt64(反转(字节数组), 0)
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Single)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(4)
        If Not 需反转 Then
            赋值对象 = BitConverter.ToSingle(字节数组, 0)
        Else
            赋值对象 = BitConverter.ToSingle(反转(字节数组), 0)
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象 As Double)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_指定长度(8)
        If Not 需反转 Then
            赋值对象 = BitConverter.ToDouble(字节数组, 0)
        Else
            赋值对象 = BitConverter.ToDouble(反转(字节数组), 0)
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象 As String, Optional ByVal 长度信息字节数 As 长度信息字节数_常量集合 = 长度信息字节数_常量集合.两字节, Optional ByVal 不赋值 As Boolean = False)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        Dim 字节数组() As Byte = 读取_根据长度信息(长度信息字节数)
        If 不赋值 = False Then
            If 字节数组 IsNot Nothing Then
                赋值对象 = 编码.GetString(字节数组)
            Else
                赋值对象 = Nothing
            End If
        End If
    End Sub

    Public Sub 读取(ByRef 赋值对象() As Byte, ByVal 长度信息字节数 As 长度信息字节数_常量集合, Optional ByVal AES解密器 As ICryptoTransform = Nothing)
        If 无标签 = False Then Throw New Exception("此为有标签SS包")
        If AES解密器 Is Nothing Then
            赋值对象 = 读取_根据长度信息(长度信息字节数)
        Else
            赋值对象 = AES解密(读取_根据长度信息(长度信息字节数), AES解密器)
        End If
    End Sub

    Public Sub 读取_有标签(ByVal 标签 As String, ByRef 赋值对象 As Object)
        If 无标签 = True Then Throw New Exception("此为无标签SS包")
        读取_有标签2(标签, 赋值对象)
    End Sub

    Public Sub 读取_有标签(ByVal 标签 As String, ByRef 赋值对象 As Object, ByVal 默认值 As Object)
        If 无标签 = True Then Throw New Exception("此为无标签SS包")
        If 读取_有标签2(标签, 赋值对象) = False Then
            赋值对象 = 默认值
        End If
    End Sub

    Private Function 读取_有标签2(ByVal 标签 As String, ByRef 赋值对象 As Object) As Boolean
        If 数据数量 > 0 Then
            Dim I As Integer
            For I = 起始索引 To 数据数量 - 1
                If String.Compare(有标签数据(I).标签, 标签) = 0 Then
                    赋值对象 = 有标签数据(I).数据
                    起始索引 = I + 1
                    Return True
                End If
            Next
            If 起始索引 > 0 Then
                For I = 0 To 起始索引 - 1
                    If String.Compare(有标签数据(I).标签, 标签) = 0 Then
                        赋值对象 = 有标签数据(I).数据
                        起始索引 = I + 1
                        Return True
                    End If
                Next
            End If
        End If
        Return False
    End Function

    Public Function 读取_重复标签(ByVal 标签 As String) As Object()
        If 无标签 = True Then Throw New Exception("此为无标签SS包")
        If 数据数量 > 0 Then
            Dim 索引(数据数量 - 1) As Integer
            Dim I, J As Integer
            For I = 0 To 数据数量 - 1
                If String.Compare(有标签数据(I).标签, 标签) = 0 Then
                    索引(J) = I
                    J += 1
                End If
            Next
            If J > 0 Then
                Dim 对象(J - 1) As Object
                For I = 0 To J - 1
                    对象(I) = 有标签数据(索引(I)).数据
                Next
                Return 对象
            End If
        End If
        Return Nothing
    End Function

    Public ReadOnly Property 有标签数据数量() As Integer
        Get
            If 无标签 = False Then
                Return 数据数量
            Else
                Throw New Exception("此为无标签SS包")
            End If
        End Get
    End Property

    Public ReadOnly Property 未读数据长度() As Integer
        Get
            If 要解读的SS包 IsNot Nothing Then
                Return 要解读的SS包.Length - 已读取字节数
            Else
                Return 0
            End If
        End Get
    End Property

    Public ReadOnly Property 已读数据长度() As Integer
        Get
            Return 已读取字节数
        End Get
    End Property

    Public ReadOnly Property 查询结果 As 查询结果_常量集合
        Get
            If 无标签 = False Then
                Dim 查询结果2 As 查询结果_常量集合
                读取_有标签(冒号_SS包保留标签, 查询结果2, 查询结果_常量集合.无)
                Return 查询结果2
            Else
                Throw New Exception("此为无标签SS包")
            End If
        End Get
    End Property

    Public ReadOnly Property 出错提示文本 As String
        Get
            If 无标签 = False Then
                Dim 错误信息2 As String = ""
                读取_有标签(感叹号_SS包保留标签, 错误信息2)
                Return 错误信息2
            Else
                Throw New Exception("此为无标签SS包")
            End If
        End Get
    End Property

End Class
