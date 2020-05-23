Public Class Form2
    Dim DebugIsAppData As Boolean = False
    Private Event Event_ProgramIsReady()
    Private DisplayControl As New class_DisplayControl(Me)
    Private WithEvents RestartTimer As Timer
    Sub New()
        InitializeComponent()
        If Environment.GetCommandLineArgs.Contains("OutputVersionAndExit") Then
            Dim sr As New IO.StreamWriter(Application.ExecutablePath & ".txt")
            sr.Write(ProgramVersionNumber)
            sr.Close()
            Environment.Exit(0)
        End If
        DebugIsAppData = Environment.GetCommandLineArgs.Contains("DebugIsAppData")
        Me.Width = 1320 'IDE display and run-time display do not match for me, so set run-time display now
    End Sub
    Private Sub Form2_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
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
        Dim doReinstallPrompt As Boolean = False
        If Application.ExecutablePath.ToLower = Folders.AppData.ToLower & "Kairen.exe".ToLower Or DebugIsAppData Then
            'launch from installed location
            If Application.ExecutablePath.ToLower <> Folders.AppData.ToLower & "Kairen.exe".ToLower Then
                Log = "DebugIsAppData : Copying this build to AppData"
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, Folders.AppData & "Kairen.exe", True)
            End If
            Log = "Launched From Installed Location"
            '
            'kairen launched from normal folder, proceed to version check
            '
        Else
            'launch from non-installed location
            Log = "Launched From Non-Installed Location"
            doReinstallPrompt = True
        End If

        If My.Computer.FileSystem.DirectoryExists(Folders.AppData) Then
            'install found - update path / cancel
            Log = "Installation Folder Present"
            If CheckInstalledVersion() Then
                '
                'launch version is same as installed/launched version
                If doReinstallPrompt Then
                    If KairenReinstallationPrompt() = False Then
                        Log = "Program will proceed as normal"
                        '
                        'kairen launched from non-install folder, proceed as normal to version check
                        '
                    Else
                        Log = "Program will restart from Installed Location"
                        Log = "Restarting in 15 seconds. Hopefully that's enough time to read all of this if you cared q="
                        '
                        'kairen reinstall chosen, restart
                        '
                        RestartTimer = New Timer()
                        RestartTimer.Interval = 15000
                        RestartTimer.Start()
                        Exit Sub
                    End If
                End If

                'Log = "*kairen should launch* a"
                RaiseEvent Event_ProgramIsReady()
                Exit Sub
            Else
                'launched version isn't same as installed version
                Log = "Prompting to install version " & ProgramVersionNumber & " over present version"
                If ThisKairenInstallationPrompt() Then
                    '
                    'kairen updated
                    'go ahead and start passing control to the user
                    '
                    'Log = "*kairen should launch* b"
                    RaiseEvent Event_ProgramIsReady()
                    Exit Sub
                Else
                    '
                    'kairen not updated
                    'don't run
                    '
                    Log = "Only an installed Kairen Version may run. This program might not function properly otherwise, so there is nothing more you can do from here."
                End If
            End If
        Else
            Log = "Installation Folder Not Found"
            Log = "Prompting User to Install Kairen"
            If NewKairenInstallationPrompt() Then
                '
                'kairen now installed
                'go ahead and start passing control to the user
                '
                'Log = "*kairen should launch* c"
                RaiseEvent Event_ProgramIsReady()
                Exit Sub
            Else
                '
                'kaire not installed
                'don't run
                '
                Log = "Only an installed Kairen Version may run. This program might not function properly otherwise, so there is nothing more you can do from here."
            End If
        End If
    End Sub
    Private WriteOnly Property Log() As String
        Set(value As String)
            If TextBox1.Text <> "" Then TextBox1.AppendText(Environment.NewLine)
            TextBox1.AppendText(value)
        End Set
    End Property
    Private Sub EventHandler_ProgramIsReady() Handles Me.Event_ProgramIsReady
        'Log = "*program is ready event fired*"
        Log = "Kairen Startup Successful"
        LoadKairen()
        DisplayControl.ProgramIsReady()
    End Sub
    Private Sub RestartTimer_Tick(sender As System.Object, e As System.EventArgs) Handles RestartTimer.Tick
        sender.Stop()
        Dim path As String = Folders.AppData & "Kairen.exe"
        'Dim arg As String = _folderToOpen
        Dim myProcess As New Process
        myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
        myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
        'myProcess.StartInfo.Arguments = arg
        myProcess.Start()
        Environment.Exit(0)
    End Sub

    Class class_DisplayControl
        Private _parentForm As Form2
        Sub New(ByRef _setParentForm As Form2)
            _parentForm = _setParentForm
        End Sub
        Sub ProgramIsReady()
            _parentForm.MenuStrip1.Enabled = True
            _parentForm.display_button_CheckforUpdates.Enabled = True
        End Sub
        Sub UserIsLoggedIn()
            _parentForm.UserLoginToolStripMenuItem.Enabled = False
            _parentForm.UserLogoutToolStripMenuItem.Enabled = True
            _parentForm.UserSetupToolStripMenuItem.Enabled = True
            _parentForm.SetAutostartUserToolStripMenuItem.Enabled = True

            _parentForm.CreateUserAccountToolStripMenuItem.Enabled = False
            _parentForm.DeleteUserAccountToolStripMenuItem.Enabled = False

            _parentForm.display_textbox_SavestatePlayerName.Enabled = True
            _parentForm.display_button_Save_savestatePlayerName.Enabled = True

            _parentForm.CheatEngineAutoAttachisSetupToolStripMenuItem.Checked = _parentForm.CheckCEAutoAttach()
            _parentForm.CheatEngineAutoLoadisSetupToolStripMenuItem.Checked = _parentForm.CheckCEAutoLoad()

            _parentForm.CheckForFolderExistence("Cheat Engine Cheat Table", "Cheat Tables\", _parentForm.CheatEngineCheatTablesAreInstalledToolStripMenuItem)
            _parentForm.CheckForFolderExistence("LUA", "LUAs\", _parentForm.LUAScriptsAreInstalledToolStripMenuItem)
            _parentForm.CheckForFolderExistence("Game Data", "Game Data\", _parentForm.GameDataIsInstalledToolStripMenuItem)

            _parentForm.UpdateMenuItem("Emulator Path", _parentForm.EmulatorPathIsSetupToolStripMenuItem)
            _parentForm.UpdateMenuItem("ISO Path", _parentForm.ISOPathIsSetupToolStripMenuItem)
            _parentForm.UpdateMenuItem("Work Drive Path", _parentForm.WorkDrivePathIsSetupToolStripMenuItem)
            _parentForm.UpdateMenuItem("LAN Play Path", _parentForm.LanPlayPathIsSetupToolStripMenuItem)

            _parentForm.Label3.Text = Program.CurrentUserName
        End Sub
        Sub NoUserLoggedIn()
            _parentForm.UserLoginToolStripMenuItem.Enabled = True
            _parentForm.UserLogoutToolStripMenuItem.Enabled = False
            _parentForm.UserSetupToolStripMenuItem.Enabled = False
            _parentForm.SetAutostartUserToolStripMenuItem.Enabled = False

            _parentForm.CreateUserAccountToolStripMenuItem.Enabled = True
            _parentForm.DeleteUserAccountToolStripMenuItem.Enabled = True

            _parentForm.display_textbox_SavestatePlayerName.Enabled = False
            _parentForm.display_button_Save_savestatePlayerName.Enabled = False
            _parentForm.display_button_LaunchGame.Enabled = False

            _parentForm.CheatEngineAutoAttachisSetupToolStripMenuItem.Checked = False
            _parentForm.CheatEngineAutoLoadisSetupToolStripMenuItem.Checked = False

            _parentForm.CheatEngineCheatTablesAreInstalledToolStripMenuItem.Checked = False
            _parentForm.LUAScriptsAreInstalledToolStripMenuItem.Checked = False
            _parentForm.GameDataIsInstalledToolStripMenuItem.Checked = False

            _parentForm.EmulatorPathIsSetupToolStripMenuItem.Checked = False
            _parentForm.ISOPathIsSetupToolStripMenuItem.Checked = False
            _parentForm.WorkDrivePathIsSetupToolStripMenuItem.Checked = False
            _parentForm.LanPlayPathIsSetupToolStripMenuItem.Checked = False

            GameIsNotReadyForLaunch()

            _parentForm.Label3.Text = ""
        End Sub
        Sub UpdateIsPresent()
            _parentForm.display_button_CheckforUpdates.Enabled = False
            _parentForm.display_button_Update.Enabled = True
        End Sub
        Sub UpdateNotPresent()
            _parentForm.display_button_CheckforUpdates.Enabled = False
        End Sub
        Sub GameIsReadyForLaunch()
            _parentForm.display_button_LaunchGame.Enabled = True
            _parentForm.display_button_CloseGame.Enabled = True
            _parentForm.Button2.Enabled = True
            _parentForm.TextBox2.Enabled = True
            _parentForm.Button3.Enabled = True
            _parentForm.TextBox3.Enabled = True
            _parentForm.Button4.Enabled = True
            _parentForm.TextBox4.Enabled = True
            _parentForm.Button5.Enabled = True
            _parentForm.TextBox5.Enabled = True
            _parentForm.Button6.Enabled = True
            _parentForm.NumericUpDown1.Enabled = True
        End Sub
        Sub GameIsNotReadyForLaunch()
            _parentForm.display_button_LaunchGame.Enabled = False
            _parentForm.display_button_CloseGame.Enabled = False
            _parentForm.Button2.Enabled = False
            _parentForm.TextBox2.Enabled = False
            _parentForm.Button3.Enabled = False
            _parentForm.TextBox3.Enabled = False
            _parentForm.Button4.Enabled = False
            _parentForm.TextBox4.Enabled = False
            _parentForm.Button5.Enabled = False
            _parentForm.TextBox5.Enabled = False
            _parentForm.Button6.Enabled = False
            _parentForm.NumericUpDown1.Enabled = False
        End Sub
        Sub UserSettingsFileUpdated()
            _parentForm.CheatEngineAutoAttachisSetupToolStripMenuItem.Checked = _parentForm.CheckCEAutoAttach(False)
            _parentForm.CheatEngineAutoLoadisSetupToolStripMenuItem.Checked = _parentForm.CheckCEAutoLoad(False)

            _parentForm.CheckForFolderExistence("Cheat Engine Cheat Table", "Cheat Tables\", _parentForm.CheatEngineCheatTablesAreInstalledToolStripMenuItem, False)
            _parentForm.CheckForFolderExistence("LUA", "LUAs\", _parentForm.LUAScriptsAreInstalledToolStripMenuItem, False)
            _parentForm.CheckForFolderExistence("Game Data", "Game Data\", _parentForm.GameDataIsInstalledToolStripMenuItem, False)

            _parentForm.UpdateMenuItem("Emulator Path", _parentForm.EmulatorPathIsSetupToolStripMenuItem, False)
            _parentForm.UpdateMenuItem("ISO Path", _parentForm.ISOPathIsSetupToolStripMenuItem, False)
            _parentForm.UpdateMenuItem("Work Drive Path", _parentForm.WorkDrivePathIsSetupToolStripMenuItem, False)
            _parentForm.UpdateMenuItem("LAN Play Path", _parentForm.LanPlayPathIsSetupToolStripMenuItem, False)
        End Sub
    End Class

    'Install-Related Code
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
    End Function
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
    End Function
    Private Function NewKairenInstallationPrompt() As Boolean
        If MsgBox("No Kairen (version 4) Installation Present. Would you like to install Kairen?" & Environment.NewLine & "This will install Kairen " & ProgramVersionNumber, MsgBoxStyle.YesNo, "Kairen: Install Kairen?") = MsgBoxResult.Yes Then
            Log = "User Chose to Install Kairen"
            InstallKairen()
            Return True
        Else
            Log = "User Chose Not to Install Kairen"
            Return False
        End If
    End Function
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
    End Function
    Private Sub InstallKairen(Optional ByVal _promptUserAndShortCutCreations As Boolean = True)
        'this is still messy, but as of 11/30/18 it is right minus the argument being unused as-is
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

        Exit Sub
        If _promptUserAndShortCutCreations Then
            Log = "Prompting Desktop Shortcut Creation"
            If MsgBox("Create Desktop Shortcut?", MsgBoxStyle.YesNo, "Kairen: Installing Kairen") = MsgBoxResult.Yes Then
                CreateDesktopShortcut()
                Log = "Desktop Shortcut Made"
            Else
                Log = "Desktop Shortcut Not Created"
            End If
            Log = "*add CreateNewAccount()*"
            CreateNewAccount()
        Else
            'copypastad from above
            If GetFoldersInFolder(Folders.AccountsFolder).Count > 0 Then
                Log = "Users are present. Skipping Create User prompt."
                '                Log = "Users Found:"
                '                For Each item In GetFoldersInFolder(Folders.AccountsFolder)
                '                    Log = "   " & item
                '                Next
            Else
                Log = "No User Profiles Exist"
                '
                'prompt to create user profile
                '
            End If
            '            If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
            '                Log = "Autoloading " & Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1)
            '                ''''''                LoadUserAccount(Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1))
            '                'Button6.Enabled = True
            '            Else
            '                Log = "No Autoload User Found"
            '                'Button3.Enabled = True
            '                'Button4.Enabled = True
            '                'Button9.Enabled = True
            '            End If
        End If
        Log = "Installation Complete"
    End Sub
    Private Sub ReinstallKairen()
        Log = "Reinstalling Kairen " & ProgramVersionNumber
        Log = "Starting Normal Installation Process"
        InstallKairen(False)
        Log = "Kairen Reinstallation Complete"
    End Sub
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
    End Sub 'Technically Modular, but it's only used in "Install-Related" roles.

    'Modular Code
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
    End Function
    Private Sub CheckForAndCreateFolder(ByVal _folderNameInLog As String, ByVal _folderPath As String)
        Log = _folderNameInLog & " Folder " & _folderPath
        If My.Computer.FileSystem.DirectoryExists(_folderPath) = False Then
            Log = _folderNameInLog & " Folder Not Found"
            My.Computer.FileSystem.CreateDirectory(_folderPath)
            Log = _folderNameInLog & " Folder Created"
        Else
            Log = _folderNameInLog & " Folder Present"
        End If
    End Sub
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
    End Function

    Private Sub LoadKairen()
        Dim userAccounts() As String = GetUserAccounts()
        Select Case userAccounts.Count
            Case Is <= 0
                'No Accounts, Prompt User Creation
                DisplayControl.NoUserLoggedIn()
            Case 1
                'One Account, Auto-Load it
                Log = "One Account Present"
                Log = "Autoloading " & userAccounts(0)
                LoadUserAccount(userAccounts(0))
            Case Is > 1
                'More than one account, Check Auto-Load setting
                Log = "Multiple Accounts Present"
                If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
                    Log = "Autoloading " & Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1)
                    LoadUserAccount(Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1))
                Else
                    Log = "No Autoload User Found"
                    DisplayControl.NoUserLoggedIn()
                End If
        End Select
    End Sub

    'Account-Related Code
    Private Function GetUserAccounts() As String()
        Dim UserAccountsList() As String = GetFoldersInFolder(Folders.AccountsFolder)
        If UserAccountsList.Count > 0 Then
            Log = "Users Found:"
            For Each item In UserAccountsList
                Log = "   " & item
            Next
        Else
            Log = "No User Profiles Exist"
        End If
        Return UserAccountsList
    End Function
    Private Function CreateNewAccount() As String
        'Optional ByVal _userToCreate As String = "")
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
        LoadUserAccount(NewUserName)
        Return NewUserName
    End Function
    Private Function DeleteAccount() As String 'does not take into account that the currently-loaded account might delete itself
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
    End Function
    Private Sub AutoStartAccount_Add(ByVal _accountName As String)
        Files.ProgramSettings.SetTaggedDataLine("Autostart Account", 1) = _accountName
        Files.ProgramSettings.Save()
        Log = "Autostart User Set To " & _accountName
    End Sub
    Private Sub AutoStartAccount_Remove()
        If Files.ProgramSettings.GetTaggedDataLine("Autostart Account", 1) <> "" Then
            Files.ProgramSettings.ClearTagsData() = "Autostart Account"
            Files.ProgramSettings.Save()
        End If
        Log = "Autostart User Removed"
    End Sub

    '
    Private Function PromptUserLogon() As String
        Log = "Prompting User Logon"
        Dim UserName As String = InputBox("Type the username you want to login as", "User Login")
        If UserName = "" Then
            Log = "No User Name chosen"
            MsgBox("No User Name chosen", MsgBoxStyle.OkOnly, "User Login")
            Return ""
        Else
            If My.Computer.FileSystem.DirectoryExists(Folders.AccountsFolder & UserName & "\") = False Then
                Log = "User Name Not Found " & UserName
                Return ""
                MsgBox("User Not Found", MsgBoxStyle.OkOnly, "User Login")
            Else
                LoadUserAccount(UserName)
                Log = "User Logged In " & UserName
                Return UserName
            End If
        End If
    End Function
    Private Sub LoadUserAccount(ByVal _accountToLoad As String)
        'Label2.Text = _accountToLoad
        'Button6.Enabled = True
        'Button10.Enabled = True
        CurrentUserName = _accountToLoad
        Folders.CurrentAccount = Folders.AccountsFolder & _accountToLoad & "\"
        Files.UserSettings = New Class_TextFile(Folders.CurrentAccount, "UserSettings.txt", False)
        AddHandler Files.UserSettings.FileChanged, AddressOf UserSettingsChanged
        Files.UserSettings.ReadFile()
        'CheckCEAutoAttach()
        'CheckLUA()
        'CheckCheatTables()
        Log = "Account Loaded " & _accountToLoad

        DisplayControl.UserIsLoggedIn()
        LaunchReadyCheck()
    End Sub
    Private Sub UnloadUserAccount()
        RemoveHandler Files.UserSettings.FileChanged, AddressOf UserSettingsChanged
        Log = "User Logged Out " & CurrentUserName
        CurrentUserName = Nothing
        Folders.CurrentAccount = Nothing
        Files.UserSettings = Nothing

        DisplayControl.NoUserLoggedIn()
    End Sub

    'Account Controls menu strip
    Private Sub UserLoginToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles UserLoginToolStripMenuItem.Click
        PromptUserLogon()
    End Sub
    Private Sub UserLogoutToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles UserLogoutToolStripMenuItem.Click
        UnloadUserAccount()
    End Sub
    Private Sub CreateUserAccountToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CreateUserAccountToolStripMenuItem.Click
        CreateNewAccount()
    End Sub
    Private Sub DeleteUserAccountToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles DeleteUserAccountToolStripMenuItem.Click
        DeleteAccount()
    End Sub

    'Program Settings menu strip
    Private Sub SetAutostartUserToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SetAutostartUserToolStripMenuItem.Click
        AutoStartAccount_Add(Program.CurrentUserName)
    End Sub
    Private Sub RemoveAutostartUserToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles RemoveAutostartUserToolStripMenuItem.Click
        AutoStartAccount_Remove()
    End Sub

    'User Setup menu strip
    Private Sub CheatEngineAutoAttachisSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CheatEngineAutoAttachisSetupToolStripMenuItem.Click
        SetupCEAutoAttach()
        sender.Checked = CheckCEAutoAttach()
    End Sub
    Private Sub CheatEngineAutoLoadisSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CheatEngineAutoLoadisSetupToolStripMenuItem.Click
        SetupCEAutoLoad()
        sender.Checked = CheckCEAutoLoad()
    End Sub
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
                UserSettingsChanged()
            Else
                If MsgBox("Auto-Attach was already setup, do you want to undo it?", MsgBoxStyle.YesNo, "CE Auto-Attach Setup") = MsgBoxResult.Yes Then
                    reg_value_cheatengine_AutoLaunch = reg_value_cheatengine_AutoLaunch.Replace(EmulatorFileName, "").Replace(";;", ";")
                    If Microsoft.VisualBasic.Left(reg_value_cheatengine_AutoLaunch, 1) = ";" Then
                        reg_value_cheatengine_AutoLaunch = Microsoft.VisualBasic.Right(reg_value_cheatengine_AutoLaunch, reg_value_cheatengine_AutoLaunch.Length - 1)
                    End If
                    My.Computer.Registry.SetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", reg_value_cheatengine_AutoLaunch, Microsoft.Win32.RegistryValueKind.String)
                    UserSettingsChanged()
                Else
                    MsgBox("Auto-Attach Not Removed", MsgBoxStyle.OkOnly, "CE Auto-Attach Setup")
                End If
            End If
        Else
            'ce's autoattach registry key is missing
            Dim EmulatorFileName As String = Microsoft.VisualBasic.Right(Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1), Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).Length() - Program.Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).LastIndexOf("\") - 1)
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", EmulatorFileName, Microsoft.Win32.RegistryValueKind.String)
            MsgBox("Auto-Attach setup complete." & vbNewLine & "The Registry Key for Cheat Engine was created from scratch, so it was missing.", MsgBoxStyle.Information, "CE Auto-Attach Setup")
            UserSettingsChanged()
        End If
    End Sub
    Private Function CheckCEAutoAttach(Optional ByVal _outputToLog As Boolean = True) As Boolean
        Try

            If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing) IsNot Nothing Then
                Dim reg_value_cheatengine_AutoLaunch As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Cheat Engine", "AutoAttach", Nothing).ToString
                Dim EmulatorFileName As String = Microsoft.VisualBasic.Right(Files.UserSettings.GetTaggedDataLine("Emulator Path", 1), Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).Length - Files.UserSettings.GetTaggedDataLine("Emulator Path", 1).LastIndexOf("\") - 1)
                If reg_value_cheatengine_AutoLaunch.Contains(EmulatorFileName) = False Then
                    'red
                    If _outputToLog Then Log = "No CE AutoAttach Setup"
                    Return False
                Else
                    If _outputToLog Then Log = "CE AutoAttach Ready"
                    Return True
                End If
            Else
                'red
                If _outputToLog Then Log = "No CE AutoAttach Setup"
                Return False
            End If

        Catch ex As Exception
        End Try
    End Function
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
                    UserSettingsChanged()
                    Exit Sub
                End If
            ElseIf continueResponse = MsgBoxResult.No Then
                continueResponse = MsgBox("Do you want to remove the Ihtol?", MsgBoxStyle.YesNoCancel, "Modification File Removal Prompt")
                If continueResponse = MsgBoxResult.Yes Then
                    Try
                        My.Computer.FileSystem.DeleteFile(cheatengine_autorunfolder & "Ihtol.lua")

                    Catch ex As Exception
                        If ex.Message = "Access to the path '" & cheatengine_autorunfolder & "Ihtol.lua" & "' is denied." Then
                            MsgBox("Could not access the file, I probably need ran as administrator to")
                        End If
                    End Try
                    If My.Computer.FileSystem.FileExists(cheatengine_autorunfolder & "Ihtol.lua") = False Then
                        MsgBox("Modification file Removed Successfully", MsgBoxStyle.Information, "Modification File Removed Successfully")
                        UserSettingsChanged()
                        Exit Sub
                    Else
                        MsgBox("Modification file could not be removed", MsgBoxStyle.Information, "Modification File Removal Failed")
                        Exit Sub
                    End If
                ElseIf continueResponse = MsgBoxResult.No Then
                    MsgBox("Process complete. No changes made.", MsgBoxStyle.Information, "Modification Process")
                    Exit Sub
                End If
            ElseIf continueResponse = MsgBoxResult.Cancel Then
                MsgBox("No changes made. Cancelling...", MsgBoxStyle.Information, "Cancelling Installation..")
                Exit Sub
            End If
        Else
            Try
                IO.File.WriteAllBytes(cheatengine_autorunfolder & "Ihtol.lua", My.Resources.Ihtol)
            Catch ex As Exception
                If ex.Message = "Access to the path '" & cheatengine_autorunfolder & "Ihtol.lua" & "' is denied." Then
                    MsgBox("Could not access the file, I probably need ran as administrator to")
                End If
            End Try
            If My.Computer.FileSystem.FileExists(cheatengine_autorunfolder & "Ihtol.lua") Then
                MsgBox("Process Complete." & vbNewLine & "Modification file installed.", MsgBoxStyle.Information, "Modification Process")
                UserSettingsChanged()
                Exit Sub
            Else
                'process a did not work, let's try b, then make c and try that i guess lol
                MsgBox("Modification File Failed to Install", MsgBoxStyle.Information, "Modification File Installation Failed")
                Exit Sub
            End If
        End If

        If 1 = 2 Then Exit Sub
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
    End Sub
    Private Function CheckCEAutoLoad(Optional ByVal _outputToLog As Boolean = True) As Boolean
        Dim reg_value_cheatengine As String = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CheatEngine\DefaultIcon", "", Nothing).ToString
        'MsgBox(reg_value_cheatengine)
        Dim cheatengine_rootfolder As String
        cheatengine_rootfolder = Microsoft.VisualBasic.Left(reg_value_cheatengine, reg_value_cheatengine.LastIndexOf("Cheat Engine"))
        If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "autorun\Ihtol.lua") = False Then
            'If My.Computer.FileSystem.FileExists(cheatengine_rootfolder & "Ihtol.lua") = False Then
            'red
            If _outputToLog Then Log = "No CE AutoLoad Setup"
            Return False
            'Else
            '    'green
            '    Log = "CE AutoLoad Ready"
            '    Return True
            'End If
        Else
            'green
            If _outputToLog Then Log = "CE AutoLoad Ready"
            Return True
        End If
    End Function

    Private Sub CheatEngineCheatTablesAreInstalledToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CheatEngineCheatTablesAreInstalledToolStripMenuItem.Click
        SetupFoldersExistence(sender, "Cheat Engine Cheat Table", "Cheat Tables", My.Resources.Cheat_Tables, CheatEngineCheatTablesAreInstalledToolStripMenuItem)
    End Sub
    Private Sub LUAScriptsAreInstalledToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles LUAScriptsAreInstalledToolStripMenuItem.Click
        SetupFoldersExistence(sender, "LUA", "LUAs", My.Resources.LUAs, LUAScriptsAreInstalledToolStripMenuItem)
    End Sub
    Private Sub GameDataIsInstalledToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GameDataIsInstalledToolStripMenuItem.Click
        SetupFoldersExistence(sender, "Game Data", "Game Data", My.Resources.Game_Data, GameDataIsInstalledToolStripMenuItem)
    End Sub
    Private Sub SetupFiles(ByVal _itemName As String, ByVal _locationToInstallAt As String, ByRef _resourceObjectToInstall As Byte())
        Log = "Install " & _itemName & " Files Initiated"
        Dim dc As Boolean = False
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & _locationToInstallAt) Then
            If MsgBox(_itemName & " Files appear to exist. Delete and reinstall Anyway?", MsgBoxStyle.YesNo, "Overwrite Existing Folder?") = MsgBoxResult.No Then
                Log = _itemName & " Files Found. Prompting for Action."
                Exit Sub
            Else
                Log = _itemName & " Files Not Present"
                dc = True
            End If
        End If
        If dc Then My.Computer.FileSystem.DeleteDirectory(Folders.CurrentAccount & _locationToInstallAt, FileIO.DeleteDirectoryOption.DeleteAllContents)
        IO.File.WriteAllBytes(Folders.CurrentAccount & _locationToInstallAt & ".zip", _resourceObjectToInstall)
        IO.Compression.ZipFile.ExtractToDirectory(Folders.CurrentAccount & _locationToInstallAt & ".zip", Folders.CurrentAccount)
        My.Computer.FileSystem.DeleteFile(Folders.CurrentAccount & _locationToInstallAt & ".zip")
        Log = _itemName & " Files Installed"
    End Sub
    Private Sub CheckForFolderExistence(ByVal _itemName As String, ByVal _locationToCheck As String, ByRef _itemToCheck As ToolStripMenuItem, Optional ByVal _outputToLog As Boolean = True)
        If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & _locationToCheck) Then
            If _outputToLog Then Log = _itemName & " Files Present"
            _itemToCheck.Checked = True
        Else
            If _outputToLog Then Log = _itemName & " Files Not Found"
            _itemToCheck.Checked = False
        End If
    End Sub
    Private Sub SetupFoldersExistence(ByRef _sender As System.Object, ByVal _itemName As String, ByVal _locationToDelete As String, ByRef _resourceObjectToInstall As Byte(), ByRef _itemToCheck As ToolStripMenuItem)
        Log = _itemName & " Setup Initiated"
        If _sender.Checked = False Then
            SetupFiles(_itemName, _locationToDelete, _resourceObjectToInstall)
            UserSettingsChanged()
        Else
            Log = "Prompting User to Remove " & _itemName & " Files"
            If MsgBox("Delete " & _itemName & " files?", MsgBoxStyle.YesNo, "Kairen Delete Folder Prompt") = MsgBoxResult.Yes Then
                My.Computer.FileSystem.DeleteDirectory(Folders.CurrentAccount & _locationToDelete & "\", FileIO.DeleteDirectoryOption.DeleteAllContents)
                Log = _itemName & " Files Removed"
                UserSettingsChanged()
            Else
                Log = _itemName & " Files are Unchanged"
            End If
        End If
        CheckForFolderExistence(_itemName, _locationToDelete, _itemToCheck)
    End Sub

    Private Sub EmulatorPathIsSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles EmulatorPathIsSetupToolStripMenuItem.Click
        PathUpdateHandler("Emulator Path", "Enter the location of your PCSX2 program", "Kairen Emulator Path Prompt", EmulatorPathIsSetupToolStripMenuItem)
    End Sub
    Private Sub ISOPathIsSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ISOPathIsSetupToolStripMenuItem.Click
        PathUpdateHandler("ISO Path", "Enter the location of your EQOA .iso", "Kairen Disc Image Path Prompt", ISOPathIsSetupToolStripMenuItem)
    End Sub
    Private Sub WorkDrivePathIsSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles WorkDrivePathIsSetupToolStripMenuItem.Click
        PathUpdateHandler("Work Drive Path", "Enter the location of your Work Drive", "Kairen Work Drive Path Prompt", WorkDrivePathIsSetupToolStripMenuItem)
    End Sub
    Private Sub LanPlayPathIsSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles LanPlayPathIsSetupToolStripMenuItem.Click
        PathUpdateHandler("LAN Play Path", "Enter the location of your LAN Play Folder", "Kairen Lan Play Path Prompt", LanPlayPathIsSetupToolStripMenuItem)
    End Sub
    Private Sub PathUpdateHandler(ByVal _valueName As String, ByVal _userPromptMessage As String, ByVal _userPromptTitle As String, ByRef _menuItemToCheck As ToolStripMenuItem)
        Log = "Prompting user to update " & _valueName
        Dim response As String = PromptUserForStringValue(_userPromptMessage, _userPromptTitle, False, Files.UserSettings.GetTaggedDataLine(_valueName, 1))
        If response <> "" Then
            Files.UserSettings.SetTaggedDataLine(_valueName, 1) = response
            Files.UserSettings.Save()
            _menuItemToCheck.Checked = True
            Log = _valueName & " Saved = " & Files.UserSettings.GetTaggedDataLine(_valueName, 1)
        Else
            Log = "Prompting to Clear Value"
            If MsgBox("Do you want to clear your " & _valueName & "?", MsgBoxStyle.YesNo, "Kairen Clear Value Prompt") = MsgBoxResult.Yes Then
                Files.UserSettings.SetTaggedDataLine(_valueName, 1) = ""
                Files.UserSettings.Save()
                _menuItemToCheck.Checked = False
                Log = _valueName & " Cleared = " & Files.UserSettings.GetTaggedDataLine(_valueName, 1)
            Else
                Log = _valueName & " Unchanged = " & Files.UserSettings.GetTaggedDataLine(_valueName, 1)
            End If
        End If
    End Sub
    Private Function PromptUserForStringValue(ByVal _message As String, ByVal _title As String, ByVal _repromptIfBlank As Boolean, Optional ByVal _defaultResponse As String = Nothing) As String
        Dim userResponse As String = InputBox(_message, _title, _defaultResponse).Trim
        If _repromptIfBlank Then
            userResponse = InputBox("Input cannot be blank." & Environment.NewLine & _message, _title, _defaultResponse).Trim
        End If
        Return userResponse
    End Function
    Private Sub UpdateMenuItem(ByVal _valueToCheck As String, ByRef _menuItemToCheck As ToolStripMenuItem, Optional ByVal _outputToLog As Boolean = True)
        Dim valueOfItem As String = Files.UserSettings.GetTaggedDataLine(_valueToCheck, 1)
        If valueOfItem.Trim <> "" Then
            _menuItemToCheck.Checked = True
            If _outputToLog Then Log = _valueToCheck & " is Setup"
        Else
            _menuItemToCheck.Checked = False
        End If
    End Sub

    Private Function LaunchReadyCheck() As Boolean
        Dim canlaunch As Boolean = True
        For Each item In UserSetupToolStripMenuItem.DropDownItems
            If TypeOf (item) Is ToolStripMenuItem Then
                If item.Checked = False Then
                    canlaunch = False
                    'Return False
                    Exit For
                End If
            End If
        Next
        If canlaunch Then
            Log = "Game is Ready to Launch!"
            DisplayControl.GameIsReadyForLaunch()
        Else
            'Log = "Game is Not Ready to Launch"
            DisplayControl.GameIsNotReadyForLaunch()
        End If
        Return canlaunch
    End Function

    Private Sub UserSettingsChanged()
        DisplayControl.UserSettingsFileUpdated()
        LaunchReadyCheck()
    End Sub

#Region "Check for Updates"
    Private Sub display_button_CheckforUpdates_Click(sender As System.Object, e As System.EventArgs) Handles display_button_CheckforUpdates.Click
        DisplayControl.UpdateNotPresent()
        CheckForUpdates()
    End Sub


    Dim WebsiteDomainAddress As String = "http://eqrh.tk/"
    Dim KairenLatestVersion As String
    Dim WithEvents CheckVersionTimer As Timer
    Private Sub CheckForUpdates()
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
    End Sub
    Private Sub CheckVersionTimer_Tick(sender As System.Object, e As System.EventArgs) Handles CheckVersionTimer.Tick
        Dim testthis As Boolean = False
        If testthis = True Then Log = " ---> Test This is On <--- "

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
            DisplayControl.UpdateNotPresent()
            Exit Sub
        End If
        KairenLatestVersion = verres
        If testthis Then KairenLatestVersion = "399"
        Log = "Latest Version Detected: " & KairenLatestVersion
        If ProgramVersionNumber <> KairenLatestVersion Then
            Log = "Your Kairen Version is: " & ProgramVersionNumber
            Log = "Enabling Program Update"
            DisplayControl.UpdateIsPresent()
        Else
            Log = "Your Kairen Version is Up-to-Date : " & ProgramVersionNumber
            DisplayControl.UpdateNotPresent()
        End If
        CheckVersionTimer.Stop()
        If testthis Then KairenLatestVersion = verres
    End Sub

    Dim WithEvents DownloadUpdateTimer As Timer
    Dim WithEvents InstallUpdateTimer As Timer
    Private Sub display_button_Update_Click(sender As System.Object, e As System.EventArgs) Handles display_button_Update.Click
        DisplayControl.UpdateNotPresent()
        DownloadUpdate()
    End Sub
    Private Sub DownloadUpdate()
        DownloadUpdateTimer = New Timer
        DownloadUpdateTimer.Interval = 1
        DownloadUpdateTimer.Start()
    End Sub
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
    End Sub
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
    End Sub
#End Region

#Region "Launch Code"
    Private Sub display_button_LaunchGame_Click(sender As System.Object, e As System.EventArgs) Handles display_button_LaunchGame.Click
        LaunchGame()
    End Sub
    Private Sub LaunchGame()
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
    End Sub
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
    End Sub
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
    End Sub
#End Region

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles display_button_CloseGame.Click
        CloseGame()
    End Sub
    Private Sub CloseGame()
        Log = " --- Sending CloseAll --- "
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("CloseAll")
        sr.WriteLine()
        sr.Write(Files.UserSettings.GetTaggedDataLine("Lan Play Path", 1))
        sr.WriteLine()
        sr.Close()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        SetLanPlayerName("1", TextBox2.Text)
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        SetLanPlayerName("2", TextBox3.Text)
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        SetLanPlayerName("3", TextBox4.Text)
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        SetLanPlayerName("4", TextBox5.Text)
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        SetMyLanPlayerNumber(NumericUpDown1.Value)
    End Sub
    Private Sub SetMyLanPlayerNumber(ByVal _myLANNumber As Integer)
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetMyLanPlayerNumber")
        sr.WriteLine()
        sr.Write(_myLANNumber)
        sr.WriteLine()
        sr.Close()
    End Sub
    Private Sub SetLanPlayerName(ByVal Number As Integer, ByVal Name As String)
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("SetLanPlayerName")
        sr.WriteLine()
        sr.Write(Number)
        sr.WriteLine()
        sr.Write(Name)
        sr.WriteLine()
        sr.Close()
    End Sub

End Class