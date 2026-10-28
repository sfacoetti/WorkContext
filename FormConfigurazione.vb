Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

' UI costruita in codice (niente designer) per restare in un file solo.
Public Class FormConfigurazione
    Inherits Form

    Public ReadOnly Property Risultato As Configurazione

    Private ReadOnly clienti As BindingList(Of Cliente)
    Private ReadOnly lista As New ListBox With {.Dock = DockStyle.Fill, .IntegralHeight = False, .DisplayMember = NameOf(Cliente.Nome)}
    Private ReadOnly txtNome As New TextBox With {.Dock = DockStyle.Fill}
    Private ReadOnly txtReti As New TextBox With {.Dock = DockStyle.Fill, .Multiline = True, .AcceptsReturn = True, .ScrollBars = ScrollBars.Vertical}
    Private ReadOnly griglia As New DataGridView With {.Dock = DockStyle.Fill, .AutoGenerateColumns = False, .RowHeadersWidth = 24, .BackgroundColor = SystemColors.Window,
                                                       .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
    Private ReadOnly dettaglio As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(8)}
    Private ReadOnly ritardo As New NumericUpDown With {.Minimum = 0, .Maximum = 600, .Width = 60, .Margin = New Padding(0, 4, 0, 0)}
    Private ReadOnly menuSfoglia As New ContextMenuStrip()
    Private rigaSfoglia As Integer
    Private attuale As Cliente
    Private caricamento As Boolean

    Public Sub New(cfg As Configurazione)
        Text = "WorkContext – clienti"
        Size = New Size(1180, 620)
        MinimumSize = New Size(700, 450)
        StartPosition = FormStartPosition.CenterScreen
        Font = SystemFonts.MessageBoxFont
        Icon = Program.IconaApp(SystemInformation.IconSize.Width)
        clienti = New BindingList(Of Cliente)(cfg.Clienti)

        ' --- elenco clienti a sinistra
        Dim btnAggiungi As New Button With {.Text = "Aggiungi", .AutoSize = True}
        Dim btnRimuovi As New Button With {.Text = "Rimuovi", .AutoSize = True}
        Dim barraLista As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .AutoSize = True}
        barraLista.Controls.AddRange({btnAggiungi, btnRimuovi})
        Dim sinistra As New Panel With {.Dock = DockStyle.Left, .Width = 220, .Padding = New Padding(8)}
        sinistra.Controls.Add(lista)
        sinistra.Controls.Add(barraLista)

        ' --- dettaglio cliente a destra
        dettaglio.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        dettaglio.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        Dim btnReteAttuale As New Button With {.Text = "Aggiungi nome della rete attuale", .AutoSize = True}
        Dim lblRete As New Label With {.AutoSize = True, .ForeColor = SystemColors.GrayText, .Text = "Rete attuale: " & IdentificativiCorrenti()}
        Dim lblMonitor As New Label With {.AutoSize = True, .ForeColor = SystemColors.GrayText, .Text = "Monitor: " & String.Join("   ",
            Screen.AllScreens.Select(Function(s, i) $"{i + 1} = {s.Bounds.Width}×{s.Bounds.Height}{If(s.Primary, " (principale)", "")}"))}
        Dim btnProva As New Button With {.Text = "Prova: apri ora questo cliente", .AutoSize = True}

        AggiungiRiga("Nome", txtNome, SizeType.AutoSize)
        AggiungiRiga("Reti (una per riga):" & vbCrLf & "nome rete, suffisso DNS" & vbCrLf & "o IP gateway", txtReti, SizeType.Absolute, 80)
        AggiungiRiga("", Impila(btnReteAttuale, lblRete), SizeType.AutoSize)
        AggiungiRiga("Cosa aprire", griglia, SizeType.Percent, 100)
        AggiungiRiga("", Impila(btnProva, lblMonitor), SizeType.AutoSize)
        CreaColonne()

        ' --- Salva / Annulla
        Dim btnSalva As New Button With {.Text = "Salva", .AutoSize = True}
        Dim btnAnnulla As New Button With {.Text = "Annulla", .AutoSize = True, .DialogResult = DialogResult.Cancel}
        Dim barraFondo As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)}
        barraFondo.Controls.AddRange({btnAnnulla, btnSalva,
            New Label With {.Text = "secondi", .AutoSize = True, .Margin = New Padding(4, 7, 24, 0)}, ritardo,
            New Label With {.Text = "Attesa prima del rilevamento (all'avvio e al cambio rete):", .AutoSize = True, .Margin = New Padding(0, 7, 4, 0)}})
        ritardo.Value = Math.Clamp(cfg.RitardoSecondi, 0, 600)
        CancelButton = btnAnnulla

        Controls.Add(dettaglio)
        Controls.Add(sinistra)
        Controls.Add(barraFondo)

        AddHandler lista.SelectedIndexChanged, Sub() Mostra(TryCast(lista.SelectedItem, Cliente))
        AddHandler txtNome.TextChanged, Sub()
                                            If caricamento OrElse attuale Is Nothing Then Return
                                            attuale.Nome = txtNome.Text
                                            caricamento = True
                                            clienti.ResetItem(clienti.IndexOf(attuale))
                                            caricamento = False
                                        End Sub
        AddHandler txtReti.TextChanged, Sub()
                                            If caricamento OrElse attuale Is Nothing Then Return
                                            attuale.Reti = txtReti.Lines.Select(Function(r) r.Trim()).Where(Function(r) r <> "").ToList()
                                        End Sub
        AddHandler btnAggiungi.Click, Sub()
                                          clienti.Add(New Cliente())
                                          lista.SelectedIndex = clienti.Count - 1
                                          txtNome.Focus()
                                          txtNome.SelectAll()
                                      End Sub
        AddHandler btnRimuovi.Click, Sub()
                                         If attuale Is Nothing Then Return
                                         If MessageBox.Show($"Rimuovere il cliente ""{attuale.Nome}""?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then clienti.Remove(attuale)
                                     End Sub
        AddHandler btnReteAttuale.Click, Sub() AggiungiReteAttuale()
        AddHandler btnProva.Click, Sub() Prova()
        AddHandler btnSalva.Click, Sub() Salva()
        AddHandler clienti.ListChanged, Sub() If Not caricamento AndAlso clienti.Count = 0 Then Mostra(Nothing)

        lista.DataSource = clienti
        Mostra(TryCast(lista.SelectedItem, Cliente))
    End Sub

    Private Sub AggiungiRiga(etichetta As String, controllo As Control, tipo As SizeType, Optional altezza As Single = 0)
        dettaglio.RowStyles.Add(New RowStyle(tipo, altezza))
        dettaglio.Controls.Add(New Label With {.Text = etichetta, .AutoSize = True, .Margin = New Padding(0, 6, 8, 0)}, 0, dettaglio.RowCount)
        dettaglio.Controls.Add(controllo, 1, dettaglio.RowCount)
        dettaglio.RowCount += 1
    End Sub

    Private Shared Function Impila(ParamArray controlli As Control()) As Control
        Dim p As New FlowLayoutPanel With {.AutoSize = True, .Dock = DockStyle.Fill, .WrapContents = True}
        For Each c In controlli
            c.Margin = New Padding(0, 3, 12, 3)
        Next
        p.Controls.AddRange(controlli)
        Return p
    End Function

    Private Sub CreaColonne()
        Dim posizioni = {
            New With {.Testo = "Non spostare", .Valore = Posizione.Nessuna},
            New With {.Testo = "Massimizzata", .Valore = Posizione.Massimizzata},
            New With {.Testo = "Metà sinistra", .Valore = Posizione.MetaSinistra},
            New With {.Testo = "Metà destra", .Valore = Posizione.MetaDestra}}
        griglia.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Programma, file o URL", .DataPropertyName = NameOf(Elemento.Destinazione), .FillWeight = 45, .MinimumWidth = 200})
        griglia.Columns.Add(New DataGridViewButtonColumn With {.HeaderText = "", .Text = "…", .UseColumnTextForButtonValue = True, .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 32})
        griglia.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Argomenti", .DataPropertyName = NameOf(Elemento.Argomenti), .FillWeight = 25, .MinimumWidth = 90})
        griglia.Columns.Add(New DataGridViewComboBoxColumn With {.HeaderText = "Monitor", .DataPropertyName = NameOf(Elemento.Monitor), .FillWeight = 10, .MinimumWidth = 75,
                            .DataSource = Enumerable.Range(1, 6).ToList(), .ValueType = GetType(Integer), .FlatStyle = FlatStyle.Flat})
        griglia.Columns.Add(New DataGridViewComboBoxColumn With {.HeaderText = "Posizione", .DataPropertyName = NameOf(Elemento.Posizione), .FillWeight = 20, .MinimumWidth = 130,
                            .DataSource = posizioni, .DisplayMember = "Testo", .ValueMember = "Valore", .ValueType = GetType(Posizione), .FlatStyle = FlatStyle.Flat})
        griglia.Columns.Add(New DataGridViewCheckBoxColumn With {.HeaderText = "In scheda", .DataPropertyName = NameOf(Elemento.NuovaScheda),
                            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 80,
                            .ToolTipText = "Solo cartelle: apre in una nuova scheda di Esplora file se è già aperto, altrimenti in una nuova finestra"})
        menuSfoglia.Items.Add("Programma o file...", Nothing, Sub() Imposta(rigaSfoglia, ScegliFile()))
        menuSfoglia.Items.Add("Cartella (Esplora file)...", Nothing, Sub() Imposta(rigaSfoglia, ScegliCartella()))
        AddHandler griglia.CellContentClick, Sub(s, e)
                                                 If e.ColumnIndex = 1 AndAlso e.RowIndex >= 0 Then Sfoglia(e.RowIndex)
                                             End Sub
        ' valore non valido digitato/letto dal JSON (es. monitor 9): segnalo sulla riga invece di far crashare la griglia
        AddHandler griglia.DataError, Sub(s, e)
                                          e.ThrowException = False
                                          griglia.Rows(e.RowIndex).ErrorText = "Valore non valido: " & e.Exception.Message
                                      End Sub
    End Sub

    Private Sub Mostra(c As Cliente)
        griglia.EndEdit()
        caricamento = True
        attuale = c
        dettaglio.Enabled = c IsNot Nothing
        txtNome.Text = If(c?.Nome, "")
        txtReti.Lines = If(c?.Reti.ToArray(), Array.Empty(Of String)())
        griglia.DataSource = If(c Is Nothing, Nothing, New BindingList(Of Elemento)(c.Elementi))
        caricamento = False
    End Sub

    Private Sub Sfoglia(riga As Integer)
        rigaSfoglia = riga
        menuSfoglia.Show(Cursor.Position)
    End Sub

    Private Function ScegliFile() As String
        Using d As New OpenFileDialog With {.Filter = "Programmi e collegamenti|*.exe;*.lnk;*.bat;*.cmd|Tutti i file|*.*", .DereferenceLinks = False}
            Return If(d.ShowDialog(Me) = DialogResult.OK, d.FileName, Nothing)
        End Using
    End Function

    Private Function ScegliCartella() As String
        Using d As New FolderBrowserDialog With {.Description = "Cartella da aprire in Esplora file", .UseDescriptionForTitle = True}
            Return If(d.ShowDialog(Me) = DialogResult.OK, d.SelectedPath, Nothing)
        End Using
    End Function

    Private Sub Imposta(riga As Integer, percorso As String)
        If percorso Is Nothing Then Return
        griglia.EndEdit()
        Dim righe = DirectCast(griglia.DataSource, BindingList(Of Elemento))
        If riga < righe.Count Then
            righe(riga).Destinazione = percorso
            righe.ResetItem(riga)
        Else ' click sulla riga vuota in fondo
            righe.Add(New Elemento With {.Destinazione = percorso})
        End If
    End Sub

    Private Sub AggiungiReteAttuale()
        Try
            Dim nuove = Rete.NomiProfili().Where(Function(n) Not attuale.Reti.Contains(n, StringComparer.OrdinalIgnoreCase)).ToList()
            If nuove.Count = 0 Then
                MessageBox.Show("Nessuna nuova rete da aggiungere.", Text)
                Return
            End If
            txtReti.Lines = txtReti.Lines.Where(Function(r) r.Trim() <> "").Concat(nuove).ToArray()
        Catch ex As Exception
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Prova()
        griglia.EndEdit()
        Dim c = attuale
        Task.Run(Sub() Avvio.ApriTutto(c, Sub(msg) BeginInvoke(Sub() MessageBox.Show(Me, msg, "Errore apertura", MessageBoxButtons.OK, MessageBoxIcon.Warning))))
    End Sub

    Private Sub Salva()
        griglia.EndEdit()
        Dim senzaNome = clienti.FirstOrDefault(Function(c) String.IsNullOrWhiteSpace(c.Nome))
        If senzaNome IsNot Nothing Then
            lista.SelectedItem = senzaNome
            MessageBox.Show("Ogni cliente deve avere un nome.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        For Each c In clienti
            c.Elementi.RemoveAll(Function(e) String.IsNullOrWhiteSpace(e.Destinazione))
        Next
        _Risultato = New Configurazione With {.Clienti = clienti.ToList(), .RitardoSecondi = CInt(ritardo.Value)}
        DialogResult = DialogResult.OK
    End Sub

    Private Shared Function IdentificativiCorrenti() As String
        Try
            Return String.Join("  |  ", Rete.Identificativi())
        Catch ex As Exception
            Return "errore: " & ex.Message
        End Try
    End Function
End Class
