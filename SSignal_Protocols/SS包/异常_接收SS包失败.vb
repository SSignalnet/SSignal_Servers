Public Class 异常_接收SS包失败
    Inherits Exception

    Dim 提示文本 As String

    Public Sub New(ByVal 提示文本1 As String)
        提示文本 = 提示文本1
    End Sub

    Public Overrides ReadOnly Property Message As String
        Get
            Return 提示文本
        End Get
    End Property

End Class
