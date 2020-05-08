
Public Module 模块_协议路径及测试路径

    Public Const 测试 As Boolean = False

    Const 测试域名1_英语 As String = "ssignal.net"
    Const 测试域名1_本国语 As String = "讯宝.网络"
    Const 测试域名2_英语 As String = "ssignal.cn"
    Const 测试域名2_本国语 As String = "讯宝.中国"

    Const 测试域名1中心服务器本机IIS调试端口 As Integer = 49676
    Public Const 测试域名1中心服务器本机IIS调试端口_SSL As Integer = 44327

    Public Const 测试域名1传送服务器本机IIS调试端口_SSL As Integer = 44341

    Const 测试域名1小宇宙中心服务器本机IIS调试端口_SSL As Integer = 44367

    Const 测试域名1主站服务器本机IIS调试端口 As Integer = 44325

    Const 测试域名1大群聊服务器本机IIS调试端口_SSL As Integer = 44351

    Const 测试域名2中心服务器本机IIS调试端口 As Integer = 49688
    Const 测试域名2中心服务器本机IIS调试端口_SSL As Integer = 44370
    Const 测试域名2传送服务器本机IIS调试端口_SSL As Integer = 44339

    Public Function 获取服务器域名(ByVal 子域名 As String) As String
        If 测试 = False Then
            Return 子域名
        Else
            If 子域名.EndsWith("." & 测试域名1_英语) OrElse 子域名.EndsWith("." & 测试域名1_本国语) Then
                If 子域名.StartsWith(讯宝中心服务器主机名 & ".") Then
                    Return "localhost:" & 测试域名1中心服务器本机IIS调试端口_SSL
                ElseIf 子域名.StartsWith(讯宝中心服务器主机名) Then
                    Return "localhost:" & 测试域名1传送服务器本机IIS调试端口_SSL
                ElseIf 子域名.StartsWith(讯宝大聊天群服务器主机名前缀) Then
                    Return "localhost:" & 测试域名1大群聊服务器本机IIS调试端口_SSL
                ElseIf 子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
                    Return "localhost:" & 测试域名1小宇宙中心服务器本机IIS调试端口_SSL
                End If
            Else
                If 子域名.StartsWith(讯宝中心服务器主机名 & ".") Then
                    Return "localhost:" & 测试域名2中心服务器本机IIS调试端口_SSL
                ElseIf 子域名.StartsWith(讯宝中心服务器主机名) Then
                    Return "localhost:" & 测试域名2传送服务器本机IIS调试端口_SSL
                ElseIf 子域名.StartsWith(讯宝大聊天群服务器主机名前缀) Then

                End If
            End If
            Return 子域名
        End If
    End Function

    Public Function 获取服务器真实域名(ByVal 子域名 As String, ByVal 域名_英语 As String) As String
        If 测试 = False Then
            Return 子域名
        Else
            If 子域名.StartsWith("localhost:") Then
                If 子域名.EndsWith(":" & 测试域名1中心服务器本机IIS调试端口_SSL) Then
                    Return 讯宝中心服务器主机名 & "." & 域名_英语
                ElseIf 子域名.EndsWith(":" & 测试域名1传送服务器本机IIS调试端口_SSL) Then
                    Return 讯宝中心服务器主机名 & "cnbj01." & 域名_英语
                ElseIf 子域名.EndsWith(":" & 测试域名1大群聊服务器本机IIS调试端口_SSL) Then
                    Return 讯宝大聊天群服务器主机名前缀 & "cnbj01." & 域名_英语
                ElseIf 子域名.EndsWith(":" & 测试域名1小宇宙中心服务器本机IIS调试端口_SSL) Then
                    Return 讯宝小宇宙中心服务器主机名 & "." & 域名_英语
                End If
            End If
            Return 子域名
        End If
    End Function

    Public Function 替换端口(ByVal 子域名 As String, ByVal 域名_英语 As String) As String
        If 测试 = False Then
            Return 子域名
        Else
            If String.Compare(域名_英语, 测试域名1_英语) = 0 Then
                Return 子域名.Replace(测试域名1中心服务器本机IIS调试端口, 测试域名1中心服务器本机IIS调试端口_SSL)
            Else
                Return 子域名.Replace(测试域名2中心服务器本机IIS调试端口, 测试域名2中心服务器本机IIS调试端口_SSL)
            End If
        End If
    End Function

    Public Function 获取传送服务器侦听端口(ByVal 域名_英语 As String) As Integer
        If 测试 = False Then
            Return 端口_传送服务器
        Else
            If String.Compare(域名_英语, 测试域名1_英语) = 0 Then
                Return 端口_传送服务器
            Else
                Return 端口_传送服务器 + 1
            End If
        End If
    End Function

    Public Function 获取中心服务器访问路径开头(ByVal 域名 As String) As String
        If 测试 = False Then
            Return "https://" & 讯宝中心服务器主机名 & "." & 域名 & "/?"
        Else
            Select Case 域名
                Case 测试域名1_英语, 测试域名1_本国语
                    Return "https://localhost:" & 测试域名1中心服务器本机IIS调试端口_SSL & "/?"
                Case Else
                    Return "https://localhost:" & 测试域名2中心服务器本机IIS调试端口_SSL & "/?"
            End Select
        End If
    End Function

    Public Function 获取传送服务器访问路径开头(ByVal 主机名 As String, ByVal 域名 As String, ByVal 媒体 As Boolean) As String
        If 媒体 = False Then
            If 测试 = False Then
                Return "https://" & 主机名 & "." & 域名 & "/?"
            Else
                If String.Compare(域名, 测试域名1_英语) = 0 Then
                    Return "https://localhost:" & 测试域名1传送服务器本机IIS调试端口_SSL & "/?"
                Else
                    Return "https://localhost:" & 测试域名2传送服务器本机IIS调试端口_SSL & "/?"
                End If
            End If
        Else
            If 测试 = False Then
                Return "https://" & 主机名 & "." & 域名 & "/media/?"
            Else
                If String.Compare(域名, 测试域名1_英语) = 0 Then
                    Return "https://localhost:" & 测试域名1传送服务器本机IIS调试端口_SSL & "/media/?"
                Else
                    Return "https://localhost:" & 测试域名2传送服务器本机IIS调试端口_SSL & "/media/?"
                End If
            End If
        End If
    End Function

    Public Function 获取大聊天群服务器访问路径开头(ByVal 子域名 As String, ByVal 媒体 As Boolean) As String
        If 媒体 = False Then
            If 测试 = False Then
                Return "https://" & 子域名 & "/?"
            Else
                Return "https://localhost:" & 测试域名1大群聊服务器本机IIS调试端口_SSL & "/?"
            End If
        Else
            If 测试 = False Then
                Return "https://" & 子域名 & "/media/?"
            Else
                Return "https://localhost:" & 测试域名1大群聊服务器本机IIS调试端口_SSL & "/media/?"
            End If
        End If
    End Function

    Public Function 获取讯友头像路径(ByVal 讯宝地址 As String, ByVal 主机名 As String, Optional ByVal 头像更新时间 As Long = 0) As String
        Dim 段() As String = 讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Dim 图标路径 As String
        If 测试 = False Then
            图标路径 = "http://" & 主机名 & "." & 段(1)
        Else
            If String.Compare(段(1), 测试域名1_英语) = 0 Then
                图标路径 = "https://localhost:" & 测试域名1传送服务器本机IIS调试端口_SSL
            Else
                图标路径 = "https://localhost:" & 测试域名2传送服务器本机IIS调试端口_SSL
            End If
        End If
        Return 图标路径 & "/icons/" & 段(0) & ".jpg" & IIf(头像更新时间 = 0, "", "?v=" & 头像更新时间)
    End Function

    Public Function 获取大聊天群图标路径(ByVal 子域名 As String, ByVal 群编号 As Long, ByVal 域名_英语 As String, Optional ByVal 图标更新时间 As Long = 0) As String
        Dim 图标路径 As String
        If 测试 = False Then
            图标路径 = "http://" & 子域名
        Else
            If String.Compare(域名_英语, 测试域名1_英语) = 0 Then
                图标路径 = "https://localhost:" & 测试域名1大群聊服务器本机IIS调试端口_SSL
            Else
                图标路径 = ""
            End If
        End If
        Return 图标路径 & "/icons/" & 群编号 & ".jpg" & IIf(图标更新时间 = 0, "", "?v=" & 图标更新时间)
    End Function

    Public Function 获取陌生人头像路径() As String
        Return "ss.jpg"
    End Function

    Public Function 获取我的头像路径(ByVal 英语用户名 As String, ByVal 主机名 As String, ByVal 头像更新时间 As Long, ByVal 域名_英语 As String) As String
        If String.IsNullOrEmpty(英语用户名) = False Then
            Dim 图标路径 As String
            If 测试 = False Then
                图标路径 = "http://" & 主机名 & "." & 域名_英语
            Else
                If String.Compare(域名_英语, 测试域名1_英语) = 0 Then
                    图标路径 = "https://localhost:" & 测试域名1传送服务器本机IIS调试端口_SSL
                Else
                    图标路径 = "https://localhost:" & 测试域名2传送服务器本机IIS调试端口_SSL
                End If
            End If
            Return 图标路径 & "/icons/" & 英语用户名 & ".jpg" & IIf(头像更新时间 = 0, "", "?v=" & 头像更新时间)
        Else
            Return 获取陌生人头像路径()
        End If
    End Function

    Public Function 获取当前用户小宇宙的访问路径(ByVal 当前用户英语用户名 As String, ByVal 域名_英语 As String) As String
        Dim 路径 As String
        If 测试 = False Then
            路径 = "https://" & 讯宝小宇宙中心服务器主机名 & "." & 域名_英语
        Else
            If String.Compare(域名_英语, 测试域名1_英语) = 0 Then
                路径 = "https://localhost:" & 测试域名1小宇宙中心服务器本机IIS调试端口_SSL
            Else
                路径 = ""
            End If
        End If
        Return 路径 & "/mytu/?LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName & "&EnglishUsername=" & 替换URI敏感字符(当前用户英语用户名)
    End Function

    Public Function 获取讯友小宇宙的访问路径(ByVal 讯友英语讯宝地址 As String, ByVal 当前用户英语讯宝地址 As String) As String
        Dim 段() As String = 讯友英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
        Dim 路径 As String
        If 测试 = False Then
            路径 = "https://" & 讯宝小宇宙中心服务器主机名 & "." & 段(1)
        Else
            If String.Compare(段(1), 测试域名1_英语) = 0 Then
                路径 = "https://localhost:" & 测试域名1小宇宙中心服务器本机IIS调试端口_SSL
            Else
                路径 = ""
            End If
        End If
        Return 路径 & "/tu/?LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户英语讯宝地址)
    End Function

    Public Function 获取大聊天群小宇宙的访问路径(ByVal 子域名 As String) As String
        Dim 路径 As String
        If 测试 = False Then
            路径 = "https://" & 子域名
        Else
            路径 = "https://localhost:" & 测试域名1大群聊服务器本机IIS调试端口_SSL
        End If
        Return 路径 & "/tu/?LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
    End Function

    Public Function 获取主站首页的访问路径() As String
        If 测试 = False Then
            Return "https://" & 讯宝移动主站服务器主机名 & "." & 讯宝网络域名_英语 & "/"
        Else
            If String.Compare(讯宝网络域名_英语, 测试域名1_英语) = 0 Then
                Return "https://localhost:" & 测试域名1主站服务器本机IIS调试端口 & "/"
            Else
                Return "https://localhost:" & 测试域名1主站服务器本机IIS调试端口 & "/"
            End If
        End If
    End Function

    Public Function 获取系统管理页面的访问路径(ByVal 域名_英语 As String) As String
        Dim 路径 As String
        If 测试 = False Then
            路径 = "https://" & 讯宝中心服务器主机名 & "." & 域名_英语
        Else
            路径 = "https://localhost:" & 测试域名1中心服务器本机IIS调试端口_SSL
        End If
        Return 路径 & "/Management.aspx"
    End Function

End Module
