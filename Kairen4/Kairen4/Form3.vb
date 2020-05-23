Imports System.Net, System.Net.Sockets, System.Text.Encoding
Public Class Form3
#Region "Dims"
    Dim DebugIsAppData As Boolean = False
    Private Event Event_ProgramIsReady()
    Private DisplayControl As New class_DisplayControl(Me)
    Private WithEvents RestartTimer As Timer
    Private InstantRestart As Boolean = False
    Private AutoReinstall As Boolean = False

    Private LatestLUAVersion As String = "4.0.0.10"
    Private LatestCEVersion As String = "4.0.0.10"

    Private TestMode As String = ""
    'Private TestMode As String = "EClient"
    'Private TestMode As String = "EServer"
#End Region
#Region "Loadup Code"
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
        ListBox1.Items.Add("Alpha Test")
        CheckBox1.Checked = True
        CheckBox1.Enabled = False
        CheckBox1.Visible = True
        ListBox1.Items.Add("Beta Test")
        CheckBox2.Checked = True
        CheckBox2.Enabled = False
        CheckBox2.Visible = True
        ListBox1.Items.Add("Chi Test")
        CheckBox3.Checked = True
        CheckBox3.Enabled = False
        CheckBox3.Visible = True
        ListBox1.Items.Add("Delta Test")
        CheckBox4.Checked = False
        CheckBox4.Enabled = False
        CheckBox4.Visible = True
        'alpha 'beta 'chi 'delta
        'epsilon, eta 'f - fhi 'gamma 'h - hu 'iota 'j - ji 'kappa 'lambda 'mu 'nu 
        'omicron, omega 'psi, phi, pi 'q - qho 'rho 'sigma 'tau, theta 'upsilon 'v - vi 'w - wi 'xi 'y - yeta 'zeta
    End Sub
    Private Sub Form3_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        If Environment.GetCommandLineArgs.Contains("DebugRelaunch") Then 'debug relaunch program from ide stuff
            AppActivate("Kairen4 - Microsoft Visual Basic 2010 Express")
            AppActivate("Kairen4 - Microsoft Visual Basic 2010 Express (Administrator)")
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
        If Environment.GetCommandLineArgs.Contains("InstantRestarts") Then 'no count down before launchng reinstalled kairen
            InstantRestart = True
        End If
        If Environment.GetCommandLineArgs.Contains("AutoReinstall") Then 'reinstalls kairen without asking
            AutoReinstall = True
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
                        If InstantRestart Then 'debug option
                            RestartTimer.Interval = 1
                        Else
                            RestartTimer.Interval = 15000
                        End If
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
            If Label1.Font.Bold = False Then
                'program log isn't checked for empty since it should have some text in it before the user can do anything
                ProgramLog += Environment.NewLine
                ProgramLog += value
            Else
                If TextBox1.Text <> "" Then TextBox1.AppendText(Environment.NewLine)
                TextBox1.AppendText(value)
            End If
        End Set
    End Property
    Private Sub EventHandler_ProgramIsReady() Handles Me.Event_ProgramIsReady
        'Log = "*program is ready event fired*"
        Log = "Kairen Startup Successful"
        LoadKairen()
        DisplayControl.ProgramIsReady()
        If TestMode <> "" Then
            If My.Computer.Name = "EIDOLON" Then ' eidolon
                TextBox7.Text = "192.168.1.17"
                TextBox8.Text = "4657"
                TextBox2.Text = "192.168.1.14"
                TextBox3.Text = "4657"
                TextBox11.Text = "Eidolon"
                If TestMode = "EServer" Then ' e = server ; t = client
                    RadioButton3.Checked = True
                    RadioButton6.Checked = True
                ElseIf TestMode = "EClient" Then ' e = client ; t = server
                    RadioButton2.Checked = True
                    RadioButton5.Checked = True
                End If
            ElseIf My.Computer.Name = "TOSHI3" Then ' toshi3
                TextBox2.Text = "192.168.1.17"
                TextBox3.Text = "4657"
                TextBox7.Text = "192.168.1.14"
                TextBox8.Text = "4657"
                TextBox11.Text = "Toshie"
                If TestMode = "EServer" Then ' e = server ; t = client
                    RadioButton2.Checked = True
                    RadioButton5.Checked = True
                ElseIf TestMode = "EClient" Then  ' e = client ; t = server
                    RadioButton3.Checked = True
                    RadioButton6.Checked = True
                End If
            End If
        Else
            TextBox8.Text = "4657"
            TextBox3.Text = "4657"
        End If

        If Environment.GetCommandLineArgs.Contains("InstantRestarts") Then 'no count down before launchng reinstalled kairen
            Log = "Note: InstantRestarts was passed."
        End If
        If Environment.GetCommandLineArgs.Contains("AutoReinstall") Then 'reinstalls kairen without asking
            Log = "Note: AutoReinstall was passed."
        End If
        'System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName).AddressList(0)
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
    Private SuppressReadyCheck As Boolean = False
    Private Function LaunchReadyCheck() As Boolean
        If SuppressReadyCheck Then Return False
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
#End Region
#Region "UI Control Code"
    Class class_DisplayControl
        Private _parentForm As Form3
        Sub New(ByRef _setParentForm As Form3)
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
            '_parentForm.UpdateMenuItem("LAN Play Path", _parentForm.LANPlayPathIsSetupToolStripMenuItem)

            _parentForm.TextBox11.Text = Program.CurrentUserName
            _parentForm.TextBox12.Text = Program.CurrentUserName

            _parentForm.Label3.Text = Program.CurrentUserName

            _parentForm.Button5.Enabled = True
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
            ' _parentForm.LANPlayPathIsSetupToolStripMenuItem.Checked = False

            GameIsNotReadyForLaunch()

            _parentForm.Label3.Text = ""

            _parentForm.Button5.Enabled = False
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
        End Sub
        Sub GameIsNotReadyForLaunch()
            _parentForm.display_button_LaunchGame.Enabled = False
            _parentForm.display_button_CloseGame.Enabled = False
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
            '_parentForm.UpdateMenuItem("LAN Play Path", _parentForm.LANPlayPathIsSetupToolStripMenuItem, False)
        End Sub
        Sub ServerHost()
            _parentForm.TextBox7.Enabled = False
            _parentForm.TextBox8.Enabled = False
            _parentForm.Button1.Enabled = False
            _parentForm.RadioButton1.Enabled = False
            _parentForm.RadioButton2.Enabled = False
            _parentForm.RadioButton3.Enabled = False
            '_parentForm.RadioButton4.Enabled = False
            _parentForm.RadioButton5.Enabled = False
            '_parentForm.RadioButton6.Enabled = False
            _parentForm.RadioButton7.Checked = True
            _parentForm.Button7.Enabled = True
            _parentForm.Button8.Enabled = True
            _parentForm.TextBox12.Enabled = False
        End Sub
        Sub ServerUnHost()
            _parentForm.Button8.Enabled = False
            _parentForm.Button7.Enabled = False
            _parentForm.Button1.Enabled = True
            _parentForm.TextBox7.Enabled = True
            _parentForm.TextBox8.Enabled = True
            _parentForm.TextBox12.Enabled = True
            _parentForm.RadioButton1.Enabled = True
            _parentForm.RadioButton2.Enabled = True
            _parentForm.RadioButton3.Enabled = True
            _parentForm.RadioButton4.Enabled = True
            _parentForm.RadioButton5.Enabled = True
            _parentForm.RadioButton6.Enabled = True
        End Sub
        Sub ServerGuest()
            _parentForm.Button3.Enabled = False
            _parentForm.TextBox2.Enabled = False
            _parentForm.TextBox3.Enabled = False
            _parentForm.TextBox11.Enabled = False
            _parentForm.RadioButton1.Enabled = False
            _parentForm.RadioButton2.Enabled = False
            _parentForm.RadioButton3.Enabled = False
            '_parentForm.RadioButton4.Enabled = False
            '_parentForm.RadioButton5.Enabled = False
            _parentForm.RadioButton6.Enabled = False
            '_parentForm.'Button2.Enabled = True
            _parentForm.Button4.Enabled = True
        End Sub
        Sub ServerUnGuest()
            _parentForm.Button4.Enabled = False
            _parentForm.Button2.Enabled = False
            _parentForm.TextBox2.Enabled = True
            _parentForm.TextBox3.Enabled = True
            _parentForm.TextBox11.Enabled = True
            _parentForm.RadioButton1.Enabled = True
            _parentForm.RadioButton2.Enabled = True
            _parentForm.RadioButton3.Enabled = True
            _parentForm.RadioButton4.Enabled = True
            _parentForm.RadioButton5.Enabled = True
            _parentForm.RadioButton6.Enabled = True
            _parentForm.Button3.Enabled = True
        End Sub
    End Class

    'Mode Select
    Private Sub RadioButton1_2_3_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, RadioButton3.CheckedChanged
        If sender Is RadioButton1 Then 'Offline Mode
            RadioButton5.Enabled = Not sender.checked
            RadioButton6.Enabled = Not sender.checked
        ElseIf sender Is RadioButton2 Then 'Client Mode
            RadioButton5.Enabled = sender.checked
            RadioButton6.Enabled = Not sender.checked
        ElseIf sender Is RadioButton3 Then 'Host Mode
            Files.ServerSettings = New Class_TextFile(Folders.CurrentAccount, "Server Settings.txt", False)
            Files.ServerSettings.ReadFile()
            RadioButton6.Enabled = sender.checked
            RadioButton5.Enabled = Not sender.checked
            GroupBox6.Visible = sender.checked
        End If
        RadioButton4.Checked = True
    End Sub

    'Option Select
    Private Sub RadioButton4_5_6_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton4.CheckedChanged, RadioButton5.CheckedChanged, RadioButton6.CheckedChanged
        If sender.checked = False Then Exit Sub
        If TestMode <> "" Then
            If My.Computer.Name = "EIDOLON" Then
                If sender Is RadioButton4 Then 'Offline Options
                    GroupBox3.Visible = sender.checked
                    GroupBox3.Size = New Point(500, 262)
                    GroupBox4.Visible = Not sender.checked
                    GroupBox4.Location = New Point(431, 294)
                    GroupBox4.Size = New Point(187, 262)
                    GroupBox5.Visible = Not sender.checked
                    GroupBox5.Location = New Point(624, 294)
                    GroupBox5.Size = New Point(187, 262)
                ElseIf sender Is RadioButton5 Then 'Client Options
                    GroupBox3.Visible = Not sender.checked
                    GroupBox4.Visible = sender.checked
                    GroupBox5.Visible = Not sender.checked
                    GroupBox4.Location = GroupBox3.Location
                    GroupBox4.Size = New Point(500, 262)
                    GroupBox5.Visible = Not sender.checked
                    GroupBox5.Location = New Point(624, 294)
                    GroupBox5.Size = New Point(187, 262)
                ElseIf sender Is RadioButton6 Then 'Host Options
                    GroupBox3.Visible = Not sender.checked
                    GroupBox4.Visible = Not sender.checked
                    GroupBox4.Location = New Point(431, 294)
                    GroupBox4.Size = New Point(187, 262)
                    GroupBox5.Visible = sender.checked
                    GroupBox5.Location = GroupBox3.Location
                    GroupBox5.Size = New Point(500, 262)
                End If
            ElseIf My.Computer.Name = "TOSHI3" Then
                If sender Is RadioButton4 Then 'Offline Options
                    GroupBox3.Visible = sender.checked
                    GroupBox3.Size = New Point(650, 262)
                    GroupBox4.Visible = Not sender.checked
                    GroupBox4.Location = New Point(431, 294)
                    GroupBox4.Size = New Point(187, 262)
                    GroupBox5.Visible = Not sender.checked
                    GroupBox5.Location = New Point(624, 294)
                    GroupBox5.Size = New Point(187, 262)
                ElseIf sender Is RadioButton5 Then 'Client Options
                    GroupBox3.Visible = Not sender.checked
                    GroupBox4.Visible = sender.checked
                    GroupBox5.Visible = sender.checked
                    GroupBox4.Location = GroupBox3.Location
                    GroupBox4.Size = New Point(650, 262)
                    GroupBox5.Visible = Not sender.checked
                    GroupBox5.Location = New Point(624, 294)
                    GroupBox5.Size = New Point(187, 262)
                ElseIf sender Is RadioButton6 Then 'Host Options
                    GroupBox3.Visible = Not sender.checked
                    GroupBox4.Visible = Not sender.checked
                    GroupBox4.Location = New Point(431, 294)
                    GroupBox4.Size = New Point(187, 262)
                    GroupBox5.Visible = sender.checked
                    GroupBox5.Location = GroupBox3.Location
                    GroupBox5.Size = New Point(650, 262)
                End If
            End If
        Else
            If sender Is RadioButton4 Then 'Offline Options
                GroupBox3.Visible = sender.checked
                GroupBox3.Size = New Point(500, 262)
                GroupBox4.Visible = Not sender.checked
                GroupBox4.Location = New Point(431, 294)
                GroupBox4.Size = New Point(187, 262)
                GroupBox5.Visible = Not sender.checked
                GroupBox5.Location = New Point(624, 294)
                GroupBox5.Size = New Point(187, 262)
            ElseIf sender Is RadioButton5 Then 'Client Options
                GroupBox3.Visible = Not sender.checked
                GroupBox4.Visible = sender.checked
                GroupBox5.Visible = Not sender.checked
                GroupBox4.Location = GroupBox3.Location
                GroupBox4.Size = New Point(500, 262)
                GroupBox5.Visible = Not sender.checked
                GroupBox5.Location = New Point(624, 294)
                GroupBox5.Size = New Point(187, 262)
            ElseIf sender Is RadioButton6 Then 'Host Options
                GroupBox3.Visible = Not sender.checked
                GroupBox4.Visible = Not sender.checked
                GroupBox4.Location = New Point(431, 294)
                GroupBox4.Size = New Point(187, 262)
                GroupBox5.Visible = sender.checked
                GroupBox5.Location = GroupBox3.Location
                GroupBox5.Size = New Point(500, 262)
            End If
        End If
        'split some of the above out into an equivalent ifnot branch?
    End Sub
#End Region
#Region "Install-Related Code"
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
        If AutoReinstall Then
            Log = "AutoReinstall will Reinstall Kairen " & ProgramVersionNumber
            ReinstallKairen()
            Return True
        End If
        Return True 'debug option
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
#End Region
#Region "Modular Code"
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
#End Region
#Region "Account-Related Code"
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
        Files.UserSettings = New Class_TextFile(Folders.CurrentAccount, "UserSettings.txt")
        Files.UserSettings.ReadFile()
        Files.UserSettings.Save()
        CurrentUserName = NewUserName
        Log = "User Settings File Created " & Folders.CurrentAccount & "UserSettings.txt"
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
    Private Sub Label3_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles Label3.MouseUp
        If My.Computer.Name <> "EIDOLON" Then Exit Sub
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If Label3.Text.ToLower.Contains("test") Then
                If MsgBox("Copy this user to a new one?", MsgBoxStyle.YesNo, "DEBUG OPTION Clone User Profile") = MsgBoxResult.Yes Then
                    Dim newname As String = InputBox("Enter New Account's Name", "Select Cloned Profile Name", Label3.Text)
                    If My.Computer.FileSystem.FileExists(Program.Folders.AccountsFolder & newname) Then
                        MsgBox("Failed to clone account -- The account name you entered already exists", MsgBoxStyle.OkOnly, "Failed to Clone Account")
                    Else
                        My.Computer.FileSystem.CopyDirectory(Program.Folders.CurrentAccount, Program.Folders.AccountsFolder & newname)
                        MsgBox("Account Clonation Done", MsgBoxStyle.OkOnly, "Account Cloning Complete")
                    End If
                End If
            End If
        End If
    End Sub 'Account Clonation
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
#End Region
#Region "Menu Strip Code"
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

        SuppressReadyCheck = True
        Program.Files.UserSettings.SetTaggedDataLine(_itemName & " Version", 1) = LatestVersionOf(_itemName)
        SuppressReadyCheck = False
        Program.Files.UserSettings.Save()
        'UserSettingsChanged()
        Log = _itemName & " Files Installed"
    End Sub
    Private Sub CheckForFolderExistence(ByVal _itemName As String, ByVal _locationToCheck As String, ByRef _itemToCheck As ToolStripMenuItem, Optional ByVal _outputToLog As Boolean = True)
        If _itemName = "LUA" Or _itemName = "Cheat Engine Cheat Table" Then
            If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & _locationToCheck) Then
                If _outputToLog Then Log = _itemName & " Files Present"
                If Program.Files.UserSettings.GetTaggedDataLine(_itemName & " Version", 1) <> LatestVersionOf(_itemName) Then
                    If _outputToLog Then Log = _itemName & " Files Out of Date"
                    _itemToCheck.Checked = False
                Else
                    If _outputToLog Then Log = _itemName & " Files Up to Date"
                    _itemToCheck.Checked = True
                End If
            Else
                If _outputToLog Then Log = _itemName & " Files Not Found"
                _itemToCheck.Checked = False
            End If
        Else
            If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & _locationToCheck) Then
                If _outputToLog Then Log = _itemName & " Files Present"
                _itemToCheck.Checked = True
            Else
                If _outputToLog Then Log = _itemName & " Files Not Found"
                _itemToCheck.Checked = False
            End If
        End If
    End Sub
    Private Function LatestVersionOf(ByVal _itemName As String) As String
        Select Case _itemName
            Case "LUA"
                Return LatestLUAVersion
            Case "Cheat Engine Cheat Table"
                Return LatestCEVersion
            Case Else
                Return "0"
        End Select
    End Function
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
        'CheckForFolderExistence(_itemName, _locationToDelete, _itemToCheck) ' removed when adding version data to luas & ce, idk if this was needed? it was giving installed message after launch is ready message
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
    Private Sub LANPlayPathIsSetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs)
        'PathUpdateHandler("LAN Play Path", "Enter the location of your LAN Play Folder", "Kairen LAN Play Path Prompt", LANPlayPathIsSetupToolStripMenuItem)
        MsgBox("breh!?!? i tot yew got ridduh meh?! hwhai ammai tier??? hwhy tis chew tier?")
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
        If Files.UserSettings IsNot Nothing Then
            Dim valueOfItem As String = Files.UserSettings.GetTaggedDataLine(_valueToCheck, 1)
            If valueOfItem IsNot Nothing Then
                If valueOfItem.Trim <> "" Then
                    _menuItemToCheck.Checked = True
                    If _outputToLog Then Log = _valueToCheck & " is Setup"
                    Exit Sub
                End If
            End If
        End If
        _menuItemToCheck.Checked = False
    End Sub
#End Region
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
            Log = "Cancelling Update Check : An Error Occured While Trying Not to Y2K."
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
#Region "Launch/Close Game Code"
    Private Sub display_button_LaunchGame_Click(sender As System.Object, e As System.EventArgs) Handles display_button_LaunchGame.Click
        LaunchGame()
    End Sub
    Private Sub LaunchGame()
        If My.Computer.FileSystem.DirectoryExists(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1)) = False Then
            Log = "Cancelling Launch : Work Drive Not Found"
            Exit Sub
        End If
        If My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt") Then
            My.Computer.FileSystem.DeleteFile(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
        End If
        LaunchGameFullExperience()
        'Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        'sr.Write("SetLanPlayFolder")
        'sr.WriteLine()
        'sr.Write(Files.UserSettings.GetTaggedDataLine("LAN Play Path", 1))
        'sr.WriteLine()
        'sr.Close()
        'SetLanPlayerName
        Log = " --- Launching Complete --- "
        GetRunOptions()
    End Sub
    Private Sub LaunchGameFullExperience()
        ' MsgBox(My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)), MsgBoxStyle.OkOnly, "0")
        'MsgBox(My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("ISO Path", 1)), MsgBoxStyle.OkOnly, "0")
        If My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)) And My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("ISO Path", 1)) Then
            Try
                Dim path As String = Files.UserSettings.GetTaggedDataLine("Emulator Path", 1)
                Dim arg As String = Chr(34) & Files.UserSettings.GetTaggedDataLine("ISO Path", 1) & Chr(34)
                Dim myProcess As New Process
                myProcess.StartInfo.WorkingDirectory = Microsoft.VisualBasic.Left(path, path.LastIndexOf("\"))
                myProcess.StartInfo.FileName = Microsoft.VisualBasic.Right(path, path.Length - path.LastIndexOf("\") - 1)
                myProcess.StartInfo.Arguments = arg
                LaunchCE("_Silent", Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1), "LANPlay")
                Threading.Thread.Sleep(500)
                myProcess.Start()
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.OkOnly, "0 - error")
            End Try
        Else
            'MsgBox("launch", MsgBoxStyle.OkOnly, "0")
            LaunchCE()
        End If
    End Sub
    Private Sub LaunchCE(Optional ByVal _suffix As String = "", Optional ByVal _copyToAndRunFromFolder As String = "", Optional ByVal _cheatTableToUse As String = "MainTable")
        If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder) Then
            'MsgBox(_cheatTableToUse, MsgBoxStyle.OkOnly, "1")
            Try
                'Shell() works too
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "LUAs\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "LUAs\", _copyToAndRunFromFolder & "LUAs\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Cheat Tables\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Cheat Tables\", _copyToAndRunFromFolder & "Cheat Tables\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Game Data\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Game Data\", _copyToAndRunFromFolder & "Game Data\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Custom Data\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Custom Data\", _copyToAndRunFromFolder & "Custom Data\", True)
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Ghosts\") = True Then My.Computer.FileSystem.CopyDirectory(Folders.CurrentAccount & "Ghosts\", _copyToAndRunFromFolder & "Ghosts\", True)
                If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\") = True Then My.Computer.FileSystem.DeleteDirectory(_copyToAndRunFromFolder & "Net Streams\", FileIO.DeleteDirectoryOption.DeleteAllContents)
                'If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\i\") = False Then My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\i\")
                'If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Net Streams\o\") = False Then My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\o\")
                If My.Computer.FileSystem.DirectoryExists(_copyToAndRunFromFolder & "Temp Data\") = True Then My.Computer.FileSystem.DeleteDirectory(_copyToAndRunFromFolder & "Temp Data\", FileIO.DeleteDirectoryOption.DeleteAllContents)
                Threading.Thread.Sleep(10)
                My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Temp Data\")
                My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\")
                My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\i\")
                My.Computer.FileSystem.CreateDirectory(_copyToAndRunFromFolder & "Net Streams\o\")
                Process.Start(_copyToAndRunFromFolder & "Cheat Tables\" & _cheatTableToUse & _suffix & ".CT")
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.OkOnly, "1 - error")
            End Try
        Else
            'MsgBox(_cheatTableToUse, MsgBoxStyle.OkOnly, "2")
            Try
                'Shell() works 
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\")
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\i\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\i\")
                If My.Computer.FileSystem.DirectoryExists(Folders.CurrentAccount & "Net Streams\o\") = False Then My.Computer.FileSystem.CreateDirectory(Folders.CurrentAccount & "Net Streams\o\")
                Process.Start(Folders.CurrentAccount & "Cheat Tables\" & _cheatTableToUse & _suffix & ".CT")
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.OkOnly, "2 - error")
            End Try
        End If
    End Sub
    Private Sub display_button_CloseGame_Click(sender As System.Object, e As System.EventArgs) Handles display_button_CloseGame.Click
        CloseGame()
    End Sub
    Private Sub CloseGame()
        Log = " --- Sending CloseAll --- "
        Dim sr As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sr.Write("CloseAll")
        sr.WriteLine()
        'sr.Write(Files.UserSettings.GetTaggedDataLine("LAN Play Path", 1))
        'sr.WriteLine()
        sr.Close()
    End Sub
#End Region
#Region "UDP Server"
    Class class_UDP_Server
        Private UDP_Sender As New Sockets.UdpClient(0)
        Private UDP_Receiver As Sockets.UdpClient
        Private WithEvents UDP_Timer As New Timer
        Public ReadOnly ServerIP As IPAddress
        Private ServerPort As Integer
        'Private ClientIP As IPAddress '
        Private ClientPort As Integer
        Private ReceivedData As String
        Public Status As String = "Uninitialized"
        Public Event UDPServerDataReceived(ByVal _receivedData As String, ByVal _senderIP As String, ByVal _portUsed As Integer)
        Public Event UDPServerStatusUpdate(ByVal _updatedStatus As String)
        Sub New(ByVal _serverIP As IPAddress, ByVal _serverPort As String)
            ServerIP = _serverIP
            FilteredNew(_serverPort)
        End Sub
        Sub New(ByVal _serverIP As String, ByVal _serverPort As String)
            ServerIP = IPAddress.Parse(_serverIP)
            FilteredNew(_serverPort)
        End Sub
        Private Sub FilteredNew(ByVal _serverPort As String)
            ServerPort = _serverPort
            ClientPort = ServerPort
            UDP_Receiver = New Sockets.UdpClient(ClientPort, Sockets.AddressFamily.InterNetwork)
            UDP_Receiver.Client.ReceiveTimeout = 100
            UDP_Receiver.Client.Blocking = False
        End Sub
        Public Sub StartUDPServer()
            UDP_Timer.Start()
            Status = "Hosting"
            RaiseEvent UDPServerStatusUpdate(Status)
        End Sub
        Public Sub StopUDPServer()
            If UDP_Receiver IsNot Nothing Then UDP_Receiver.Close()
            If UDP_Sender IsNot Nothing Then UDP_Sender.Close()
            UDP_Timer.Stop()
            Status = "Not Hosting"
            RaiseEvent UDPServerStatusUpdate(Status)
        End Sub
        Private Sub timer_UDP_Tick(sender As System.Object, e As System.EventArgs) Handles UDP_Timer.Tick
            Dim endpoint As IPEndPoint = New IPEndPoint(ServerIP, ServerPort)
            If UDP_Receiver.Available > 0 Then
                Try
                    Dim receivedbytes() As Byte = UDP_Receiver.Receive(endpoint)
                    ReceivedData = ASCII.GetString(receivedbytes)
                    If Microsoft.VisualBasic.Left(ASCII.GetString(receivedbytes), 4) <> "Rec:" Then
                        'a'If ASCII.GetString(receivedbytes).Contains("get") = False And Microsoft.VisualBasic.Left(ASCII.GetString(receivedbytes), 4) <> "Rec:" And checkbox_UDPSendACK.Checked = True Then
                        Dim now As String = GetRightNow()
                        'SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]") 'sends ack to server since no ip specd in SendUDP call
                        'If checkbox_UDPEnable_AdditionalReceiverIPs.Checked Then
                        'Dim additionalIPs() As String = textbox_UDPAdditionalReceiverIPs.Text.Split(",")
                        'For Each ip In additionalIPs
                        'SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]", ip)
                        'Next
                        'End If
                        'If (checkbox_UDPACKSendersToo.Checked And (EndPoint.ToString <> textbox_ReceiverIP.Text)) Or (checkbox_UDPEnable_AdditionalReceiverIPs.Checked And (additionalIPs.Contains(EndPoint.Address.ToString) = False)) Then
                        SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]", endpoint.Address.ToString) 'send ack to whatever ip sent the message
                        'End If
                        'a'End If
                        'If CheckBox3.Checked Then swOutput(ASCII.GetString(receivedbytes), EndPoint.Address.ToString)
                        RaiseEvent UDPServerDataReceived(ReceivedData, endpoint.Address.ToString, ServerPort)
                    End If
                Catch ex As Exception
                    'MsgBox("error: " & ex.Message)
                End Try
            End If
        End Sub
        Public Sub SendUDP(ByVal _dataToSend As String, ByVal _ipToSendTo As String)
            FilteredSendUDP(_dataToSend, _ipToSendTo)
        End Sub
        Public Sub SendUDP(ByVal _dataToSend As String, ByVal _ipToSendTo As IPAddress)
            FilteredSendUDP(_dataToSend, _ipToSendTo.ToString)
        End Sub
        Private Sub FilteredSendUDP(ByVal _dataToSend As String, ByVal _ipToSendTo As String)
            UDP_Sender.Connect(_ipToSendTo, ServerPort)
            Dim sendbytes() As Byte = ASCII.GetBytes(_dataToSend)
            UDP_Sender.Send(sendbytes, sendbytes.Length)
        End Sub

        Private Function GetRightNow() As String
            Dim tod, h, m, s As String
            h = TimeOfDay.Hour.ToString
            m = TimeOfDay.Minute.ToString
            s = TimeOfDay.Second.ToString
            If h.Length < 2 Then
                tod = "0" & h
            Else
                tod = h
            End If
            tod += ":"
            If m.Length < 2 Then
                tod += "0" & m
            Else
                tod += m
            End If
            tod += ":"
            If s.Length < 2 Then
                tod += "0" & s
            Else
                tod += s
            End If
            Return tod
        End Function
    End Class

    'starts udp timer
    Private WithEvents Server_UDP As class_UDP_Server
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        'server start hosting
        If TextBox12.Text = "" Then Exit Sub
        DisplayControl.ServerHost()
        Server_UDP = New class_UDP_Server(TextBox7.Text, TextBox8.Text)
        ClientHandler = New class_ClientHandler
        Server_UDP.StartUDPServer()
        MyLocationFile = Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\o\FAPI Data2.txt"
        ConnectedPlayersList = New List(Of String)
    End Sub 'server start hosting
    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        'server stop hosting
        DisplayControl.ServerUnHost()
        timer_OutputLocation.Stop()
        Server_UDP.StopUDPServer()
        Server_UDP = Nothing
        ListBox2.Items.Clear()
    End Sub 'server stop hosting

    Private Sub Button8_Click(sender As System.Object, e As System.EventArgs) Handles Button8.Click
        'server send chat message
        ' Server_UDP.TryUDPSend = TextBox10.Text
        'If My.Computer.Name = "EIDOLON" Then
        '    Server_UDP.SendUDP("[chat]" & TextBox10.Text, "192.168.1.14")
        'ElseIf My.Computer.Name = "TOSHI3" Then
        '    Server_UDP.SendUDP("[chat]" & TextBox10.Text, "192.168.1.17")
        'Else
        If ClientHandler.Clients.Count > 0 Then
            For Each client In ClientHandler.Clients
            Next
        End If
        If PlayerHandler.Players.Count > 0 Then
            For Each player In PlayerHandler.Players
                If CheckBox10.Checked Then
                    Server_UDP.SendUDP("[chat]" & TextBox12.Text & " says: " & TextBox10.Text, player.IPAddress)
                Else
                    Server_UDP.SendUDP("[chat][The Server] says: " & TextBox10.Text, player.IPAddress)
                End If
            Next
        End If
        'End If
        nextmessage = ""
        previousmessage = TextBox10.Text
        TextBox10.Clear()
        If TextBox9.Text <> "" Then TextBox9.Text += Environment.NewLine
        If CheckBox10.Checked Then
            TextBox9.AppendText("You say: " & previousmessage)
        Else
            TextBox9.AppendText("[The Server] says: " & previousmessage)
        End If
    End Sub 'server send chat message
    Dim previousmessage As String
    Dim nextmessage As String
    Private Sub TextBox10_KeyUp(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox10.KeyUp
        If e.KeyCode = Keys.Down Then
            If TextBox10.Text = "" Then TextBox10.Text = nextmessage
            If TextBox10.Text = previousmessage Then TextBox10.Text = ""
        ElseIf e.KeyCode = Keys.Up Then
            If TextBox10.Text = "" Then
                TextBox10.Text = previousmessage
                TextBox10.SelectionStart = TextBox10.TextLength + 1
            End If
            If TextBox10.Text <> "" And TextBox10.Text <> previousmessage Then
                nextmessage = TextBox10.Text
                TextBox10.Text = ""
            End If
        End If
    End Sub
    Private Sub TextBox10_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox10.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            Button8.PerformClick()
        End If
    End Sub

    'mn my name; nr name request; ua unavailabe name; na name accepted; 
    Private Sub UDPServerReceivedData_EventHandler(ByVal _receivedData As String, ByVal _senderIP As String, ByVal _portUsed As Integer) Handles Server_UDP.UDPServerDataReceived
        If ClientHandler.ClientStatusChecker(_senderIP) = False And _receivedData = "[join]" Then
            If Files.ServerSettings.GetTaggedData("Blacklist").Contains(_senderIP) Then
                UDPServerNotes("Blacklisted IP rejected from joining: " & _senderIP)
            Else
                ClientHandler.AddClient(_senderIP)
                Server_UDP.SendUDP("[accepted]", _senderIP)
            End If
        ElseIf ClientHandler.ClientStatusChecker(_senderIP) = False Then
            UDPServerNotes("Nonclient message rejected from: " & _senderIP)
        Else
            If _receivedData = "[leave]" Then
                ClientHandler.RemoveClient(_senderIP)
            ElseIf Microsoft.VisualBasic.Left(_receivedData, 4) = "[mn]" Then
                If NameRequestedList.Contains(_senderIP) Then
                    Select Case PlayerHandler.AddPlayer(Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 4), _senderIP)
                        Case Is = "Success"
                            Server_UDP.SendUDP("[na]", _senderIP)
                            NameRequestedList.Remove(_senderIP)
                            If ClientHandler.Clients.Count > 0 Then timer_OutputLocation.Start()
                        Case Is = "Name Taken"
                            UDPServerNotes("Unavailable Name requested by: " & _senderIP)
                            Server_UDP.SendUDP("[ua]", _senderIP)
                            NameRequestedList.Remove(_senderIP)
                            ClientHandler.RemoveClient(_senderIP)
                    End Select
                Else
                    'should not get a My Name statement landing here .. should be taken care of or dealing with named people already
                End If
            ElseIf Microsoft.VisualBasic.Left(_receivedData, 4) = "[ml]" And CheckBox3.Checked Then
                Dim x, y, z As String
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 4)
                x = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                y = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                z = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)

                'UDPServerNotes(PlayerHandler.GetPlayerName(_senderIP) & " X: " & x)
                'UDPServerNotes(PlayerHandler.GetPlayerName(_senderIP) & " Y: " & y)
                'UDPServerNotes(PlayerHandler.GetPlayerName(_senderIP) & " Z: " & z)
                'UDPServerNotes(PlayerHandler.GetPlayerName(_senderIP) & " F: " & _receivedData)
                For Each player In ConnectedPlayersList
                    If PlayerHandler.GetPlayerName(_senderIP) <> player Then
                        Server_UDP.SendUDP("[upl]" & PlayerHandler.GetPlayerName(_senderIP) & ":" & x & ":" & y & ":" & z & ":" & _receivedData, PlayerHandler.GetPlayerName(player))
                    End If
                Next
                If ConnectedPlayersList.Contains(PlayerHandler.GetPlayerName(_senderIP)) Then
                    If My.Computer.FileSystem.FileExists(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt") = False Then
                        Dim sw As New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt")
                        sw.WriteLine("UpdatePlayerData")
                        sw.WriteLine(PlayerHandler.GetPlayerName(_senderIP))
                        sw.WriteLine(x)
                        sw.WriteLine(y)
                        sw.WriteLine(z)
                        sw.WriteLine(_receivedData)
                        sw.Close()
                    End If
                Else
                    ConnectedPlayersList.Add(PlayerHandler.GetPlayerName(_senderIP))
                    Dim sw As New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Temp Data\" & PlayerHandler.GetPlayerName(_senderIP) & ".txt")
                    sw.WriteLine("0.2.0")
                    sw.WriteLine(PlayerHandler.GetPlayerName(_senderIP))
                    sw.WriteLine(PlayerHandler.GetPlayerName(_senderIP))
                    sw.WriteLine(x)
                    sw.WriteLine(y)
                    sw.WriteLine(z)
                    sw.WriteLine(_receivedData)
                    sw.WriteLine("h")
                    sw.WriteLine("m")
                    sw.WriteLine("c")
                    sw.WriteLine("60")
                    sw.WriteLine("255")
                    sw.Close()
                    Threading.Thread.Sleep(25)
                    sw = New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt")
                    sw.WriteLine("CreateLANPlayer")
                    sw.WriteLine(PlayerHandler.GetPlayerName(_senderIP))
                    'sw.WriteLine(x)
                    'sw.WriteLine(y)
                    'sw.WriteLine(z)
                    'sw.WriteLine(_receivedData)
                    sw.Close()
                End If
                'MsgBox((Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDI_Unreliable.txt"))
            Else
                'chat below
                If Microsoft.VisualBasic.Left(_receivedData, 6) = "[chat]" Then
                    _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 6)
                    If PlayerHandler.GetPlayerName(_senderIP) <> "" Then
                        Dim udpsenddata As String = PlayerHandler.GetPlayerName(_senderIP) & " says: " & _receivedData
                        If TextBox9.Text <> "" Then TextBox9.Text += Environment.NewLine
                        TextBox9.AppendText(udpsenddata)
                        udpsenddata = "[chat]" & udpsenddata
                        If PlayerHandler.Players.Count > 0 Then
                            For Each player In PlayerHandler.Players
                                If player.IPAddress <> _senderIP Then Server_UDP.SendUDP(udpsenddata, player.IPAddress)
                            Next
                        End If
                    Else
                        'nameless player
                    End If
                Else
                    UDPServerNotes("Nonchat message rejected from: " & _senderIP)
                End If
            End If
        End If
    End Sub
    Private Sub UDPServerStatusUpdate_EventHandler(ByVal _updatedStatus As String) Handles Server_UDP.UDPServerStatusUpdate
        UDPServerNotes(_updatedStatus)
        Label14.Text = _updatedStatus
    End Sub

    Private Sub UDPServerNotes(ByVal _newNote As String)
        'If TextBox6.Text <> "" Then TextBox6.Text += Environment.NewLine
        'TextBox6.Text += _newNote
        If TextBox6.Text <> "" Then TextBox6.Text += Environment.NewLine
        TextBox6.AppendText(_newNote)
    End Sub
    Dim NameRequestedList As New List(Of String) 'make this not initialize unless you start hosting a server

    'Client-Control Code
    Class class_ClientHandler
        Public ReadOnly Property Clients As List(Of IPAddress)
            Get
                Return ClientList
            End Get
        End Property
        Private ClientList As New List(Of IPAddress)
        Public Event ClientAdded(ByVal _clientIP As IPAddress)
        Public Event ClientRemoved(ByVal _clientIP As IPAddress)
        Sub New()

        End Sub

        'Check Client's Status
        Public Function ClientStatusChecker(ByVal _clientIP As String)
            Return FilteredClientStatusChecker(IPAddress.Parse(_clientIP))
        End Function
        Public Function ClientStatusChecker(ByVal _clientIP As IPAddress)
            Return FilteredClientStatusChecker(_clientIP)
        End Function
        Private Function FilteredClientStatusChecker(ByVal _clientIP As IPAddress)
            If ClientList.Contains(_clientIP) Then
                Return True
            Else
                Return False
            End If
        End Function

        'Add Client
        Public Function AddClient(ByVal _clientIP As String)
            Return FilteredAddClient(IPAddress.Parse(_clientIP))
        End Function
        Public Function AddClient(ByVal _clientIP As IPAddress)
            Return FilteredAddClient(_clientIP)
        End Function
        Private Function FilteredAddClient(ByVal _clientIP As IPAddress, Optional ByVal _doNotRaiseClientAddedEvent As Boolean = False)
            If ClientList.Contains(_clientIP) = False Then
                ClientList.Add(_clientIP)
                If _doNotRaiseClientAddedEvent = False Then RaiseEvent ClientAdded(_clientIP)
                Return True
            Else
                Return False
            End If
        End Function

        'Remove Client
        Public Function RemoveClient(ByVal _clientIP As String)
            Return FilteredRemoveClient(IPAddress.Parse(_clientIP))
        End Function
        Public Function RemoveClient(ByVal _clientIP As IPAddress)
            Return FilteredRemoveClient(_clientIP)
        End Function
        Private Function FilteredRemoveClient(ByVal _clientIP As IPAddress, Optional ByVal _doNotRaiseClientRemovedEvent As Boolean = False)
            If ClientList.Contains(_clientIP) Then
                ClientList.Remove(_clientIP)
                If _doNotRaiseClientRemovedEvent = False Then RaiseEvent ClientRemoved(_clientIP)
                Return True
            Else
                Return False
            End If
        End Function
    End Class
    Private WithEvents ClientHandler As class_ClientHandler
    Private Sub EventHandler_ClientAdded(ByVal _clientIP As IPAddress) Handles ClientHandler.ClientAdded
        ListBox2.Items.Add(_clientIP.ToString)
        UDPServerNotes("Client Added: " & _clientIP.ToString)
        If CheckBox2.Checked Then
            'ask player for name before below line
            NameRequestedList.Add(_clientIP.ToString)
            Server_UDP.SendUDP("[nr]", _clientIP)
        End If
    End Sub
    Private Sub EventHandler_ClientRemoved(ByVal _clientIP As IPAddress) Handles ClientHandler.ClientRemoved
        ListBox2.Items.Remove(_clientIP.ToString)
        PlayerHandler.RemovePlayer_ByIP(_clientIP)
        ConnectedPlayersList.Remove(PlayerHandler.GetPlayerName(_clientIP.ToString))
        UDPServerNotes("Client Removed: " & _clientIP.ToString)
        If ClientHandler.Clients.Count < 1 Then timer_OutputLocation.Stop()
    End Sub

    'beta test code 'assigning player names
    Class class_PlayersHandler
        Public ReadOnly Property Playersold(Optional ByVal _index As Integer = 0) As List(Of String)
            Get
                Dim returningPlayerList As New List(Of String)
                For Each player In PlayersList
                    returningPlayerList.Add(player.Name)
                Next
                Return returningPlayerList
            End Get
        End Property
        Public ReadOnly Property Players() As List(Of class_PlayerClass)
            Get
                Return PlayersList
            End Get
        End Property
        Private PlayersList As New List(Of class_PlayerClass)
        Sub New()

        End Sub
        Public Function AddPlayer(ByVal _playerName As String, ByVal _playerIPAddress As String) As String
            Dim tp As String = _playerName.ToLower
            tp = tp _
                .Replace("a", "") _
                .Replace("b", "") _
                .Replace("c", "") _
                .Replace("d", "") _
                .Replace("e", "") _
                .Replace("f", "") _
                .Replace("g", "") _
                .Replace("h", "") _
                .Replace("i", "") _
                .Replace("j", "") _
                .Replace("k", "") _
                .Replace("l", "") _
                .Replace("m", "") _
                .Replace("n", "") _
                .Replace("o", "") _
                .Replace("p", "") _
                .Replace("q", "") _
                .Replace("r", "") _
                .Replace("s", "") _
                .Replace("t", "") _
                .Replace("u", "") _
                .Replace("v", "") _
                .Replace("w", "") _
                .Replace("x", "") _
                .Replace("y", "") _
                .Replace("z", "")
            If tp.Length > 0 Then
                Return "Name Taken"
            End If
            Dim PlayerNameAlreadyTaken As Boolean = True
            For Each player In PlayersList
                If player.Name = _playerName Then
                    Return "Name Taken"
                End If
            Next
            PlayersList.Add(New class_PlayerClass(_playerName, _playerIPAddress))
            Return "Success"
        End Function
        Public Function GetPlayerName(ByVal _playerIP As String) As String
            For Each player In PlayersList
                If player.IPAddress = _playerIP Then
                    Return player.Name
                End If
            Next
            Return ""
        End Function
        Public Function RemovePlayer(ByVal _playerName As String) As String
            For Each player In PlayersList
                If player.Name = _playerName Then
                    PlayersList.Remove(player)
                    Return "Success"
                End If
            Next
            Return "Fail"
        End Function
        Public Function RemovePlayer_ByIP(ByVal _playerIP As IPAddress) As String
            For Each player In PlayersList
                If player.IPAddress = _playerIP.ToString Then
                    PlayersList.Remove(player)
                    Return "Success"
                End If
            Next
            Return "Fail"
        End Function
        Public Function RemovePlayer_ByIP(ByVal _playerIP As String) As String
            For Each player In PlayersList
                If player.IPAddress.ToString = _playerIP Then
                    PlayersList.Remove(player)
                    Return "Success"
                End If
            Next
            Return "Fail"
        End Function
        Class class_PlayerClass
            Private pName As String
            Public Property Name As String
                Get
                    Return pName
                End Get
                Set(value As String)
                    pName = value
                End Set
            End Property
            Private pIPAddress As String
            Public Property IPAddress As String
                Get
                    Return pIPAddress
                End Get
                Set(value As String)
                    pIPAddress = value
                End Set
            End Property
            Sub New(ByVal _playerName As String, ByVal _playerIPAddress As String)
                Name = _playerName
                pIPAddress = _playerIPAddress
            End Sub
        End Class
    End Class
    Private PlayerHandler As New class_PlayersHandler

    'Client/Black/White Lists
    Private Sub ListBox2_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles ListBox2.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If sender.SelectedIndex < 0 Then Exit Sub
            If e.Button <> Windows.Forms.MouseButtons.Right Then Exit Sub
            If GroupBox6.Text = "Client List" Then
                ContextMenuStrip1.Items.Clear()
                ContextMenuStrip1.Items.Add("Kick")
                ContextMenuStrip1.Items.Add("Kick and Blacklist")
                ContextMenuStrip1.Items.Add("Blacklist")
                'ContextMenuStrip1.Items.Add("Whitelist")
                ContextMenuStrip1.Show(Cursor.Position)
            ElseIf GroupBox6.Text = "Blacklist" Then
                ContextMenuStrip1.Items.Clear()
                ContextMenuStrip1.Items.Add("Remove")
                ContextMenuStrip1.Show(Cursor.Position)
            ElseIf GroupBox6.Text = "Whitelist" Then
                ContextMenuStrip1.Items.Clear()
                ContextMenuStrip1.Items.Add("Remove")
                ContextMenuStrip1.Show(Cursor.Position)
            End If

        End If
    End Sub
    Private Sub ContextMenuStrip1_ItemClicked(sender As System.Object, e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles ContextMenuStrip1.ItemClicked
        If GroupBox6.Text = "Client List" Then
            Select Case e.ClickedItem.Text
                Case "Kick"
                    UDPServerNotes("Kicking " & ListBox2.SelectedItem.ToString)
                    Server_UDP.SendUDP("[kicked]", ListBox2.SelectedItem.ToString) ', ListBox2.SelectedValue)
                    ClientHandler.RemoveClient(ListBox2.SelectedItem.ToString)
                Case "Kick and Blacklist"
                    UDPServerNotes("Kicking & Blacklisting" & ListBox2.SelectedItem.ToString)
                    Server_UDP.SendUDP("[kicked]", ListBox2.SelectedItem.ToString)
                    Files.ServerSettings.AddToTaggedData("Blacklist") = ListBox2.SelectedItem.ToString
                    Files.ServerSettings.Save()
                    Files.ServerSettings.ReadFile()
                    ClientHandler.RemoveClient(ListBox2.SelectedItem.ToString)
                Case "Blacklist"
                    UDPServerNotes("Blacklisting " & ListBox2.SelectedItem.ToString)
                    Files.ServerSettings.AddToTaggedData("Blacklist") = ListBox2.SelectedItem.ToString
                    Files.ServerSettings.Save()
                    Files.ServerSettings.ReadFile()
                Case "Whitelist"
                    UDPServerNotes("Whitelisting " & ListBox2.SelectedItem.ToString)
                    Files.ServerSettings.AddToTaggedData("Whitelist") = ListBox2.SelectedItem.ToString
                    Files.ServerSettings.Save()
                    Files.ServerSettings.ReadFile()
            End Select
        ElseIf GroupBox6.Text = "Blacklist" Then
            Select Case e.ClickedItem.Text
                Case "Remove"
                    UDPServerNotes("Deblacklisting " & ListBox2.SelectedItem.ToString)
                    Files.ServerSettings.RemoveDataFromTag("Blacklist") = ListBox2.SelectedItem.ToString
                    Files.ServerSettings.Save()
                    Files.ServerSettings.ReadFile()
                    RadioButton8.Checked = False
                    RadioButton8.Checked = True
            End Select
        ElseIf GroupBox6.Text = "Whitelist" Then
            Select Case e.ClickedItem.Text
                Case "Remove"
                    UDPServerNotes("Dewhitelisting " & ListBox2.SelectedItem.ToString)
                    Files.ServerSettings.RemoveDataFromTag("Whitelist") = ListBox2.SelectedItem.ToString
                    Files.ServerSettings.Save()
                    Files.ServerSettings.ReadFile()
                    RadioButton9.Checked = False
                    RadioButton9.Checked = True
            End Select
        End If
    End Sub
    Private Sub RadioButton7_8_9_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton7.CheckedChanged, RadioButton8.CheckedChanged, RadioButton9.CheckedChanged
        If sender.checked = False Then Exit Sub
        If sender Is RadioButton7 Then
            ListBox2.Items.Clear()
            GroupBox6.Text = "Client List"
            If ClientHandler IsNot Nothing Then
                For Each item In ClientHandler.Clients
                    ListBox2.Items.Add(item)
                Next
            End If
        ElseIf sender Is RadioButton8 Then
            ListBox2.Items.Clear()
            GroupBox6.Text = "Blacklist"
            '            If Files.ServerSettings.GetTaggedData("Blacklist").Count > 0 Then
            If Files.ServerSettings IsNot Nothing Then

                For Each item In Files.ServerSettings.GetTaggedData("Blacklist")
                    ListBox2.Items.Add(item)
                Next
            End If
        ElseIf sender Is RadioButton9 Then
            ListBox2.Items.Clear()
            GroupBox6.Text = "Whitelist"
            'If Files.ServerSettings.GetTaggedData("Whitelist").Count > 0 Then
            If Files.ServerSettings IsNot Nothing Then
                For Each item In Files.ServerSettings.GetTaggedData("Whitelist")
                    ListBox2.Items.Add(item)
                Next
            End If
        End If
    End Sub
#End Region
#Region "UDP Client"
    Class class_UDP_Client
        Private UDP_Sender As New Sockets.UdpClient(0)
        Private UDP_Receiver As Sockets.UdpClient
        Private WithEvents UDP_Timer As New Timer
        Public ReadOnly ServerIP As IPAddress
        Private ServerPort As Integer
        'Private ClientIP As IPAddress '
        Private ClientPort As Integer
        Private ReceivedData As String
        Public Status As String = "Uninitialized"
        Public Event UDPServerDataReceived(ByVal _receivedData As String, ByVal _senderIP As String, ByVal _portUsed As Integer)
        Public Event UDPServerStatusUpdate(ByVal _updatedStatus As String)
        Sub New(ByVal _serverIP As IPAddress, ByVal _serverPort As String)
            ServerIP = _serverIP
            FilteredNew(_serverPort)
        End Sub
        Sub New(ByVal _serverIP As String, ByVal _serverPort As String)
            ServerIP = IPAddress.Parse(_serverIP)
            FilteredNew(_serverPort)
        End Sub
        Private Sub FilteredNew(ByVal _serverPort As String)
            ServerPort = _serverPort
            ClientPort = ServerPort
            UDP_Receiver = New Sockets.UdpClient(ClientPort, Sockets.AddressFamily.InterNetwork)
            UDP_Receiver.Client.ReceiveTimeout = 100
            UDP_Receiver.Client.Blocking = False
        End Sub
        Public Sub StartUDPServer()
            UDP_Timer.Start()
            Status = "Hosting"
            RaiseEvent UDPServerStatusUpdate(Status)
        End Sub
        Public Sub StopUDPServer()
            If UDP_Receiver IsNot Nothing Then UDP_Receiver.Close()
            If UDP_Sender IsNot Nothing Then UDP_Sender.Close()
            UDP_Timer.Stop()
            Status = "Not Hosting"
            RaiseEvent UDPServerStatusUpdate(Status)
        End Sub
        Private Sub timer_UDP_Tick(sender As System.Object, e As System.EventArgs) Handles UDP_Timer.Tick
            Dim endpoint As IPEndPoint = New IPEndPoint(ServerIP, ServerPort)
            If UDP_Receiver.Available > 0 Then
                Try
                    Dim receivedbytes() As Byte = UDP_Receiver.Receive(endpoint)
                    ReceivedData = ASCII.GetString(receivedbytes)
                    If Microsoft.VisualBasic.Left(ASCII.GetString(receivedbytes), 4) <> "Rec:" Then
                        'a'If ASCII.GetString(receivedbytes).Contains("get") = False And Microsoft.VisualBasic.Left(ASCII.GetString(receivedbytes), 4) <> "Rec:" And checkbox_UDPSendACK.Checked = True Then
                        Dim now As String = GetRightNow()
                        'SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]") 'sends ack to server since no ip specd in SendUDP call
                        'If checkbox_UDPEnable_AdditionalReceiverIPs.Checked Then
                        'Dim additionalIPs() As String = textbox_UDPAdditionalReceiverIPs.Text.Split(",")
                        'For Each ip In additionalIPs
                        'SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]", ip)
                        'Next
                        'End If
                        'If (checkbox_UDPACKSendersToo.Checked And (EndPoint.ToString <> textbox_ReceiverIP.Text)) Or (checkbox_UDPEnable_AdditionalReceiverIPs.Checked And (additionalIPs.Contains(EndPoint.Address.ToString) = False)) Then
                        SendUDP("Rec: " & ASCII.GetString(receivedbytes) & " [" & now & "]", endpoint.Address.ToString) 'send ack to whatever ip sent the message
                        'End If
                        'a'End If
                        'If CheckBox3.Checked Then swOutput(ASCII.GetString(receivedbytes), EndPoint.Address.ToString)
                        RaiseEvent UDPServerDataReceived(ReceivedData, endpoint.Address.ToString, ServerPort)
                    End If
                Catch ex As Exception

                    'MsgBox("error: " & ex.Message)
                End Try
            End If
        End Sub
        Public Sub SendUDP(ByVal _dataToSend As String, Optional ByVal _ipToSendTo As String = Nothing)
            If _ipToSendTo = Nothing Then _ipToSendTo = ServerIP.ToString
            UDP_Sender.Connect(_ipToSendTo, ServerPort)
            Dim sendbytes() As Byte = ASCII.GetBytes(_dataToSend)
            UDP_Sender.Send(sendbytes, sendbytes.Length)
        End Sub

        Private Function GetRightNow() As String
            Dim tod, h, m, s As String
            h = TimeOfDay.Hour.ToString
            m = TimeOfDay.Minute.ToString
            s = TimeOfDay.Second.ToString
            If h.Length < 2 Then
                tod = "0" & h
            Else
                tod = h
            End If
            tod += ":"
            If m.Length < 2 Then
                tod += "0" & m
            Else
                tod += m
            End If
            tod += ":"
            If s.Length < 2 Then
                tod += "0" & s
            Else
                tod += s
            End If
            Return tod
        End Function
    End Class

    'guest options pane kinda starts here
    Private WithEvents Client_UDP As class_UDP_Client
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        If TestMode <> "" Then
            If My.Computer.Name = "TOSHI3" Then
                toshiereconnecttimer.Interval = 900
                toshiereconnecttimer.Start()
            End If
        End If
        'client connect to server
        DisplayControl.ServerGuest()
        Client_UDP = New class_UDP_Client(TextBox2.Text, TextBox3.Text)
        Client_UDP.StartUDPServer()
        Client_UDP.SendUDP("[join]")
        clientConnectionTimeout_timer.Interval = 3000
        clientConnectionTimeout_timer.Start()
        ConnectedPlayersList = New List(Of String)
        MyLocationFile = Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\o\FAPI Data2.txt"
    End Sub 'client connect to server
    Private WithEvents clientConnectionTimeout_timer As New Timer
    Private Sub clientConnectionTimeout_sub(sender As System.Object, e As System.EventArgs) Handles clientConnectionTimeout_timer.Tick
        UDPServerNotes("Failed to connect to server")
        DisconnectClientCode()
        clientConnectionTimeout_timer.Stop()
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        'client disconnect from server
        UDPClientNotes("Leaving Server")
        DisconnectClientCode()
    End Sub 'client disconnect from server
    Private Sub DisconnectClientCode(Optional ByVal _sendLeaveMessage As Boolean = True)
        DisplayControl.ServerUnGuest()
        timer_OutputLocation.Stop()
        If _sendLeaveMessage Then Client_UDP.SendUDP("[leave]")
        Client_UDP.StopUDPServer()
        Client_UDP = Nothing
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        'client send chat message
        nextmessage = ""
        previousmessage = TextBox4.Text
        TextBox4.Clear()
        If TextBox5.Text <> "" Then TextBox5.Text += Environment.NewLine
        TextBox5.AppendText("You say: " & previousmessage)
        Client_UDP.SendUDP("[chat]" & previousmessage)
    End Sub 'client send chat message
    Private Sub TextBox4_KeyUp(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox4.KeyUp
        If e.KeyCode = Keys.Down Then
            If TextBox4.Text = "" Then TextBox4.Text = nextmessage
            If TextBox4.Text = previousmessage Then TextBox4.Text = ""
        ElseIf e.KeyCode = Keys.Up Then
            If TextBox4.Text = "" Then
                TextBox4.Text = previousmessage
                TextBox4.SelectionStart = TextBox4.TextLength + 1
            End If
            If TextBox4.Text <> "" And TextBox4.Text <> previousmessage Then
                nextmessage = TextBox4.Text
                TextBox4.Text = ""
            End If
        End If
    End Sub
    Private Sub TextBox4_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox4.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            Button2.PerformClick()
        End If
    End Sub

    Private Sub UDPClientReceivedData_EventHandler(ByVal _receivedData As String, ByVal _senderIP As String, ByVal _portUsed As Integer) Handles Client_UDP.UDPServerDataReceived
        If _senderIP <> Client_UDP.ServerIP.ToString Then
            UDPServerNotes("Nonserver message rejected from: " & _senderIP)
            'chat below
        Else
            If _receivedData = "[kicked]" Then
                DisconnectClientCode(False)
            ElseIf _receivedData = "[accepted]" Then
                clientConnectionTimeout_timer.Stop()
                UDPServerNotes("Connected")
                Label8.Text = "Connected"
                Button2.Enabled = True
            ElseIf _receivedData = "[nr]" Then
                If TextBox11.Text.Length <= 13 Then 'basically a pointless check since the tb is limited to 13, but hey!
                    Client_UDP.SendUDP("[mn]" & TextBox11.Text)
                Else
                    DisconnectClientCode(True)
                End If
            ElseIf _receivedData = "[ua]" Then
                DisconnectClientCode(False)
                UDPServerNotes("Your desired name is unavailable. Please try again with a different one.")
            ElseIf _receivedData = "[na]" Then
                UDPServerNotes("Your name has been accepted!")
                timer_OutputLocation.Start()
            ElseIf Microsoft.VisualBasic.Left(_receivedData, 5) = "[upl]" Then 'update player location
                Dim pName As String
                Dim x, y, z As String

                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 5)
                ' _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                pName = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))

                '_receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 7)
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                x = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))

                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                y = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))

                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)
                z = Microsoft.VisualBasic.Left(_receivedData, _receivedData.IndexOf(":"))

                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - _receivedData.IndexOf(":") - 1)

                If ConnectedPlayersList.Contains(pName) Then
                    If My.Computer.FileSystem.FileExists(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt") = False Then
                        Dim sw As New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt")
                        sw.WriteLine("UpdatePlayerData")
                        sw.WriteLine(pName)
                        sw.WriteLine(x)
                        sw.WriteLine(y)
                        sw.WriteLine(z)
                        sw.WriteLine(_receivedData)
                        sw.Close()
                    End If
                Else
                    ConnectedPlayersList.Add(pName)
                    Dim sw As New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Temp Data\" & pName & ".txt")
                    sw.WriteLine("0.2.0")
                    sw.WriteLine(pName)
                    sw.WriteLine(pName)
                    sw.WriteLine(x)
                    sw.WriteLine(y)
                    sw.WriteLine(z)
                    sw.WriteLine(_receivedData)
                    sw.WriteLine("h")
                    sw.WriteLine("m")
                    sw.WriteLine("c")
                    sw.WriteLine("60")
                    sw.WriteLine("255")
                    sw.Close()
                    Threading.Thread.Sleep(25)
                    sw = New IO.StreamWriter(Program.Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FDi_Unreliable.txt")
                    sw.WriteLine("CreateLANPlayer")
                    sw.WriteLine(pName)
                    'sw.WriteLine(x)
                    'sw.WriteLine(y)
                    'sw.WriteLine(z)
                    'sw.WriteLine(_receivedData)
                    sw.Close()
                End If
            ElseIf Microsoft.VisualBasic.Left(_receivedData, 6) = "[chat]" Then
                _receivedData = Microsoft.VisualBasic.Right(_receivedData, _receivedData.Length - 6)
                If TextBox5.Text <> "" Then TextBox5.Text += Environment.NewLine
                'TextBox5.AppendText(_senderIP & " says: " & _receivedData)
                TextBox5.AppendText(_receivedData)
            Else
                UDPServerNotes("Nonchat message from server: " & _receivedData)
            End If
        End If
    End Sub
    Private Sub UDPClientStatusUpdate_EventHandler(ByVal _updatedStatus As String) Handles Client_UDP.UDPServerStatusUpdate
        If _updatedStatus = "Hosting" Then
            UDPClientNotes("Connecting to " & TextBox2.Text)
            Label8.Text = "Connecting"
        ElseIf _updatedStatus = "Not Hosting" Then
            Label8.Text = "Not Connected"
            UDPClientNotes("Not Connected")
        End If
    End Sub
    Private Sub UDPClientNotes(ByVal _newNote As String)
        'If TextBox6.Text <> "" Then TextBox6.Text += Environment.NewLine
        'TextBox6.Text += _newNote
        If TextBox6.Text <> "" Then TextBox6.AppendText(Environment.NewLine)
        TextBox6.AppendText(_newNote)
    End Sub
#End Region
#Region "Client-Server Code"
    'Client And Server
    'chi test code sending player location
    Dim MyLocationFile As String
    Dim MyLocationSR As IO.StreamReader
    Dim MyX, MyY, MyZ, MyF As String
    Private Sub timer_OutputLocation_Tick(sender As System.Object, e As System.EventArgs) Handles timer_OutputLocation.Tick
        UDPServerNotes("Hit")
        Try
            If My.Computer.FileSystem.FileExists(MyLocationFile) Then
                MyLocationSR = New IO.StreamReader(MyLocationFile)
                MyX = MyLocationSR.ReadLine
                MyLocationSR.ReadLine()
                MyY = MyLocationSR.ReadLine
                MyLocationSR.ReadLine()
                MyZ = MyLocationSR.ReadLine
                MyLocationSR.ReadLine()
                MyF = MyLocationSR.ReadLine
                MyLocationSR.Close()
                My.Computer.FileSystem.DeleteFile(MyLocationFile, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
                If RadioButton2.Checked Then
                    Client_UDP.SendUDP("[ml]" & MyX & ":" & MyY & ":" & MyZ & ":" & MyF)
                ElseIf RadioButton3.Checked Then
                    For Each client In ClientHandler.Clients
                        If client.ToString <> TextBox7.Text Then Server_UDP.SendUDP("[upl]" & TextBox12.Text & ":" & MyX & ":" & MyY & ":" & MyZ & ":" & MyF, client)
                    Next
                End If
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private ConnectedPlayersList As New List(Of String) 'CS List of Player Names
#End Region
#Region "Program Logs Code"
    Dim ProgramLog As String
    Dim UpdateNotes As String = _
        "Update Notes - Kairen 4.0.0.12 - Unreleased Version" & Environment.NewLine & _
         Environment.NewLine & _
        "Update Notes - Kairen 4.0.0.11 - 04/30/2019" & Environment.NewLine & _
        "   -Added a test version of the NPC Dialogue Documenter" & Environment.NewLine & _
         Environment.NewLine & _
        "Update Notes - Kairen 4.0.0.10 - 04/28/2019" & Environment.NewLine & _
        "   -Added Version Checks for LUA & Cheat Table Installations" & Environment.NewLine & _
        "   -Fixed Error that misnamed the User Settings file" & Environment.NewLine & _
        "   -Fixed Issues with Kairen incorrectly thinking items weren't installed" & Environment.NewLine & _
        "   -Fixed Offline Options going unselectable" & Environment.NewLine & _
        "   -Updated RunOptions Functionality" & Environment.NewLine & _
         Environment.NewLine & _
        "Update Notes - Kairen 4.0.0.9 - 04/28/2019" & Environment.NewLine & _
        "   -Added Ability to Toggle LUA RunOptions in Kairen" & Environment.NewLine & _
        "   -Added Ability to Toggle sending messages as the server or a user" & Environment.NewLine & _
        "   -Updated Ability for the server host to choose a Username" & Environment.NewLine & _
         Environment.NewLine & _
        "Update Notes - Kairen 4.0.0.8 - 04/26/2019" & Environment.NewLine & _
        "   -Added functions to detect your IP Addresses" & Environment.NewLine & _
        "   -Added Toggle Ability to toggle through found IP Addresses" & Environment.NewLine & _
        "   -Added port 4657 to be filled in automatically" & Environment.NewLine & _
        "   -Removed ability to toggle Chi Test" & Environment.NewLine & _
        "   -Changed " & Chr(34) & "Form3" & Chr(34) & " to " & Chr(34) & "Kairen" & Chr(34) & Environment.NewLine & _
        "   -Fixed Program Logs from sometimes going to the Update Notes log" & Environment.NewLine & _
        "   -Fixed error when trying to write player location while player location is being read" & Environment.NewLine & _
         Environment.NewLine & _
        "Update Notes - Kairen 4.0.0.7 - 04/24/2019" & Environment.NewLine & _
        "   -Added " & Chr(34) & "Update Notes" & Chr(34) & " section to Kairen." & Environment.NewLine & _
        "   -Fixed " & Chr(34) & "Server Options" & Chr(34) & " and " & Chr(34) & "Client Options" & Chr(34) & " windows from displaying incorrectly"
    Private Sub Label1_17_Click(sender As System.Object, e As System.EventArgs) Handles Label1.Click, Label17.Click
        'unset unselected here
        If sender IsNot Label1 And Label1.Font.Bold Then
            Label1.Font = New Font(Label1.Font, FontStyle.Regular)
            ProgramLog = TextBox1.Text
        ElseIf sender IsNot Label17 And Label17.Font.Bold Then
            Label17.Font = New Font(Label17.Font, FontStyle.Regular)
            UpdateNotes = TextBox1.Text
        End If

        'set selected here
        If sender Is Label1 Then
            Label1.Font = New Font(Label1.Font, FontStyle.Bold)
            TextBox1.Text = ProgramLog
        ElseIf sender Is Label17 Then
            Label17.Font = New Font(Label17.Font, FontStyle.Bold)
            TextBox1.Text = UpdateNotes
        End If
    End Sub
#End Region
#Region "Get IP Code"
    Public Function TryIPGet() As String
        Try
            Dim rt As String
            Dim wRequest As WebRequest
            Dim wResponse As WebResponse
            Dim SR As IO.StreamReader
            wRequest = WebRequest.Create("https://www.ip-adress.com/")
            wResponse = wRequest.GetResponse
            SR = New IO.StreamReader(wResponse.GetResponseStream)
            rt = SR.ReadToEnd
            SR.Close()
            rt = Mid(rt, rt.IndexOf("Your IP address is: <strong>") + 1)
            rt = Microsoft.VisualBasic.Left(rt, rt.IndexOf("</strong></h1>"))
            rt = Microsoft.VisualBasic.Right(rt, rt.IndexOf("<strong>") - 6)
            Return rt
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Private Sub GetMyHouseholdIPToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GetMyHouseholdIPToolStripMenuItem.Click
        For Each ip In System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName).AddressList
            Log = "Detected Household IP Address: " & ip.ToString
            If MyIPs.Contains(ip.ToString) = False Then MyIPs.Add(ip.ToString)
        Next
    End Sub
    Dim MyIPs As New List(Of String)
    Private Sub GetMyInternetIPToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GetMyInternetIPToolStripMenuItem.Click
        Log = "Reading the webpage " & Chr(34) & "https://www.ip-adress.com/" & Chr(34) & " for your IP address.."
        getiptimer = New Timer
        getiptimer.Start()
    End Sub
    Dim WithEvents getiptimer As Timer
    Private Sub getiptimer_Tick(sender As System.Object, e As System.EventArgs) Handles getiptimer.Tick
        Dim posip As String = TryIPGet()
        Try
            IPAddress.Parse(posip)
            Log = "Detected Internet IP Address: " & posip
            If MyIPs.Contains(posip) = False Then MyIPs.Add(posip)
        Catch ex As Exception
            Log = "Error: Unable to retrieve your IP Address at this time."
            Log = "       You can google for it with this: " & Chr(34) & "What is my IP?" & Chr(34)
        End Try
        getiptimer.Stop()
    End Sub
    Private Sub Label11_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Label11.MouseEnter
        'If MyIPs.Count > 0 Then Cursor = Cursors.Help
        Cursor = Cursors.Help
    End Sub
    Private Sub Label11_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Label11.MouseLeave
        If Cursor = Cursors.Help Then Cursor = Cursors.Default
        showingtooltip = False
    End Sub
    Private Sub Label11_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles Label11.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If MyIPs.Count <= 0 Then
                '   TemporaryCursorChanger(Cursors.No)
                Cursor = Cursors.Default
            Else
                If TextBox7.Text = "" Then
                    TextBox7.Text = MyIPs.Item(0)
                Else
                    For i As Integer = 0 To MyIPs.Count - 1
                        If TextBox7.Text = MyIPs.Item(i) Then
                            If i <= MyIPs.Count - 2 Then
                                TextBox7.Text = MyIPs.Item(i + 1)
                                Exit For
                            Else
                                TextBox7.Text = MyIPs.Item(0)
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
        End If
    End Sub
    Dim showingtooltip As Boolean = False
    Private Sub Label11_MouseHover(sender As System.Object, e As System.EventArgs) Handles Label11.MouseHover
        If showingtooltip Then Exit Sub
        showingtooltip = True
        If MyIPs.Count < 1 Then
            ToolTip1.ToolTipTitle = "Use the [Help] -> [Get IP] functions to detect IPs first"
            ToolTip1.Show("Right Click to cycle through detected IP Addresses", Label11)
        Else
            ToolTip1.ToolTipTitle = Nothing
            ToolTip1.Show("Right Click to cycle through detected IP Addresses", Label11)
        End If
    End Sub
    Private Sub TemporaryCursorChanger(ByVal _NewTempStyle As Cursor)
        timer_TemporaryCursorChanger.Interval = 100
        Cursor = _NewTempStyle
        timer_TemporaryCursorChanger.Start()
    End Sub
    Dim WithEvents timer_TemporaryCursorChanger As New Timer
    Private Sub timer_TemporaryCursorChanger_Tick(sender As System.Object, e As System.EventArgs) Handles timer_TemporaryCursorChanger.Tick
        Cursor = Cursors.Default
        'Threading.Thread.Sleep(1000)
        timer_TemporaryCursorChanger.Stop()
    End Sub
    Private Sub Label11_MouseDown(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles Label11.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If MyIPs.Count <= 0 Then
                Cursor = Cursors.No
            End If
        End If
    End Sub
#End Region
#Region "RunOptions Interfacing Code"
    Private Sub GetRunOptions(Optional ByVal _waitBeforeAsk As Boolean = False)
        If _waitBeforeAsk Then
            RunOptionsWaitBeforeAsk = "3"
        Else
            RunOptionsWaitBeforeAsk = "2"
        End If
        timer_RunOptionsGetter.Start()
    End Sub
    Private Sub Button10_Click(sender As System.Object, e As System.EventArgs) Handles Button10.Click
        Log = " --- Requesting RunOptions List --- "
        GetRunOptions()
    End Sub
    Private RunOptionsWaitBeforeAsk As String = "0"
    Private WithEvents timer_RunOptionsGetter As New Timer
    Private Sub timer_RunOptionsGetter_Tick(sender As System.Object, e As System.EventArgs) Handles timer_RunOptionsGetter.Tick
        If RunOptionsWaitBeforeAsk = "3" Then
            If My.Computer.FileSystem.FileExists(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt") = False Then RunOptionsWaitBeforeAsk = "2"
        ElseIf RunOptionsWaitBeforeAsk = "2" Then
            Dim sw As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
            sw.Write("GetRunOptions")
            sw.Close()
            RunOptionsWaitBeforeAsk = "1"
        ElseIf RunOptionsWaitBeforeAsk = "1" Then
            If My.Computer.FileSystem.FileExists("Q:\Net Streams\o\FAPI Data Request.txt") Then
                Dim OptionName(-1) As String
                Dim OptionSetting(-1) As String
                Dim sr As New IO.StreamReader("Q:\Net Streams\o\FAPI Data Request.txt")
                Do Until sr.EndOfStream
                    ReDim Preserve OptionName(OptionName.Length)
                    OptionName(OptionName.Length - 1) = sr.ReadLine
                    ReDim Preserve OptionSetting(OptionSetting.Length)
                    OptionSetting(OptionSetting.Length - 1) = sr.ReadLine
                Loop
                sr.Close()
                Button11.Enabled = False
                ListBox3.Items.Clear()
                For i As Integer = 0 To OptionName.Length - 1
                    ListBox3.Items.Add(OptionName(i) & ".Do = " & OptionSetting(i))
                Next
                timer_RunOptionsGetter.Stop()
                My.Computer.FileSystem.DeleteFile("Q:\Net Streams\o\FAPI Data Request.txt")
            End If
            'Q:\Net Streams\o\FAPI Data Request.txt
        Else
            timer_RunOptionsGetter.Stop()
        End If
    End Sub
    Private Sub Button11_Click(sender As System.Object, e As System.EventArgs) Handles Button11.Click
        If ListBox3.SelectedIndex = -1 Then Exit Sub
        Dim optionToToggle As String = Microsoft.VisualBasic.Left(ListBox3.SelectedItem.ToString, ListBox3.SelectedItem.ToString.IndexOf(".Do = "))
        Dim optionsCurrentValue As String = Microsoft.VisualBasic.Right(ListBox3.SelectedItem.ToString, ListBox3.SelectedItem.ToString.Length - ListBox3.SelectedItem.ToString.IndexOf(".Do = ") - 6)
        'MsgBox("[" & optionToToggle & "] is [" & Microsoft.VisualBasic.Right(ListBox3.SelectedItem.ToString, ListBox3.SelectedItem.ToString.Length - ListBox3.SelectedItem.ToString.IndexOf(".Do = ") - 6) & "]")
        Dim sw As New IO.StreamWriter(Files.UserSettings.GetTaggedDataLine("Work Drive Path", 1) & "Net Streams\i\FAPI Data Request.txt")
        sw.WriteLine("SetRunOption")
        sw.WriteLine(optionToToggle)
        sw.Write(Not CBool(optionsCurrentValue))
        sw.Close()     
        GetRunOptions(True)
    End Sub
    Private Sub ListBox3_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ListBox3.SelectedIndexChanged
        'exit sub when button is set to false, otherwise let fall through to be set to true
        'this is a filter
        If ListBox3.SelectedIndex = -1 Then
            Button11.Enabled = False
            Exit Sub
        End If
        If Microsoft.VisualBasic.Left(ListBox3.SelectedItem.ToString, ListBox3.SelectedItem.ToString.IndexOf(".Do = ")) = "UpdateKanizah" Then
            Button11.Enabled = False
            Exit Sub
        End If
        Button11.Enabled = True
    End Sub
#End Region
#Region "Personal-Level Debug Code"
    Private WithEvents toshiereconnecttimer As New Timer
    Private Sub toshiereconnecttimer_Tick(sender As System.Object, e As System.EventArgs) Handles toshiereconnecttimer.Tick
        If Label8.Text <> "Connected" Then
            Button3.PerformClick()
        Else
            toshiereconnecttimer.Stop()
        End If
    End Sub
#End Region

    Dim DialogueDocumenter As Form_DialogueDocumenter
    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        If DialogueDocumenter Is Nothing Then
            DialogueDocumenter = New Form_DialogueDocumenter(Me)
            DialogueDocumenter.Show()
        ElseIf DialogueDocumenter.IsDisposed Then
            DialogueDocumenter = New Form_DialogueDocumenter(Me)
            DialogueDocumenter.Show()
        End If
    End Sub

End Class