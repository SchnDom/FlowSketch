# Arbeitspaket 1 – Anforderungen und Austauschfälle

**Status:** begonnen; Schwerpunkt und primäre Abnahme durch den Anwender bestätigt, Details und Abnahmekriterien noch offen.  
**Ziel:** typische Aufgaben, Austauschziele, Verantwortlichkeiten und den Umgang mit nicht unterstützten Angaben mit repräsentativen Anwendern abstimmen, bevor Profile oder Importfunktionen festgelegt werden.

## Anwenderinput vom 21.10.2026

- **Priorität:** zuerst Ablaufdokumentation und Aktoren. Der zentrale Informationsbedarf des Excel-Exports lautet: **Welcher Aktor hat in welchem Szenario/Schritt welchen Zustand?**
- **Fachliche Abnahme:** primär durch den Anwender.
- **Möglicher späterer Austausch:** TIA Portal ist denkbar, liegt aber ausdrücklich noch weit außerhalb des aktuellen Umsetzungsumfangs.
- **Weitere Idee, noch kein Auftrag:** DXF-Import mit Erkennung von Rohren und Symbolen sowie automatischer Betriebsmittelerstellung. Voraussetzung ist eine Definition der Symboldaten aus dem CAD-Programm; dieses Austauschprofil und die Erkennung sind noch zu klären.
- **Mögliche fachliche Erweiterungen, noch nicht priorisiert:** komplexe Regelungen als Dokumentation darstellen (2P, 3P, Totband, PID und Kaskade, jeweils einschließlich Parameter) sowie Alarme und ihre Anlagenwirkungen als Fehlerpfad je Szenario/Schritt beschreiben. Gewünschte Integrationen und konkrete Ausprägungen kann der Anwender derzeit noch nicht festlegen.
- **Beispieldaten:** Die vorhandene FlowSketch-Demo ist eine Demo und bildet keine reale Anlage ab. Das bestätigt ihren Demo-Charakter, ersetzt aber keine gesonderte Klärung von Herkunft/Nutzungsrechten.

## Anwenderinput vom 22.10.2026

- **Excel-Aktorenliste:** Neben Szenario, Schritt, Aktor und Zustand sollen Funktion, Einbauort und Beschreibung ausgegeben werden. Funktion, Einbauort und Beschreibung werden in jeder Szenario-/Schrittzeile des Aktors wiederholt.
- **Unterscheidbarkeit:** Tag und Einbauort helfen, Betriebsmittel zu unterscheiden, etwa Motoren mit gleichem Tag an verschiedenen Orten.
- **Tag-Prüfung:** Die bestehende Prüfung auf eindeutige Tags bleibt vorerst unverändert. Eine spätere Änderung ist lediglich eine Möglichkeit und keine aktuelle Anforderung.

## Belegter Ist-Stand

Diese Bestandsaufnahme beschreibt den Repository-Stand, nicht bestätigte Anwenderanforderungen:

- FlowSketch bearbeitet ein Overlay getrennt von der unveränderten Referenz-PDF. Eine Freigabe ist an die PDF-Prüfung gebunden und Engineering-Änderungen widerrufen sie.
- Projekte enthalten unter anderem Geräte mit Tag, Funktion und Einbauort, Verbindungen, Medien, Szenarien und Schritte. Schrittwerte können Szenariowerte überschreiben; alte Schritte ohne Szenario bleiben unzugeordnet.
- Der XLSX-Aktorenexport ist ein Einweg-Export mit den Spalten **Szenario**, **Schritt**, **Aktor** und **Zustand**. Er umfasst Aktoren je Szenario/Schritt, nicht das vollständige Projekt. Die konkrete Aktorauswahl ist dokumentiert; ein Projektimport ist nicht vorhanden.
- Das Repository enthält eine Demo-Projektdatei, ihre PNG-Vorschau und eine erzeugte Aktorenliste. Anwenderbestätigung: Die Demo bildet keine reale Anlage ab. Sie benötigt keine Referenz-PDF. Ein gesonderter Nutzungsrechtsnachweis ist im Repository derzeit nicht dokumentiert.
- FlowSketch ist keine SPS-/HMI-Steuerung, führt keine Szenarien automatisch aus und berechnet keine hydraulischen oder thermischen Größen.

## Priorisierte Anwendungs- und Austauschfälle

Priorität und Abnahme sind nach dem Anwenderinput aktualisiert. Weitere Details, Abnahmekriterien und nicht genannte Systeme bleiben offen.

| Rang / Fall | Ausgangspunkt und bestätigter Bedarf | Noch konkretisieren | Verantwortlichkeit / Abnahme |
| --- | --- | --- | --- |
| 1 – Ablaufdokumentation | Szenarien, Schritte sowie geerbte und überschreibende Zustände sind vorhanden; zentrale Priorität. Keine automatische Ausführung. | Welche Ablaufangaben braucht der Anwender zum Verständnis und zur Pflege? Welche fachlichen Fälle müssen dokumentiert und abgenommen werden? | Anwender ist primär fachlich abnehmend. |
| 1 – Aktoren-/Zustandsliste | Der XLSX-Export enthält bereits Szenario, Schritt, Aktor und Zustand. Gewünschte Kernaussage: welcher Aktor hat in welchem Szenario/Schritt welchen Zustand? | Reichen die vorhandenen Spalten und Zustandswerte? Welche Darstellung, Sortierung, Filterung oder zusätzliche Angaben sind für den Arbeitsalltag erforderlich? Wird die Datei weiterbearbeitet oder nur ausgegeben? | Anwender ist primär fachlich abnehmend. |
| Potenziell später – Regelungsdokumentation | Gewünscht ist eine Darstellungs-/Dokumentationsmöglichkeit für 2P-, 3P-, Totband-, PID- und Kaskadenregelungen einschließlich Parametern. Noch nicht priorisiert oder spezifiziert. | Welche Daten/Parameter je Regelungsart, Einheiten, Signalbezüge und Ansichten werden benötigt? Wie sind Kaskaden und Szenario-/Schrittvarianten darzustellen? Nur dokumentieren/visualisieren; keine Regelung ausführen oder Prozessverhalten simulieren. | Anwender; fachliche Abnahmekriterien offen. |
| Potenziell später – Alarm- und Fehlerpfaddokumentation | Alarme samt Auswirkungen auf die Anlage als Fehlerpfad je Szenario/Schritt beschreiben. Noch nicht spezifiziert. | Auslöser, betroffene Geräte, Zustandsänderungen, Verzweigungen, Priorität, Quittierung, Rückkehr in Normalbetrieb und Beziehungen zwischen Alarmen/Schritten klären. Gewünschte Darstellung und Exporte offen. | Anwender; fachliche Abnahmekriterien offen. |
| Später, noch nicht priorisiert – DXF | Anwenderinteresse an Rohren-/Symbolerkennung und automatischer Betriebsmittelerstellung. | CAD-Programm und DXF-Variante, verfügbare Symboldaten/-definitionen, Erkennungsgrenzen, Fehlermeldungen und manuelle Bestätigung klären. Kein Implementierungsumfang festgelegt. | Anwender und CAD-Datenverantwortliche; Abnahmeverfahren offen. |
| Fernes mögliches Ziel – TIA Portal | Als denkbares Zielsystem genannt, aber ausdrücklich noch weit entfernt. | Nutzen, Richtung, Austauschformat, Version und konkreter Umfang erst nach Bedarfserhebung bestimmen. Keine Integration zugesagt. | Zielsystemverantwortliche und Abnahme offen. |
| Nicht als aktuelle Priorität bestätigt – PDF-Review | Referenz-PDF, getrennt gespeichertes Overlay, PDF-Hashprüfung und Offline-Review-Paket sind vorhanden. | Nur weiter verfolgen, falls zusätzlicher Bedarf über vorhandene Funktionen hinaus benannt wird. | Anwenderabnahme für Zusatzbedarf offen. |

## Erhebungsbogen je Austauschfall

Für Ablaufdokumentation und Aktoren-/Zustandsliste zuerst gemeinsam mit dem Anwender konkretisieren; für spätere Integrationen die technischen Verantwortlichen hinzuziehen:

1. Aufgabe, Auslöser, Häufigkeit und beteiligte Rollen; heute verwendete Werkzeuge und typische Fehlerfälle.
2. Quell- und Zielsystem einschließlich Version, Datenverantwortung und führender Quelle je Feld.
3. Benötigte Objekte und Felder, Kennungen, Beziehungen, Szenario-/Schrittbezug sowie Referenz- und Revisionsinformationen.
4. Gewünschtes Ergebnis, Austauschformat, Richtung, Umfang und Zeitpunkt; akzeptable manuelle Arbeitsschritte.
5. Verhalten bei unbekannten, ungültigen oder nicht unterstützten Feldern. Entscheidung je Fall: sichtbar melden, Import/Austausch anhalten, mit Bestätigung fortsetzen oder verlustfrei erhalten, sofern das Format dies erlaubt. Keine stille Verwerfung.
6. Kritikalität, erwartbare Fehler und konkretes Abnahmekriterium; verantwortliche fachliche Abnahme.

Für den Excel-Schwerpunkt insbesondere klären, ob der vorhandene Export den benötigten Anlagenzustand verständlich und vollständig wiedergibt und woran der Anwender eine fachlich richtige Ausgabe erkennt.

## Prüfkorpus und Rechte

| Material | Aktueller Status | Nächster Schritt |
| --- | --- | --- |
| `examples/FlowSketch-Demo.pid2process.json`, `FlowSketch-Demo.png`, `FlowSketch-Demo-Aktoren.xlsx` | Im Repository vorhanden und als FlowSketch-Demo dokumentiert; keine Referenz-PDF. Ein separater Rechte-/Herkunftsnachweis ist nicht vermerkt. | Herkunft und Nutzungsrechte dokumentieren, bevor die Dateien als extern freigegebener Prüfkorpus bezeichnet werden. |
| Repräsentative Referenzpläne und reale Austauschdaten | Noch nicht gesammelt; keine solchen Unterlagen ungeprüft in das Repository übernehmen. | Nur nicht vertrauliche oder ausdrücklich zur Nutzung freigegebene Beispiele aufnehmen; Quelle, Erlaubnis, Umfang und Ablageentscheidung dokumentieren. |
| Austauschfall-Testdaten | Noch nicht aus Anwenderfällen abgeleitet. | Nach der Erhebung künstliche oder freigegebene Fixtures samt erwarteten Ergebnissen und Fehlerfällen erstellen. |

## Abgrenzung

FlowSketch bleibt ein Review- und Dokumentationswerkzeug mit unveränderter Referenz-PDF und separat gespeichertem Overlay. Nicht Teil dieses Arbeitspakets sind Anlagensteuerung oder SPS-/HMI-Anbindung, automatische Szenarioausführung, hydraulische oder thermische Simulation sowie automatische fachliche Freigaben. Normkonformität oder Vollständigkeit wird nicht behauptet.

## Offene Punkte / Abnahme

- [ ] Konkrete Abnahmekriterien mit dem primär fachlich abnehmenden Anwender festlegen.
- [ ] Ablaufdokumentation und Aktoren-/Zustandsliste mit dem Anwender konkretisieren: Zustandsbedeutung, erwartete Vollständigkeit, Ausgabeform und fachliche Abnahmekriterien.
- [ ] Mögliche Regelungsdarstellung und Alarm-/Fehlerpfade zunächst anhand konkreter Beispiele und benötigter Parameter/Auswirkungen spezifizieren und priorisieren; keine Steuerungs- oder Simulationsfunktion daraus ableiten.
- [ ] DXF-Idee nur bei weiterer Priorisierung mit CAD-Symboldaten, Dateivariante, Erkennungsgrenzen und notwendiger manueller Prüfung konkretisieren; TIA Portal bleibt ein fernes mögliches Ziel ohne zugesagte Integration.
- [ ] Verhalten bei unbekannten oder nicht unterstützten Angaben sowie Datenverantwortung für tatsächlich priorisierte Fälle festhalten.
- [ ] Herkunft und Nutzungsrechte des vorhandenen Demo-Prüfkorpus klären; geeignete nicht vertrauliche Referenzpläne/Austauschdaten sammeln oder ausdrücklich feststellen, dass keine aufgenommen werden.
- [ ] Priorisierte Anwendungsfälle und geeignete Prüfdaten fachlich abnehmen.

**Abhängigkeit:** Der Anwender hat die Richtung und primäre Abnahme benannt. Die fehlenden Detailanforderungen und Abnahmekriterien für Aktorzustände, Regelungsdarstellung und Fehlerpfade sowie gegebenenfalls CAD-/Zielsystemangaben können nicht aus dem Repository abgeleitet werden.
