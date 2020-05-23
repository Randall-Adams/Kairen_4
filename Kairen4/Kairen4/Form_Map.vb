Public Class Form_Map

    'map is 1920x1680
    'with top and left border lines
    '  zones are 120x120
    '  subzones are 40x40
    '600,0 is (0,0) on map
    '1800,240 is neriak
    '4000,4000 raw from emu is permafrost 0,0
    Private Sub Form_Map_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'PictureBox1.Image = My.Resources.EQOA_Zone_Map.Clone(New Rectangle(1800, 240, 929, 706), My.Resources.EQOA_Zone_Map.PixelFormat)
        Me.Size = New Point(750, 775)
        PictureBox1.Size = New Point(721, 721)
        'Dim CropRect As New Rectangle(600, 0, 800, 721) ' Permafrost
        Dim CropRect As New Rectangle(600, 480, 800, 721) ' wyndhaven
        'Dim CropRect As New Rectangle(720, 1080, 800, 721) ' Geomancer's Citadel
        'Dim CropRect As New Rectangle(0, 960, 800, 721) ' Sylhilthis' Dwell
        'Dim CropRect As New Rectangle(1680, 0, 800, 721) ' Lavastorm
        'Dim CropRect As New Rectangle(1200, 240, 800, 721) ' Moradhim
        Dim OriginalImage = My.Resources.EQOA_Zone_Map
        Dim CropImage = New Bitmap(CropRect.Width, CropRect.Height)
        Using grp = Graphics.FromImage(CropImage)
            grp.DrawImage(OriginalImage, New Rectangle(0, 0, CropRect.Width, CropRect.Height), CropRect, GraphicsUnit.Pixel)
            OriginalImage.Dispose()
            PictureBox1.Image = CropImage
        End Using
    End Sub

    'Dim ClickPoint As New Point(0, 0)
    Private Sub PictureBox1_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp
        'ClickPoint = New Point(e.X, e.Y)
        SetPoint(e.X, e.Y)
    End Sub
    Public Sub SetPoint(ByVal x As Integer, ByVal y As Integer)
        If (x = 0) And (y = 0) Then
            PictureBox2.Size = New Point(0, 0)
        Else
            PictureBox2.Size = New Point(7, 7)
            PictureBox2.Location = New Point(x + 6, y + 6)
            PictureBox2.Image = My.Resources.BlueSquare
        End If
        Me.Text = PictureBox2.Location.X - 6 & ", " & PictureBox2.Location.Y - 6
    End Sub
    Public Sub SetLocation(ByVal x As Integer, ByVal y As Integer)
        x -= 4000
        y -= 4000

        x *= 0.06
        y *= 0.06
        'x /= 16.7
        'y /= 16.7

        'permafrost

        'mora
        'x -= 600
        'y -= 240

        'geo cit
        'y += 120

        'wyndhaven
        ' x -= 480
        y -= 480

        'odus
        'y += 120

        SetPoint(x, y)
    End Sub
End Class