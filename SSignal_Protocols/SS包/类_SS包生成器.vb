Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Security.Cryptography
Imports System.IO

Public Class ��_SS��������

    Private Structure �ֽ�����_��������
        Dim �ֽ�����() As Byte
        Dim ������Ϣ�ֽ��� As ������Ϣ�ֽ���_��������
    End Structure

    Friend SS������() As SS������_��������
    Friend SS���������� As Integer
    Dim �ޱ�ǩ2 As Boolean
    Dim SS������ As SS������_��������
    Dim ���� As Encoding
    Protected ��ѯ���1 As ��ѯ���_�������� = ��ѯ���_��������.��

    Public Sub New(Optional ByVal �ޱ�ǩ1 As Boolean = False, Optional ByVal ��������1 As Integer = 0, Optional ByVal SS������1 As SS������_�������� = SS������_��������.Unicode_LittleEndian)
        ��ѯ���1 = ��ѯ���_��������.��
        Call ��ʼ��(�ޱ�ǩ1, ��������1, SS������1)
    End Sub

    Public Sub New(ByVal ��ѯ���2 As ��ѯ���_��������, Optional ByVal ��������1 As Integer = 0, Optional ByVal SS������1 As SS������_�������� = SS������_��������.Unicode_LittleEndian)
        ��ѯ���1 = ��ѯ���2
        Call ��ʼ��(False, ��������1, SS������1)
        Call ���_�б�ǩ(ð��_SS��������ǩ, ��ѯ���1)
    End Sub

    Public Sub New(ByVal ������Ϣ As String, Optional ByVal SS������1 As SS������_�������� = SS������_��������.Unicode_LittleEndian)
        ��ѯ���1 = ��ѯ���_��������.����
        Call ��ʼ��(False, 2, SS������1)
        Call ���_�б�ǩ(ð��_SS��������ǩ, ��ѯ���1)
        Call ���_�б�ǩ(��̾��_SS��������ǩ, ������Ϣ)
    End Sub

    Protected Sub ��ʼ��(ByVal �ޱ�ǩ1 As Boolean, ByVal ��������1 As Integer, ByVal SS������1 As SS������_��������)
        �ޱ�ǩ2 = �ޱ�ǩ1
        If ��������1 < 1 Then ��������1 = 10
        ReDim SS������(��������1 - 1)
        SS������ = SS������1
        Select Case SS������
            Case SS������_��������.Unicode_LittleEndian : ���� = Unicode
            Case SS������_��������.Unicode_BigEndian : ���� = BigEndianUnicode
            Case SS������_��������.UTF8 : ���� = UTF8
            Case SS������_��������.ASCII : ���� = ASCII
            Case SS������_��������.UTF32 : ���� = UTF32
            Case SS������_��������.UTF7 : ���� = UTF7
            Case Else : SS������ = SS������_��������.UTF8 : ���� = UTF8
        End Select
    End Sub

    Public ReadOnly Property ��ѯ��� As ��ѯ���_��������
        Get
            If ��ѯ���1 = ��ѯ���_��������.�� Then
                Throw New Exception("��SS��û�в�ѯ�����")
            Else
                Return ��ѯ���1
            End If
        End Get
    End Property

    Public ReadOnly Property ������ʾ�ı� As String
        Get
            If ��ѯ���1 = ��ѯ���_��������.���� Then
                Dim I As Integer
                For I = 0 To SS���������� - 1
                    If String.Compare(SS������(I).��ǩ, ��̾��_SS��������ǩ) = 0 Then
                        Return SS������(I).����
                    End If
                Next
            End If
            Return ""
        End Get
    End Property

    Public ReadOnly Property ������ As Integer
        Get
            Return SS����������
        End Get
    End Property

    Public ReadOnly Property �ޱ�ǩ As Boolean
        Get
            Return �ޱ�ǩ2
        End Get
    End Property

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal ���ֵ As Boolean)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.���ֵ, ���ֵ, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal ���ֵ As Boolean)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.���ֵ, ���ֵ)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �ֽ� As Byte)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�ֽ�, �ֽ�, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �ֽ� As Byte)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.�ֽ�, �ֽ�)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �з��Ŷ����� As Short)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�з��Ŷ�����, �з��Ŷ�����, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �з��Ŷ����� As Short)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.�з��Ŷ�����, �з��Ŷ�����)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �з������� As Integer)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�з�������, �з�������, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �з������� As Integer)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.�з�������, �з�������)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �з��ų����� As Long)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�з��ų�����, �з��ų�����, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �з��ų����� As Long)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.�з��ų�����, �з��ų�����)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �����ȸ����� As Single)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�����ȸ�����, �����ȸ�����, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �����ȸ����� As Single)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.�����ȸ�����, �����ȸ�����)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal ˫���ȸ����� As Double)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.˫���ȸ�����, ˫���ȸ�����, ��ǩ)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal ˫���ȸ����� As Double)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        Call ���(SS����������_��������.˫���ȸ�����, ˫���ȸ�����)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �ַ��� As String)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�ַ���, �ַ���, ��ǩ, ������Ϣ�ֽ���_��������.���ֽ�)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �ַ��� As String, Optional ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_�������� = ������Ϣ�ֽ���_��������.���ֽ�)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If ������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ� AndAlso String.IsNullOrEmpty(�ַ���) Then Throw New Exception("������Ϣ�ֽ�������Ϊ��")
        Call ���(SS����������_��������.�ַ���, �ַ���, , ������Ϣ�ֽ���)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal �ֽ�����() As Byte)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.�ֽ�����, �ֽ�����, ��ǩ, ������Ϣ�ֽ���_��������.���ֽ�)
    End Sub

    Public Sub ���_�ޱ�ǩ(ByVal �ֽ�����() As Byte, Optional ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_�������� = ������Ϣ�ֽ���_��������.���ֽ�)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If ������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ� AndAlso �ֽ����� Is Nothing Then Throw New Exception("������Ϣ�ֽ�������Ϊ��")
        Call ���(SS����������_��������.�ֽ�����, �ֽ�����, , ������Ϣ�ֽ���)
    End Sub

    Public Sub ���_�б�ǩ(ByVal ��ǩ As String, ByVal SS�������� As ��_SS��������)
        If �ޱ�ǩ2 = True Then Throw New Exception("��Ϊ�ޱ�ǩSS��")
        Call ���(SS����������_��������.��SS��, SS��������, ��ǩ, ������Ϣ�ֽ���_��������.���ֽ�)
    End Sub

    Public Sub ���_������(ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_��������)
        If �ޱ�ǩ2 = False Then Throw New Exception("��Ϊ�б�ǩSS��")
        If ������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ� Then Throw New Exception("������Ϣ�ֽ�������Ϊ��")
        Call ���(SS����������_��������.��, Nothing, , ������Ϣ�ֽ���)
    End Sub

    Private Sub ���(ByVal �������� As SS����������_��������, ByVal ���� As Object,
                   Optional ByVal ��ǩ As String = Nothing, Optional ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_�������� = ������Ϣ�ֽ���_��������.���ֽ�)
        If SS���������� = SS������.Length Then ReDim Preserve SS������(SS���������� * 2 - 1)
        With SS������(SS����������)
            .���� = ��������
            .���� = ����
            .������Ϣ�ֽ��� = ������Ϣ�ֽ���
            .��ǩ = ��ǩ
        End With
        SS���������� += 1
    End Sub

    Public Function ����SS��(ByVal ���������� As Socket, Optional ByVal AES������ As ICryptoTransform = Nothing) As Boolean
        Dim �ֽ�����() As Byte = ����SS��(AES������)
        If �ֽ����� IsNot Nothing Then
            Dim SS�������� As New ��_SS��������(True)
            SS��������.���_�ޱ�ǩ(CByte(AscW(SS���ߵ�λ��ʶ_L)))
            SS��������.���_�ޱ�ǩ(�ֽ�����, ������Ϣ�ֽ���_��������.���ֽ�)
            �ֽ����� = SS��������.�����ֽ�����
            If ����������.Send(�ֽ�����) = �ֽ�����.Length Then Return True
        End If
        Return False
    End Function

    Public Function ����SS��(Optional ByVal AES������ As ICryptoTransform = Nothing) As Byte()
        If SS���������� = 0 Then Return Nothing
        Dim �ֽ���������() As �ֽ�����_�������� = ת��()
        Dim I, �������ܳ��� As Integer
        For I = 0 To �ֽ���������.Length - 1
            With �ֽ���������(I)
                Select Case .������Ϣ�ֽ���
                    Case ������Ϣ�ֽ���_��������.���ֽ�
                        �������ܳ��� += .�ֽ�����.Length
                    Case ������Ϣ�ֽ���_��������.���ֽ�, ������Ϣ�ֽ���_��������.���ֽ�
                        �������ܳ��� += .������Ϣ�ֽ��� + .�ֽ�����.Length
                    Case Else : Return Nothing
                End Select
            End With
        Next
        Dim �ֽ�����(SS����ʶ_�б�ǩ.Length + 5 + �������ܳ���) As Byte
        Dim �ֽ�����1() As Byte
        If �ޱ�ǩ2 = False Then
            �ֽ�����1 = ASCII.GetBytes(SS����ʶ_�б�ǩ)
        Else
            �ֽ�����1 = ASCII.GetBytes(SS����ʶ_�ޱ�ǩ)
        End If
        Dim J As Integer
        Array.Copy(�ֽ�����1, 0, �ֽ�����, J, �ֽ�����1.Length)
        J += �ֽ�����1.Length
        �ֽ�����(J) = CByte(AscW(SS���ߵ�λ��ʶ_L))
        J += 1
        �ֽ�����(J) = SS������
        J += 1
        If �ޱ�ǩ2 = False Then
            �ֽ�����1 = BitConverter.GetBytes(CInt(�ֽ���������.Length / 3))
        Else
            �ֽ�����1 = BitConverter.GetBytes(�������ܳ���)
        End If
        Array.Copy(�ֽ�����1, 0, �ֽ�����, J, �ֽ�����1.Length)
        J += �ֽ�����1.Length
        For I = 0 To �ֽ���������.Length - 1
            With �ֽ���������(I)
                If .������Ϣ�ֽ��� <> ������Ϣ�ֽ���_��������.���ֽ� Then
                    Select Case .������Ϣ�ֽ���
                        Case ������Ϣ�ֽ���_��������.���ֽ� : �ֽ�����1 = BitConverter.GetBytes(CShort(.�ֽ�����.Length))
                        Case ������Ϣ�ֽ���_��������.���ֽ� : �ֽ�����1 = BitConverter.GetBytes(.�ֽ�����.Length)
                        Case Else : Continue For
                    End Select
                    Array.Copy(�ֽ�����1, 0, �ֽ�����, J, �ֽ�����1.Length)
                    J += �ֽ�����1.Length
                End If
                Array.Copy(.�ֽ�����, 0, �ֽ�����, J, .�ֽ�����.Length)
                J += .�ֽ�����.Length
            End With
        Next
        If AES������ Is Nothing Then
            Return �ֽ�����
        Else
            Return AES����(�ֽ�����, AES������)
        End If
    End Function

    Public Function �����ֽ�����(Optional ByVal ָ������ As Integer = 0) As Byte()
        If ��ѯ���1 <> ��ѯ���_��������.�� Then
            Throw New Exception("��Ϊ��ѯ�����SS�������������ֽ����顣������SS����")
        End If
        If SS���������� = 0 Then Return Nothing
        Dim �ֽ���������() As �ֽ�����_�������� = ת��()
        Dim I, �ܳ��� As Integer
        For I = 0 To �ֽ���������.Length - 1
            With �ֽ���������(I)
                Select Case .������Ϣ�ֽ���
                    Case ������Ϣ�ֽ���_��������.���ֽ�
                        �ܳ��� += .�ֽ�����.Length
                    Case ������Ϣ�ֽ���_��������.���ֽ�, ������Ϣ�ֽ���_��������.���ֽ�
                        �ܳ��� += .������Ϣ�ֽ��� + .�ֽ�����.Length
                    Case Else : Return Nothing
                End Select
            End With
        Next
        If ָ������ > 0 Then If �ܳ��� < ָ������ Then �ܳ��� = ָ������
        Dim �ֽ�����(�ܳ��� - 1) As Byte
        Dim �ֽ�����1() As Byte
        Dim J As Integer
        For I = 0 To �ֽ���������.Length - 1
            If �ֽ���������(I).������Ϣ�ֽ��� <> ������Ϣ�ֽ���_��������.���ֽ� Then
                Select Case �ֽ���������(I).������Ϣ�ֽ���
                    Case ������Ϣ�ֽ���_��������.���ֽ� : �ֽ�����1 = BitConverter.GetBytes(CShort(�ֽ���������(I).�ֽ�����.Length))
                    Case ������Ϣ�ֽ���_��������.���ֽ� : �ֽ�����1 = BitConverter.GetBytes(�ֽ���������(I).�ֽ�����.Length)
                    Case Else : Continue For
                End Select
                Array.Copy(�ֽ�����1, 0, �ֽ�����, J, �ֽ�����1.Length)
                J += �ֽ�����1.Length
            End If
            Array.Copy(�ֽ���������(I).�ֽ�����, 0, �ֽ�����, J, �ֽ���������(I).�ֽ�����.Length)
            J += �ֽ���������(I).�ֽ�����.Length
        Next
        Return �ֽ�����
    End Function

    Private Function ת��() As �ֽ�����_��������()
        Dim ����() As �ֽ�����_��������
        Dim I As Integer
        If �ޱ�ǩ2 = False Then
            ReDim ����(SS���������� * 3 - 1)
            Dim J As Integer
            For I = 0 To SS���������� - 1
                With SS������(I)
                    ����(J) = ת�����ֽ�����(.��ǩ, SS����������_��������.�ַ���, ������Ϣ�ֽ���_��������.���ֽ�)
                    J += 1
                    ����(J) = ת�����ֽ�����(.����, SS����������_��������.�ֽ�, ������Ϣ�ֽ���_��������.���ֽ�)
                    J += 1
                    ����(J) = ת�����ֽ�����(.����, .����, .������Ϣ�ֽ���)
                    J += 1
                End With
            Next
        Else
            ReDim ����(SS���������� - 1)
            For I = 0 To SS���������� - 1
                With SS������(I)
                    ����(I) = ת�����ֽ�����(.����, .����, .������Ϣ�ֽ���)
                End With
            Next
        End If
        Return ����
    End Function

    Private Function ת�����ֽ�����(ByVal ��ת������ As Object, ByVal �������� As SS����������_��������,
                             ByVal ������Ϣ�ֽ��� As ������Ϣ�ֽ���_��������) As �ֽ�����_��������
        Dim ���� As New �ֽ�����_��������
        ����.������Ϣ�ֽ��� = ������Ϣ�ֽ���
        Select Case ��������
            Case SS����������_��������.�ַ���
                If String.IsNullOrEmpty(��ת������) = False Then
                    ����.�ֽ����� = ����.GetBytes(CStr(��ת������))
                Else
                    ����.�ֽ����� = New Byte(������Ϣ�ֽ��� - 1) {}
                    ����.������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                End If
            Case SS����������_��������.�з��ų�����
                ����.�ֽ����� = BitConverter.GetBytes(CLng(��ת������))
            Case SS����������_��������.�з�������
                ����.�ֽ����� = BitConverter.GetBytes(CInt(��ת������))
            Case SS����������_��������.�з��Ŷ�����
                ����.�ֽ����� = BitConverter.GetBytes(CShort(��ת������))
            Case SS����������_��������.��SS��
                If ��ת������ IsNot Nothing Then
                    ����.�ֽ����� = CType(��ת������, ��_SS��������).����SS��
                    If ����.�ֽ����� Is Nothing Then GoTo ��ת��1
                Else
��ת��1:
                    ����.�ֽ����� = New Byte(������Ϣ�ֽ��� - 1) {}
                    ����.������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                End If
            Case SS����������_��������.�ֽ�����
                If ��ת������ IsNot Nothing Then
                    ����.�ֽ����� = ��ת������
                Else
                    ����.�ֽ����� = New Byte(������Ϣ�ֽ��� - 1) {}
                    ����.������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
                End If
            Case SS����������_��������.���ֵ
                If CBool(��ת������) = False Then
                    ����.�ֽ����� = New Byte() {0}
                Else
                    ����.�ֽ����� = New Byte() {1}
                End If
            Case SS����������_��������.�ֽ�
                ����.�ֽ����� = New Byte() {��ת������}
            Case SS����������_��������.�����ȸ�����
                ����.�ֽ����� = BitConverter.GetBytes(CSng(��ת������))
            Case SS����������_��������.˫���ȸ�����
                ����.�ֽ����� = BitConverter.GetBytes(CDbl(��ת������))
            Case SS����������_��������.��
                ����.�ֽ����� = New Byte(������Ϣ�ֽ��� - 1) {}
                ����.������Ϣ�ֽ��� = ������Ϣ�ֽ���_��������.���ֽ�
        End Select
        Return ����
    End Function

    Public Sub �ܷ����ɴ��ı�()
        If �ޱ�ǩ2 = True Then Throw New Exception("�ޱ�ǩSS���������ɴ��ı�")
        If SS���������� > 0 Then
            Dim I As Integer
            For I = 0 To SS���������� - 1
                With SS������(I)
                    If ��ǩ�Ƿ�ϸ�(.��ǩ) = False Then
                        Throw New Exception("��ǩ�����зǷ��ַ���")
                    End If
                    If .���� = SS����������_��������.��SS�� Then
                        CType(.����, ��_SS��������).�ܷ����ɴ��ı�()
                    End If
                End With
            Next
        End If
    End Sub

    Public Function ���ɴ��ı�(Optional ByVal �㼶 As Integer = 0) As String
        Call �ܷ����ɴ��ı�()
        If SS���������� > 0 Then
            Dim �䳤�ı� As New StringBuilder()
            Dim �ı�д���� As New StringWriter(�䳤�ı�)
            Dim �ո��ַ��� As String = Nothing
            If �㼶 > 0 Then
                �ո��ַ��� = Space(�㼶)
            Else
                If �㼶 < 0 Then �㼶 = 0
                �ı�д����.Write(SS����ʶ_���ı�)
            End If
            Dim I As Integer
            For I = 0 To SS���������� - 1
                With SS������(I)
                    �ı�д����.Write(vbLf)
                    If �㼶 > 0 Then �ı�д����.Write(�ո��ַ���)
                    Select Case .����
                        Case SS����������_��������.�ַ���
                            �ı�д����.Write("S")
                        Case SS����������_��������.�з��ų�����
                            �ı�д����.Write("8")
                        Case SS����������_��������.�з�������
                            �ı�д����.Write("4")
                        Case SS����������_��������.�з��Ŷ�����
                            �ı�д����.Write("2")
                        Case SS����������_��������.��SS��
                            �ı�д����.Write("SS")
                        Case SS����������_��������.�ֽ�����
                            �ı�д����.Write("BT")
                        Case SS����������_��������.���ֵ
                            �ı�д����.Write("BL")
                        Case SS����������_��������.�ֽ�
                            �ı�д����.Write("1")
                        Case SS����������_��������.�����ȸ�����
                            �ı�д����.Write("4F")
                        Case SS����������_��������.˫���ȸ�����
                            �ı�д����.Write("8F")
                    End Select
                    �ı�д����.Write(":")
                    �ı�д����.Write(.��ǩ)
                    �ı�д����.Write("=")
                    Select Case .����
                        Case SS����������_��������.�ַ���
                            If String.IsNullOrEmpty(.����) = False Then
                                �ı�д����.Write(CStr(.����).Replace("&", "&amp;").Replace(vbCr, "").Replace(vbLf, "&;"))
                            End If
                        Case SS����������_��������.�з��ų�����
                            �ı�д����.Write(CLng(.����))
                        Case SS����������_��������.�з�������
                            �ı�д����.Write(CInt(.����))
                        Case SS����������_��������.�з��Ŷ�����
                            �ı�д����.Write(CShort(.����))
                        Case SS����������_��������.��SS��
                            �ı�д����.Write(CType(.����, ��_SS��������).���ɴ��ı�(�㼶 + 1))
                        Case SS����������_��������.�ֽ�����
                            If .���� IsNot Nothing Then
                                �ı�д����.Write(System.Convert.ToBase64String(.����, Base64FormattingOptions.None))
                            End If
                        Case SS����������_��������.���ֵ
                            �ı�д����.Write(IIf(CBool(.����), "true", "false"))
                        Case SS����������_��������.�ֽ�
                            �ı�д����.Write(CByte(.����))
                        Case SS����������_��������.�����ȸ�����
                            �ı�д����.Write(CSng(.����))
                        Case SS����������_��������.˫���ȸ�����
                            �ı�д����.Write(CDbl(.����))
                    End Select
                End With
            Next
            �ı�д����.Close()
            Return �ı�д����.ToString
        Else
            Return ""
        End If
    End Function

    Private Function ��ǩ�Ƿ�ϸ�(ByVal ��ǩ As String) As Boolean
        If ��ǩ.StartsWith(" ") Then Return False
        Dim �Ƿ��ַ�() As String = New String() {"=", vbCr, vbLf}
        Dim I As Integer
        For I = 0 To �Ƿ��ַ�.Length - 1
            If ��ǩ.Contains(�Ƿ��ַ�(I)) Then Return False
        Next
        Return True
    End Function

End Class
