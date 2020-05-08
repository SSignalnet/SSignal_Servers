
Public Module 模块_定义和声明

    Public Const 身份码类型_手机号 As String = "Phone"
    Public Const 身份码类型_本国语用户名 As String = "Native"
    Public Const 身份码类型_英语用户名 As String = "English"
    Public Const 身份码类型_电子邮箱地址 As String = "Email"

    Public Const 验证码的有效时间_分钟 As Byte = 10
    Public Const 验证码的时间间隔_分钟 As Byte = 1
    Public Const 最近操作次数统计时间_分钟 As Byte = 10

    Public Const 收发时限 As Integer = 10000

    Public Const 职能_管理员 As String = "z"
    Public Const 职能_副管理员 As String = "f"

    Public Enum 流星语列表项样式_常量集合 As Byte
        无图 = 1
        一幅小图片 = 2
        三幅小图片 = 3
        一幅大图片 = 4
    End Enum

    Public Structure 域名_复合数据
        Dim 英语, 本国语 As String
    End Structure

End Module
