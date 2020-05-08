Imports System.IO
Imports SSignal_Protocols
Imports SSignal_ChatGroupCode

Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim 访问路径 As String = Request.Url.ToString
        If 访问路径.StartsWith("http://") AndAlso 测试 = False Then
            Response.Clear()
            Response.End()
            Return
        End If
        Dim 修改时间 As String = Request.Headers("If-Modified-Since")
        Dim 日期 As Date
        If String.IsNullOrEmpty(修改时间) = False Then
            If Date.TryParse(修改时间, 日期) = False Then 修改时间 = Nothing
        End If
        Dim 文件路径 As String = Nothing
        Try
            Dim 类 As New 类_处理请求(Application, Context, Request)
            文件路径 = 类.获取小宇宙数据的文件路径()
        Catch ex As Exception
        End Try
        Response.Clear()
        If String.IsNullOrEmpty(文件路径) = False Then
            Dim 文件流 As FileStream
            Try
                Dim 文件信息 As New FileInfo(文件路径)
                If 文件信息.Exists Then
                    Dim 修改日期 As String = 文件信息.LastWriteTimeUtc.ToString("ddd, dd MMM yyyy HH:mm:ss", Globalization.CultureInfo.GetCultureInfo("en-us")) & " GMT"
                    Response.AppendHeader("Last-Modified", 修改日期)
                    Response.AppendHeader("Content-Length", 文件信息.Length)
                    If String.IsNullOrEmpty(修改时间) = False Then
                        If 文件信息.LastWriteTimeUtc.Ticks < 日期.Ticks Then
                            Response.StatusCode = 304
                            Exit Try
                        End If
                    End If
                    文件流 = New FileStream(文件路径, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Dim 字节数组(8191) As Byte
                    Dim 读取的字节数 As Integer
                    Do
                        读取的字节数 = 文件流.Read(字节数组, 0, 字节数组.Length)
                        If 读取的字节数 > 0 Then
                            Response.OutputStream.Write(字节数组, 0, 读取的字节数)
                        End If
                    Loop Until 读取的字节数 = 0
                End If
            Catch ex As Exception
            End Try
        End If
        Response.End()
    End Sub

End Class