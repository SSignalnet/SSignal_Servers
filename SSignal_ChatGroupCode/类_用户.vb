Imports System.Net.Sockets
Imports System.Security.Cryptography

Friend Class 类_用户
    Implements IDisposable

    Friend 英语SS地址, 本国语SS地址, 连接凭据, 主机名 As String
    Friend 位置号 As Short
    Friend SS序号_电脑发送, SS序号_手机发送, SS群消息序号 As Long

    Friend 网络连接器_电脑, 网络连接器_手机 As Socket
    Friend 电脑连接步骤未完成, 手机连接步骤未完成 As Boolean

    Friend AES加解密模块_电脑, AES加解密模块_手机 As RijndaelManaged
    Friend AES加密器_电脑, AES解密器_电脑, AES加密器_手机, AES解密器_手机 As ICryptoTransform

    Friend 网络地址_电脑(), 网络地址_手机() As Byte

#Region "IDisposable Support"
    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                If 网络连接器_电脑 IsNot Nothing Then
                    Try
                        网络连接器_电脑.Close()
                    Catch ex As Exception
                    End Try
                    网络连接器_电脑 = Nothing
                End If
                If 网络连接器_手机 IsNot Nothing Then
                    Try
                        网络连接器_手机.Close()
                    Catch ex As Exception
                    End Try
                    网络连接器_手机 = Nothing
                End If
                If AES加密器_电脑 IsNot Nothing Then
                    AES加密器_电脑.Dispose()
                    AES加密器_电脑 = Nothing
                End If
                If AES解密器_电脑 IsNot Nothing Then
                    AES解密器_电脑.Dispose()
                    AES解密器_电脑 = Nothing
                End If
                If AES加密器_手机 IsNot Nothing Then
                    AES加密器_手机.Dispose()
                    AES加密器_手机 = Nothing
                End If
                If AES解密器_手机 IsNot Nothing Then
                    AES解密器_手机.Dispose()
                    AES解密器_手机 = Nothing
                End If
                If AES加解密模块_电脑 IsNot Nothing Then
                    AES加解密模块_电脑.Dispose()
                    AES加解密模块_电脑 = Nothing
                End If
                If AES加解密模块_手机 IsNot Nothing Then
                    AES加解密模块_手机.Dispose()
                    AES加解密模块_手机 = Nothing
                End If
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

End Class
