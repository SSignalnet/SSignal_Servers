Imports System.Net.Sockets
Imports System.IO
Imports System.Security.Cryptography

Public Module 模块_SS包

    Public Const SS包标识_无标签 As String = "SSNT"
    Public Const SS包标识_有标签 As String = "SSTG"
    Public Const SS包标识_纯文本 As String = "SSTX"
    Public Const SS包高低位标识_L As String = "L"  'Little
    Public Const SS包高低位标识_B As String = "B"  'Big
    Public Const 问号_SS包保留标签 As String = "?"   '查询
    Public Const 冒号_SS包保留标签 As String = ":"    '结果
    Public Const 感叹号_SS包保留标签 As String = "!"   '出错
    Public Const 井号_SS包保留标签 As String = "#"
    Public Const 星号_SS包保留标签 As String = "*"

    Public Enum 长度信息字节数_常量集合 As Byte
        零字节 = 0
        两字节 = 2
        四字节 = 4
    End Enum

    Public Enum SS包数据类型_常量集合 As Byte
        无 = 0
        真假值 = 1
        字节 = 2
        有符号短整数 = 3
        有符号整数 = 4
        有符号长整数 = 5
        单精度浮点数 = 6
        双精度浮点数 = 7
        字符串 = 8
        字节数组 = 9
        子SS包 = 10
    End Enum

    Public Enum SS包编码_常量集合 As Byte
        ASCII = 1
        UTF7 = 7
        UTF8 = 8
        Unicode_LittleEndian = 16
        Unicode_BigEndian = 17
        UTF32 = 32
    End Enum

    Public Structure SS包数据_复合数据
        Dim 类型 As SS包数据类型_常量集合
        Dim 数据 As Object
        Dim 长度信息字节数 As 长度信息字节数_常量集合
        Dim 标签 As String
    End Structure

    Public Function 接收指定长度的数据(ByVal 网络连接器 As Socket, ByVal 数据长度 As Integer) As Byte()
        If 数据长度 < 1 Then Return Nothing
        Dim 字节数组(数据长度 - 1) As Byte
        Dim 收到的数据的长度, 已收数据总长度 As Integer
接收剩余数据:
        收到的数据的长度 = 网络连接器.Receive(字节数组, 已收数据总长度, 数据长度, SocketFlags.None)
        If 收到的数据的长度 > 0 Then
            数据长度 -= 收到的数据的长度
            If 数据长度 > 0 Then
                已收数据总长度 += 收到的数据的长度
                GoTo 接收剩余数据
            Else
                Return 字节数组
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Function 反转(ByVal 字节数组() As Byte, Optional ByVal 开始位置 As Integer = 0, Optional ByVal 长度 As Integer = 0) As Byte()
        Dim 字节数组2() As Byte
        If 长度 > 0 Then
            ReDim 字节数组2(长度 - 1)
            Dim I, J As Integer
            For I = 开始位置 + 长度 - 1 To 开始位置 Step -1
                字节数组2(J) = 字节数组(I)
                J += 1
            Next
        Else
            ReDim 字节数组2(字节数组.Length - 开始位置 - 1)
            Dim I, J As Integer
            For I = 字节数组.Length - 1 To 开始位置 Step -1
                字节数组2(J) = 字节数组(I)
                J += 1
            Next
        End If
        Return 字节数组2
    End Function

    Public Function AES加密(ByVal 字节数组() As Byte, ByVal AES加密器 As ICryptoTransform) As Byte()
        Dim 加密转换流 As CryptoStream = Nothing
        Dim 内存流 As MemoryStream = Nothing
        Try
            内存流 = New MemoryStream
            加密转换流 = New CryptoStream(内存流, AES加密器, CryptoStreamMode.Write)
            加密转换流.Write(字节数组, 0, 字节数组.Length)
            加密转换流.FlushFinalBlock()
            字节数组 = 内存流.ToArray
            加密转换流.Close()
            内存流.Close()
        Catch ex As Exception
            If 加密转换流 IsNot Nothing Then
                Try
                    加密转换流.Close()
                Catch ex1 As Exception
                End Try
            End If
            If 内存流 IsNot Nothing Then 内存流.Close()
        End Try
        Return 字节数组
    End Function

    Public Function AES解密(ByVal 字节数组() As Byte, ByVal AES解密器 As ICryptoTransform) As Byte()
        Dim 加密转换流 As CryptoStream = Nothing
        Dim 内存流 As MemoryStream = Nothing
        Try
            内存流 = New MemoryStream(字节数组)
            加密转换流 = New CryptoStream(内存流, AES解密器, CryptoStreamMode.Read)
            ReDim 字节数组(内存流.Length - 1)
            Dim 长度 As Integer = 加密转换流.Read(字节数组, 0, 字节数组.Length)
            加密转换流.Close()
            内存流.Close()
            If 长度 < 字节数组.Length Then ReDim Preserve 字节数组(长度 - 1)
        Catch ex As Exception
            If 加密转换流 IsNot Nothing Then
                Try
                    加密转换流.Close()
                Catch ex1 As Exception
                End Try
            End If
            If 内存流 IsNot Nothing Then 内存流.Close()
        End Try
        Return 字节数组
    End Function

End Module
