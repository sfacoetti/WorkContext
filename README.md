<img src="WorkContext.png" width="96" align="right" alt="WorkContext">

# WorkContext

[Italiano](#italiano) · [English](#english)

---

## Italiano

WorkContext è una piccola applicazione per Windows che riconosce **da quale cliente ti trovi** in base alla rete a cui è connesso il PC e, dopo la tua conferma, **apre automaticamente programmi, file e pagine web** che ti servono per quel cliente, disponendo le finestre sul monitor e nella posizione che hai scelto.

### Cosa fa

- **Resta nell'area di notifica** (accanto all'orologio) e lavora in background.
- **Riconosce il cliente** all'avvio e a ogni cambio di rete, usando solo informazioni locali:
  - nome della rete di Windows (per il Wi-Fi di solito coincide con l'SSID);
  - suffisso DNS della connessione (es. `cliente.local`);
  - indirizzo IP del gateway.

  Non usa geolocalizzazione né IP pubblico e non contatta servizi esterni.
- **Aspetta un tempo configurabile** (predefinito 5 secondi) per dare alla rete il tempo di stabilizzarsi.
- **Chiede conferma** con un popup in stile "notch" in alto al centro dello schermo: *Apri* (Invio) o *Ignora* (Esc).
- **Apre l'ambiente di lavoro**: programmi, file, collegamenti, URL e cartelle, ciascuno con argomenti, monitor e posizione (non spostare, massimizzata, metà sinistra, metà destra).
- **Apre cartelle in Esplora file** in una nuova finestra oppure, con l'opzione *In scheda*, in una nuova scheda della finestra di Esplora file già aperta (Windows 11). Se Esplora file non è aperto, usa una nuova finestra.
- **Non ti disturba** a ogni micro-disconnessione del Wi-Fi: chiede di nuovo solo quando cambi cliente o riavvii l'app.

### Menu dell'icona

| Voce | Funzione |
|------|----------|
| Cliente: … | Cliente rilevato in questo momento |
| Apri ambiente ora | Apre subito l'ambiente del cliente rilevato, senza conferma |
| Configura clienti... | Apre la finestra di configurazione (anche con doppio clic sull'icona) |
| Avvia con Windows | Avvia WorkContext al login (chiave `HKCU\...\Run`) |
| Esci | Chiude l'applicazione |

### Configurazione

Dalla finestra di configurazione si definiscono, per ogni cliente:

- **Nome**;
- **Reti** che lo identificano, una per riga: nome rete, suffisso DNS o IP gateway. Il pulsante *Aggiungi nome della rete attuale* inserisce la rete a cui sei connesso;
- **Cosa aprire**: programma/file/URL/cartella (pulsante `…` per scegliere un file o una cartella), argomenti, monitor, posizione, *In scheda* (solo cartelle);
- **Prova**: apre subito l'ambiente del cliente selezionato.

In fondo alla finestra si imposta l'**attesa prima del rilevamento** in secondi.

I dati sono salvati in `%APPDATA%\WorkContext\clienti.json`. A ogni salvataggio la versione precedente viene copiata in `clienti.json.bak`. Un file JSON danneggiato non viene mai sovrascritto: l'app segnala l'errore.

### Limiti noti

- Il posizionamento funziona quando il programma apre una **nuova finestra**. Se il programma è già aperto (es. Teams) o apre una scheda in una finestra esistente, la finestra non viene spostata e compare un avviso.
- Gli URL con una posizione impostata si aprono in una **nuova finestra di Microsoft Edge**, perché una scheda del browser predefinito non si può posizionare. Gli URL con "Non spostare" usano il browser predefinito.
- Non si possono spostare le finestre di programmi avviati **come amministratore**.
- Il popup di conferma compare sul monitor principale.
- L'apertura *In scheda* usa un comando interno di Esplora file (Windows 11 22H2 o successivo), non documentato da Microsoft. Se non è disponibile, la cartella si apre in una nuova finestra. In scheda monitor e posizione vengono ignorati, perché la finestra esiste già.

### Tecnologie

- **VB.NET** su **.NET 10** (Windows).
- **Windows Forms**: icona nell'area di notifica, menu, finestra di configurazione.
- **WPF**: popup di conferma "notch" (trasparenza, angoli antialias, animazioni).
- **API Windows**: `NetworkListManager` (COM) per i nomi delle reti, `System.Net.NetworkInformation` per DNS e gateway, `user32` (`EnumWindows`, `SetWindowPos`) per posizionare le finestre.
- **System.Text.Json** per la configurazione.
- Nessun pacchetto NuGet esterno.

### Compilazione e avvio

Requisiti: Windows 10/11 e .NET SDK 10.

```powershell
dotnet build -c Release
.\bin\Release\net10.0-windows\WorkContext.exe
```

Argomenti da riga di comando:

| Argomento | Effetto |
|-----------|---------|
| `--config` | Apre subito la finestra di configurazione |
| `--selftest` | Esegue i controlli interni e stampa l'esito (`WorkContext.exe --selftest \| Out-String`); il codice di uscita è il numero di controlli falliti |

### Struttura del progetto

| File | Contenuto |
|------|-----------|
| `Program.vb` | Avvio, icona nell'area di notifica, rilevamento, consenso |
| `Rete.vb` | Lettura di nomi rete, suffissi DNS e gateway |
| `Config.vb` | Modello dati e lettura/scrittura di `clienti.json` |
| `Avvio.vb` | Apertura degli elementi e posizionamento delle finestre |
| `Esplora.vb` | Apertura di cartelle in una nuova scheda di Esplora file |
| `FormConfigurazione.vb` | Finestra di configurazione clienti |
| `Notch.xaml`, `Notch.xaml.vb` | Popup di conferma |
| `SelfTest.vb` | Controlli eseguiti con `--selftest` |
| `PLAN.md` | Piano di lavoro e stato delle verifiche |

---

## English

WorkContext is a small Windows application that detects **which customer site you are at** from the network your PC is connected to and, after you confirm, **automatically opens the programs, files and web pages** you need for that customer, placing each window on the monitor and in the position you chose.

### What it does

- **Lives in the notification area** (next to the clock) and runs in the background.
- **Detects the customer** at startup and on every network change, using local information only:
  - Windows network name (for Wi-Fi this usually matches the SSID);
  - connection DNS suffix (e.g. `customer.local`);
  - gateway IP address.

  It does not use geolocation or your public IP and does not contact any external service.
- **Waits a configurable delay** (5 seconds by default) so the network can settle.
- **Asks for confirmation** with a "notch"-style popup at the top center of the screen: *Apri* / Open (Enter) or *Ignora* / Ignore (Esc).
- **Opens the work environment**: programs, files, shortcuts, URLs and folders, each with arguments, monitor and position (don't move, maximized, left half, right half).
- **Opens folders in File Explorer** in a new window or, with the *In scheda* (in tab) option, in a new tab of the File Explorer window already open (Windows 11). If File Explorer is not open, a new window is used.
- **Doesn't nag you** on every Wi-Fi hiccup: it asks again only when the customer changes or the app restarts.

### Tray menu

| Item | Function |
|------|----------|
| Cliente: … | Customer currently detected |
| Apri ambiente ora | Opens the detected customer's environment now, without confirmation |
| Configura clienti... | Opens the configuration window (also by double-clicking the icon) |
| Avvia con Windows | Starts WorkContext at login (`HKCU\...\Run` key) |
| Esci | Exits the application |

### Configuration

In the configuration window you define, for each customer:

- **Name**;
- **Networks** that identify it, one per line: network name, DNS suffix or gateway IP. The *Aggiungi nome della rete attuale* (add current network name) button inserts the network you are connected to;
- **What to open**: program/file/URL/folder (`…` button to pick a file or a folder), arguments, monitor, position, *In scheda* (folders only);
- **Prova** (Test): opens the selected customer's environment right away.

At the bottom of the window you set the **delay before detection** in seconds.

Data is stored in `%APPDATA%\WorkContext\clienti.json`. On every save the previous version is copied to `clienti.json.bak`. A corrupted JSON file is never overwritten: the app reports the error instead.

### Known limitations

- Window placement works when the program opens a **new window**. If the program is already running (e.g. Teams) or opens a tab in an existing window, the window is not moved and a notification is shown.
- URLs with a position set open in a **new Microsoft Edge window**, because a tab in the default browser cannot be positioned. URLs set to "don't move" use the default browser.
- Windows of programs running **as administrator** cannot be moved.
- The confirmation popup appears on the primary monitor.
- Opening *In scheda* (in tab) uses an internal File Explorer command (Windows 11 22H2 or later) that Microsoft does not document. If it is not available, the folder opens in a new window. In a tab, monitor and position are ignored because the window already exists.

### Technologies

- **VB.NET** on **.NET 10** (Windows).
- **Windows Forms**: notification-area icon, menu, configuration window.
- **WPF**: "notch" confirmation popup (transparency, anti-aliased corners, animations).
- **Windows APIs**: `NetworkListManager` (COM) for network names, `System.Net.NetworkInformation` for DNS and gateway, `user32` (`EnumWindows`, `SetWindowPos`) for window placement.
- **System.Text.Json** for configuration.
- No external NuGet packages.

### Build and run

Requirements: Windows 10/11 and .NET SDK 10.

```powershell
dotnet build -c Release
.\bin\Release\net10.0-windows\WorkContext.exe
```

Command-line arguments:

| Argument | Effect |
|----------|--------|
| `--config` | Opens the configuration window immediately |
| `--selftest` | Runs the built-in checks and prints the result (`WorkContext.exe --selftest \| Out-String`); the exit code is the number of failed checks |

### Project structure

| File | Contents |
|------|----------|
| `Program.vb` | Startup, notification-area icon, detection, consent |
| `Rete.vb` | Reads network names, DNS suffixes and gateways |
| `Config.vb` | Data model and `clienti.json` read/write |
| `Avvio.vb` | Opens items and positions windows |
| `Esplora.vb` | Opens folders in a new File Explorer tab |
| `FormConfigurazione.vb` | Customer configuration window |
| `Notch.xaml`, `Notch.xaml.vb` | Confirmation popup |
| `SelfTest.vb` | Checks run by `--selftest` |
| `PLAN.md` | Work plan and verification status |
