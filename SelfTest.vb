Imports System.IO

' Lanciare con: WorkContext.exe --selftest | Out-String   (exit code = numero di verifiche fallite)
Public Module SelfTest
    Private falliti As Integer

    Public Function Esegui() As Integer
        Dim cfg As New Configurazione
        cfg.Clienti.Add(New Cliente With {.Nome = "A", .Reti = New List(Of String) From {"Rete A"}})
        cfg.Clienti.Add(New Cliente With {.Nome = "B", .Reti = New List(Of String) From {"b.local", " 10.0.0.1 "}})
        Verifica(If(cfg.Trova({"rete a"})?.Nome, "") = "A", "nome rete, maiuscole ignorate")
        Verifica(If(cfg.Trova({"x", "10.0.0.1"})?.Nome, "") = "B", "gateway con spazi")
        Verifica(cfg.Trova({"altro"}) Is Nothing, "nessun cliente")
        Verifica(cfg.Trova(Array.Empty(Of String)()) Is Nothing, "nessuna rete")

        Dim tmp = Path.Combine(Path.GetTempPath(), $"wc-selftest-{Guid.NewGuid()}.json")
        cfg.Clienti(0).Elementi.Add(New Elemento With {.Destinazione = "notepad.exe", .Monitor = 2, .Posizione = Posizione.MetaDestra})
        cfg.RitardoSecondi = 42
        cfg.Salva(tmp)
        cfg.Salva(tmp)
        Dim letto = Configurazione.Carica(tmp)
        Dim e = letto.Clienti(0).Elementi(0)
        Verifica(e.Monitor = 2 AndAlso e.Posizione = Posizione.MetaDestra AndAlso letto.RitardoSecondi = 42, "round-trip JSON")
        File.WriteAllText(tmp & ".vecchio", "{ ""Clienti"": [] }")
        Verifica(Configurazione.Carica(tmp & ".vecchio").RitardoSecondi = 5, "JSON vecchio senza RitardoSecondi -> 5 s")
        File.Delete(tmp & ".vecchio")
        Verifica(File.Exists(tmp & ".bak"), "backup .bak prima di sovrascrivere")
        File.WriteAllText(tmp, "{ rotto")
        Try
            Configurazione.Carica(tmp)
            Verifica(False, "JSON corrotto deve dare errore")
        Catch ex As Exception
            Verifica(File.ReadAllText(tmp) = "{ rotto", "JSON corrotto non sovrascritto")
        End Try
        File.Delete(tmp) : File.Delete(tmp & ".bak")

        Console.WriteLine("Identificativi rete attuale: " & String.Join(" | ", Rete.Identificativi()))
        Console.WriteLine(If(falliti = 0, "TUTTO OK", $"{falliti} FALLITI"))
        Return falliti
    End Function

    Private Sub Verifica(ok As Boolean, nome As String)
        Console.WriteLine($"{If(ok, "OK  ", "FAIL")} {nome}")
        If Not ok Then falliti += 1
    End Sub
End Module
