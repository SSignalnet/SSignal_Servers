
Public Module 模块_协议路径

    Public Function 获取路径_验证讯宝地址真实性(ByVal 子域名_地址所属域的中心服务器 As String, ByVal 访问服务器的凭据 As String, ByVal 讯宝地址 As String, ByVal 主机名_发起验证请求的服务器 As String, ByVal 域名_英语 As String, ByVal 域名_本国语 As String) As String
        Return "https://" & 子域名_地址所属域的中心服务器 & "/?C=VerifySSAddress&Credential=" & 替换URI敏感字符(访问服务器的凭据) & "&SSAddress=" & 替换URI敏感字符(讯宝地址) & "&Domain_English=" & 替换URI敏感字符(主机名_发起验证请求的服务器 & "." & 域名_英语) & IIf(String.IsNullOrEmpty(域名_本国语), "", "&Domain_Native=" & 替换URI敏感字符(主机名_发起验证请求的服务器 & "." & 域名_本国语))
    End Function

    Public Function 获取路径_验证服务器真实性(ByVal 子域名_待验证服务器 As String, ByVal 访问服务器的凭据 As String, ByVal 子域名_发起验证请求的服务器 As String, Optional ByVal 本国语域名_待验证服务器 As String = Nothing) As String
        Return "https://" & 获取服务器域名(子域名_待验证服务器) & "/?C=VerifySSServer&Credential=" & 替换URI敏感字符(访问服务器的凭据) & "&Domain_Ask=" & 替换URI敏感字符(子域名_发起验证请求的服务器) & "&Domain_English=" & 替换URI敏感字符(子域名_待验证服务器) & IIf(String.IsNullOrEmpty(本国语域名_待验证服务器), "", "&Domain_Native=" & 替换URI敏感字符(本国语域名_待验证服务器))
    End Function

    Public Function 获取路径_获取大聊天群服务器连接凭据(ByVal 子域名_大聊天群服务器 As String, ByVal 访问服务器的凭据 As String,
                                       ByVal 传送服务器主机名 As String, ByVal 英语用户名 As String, ByVal 设备类型 As String, ByVal 群编号 As Long, ByVal 域名_英语 As String) As String
        Return "https://" & 子域名_大聊天群服务器 & "/?C=GetCredential&Credential=" & 替换URI敏感字符(访问服务器的凭据) & "&Domain=" & 传送服务器主机名 & "." & 域名_英语 & "&EnglishSSAddress=" & 替换URI敏感字符(英语用户名 & 讯宝地址标识 & 域名_英语) & "&DeviceType=" & 设备类型 & "&GroupID=" & 群编号
    End Function

    Public Function 获取路径_获取小宇宙服务器连接凭据(ByVal 子域名_小宇宙服务器 As String, ByVal 访问服务器的凭据 As String,
                                       ByVal 传送服务器主机名 As String, ByVal 英语用户名 As String, ByVal 设备类型 As String, ByVal 域名_英语 As String) As String
        Return "https://" & 子域名_小宇宙服务器 & "/?C=GetCredential&Credential=" & 替换URI敏感字符(访问服务器的凭据) & "&Domain=" & 传送服务器主机名 & "." & 域名_英语 & "&EnglishSSAddress=" & 替换URI敏感字符(英语用户名 & 讯宝地址标识 & 域名_英语) & "&DeviceType=" & 设备类型
    End Function

End Module
