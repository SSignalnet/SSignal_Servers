Friend Class 类_用户信息

    Friend Enum 范围_常量集合 As Byte
        全部 = 0
        用户名 = 1
        主机名 = 2
        用户名和主机名 = 3
        手机号 = 4
        电子邮箱地址 = 5
        职能 = 6
        职能和主机名 = 7
    End Enum

    Friend 用户编号, 手机号, 登录时间_电脑, 登录时间_手机 As Long
    Friend 英语用户名, 本国语用户名, 主机名, 电子邮箱地址, 职能 As String
    Friend 位置号 As Short
    Friend 网络地址_电脑, 网络地址_手机 As String
    Friend 范围 As 范围_常量集合

    Friend Sub New(ByVal 范围1 As 范围_常量集合)
        范围 = 范围1
    End Sub

End Class
