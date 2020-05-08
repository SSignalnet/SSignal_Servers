
Public Module 模块_定义和声明

    Public 域名_英语, 域名_本国语 As String
    Public 域名验证 As Boolean = False

    Public Const XML已达上限 As String = "<REACHLIMIT/>"

    Friend Enum 操作代码_常量集合 As Byte
        无 = 0
        修改群名称 = 1
        修改群图标 = 2
        修改角色 = 3
        删减成员 = 4
        修改昵称 = 5
    End Enum

    Friend Enum 小宇宙操作代码_常量集合 As Byte
        无 = 0
        发布 = 1
        评论 = 2
        回复 = 3
        点赞 = 4
    End Enum

End Module
