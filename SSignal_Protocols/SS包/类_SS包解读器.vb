Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Security.Cryptography

Public Class ��_SS�������

    Protected Ҫ�����SS��() As Byte
    Dim �б�ǩ����() As SS������_��������
    Dim ��������, �Ѷ�ȡ�ֽ���, ��ʼ���� As Integer
    Protected �ޱ�ǩ As Boolean
    Dim �跴ת As Boolean
    Dim ���� As Encoding

    Public Sub New()

    End Sub

    Public Sub New(ByVal ���������� As Socket, Optional ByVal AES������ As ICryptoTransform = Nothing, Optional ByVal ��󳤶� As Integer = 0)
        Dim �ֽ�����() As Byte = ����ָ�����ȵ�����(����������, 5)
        If �ֽ����� Is Nothing Then
            Throw New �쳣_����SS��ʧ��("����SS��ʧ�ܡ�")
        End If
        Dim ���� As Integer
        Select Case ChrW(�ֽ�����(0))
            Case SS���ߵ�λ��ʶ_L
                ���� = BitConverter.ToInt32(�ֽ�����, 1)
            Case SS���ߵ�λ��ʶ_B
                ���� = BitConverter.ToInt32(��ת(�ֽ�����, 1, 4), 0)
            Case Else
                Throw New Exception("�޷��ж��Ǹ�λ��ǰ���ǵ�λ��ǰ��")
        End Select
        If ���� < SS����ʶ_�б�ǩ.Length Then
            Throw New Exception("δ���յ�SS����")
        End If
        If ��󳤶� > 0 Then
            If ���� > ��󳤶� Then
                Throw New Exception("���ݳ��ȴ���ָ������󳤶ȡ�")
            End If
        End If
        �ֽ����� = ����ָ�����ȵ�����(����������, ����)
        If �ֽ����� Is Nothing Then
            Throw New �쳣_����SS��ʧ��("����SS��ʧ�ܡ�")
        End If
        ���(�ֽ�����, AES������)
    End Sub

    Public Sub New(ByVal Ҫ�����SS��1() As Byte, Optional ByVal AES������ As ICryptoTransform = Nothing)
        ���(Ҫ�����SS��1, AES������)
    End Sub

    Public Sub New(ByVal SS��������_�б�ǩ As ��_SS��������)
        If SS��������_�б�ǩ.�ޱ�ǩ = True Then Throw New Exception("�����ޱ�ǩSS����������")
        �б�ǩ���� = SS��������_�б�ǩ.SS������
        �������� = SS��������_�б�ǩ.SS����������
    End Sub

    Private Sub ���(ByVal Ҫ�����SS��1() As Byte, ByVal AES������ As ICryptoTransform)
        If AES������ Is Nothing Then
            Ҫ�����SS�� = Ҫ�����SS��1
        Else
            Ҫ�����SS�� = AES����(Ҫ�����SS��1, AES������)
        End If
        Dim SS������ As SS������_��������
        Select Case ASCII.GetString(Ҫ�����SS��, 0, SS����ʶ_�б�ǩ.Length)
            Case SS����ʶ_�ޱ�ǩ
                �Ѷ�ȡ�ֽ��� = SS����ʶ_�б�ǩ.Length
                Select Case ChrW(Ҫ�����SS��(�Ѷ�ȡ�ֽ���))
                    Case SS���ߵ�λ��ʶ_L
                    Case SS���ߵ�λ��ʶ_B
                        �跴ת = True
                    Case Else
                        Throw New Exception("�޷��ж��Ǹ�λ��ǰ���ǵ�λ��ǰ��")
                End Select
                �Ѷ�ȡ�ֽ��� += 1
                SS������ = Ҫ�����SS��(�Ѷ�ȡ�ֽ���)
                �Ѷ�ȡ�ֽ��� += 1
                Dim �ܳ��� As Integer
                If Not �跴ת Then
                    �ܳ��� = BitConverter.ToInt32(Ҫ�����SS��, �Ѷ�ȡ�ֽ���)
                Else
                    �ܳ��� = BitConverter.ToInt32(��ת(Ҫ�����SS��, �Ѷ�ȡ�ֽ���, 4), 0)
                End If
                If �ܳ��� <= 0 Then
                    Throw New Exception("�����𻵡�")
                End If
                �Ѷ�ȡ�ֽ��� += 4
                If �Ѷ�ȡ�ֽ��� + �ܳ��� <> Ҫ�����SS��.Length Then
                    Throw New Exception("�����𻵡�")
                End If
                ѡ�����(SS������)
                �ޱ�ǩ = True
            Case SS����ʶ_�б�ǩ
                �Ѷ�ȡ�ֽ��� = SS����ʶ_�б�ǩ.Length
                Select Case ChrW(Ҫ�����SS��(�Ѷ�ȡ�ֽ���))
                    Case SS���ߵ�λ��ʶ_L
                    Case SS���ߵ�λ��ʶ_B
                        �跴ת = True
                    Case Else
                        Throw New Exception("�޷��ж��Ǹ�λ��ǰ���ǵ�λ��ǰ��")
                End Select
                �Ѷ�ȡ�ֽ��� += 1
                SS������ = Ҫ�����SS��(�Ѷ�ȡ�ֽ���)
                �Ѷ�ȡ�ֽ��� += 1
                If Not �跴ת Then
                    �������� = BitConverter.ToInt32(Ҫ�����SS��, �Ѷ�ȡ�ֽ���)
                Else
                    �������� = BitConverter.ToInt32(��ת(Ҫ�����SS��, �Ѷ�ȡ�ֽ���, 4), 0)
                End If
                If �������� <= 0 Then
                    Throw New Exception("�����𻵡�")
                End If
                �Ѷ�ȡ�ֽ��� += 4
                ѡ�����(SS������)
                �ޱ�ǩ = True
                ReDim �б�ǩ����(�������� - 1)
                Dim I As Integer
                While δ�����ݳ��� > 0
                    With �б�ǩ����(I)
                        Call ��ȡ(.��ǩ)
                        Call ��ȡ�ֽ�(.����)
                        Select Case .����
                            Case SS����������_��������.�ַ���
                                .������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                                Dim �ַ��� As String = Nothing
                                Call ��ȡ(�ַ���, .������Ϣ�ֽ���)
                                .���� = �ַ���
                            Case SS����������_��������.�з��ų�����
                                Dim �з��ų����� As Long
                                Call ��ȡ(�з��ų�����)
                                .���� = �з��ų�����
                            Case SS����������_��������.�з�������
                                Dim �з������� As Integer
                                Call ��ȡ(�з�������)
                                .���� = �з�������
                            Case SS����������_��������.�з��Ŷ�����
                                Dim �з��Ŷ����� As Short
                                Call ��ȡ(�з��Ŷ�����)
                                .���� = �з��Ŷ�����
                            Case SS����������_��������.���ֵ
                                Dim ���ֵ As Boolean
                                Call ��ȡ(���ֵ)
                                .���� = ���ֵ
                            Case SS����������_��������.�ֽ�
                                Dim �ֽ� As Byte
                                Call ��ȡ(�ֽ�)
                                .���� = �ֽ�
                            Case SS����������_��������.��SS��
                                .������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                                Dim �ֽ�����() As Byte = Nothing
                                Call ��ȡ(�ֽ�����, .������Ϣ�ֽ���)
                                If �ֽ����� IsNot Nothing Then
                                    .���� = New ��_SS�������(�ֽ�����)
                                End If
                            Case SS����������_��������.�ֽ�����
                                .������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                                Dim �ֽ�����() As Byte = Nothing
                                Call ��ȡ(�ֽ�����, .������Ϣ�ֽ���)
                                .���� = �ֽ�����
                            Case SS����������_��������.�����ȸ�����
                                Dim �����ȸ����� As Single
                                Call ��ȡ(�����ȸ�����)
                                .���� = �����ȸ�����
                            Case SS����������_��������.˫���ȸ�����
                                Dim ˫���ȸ����� As Double
                                Call ��ȡ(˫���ȸ�����)
                                .���� = ˫���ȸ�����
                        End Select
                    End With
                    I += 1
                    If I >= �������� Then Exit While
                End While
                �ޱ�ǩ = False
                If �������� <> I Then
                    �������� = I
                    Throw New Exception("�����𻵡�")
                End If
            Case Else
                Throw New Exception("�ⲻ��SS����")
        End Select
    End Sub

    Private Sub ѡ�����(ByVal SS������ As SS������_��������)
        Select Case SS������
            Case SS������_��������.Unicode_LittleEndian : ���� = Unicode
            Case SS������_��������.Unicode_BigEndian : ���� = BigEndianUnicode
            Case SS������_��������.UTF8 : ���� = UTF8
            Case SS������_��������.ASCII : ���� = ASCII
            Case SS������_��������.UTF32 : ���� = UTF32
            Case SS������_��������.UTF7 : ���� = UTF7
            Case Else : ���� = UTF8
        End Select
    End Sub

    Public Sub ������ı�(ByVal �ı� As String)
        If String.IsNullOrEmpty(�ı�) Then Return
        Dim ��() As String = �ı�.Split(New String() {vbLf}, StringSplitOptions.RemoveEmptyEntries)
        If ��.Length > 1 Then
            If String.Compare(��(0), SS����ʶ_���ı�) <> 0 Then
                Throw New Exception("�Ҳ���SS����ʶ��" & SS����ʶ_���ı�)
            End If
            ������ı�(��, 1)
        End If
    End Sub

    Public Function ������ı�(ByVal ��() As String, ByVal ��ʼ�� As Integer, Optional ByVal �㼶 As Integer = 0) As Integer
        ReDim �б�ǩ����(19)
        Dim ĳһ��, ��ǩ As String
        Dim ���� As SS����������_��������
        Dim ���� As Object
        Dim �ո��ַ��� As String = Nothing
        If �㼶 > 0 Then
            �ո��ַ��� = Space(�㼶)
        ElseIf �㼶 < 0 Then
            �㼶 = 0
        End If
        Dim I, J, K As Integer
        For I = ��ʼ�� To ��.Length - 1
��ת��2:
            ĳһ�� = ��(I)
            If �㼶 > 0 Then
                If ĳһ��.StartsWith(�ո��ַ���) = False Then Return I
                ĳһ�� = ĳһ��.Substring(�ո��ַ���.Length)
            End If
            If ĳһ��.StartsWith(" ") Then GoTo ��ת��1
            K = ĳһ��.IndexOf(":", 1)
            If K < 0 Then GoTo ��ת��1
            Select Case ĳһ��.Substring(0, K)
                Case "S" : ���� = SS����������_��������.�ַ���
                Case "8" : ���� = SS����������_��������.�з��ų�����
                Case "4" : ���� = SS����������_��������.�з�������
                Case "2" : ���� = SS����������_��������.�з��Ŷ�����
                Case "SS" : ���� = SS����������_��������.��SS��
                Case "BT" : ���� = SS����������_��������.�ֽ�����
                Case "BL" : ���� = SS����������_��������.���ֵ
                Case "1" : ���� = SS����������_��������.�ֽ�
                Case "4F" : ���� = SS����������_��������.�����ȸ�����
                Case "8F" : ���� = SS����������_��������.˫���ȸ�����
                Case Else : GoTo ��ת��1
            End Select
            J = ĳһ��.IndexOf("=", K + 2)
            If J < 0 Then GoTo ��ת��1
            K += 1
            ��ǩ = ĳһ��.Substring(K, J - K)
            If ���� <> SS����������_��������.��SS�� Then
                If ���� <> SS����������_��������.�ַ��� Then
                    If ���� <> SS����������_��������.�ֽ����� Then
                        If J = ĳһ��.Length - 1 Then GoTo ��ת��1
                        Select Case ����
                            Case SS����������_��������.�з��ų�����
                                Dim �з��ų����� As Long
                                If Long.TryParse(ĳһ��.Substring(J + 1), �з��ų�����) = False Then GoTo ��ת��1
                                ���� = �з��ų�����
                            Case SS����������_��������.�з�������
                                Dim �з������� As Integer
                                If Integer.TryParse(ĳһ��.Substring(J + 1), �з�������) = False Then GoTo ��ת��1
                                ���� = �з�������
                            Case SS����������_��������.�з��Ŷ�����
                                Dim �з��Ŷ����� As Short
                                If Short.TryParse(ĳһ��.Substring(J + 1), �з��Ŷ�����) = False Then GoTo ��ת��1
                                ���� = �з��Ŷ�����
                            Case SS����������_��������.���ֵ
                                If String.Compare(ĳһ��.Substring(J + 1), "true", True) = 0 Then
                                    ���� = True
                                Else
                                    ���� = False
                                End If
                            Case SS����������_��������.�ֽ�
                                Dim �ֽ� As Byte
                                If Byte.TryParse(ĳһ��.Substring(J + 1), �ֽ�) = False Then GoTo ��ת��1
                                ���� = �ֽ�
                            Case SS����������_��������.�����ȸ�����
                                Dim �����ȸ����� As Single
                                If Single.TryParse(ĳһ��.Substring(J + 1), �����ȸ�����) = False Then GoTo ��ת��1
                                ���� = �����ȸ�����
                            Case SS����������_��������.˫���ȸ�����
                                Dim ˫���ȸ����� As Double
                                If Double.TryParse(ĳһ��.Substring(J + 1), ˫���ȸ�����) = False Then GoTo ��ת��1
                                ���� = ˫���ȸ�����
                            Case Else
                                GoTo ��ת��1
                        End Select
                    Else
                        If J < ĳһ��.Length - 1 Then
                            ���� = System.Convert.FromBase64String(ĳһ��.Substring(J + 1))
                        Else
                            ���� = Nothing
                        End If
                    End If
                Else
                    If J < ĳһ��.Length - 1 Then
                        ���� = ĳһ��.Substring(J + 1).Replace("&;", vbCrLf).Replace("&amp;", "&")
                    Else
                        ���� = ""
                    End If
                End If
                ����б�ǩ����(��ǩ, ����, ����)
            Else
                Dim SS������� As New ��_SS�������
                J = SS�������.������ı�(��, I + 1, �㼶 + 1)
                ����б�ǩ����(��ǩ, ����, SS�������)
                I = J
                If J < ��.Length Then
                    GoTo ��ת��2
                Else
                    Exit For
                End If
            End If
        Next
        �ޱ�ǩ = False
        Return I
��ת��1��
        Throw New Exception("�����д����ڵ�" & I + 1 & "��")
    End Function

    Private Sub ����б�ǩ����(ByVal ��ǩ As String, ByVal ���� As SS����������_��������, ByVal ���� As Object)
        If �б�ǩ����.Length = �������� Then ReDim Preserve �б�ǩ����(�������� * 2 - 1)
        With �б�ǩ����(��������)
            .��ǩ = ��ǩ
            .���� = ����
            .���� = ����
        End With
        �������� += 1
    End Sub

    Public Function ��ȡ_ָ������(ByVal ���� As Integer) As Byte()
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If ���� < 1 Then
            Return Nothing
        Else
            If �Ѷ�ȡ�ֽ��� + ���� > Ҫ�����SS��.Length Then
                Throw New Exception("���ݳ��Ȳ���")
            Else
                Dim �ֽ�����(���� - 1) As Byte
                Array.Copy(Ҫ�����SS��, �Ѷ�ȡ�ֽ���, �ֽ�����, 0, ����)
                �Ѷ�ȡ�ֽ��� += ����
                Return �ֽ�����
            End If
        End If
    End Function

    Public Function ��ȡ_���ݳ�����Ϣ(ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_��������) As Byte()
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If ������Ϣ�ֽ��� < ������Ϣ�ֽ���_��������.���ֽ� Then
            Throw New Exception("������Ϣ�ֽ������Ϸ�")
        Else
            If �Ѷ�ȡ�ֽ��� + ������Ϣ�ֽ��� > Ҫ�����SS��.Length Then
                Throw New Exception("���ݳ��Ȳ���")
            Else
                Select Case ������Ϣ�ֽ���
                    Case ������Ϣ�ֽ���_��������.���ֽ�
                        Dim ���� As Integer
                        If Not �跴ת Then
                            ���� = BitConverter.ToInt16(Ҫ�����SS��, �Ѷ�ȡ�ֽ���)
                        Else
                            ���� = BitConverter.ToInt16(��ת(Ҫ�����SS��, �Ѷ�ȡ�ֽ���, 2), 0)
                        End If
                        �Ѷ�ȡ�ֽ��� += ������Ϣ�ֽ���
                        Return ��ȡ_ָ������(����)
                    Case ������Ϣ�ֽ���_��������.���ֽ�
                        Dim ���� As Integer
                        If Not �跴ת Then
                            ���� = BitConverter.ToInt32(Ҫ�����SS��, �Ѷ�ȡ�ֽ���)
                        Else
                            ���� = BitConverter.ToInt32(��ת(Ҫ�����SS��, �Ѷ�ȡ�ֽ���, 4), 0)
                        End If
                        �Ѷ�ȡ�ֽ��� += ������Ϣ�ֽ���
                        Return ��ȡ_ָ������(����)
                    Case Else
                        Throw New Exception("������Ϣ�ֽ������Ϸ�")
                End Select
            End If
        End If
    End Function

    Public Sub ��ȡ(ByRef ��ֵ���� As Boolean)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(1)
        If �ֽ�����(0) = 0 Then
            ��ֵ���� = False
        Else
            ��ֵ���� = True
        End If
    End Sub

    Public Sub ��ȡ�ֽ�(ByRef ��ֵ���� As Object)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(1)
        ��ֵ���� = �ֽ�����(0)
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Byte)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(1)
        ��ֵ���� = �ֽ�����(0)
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Short)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(2)
        If Not �跴ת Then
            ��ֵ���� = BitConverter.ToInt16(�ֽ�����, 0)
        Else
            ��ֵ���� = BitConverter.ToInt16(��ת(�ֽ�����), 0)
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Integer)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(4)
        If Not �跴ת Then
            ��ֵ���� = BitConverter.ToInt32(�ֽ�����, 0)
        Else
            ��ֵ���� = BitConverter.ToInt32(��ת(�ֽ�����), 0)
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Long)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(8)
        If Not �跴ת Then
            ��ֵ���� = BitConverter.ToInt64(�ֽ�����, 0)
        Else
            ��ֵ���� = BitConverter.ToInt64(��ת(�ֽ�����), 0)
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Single)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(4)
        If Not �跴ת Then
            ��ֵ���� = BitConverter.ToSingle(�ֽ�����, 0)
        Else
            ��ֵ���� = BitConverter.ToSingle(��ת(�ֽ�����), 0)
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As Double)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_ָ������(8)
        If Not �跴ת Then
            ��ֵ���� = BitConverter.ToDouble(�ֽ�����, 0)
        Else
            ��ֵ���� = BitConverter.ToDouble(��ת(�ֽ�����), 0)
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ���� As String, Optional ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_�������� = ������Ϣ�ֽ���_��������.���ֽ�, Optional ByVal ����ֵ As Boolean = False)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Dim �ֽ�����() As Byte = ��ȡ_���ݳ�����Ϣ(������Ϣ�ֽ���)
        If ����ֵ = False Then
            If �ֽ����� IsNot Nothing Then
                ��ֵ���� = ����.GetString(�ֽ�����)
            Else
                ��ֵ���� = Nothing
            End If
        End If
    End Sub

    Public Sub ��ȡ(ByRef ��ֵ����() As Byte, ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_��������, Optional ByVal AES������ As ICryptoTransform = Nothing)
        If �ޱ�ǩ = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If AES������ Is Nothing Then
            ��ֵ���� = ��ȡ_���ݳ�����Ϣ(������Ϣ�ֽ���)
        Else
            ��ֵ���� = AES����(��ȡ_���ݳ�����Ϣ(������Ϣ�ֽ���), AES������)
        End If
    End Sub

    Public Sub ��ȡ_�б�ǩ(ByVal ��ǩ As String, ByRef ��ֵ���� As Object)
        If �ޱ�ǩ = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        ��ȡ_�б�ǩ2(��ǩ, ��ֵ����)
    End Sub

    Public Sub ��ȡ_�б�ǩ(ByVal ��ǩ As String, ByRef ��ֵ���� As Object, ByVal Ĭ��ֵ As Object)
        If �ޱ�ǩ = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        If ��ȡ_�б�ǩ2(��ǩ, ��ֵ����) = False Then
            ��ֵ���� = Ĭ��ֵ
        End If
    End Sub

    Private Function ��ȡ_�б�ǩ2(ByVal ��ǩ As String, ByRef ��ֵ���� As Object) As Boolean
        If �������� > 0 Then
            Dim I As Integer
            For I = ��ʼ���� To �������� - 1
                If String.Compare(�б�ǩ����(I).��ǩ, ��ǩ) = 0 Then
                    ��ֵ���� = �б�ǩ����(I).����
                    ��ʼ���� = I + 1
                    Return True
                End If
            Next
            If ��ʼ���� > 0 Then
                For I = 0 To ��ʼ���� - 1
                    If String.Compare(�б�ǩ����(I).��ǩ, ��ǩ) = 0 Then
                        ��ֵ���� = �б�ǩ����(I).����
                        ��ʼ���� = I + 1
                        Return True
                    End If
                Next
            End If
        End If
        Return False
    End Function

    Public Function ��ȡ_�ظ���ǩ(ByVal ��ǩ As String) As Object()
        If �ޱ�ǩ = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        If �������� > 0 Then
            Dim ����(�������� - 1) As Integer
            Dim I, J As Integer
            For I = 0 To �������� - 1
                If String.Compare(�б�ǩ����(I).��ǩ, ��ǩ) = 0 Then
                    ����(J) = I
                    J += 1
                End If
            Next
            If J > 0 Then
                Dim ����(J - 1) As Object
                For I = 0 To J - 1
                    ����(I) = �б�ǩ����(����(I)).����
                Next
                Return ����
            End If
        End If
        Return Nothing
    End Function

    Public ReadOnly Property �б�ǩ��������() As Integer
        Get
            If �ޱ�ǩ = False Then
                Return ��������
            Else
                Throw New Exception("��Ϊ�ޱ�ǩSS��")
            End If
        End Get
    End Property

    Public ReadOnly Property δ�����ݳ���() As Integer
        Get
            If Ҫ�����SS�� IsNot Nothing Then
                Return Ҫ�����SS��.Length - �Ѷ�ȡ�ֽ���
            Else
                Return 0
            End If
        End Get
    End Property

    Public ReadOnly Property �Ѷ����ݳ���() As Integer
        Get
            Return �Ѷ�ȡ�ֽ���
        End Get
    End Property

    Public ReadOnly Property ��ѯ��� As ��ѯ���_��������
        Get
            If �ޱ�ǩ = False Then
                Dim ��ѯ���2 As ��ѯ���_��������
                ��ȡ_�б�ǩ(ð��_SS��������ǩ, ��ѯ���2, ��ѯ���_��������.��)
                Return ��ѯ���2
            Else
                Throw New Exception("��Ϊ�ޱ�ǩSS��")
            End If
        End Get
    End Property

    Public ReadOnly Property ������ʾ�ı� As String
        Get
            If �ޱ�ǩ = False Then
                Dim ������Ϣ2 As String = ""
                ��ȡ_�б�ǩ(��̾��_SS��������ǩ, ������Ϣ2)
                Return ������Ϣ2
            Else
                Throw New Exception("��Ϊ�ޱ�ǩSS��")
            End If
        End Get
    End Property

End Class
