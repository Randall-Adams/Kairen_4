Public Class Form_DialogueDocumenter

    Dim dd As Class_TextFile
    Private SaveFolder As String
    Private MyParentForm As Form
    Sub New(ByRef _parentForm As Form)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SaveFolder = Program.Folders.CurrentAccount & "Dialogues\"
        If My.Computer.FileSystem.DirectoryExists(SaveFolder) = False Then My.Computer.FileSystem.CreateDirectory(SaveFolder)
        MyParentForm = _parentForm
    End Sub
    Private Sub Form_DialogueDocumenter_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim DialogueItems() As String = ReturnFilesFromFolder(SaveFolder)
        For Each item In DialogueItems
            ListBox1.Items.Add(item)
        Next
    End Sub
    Public Function ReturnFilesFromFolder(ByVal _lookInThisFolder As String, Optional ByVal _onlyReturnThisExtension As String = "") As String()
        If My.Computer.FileSystem.DirectoryExists(_lookInThisFolder) = False Then Return Nothing

        Dim _returnVar() As String
        Dim i As Integer = 0

        Dim di As New IO.DirectoryInfo(_lookInThisFolder)
        Dim diar1 As IO.FileInfo() = di.GetFiles()
        Dim dra As IO.FileInfo

        'list the names of all files in the specified directory
        If diar1.Length = 0 Then
            ReDim _returnVar(0)
            _returnVar(0) = ""
        ElseIf _onlyReturnThisExtension = "" Then
            For Each dra In diar1
                ReDim Preserve _returnVar(i)
                _returnVar(i) = dra.ToString()
                i = i + 1
            Next
        Else
            For Each dra In diar1
                If Microsoft.VisualBasic.Right(dra.ToString(), _onlyReturnThisExtension.Length) = _onlyReturnThisExtension Then
                    ReDim Preserve _returnVar(i)
                    _returnVar(i) = dra.ToString.Replace(_onlyReturnThisExtension, "")
                    i = i + 1
                End If
            Next
        End If

        Return _returnVar
    End Function

    Dim dialogues(-1) As String
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        'left
        If Label3.Text > 1 Then
            If dialogues.Count <= Label3.Text - 1 Then ReDim Preserve dialogues(Label3.Text - 1)
            dialogues(Label3.Text - 1) = TextBox1.Text
            Label3.Text = CInt(Label3.Text) - 1
            TextBox1.Clear()
            If dialogues(Label3.Text) IsNot Nothing Then
                TextBox1.Text = dialogues(Label3.Text - 1)
            End If
        End If

    End Sub
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        'right
        If dialogues.Count <= Label3.Text Then ReDim Preserve dialogues(Label3.Text)
        dialogues(Label3.Text - 1) = TextBox1.Text
        TextBox1.Clear()
        If dialogues.Count >= Label3.Text Then
            TextBox1.Text = dialogues(Label3.Text)
        End If
        Label3.Text = CInt(Label3.Text) + 1
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        If dialogues.Count <= Label3.Text - 1 Then ReDim Preserve dialogues(Label3.Text - 1)
        dialogues(Label3.Text - 1) = TextBox1.Text
        dd = New Class_TextFile(SaveFolder, TextBox3.Text & ".txt")
        dd.ReadFile()
        dd.SetTaggedDataLine("NPC Name", 1) = TextBox2.Text
        dd.SetTaggedDataLine("File Name", 1) = TextBox3.Text
        For i As Integer = 0 To dialogues.Count - 1
            dd.SetTaggedDataLine("Dialogue " & i + 1, 1) = dialogues(i)
        Next
        dd.Save()
        Dim DialogueItems() As String = ReturnFilesFromFolder(SaveFolder)
        ListBox1.Items.Clear()
        For Each item In DialogueItems
            ListBox1.Items.Add(item)
        Next
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        Label3.Text = 1
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        dd = New Class_TextFile(SaveFolder, ListBox1.SelectedItem)
        dd.ReadFile()
        ReDim dialogues(-1)
        For Each item In dd.GetListOfTags
            If item = "NPC Name" Then
                TextBox2.Text = dd.GetTaggedDataLine(item, 1)
            ElseIf item = "File Name" Then
                TextBox3.Text = dd.GetTaggedDataLine(item, 1)
            Else
                ReDim Preserve dialogues(dialogues.Length)
                dialogues(dialogues.Length - 1) = dd.GetTaggedDataLine(item, 1)
            End If
        Next
        TextBox1.Text = dd.GetTaggedDataLine("Dialogue 1", 1)
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        Dim path As String = "C:\Windows\Explorer.exe"
        Dim arg As String = SaveFolder
        Dim myProcess As New Process
        myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
        myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
        myProcess.StartInfo.Arguments = arg
        myProcess.Start()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBox1.CheckedChanged
        TopMost = CheckBox1.Checked
    End Sub

    Private Sub Form_DialogueDocumenter_FormClosing(sender As System.Object, e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        MyParentForm.Visible = True
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBox2.CheckedChanged
        MyParentForm.Visible = Not CheckBox2.Checked
    End Sub
End Class