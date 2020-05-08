Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.Encoding
Imports System.Drawing

Public Module 模块_协议方法

    Public Function 访问其它服务器(ByVal 路径 As String, Optional ByVal 要发送的字节数组() As Byte = Nothing, Optional ByVal 等待时长 As Integer = 0) As Object
        Dim 重试次数 As Integer
        Dim 收到的字节数组() As Byte = Nothing
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(路径)
            HTTP网络请求.Method = "POST"
            If 等待时长 < 10000 Then
                HTTP网络请求.Timeout = 10000
            Else
                HTTP网络请求.Timeout = 等待时长
            End If
            If 要发送的字节数组 Is Nothing Then
                HTTP网络请求.ContentType = "text/xml"
                HTTP网络请求.ContentLength = 0
            Else
                HTTP网络请求.ContentType = "application/octet-stream"
                HTTP网络请求.ContentLength = 要发送的字节数组.Length
                Dim 流 As Stream = HTTP网络请求.GetRequestStream
                流.Write(要发送的字节数组, 0, 要发送的字节数组.Length)
                流.Close()
            End If
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    ReDim 收到的字节数组(HTTP网络回应.ContentLength - 1)
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
继续:
                    收到的字节数 = 输入流.Read(收到的字节数组, 收到的总字节数, 收到的字节数组.Length - 收到的总字节数)
                    If 收到的字节数 > 0 Then
                        收到的总字节数 += 收到的字节数
                        If 收到的总字节数 < 收到的字节数组.Length Then
                            GoTo 继续
                        End If
                    End If
                End If
            End Using
            If 收到的字节数组 IsNot Nothing Then
                If 收到的总字节数 = 收到的字节数组.Length Then
                    Return 收到的字节数组
                Else
                    If 重试次数 < 1 Then
                        重试次数 += 1
                        GoTo 重试
                    End If
                End If
            End If
            Return New 类_SS包生成器(查询结果_常量集合.失败)
        Catch ex As Exception
            If 重试次数 < 1 Then
                重试次数 += 1
                GoTo 重试
            Else
                Return New 类_SS包生成器(ex.Message)
            End If
        End Try
    End Function

    Const 忽略的字符 As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.!~*'()"

    Public Function 替换URI敏感字符(ByVal URI字符串 As String) As String
        If String.IsNullOrEmpty(URI字符串) = False Then
            Dim 变长文本 As New StringBuilder(URI字符串.Length * 3)
            Dim 文本写入器 As New StringWriter(变长文本)
            Try
                Dim 字符数组() As Char = URI字符串.ToCharArray
                Dim I As Integer
                For I = 0 To 字符数组.Length - 1
                    If 忽略的字符.Contains(字符数组(I)) Then
                        文本写入器.Write(字符数组(I))
                    Else
                        文本写入器.Write(转换成16进制字符串(UTF8.GetBytes(字符数组(I))))
                    End If
                Next
                文本写入器.Close()
                Return 文本写入器.ToString
            Catch ex As Exception
            End Try
        End If
        Return URI字符串
    End Function

    Private Function 转换成16进制字符串(ByVal 字节数组 As Byte()) As String
        Dim 变长文本 As New StringBuilder(字节数组.Length * 3)
        Dim 文本写入器 As New StringWriter(变长文本)
        Dim I As Integer
        For I = 0 To 字节数组.Length - 1
            文本写入器.Write("%")
            If 字节数组(I) < 16 Then 文本写入器.Write("0")
            文本写入器.Write(System.Convert.ToString(字节数组(I), 16))
        Next
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Public Function 生成文件名_发送语音图片短视频时(ByVal 本次发送序号 As Long, ByVal 扩展名 As String) As String
        Return 生成大写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器) & 特征字符_下划线 & 本次发送序号 & "." & 扩展名
    End Function

    Public Function 生成文件名_发送文件时(ByVal 本次发送序号 As Long, ByVal 文件名 As String) As String
        Dim SS包生成器 As New 类_SS包生成器
        SS包生成器.添加_有标签("O", 文件名)     'Original FileName
        SS包生成器.添加_有标签("S", 生成大写英文字母与数字的随机字符串(长度_常量集合.连接凭据_服务器) & 特征字符_下划线 & 本次发送序号 & Path.GetExtension(文件名))    'Saved FileName
        Return SS包生成器.生成纯文本
    End Function

    Public Function 生成大写英文字母与数字的随机字符串(ByVal 长度 As Short) As String
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
    End Function

    Public Sub 生成预览图片(ByVal 原图路径 As String)
        Dim 原图 As Bitmap = Nothing
        Dim 预览图片 As Bitmap = Nothing
        Try
            原图 = New Bitmap(原图路径)
            If 原图.Width > 最大值_常量集合.讯宝预览图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.讯宝预览图片宽高_像素 Then
                Dim 缩小比例 As Double
                If 原图.Height > 原图.Width Then
                    缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Height
                Else
                    缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Width
                End If
                预览图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
            Else
                预览图片 = New Bitmap(原图.Width, 原图.Height)
            End If
            Dim 绘图器 As Graphics = Graphics.FromImage(预览图片)
            绘图器.FillRectangle(New SolidBrush(Color.White), 0, 0, 预览图片.Width, 预览图片.Height)
            绘图器.DrawImage(原图, 0, 0, 预览图片.Width, 预览图片.Height)
            原图.Dispose()
            绘图器.Dispose()
            预览图片.Save(原图路径 & ".jpg", Imaging.ImageFormat.Jpeg)
            预览图片.Dispose()
        Catch ex As Exception
            If 原图 IsNot Nothing Then 原图.Dispose()
            If 预览图片 IsNot Nothing Then 预览图片.Dispose()
            Throw ex
        End Try
    End Sub

    Public Function 是否是中文用户名(ByVal 用户名 As String) As Boolean
        If String.IsNullOrEmpty(用户名) = True Then Return False
        If 用户名.Length < 最小值_常量集合.本国语用户名长度 OrElse 用户名.Length > 最大值_常量集合.本国语用户名长度 Then Return False
        Dim 字符数组() As Char = 用户名.ToCharArray
        Const 下界 As Integer = &H4E00
        Const 上界 As Integer = &H9FA5
        Dim I, K, 汉字数 As Integer
        For I = 0 To 字符数组.Length - 1
            K = AscW(字符数组(I))
            If K >= 下界 AndAlso K <= 上界 Then
                汉字数 += 1
            ElseIf K >= 48 AndAlso K <= 57 Then  '0-9的数字
            ElseIf K = 95 Then  '下划线
            Else
                Return False
            End If
        Next
        If 汉字数 > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function 是否是英文用户名(ByVal 用户名 As String) As Boolean
        If String.IsNullOrEmpty(用户名) = True Then Return False
        If 用户名.Length < 最小值_常量集合.英语用户名长度 OrElse 用户名.Length > 最大值_常量集合.英语用户名长度 Then Return False
        Dim 字符数组() As Char = 用户名.ToCharArray
        Dim I, K, 英语小写字母数 As Integer
        For I = 0 To 字符数组.Length - 1
            K = AscW(字符数组(I))
            If K >= 97 AndAlso K <= 122 Then
                英语小写字母数 += 1
            ElseIf K >= 48 AndAlso K <= 57 Then  '0-9的数字
            ElseIf K = 95 Then  '下划线
            Else
                Return False
            End If
        Next
        If 英语小写字母数 > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function 是否是有效的讯宝或电子邮箱地址(ByVal 字符串 As String) As Boolean
        If String.IsNullOrEmpty(字符串) Then Return False
        If 字符串.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then Return False
        Dim 段() As String = 字符串.Split(New Char() {"@"c})
        If 段.Length = 2 Then
            If 字符串.Contains(" ") Then Return False
            If 字符串.Contains("."c) = False Then Return False
            Dim 节() As String = 段(1).Split(New Char() {"."c})
            If 节.Length < 2 Then Return False
            If 节.Length > 5 Then Return False
            Dim I As Short
            For I = 0 To 节.Length - 1
                If String.IsNullOrEmpty(节(I)) = True Then Return False
            Next
            Return True
        Else
            Return False
        End If
    End Function

    Public Function 检查英语域名(ByVal 英语域名 As String) As Boolean
        If String.IsNullOrEmpty(英语域名) Then Return False
        If 英语域名.Length > 最大值_常量集合.域名长度 Then Return False
        Dim 段() As String = 英语域名.Split(".")
        If 段.Length < 2 OrElse 段.Length > 3 Then Return False
        Dim I As Integer
        For I = 0 To 段.Length - 1
            If String.IsNullOrEmpty(段(I)) Then Return False
        Next
        Dim 字符数组() As Char = 段(0).ToCharArray
        Dim K As Integer
        For I = 0 To 字符数组.Length - 1
            K = AscW(字符数组(I))
            If K >= 97 AndAlso K <= 122 Then
            ElseIf K >= 48 AndAlso K <= 57 Then  '0-9的数字
            ElseIf K = 95 Then  '下划线
            ElseIf K = 45 Then  '中横线
            Else
                Return False
            End If
        Next
        Dim J As Integer
        For I = 1 To 段.Length - 1
            字符数组 = 段(I).ToCharArray
            For J = 0 To 字符数组.Length - 1
                K = AscW(字符数组(J))
                If K >= 97 AndAlso K <= 122 Then
                Else
                    Return False
                End If
            Next
        Next
        Return True
    End Function

    Public Function 检查本国语域名(ByVal 本国语域名 As String, ByVal 语言代码 As String) As Boolean
        If String.IsNullOrEmpty(本国语域名) Then Return False
        If 本国语域名.Length > 最大值_常量集合.域名长度 Then Return False
        Dim 段() As String = 本国语域名.Split(".")
        If 段.Length < 2 OrElse 段.Length > 3 Then Return False
        Dim I As Integer
        For I = 0 To 段.Length - 1
            If String.IsNullOrEmpty(段(I)) Then Return False
        Next
        Select Case 语言代码
            Case 语言代码_中文
                Const 下界 As Integer = &H4E00
                Const 上界 As Integer = &H9FA5
                Dim 字符数组() As Char = 段(0).ToCharArray
                Dim K, 汉字数 As Integer
                For I = 0 To 字符数组.Length - 1
                    K = AscW(字符数组(I))
                    If K >= 下界 AndAlso K <= 上界 Then
                        汉字数 += 1
                    ElseIf K >= 48 AndAlso K <= 57 Then  '0-9的数字
                    Else
                        Return False
                    End If
                Next
                If 汉字数 = 0 Then Return False
                Dim J As Integer
                For I = 1 To 段.Length - 1
                    字符数组 = 段(I).ToCharArray
                    For J = 0 To 字符数组.Length - 1
                        K = AscW(字符数组(J))
                        If K >= 下界 AndAlso K <= 上界 Then
                        Else
                            Return False
                        End If
                    Next
                Next
            Case Else
                Dim 字符数组() As Char
                Dim K, J As Integer
                For I = 0 To 段.Length - 1
                    字符数组 = 段(I).ToCharArray
                    For J = 0 To 字符数组.Length - 1
                        K = AscW(字符数组(J))
                        If K >= 97 AndAlso K <= 122 Then
                            Return False
                        ElseIf K >= 65 AndAlso K <= 90 Then
                            Return False
                        ElseIf K = 32 Then  '空格
                            Return False
                        End If
                    Next
                Next
        End Select
        Return True
    End Function

End Module
