<img src="_media/FlowSketch.svg" width="72" alt="FlowSketch">

# FlowSketch

**Prozesspläne erstellen und Abläufe dokumentieren.**

FlowSketch ist ein deutschsprachiger Windows-Editor für funktionelle R&I-Overlays und Prozesszeichnungen. Referenz-PDFs bleiben unverändert; Änderungen werden in einer separaten Projektdatei gespeichert.

**Version 0.10.0** · [Bedienhilfe](docs/Hilfe.md) · [Demo](examples/README.md) · [Projektstand](ProjektZustand.md)

![FlowSketch-Editor](_media/help/blank-editor.png)

## Funktionen

- Prozesssymbole, Rohrpfade, Anschlüsse und Medienwege auf eigenen Blättern oder über PDF-Plänen bearbeiten.
- Szenarien und Schritte mit Geräteinformationen und Zuständen dokumentieren.
- PDF- und JPG-Pläne sowie Offline-Review-Pakete und Aktorenlisten für Excel exportieren.
- Mehrere Dokumente in Tabs bearbeiten; Änderungen rückgängig machen und wiederholen.
- PDF-Referenzen prüfen und Engineering-Freigaben verwalten.

FlowSketch stellt funktionelle Verbindungen dar. Es simuliert keine hydraulischen oder thermischen Vorgänge und verbindet sich nicht mit SPS oder HMI.

## Voraussetzungen

| Zweck | Voraussetzung |
| --- | --- |
| Programm starten | Windows 10 (Build 19041 oder neuer) bzw. Windows 11, .NET 10 Desktop Runtime, x64 |
| Quellen bauen | Windows und .NET 10 SDK |
| Viewer- und Hilfetests | Zusätzlich Node.js |

Für den Excel-Export ist Microsoft Excel nicht erforderlich.

## Aus Quellen starten

```powershell
git clone https://github.com/SchnDom/FlowSketch.git
cd FlowSketch
dotnet restore PID2Process.sln
dotnet run --project src/PID2Process/PID2Process.csproj
```

Über **Hilfe → Demo öffnen** lässt sich eine Beispielanlage als unabhängige Arbeitskopie laden. Weitere Informationen und Dateien gibt es unter [examples](examples/README.md).

## Versionierte Ausgabe

```powershell
./scripts/Publish-Release.ps1
```

Die Anwendungsversion steht in [PID2Process.csproj](src/PID2Process/PID2Process.csproj). Der Build wird unter `bin/<Version>/` abgelegt.

## Prüfen

```powershell
dotnet run --project src/PID2Process/PID2Process.csproj -c Release -- --model-test
node tests/review-scene.test.cjs
node tests/review-zoom.test.cjs
node tests/references.test.cjs
./scripts/Publish-Release.ps1
node tests/help.test.cjs
```

`--model-test` läuft ohne sichtbares Fenster. Modell-, Viewer- und Hilfetests prüfen unterschiedliche Bereiche; eine erfolgreiche Kompilierung ersetzt keine Bedienprüfung.

## Bedienung

| Aktion | Bedienung |
| --- | --- |
| Rohrpfad zeichnen | `R`, Eckpunkte klicken, mit `Enter` oder Doppelklick abschließen |
| Ansicht verschieben / zoomen | Mittlere Maustaste ziehen / Mausrad |
| Eigenschaften öffnen | Doppelklick oder Kontextmenü |
| Speichern | `Strg+S` |
| Rückgängig / Wiederholen | `Strg+Z` / `Strg+Y` |

## Lizenz

[MIT-Lizenz](LICENSE) · [Drittanbieterhinweise](THIRD-PARTY-NOTICES.md) · [Changelog](CHANGELOG.md)

Copyright (c) 2026 Dominik Schneeberger
