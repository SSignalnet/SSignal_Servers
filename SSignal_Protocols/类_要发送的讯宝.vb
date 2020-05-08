
Public Class 类_要发送的讯宝

    Public Enum 状态_常量集合 As Byte
        等待 = 0
        发送中 = 1
        结束 = 2
    End Enum

    Public 时间, 发送者编号, 序号, 文本编号 As Long
    Public 讯宝指令 As 讯宝指令_常量集合
    Public 发送者英语地址, 群主英语地址, 接收者主机名, 文本 As String
    Public 接收者() As 接收者_复合数据
    Public 设备类型 As 设备类型_常量集合
    Public 群编号, 秒数 As Byte
    Public 当前状态 As 状态_常量集合
    Public 发送失败的原因 As 讯宝指令_常量集合
    Public 文本库号, 宽度, 高度, 发送者位置号 As Short

    Public 子域名_发送服务器, 发送凭据, 子域名_接收服务器 As String

End Class
