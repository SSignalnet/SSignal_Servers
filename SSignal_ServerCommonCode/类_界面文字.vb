Imports SSignal_Protocols

Public Class ��_��������

#Region "���������"

    Const �滻��ʶ As String = "#%"
    Const ��������ʶ_ǰ�� As String = "$%"
    Const ��������ʶ_���� As String = "/%"
    '������There is$% are$% #% apple/% apples/% on the table.

    Dim �ֶη�_�滻��ʶ() As String = New String() {�滻��ʶ}
    Dim �ֶη�_��������ʶ_ǰ��() As String = New String() {��������ʶ_ǰ��}
    Dim �ֶη�_��������ʶ_����() As String = New String() {��������ʶ_����}

    Dim ����() As String
    Public �������� As Integer
    Dim ��Ӣ�� As Boolean

#End Region

    Public Sub New(ByVal ��ҳ���� As String, Optional ByVal �������� As String = Nothing)
        �������� = 0
        ���� = Nothing
        If String.Compare(��ҳ����, ���Դ���_Ӣ��) = 0 Then
            ��Ӣ�� = True
            Return
        End If
        If String.IsNullOrEmpty(��������) Then Return
        Dim ��() As String = ��������.Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
        If ��.Length = 0 Then Return
        Dim I, J As Integer
        Dim ĳһ�� As String
        Dim �ҵ��� As Boolean
        ReDim ����(99)
        For I = 0 To ��.Length - 1
            ĳһ�� = ��(I).TrimStart
            If ĳһ��.StartsWith("[") Then
                ĳһ�� = ĳһ��.TrimEnd
                If ĳһ��.EndsWith("]") Then
                    If ĳһ��.Length > 2 Then
                        If String.Compare(��ҳ����, ĳһ��.Substring(1, ĳһ��.Length - 2)) = 0 Then
                            �ҵ��� = True
                        ElseIf �ҵ��� = True Then
                            Exit For
                        End If
                    End If
                End If
            ElseIf �ҵ��� = True AndAlso ĳһ��.StartsWith("<") Then
                J = ĳһ��.IndexOf(">")
                If J > 1 Then
                    J += 1
                    If Val(ĳһ��.Substring(1, J - 2)) <> �������� Then
                        �������� = 0
                        ���� = Nothing
                        Return
                    End If
                    If ĳһ��.Length > J Then
                        ����(��������) = ĳһ��.Substring(J, ĳһ��.Length - J)
                    End If
                    �������� += 1
                End If
            End If
        Next
        If �������� > 0 Then If �������� < ����.Length Then ReDim Preserve ����(�������� - 1)
    End Sub

    Public Function ��ȡ(ByVal ���� As Integer, ByVal �������� As String, Optional ByVal Ҫ������ı�() As Object = Nothing) As String
        If ��Ӣ�� = False Then
            If �������� > 0 Then
                If ���� >= 0 AndAlso ���� < �������� Then
                    If Ҫ������ı� Is Nothing Then
                        Return ����(����)
                    Else
                        Return �����ı�(����(����), Ҫ������ı�)
                    End If
                End If
            End If
        End If
        If Ҫ������ı� Is Nothing Then
            Return ��������
        Else
            Return �����ı�(��������, Ҫ������ı�)
        End If
    End Function

    Public Function JS��ȡ(ByVal ���� As Integer) As String
        Dim ����1 As String = ����(����)
        If String.IsNullOrEmpty(����1) = False Then
            If ����1.Contains(ChrW(34)) = False Then
                Return ChrW(34) & ����1 & ChrW(34)
            ElseIf ����1.Contains("'") = False Then
                Return "'" & ����1 & "'"
            Else
                Return "''"
            End If
        Else
            Return "''"
        End If
    End Function

    Private Function �����ı�(ByVal ԭ�ı� As String, ByVal Ҫ������ı�() As Object) As String
        Dim ��() As String = ԭ�ı�.Split(�ֶη�_�滻��ʶ, StringSplitOptions.None)
        If ��.Length > 1 Then
            Dim I, K As Integer
            Dim �滻����ı� As String = ""
            For I = 0 To ��.Length - 1
                If I = 0 Then
                    If I < Ҫ������ı�.Length Then
                        If ��(I).IndexOf(��������ʶ_ǰ��) < 0 Then
                            �滻����ı� = ��(I)
                        Else
                            Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_ǰ��, StringSplitOptions.RemoveEmptyEntries)
                            If ��1.Length > 1 Then
                                K = 0
                                If ��1.Length > 2 Then
                                    For K = 0 To ��1.Length - 3
                                        �滻����ı� &= ��1(K)
                                    Next
                                End If
                                Select Case Val(Ҫ������ı�(I))
                                    Case 1, 0 : �滻����ı� &= ��1(K)
                                    Case Else : �滻����ı� &= ��1(K + 1)
                                End Select
                            Else
                                �滻����ı� = ��(I)
                            End If
                        End If
                    Else
                        �滻����ı� = ��(I)
                    End If
                Else
                    K = I - 1
                    If K < Ҫ������ı�.Length Then
                        If ��(I).IndexOf(��������ʶ_����) > 0 Then
                            Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_����, StringSplitOptions.RemoveEmptyEntries)
                            If ��1.Length > 1 Then
                                Select Case Val(Ҫ������ı�(K))
                                    Case 1, 0 : �滻����ı� &= Ҫ������ı�(K) & ��1(0)
                                    Case Else : �滻����ı� &= Ҫ������ı�(K) & ��1(1)
                                End Select
                                If ��1.Length > 2 Then
                                    For K = 2 To ��1.Length - 1
                                        �滻����ı� &= ��1(K)
                                    Next
                                End If
                            Else
                                GoTo Line1
                            End If
                        Else
                            If I < Ҫ������ı�.Length Then
                                If ��(I).IndexOf(��������ʶ_ǰ��) >= 0 Then
                                    Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_ǰ��, StringSplitOptions.RemoveEmptyEntries)
                                    If ��1.Length > 1 Then
                                        If ��1.Length > 2 Then
                                            For K = 0 To ��1.Length - 3
                                                �滻����ı� &= ��1(K)
                                            Next
                                        End If
                                        Select Case Val(Ҫ������ı�(I))
                                            Case 1, 0 : �滻����ı� &= ��1(0)
                                            Case Else : �滻����ı� &= ��1(1)
                                        End Select
                                    Else
                                        GoTo Line1
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            Else
Line1:
                                �滻����ı� &= Ҫ������ı�(K) & ��(I)
                            End If
                        End If
                    Else
                        �滻����ı� &= �滻��ʶ & ��(I)
                    End If
                End If
            Next
            Return �滻����ı�
        Else
            Return ԭ�ı�
        End If
    End Function

End Class
