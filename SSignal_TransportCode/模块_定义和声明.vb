
Public Module 模块_定义和声明

    Public 域名_英语, 域名_本国语 As String
    Public 域名验证 As Boolean = False

    Friend Enum 讯宝推送状态_常量集合 As Byte
        等待 = 0
        推送中 = 1
        结束 = 2
    End Enum

    Friend Enum 讯宝接收结果_常量集合 As Byte
        待确认 = 0
        电脑端接收成功 = 1
        手机端接收成功 = 2
    End Enum

    Friend Structure 收发统计_复合数据
        Dim 用户编号 As Long
        Dim 今日几号 As Integer
        Dim 今日几时 As Byte
        Dim 今日发送, 今日接收,
                昨日发送, 昨日接收,
                前日发送, 前日接收,
                时段发送, 位置号 As Short
    End Structure

    Friend Structure 接收统计_复合数据
        Dim 今日几号 As Integer
        Dim 今日几时 As Byte
        Dim 今日接收, 时段接收 As Short
    End Structure

    Friend Structure 创建的群_复合数据
        Dim 用户编号 As Long
        Dim 位置号 As Short
        Dim 数量 As Byte
    End Structure

End Module
