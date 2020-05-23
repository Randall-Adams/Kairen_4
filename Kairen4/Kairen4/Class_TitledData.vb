Public Class Class_TitledData

    Private __currenttagname As String
    Private __tagsData As Object
    Public ReadOnly Property Name
        Get
            Return vtitle
        End Get
    End Property
    Private vtitle As String
    Private vlinesoftext As List(Of String)
    Public ReadOnly Property GetData As List(Of String)
        Get
            Return vlinesoftext
        End Get
    End Property
    Public ReadOnly Property GetData(ByVal _lineNumber) As String
        Get
            Return vlinesoftext.Item(_lineNumber)
        End Get
    End Property
    Public WriteOnly Property SetData As List(Of String)
        Set(value As List(Of String))
            vlinesoftext = RemoveDoublsFromListOfString(value) 'prevents double data
        End Set
    End Property
    Public WriteOnly Property SetData(ByVal _lineNumber As Integer) As String
        Set(value As String)
            If vlinesoftext.Contains(value) Then Exit Property 'prevents double data
            'if setting a line number that exists in the file already...
            If _lineNumber < vlinesoftext.Count Then
                vlinesoftext.Item(_lineNumber) = value
                Exit Property
            End If
            'if setting a new line number in the file...
            'adds the blank lines between the last line in the file and the new line in the file that's going to be added
            For i As Integer = vlinesoftext.Count + 1 To _lineNumber - 1
                vlinesoftext.Add("")
            Next
            vlinesoftext.Add(value)
        End Set
    End Property
    Public WriteOnly Property InsertData(ByVal _lineNumber As Integer) As String
        Set(value As String)
            If vlinesoftext.Contains(value) Then Exit Property 'prevents double data
            If _lineNumber <= vlinesoftext.Count Then
                vlinesoftext.Insert(_lineNumber - 1, value)
                Exit Property
            End If
            For i As Integer = vlinesoftext.Count + 1 To _lineNumber - 1
                vlinesoftext.Add("")
            Next
            vlinesoftext.Add(value)
            'vlinesoftext(_lineNumber) = value
        End Set
    End Property
    Public WriteOnly Property AppendData As String
        Set(value As String)
            If vlinesoftext.Contains(value) Then Exit Property 'prevents double data
            vlinesoftext.Add(value)
        End Set
    End Property
    Public WriteOnly Property RemoveData As String
        Set(value As String)
            vlinesoftext.Remove(value)
        End Set
    End Property
    Public WriteOnly Property RemoveLine As Integer
        Set(value As Integer)
            vlinesoftext.RemoveAt(value - 1)
        End Set
    End Property
    Public Sub ClearData()
        vlinesoftext = New List(Of String)
    End Sub
    Sub New(ByVal _title As String)
        vtitle = _title
        vlinesoftext = New List(Of String)
    End Sub
    Sub New(ByVal _title As String, ByVal _data As List(Of String))
        vtitle = _title
        vlinesoftext = RemoveDoublsFromListOfString(_data) 'prevents double data
    End Sub
    Sub New(ByVal _title As String, ByVal _data As String)
        vtitle = _title
        vlinesoftext = New List(Of String)
        vlinesoftext.Add(_data)
    End Sub

    Private Function RemoveDoublsFromListOfString(ByVal _list As List(Of String))
        Dim templist As New List(Of String) ' templist code is to remove doubles
        For Each item In _list
            If templist.Contains(item) = False Then
                templist.Add(item)
            End If
        Next
        Return templist
    End Function
End Class
