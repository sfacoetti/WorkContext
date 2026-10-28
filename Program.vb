Imports System.Drawing
Imports System.Net.NetworkInformation
Imports System.Threading
Imports System.Windows.Forms
Imports Microsoft.Win32

Public Module Program
    Private Const ChiaveAvvio = "Software\Microsoft\Windows\CurrentVersion\Run"
    Private config As Configurazione
    Private icona As NotifyIcon
    Private voceCliente As ToolStripMenuItem
    Private clienteAttuale As Cliente
    Private ultimoAperto As String ' evita di richiedere/riaprire tutto a ogni micro-disconnessione del Wi-Fi
    Private configAperta As Boolean
    Private ui As SynchronizationContext
    Private notchAperta As Notch
    Private ReadOnly attesa As New System.Windows.Forms.Timer() ' la rete cambia a raffiche: aspetto che si stabilizzi

    <STAThread>
    Public Sub Main(args As String())
        If args.Contains("--selftest") Then Environment.Exit(SelfTest.Esegui())

        Dim primo As Boolean
        Using m As New Mutex(True, "WorkContext.Tray", primo)
            If Not primo Then Return
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            Try
                config = Configurazione.Carica()
            Catch ex As Exception
                MessageBox.Show($"Impossibile leggere {Configurazione.PercorsoPredefinito}:{vbCrLf}{ex.Message}", "WorkContext", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try

            Dim menu As New ContextMenuStrip()
            ui = SynchronizationContext.Current
            voceCliente = New ToolStripMenuItem("Cliente: —") With {.Enabled = False}
            Dim voceAvvio As New ToolStripMenuItem("Avvia con Windows") With {.CheckOnClick = True, .Checked = AvvioAutomatico}
            AddHandler voceAvvio.CheckedChanged, Sub() AvvioAutomatico = voceAvvio.Checked
            menu.Items.Add(voceCliente)
            menu.Items.Add(New ToolStripSeparator())
            menu.Items.Add("Apri ambiente ora", Nothing, Sub() ApriAmbiente(clienteAttuale))
            menu.Items.Add("Configura clienti...", Nothing, Sub() Configura())
            menu.Items.Add(voceAvvio)
            menu.Items.Add(New ToolStripSeparator())
            menu.Items.Add("Esci", Nothing, Sub() Application.Exit())

            icona = New NotifyIcon With {.Icon = IconaApp(SystemInformation.SmallIconSize.Width), .Text = "WorkContext", .ContextMenuStrip = menu, .Visible = True}
            AddHandler icona.DoubleClick, Sub() Configura()
            AddHandler attesa.Tick, Sub()
                                        attesa.Stop()
                                        Rileva()
                                    End Sub
            AddHandler NetworkChange.NetworkAddressChanged, Sub() ui.Post(Sub() AvviaAttesa(), Nothing)
            If args.Contains("--config") Then Configura()
            AvviaAttesa()
            Application.Run()
            icona.Visible = False
        End Using
    End Sub

    Private Sub AvviaAttesa()
        attesa.Stop()
        attesa.Interval = Math.Max(1, config.RitardoSecondi * 1000)
        attesa.Start()
    End Sub

    Private Sub Rileva()
        Dim ids As List(Of String)
        Try
            ids = Rete.Identificativi()
        Catch ex As Exception
            Avvisa("Errore rilevamento rete", ex.Message, ToolTipIcon.Error)
            Return
        End Try
        clienteAttuale = config.Trova(ids)
        voceCliente.Text = "Cliente: " & If(clienteAttuale?.Nome, "nessuno")
        icona.Text = "WorkContext – " & If(clienteAttuale?.Nome, "nessun cliente")
        If clienteAttuale IsNot Nothing AndAlso clienteAttuale.Nome <> ultimoAperto Then ChiediConsenso(clienteAttuale, ids)
    End Sub

    Private Sub ChiediConsenso(c As Cliente, ids As List(Of String))
        ultimoAperto = c.Nome ' anche se ignori, non te lo richiede a ogni micro-disconnessione
        notchAperta?.Rispondi(False) ' un nuovo rilevamento sostituisce quello in attesa
        Dim rete = c.Reti.FirstOrDefault(Function(r) ids.Contains(r.Trim(), StringComparer.OrdinalIgnoreCase))
        Dim n As Notch = Nothing
        n = New Notch(c, rete, Sub(apri)
                                   If notchAperta Is n Then notchAperta = Nothing
                                   If apri Then ApriAmbiente(c)
                               End Sub)
        notchAperta = n
        System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(n) ' Invio/Esc nel loop WinForms
        n.Show()
    End Sub

    Private Sub ApriAmbiente(c As Cliente)
        If c Is Nothing Then
            Avvisa("WorkContext", "Nessun cliente riconosciuto su questa rete.", ToolTipIcon.Info)
            Return
        End If
        ultimoAperto = c.Nome
        Avvisa("WorkContext", $"Cliente {c.Nome}: apro {c.Elementi.Count} elementi.", ToolTipIcon.Info)
        Task.Run(Sub() Avvio.ApriTutto(c, Sub(msg) ui.Post(Sub() Avvisa("Errore apertura", msg, ToolTipIcon.Warning), Nothing)))
    End Sub

    Private Sub Configura()
        If configAperta Then Return
        configAperta = True
        Try
            Using f As New FormConfigurazione(config.Clona())
                If f.ShowDialog() = DialogResult.OK Then
                    f.Risultato.Salva()
                    config = f.Risultato
                    Rileva()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "WorkContext", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            configAperta = False
        End Try
    End Sub

    ''' <summary>Icona dell'exe (WorkContext.ico) alla misura richiesta, così non viene sgranata.</summary>
    Public Function IconaApp(misura As Integer) As Icon
        Return If(Icon.ExtractIcon(Environment.ProcessPath, 0, misura), SystemIcons.Application)
    End Function

    Private Sub Avvisa(titolo As String, testo As String, tipo As ToolTipIcon)
        icona.ShowBalloonTip(5000, titolo, testo, tipo)
    End Sub

    Private Property AvvioAutomatico As Boolean
        Get
            Using k = Registry.CurrentUser.OpenSubKey(ChiaveAvvio)
                Return k?.GetValue("WorkContext") IsNot Nothing
            End Using
        End Get
        Set(value As Boolean)
            Using k = Registry.CurrentUser.CreateSubKey(ChiaveAvvio)
                If value Then k.SetValue("WorkContext", $"""{Environment.ProcessPath}""") Else k.DeleteValue("WorkContext", False)
            End Using
        End Set
    End Property
End Module
