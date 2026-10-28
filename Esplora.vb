' Option Strict Off solo qui: late binding su Shell.Application (come in Rete.vb).
Option Strict Off

Imports System.Threading

Public Module Esplora
    Private Declare Auto Function FindWindow Lib "user32" (cls As String, titolo As String) As IntPtr
    Private Declare Auto Function FindWindowEx Lib "user32" (padre As IntPtr, dopo As IntPtr, cls As String, titolo As String) As IntPtr
    Private Declare Auto Function PostMessage Lib "user32" (h As IntPtr, msg As Integer, w As IntPtr, l As IntPtr) As Boolean
    ' comando interno "Nuova scheda" di Esplora file (Windows 11 22H2+): non documentato, se sparisce si ripiega su una nuova finestra
    Private Const WM_COMMAND = &H111, CMD_NUOVA_SCHEDA = &HA21B

    ''' <summary>Apre la cartella in una nuova scheda della finestra di Esplora file più recente.
    ''' False se non c'è nessuna finestra aperta o le schede non sono supportate.</summary>
    Public Function ApriInScheda(cartella As String) As Boolean
        Dim finestra = FindWindow("CabinetWClass", Nothing) ' la prima in ordine Z = l'ultima usata
        If finestra = IntPtr.Zero Then Return False
        Dim scheda = FindWindowEx(finestra, IntPtr.Zero, "ShellTabWindowClass", Nothing)
        If scheda = IntPtr.Zero Then Return False

        Dim shell As Object = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))
        Dim prima As New List(Of Object)
        For Each w As Object In shell.Windows()
            prima.Add(w)
        Next
        PostMessage(scheda, WM_COMMAND, New IntPtr(CMD_NUOVA_SCHEDA), IntPtr.Zero)

        Dim fine = DateTime.Now.AddSeconds(5)
        Do While DateTime.Now < fine
            Thread.Sleep(200)
            For Each w As Object In shell.Windows()
                If CLng(w.HWND) = finestra.ToInt64() AndAlso Not prima.Contains(w) Then
                    w.Navigate2(cartella)
                    Return True
                End If
            Next
        Loop
        Return False
    End Function
End Module
