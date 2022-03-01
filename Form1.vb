Imports System.Net.NetworkInformation

Public Class Form1

    Private mousePoint As Point
    Private ProxyTag As Boolean = False
    Private showBalloon As Boolean = True

    Private INIfile As String = AppDomain.CurrentDomain.BaseDirectory & "ProxyAutoChange.ini"
    Private Sub EnabledProxy()
        Dim clsProxy As New IEProxy
        Dim proxyConnect As String = ""
        Dim proxyServer As String = ReadIni(INIfile, "SETTING", "ProxyServer", "")
        Dim proxyPort As String = ReadIni(INIfile, "SETTING", "ProxyPort", "")

        If proxyServer = "" Or proxyPort = "" Then
            NotifyIcon1.ShowBalloonTip(2000, "Failed", "It is not setting. Use after setting.", ToolTipIcon.Error)
            showMe()
        End If

        proxyConnect = String.Format("{0}:{1}", proxyServer, proxyPort)

        If clsProxy.SetProxy(proxyConnect) Then
            'MessageBox.Show("Proxy successfully enabled.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            If Not (ProxyTag) Or showBalloon Then NotifyIcon1.ShowBalloonTip(1000, "Success", "Proxy successfully enabled.", ToolTipIcon.Info)
            ProxyTag = True
            showBalloon = False
            NotifyIcon1.Text = "Proxy enabled."
            NotifyIcon1.Icon = My.Resources.Sunny
            CtxMenu.Items(2).Enabled = False
            CtxMenu.Items(3).Enabled = True

        Else
            'MessageBox.Show("Error enabling proxy.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            NotifyIcon1.ShowBalloonTip(2000, "Failed", "Error enabling proxy.", ToolTipIcon.Error)
        End If
    End Sub

    Private Sub DisabledProxy()
        Dim clsProxy As New IEProxy

        If clsProxy.DisableProxy Then
            'MessageBox.Show("Proxy successfully disabled.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            If ProxyTag Or showBalloon Then NotifyIcon1.ShowBalloonTip(1000, "Success", "Proxy successfully disabled.", ToolTipIcon.Info)
            ProxyTag = False
            showBalloon = False
            NotifyIcon1.Text = "Proxy disabled."
            NotifyIcon1.Icon = My.Resources.Moon
            CtxMenu.Items(2).Enabled = True
            CtxMenu.Items(3).Enabled = False
        Else
            'MessageBox.Show("Error disabling proxy.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            NotifyIcon1.ShowBalloonTip(2000, "Failed", "Error disabling proxy.", ToolTipIcon.Error)
        End If
    End Sub

    Private Sub ChangeNetwork()
        Dim currentIP As String = ReadIni(INIfile, "SETTING", "IPDetection", "")
        Dim isCurrentIP As Boolean = False
        Dim strHostName = System.Net.Dns.GetHostName()

        For Each ilist As Net.IPAddress In Net.Dns.GetHostEntry(strHostName).AddressList.Where(Function(a As Net.IPAddress) Not a.IsIPv6LinkLocal AndAlso Not a.IsIPv6Multicast AndAlso Not a.IsIPv6SiteLocal)
            If ilist.ToString.StartsWith(currentIP) Then
                isCurrentIP = True
                Exit For
            End If
        Next

        If isCurrentIP Then
            EnabledProxy()
        Else
            DisabledProxy()
        End If
    End Sub

    Private Sub showMe()
        Me.txtIPdetect.Text = ReadIni(INIfile, "SETTING", "IPDetection", "")
        Me.txtProxyserver.Text = ReadIni(INIfile, "SETTING", "ProxyServer", "")
        Me.txtProxyport.Text = ReadIni(INIfile, "SETTING", "ProxyPort", "")

        Me.Show()   '.Visible = True
        Me.ShowInTaskbar = True  '현재 프로그램을 테스크 바에 표시하게 한다.    
        Me.WindowState = FormWindowState.Normal  ' 폼을 윈도 상태를 normal
        NotifyIcon1.Visible = False '트레이의 아이콘을 보이지 않게 한다. 

    End Sub

    Private Sub hideMe()

        Me.Hide()                   '폼을 보이지 않게 한다. alt+tab 시 보이지 않는다.
        NotifyIcon1.Visible = True   '트레이의 아이콘을 보이게 한다.
        Me.NotifyIcon1.Text = "ProxyAutoChagne is Running!"

    End Sub

    Private Sub CtxMenu_Click(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles CtxMenu.ItemClicked
        Select Case e.ClickedItem.ToString()
            Case "Show"
                showMe()
            Case "Proxy Enabled"
                EnabledProxy()
            Case "Proxy Disabled"
                DisabledProxy()
            Case "App Exit"
                Application.Exit()
        End Select

    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        ' 네트워크 변경(ip 변경) 감지 event 등록
        AddHandler NetworkChange.NetworkAddressChanged, AddressOf ChangeNetwork

        ' form 숨기기
        hideMe()

        ' 프로그램 실행 시 처음 실행하기
        ChangeNetwork()

    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        ' 트레이 아이콘 더블클릭으로 form 표시
        showMe()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        hideMe()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        WriteIni(INIfile, "SETTING", "IPDetection", txtIPdetect.Text)
        WriteIni(INIfile, "SETTING", "ProxyServer", txtProxyserver.Text)
        WriteIni(INIfile, "SETTING", "ProxyPort", txtProxyport.Text)

        hideMe()
        EnabledProxy()
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown, SplitContainer1.Panel1.MouseDown, Label4.MouseDown
        If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
            mousePoint = New Point(e.X, e.Y)
        End If
    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove, SplitContainer1.Panel1.MouseMove, Label4.MouseMove
        If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
            Me.Location = New Point(Me.Location.X + (e.X - mousePoint.X), Me.Location.Y + (e.Y - mousePoint.Y))
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        hideMe()
    End Sub

    Private Function ConfirmExit() As Integer
        Return MsgBox("프로그램을 종료하시겠습니까?", MsgBoxStyle.OkCancel, "종료 확인")
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If ConfirmExit() = 1 Then
            Application.Exit()
        End If
    End Sub
End Class
