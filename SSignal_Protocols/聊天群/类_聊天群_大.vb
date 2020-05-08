Imports System.Threading

Public Class 类_聊天群_大
    Inherits 类_聊天群

    Public 编号, 图标更新时间, 最新讯宝的发送时间, 检查时间 As Long
    Public 主机名, 英语域名, 本国语域名, 子域名, 名称, 连接凭据 As String
    Public 我的角色 As 群角色_常量集合

    Public 线程 As Thread

End Class
