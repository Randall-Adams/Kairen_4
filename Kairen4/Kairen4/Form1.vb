Public Class Form1
    'Dim WithEvents UserSettings As Class_TextFile = Files.UserSettings
    Dim WithEvents LoadTimer As New Timer
    Dim WithEvents CheckVersionTimer As Timer
    Dim WithEvents DownloadUpdateTimer As Timer
    Dim WithEvents InstallUpdateTimer As Timer
    Dim LoadTimerTickOneHasHappened As Boolean = False
    Dim ProgramStartPromptNumber As Integer = -1
    Dim DebugIsAppData As Boolean = False
    Dim KairenLatestVersion As String
    Dim WebsiteDomainAddress As String = "http://eqrh.tk/"
    'Delegate Sub UpdateSettings
    'Public vUpdateSettings As UpdateSettings
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        If Environment.GetCommandLineArgs.Contains("OutputVersionAndExit") Then
            Dim sr As New IO.StreamWriter(Application.ExecutablePath & ".txt")
            sr.Write(ProgramVersionNumber)
            sr.Close()
            Environment.Exit(0)
        End If
        DebugIsAppData = Environment.GetCommandLineArgs.Contains("DebugIsAppData")
        Me.Width = 1285 'IDE display and run-time display do not match for me, so set run-time display now
        LoadTimer.Interval = 1000
    End Sub
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        If Environment.GetCommandLineArgs.Contains("DebugRelaunch") Then 'debug relaunch program from ide stuff
            AppActivate("Kairen4 - Microsoft Visual Basic 2010 Express")
            SendKeys.Send("{F5}")
            Environment.Exit(0)
        ElseIf Environment.GetCommandLineArgs.Contains("UpdateRename") Then
            My.Computer.FileSystem.CopyFile(Application.ExecutablePath, Folders.AppData & "Kairen.exe", True)
            Dim path As String = Folders.AppData & "Kairen.exe"
            'Dim arg As String = _folderToOpen
            Dim myProcess As New Process
            myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
            myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
            'myProcess.StartInfo.Arguments = arg
            myProcess.Start()
            Environment.Exit(0)
        End If
        Log = "Launched at " & TimeOfDay
        Log = "Version " & ProgramVersionNumber
        Log = "Launched From " & Application.ExecutablePath
        Log = "Installation Folder " & Folders.AppData
        If Application.ExecutablePath.ToLower <> Folders.AppData.ToLower & "Kairen.exe".ToLower Then
            'launch from non-installed location
            Log = "Launched From Non-Installed Location"
            If My.Computer.FileSystem.DirectoryExists(Folders.AppData) Then
                'install found - update path / cancel
                Log = "Installation Folder Present"
                If CheckInstalledVersion() Then
                    'launched version is same as installed version
                    If DebugIsAppData Then
                        'debug option - act as if this is running from the installed location, also copy this version to installed location
                        Log = "DebugIsAppData : Copying this build to AppData"
                        My.Computer.FileSystem.CopyFile(Application.ExecutablePath, Folders.AppData & "Kairen.exe", True)
                        Log = "DebugIsAppData : Proceeding as Normal"
                        ProgramStartPromptNumber = 0
                    Else
                        'launched installed version from non-installed location
                        Log = "Prompting for action"
                        ProgramStartPromptNumber = 2
                    End If
                Else
                    'launched version isn't same as installed version
                    Log = "Prompting to install version " & ProgramVersionNumber & " over present version"
                    ProgramStartPromptNumber = 3
                End If
            Else
                'no install found - new install path / cancel
                Log = "Installation Folder Not Found"
                Log = "Prompting Kairen Install"
                ProgramStartPromptNumber = 1
            End If
        Else
            Log = "Launched From Installed Location"
            'launch from installed location
            If CheckInstalledVersion() Then
                ProgramStartPromptNumber = 0
            Else
                'launched version isn't same as installed version
                Log = "Prompting to install version " & ProgramVersionNumber & " over present version"
                ProgramStartPromptNumber = 3
            End If
        End If

        'LoadTimer.Interval = 3000
        LoadTimer.Start()
    End Sub
    Private Function CheckInstalledVersion() As Boolean
        If Files.ProgramSettings.GetTaggedDataLine("Installed Version", 1) <> ProgramVersionNumber Then
            'launched version isn't same as installed version
            Log = "Installed Version Found = " & Files.ProgramSettings.GetTaggedDataLine("Installed Version", 1)
            Return False
        Else
            'launched version is same as installed version
            Log = "This Version of Kairen is currently installed (" & ProgramVersionNumber & ")"
            Return True
        End If
    End Function '*2
    Private WriteOnly Property Log() As String
        Set(value As String)
            If TextBox1.Text <> "" Then TextBox1.AppendText(Environment.NewLine)
            TextBox1.AppendText(value)
        End Set
    End Property '*2

    Private Sub LoadTimer_Tick() Handles LoadTimer.Tick
        If LoadTimerTickOneHasHappened = False Then
            'boot-up processing here
            LoadTimer.Stop()
            LoadTimerTickOneHasHappened = True
            Select Case ProgramStartPromptNumber
                Case 0
                    'launched from installed location
                    If GetFoldersInFolder(Folders.AccountsFolder).Count > 0 Then
                        Log = "Users Found:"
                        For Each item In GetFoldersInFolder(Folders.AccountsFolder)
                            Log = "   " & item
                        Next
                    Else
                        Log = "No User Profiles Exist"
                    End If
                    If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
                        Log = "Autoloading " & Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1)
                        LoadUserAccount(Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1))
                        Button6.Enabled = True
                    Else
                        Log = "No Autoload User Found"
                        Button3.Enabled = True
                        Button4.Enabled = True
                        Button9.Enabled = True
                    End If
                    LoadTimer.Start()
                Case 1
                    'no install detected, prompt to install this one new
                    If NewKairenInstallationPrompt() Then
                        If CurrentUserName = "" Then
                            Button4.Enabled = True
                        End If
                        LoadTimer.Start()
                    End If
                Case 2
                    'this install detected, prompt to re-install or continue/exit
                    If KairenReinstallationPrompt() = False Then
                        Log = "Program will proceed as normal"
                        ProgramStartPromptNumber = 0
                        LoadTimer.Start()
                    Else
                        Log = "Program will restart from Installed Location"
                        Log = "Restarting in 15 seconds. Hopefully that's enough time to read all of this if you cared q="
                        LoadTimerTickOneHasHappened = False
                        ProgramStartPromptNumber = 4
                        LoadTimer.Interval = 15000
                        LoadTimer.Start()
                    End If
                Case 3
                    'some not-this-install detected, prompt to install this one
                    If ThisKairenInstallationPrompt() = False Then
                        Log = "Only an installed Kairen Version may run. This program might not function properly otherwise, so there is nothing more you can do from here."
                    Else
                        If CurrentUserName = "" Then
                            Button3.Enabled = True
                            Button4.Enabled = True
                            Button9.Enabled = True
                        End If
                        ProgramStartPromptNumber = 0
                        LoadTimer.Start()
                    End If
                Case 4
                    'stem from case 2 - this relaunches kairen from install path after reinstall of current version
                    '(case 2 = 'this install detected, prompt to re-install or continue/exit)
                    Dim path As String = Folders.AppData & "Kairen.exe"
                    'Dim arg As String = _folderToOpen
                    Dim myProcess As New Process
                    myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
                    myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
                    'myProcess.StartInfo.Arguments = arg
                    myProcess.Start()
                    Environment.Exit(0)
            End Select
        Else
            'normal run-time processing here
            Select Case ProgramStartPromptNumber
                Case 0
                    'setup

                    ProgramStartPromptNumber = 1
                Case 1
                    'checks round 1
                    'check for pcsx2 'i got this done elsewhere, before this (filesettings load event)
                    'check for iso 'i got this done elsewhere, before this (filesettings load event)
                    'check for ce auto-attach 'done
                    'check for ce auto-load 'done

                    CheckCEAutoAttach()
                    CheckCEAutoLoad()
                    CheckLUA()
                    CheckCheatTables()
                    CheckGameData()
                    Button17.Enabled = True
                    ProgramStartPromptNumber = 2
                Case 2

                    ProgramStartPromptNumber = 3
                Case Else
                    ProgramStartPromptNumber += 1
            End Select
            If 0 > ProgramStartPromptNumber Then Log = "pspn = " & ProgramStartPromptNumber
        End If
    End Sub

    Private Function NewKairenInstallationPrompt() As Boolean
        If MsgBox("No Kairen (version 4) Installation Present. Would you like to install Kairen?" & Environment.NewLine & "This will install Kairen " & ProgramVersionNumber, MsgBoxStyle.YesNo, "Kairen: Install Kairen?") = MsgBoxResult.Yes Then
            Log = "User Chose to Install Kairen"
            InstallKairen()
            Return True
        Else
            Log = "User Chose Not to Install Kairen"
            Return False
        End If
    End Function '*2
    Private Function ThisKairenInstallationPrompt() As Boolean
        If MsgBox("Kairen Version 4, Version Number " & Files.ProgramSettings.GetTaggedDataLine("Installed Version", 1) & " is Presently Installed." & Environment.NewLine & _
                  "Would you like to install Kairen 4, Version " & ProgramVersionNumber & " in it's place?", MsgBoxStyle.YesNo, "Kairen: Update Kairen?") = MsgBoxResult.Yes Then
            Log = "User Chose to Update Kairen (" & Files.ProgramSettings.GetTaggedDataLine("Installed Version", 1) & " -----> " & ProgramVersionNumber & ")"
            InstallKairen(False)
            Return True
        Else
            Log = "User Chose Not to Update Kairen (" & Files.ProgramSettings.GetTaggedDataLine("Installed Version", 1) & " --/--> " & ProgramVersionNumber & ")"
            Return False
        End If
    End Function '*2
    Private Function KairenReinstallationPrompt() As Boolean
        If MsgBox("This Version of Kairen is " & ProgramVersionNumber & " and is currently installed." & Environment.NewLine & _
                  "You ran this program from the folder: " & Environment.NewLine & _
                  Application.ExecutablePath & Environment.NewLine & _
                  "Instead of it's installed location: " & Environment.NewLine & _
                  Folders.AppData & "Kairen.exe" & Environment.NewLine & _
                  " Did you want to Reinstall it?", _
                  MsgBoxStyle.YesNo, _
                  "Kairen: Reinstall Kairen?") _
              = MsgBoxResult.Yes Then
            Log = "User Chose to Reinstall Kairen " & ProgramVersionNumber
            ReinstallKairen()
            Return True
        Else
            Log = "User Chose Not to Reinstall Kairen " & ProgramVersionNumber
            Return False
        End If
    End Function '*2
    Private Sub ReinstallKairen()
        Log = "Reinstalling Kairen " & ProgramVersionNumber
        Log = "Starting Normal Installation Process"
        InstallKairen(False)
        Log = "Kairen Reinstallation Complete"
    End Sub '*2
    Private Sub InstallKairen(Optional ByVal _promptUserAndShortCutCreations As Boolean = True)
        Log = "Installing Kairen version " & ProgramVersionNumber
        CheckForAndCreateFolder("Installation", Folders.AppData) 'app data folder
        CheckForAndCreateFolder("Users", Folders.AccountsFolder)
        If Application.ExecutablePath <> Folders.AppData & "Kairen.exe" Then
            My.Computer.FileSystem.CopyFile(Application.ExecutablePath, Folders.AppData & "Kairen.exe", True)
            Log = "Kairen.exe Installed"
        End If
        Files.ProgramSettings.SetTaggedDataLine("Installed Version", 1) = ProgramVersionNumber
        Files.ProgramSettings.Save()
        Log = "Program Settings Updated"
        Log = "Kairen " & ProgramVersionNumber & " Installation Complete"
        If _promptUserAndShortCutCreations Then
            Log = "Prompting Desktop Shortcut Creation"
            If MsgBox("Create Desktop Shortcut?", MsgBoxStyle.YesNo, "Kairen: Installing Kairen") = MsgBoxResult.Yes Then
                CreateDesktopShortcut()
                Log = "Desktop Shortcut Made"
            Else
                Log = "Desktop Shortcut Not Created"
            End If
            CreateNewAccount()
        Else
            'copypastad from above
            If GetFoldersInFolder(Folders.AccountsFolder).Count > 0 Then
                Log = "Users Found:"
                For Each item In GetFoldersInFolder(Folders.AccountsFolder)
                    Log = "   " & item
                Next
            Else
                Log = "No User Profiles Exist"
            End If
            If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
                Log = "Autoloading " & Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1)
                LoadUserAccount(Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1))
                'Button6.Enabled = True
            Else
                Log = "No Autoload User Found"
                'Button3.Enabled = True
                'Button4.Enabled = True
                'Button9.Enabled = True
            End If
        End If

    End Sub '*2
    Sub CreateDesktopShortcut()
        'wsh = windows script host (shell) (object)
        Dim wsh As Object = CreateObject("WScript.Shell")
        wsh = CreateObject("WScript.Shell")
        Dim ShortcutObject, FolderPath
        ' Read desktop path using WshSpecialFolders object
        FolderPath = wsh.SpecialFolders("Desktop")
        ' Create a shortcut object on the desktop
        ShortcutObject = wsh.CreateShortcut(FolderPath & "\Kairen.lnk")
        ' Set shortcut object properties and save it
        ShortcutObject.TargetPath = wsh.ExpandEnvironmentStrings(Folders.AppData & "Kairen.exe")
        ShortcutObject.WorkingDirectory = wsh.ExpandEnvironmentStrings(Folders.AppData)
        ShortcutObject.WindowStyle = 4
        'Use this next line to assign a icon other then the default icon for the exe
        'ShortcutObject.IconLocation = WSHShell.ExpandEnvironmentStrings("path to a file with an embeded icon", icon index number)
        'Save the shortcut
        ShortcutObject.Save()
    End Sub '*2
    Private Function GetFoldersInFolder(ByVal _folderPath As String) As String()
        Dim _returnVar(-1) As String
        If My.Computer.FileSystem.DirectoryExists(_folderPath) = False Then Return _returnVar

        Dim i As Integer = 0

        Dim di As New IO.DirectoryInfo(_folderPath)
        Dim diar1 As IO.DirectoryInfo() = di.GetDirectories()
        Dim dra As IO.DirectoryInfo

        'list the names of all files in the specified directory
        If diar1.Length = 0 Then
            ReDim _returnVar(-1)
        Else
            For Each dra In diar1
                ReDim Preserve _returnVar(i)
                _returnVar(i) = dra.ToString.Replace(_folderPath, "")
                i = i + 1
            Next
        End If

        Return _returnVar
    End Function '*2
    Private Sub CheckForAndCreateFolder(ByVal _folderNameInLog As String, ByVal _folderPath As String)
        Log = _folderNameInLog & " Folder " & _folderPath
        If My.Computer.FileSystem.DirectoryExists(_folderPath) = False Then
            Log = _folderNameInLog & " Folder Not Found"
            My.Computer.FileSystem.CreateDirectory(_folderPath)
            Log = _folderNameInLog & " Folder Created"
        Else
            Log = _folderNameInLog & " Folder Present"
        End If
    End Sub '*2

    Private Function CreateNewAccount() As String 'Optional ByVal _userToCreate As String = "")
        Log = "Promptng New User Creation"
        Dim NewUserName As String
        Do
            NewUserName = ""
            NewUserName = InputBox("Choose your New User Name", "New User Creation").Trim
            If NewUserName <> "" Then
                Log = "New User Name = " & NewUserName
                If My.Computer.FileSystem.DirectoryExists(Folders.AccountsFolder & NewUserName & "\") Then
                    MsgBox("That User Name is already taken.", MsgBoxStyle.OkOnly)
                    Log = "Repromptng New User Creation - New User Name Taken"
                Else

                    Exit Do
                End If
            Else
                If MsgBox("No User Name provided. Cancel User Creation?", MsgBoxStyle.YesNo, "New User Creation") = MsgBoxResult.Yes Then
                    Log = "New User Creation Cancelled"
                    Return ""
                Else
                    Log = "Repromptng New User Creation - No Name Chosen"
                End If
            End If
        Loop

        Folders.CurrentAccount = Folders.AccountsFolder & NewUserName & "\"
        Log = "Current User Folder " & Folders.CurrentAccount
        My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount)
        Log = "Current User Folder Created"
        Files.UserSettings = New Class_TextFile(Folders.CurrentAccount, "Settings.txt")
        Files.UserSettings.ReadFile()
        Files.UserSettings.Save()
        CurrentUserName = NewUserName
        Log = "User Settings File Created " & Folders.CurrentAccount & "Settings.txt"
        Log = "User Created " & NewUserName
        Log = "Account Loaded " & NewUserName
        Button6.Enabled = True
        Button10.Enabled = True
        Label2.Text = CurrentUserName
        Return NewUserName
    End Function '*2
    Private Function DeleteAccount() As String
        Log = "Prompting Delete User"
        Dim DeletingUserName As String
        Do
            DeletingUserName = ""
            DeletingUserName = InputBox("Choose the User Name you want to delete", "Delete User").Trim
            If DeletingUserName <> "" Then
                Log = "User to Delete = " & DeletingUserName
                If My.Computer.FileSystem.DirectoryExists(Folders.AccountsFolder & DeletingUserName & "\") = False Then
                    MsgBox("That User Name does not exist.", MsgBoxStyle.OkOnly)
                    Log = "User Doesn't Exist to delete"
                    Return ""
                Else
                    My.Computer.FileSystem.DeleteDirectory(Folders.AccountsFolder & DeletingUserName, FileIO.DeleteDirectoryOption.DeleteAllContents)
                    Log = "Deleted User " & DeletingUserName
                    Return DeletingUserName
                End If
            Else
                If MsgBox("No User Name provided. Cancel User Deletion?", MsgBoxStyle.YesNo, "Delete User") = MsgBoxResult.Yes Then
                    Log = "User Deletion Cancelled"
                    Return ""
                Else
                    Log = "Repromptng User Deletion - No Name Chosen"
                End If
            End If
        Loop
    End Function '*2

    Private Sub LoadUserAccount(ByVal _accountToLoad As String)
        Label2.Text = _accountToLoad
        Button6.Enabled = True
        Button10.Enabled = True
        CurrentUserName = _accountToLoad
        Folders.CurrentAccount = Folders.AccountsFolder & _accountToLoad & "\"
        Files.UserSettings = New Class_TextFile(Folders.CurrentAccount, "UserSettings.txt", False)
        AddHandler Files.UserSettings.FileLoaded, AddressOf UpdateUserSettingsStuff
        Files.UserSettings.ReadFile()
        CheckCEAutoAttach()
        CheckLUA()
        CheckCheatTables()
        Log = "Account Loaded " & _accountToLoad
    End Sub '*2
    Private Sub UnloadUserAccount()
        RemoveHandler Files.UserSettings.FileLoaded, AddressOf UpdateUserSettingsStuff
        Files.UserSettings = Nothing
        Log = "User Logged Out " & Program.CurrentUserName
        Program.CurrentUserName = Nothing
        Folders.CurrentAccount = Nothing
        Files.UserSettings = Nothing
        Label2.Text = "Not logged in"
        Button3.Enabled = True
        Button4.Enabled = True
        Button9.Enabled = True
        Button10.Enabled = False
        TextBox2.Text = ""
        TextBox3.Text = ""
        TextBox4.Text = ""
        CheckBox1.Checked = False
        CheckBox3.Checked = False
        CheckBox4.Checked = False
    End Sub '*2
    Private Sub UpdateUserSettingsStuff() 'Handles UserSettings.FileLoaded
        'Log = "   -   UserSettings.FileLoaded Handled by UpdateUserSettingsStuff()   -   "
        If TextBox2.Text.Trim = "" Then TextBox2.Text = Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)
        If TextBox3.Text.Trim = "" Then TextBox3.Text = Files.UserSettings.GetTaggedDataLine("ISO Path", 1)
        If TextBox4.Text.Trim = "" Then TextBox4.Text = Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1)
        If TextBox5.Text.Trim = "" Then TextBox5.Text = Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1)
    End Sub '*?

    'dev buttons
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        OpenFolderLocation(Folders.AppData)
    End Sub
    Sub OpenFolderLocation(ByVal _folderToOpen As String)
        If My.Computer.FileSystem.DirectoryExists(_folderToOpen) Then
            Dim path As String = "C:\Windows\Explorer.exe"
            Dim arg As String = _folderToOpen
            Dim myProcess As New Process
            myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
            myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
            myProcess.StartInfo.Arguments = arg
            myProcess.Start()
        Else
            MsgBox("The " & Chr(34) & Folders.AppData & Chr(34) & " folder does not exist.", MsgBoxStyle.OkOnly, "The requested folder does not exist")
        End If
    End Sub
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        OpenFolderLocation(My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\Visual Studio 2010\Projects\Kairen4\Kairen4\bin\Debug")
    End Sub
    Private Sub Button8_MouseDown(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles Button8.MouseDown
        Files.ProgramSettings.SetTaggedDataLine("Installed Version", 1) = "SP00FIFYED"
        Files.ProgramSettings.Save()
        If e.Button = Windows.Forms.MouseButtons.Left Then
            Dim path As String = Application.ExecutablePath
            'Dim arg As String = "DebugRelaunch"
            Dim myProcess As New Process
            myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
            myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
            'myProcess.StartInfo.Arguments = arg
            'Threading.Thread.Sleep(10)
            myProcess.Start()
            Application.Exit()
            'Environment.Exit(0) 'this one erros here now, it didn't before i changed some stuff.
        Else
            IDEKairenRestart()
        End If
    End Sub
    Private Sub IDEKairenRestart()
        Dim path As String = Application.ExecutablePath
        Dim arg As String = "DebugRelaunch"
        Dim myProcess As New Process
        myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
        myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
        myProcess.StartInfo.Arguments = arg
        myProcess.Start()
        Environment.Exit(0)
    End Sub
    Private Sub Button7_MouseDown(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles Button7.MouseDown
        'delete app data folder and exit
        If MsgBox("This is a dev command -- it will delete everything" & Environment.NewLine & "Continue?", MsgBoxStyle.YesNo, "Goeth Fordwardo?") = MsgBoxResult.No Then Exit Sub
        Button7.PerformClick() 'forgot why this is here, or maybe i should have deleted it lol
        If My.Computer.FileSystem.DirectoryExists(Folders.AppData) = False Then
            Log = "No AppData Folder Found - " & Folders.AppData
            Exit Sub
        End If
        My.Computer.FileSystem.DeleteDirectory(Folders.AppData, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        Log = "AppData Folder Deleted:"
        Log = "   " & Folders.AppData
        If e.Button = Windows.Forms.MouseButtons.Right Then
            IDEKairenRestart()
        Else
            Environment.Exit(0)
        End If
    End Sub


    '
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        Log = "Prompting User Logon"
        Dim UserName As String = InputBox("Type the username you want to login as", "User Login")
        If UserName = "" Then
            Log = "No User Name chosen"
            MsgBox("No User Name chosen", MsgBoxStyle.OkOnly, "User Login")
        Else
            If My.Computer.FileSystem.DirectoryExists(Folders.AccountsFolder & UserName & "\") = False Then
                Log = "User Name Not Found " & UserName
                MsgBox("User Not Found", MsgBoxStyle.OkOnly, "User Login")
            Else
                LoadUserAccount(UserName)
                Button3.Enabled = False
                Button4.Enabled = False
                Button9.Enabled = False
                Log = "User Logged In " & UserName
            End If
        End If
    End Sub '*2
    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If CreateNewAccount() <> "" Then
            Button3.Enabled = False
            Button4.Enabled = False
            Button9.Enabled = False
        End If
    End Sub '*-
    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
            Files.ProgramSettings.ClearTagsData() = "Autostart Account"
            Files.ProgramSettings.Save()
        End If
        Log = "Autostart User Removed"
    End Sub '*2
    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        Files.ProgramSettings.SetTaggedDataLine("Autostart Account", 1) = Label2.Text
        Files.ProgramSettings.Save()
        Log = "Autostart User Set To " & CurrentUserName
    End Sub '*2
    Private Sub Button9_Click(sender As System.Object, e As System.EventArgs) Handles Button9.Click
        DeleteAccount()
    End Sub '*-
    Private Sub Button10_Click(sender As System.Object, e As System.EventArgs) Handles Button10.Click
        UnloadUserAccount()
    End Sub '*-

    Private Sub TextBox2_DragEnter(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) = True Then
            Dim data() As String = e.Data.GetData(DataFormats.FileDrop)
            If Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".lnk" Then
                Dim obj As Object
                obj = CreateObject("WScript.Shell")
                Dim Shortcut As Object
                Shortcut = obj.CreateShortcut(data(0))
                If Microsoft.VisualBasic.Right(Shortcut.TargetPath, 4).ToLower = ".exe" Then
                    e.Effect = DragDropEffects.Copy
                End If
            ElseIf Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".exe" Then
                e.Effect = DragDropEffects.Copy
            End If
        End If
    End Sub '*+
    Private Sub TextBox2_DragDrop(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim data() As String = e.Data.GetData(DataFormats.FileDrop)
            If Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".lnk" Then
                Dim obj As Object
                obj = CreateObject("WScript.Shell")
                Dim Shortcut As Object
                Shortcut = obj.CreateShortcut(data(0))
                TextBox2.Text = Shortcut.TargetPath
                TextBox2.Focus()
                Log = "Drag & Drop .exe Shortcut Successful - " & data(0)
            ElseIf Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".exe" Then
                TextBox2.Text = data(0)
                TextBox2.Focus()
                Log = "Drag & Drop .exe File Successful - " & data(0)
            End If
        End If
    End Sub '*+
    Private Sub TextBox3_DragEnter(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles TextBox3.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) = True Then
            Dim data() As String = e.Data.GetData(DataFormats.FileDrop)
            If Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".iso" Then
                e.Effect = DragDropEffects.Copy
            End If
        End If
    End Sub '*+
    Private Sub TextBox3_DragDrop(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles TextBox3.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim data() As String = e.Data.GetData(DataFormats.FileDrop)
            If Microsoft.VisualBasic.Right(data(0), 4).ToLower = ".iso" Then
                TextBox3.Text = data(0)
                Log = "Drag & Drop .iso Shortcut Successful - " & data(0)
            End If
        End If
    End Sub '*+

    Private Sub CheckBox1_Click(sender As System.Object, e As System.EventArgs) Handles CheckBox1.Click
        If sender.Checked = False Then
            SetupCEAutoAttach()
        Else
            Log = "Undo CE AutoAttach not reimplemented yet"
        End If
    End Sub '*?
    Private Sub CheckBox2_Click(sender As System.Object, e As System.EventArgs) Handles CheckBox2.Click
        If sender.Checked = False Then
            SetupCEAutoLoad()
        Else
            Log = "Undo CE AutoLoad not reimplemented yet"
        End If
    End Sub '*2
    Private Sub SetupCEAutoAttach()
        'if pcsx2 isn't present then exit
        If My.Computer.FileSystem.FileExists(Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)) = False Then
            MsgBox("Warning! Your Emulator seems to be missing, cancelling Auto-Attach Setup.", MsgBoxStyle.Exclamation, "Auto-Attach Setup Error")
            Exit Sub
        End If
        'HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CheatEngine\DefaultIcon ' has my machines cheat engine location
        'HKEY_CURRENT_USER\SOFTWARE\CheatEngine\DefaultIcon ' has my machines cheat engine location
        'C:\Program Files (x86)\Cheat Engine 6.4\Cheat Engine.exe,0
        If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing) IsNot Nothing Then
            Dim reg_value_cheatengine_AutoLaunch As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing).ToString
            Dim EmulatorFileName As String = Microsoft.VisualBasic.Right(Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1), Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).Length() - Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).LastIndexOf("\") - 1)
            ' MsgBox(EmulatorFileName)
            ' MsgBox(reg_value_cheatengine_AutoLaunch.Contains(EmulatorFileName))
            If reg_value_cheatengine_AutoLaunch.Contains(EmulatorFileName) = False Then
                'MsgBox("installing auto attach")
                Dim lastindexof As String = reg_value_cheatengine_AutoLaunch.LastIndexOf(";")
                Dim length As String = (reg_value_cheatengine_AutoLaunch.Length - 1)
                If length = 0 Or lastindexof <> length Then
                    My.Computer.Registry.SetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", reg_value_cheatengine_AutoLaunch & ";" & EmulatorFileName, Microsoft.Win32.RegistryValueKind.String)
                Else
                    My.Computer.Registry.SetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", reg_value_cheatengine_AutoLaunch & EmulatorFileName, Microsoft.Win32.RegistryValueKind.String)
                End If
                MsgBox("Auto-Attach setup complete.", MsgBoxStyle.Information, "CE Auto-Attach Setup")
            Else
                MsgBox("Auto-Attach was already setup.", MsgBoxStyle.Information, "CE Auto-Attach Setup")
            End If
        Else
            'ce's autoattach registry key is missing
            Dim EmulatorFileName As String = Microsoft.VisualBasic.Right(Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1), Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).Length() - Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).LastIndexOf("\") - 1)
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", EmulatorFileName, Microsoft.Win32.RegistryValueKind.String)
            MsgBox("Auto-Attach setup complete." & vbNewLine & "The Registry Key for Cheat Engine was created from scratch, so it was missing.", MsgBoxStyle.Information, "CE Auto-Attach Setup")
        End If
    End Sub '*2
    Private Sub SetupCEAutoLoad()
        'HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CheatEngine\DefaultIcon ' has my machines cheat engine location
        'C:\Program Files (x86)\Cheat Engine 6.4\Cheat Engine.exe,0
        Dim reg_value_cheatengine As String = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CheatEngine\DefaultIcon", "", Nothing).ToString
        'MsgBox(reg_value_cheatengine)
        Dim cheatengine_rootfolder As String
        Dim cheatengine_autorunfolder As String
        cheatengine_rootfolder = Microsoft.VisualBasic.Left(reg_value_cheatengine, reg_value_cheatengine.LastIndexOf("Cheat Engine"))
        cheatengine_autorunfolder = cheatengine_rootfolder & "autorun\"
        If My.Computer.FileSystem.FileExists(cheatengine_autorunfolder & "Ihtol.lua") Then
            'plan a installation, in ce\autorun folder
            Dim continueResponse As MsgBoxResult
            continueResponse = MsgBox("The modification file is already present. Do you want to overwrite it?", MsgBoxStyle.YesNoCancel, "Modification File Already Present")
            If continueResponse = MsgBoxResult.Yes Then
                IO.File.WriteAllBytes(cheatengine_autorunfolder & "Ihtol.lua", My.Resources.Ihtol)
                If My.Computer.FileSystem.FileExists(cheatengine_autorunfolder) Then
                    MsgBox("Process Complete." & vbNewLine & "Modification file installed.", MsgBoxStyle.Information, "Modification Process")
                    Exit Sub
                End If
            ElseIf continueResponse = MsgBoxResult.No Then
                MsgBox("Process complete. No changes made.", MsgBoxStyle.Information, "Modification Process")
                Exit Sub
            ElseIf continueResponse = MsgBoxResult.Cancel Then
                MsgBox("No changes made. Cancelling...", MsgBoxStyle.Information, "Cancelling Installation..")
                Exit Sub
            End If
        Else
            IO.File.WriteAllBytes(cheatengine_autorunfolder & "Ihtol.lua", My.Resources.Ihtol)
            If My.Computer.FileSystem.FileExists(cheatengine_autorunfolder & "Ihtol.lua") Then
                MsgBox("Process Complete." & vbNewLine & "Modification file installed.", MsgBoxStyle.Information, "Modification Process")
                Exit Sub
            Else
                'process a did not work, let's try b, then make c and try that i guess lol
            End If
        End If
        'plan b installation, add directly into main.lua file
        If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "Ihtol.lua") Then
            Dim continueResponse As MsgBoxResult
            continueResponse = MsgBox("The modification file is already present. Do you want to overwrite it?", MsgBoxStyle.YesNoCancel, "Modification File Already Present")
            If continueResponse = MsgBoxResult.Yes Then
                IO.File.WriteAllBytes(cheatengine_rootfolder & "Ihtol.lua", My.Resources.Ihtol)
            ElseIf continueResponse = MsgBoxResult.No Then
                '
            ElseIf continueResponse = MsgBoxResult.Cancel Then
                MsgBox("No changes made. Cancelling...", MsgBoxStyle.OkOnly, "Cancelling Installation..")
                Exit Sub
            End If
        End If
        Dim ces_mainlua As List(Of String)
        Dim sr As New IO.StreamReader(cheatengine_rootfolder & "\main.lua")
        Do Until sr.EndOfStream
            ces_mainlua.Add(sr.ReadLine)
        Loop
        sr.Close()
        For Each _line In ces_mainlua
            If Microsoft.VisualBasic.Left(_line, 17) = "--Runnindatshityo" Then
                MsgBox("Process Complete." & vbNewLine & "Modification reference already detected, no change made.", MsgBoxStyle.Information, "Modification Process")
                Exit Sub
            End If
        Next
        If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "\main - This was your original copy.lua") = False Then
            My.Computer.FileSystem.CopyFile(cheatengine_rootfolder & "\main.lua", cheatengine_rootfolder & "\main - This was your original copy.lua", False)
        End If

        'ces_mainlua.CurrentIndex = ces_mainlua.Count
        ces_mainlua.Add("--Runnindatshityo " & Program.ProgramVersionNumber)
        ces_mainlua.Add("local f=io.open(" & Chr(34) & "Ihtol.lua" & Chr(34) & "," & Chr(34) & "r" & Chr(34) & ")")
        ces_mainlua.Add("if f~=nil then")
        ces_mainlua.Add("io.close(f)")
        ces_mainlua.Add("dofile(" & Chr(34) & "Ihtol.lua" & Chr(34) & ")")
        ces_mainlua.Add("else")
        ces_mainlua.Add("print(" & Chr(34) & "You may not find your way today after all.." & Chr(34) & ")")
        ces_mainlua.Add("end")
        Dim sw As New IO.StreamWriter(cheatengine_rootfolder & "\main.lua")
        For Each item In ces_mainlua
            sw.WriteLine(item)
        Next
        'Shell(http_path & " " & action_info)
        'add error check here '"C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -osint -url "%1" 'is my machine's entry
        MsgBox("Modification installed." & vbNewLine & "Modification reference wrote.", MsgBoxStyle.Information, "Modification Process")
    End Sub '*2
    Private Sub CheckCEAutoAttach()
        Try

            If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing) IsNot Nothing Then
                Dim reg_value_cheatengine_AutoLaunch As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing).ToString
                Dim EmulatorFileName As String = Microsoft.VisualBasic.Right(Files.UserSettings.GetTaggedDataLine("Emulator Path", 1), Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).Length - Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).LastIndexOf("\") - 1)
                If reg_value_cheatengine_AutoLaunch.Contains(EmulatorFileName) = False Then
                    'red
                    CheckBox1.Checked = False
                    Log = "No CE AutoAttach Setup"
                Else
                    CheckBox1.Checked = True
                    Log = "CE AutoAttach Ready"
                End If
            Else
                'red
                CheckBox1.Checked = False
                Log = "No CE AutoAttach Setup"
            End If

        Catch ex As Exception
        End Try
    End Sub '*2
    Private Sub CheckCEAutoLoad()
        Dim reg_value_cheatengine As String = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CheatEngine\DefaultIcon", "", Nothing).ToString
        'MsgBox(reg_value_cheatengine)
        Dim cheatengine_rootfolder As String
        cheatengine_rootfolder = Microsoft.VisualBasic.Left(reg_value_cheatengine, reg_value_cheatengine.LastIndexOf("Cheat Engine"))
        If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "autorun\Ihtol.lua") = False Then
            If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "Ihtol.lua") = False Then
                'red
                CheckBox2.Checked = False
                Log = "No CE AutoLoad Setup"
            Else
                'green
                Log = "CE AutoLoad Ready"
                CheckBox2.Checked = True
            End If
        Else
            'green
            Log = "CE AutoLoad Ready"
            CheckBox2.Checked = True
        End If
    End Sub '*2

    Private Sub Button11_Click(sender As System.Object, e As System.EventArgs) Handles Button11.Click
        If TextBox2.Text.Trim <> "" Then
            Files.UserSettings.SetTaggedDataLine("Emulator Path", 1) = TextBox2.Text
            Files.UserSettings.Save()
            Log = "Emulator Path Saved = " & Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)
        End If
    End Sub '*2
    Private Sub Button12_Click(sender As System.Object, e As System.EventArgs) Handles Button12.Click
        TextBox2.Text = ""
        Files.UserSettings.SetTaggedDataLine("Emulator Path", 1) = ""
        Files.UserSettings.Save()
        Log = "Emulator Path Cleared = " & Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)
    End Sub '*2
    Private Sub Button14_Click(sender As System.Object, e As System.EventArgs) Handles Button14.Click
        If TextBox3.Text.Trim <> "" Then
            Files.UserSettings.SetTaggedDataLine("ISO Path", 1) = TextBox3.Text
            Files.UserSettings.Save()
            Log = "ISO Path Saved = " & Files.UserSettings.GetTaggedDataLine("ISO Path", 1)
        End If
    End Sub '*2
    Private Sub Button13_Click(sender As System.Object, e As System.EventArgs) Handles Button13.Click
        TextBox3.Text = ""
        Files.UserSettings.SetTaggedDataLine("ISO Path", 1) = ""
        Files.UserSettings.Save()
        Log = "ISO Path Cleared = " & Files.UserSettings.GetTaggedDataLine("ISO Path", 1)
    End Sub '*2
    Private Sub Button15_Click(sender As System.Object, e As System.EventArgs) Handles Button15.Click
        If TextBox4.Text.Trim <> "" Then
            Files.UserSettings.SetTaggedDataLine("Work Drive Path", 1) = TextBox4.Text
            Files.UserSettings.Save()
            Log = "Work Drive Path Saved = " & Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1)
        End If
    End Sub '*2
    Private Sub Button16_Click(sender As System.Object, e As System.EventArgs) Handles Button16.Click
        TextBox4.Text = ""
        Files.UserSettings.SetTaggedDataLine("Work Drive Path", 1) = ""
        Files.UserSettings.Save()
        Log = "Work Drive Path Cleared = " & Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1)
    End Sub '*2
    Private Sub Button18_Click(sender As System.Object, e As System.EventArgs) Handles Button18.Click
        If TextBox5.Text.Trim <> "" Then
            Files.UserSettings.SetTaggedDataLine("Lan Play Path", 1) = TextBox5.Text
            Files.UserSettings.Save()
            Log = "Lan Play Path Saved = " & Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1)
        End If
    End Sub '*2
    Private Sub Button19_Click(sender As System.Object, e As System.EventArgs) Handles Button19.Click
        TextBox5.Text = ""
        Files.UserSettings.SetTaggedDataLine("Lan Play Path", 1) = ""
        Files.UserSettings.Save()
        Log = "Lan Play Path Cleared = " & Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1)
    End Sub '*2

    Private Sub CheckBox3_Click(sender As System.Object, e As System.EventArgs) Handles CheckBox3.Click
        If sender.Checked = False Then
            SetupLUA()
        Else
            Log = "Uninstall LUA not implemented yet"
        End If
    End Sub '*2
    Private Sub SetupLUA()
        Log = "Install LUA Initiated"
        Dim dl As Boolean = False
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "LUAs\") Then
            If MsgBox("LUA files appear to exist. Delete and reinstall Anyway?", MsgBoxStyle.YesNo, "Overwrite Existing Folder?") = MsgBoxResult.No Then
                Log = "LUA Files Found. Prompting for Action."
                Exit Sub
            Else
                Log = "LUA Files Not Present."
                dl = True
            End If
        End If
        If dl Then My.Computer.FileSystem.DeleteDirectory(Folders.CurrentAccount & "LUAs\", FileIO.DeleteDirectoryOption.DeleteAllContents)
        IO.File.WriteAllBytes(Folders.CurrentAccount & "LUAs.zip", My.Resources.LUAs)
        IO.Compression.ZipFile.ExtractToDirectory(Folders.CurrentAccount & "LUAs.zip", Folders.CurrentAccount)
        My.Computer.FileSystem.DeleteFile(Folders.CurrentAccount & "LUAs.zip")
        CheckBox3.Checked = True
        Log = "LUA Files Installed"
    End Sub '*2
    Private Sub CheckLUA()
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "LUAs\") Then
            Log = "LUA Files Present"
            CheckBox3.Checked = True
        Else
            Log = "LUA Files Not Found"
            CheckBox3.Checked = False
        End If
    End Sub '*2
    Private Sub SetupCheatTables()
        Log = "Install Cheat Tables Initiated"
        Dim dc As Boolean = False
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Cheat Tables\") Then
            If MsgBox("Cheat Table files appear to exist. Delete and reinstall Anyway?", MsgBoxStyle.YesNo, "Overwrite Existing Folder?") = MsgBoxResult.No Then
                Log = "Cheat Table Files Found. Prompting for Action."
                Exit Sub
            Else
                Log = "Cheat Table Files Not Present"
                dc = True
            End If
        End If
        If dc Then My.Computer.FileSystem.DeleteDirectory(Folders.CurrentAccount & "Cheat Tables\", FileIO.DeleteDirectoryOption.DeleteAllContents)
        IO.File.WriteAllBytes(Folders.CurrentAccount & "Cheat Tables.zip", My.Resources.Cheat_Tables)
        IO.Compression.ZipFile.ExtractToDirectory(Folders.CurrentAccount & "Cheat Tables.zip", Folders.CurrentAccount)
        My.Computer.FileSystem.DeleteFile(Folders.CurrentAccount & "Cheat Tables.zip")
        CheckBox4.Checked = True
        Log = "Cheat Table Files Installed"
    End Sub '*2
    Private Sub CheckCheatTables()
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Cheat Tables\") Then
            Log = "Cheat Table Files Present"
            CheckBox4.Checked = True
        Else
            Log = "Cheat Table Files Not Found"
            CheckBox4.Checked = False
        End If
    End Sub '*2
    Private Sub SetupGameData()
        Log = "Install Game Data Initiated"
        Dim dc As Boolean = False
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Game Data\") Then
            If MsgBox("Game Data files appear to exist. Delete and reinstall Anyway?", MsgBoxStyle.YesNo, "Overwrite Existing Folder?") = MsgBoxResult.No Then
                Log = "Game Data Files Found. Prompting for Action."
                Exit Sub
            Else
                Log = "Game Data Files Not Present"
                dc = True
            End If
        End If
        If dc Then My.Computer.FileSystem.DeleteDirectory(Folders.CurrentAccount & "Game Data\", FileIO.DeleteDirectoryOption.DeleteAllContents)
        IO.File.WriteAllBytes(Folders.CurrentAccount & "Game Data.zip", My.Resources.Game_Data)
        IO.Compression.ZipFile.ExtractToDirectory(Folders.CurrentAccount & "Game Data.zip", Folders.CurrentAccount)
        My.Computer.FileSystem.DeleteFile(Folders.CurrentAccount & "Game Data.zip")
        CheckBox5.Checked = True
        Log = "Game Data Files Installed"
    End Sub '*2
    Private Sub CheckGameData()
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Game Data\") Then
            Log = "Game Data Files Present"
            CheckBox5.Checked = True
        Else
            Log = "Game Data Files Not Found"
            CheckBox5.Checked = False
        End If
    End Sub '*2

    Private Sub CheckBox4_Click(sender As System.Object, e As System.EventArgs) Handles CheckBox4.Click
        SetupCheatTables()
    End Sub '*-

    Private Sub Button17_Click(sender As System.Object, e As System.EventArgs) Handles Button17.Click
        If My.Computer.FileSystem.DirectoryExists(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1)) = False Then
            Log = "Cancelling Launch : Work Drive Not Found"
            Exit Sub
        End If
        LaunchGameFullExperience()
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetLanPlayFolder")
        sr.WriteLine()
        sr.Write(Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1))
        sr.WriteLine()
        sr.Close()
        'SetLanPlayerName
        Log = " --- Launching Complete --- "
    End Sub '*2
    Private Sub LaunchCE(Optional ByVal _suffix As String = "", Optional ByVal _copyToAndRunFromFolder As String = "")
        If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder) Then
            Try
                'Shell() works too
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "LUAs\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "LUAs\", _copyToAndRunFromFolder & "LUAs\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Cheat Tables\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Cheat Tables\", _copyToAndRunFromFolder & "Cheat Tables\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Game Data\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Game Data\", _copyToAndRunFromFolder & "Game Data\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Custom Data\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Custom Data\", _copyToAndRunFromFolder & "Custom Data\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Ghosts\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Ghosts\", _copyToAndRunFromFolder & "Ghosts\", True)
                If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\") = False Then My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\")
                If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\i\") = False Then My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\i\")
                If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\o\") = False Then My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\o\")
                Process.Start(_copyToAndRunFromFolder & "Cheat Tables\" & "MainTable" & _suffix & ".CT")
            Catch ex As Exception
            End Try
        Else
            Try
                'Shell() works 
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\")
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\i\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\i\")
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\o\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\o\")
                Process.Start(Folders.CurrentAccount & "Cheat Tables\" & "MainTable" & _suffix & ".CT")
            Catch ex As Exception
            End Try
        End If
    End Sub '*2
    Private Sub LaunchGameFullExperience()
        If My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)) And My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("ISO Path", 1)) Then
            Try
                Dim path As String = Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)
                Dim arg As String = Chr(34) & Files.UserSettings.GetTaggedDataLine("ISO Path", 1) & Chr(34)
                Dim myProcess As New Process
                myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
                myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
                myProcess.StartInfo.Arguments = arg
                LaunchCE("_Silent", Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1))
                Threading.Thread.Sleep(500)
                myProcess.Start()
            Catch ex As Exception
            End Try
        Else
            LaunchCE()
        End If
    End Sub '*2

    Private Sub Button20_Click(sender As System.Object, e As System.EventArgs) Handles Button20.Click
        Log = " --- Sending CloseAll --- "
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("CloseAll")
        sr.WriteLine()
        sr.Write(Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1))
        sr.WriteLine()
        sr.Close()
    End Sub '*2

    Private Sub Button21_Click(sender As System.Object, e As System.EventArgs) Handles Button21.Click
        SetLanPlayerName("1", TextBox6.Text)
    End Sub '*2
    Private Sub Button22_Click(sender As System.Object, e As System.EventArgs) Handles Button22.Click
        SetLanPlayerName("2", TextBox7.Text)
    End Sub '*2
    Private Sub Button23_Click(sender As System.Object, e As System.EventArgs) Handles Button23.Click
        SetLanPlayerName("3", TextBox8.Text)
    End Sub '*2
    Private Sub Button24_Click(sender As System.Object, e As System.EventArgs) Handles Button24.Click
        SetLanPlayerName("4", TextBox9.Text)
    End Sub '*2
    Private Sub SetLanPlayerName(ByVal Number As Integer, ByVal Name As String)
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetLanPlayerName")
        sr.WriteLine()
        sr.Write(Number)
        sr.WriteLine()
        sr.Write(Name)
        sr.WriteLine()
        sr.Close()
    End Sub '*2

    Private Sub Button25_Click(sender As System.Object, e As System.EventArgs) Handles Button25.Click
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetMyLanPlayerNumber")
        sr.WriteLine()
        sr.Write(NumericUpDown1.Value)
        sr.WriteLine()
        sr.Close()
    End Sub '*2

    Private Sub Button26_Click(sender As System.Object, e As System.EventArgs) Handles Button26.Click
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetMyLanPlayerName")
        sr.WriteLine()
        sr.Write(TextBox10.Text)
        sr.WriteLine()
        sr.Close()
    End Sub '*2

    Private Sub CheckBox5_Click(sender As System.Object, e As System.EventArgs) Handles CheckBox5.Click
        If sender.Checked = False Then
            SetupGameData()
        Else
            Log = "Uninstall Game Data not implemented yet"
        End If
    End Sub '*?

    Public Function ReadLineFromFileOnline(ByVal _urlToCheck As String) As String
        'this should always be ran in a secondary thready
        Try
            Dim rt As String
            Dim wRequest As System.Net.WebRequest
            Dim wResponse As System.Net.WebResponse
            Dim SR As System.IO.StreamReader
            wRequest = System.Net.WebRequest.Create(_urlToCheck)
            wResponse = wRequest.GetResponse
            SR = New System.IO.StreamReader(wResponse.GetResponseStream)
            rt = SR.ReadToEnd
            SR.Close()
            rt = CStr(rt.Trim)
            Return rt
        Catch ex As Exception
            Return "error"
        End Try
    End Function '*2

    Private Sub Button27_Click(sender As System.Object, e As System.EventArgs) Handles Button27.Click
        Button27.Enabled = False ' Check Latest Version button
        If (Year(Today) < 2019) Or (Year(Today) = 2019 And Month(Today) < 11) Then
            'do update
            CheckVersionTimer = New Timer
            CheckVersionTimer.Interval = 1
            CheckVersionTimer.Start()
        ElseIf (Year(Today) > 2019) Or (Year(Today) = 2019 And Month(Today) >= 11) Then
            'do not update
            Log = "Cancelling Update Check : For security, this won't update past October, 2019 in case I do not control the domain after that"
        Else
            'error in my code! )';
            Log = "Cancelling Update Check : And Error Occured While Trying Not to Y2K."
        End If
    End Sub '*2
    Private Sub CheckVersionTimer_Tick(sender As System.Object, e As System.EventArgs) Handles CheckVersionTimer.Tick
        CheckVersionTimer.Interval = 60000
        'this whole process needs multithreaded
        Log = "Checking Version for Latest Kairen Release"
        Dim verres As String = ReadLineFromFileOnline(WebsiteDomainAddress & "databits/kairenprogram/kairenlatestversion.txt")
        If verres = "error" Or verres.Length > 15 Then
            Log = "An error occured while checking Kairen's newest version. You will have to restart Kairen to check again."
            If verres.Length > 15 Then
                Log = "Error: 16"
            ElseIf verres = "error" Then
                Log = "Error: 0"
            End If
            Exit Sub
        End If
        KairenLatestVersion = verres
        Log = "Latest Version Detected: " & KairenLatestVersion
        If ProgramVersionNumber <> KairenLatestVersion Then
            Log = "Your Kairen Version is: " & ProgramVersionNumber
            Log = "Enabling Program Update"
        Else
            Log = "Your Kairen Version is Up-to-Date : " & ProgramVersionNumber
        End If
        Button28.Enabled = True ' Update Button
        CheckVersionTimer.Stop()
    End Sub '*2

    Private Sub Button28_Click(sender As System.Object, e As System.EventArgs) Handles Button28.Click
        Button28.Enabled = False ' Update Button
        DownloadUpdateTimer = New Timer
        DownloadUpdateTimer.Interval = 1
        DownloadUpdateTimer.Start()
    End Sub '*2
    Private Sub DownloadUpdateTimer_Tick(sender As System.Object, e As System.EventArgs) Handles DownloadUpdateTimer.Tick
        DownloadUpdateTimer.Interval = 60000
        DownloadUpdateTimer.Stop()
        Log = "Downloading Kairen " & KairenLatestVersion & " Initiated"
        Dim lv As Object
        lv = ReadLineFromFileOnline(WebsiteDomainAddress & "databits/kairenprogram/" & KairenLatestVersion & ".txt")
        If lv = "error" Then
            Log = "An error occured while downloading the newest Kairen. You will have to restart Kairen to try again."
        End If

        Dim remoteUri As String = lv
        Dim myStringWebResource As String = Nothing
        Dim myWebClient As New System.Net.WebClient()
        myStringWebResource = remoteUri
        Try
            '            myWebClient.DownloadFile(myStringWebResource, My.Computer.FileSystem.SpecialDirectories.Desktop & "\Kairen " & KairenLatestVersion & ".exe")
            myWebClient.DownloadFile(myStringWebResource, Folders.AppData & "Kairen " & KairenLatestVersion & ".exe")
        Catch ex As Exception
            If ex.Message = "The underlying connection was closed: An unexpected error occurred on a receive." Then
                Log = "An error occured while downloading Kairen " & KairenLatestVersion & ". You will have to restart Kairen to try again."
                Log = "The error Windows told me was: " & Chr(34) & ex.Message & Chr(34)
                Log = "For me, this meant my Anti-Virus was blocking it's download."
                Exit Sub
            Else
                Log = "An error occured while downloading Kairen " & KairenLatestVersion & ". You will have to restart Kairen to try again."
                Exit Sub
            End If
        End Try

        Log = "Kairen " & KairenLatestVersion & " downloaded successfully."
        Log = "Launching Kairen " & KairenLatestVersion & " in 15 seconds."
        InstallUpdateTimer = New Timer
        InstallUpdateTimer.Interval = 15000
        InstallUpdateTimer.Start()
    End Sub '*2
    Private Sub InstallUpdateTimer_Tick(sender As System.Object, e As System.EventArgs) Handles InstallUpdateTimer.Tick
        InstallUpdateTimer.Interval = 2000
        InstallUpdateTimer.Stop()
        Dim path As String = Folders.AppData & "Kairen " & KairenLatestVersion & ".exe"
        Dim arg As String = "UpdateRename"
        Dim myProcess As New Process
        myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
        myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
        myProcess.StartInfo.Arguments = arg
        Try
            myProcess.Start()
        Catch ex As Exception
            If ex.Message = "The operation was canceled by the user" Then
                MsgBox("You cancelled starting the newly downloaded Kairen. You will have to start it yourself now." & Environment.NewLine & Environment.NewLine & Folders.AppData & "Kairen " & KairenLatestVersion & ".exe", MsgBoxStyle.Information, "User Cancelled Kairen Start")
            Else
                MsgBox("Unknown Error occured while trying to launch new Kairen.", MsgBoxStyle.Exclamation, "Unknown Error Occured")
            End If
        End Try
        Environment.Exit(0)
    End Sub '*2

    Private Sub Button29_Click(sender As System.Object, e As System.EventArgs) Handles Button29.Click
        Exit Sub
        Dim newDomainToUse As String
        If Year(Today) <= 2019 And Month(Today) <= 10 Then
            newDomainToUse = InputBox("Input the website Kairen should check for updates at." & Environment.NewLine & Environment.NewLine & _
                      Chr(34) & "http://eqrh.tk/" & Chr(34) & " has been filled in for you since the date is before November, 2019." & Environment.NewLine & Environment.NewLine & _
                      "Leave blank to make no changes.", "Input Kairen Update Domain", WebsiteDomainAddress)
            If newDomainToUse = "" Or WebsiteDomainAddress Then Exit Sub
            Files.ProgramSettings.SetTaggedDataLine("Update Domain", 1) = newDomainToUse
            Files.ProgramSettings.SetTaggedDataLine("Update Domain", 2) = 11
            Files.ProgramSettings.SetTaggedDataLine("Update Domain", 3) = 2019
        Else
            newDomainToUse = InputBox("Input the website Kairen should check for updates at." & Environment.NewLine & Environment.NewLine & _
                      "Leave blank to make no changes.", "Input Kairen Update Domain", WebsiteDomainAddress)
            If newDomainToUse = "" Or WebsiteDomainAddress Then Exit Sub
            Files.ProgramSettings.SetTaggedDataLine("Update Domain", 1) = newDomainToUse
        End If
    End Sub '*?

End Class
