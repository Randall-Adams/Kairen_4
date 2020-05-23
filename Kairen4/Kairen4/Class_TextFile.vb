Public Class Class_TextFile
    Private vFolderLocation As String
    Private vFileName As String
    Public Event FileLoaded()
    Public Event FileSaved()
    Public Event FileChanged()
    'maybe add optional parameters to suppress events
    Private ReadOnly Property FullFileName As String
        Get
            Return vFolderLocation & vFileName
        End Get
    End Property
    Private vTaggedData As List(Of Class_TitledData)
    Private vfileContents_raw As New List(Of String)
    Sub New(ByVal _folderLocation As String, ByVal _fileName As String, Optional ByVal AutoReadFile As Boolean = False)
        vFolderLocation = _folderLocation
        vFileName = _fileName
        If AutoReadFile Then ReadFile()
    End Sub
    Public Sub ReadFile()
        vfileContents_raw = New List(Of String)
        If My.Computer.FileSystem.FileExists(FullFileName) Then
            Dim sr As New IO.StreamReader(FullFileName)
            Do Until sr.EndOfStream
                vfileContents_raw.Add(sr.ReadLine)
            Loop
            sr.Close()
        End If
        ParseTagSections()
        RaiseEvent FileLoaded()
    End Sub
    Public Sub Save()
        If My.Computer.FileSystem.DirectoryExists(vFolderLocation) = False Then
            My.Computer.FileSystem.CreateDirectory(vFolderLocation)
            If My.Computer.FileSystem.DirectoryExists(vFolderLocation) = False Then
                Exit Sub
            End If
        End If
        Dim sw As New IO.StreamWriter(FullFileName)
        If vTaggedData IsNot Nothing Then
            For i As Integer = 0 To vTaggedData.Count - 1
                sw.Write("[Start] " & vTaggedData.Item(i).Name)
                sw.WriteLine()
                For Each item In vTaggedData.Item(i).GetData
                    sw.Write(item)
                    sw.WriteLine()
                Next
                sw.Write("[End] " & vTaggedData.Item(i).Name)
                If i < vTaggedData.Count - 1 Then
                    sw.WriteLine()
                End If
            Next
        End If
        sw.Close()
        ReadFile()
        '   ParseTagSections()
        RaiseEvent FileSaved()
    End Sub

    Private vListOfTags As List(Of String)
    Public ReadOnly Property GetListOfTags() As List(Of String)
        Get
            Return vListOfTags
        End Get
    End Property

    Public ReadOnly Property GetTaggedData(ByVal _tag As String) As List(Of String)
        Get
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    Return vTaggedData.Item(i).GetData
                End If
            Next
            Return New List(Of String)
        End Get
    End Property
    Public ReadOnly Property GetTaggedDataLine(ByVal _tag As String, Optional ByVal _lineNumber As Integer = 0) As String
        Get
            If vTaggedData Is Nothing Then Return Nothing
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    If vTaggedData.Item(i).GetData.Count = 0 Then
                        Return Nothing
                    Else
                        If _lineNumber = 0 Then
                            Return vTaggedData.Item(i).GetData(0)
                        Else
                            Return vTaggedData.Item(i).GetData(_lineNumber - 1)
                        End If
                    End If
                End If
            Next
            Return Nothing
        End Get
    End Property

    Public WriteOnly Property SetTaggedData(ByVal _tag As String) As List(Of String)
        Set(value As List(Of String))
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    vTaggedData.Item(i).SetData = value
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(_tag, value))
            RaiseEvent FileChanged()
        End Set
    End Property
    Public WriteOnly Property SetTaggedDataLine(ByVal _tag As String, Optional ByVal _lineNumber As Integer = 0) As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    If _lineNumber = 0 Then
                        Dim td As New List(Of String)
                        td.Add(value)
                        vTaggedData.Item(i).SetData = td
                    Else
                        vTaggedData.Item(i).SetData(_lineNumber - 1) = value
                    End If
                    'Dim temp As New List(Of String)
                    'temp.Add(value)
                    'vTaggedData.Item(i).SetData = temp
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(_tag, value))
            RaiseEvent FileChanged()
        End Set
    End Property
    Public WriteOnly Property InsertTaggedData(ByVal _tag As String, ByVal _lineNumber As Integer) As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    vTaggedData.Item(i).InsertData(_lineNumber) = value
                    'Dim temp As New List(Of String)
                    'temp.Add(value)
                    'vTaggedData.Item(i).SetData = temp
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(_tag, value))
            RaiseEvent FileChanged()
        End Set
    End Property
    Public WriteOnly Property AddToTaggedData(ByVal _tag As String) As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    If vTaggedData.Item(i).Name = _tag Then
                        vTaggedData.Item(i).AppendData() = value
                    End If
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(_tag, value))
            RaiseEvent FileChanged()
        End Set
    End Property

    Public WriteOnly Property RemoveTaggedDataLine(ByVal _tag As String) As Integer
        Set(value As Integer)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    vTaggedData.Item(i).RemoveLine = value
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
        End Set
    End Property
    Public WriteOnly Property RemoveDataFromTag(ByVal _tag As String) As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    vTaggedData.Item(i).RemoveData = value
                    RaiseEvent FileChanged()
                    Exit Property
                    Dim temp As List(Of String) = GetTagsData(_tag)
                    If temp.Contains(value) Then
                        temp.RemoveAt(value)
                        vTaggedData(i).SetData = temp
                    End If
                    Exit Property
                End If
            Next
        End Set
    End Property

    Public WriteOnly Property RemoveTag() As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = value Then
                    vTaggedData.RemoveAt(i)
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
        End Set
    End Property

    Public WriteOnly Property AddTagToFile() As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = value Then
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(value))
            RaiseEvent FileChanged()
        End Set
    End Property

    Public WriteOnly Property ReplaceTaggedData(ByVal _tag As String, ByVal _oldData As String) As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = _tag Then
                    Dim td As List(Of String) = vTaggedData.Item(i).GetData
                    For i2 As Integer = 0 To td.Count
                        If td.Item(i2) = _oldData Then
                            td.Item(i2) = value
                            vTaggedData.Item(i).SetData = td
                            RaiseEvent FileChanged()
                            Exit Property
                        End If
                    Next
                End If
            Next
            vTaggedData.Add(New Class_TitledData(_tag, value))
            RaiseEvent FileChanged()
        End Set
    End Property

    Public WriteOnly Property ClearTagsData As String
        Set(value As String)
            For i As Integer = 0 To vTaggedData.Count - 1
                If vTaggedData.Item(i).Name = value Then
                    vTaggedData.Item(i).ClearData()
                    RaiseEvent FileChanged()
                    Exit Property
                End If
            Next
            vTaggedData.Add(New Class_TitledData(value))
            RaiseEvent FileChanged()
        End Set
    End Property

    Private Function ParseTagSections()
        vTaggedData = New List(Of Class_TitledData)
        vListOfTags = New List(Of String)
        Dim irawdata As Integer = 0 'index for raw data position
        Dim icurrenttagsend As Integer = 0 'index for raw data position
        Dim currenttagname As String
        Dim TagsData As Object
        Do While irawdata < vfileContents_raw.Count
            If ThisIsATagStarter(vfileContents_raw.Item(irawdata)) Then
                currenttagname = GetTagFromTagLine(vfileContents_raw.Item(irawdata))
                icurrenttagsend = GetTagsEnd(currenttagname, irawdata + 1)
                If icurrenttagsend < 0 Then
                    'error current tag's end not found
                    Return -2
                Else
                    If vListOfTags.Contains(currenttagname) Then 'ThisTagAlreadyExists Then
                        'error tag appears more than once in file
                        Return -3
                    Else
                        'this tag does not exist already
                        vListOfTags.Add(currenttagname)
                        TagsData = GetTagsData(irawdata)
                        If TypeOf (TagsData) Is Integer Then
                            'error in gettagsdata detected
                            '
                            'tagsdata is not a list right now
                            '
                            Return -4
                        Else
                            'tags data gathered successfully
                            Dim tv As List(Of String) = TagsData
                            vTaggedData.Add(New Class_TitledData(currenttagname, tv))
                            irawdata = icurrenttagsend + 1
                            icurrenttagsend = Nothing
                            currenttagname = Nothing
                            TagsData = Nothing
                        End If
                    End If
                End If
            Else
                'error not a tag starter
                Return -1
            End If
        Loop
        Return 0
    End Function
    Private Function GetTagsData(ByVal _tagsIndex As Integer)
        '_tagsindex is index where thee start tag is
        'first, check if a tag starts
        Dim thistag As String
        Dim thistagsendindex As Integer 'index where the close tag is
        If ThisIsATagStarter(vfileContents_raw.Item(_tagsIndex)) Then
            'proper tag start detected (this is a tag start)
            thistag = GetTagFromTagLine(vfileContents_raw.Item(_tagsIndex))
            thistagsendindex = GetTagsEnd(thistag, _tagsIndex + 1)
            If thistagsendindex < 0 Then
                Return -2 'proper tag end not detected (more details in GetTagsEnd's error code)
            Else
                'this tag's end found
                Dim tagsData As New List(Of String)
                Dim i As Integer = _tagsIndex + 1 'index where data starts
                Dim i2 As Integer = thistagsendindex
                Do While i < i2
                    tagsData.Add(vfileContents_raw.Item(i))
                    i += 1
                Loop
                Return tagsData
            End If
        Else
            'proper tag start not detected (the first line isn't a tag start)
            Return -1
        End If
    End Function
    Private Function GetTagsEnd(ByVal _tag As String, ByVal _startIndex As Integer)
        Dim i As Integer = _startIndex
        Do Until i = vfileContents_raw.Count ' + 1
            If ThisIsATagStarter(vfileContents_raw.Item(i)) Then
                Return -2 'improper tag start detected (a tag has started before the current tag has ended)
            Else
                If ThisIsATagEnder(vfileContents_raw.Item(i)) Then
                    If GetTagFromTagLine(vfileContents_raw.Item(i)) = _tag Then
                        'proper tag close detected (the tag ending now is the currently opened tag)
                        Return i
                    Else
                        Return -3 'improper tag end detected (another tag has ended before the current tag has ended)
                    End If
                End If
            End If
            i += 1
        Loop
        Return -1 'this file ends before anymore tags open or close
    End Function
    Private Function GetTagFromTagLine(ByVal _tagLineToGetTagFrom) As String
        If ThisIsATagStarter(_tagLineToGetTagFrom) Then
            Return Right(_tagLineToGetTagFrom, _tagLineToGetTagFrom.length - 8)
        ElseIf ThisIsATagEnder(_tagLineToGetTagFrom) Then
            Return Right(_tagLineToGetTagFrom, _tagLineToGetTagFrom.length - 6)
        Else
            Return ""
        End If
    End Function

    Private Function ThisIsATagStarter(ByVal _lineToCheck As String) As Boolean
        If Left(_lineToCheck, 8) = "[Start] " Then Return True Else Return False
    End Function
    Private Function ThisIsATagEnder(ByVal _lineToCheck As String) As Boolean
        If Left(_lineToCheck, 6) = "[End] " Then Return True Else Return False
    End Function
    Private Function ThisIsATagStarterOrEnder(ByVal _lineToCheck As String) As Boolean
        If ThisIsATagStarter(_lineToCheck) Or ThisIsATagEnder(_lineToCheck) Then Return True Else Return False
    End Function
End Class
