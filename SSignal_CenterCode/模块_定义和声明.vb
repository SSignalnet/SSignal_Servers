Public Module 模块_定义和声明

    Public 域名_英语, 域名_本国语 As String
    Public 域名验证 As Boolean = False

    Friend Const 文件名_注册许可 As String = "AnyoneCanRegister.txt"

    Friend Enum 操作代码_常量集合 As Byte
        无 = 0
        更换用户密码 = 1
        更换电子邮箱地址 = 2
        更换手机号 = 3
        创建大聊天群 = 4
    End Enum

    Friend Structure 用户_复合数据
        Dim 编号 As Long
        Dim 英语用户名, 本国语用户名 As String
        Dim 位置号 As Short
        Dim 停用 As Boolean
    End Structure

    Friend Structure 用户2_复合数据
        Dim 编号, 手机号, 登录时间_电脑, 登录时间_手机 As Long
        Dim 英语用户名, 本国语用户名, 主机名 As String
        Dim 电子邮箱地址, 职能 As String
        Dim 停用 As Boolean
        Dim 网络地址_电脑(), 网络地址_手机() As Byte
    End Structure

    Friend Structure 服务器_复合数据
        Dim 主机名, 故障信息() As String
        Dim 网络地址() As Byte
        Dim 停用 As Boolean
        Dim 统计, 时间, 时间_故障信息() As Long
        Dim 故障信息数量 As Short
    End Structure

    Friend ReadOnly Property 网站链接 As String
        Get
            Return "https://www." & 域名_英语
        End Get
    End Property

    Friend ReadOnly Property 管理员电子邮箱地址 As String
        Get
            Return "master@" & 域名_英语
        End Get
    End Property

End Module
