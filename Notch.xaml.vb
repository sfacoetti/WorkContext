Imports System.ComponentModel
Imports System.Windows.Media.Animation

''' <summary>Popup di consenso in stile "notch": chiama scelta(True) per Apri, scelta(False) per Ignora/Esc/chiusura.</summary>
Partial Public Class Notch
    Private ReadOnly scelta As Action(Of Boolean)
    Private risposto As Boolean

    Public Sub New(cliente As Cliente, rete As String, scelta As Action(Of Boolean))
        InitializeComponent()
        Me.scelta = scelta
        NomeCliente.Text = cliente.Nome
        Iniziale.Text = If(cliente.Nome.Trim() = "", "?", cliente.Nome.Trim().Substring(0, 1).ToUpper())
        Dim n = cliente.Elementi.Count
        Dettaglio.Text = $"{rete}  ·  {n} {If(n = 1, "elemento", "elementi")} da aprire"
        Dim area = System.Windows.SystemParameters.WorkArea
        Left = area.Left + (area.Width - Width) / 2
        Top = 0 ' attaccato al bordo superiore, come una notch
        AddHandler BtnApri.Click, Sub() Rispondi(True)
        AddHandler BtnIgnora.Click, Sub() Rispondi(False)
        AddHandler Loaded, Sub() Anima(1, 1, 1, 420, New BackEase With {.Amplitude = 0.35, .EasingMode = EasingMode.EaseOut}, Nothing)
    End Sub

    Public Sub Rispondi(apri As Boolean)
        If risposto Then Return ' doppio clic o Invio ripetuto: una sola risposta
        risposto = True
        scelta(apri)
        Anima(0.3, 0.2, 0, 180, New CubicEase With {.EasingMode = EasingMode.EaseIn}, Sub() Close())
    End Sub

    Protected Overrides Sub OnClosing(e As CancelEventArgs)
        If Not risposto Then ' Alt+F4 = Ignora
            risposto = True
            scelta(False)
        End If
        MyBase.OnClosing(e)
    End Sub

    Private Sub Anima(sx As Double, sy As Double, opacita As Double, ms As Integer, easing As IEasingFunction, fine As Action)
        Dim durata = TimeSpan.FromMilliseconds(ms)
        Dim a = New DoubleAnimation(sx, durata) With {.EasingFunction = easing}
        If fine IsNot Nothing Then AddHandler a.Completed, Sub() fine()
        Scala.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, a)
        Scala.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, New DoubleAnimation(sy, durata) With {.EasingFunction = easing})
        Contenuto.BeginAnimation(OpacityProperty, New DoubleAnimation(opacita, durata) With {.BeginTime = If(opacita > 0, TimeSpan.FromMilliseconds(120), TimeSpan.Zero)})
    End Sub
End Class
