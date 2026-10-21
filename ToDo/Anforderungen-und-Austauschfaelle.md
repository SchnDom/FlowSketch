# Arbeitspaket 1 – Anforderungen und Austauschfälle

**Status:** begonnen; Arbeitsstand, keine fachliche Abnahme.  
**Ziel:** typische Aufgaben, Austauschziele, Verantwortlichkeiten und den Umgang mit nicht unterstützten Angaben mit repräsentativen Anwendern abstimmen, bevor Profile oder Importfunktionen festgelegt werden.

## Belegter Ist-Stand

Diese Bestandsaufnahme beschreibt den Repository-Stand, nicht bestätigte Anwenderanforderungen:

- FlowSketch bearbeitet ein Overlay getrennt von der unveränderten Referenz-PDF. Eine Freigabe ist an die PDF-Prüfung gebunden und Engineering-Änderungen widerrufen sie.
- Projekte enthalten unter anderem Geräte mit Tag, Funktion und Einbauort, Verbindungen, Medien, Szenarien und Schritte. Schrittwerte können Szenariowerte überschreiben; alte Schritte ohne Szenario bleiben unzugeordnet.
- Der XLSX-Aktorenexport ist ein Einweg-Export mit den Spalten **Szenario**, **Schritt**, **Aktor** und **Zustand**. Er umfasst Aktoren je Szenario/Schritt, nicht das vollständige Projekt. Die konkrete Aktorauswahl ist dokumentiert; ein Projektimport ist nicht vorhanden.
- Das Repository enthält eine Demo-Projektdatei, ihre PNG-Vorschau und eine erzeugte Aktorenliste. Die Demo ist laut Begleitdokumentation keine reale Anlagensteuerung und benötigt keine Referenz-PDF. Ein Nutzungsrechtsnachweis oder eine Referenz-PDF ist im Repository derzeit nicht dokumentiert.
- FlowSketch ist keine SPS-/HMI-Steuerung, führt keine Szenarien automatisch aus und berechnet keine hydraulischen oder thermischen Größen.

## Vorläufige Austauschfälle

Die Fälle stammen aus der Roadmap und sind noch durch Anwender zu priorisieren. Unbekannte Systeme oder Abläufe sind ausdrücklich offen.

| Fall | Belegter Ausgangspunkt | Mit Anwendern zu klären | Verantwortlichkeit / Abnahme |
| --- | --- | --- | --- |
| PDF-Review | Referenz-PDF, getrennt gespeichertes Overlay, PDF-Hashprüfung und Offline-Review-Paket sind vorhanden. | Wer erstellt, prüft und verteilt ein Review? Welche Identität, Szenario-/Schrittansicht und Prüfhinweise müssen mitgegeben werden? Welche Fehler (z. B. falscher Planstand) sind kritisch? | Planverantwortliche und Reviewer benennen; fachliche Abnahme offen. |
| Ablaufdokumentation | Szenarien, Schritte sowie geerbte und überschreibende Zustände sind vorhanden; keine automatische Ausführung. | Wer pflegt Abläufe? Welche Voraussetzungen, Aktionen und Übergänge müssen dokumentiert werden? Wie wird die dargestellte Auswahl im Review verwendet? | Prozess-/Ablaufverantwortliche benennen; fachliche Abnahme offen. |
| Aktorenliste pflegen | XLSX-Ausgabe mit Szenario, Schritt, Aktor und Zustand ist vorhanden; kein Tabellenimport. | Ist die Liste nur Ausgabe oder wird sie außerhalb FlowSketch weiterbearbeitet? Welche Felder, Änderungswege, Kennungen und Aktualisierungsregeln sind erforderlich? | Listenverantwortliche und führendes System benennen; fachliche Abnahme offen. |
| Übergabe an ein Engineering-System | Ein Zielsystem oder Austauschformat ist im Repository nicht festgelegt. | Quell- und Zielsystem samt Version, Austauschformat, Umfang, Richtung, Häufigkeit, benötigte Felder und führende Datenquelle erheben. | Zielsystemverantwortliche und fachlich abnehmende Person benennen; offen. |

## Erhebungsbogen je Austauschfall

Für jeden priorisierten Fall gemeinsam mit repräsentativen Anwendern ausfüllen:

1. Aufgabe, Auslöser, Häufigkeit und beteiligte Rollen; heute verwendete Werkzeuge und typische Fehlerfälle.
2. Quell- und Zielsystem einschließlich Version, Datenverantwortung und führender Quelle je Feld.
3. Benötigte Objekte und Felder, Kennungen, Beziehungen, Szenario-/Schrittbezug sowie Referenz- und Revisionsinformationen.
4. Gewünschtes Ergebnis, Austauschformat, Richtung, Umfang und Zeitpunkt; akzeptable manuelle Arbeitsschritte.
5. Verhalten bei unbekannten, ungültigen oder nicht unterstützten Feldern. Entscheidung je Fall: sichtbar melden, Import/Austausch anhalten, mit Bestätigung fortsetzen oder verlustfrei erhalten, sofern das Format dies erlaubt. Keine stille Verwerfung.
6. Kritikalität, erwartbare Fehler und konkretes Abnahmekriterium; verantwortliche fachliche Abnahme.

## Prüfkorpus und Rechte

| Material | Aktueller Status | Nächster Schritt |
| --- | --- | --- |
| `examples/FlowSketch-Demo.pid2process.json`, `FlowSketch-Demo.png`, `FlowSketch-Demo-Aktoren.xlsx` | Im Repository vorhanden und als FlowSketch-Demo dokumentiert; keine Referenz-PDF. Ein separater Rechte-/Herkunftsnachweis ist nicht vermerkt. | Herkunft und Nutzungsrechte dokumentieren, bevor die Dateien als extern freigegebener Prüfkorpus bezeichnet werden. |
| Repräsentative Referenzpläne und reale Austauschdaten | Noch nicht gesammelt; keine solchen Unterlagen ungeprüft in das Repository übernehmen. | Nur nicht vertrauliche oder ausdrücklich zur Nutzung freigegebene Beispiele aufnehmen; Quelle, Erlaubnis, Umfang und Ablageentscheidung dokumentieren. |
| Austauschfall-Testdaten | Noch nicht aus Anwenderfällen abgeleitet. | Nach der Erhebung künstliche oder freigegebene Fixtures samt erwarteten Ergebnissen und Fehlerfällen erstellen. |

## Abgrenzung

FlowSketch bleibt ein Review- und Dokumentationswerkzeug mit unveränderter Referenz-PDF und separat gespeichertem Overlay. Nicht Teil dieses Arbeitspakets sind Anlagensteuerung oder SPS-/HMI-Anbindung, automatische Szenarioausführung, hydraulische oder thermische Simulation sowie automatische fachliche Freigaben. Normkonformität oder Vollständigkeit wird nicht behauptet.

## Offene Punkte / Abnahme

- [ ] Repräsentative Anwender und fachliche Abnehmende für die vier Austauschfälle benennen.
- [ ] Aufgaben, Prioritäten, Fehlerfälle und Akzeptanzkriterien mit ihnen erheben.
- [ ] Quell-/Zielsysteme, Feldumfang, Verantwortlichkeit und Verhalten bei nicht unterstützten Feldern je priorisiertem Fall festhalten.
- [ ] Herkunft und Nutzungsrechte des vorhandenen Demo-Prüfkorpus klären; geeignete nicht vertrauliche Referenzpläne/Austauschdaten sammeln oder ausdrücklich feststellen, dass keine aufgenommen werden.
- [ ] Priorisierte Anwendungsfälle und gepflegten Prüfkorpus fachlich abnehmen.

**Abhängigkeit:** Diese Punkte benötigen Angaben und Abnahme durch repräsentative Anwender. Der vorliegende Arbeitsstand ersetzt keine Interviews und trifft keine Entscheidung für ein Engineering-Zielsystem.
