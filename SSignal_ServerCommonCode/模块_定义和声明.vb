
Public Module 模块_定义和声明

    Public Const XML成功 As String = "<SUCCEED/>"
    Public Const XML失败 As String = "<FAILED/>"
    Public Const XML不正确 As String = "<INCORRECT/>"
    Public Const XML无权操作 As String = "<NOTAUTHORIZED/>"
    Public Const XML数据库未就绪 As String = "<DATABASENOTREADY/>"
    Public Const XML凭据无效 As String = "<INVALIDCREDENTIAL/>"
    Public Const XML已停用 As String = "<DISABLED/>"

    Public Enum 系统任务类型_常量集合 As Byte
        无 = 0
        '暂停服务 = 1
        邮箱密码 = 2
    End Enum

    Public Enum 讯友录变动_常量集合 As Byte
        无 = 0
        添加 = 1
        修改拉黑 = 2
        删除 = 3
        重命名标签 = 4
    End Enum

End Module
