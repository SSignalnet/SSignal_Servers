Imports SSignal_Protocols

Friend Class 类_要推送的讯宝

    Friend 发送时间, 接收者编号, 发送序号, 同服发送者编号, 文本编号 As Long
    Friend 发送者英语讯宝地址, 群主讯宝地址 As String
    Friend 位置号, 文本库号, 宽度, 高度 As Short
    Friend 讯宝指令 As 讯宝指令_常量集合
    Friend 文本 As String
    Friend 秒数, 群编号 As Byte
    Friend 当前状态 As 讯宝推送状态_常量集合
    Friend 不删除, 文本一样 As Boolean
    Friend 设备类型 As 设备类型_常量集合

End Class
