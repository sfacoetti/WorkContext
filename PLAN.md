# PLAN — WorkContext

Obiettivo: app VB.NET (.NET 10, WinForms) nell'area di notifica che riconosce il cliente dalla rete
(nome profilo di rete, suffisso DNS, IP gateway — niente geolocalizzazione né IP pubblico) e apre
programmi/URL configurati, posizionandoli su monitor/zona scelti. Finestra di configurazione clienti.

| # | Passo | Come lo verifico | Stato |
|---|-------|------------------|-------|
| 1 | Progetto `WorkContext.vbproj` + modello dati/JSON (`Config.vb`) | `dotnet build`; `WorkContext.exe --selftest \| Out-String` round-trip, `.bak`, JSON corrotto non sovrascritto | verificato |
| 2 | Rilevamento rete (`Rete.vb`) | `--selftest` stampa `BRIMIND WIFI 3 \| 192.168.51.1` | verificato |
| 3 | Apertura e posizionamento (`Avvio.vb`) | avvio app: notepad metà sinistra, Edge metà destra (rect letti) | verificato (1° tentativo Edge senza finestra, non riprodotto) |
| 4 | Tray + cambio rete + avvio con Windows (`Program.vb`) | app parte e apre l'ambiente BRIMIND | parziale: cambio rete reale e voce "Avvia con Windows" non provati |
| 5 | Finestra configurazione (`FormConfigurazione.vb`) | UI Automation: aggiungi, rinomina, nome vuoto rifiutato, Annulla, Salva + `.bak` | verificato; "…" Sfoglia e "Prova" non cliccati |
| 6 | Attesa impostabile (`RitardoSecondi`, default 5) + popup consenso "notch" WPF (`Notch.xaml`) | UI Automation: Ignora=0 aperture, Apri=1, doppio Apri=1, Esc=0; notch centrata in alto; selftest default 5 s | verificato; Alt+F4 e cambio rete reale non provati |
| 7 | Icona `WorkContext.ico` (nuova icona dell'utente, solo 256 → rigenerata multi-misura 16–256; file utente in `.ico.256.bak`, prima icona in `.ico.bak`) su exe, tray, finestra | estrazione dall'exe a 16/24/32/48/256; screenshot barra titolo | verificato; icona nel tray non fotografata |
| 8 | Cartelle in Esplora file: nuova finestra o nuova scheda (`Esplora.vb`, `NuovaScheda`), `…` con scelta file/cartella | prova reale: scheda aggiunta alla finestra esistente + `C:\Windows` in nuova finestra a metà destra; menu `…` con 2 voci; selftest round-trip | verificato; fallback senza Esplora aperto e dialog cartella non provati; 1° giro: finestra aperta su `C:\` e sparita, non riprodotto |

Config: `%APPDATA%\WorkContext\clienti.json` (backup `clienti.json.bak` a ogni salvataggio).
Build: `dotnet build -c Release` → `bin\Release\net10.0-windows\WorkContext.exe` (`--config` apre subito la configurazione).
