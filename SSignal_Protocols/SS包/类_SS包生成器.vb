Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Security.Cryptography
Imports System.IO

Public Class 类_SS包生成器

    Private Structure 字节数据_复合数据
        Dim 字节数组() As Byte
        Dim 长度信息字节数 As 长度信息字节数_常量集合
    End Structure

    Friend SS包数据() As SS包数据_复合数据
    Friend SS包数据数量 As Integer
    Dim 无标签2 As Boolean
    Dim SS包编码 As SS包编码_常量集合
    Dim 编码 As Encoding
    Protected 查询结果1 As 查询结果_常量集合 = 查询结果_常量集合.无

    Public Sub New(Optional ByVal 无标签1 As Boolean = False, Optional ByVal 数据数量1 As Integer = 0, Optional ByVal SS包编码1 As SS包编码_常量集合 = SS包编码_常量集合.Unicode_LittleEndian)
        查询结果1 = 查询结果_常量集合.无
        Call 初始化(无标签1, 数据数量1, SS包编码1)
    End Sub

    Public Sub New(ByVal 查询结果2 As 查询结果_常量集合, Optional ByVal 数据数量1 As Integer = 0, Optional ByVal SS包编码1 As SS包编码_常量集合 = SS包编码_常量集合.Unicode_LittleEndian)
        查询结果1 = 查询结果2
        Call 初始化(False, 数据数量1, SS包编码1)
        Call 添加_有标签(冒号_SS包保留标签, 查询结果1)
    End Sub

    Public Sub New(ByVal 错误信息 As String, Optional ByVal SS包编码1 As SS包编码_常量集合 = SS包编码_常量集合.Unicode_LittleEndian)
        查询结果1 = 查询结果_常量集合.出错
        Call 初始化(False, 2, SS包编码1)
        Call 添加_有标签(冒号_SS包保留标签, 查询结果1)
        Call 添加_有标签(感叹号_SS包保留标签, 错误信息)
    End Sub

    Protected Sub 初始化(ByVal 无标签1 As Boolean, ByVal 数据数量1 As Integer, ByVal SS包编码1 As SS包编码_常量集合)
        无标签2 = 无标签1
        If 数据数量1 < 1 Then 数据数量1 = 10
        ReDim SS包数据(数据数量1 - 1)
        SS包编码 = SS包编码1
        Select Case SS包编码
            Case SS包编码_常量集合.Unicode_LittleEndian : 编码 = Unicode
            Case SS包编码_常量集合.Unicode_BigEndian : 编码 = BigEndianUnicode
            Case SS包编码_常量集合.UTF8 : 编码 = UTF8
            Case SS包编码_常量集合.ASCII : 编码 = ASCII
            Case SS包编码_常量集合.UTF32 : 编码 = UTF32
            Case SS包编码_常量集合.UTF7 : 编码 = UTF7
            Case Else : SS包编码 = SS包编码_常量集合.UTF8 : 编码 = UTF8
        End Select
    End Sub

    Public ReadOnly Property 查询结果 As 查询结果_常量集合
        Get
            If 查询结果1 = 查询结果_常量集合.无 Then
                Throw New Exception("此SS包没有查询结果。")
            Else
                Return 查询结果1
            End If
        End Get
    End Property

    Public ReadOnly Property 出错提示文本 As String
        Get
            If 查询结果1 = 查询结果_常量集合.出错 Then
                Dim I As Integer
                For I = 0 To SS包数据数量 - 1
                    If String.Compare(SS包数据(I).标签, 感叹号_SS包保留标签) = 0 Then
                        Return SS包数据(I).数据
                    End If
                Next
            End If
            Return ""
        End Get
    End Property

    Public ReadOnly Property 数据量 As Integer
        Get
            Return SS包数据数量
        End Get
    End Property

    Public ReadOnly Property 无标签 As Boolean
        Get
            Return 无标签2
        End Get
    End Property

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 真假值 As Boolean)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.真假值, 真假值, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 真假值 As Boolean)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.真假值, 真假值)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 字节 As Byte)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.字节, 字节, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 字节 As Byte)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.字节, 字节)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 有符号短整数 As Short)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号短整数, 有符号短整数, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 有符号短整数 As Short)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号短整数, 有符号短整数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 有符号整数 As Integer)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号整数, 有符号整数, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 有符号整数 As Integer)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号整数, 有符号整数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 有符号长整数 As Long)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号长整数, 有符号长整数, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 有符号长整数 As Long)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.有符号长整数, 有符号长整数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 单精度浮点数 As Single)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.单精度浮点数, 单精度浮点数, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 单精度浮点数 As Single)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.单精度浮点数, 单精度浮点数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 双精度浮点数 As Double)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.双精度浮点数, 双精度浮点数, 标签)
    End Sub

    Public Sub 添加_无标签(ByVal 双精度浮点数 As Double)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        Call 添加(SS包数据类型_常量集合.双精度浮点数, 双精度浮点数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 字符串 As String)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.字符串, 字符串, 标签, 长度信息字节数_常量集合.四字节)
    End Sub

    Public Sub 添加_无标签(ByVal 字符串 As String, Optional ByVal 长度信息字节数 As 长度信息字节数_常量集合 = 长度信息字节数_常量集合.两字节)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        If 长度信息字节数 = 长度信息字节数_常量集合.零字节 AndAlso String.IsNullOrEmpty(字符串) Then Throw New Exception("长度信息字节数不能为零")
        Call 添加(SS包数据类型_常量集合.字符串, 字符串, , 长度信息字节数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal 字节数组() As Byte)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.字节数组, 字节数组, 标签, 长度信息字节数_常量集合.四字节)
    End Sub

    Public Sub 添加_无标签(ByVal 字节数组() As Byte, Optional ByVal 长度信息字节数 As 长度信息字节数_常量集合 = 长度信息字节数_常量集合.零字节)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        If 长度信息字节数 = 长度信息字节数_常量集合.零字节 AndAlso 字节数组 Is Nothing Then Throw New Exception("长度信息字节数不能为零")
        Call 添加(SS包数据类型_常量集合.字节数组, 字节数组, , 长度信息字节数)
    End Sub

    Public Sub 添加_有标签(ByVal 标签 As String, ByVal SS包生成器 As 类_SS包生成器)
        If 无标签2 = True Then Throw New Exception("此为无标签SS包")
        Call 添加(SS包数据类型_常量集合.子SS包, SS包生成器, 标签, 长度信息字节数_常量集合.四字节)
    End Sub

    Public Sub 添加_无数据(ByVal 长度信息字节数 As 长度信息字节数_常量集合)
        If 无标签2 = False Then Throw New Exception("此为有标签SS包")
        If 长度信息字节数 = 长度信息字节数_常量集合.零字节 Then Throw New Exception("长度信息字节数不能为零")
        Call 添加(SS包数据类型_常量集合.无, Nothing, , 长度信息字节数)
    End Sub

    Private Sub 添加(ByVal 数据类型 As SS包数据类型_常量集合, ByVal 数据 As Object,
                   Optional ByVal 标签 As String = Nothing, Optional ByVal 长度信息字节数 As 长度信息字节数_常量集合 = 长度信息字节数_常量集合.零字节)
        If SS包数据数量 = SS包数据.Length Then ReDim Preserve SS包数据(SS包数据数量 * 2 - 1)
        With SS包数据(SS包数据数量)
            .类型 = 数据类型
            .数据 = 数据
            .长度信息字节数 = 长度信息字节数
            .标签 = 标签
        End With
        SS包数据数量 += 1
    End Sub

    Public Function 发送SS包(ByVal 网络连接器 As Socket, Optional ByVal AES加密器 As ICryptoTransform = Nothing) As Boolean
        Dim 字节数组() As Byte = 生成SS包(AES加密器)
        If 字节数组 IsNot Nothing Then
            Dim SS包生成器 As New 类_SS包生成器(True)
            SS包生成器.添加_无标签(CByte(AscW(SS包高低位标识_L)))
            SS包生成器.添加_无标签(字节数组, 长度信息字节数_常量集合.四字节)
            字节数组 = SS包生成器.生成字节数组
            If 网络连接器.Send(字节数组) = 字节数组.Length Then Return True
        End If
        Return False
    End Function

    Public Function 生成SS包(Optional ByVal AES加密器 As ICryptoTransform = Nothing) As Byte()
        If SS包数据数量 = 0 Then Return Nothing
        Dim 字节数据数组() As 字节数据_复合数据 = 转换()
        Dim I, 数据区总长度 As Integer
        For I = 0 To 字节数据数组.Length - 1
            With 字节数据数组(I)
                Select Case .长度信息字节数
                    Case 长度信息字节数_常量集合.零字节
                        数据区总长度 += .字节数组.Length
                    Case 长度信息字节数_常量集合.两字节, 长度信息字节数_常量集合.四字节
                        数据区总长度 += .长度信息字节数 + .字节数组.Length
                    Case Else : Return Nothing
                End Select
            End With
        Next
        Dim 字节数组(SS包标识_有标签.Length + 5 + 数据区总长度) As Byte
        Dim 字节数组1() As Byte
        If 无标签2 = False Then
            字节数组1 = ASCII.GetBytes(SS包标识_有标签)
        Else
            字节数组1 = ASCII.GetBytes(SS包标识_无标签)
        End If
        Dim J As Integer
        Array.Copy(字节数组1, 0, 字节数组, J, 字节数组1.Length)
        J += 字节数组1.Length
        字节数组(J) = CByte(AscW(SS包高低位标识_L))
        J += 1
        字节数组(J) = SS包编码
        J += 1
        If 无标签2 = False Then
            字节数组1 = BitConverter.GetBytes(CInt(字节数据数组.Length / 3))
        Else
            字节数组1 = BitConverter.GetBytes(数据区总长度)
        End If
        Array.Copy(字节数组1, 0, 字节数组, J, 字节数组1.Length)
        J += 字节数组1.Length
        For I = 0 To 字节数据数组.Length - 1
            With 字节数据数组(I)
                If .长度信息字节数 <> 长度信息字节数_常量集合.零字节 Then
                    Select Case .长度信息字节数
                        Case 长度信息字节数_常量集合.两字节 : 字节数组1 = BitConverter.GetBytes(CShort(.字节数组.Length))
                        Case 长度信息字节数_常量集合.四字节 : 字节数组1 = BitConverter.GetBytes(.字节数组.Length)
                        Case Else : Continue For
                    End Select
                    Array.Copy(字节数组1, 0, 字节数组, J, 字节数组1.Length)
                    J += 字节数组1.Length
                End If
                Array.Copy(.字节数组, 0, 字节数组, J, .字节数组.Length)
                J += .字节数组.Length
            End With
        Next
        If AES加密器 Is Nothing Then
            Return 字节数组
        Else
            Return AES加密(字节数组, AES加密器)
        End If
    End Function

    Public Function 生成字节数组(Optional ByVal 指定长度 As Integer = 0) As Byte()
        If 查询结果1 <> 查询结果_常量集合.无 Then
            Throw New Exception("作为查询结果的SS包，不能生成字节数组。请生成SS包。")
        End If
        If SS包数据数量 = 0 Then Return Nothing
        Dim 字节数据数组() As 字节数据_复合数据 = 转换()
        Dim I, 总长度 As Integer
        For I = 0 To 字节数据数组.Length - 1
            With 字节数据数组(I)
                Select Case .长度信息字节数
                    Case 长度信息字节数_常量集合.零字节
                        总长度 += .字节数组.Length
                    Case 长度信息字节数_常量集合.两字节, 长度信息字节数_常量集合.四字节
                        总长度 += .长度信息字节数 + .字节数组.Length
                    Case Else : Return Nothing
                End Select
            End With
        Next
        If 指定长度 > 0 Then If 总长度 < 指定长度 Then 总长度 = 指定长度
        Dim 字节数组(总长度 - 1) As Byte
        Dim 字节数组1() As Byte
        Dim J As Integer
        For I = 0 To 字节数据数组.Length - 1
            If 字节数据数组(I).长度信息字节数 <> 长度信息字节数_常量集合.零字节 Then
                Select Case 字节数据数组(I).长度信息字节数
                    Case 长度信息字节数_常量集合.两字节 : 字节数组1 = BitConverter.GetBytes(CShort(字节数据数组(I).字节数组.Length))
                    Case 长度信息字节数_常量集合.四字节 : 字节数组1 = BitConverter.GetBytes(字节数据数组(I).字节数组.Length)
                    Case Else : Continue For
                End Select
                Array.Copy(字节数组1, 0, 字节数组, J, 字节数组1.Length)
                J += 字节数组1.Length
            End If
            Array.Copy(字节数据数组(I).字节数组, 0, 字节数组, J, 字节数据数组(I).字节数组.Length)
            J += 字节数据数组(I).字节数组.Length
        Next
        Return 字节数组
    End Function

    Private Function 转换() As 字节数据_复合数据()
        Dim 数据() As 字节数据_复合数据
        Dim I As Integer
        If 无标签2 = False Then
            ReDim 数据(SS包数据数量 * 3 - 1)
            Dim J As Integer
            For I = 0 To SS包数据数量 - 1
                With SS包数据(I)
                    数据(J) = 转换成字节数据(.标签, SS包数据类型_常量集合.字符串, 长度信息字节数_常量集合.两字节)
                    J += 1
                    数据(J) = 转换成字节数据(.类型, SS包数据类型_常量集合.字节, 长度信息字节数_常量集合.零字节)
                    J += 1
                    数据(J) = 转换成字节数据(.数据, .类型, .长度信息字节数)
                    J += 1
                End With
            Next
        Else
            ReDim 数据(SS包数据数量 - 1)
            For I = 0 To SS包数据数量 - 1
                With SS包数据(I)
                    数据(I) = 转换成字节数据(.数据, .类型, .长度信息字节数)
                End With
            Next
        End If
        Return 数据
    End Function

    Private Function 转换成字节数据(ByVal 待转换数据 As Object, ByVal 数据类型 As SS包数据类型_常量集合,
                             ByVal 长度信息字节数 As 长度信息字节数_常量集合) As 字节数据_复合数据
        Dim 数据 As New 字节数据_复合数据
        数据.长度信息字节数 = 长度信息字节数
        Select Case 数据类型
            Case SS包数据类型_常量集合.字符串
                If String.IsNullOrEmpty(待转换数据) = False Then
                    数据.字节数组 = 编码.GetBytes(CStr(待转换数据))
                Else
                    数据.字节数组 = New Byte(长度信息字节数 - 1) {}
                    数据.长度信息字节数 = 长度信息字节数_常量集合.零字节
                End If
            Case SS包数据类型_常量集合.有符号长整数
                数据.字节数组 = BitConverter.GetBytes(CLng(待转换数据))
            Case SS包数据类型_常量集合.有符号整数
                数据.字节数组 = BitConverter.GetBytes(CInt(待转换数据))
            Case SS包数据类型_常量集合.有符号短整数
                数据.字节数组 = BitConverter.GetBytes(CShort(待转换数据))
            Case SS包数据类型_常量集合.子SS包
                If 待转换数据 IsNot Nothing Then
                    数据.字节数组 = CType(待转换数据, 类_SS包生成器).生成SS包
                    If 数据.字节数组 Is Nothing Then GoTo 跳转点1
                Else
跳转点1:
                    数据.字节数组 = New Byte(长度信息字节数 - 1) {}
                    数据.长度信息字节数 = 长度信息字节数_常量集合.零字节
                End If
            Case SS包数据类型_常量集合.字节数组
                If 待转换数据 IsNot Nothing Then
                    数据.字节数组 = 待转换数据
                Else
                    数据.字节数组 = New Byte(长度信息字节数 - 1) {}
                    数据.长度信息字节数 = 长度信息字节数_常量集合.零字节
                End If
            Case SS包数据类型_常量集合.真假值
                If CBool(待转换数据) = False Then
                    数据.字节数组 = New Byte() {0}
                Else
                    数据.字节数组 = New Byte() {1}
                End If
            Case SS包数据类型_常量集合.字节
                数据.字节数组 = New Byte() {待转换数据}
            Case SS包数据类型_常量集合.单精度浮点数
                数据.字节数组 = BitConverter.GetBytes(CSng(待转换数据))
            Case SS包数据类型_常量集合.双精度浮点数
                数据.字节数组 = BitConverter.GetBytes(CDbl(待转换数据))
            Case SS包数据类型_常量集合.无
                数据.字节数组 = New Byte(长度信息字节数 - 1) {}
                数据.长度信息字节数 = 长度信息字节数_常量集合.零字节
        End Select
        Return 数据
    End Function

    Public Sub 能否生成纯文本()
        If 无标签2 = True Then Throw New Exception("无标签SS包不能生成纯文本")
        If SS包数据数量 > 0 Then
            Dim I As Integer
            For I = 0 To SS包数据数量 - 1
                With SS包数据(I)
                    If 标签是否合格(.标签) = False Then
                        Throw New Exception("标签名称有非法字符。")
                    End If
                    If .类型 = SS包数据类型_常量集合.子SS包 Then
                        CType(.数据, 类_SS包生成器).能否生成纯文本()
                    End If
                End With
            Next
        End If
    End Sub

    Public Function 生成纯文本(Optional ByVal 层级 As Integer = 0) As String
        Call 能否生成纯文本()
        If SS包数据数量 > 0 Then
            Dim 变长文本 As New StringBuilder()
            Dim 文本写入器 As New StringWriter(变长文本)
            Dim 空格字符串 As String = Nothing
            If 层级 > 0 Then
                空格字符串 = Space(层级)
            Else
                If 层级 < 0 Then 层级 = 0
                文本写入器.Write(SS包标识_纯文本)
            End If
            Dim I As Integer
            For I = 0 To SS包数据数量 - 1
                With SS包数据(I)
                    文本写入器.Write(vbLf)
                    If 层级 > 0 Then 文本写入器.Write(空格字符串)
                    Select Case .类型
                        Case SS包数据类型_常量集合.字符串
                            文本写入器.Write("S")
                        Case SS包数据类型_常量集合.有符号长整数
                            文本写入器.Write("8")
                        Case SS包数据类型_常量集合.有符号整数
                            文本写入器.Write("4")
                        Case SS包数据类型_常量集合.有符号短整数
                            文本写入器.Write("2")
                        Case SS包数据类型_常量集合.子SS包
                            文本写入器.Write("SS")
                        Case SS包数据类型_常量集合.字节数组
                            文本写入器.Write("BT")
                        Case SS包数据类型_常量集合.真假值
                            文本写入器.Write("BL")
                        Case SS包数据类型_常量集合.字节
                            文本写入器.Write("1")
                        Case SS包数据类型_常量集合.单精度浮点数
                            文本写入器.Write("4F")
                        Case SS包数据类型_常量集合.双精度浮点数
                            文本写入器.Write("8F")
                    End Select
                    文本写入器.Write(":")
                    文本写入器.Write(.标签)
                    文本写入器.Write("=")
                    Select Case .类型
                        Case SS包数据类型_常量集合.字符串
                            If String.IsNullOrEmpty(.数据) = False Then
                                文本写入器.Write(CStr(.数据).Replace("&", "&amp;").Replace(vbCr, "").Replace(vbLf, "&;"))
                            End If
                        Case SS包数据类型_常量集合.有符号长整数
                            文本写入器.Write(CLng(.数据))
                        Case SS包数据类型_常量集合.有符号整数
                            文本写入器.Write(CInt(.数据))
                        Case SS包数据类型_常量集合.有符号短整数
                            文本写入器.Write(CShort(.数据))
                        Case SS包数据类型_常量集合.子SS包
                            文本写入器.Write(CType(.数据, 类_SS包生成器).生成纯文本(层级 + 1))
                        Case SS包数据类型_常量集合.字节数组
                            If .数据 IsNot Nothing Then
                                文本写入器.Write(System.Convert.ToBase64String(.数据, Base64FormattingOptions.None))
                            End If
                        Case SS包数据类型_常量集合.真假值
                            文本写入器.Write(IIf(CBool(.数据), "true", "false"))
                        Case SS包数据类型_常量集合.字节
                            文本写入器.Write(CByte(.数据))
                        Case SS包数据类型_常量集合.单精度浮点数
                            文本写入器.Write(CSng(.数据))
                        Case SS包数据类型_常量集合.双精度浮点数
                            文本写入器.Write(CDbl(.数据))
                    End Select
                End With
            Next
            文本写入器.Close()
            Return 文本写入器.ToString
        Else
            Return ""
        End If
    End Function

    Private Function 标签是否合格(ByVal 标签 As String) As Boolean
        If 标签.StartsWith(" ") Then Return False
        Dim 非法字符() As String = New String() {"=", vbCr, vbLf}
        Dim I As Integer
        For I = 0 To 非法字符.Length - 1
            If 标签.Contains(非法字符(I)) Then Return False
        Next
        Return True
    End Function

End Class
