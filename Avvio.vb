Imports System.Threading
Imports System.Windows.Forms

Public Module Avvio
    Private Delegate Function EnumProc(h As IntPtr, l As IntPtr) As Boolean
    Private Declare Auto Function EnumWindows Lib "user32" (cb As EnumProc, l As IntPtr) As Boolean
    Private Declare Auto Function IsWindowVisible Lib "user32" (h As IntPtr) As Boolean
    Private Declare Auto Function GetWindow Lib "user32" (h As IntPtr, cmd As Integer) As IntPtr
    Private Declare Auto Function GetWindowTextLength Lib "user32" (h As IntPtr) As Integer
    Private Declare Auto Function ShowWindow Lib "user32" (h As IntPtr, cmd As Integer) As Boolean
    Private Declare Auto Function SetWindowPos Lib "user32" (h As IntPtr, after As IntPtr, x As Integer, y As Integer, cx As Integer, cy As Integer, flags As UInteger) As Boolean

    Private Const GW_OWNER = 4, SW_RESTORE = 9, SW_MAXIMIZE = 3
    Private Const SWP_NOZORDER As UInteger = &H4, SWP_NOACTIVATE As UInteger = &H10

    ''' <summary>Apre gli elementi uno alla volta (serve per capire quale finestra nuova è di chi). Da chiamare fuori dal thread UI.</summary>
    Public Sub ApriTutto(c As Cliente, errore As Action(Of String))
        For Each e In c.Elementi
            If String.IsNullOrWhiteSpace(e.Destinazione) Then Continue For
            Try
                Apri(e)
            Catch ex As Exception
                errore($"{e.Destinazione}: {ex.Message}")
            End Try
        Next
    End Sub

    Private Sub Apri(e As Elemento)
        Dim dest = Environment.ExpandEnvironmentVariables(e.Destinazione.Trim())
        ' in scheda la finestra esiste già: monitor/posizione non si applicano
        If e.NuovaScheda AndAlso IO.Directory.Exists(dest) AndAlso Esplora.ApriInScheda(dest) Then Return
        Dim psi As New ProcessStartInfo(dest, Environment.ExpandEnvironmentVariables(e.Argomenti)) With {.UseShellExecute = True}
        If e.Posizione <> Posizione.Nessuna AndAlso IsUrl(dest) Then
            ' shortcut: nel browser predefinito l'URL finisce in una scheda non posizionabile, quindi uso una nuova finestra Edge; aggiungere scelta browser se serve.
            psi = New ProcessStartInfo("msedge.exe", $"--new-window ""{dest}"" {e.Argomenti}") With {.UseShellExecute = True}
        End If
        If e.Posizione = Posizione.Nessuna Then
            Process.Start(psi)
            Return
        End If

        Dim prima = FinestreVisibili()
        Process.Start(psi)
        Dim h = AttendiNuovaFinestra(prima, TimeSpan.FromSeconds(20))
        If h = IntPtr.Zero Then Throw New TimeoutException("nessuna nuova finestra da posizionare (forse l'app era già aperta)")
        Thread.Sleep(500) ' lascia che l'app finisca di ripristinare la sua posizione salvata
        Posiziona(h, e)
    End Sub

    Private Function IsUrl(s As String) As Boolean
        Dim u As Uri = Nothing
        Return Uri.TryCreate(s, UriKind.Absolute, u) AndAlso (u.Scheme = Uri.UriSchemeHttp OrElse u.Scheme = Uri.UriSchemeHttps)
    End Function

    Private Function FinestreVisibili() As HashSet(Of IntPtr)
        Dim ris As New HashSet(Of IntPtr)
        EnumWindows(Function(h, l)
                        If IsWindowVisible(h) AndAlso GetWindow(h, GW_OWNER) = IntPtr.Zero AndAlso GetWindowTextLength(h) > 0 Then ris.Add(h)
                        Return True
                    End Function, IntPtr.Zero)
        Return ris
    End Function

    ' shortcut: prende la prima finestra nuova di qualunque processo; uno splash screen o un'altra app aperta nello stesso momento può "rubarla".
    Private Function AttendiNuovaFinestra(prima As HashSet(Of IntPtr), timeout As TimeSpan) As IntPtr
        Dim fine = DateTime.Now + timeout
        Do While DateTime.Now < fine
            Thread.Sleep(300)
            Dim nuova = FinestreVisibili().FirstOrDefault(Function(h) Not prima.Contains(h))
            If nuova <> IntPtr.Zero Then Return nuova
        Loop
        Return IntPtr.Zero
    End Function

    Private Sub Posiziona(h As IntPtr, e As Elemento)
        Dim schermi = Screen.AllScreens
        Dim area = schermi(Math.Clamp(e.Monitor, 1, schermi.Length) - 1).WorkingArea
        Select Case e.Posizione
            Case Posizione.MetaSinistra
                area.Width \= 2
            Case Posizione.MetaDestra
                area.X += area.Width \ 2
                area.Width -= area.Width \ 2
        End Select
        ShowWindow(h, SW_RESTORE)
        SetWindowPos(h, IntPtr.Zero, area.X, area.Y, area.Width, area.Height, SWP_NOZORDER Or SWP_NOACTIVATE)
        If e.Posizione = Posizione.Massimizzata Then ShowWindow(h, SW_MAXIMIZE) ' massimizza sul monitor dove è stata spostata
    End Sub
End Module
