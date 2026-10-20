# FlowSketch

Windows-Engineering-Editor für funktionelle Overlays auf unveränderten R&I-PDFs. Oberfläche und Kommunikation deutsch.

- Vor Arbeiten `ProjektZustand.md` und betroffene Quellen lesen. Projektfortschritt dort aktuell halten.
- PDF-Seiten beim Zoomen aus der Original-PDF in zur Anzeige/DPI passender Auflösung neu rendern. Keine feste Screenshot-Auflösung hochskalieren. Logische Seitengröße von Rasterauflösung trennen. Eigene Zeichenfläche verwenden; mehrere PDFs unabhängig in Tabs bearbeiten.
- Normierte, seitenbezogene Positionen und Größen speichern. Mittlere Maustaste: Pan; rechte: Kontextmenü.
- Gerätedaten bei Platzierung und Doppelklick öffnen. Symbolvorschau am Mauszeiger zeigen.
- Kompakte Menüzeile und schmale kategorisierte Bibliothek wie Referenzgrafik verwenden.
- Keine SPS-/HMI-Verbindung oder Steuerlogik. Referenz-PDF nie verändern.
- Engineering-Änderungen widerrufen Freigabe. PDF-Hash prüfen.
- Nutzeränderungen und Test-PDFs erhalten. Dateien mit apply_patch bearbeiten.
- Build, Tests und visuelle Laufzeitprüfung getrennt berichten. Kompilierung beweist keine Bedienbarkeit.
- Auslieferbare Builds versionieren: Version in Projektdatei, Fenstertitel, Über-uns-Dialog und EXE-Dateiinformationen; Ausgabe nach `bin/<Version>`. Bei neuen Auslieferungen Versionsnummer erhöhen und frühere Versionsordner erhalten.
- Szenarien enthalten mehrere Schritte. Schritte referenzieren ihr Szenario über `ScenarioId`; eigene Zustands-/Textzuweisungen überschreiben Szenariowerte. Bestehende Schritte ohne Szenario nicht stillschweigend zuordnen.
- Funktion und Einbauort gehören an jedes Symbol. Medienquellen: freier Name und Gas/Flüssigkeit/Feststoff/Gemisch. Rohrpfade und Anschlüsse normiert speichern; Abzweige bleiben an Pfadsegment und Anteil gebunden.
- Medienweitergabe ist eine fachliche Verbindungsdarstellung, keine hydraulische Simulation. Editor, statische Exporte und Review-Viewer müssen gleiche Szenario-/Schrittwerte verwenden.
- Bei untersagter Bildschirmübernahme keine Windows-UI-Automation und keinen sichtbaren `--self-test` starten. `--model-test` prüft ohne Fensteranzeige, Maus-/Tastatureingaben oder Vordergrundwechsel. Layout nur aus gespeicherten Prüfbildern prüfen.
