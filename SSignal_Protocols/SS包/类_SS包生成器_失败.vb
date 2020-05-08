Public Class 类_SS包生成器_失败
    Inherits 类_SS包生成器

    Public Sub New(ByVal 提示信息 As String, Optional ByVal SS包编码1 As SS包编码_常量集合 = SS包编码_常量集合.Unicode_LittleEndian)
        查询结果1 = 查询结果_常量集合.失败
        Call 初始化(False, 2, SS包编码1)
        Call 添加_有标签(冒号_SS包保留标签, 查询结果1)
        Call 添加_有标签(感叹号_SS包保留标签, 提示信息)
    End Sub

End Class
