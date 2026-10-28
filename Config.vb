Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

Public Enum Posizione
    Nessuna
    Massimizzata
    MetaSinistra
    MetaDestra
End Enum

Public Class Elemento
    Public Property Destinazione As String = ""
    Public Property Argomenti As String = ""
    Public Property Monitor As Integer = 1
    Public Property Posizione As Posizione = Posizione.Nessuna
End Class

Public Class Cliente
    Public Property Nome As String = "Nuovo cliente"
    ''' <summary>Nomi rete, suffissi DNS o IP gateway che identificano il cliente.</summary>
    Public Property Reti As New List(Of String)
    Public Property Elementi As New List(Of Elemento)
End Class

Public Class Configurazione
    Public Property Clienti As New List(Of Cliente)
    ''' <summary>Attesa dopo l'avvio o un cambio di rete prima di rilevare il cliente (la rete si stabilizza).</summary>
    Public Property RitardoSecondi As Integer = 5

    Public Shared ReadOnly PercorsoPredefinito As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkContext", "clienti.json")

    Private Shared ReadOnly Opzioni As JsonSerializerOptions = CreaOpzioni()

    Private Shared Function CreaOpzioni() As JsonSerializerOptions
        Dim o As New JsonSerializerOptions With {.WriteIndented = True}
        o.Converters.Add(New JsonStringEnumConverter())
        Return o
    End Function

    Public Function Trova(identificativi As IEnumerable(Of String)) As Cliente
        Return Clienti.FirstOrDefault(Function(c) c.Reti.Any(
            Function(r) identificativi.Any(Function(i) String.Equals(i.Trim(), r.Trim(), StringComparison.OrdinalIgnoreCase))))
    End Function

    ''' <summary>Se il file non esiste lo crea con un esempio; se è corrotto lancia l'eccezione (mai sovrascritto).</summary>
    Public Shared Function Carica(Optional percorso As String = Nothing) As Configurazione
        percorso = If(percorso, PercorsoPredefinito)
        If Not File.Exists(percorso) Then
            Dim es = Esempio()
            es.Salva(percorso)
            Return es
        End If
        Return JsonSerializer.Deserialize(Of Configurazione)(File.ReadAllText(percorso), Opzioni)
    End Function

    Public Sub Salva(Optional percorso As String = Nothing)
        percorso = If(percorso, PercorsoPredefinito)
        Directory.CreateDirectory(Path.GetDirectoryName(percorso))
        If File.Exists(percorso) Then File.Copy(percorso, percorso & ".bak", True)
        File.WriteAllText(percorso & ".tmp", JsonSerializer.Serialize(Me, Opzioni))
        File.Move(percorso & ".tmp", percorso, True)
    End Sub

    Public Function Clona() As Configurazione
        Return JsonSerializer.Deserialize(Of Configurazione)(JsonSerializer.Serialize(Me, Opzioni), Opzioni)
    End Function

    Private Shared Function Esempio() As Configurazione
        Dim brimind As New Cliente With {.Nome = "BRIMIND", .Reti = New List(Of String) From {"BRIMIND WIFI 3"}}
        brimind.Elementi.Add(New Elemento With {.Destinazione = "notepad.exe", .Posizione = Posizione.MetaSinistra})
        brimind.Elementi.Add(New Elemento With {.Destinazione = "https://outlook.office.com", .Posizione = Posizione.MetaDestra})
        Return New Configurazione With {.Clienti = New List(Of Cliente) From {brimind}}
    End Function
End Class
