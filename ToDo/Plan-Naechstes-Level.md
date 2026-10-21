# FlowSketch auf das nächste Level heben

## Zielbild

FlowSketch soll das verlässliche, leicht zugängliche Werkzeug für Reviews und Ablaufdokumentation auf vorhandenen R&I-/P&ID-Plänen werden. Es soll fachliche Angaben nachvollziehbar prüfen und strukturierte Daten mit anderen Engineering-Werkzeugen austauschbar machen, ohne seine Stärke – die unveränderte Referenz-PDF mit separat gespeichertem Overlay – aufzugeben.

Das ist keine Roadmap zu einem vollwertigen CAD-, Simulations- oder Leitsystem. FlowSketch bleibt ohne SPS-/HMI-Anbindung und ohne hydraulische oder thermische Berechnung. Normkonformität oder Anlagenfreigabe darf nur für konkret geprüfte Profile und Anwendungsfälle behauptet werden.

## Ausgangslage

Der dokumentierte Stand ist Version 0.8.1. Bereits vorhanden sind unter anderem normierte Overlay-Geometrie, Geräte und Rohranschlüsse, Medienwege, Szenarien und Schritte, PDF-/JPG-Exporte, ein Offline-Review-Paket, Aktorenlisten sowie eine Freigabe, die an den PDF-Hash gebunden ist. Die Tag-, Funktions- und Ortsangaben sind getrennte Felder, Tags sind jedoch Freitext. Der Excel-Export ist eine Aktorenliste und kein vollständiger Projektimport/-export.

Die folgenden Arbeitspakete bauen darauf auf. Sie sind nach Abhängigkeit und Nutzen geordnet; größere Integrationen stehen unter einem Entscheidungsvorbehalt.

## Arbeitspaket 1 – Anforderungen und Austauschfälle festlegen

**Status: begonnen.** Die Priorität Ablaufdokumentation und Aktoren-/Zustandsliste sowie der Anwender als primär fachlich Abnehmender sind bestätigt. Der Arbeitsbogen [Anforderungen-und-Austauschfaelle.md](Anforderungen-und-Austauschfaelle.md) hält die Richtung fest: Excel beantwortet, welcher Aktor in welchem Szenario/Schritt welchen Zustand hat. DXF-Erkennung mit CAD-Symboldaten ist eine spätere, noch offene Idee; TIA Portal ein fernes mögliches Ziel ohne zugesagte Integration. Details, Prüfdaten/Rechteklärung und fachliche Abnahmekriterien bleiben offen.

**Zweck:** Vor Norm- oder Importfunktionen klären, welche Anwender und Zielsysteme tatsächlich unterstützt werden sollen.

**Aufgaben**
- Zuerst Ablaufdokumentation und Pflege der Aktoren-/Zustandsliste mit dem Anwender konkretisieren; weitere Fälle nur nach bestätigter Priorität vertiefen.
- Beispieldaten und Referenzpläne mit geklärten Nutzungsrechten sammeln; vertrauliche Anlagenunterlagen nicht ungeprüft ins Repository übernehmen.
- Für jeden Austauschfall Quell-/Zielsystem, benötigte Daten, Verantwortlichkeit und gewünschtes Verhalten bei nicht unterstützten Feldern definieren.
- Eine verbindliche Abgrenzung dokumentieren: keine Anlagensteuerung, keine Simulation, keine automatische Freigabe fachlicher Inhalte.

**Ergebnis / Abnahme**
- Priorisierte Anwendungsfälle und ein gepflegter, nicht vertraulicher Prüfkorpus liegen vor.
- Für jedes spätere Arbeitspaket ist klar, wer den Nutzen fachlich abnimmt und was ausdrücklich außerhalb des Umfangs liegt.

## Arbeitspaket 2 – Projektprofile für Kennzeichnungen und Symbole

**Zweck:** Wiedererkennbare Kennzeichnungen und eine nachvollziehbare Symbolauswahl ermöglichen, ohne kundenspezifische Konventionen als universell auszugeben.

**Aufgaben**
- Ein wählbares Kennzeichnungsprofil entwerfen: zulässige Tags, Nummernregeln, Geltungsbereich der Eindeutigkeit und gegebenenfalls Tag-Schemata für Instrumente.
- Tag, Geräteklasse, Funktion und Einbauort als verschiedene Informationen behandeln; vorhandene Freitextwerte und alte Projekte erhalten.
- Symbolbibliothek nach Geräteklassen und Varianten organisieren. Für Symbole Metadaten wie interne Klasse, Anschlussports, Varianten und Herkunft/Profil verwalten.
- ISO 10628-2/ISO 14617 für passende Prozesssymbolik und ISA-5.1 für Instrumentierung als zu prüfende Grundlagen bewerten. Normausgaben, konkrete Zeichnungen und Nutzungsrechte prüfen, bevor Symbole als normgerecht bezeichnet oder übernommen werden.
- Eigene oder Betreiberprofile ermöglichen, ohne die Bibliothek auf eine einzige Konvention festzulegen.

**Ergebnis / Abnahme**
- Ein dokumentiertes Profil deckt die gewählten Kernklassen ab; Symbole und Tag-Regeln sind fachlich geprüft und ihre Grenzen benannt.
- Ein Öffnen und Speichern älterer Projekte verändert weder bestehende Tags noch Zuordnungen stillschweigend.

## Arbeitspaket 3 – Fachliche Validierung im Projekt

**Zweck:** Fehlende, doppelte oder widersprüchliche Angaben auffindbar machen, ohne individuelle Projektregeln zu blockieren.

**Aufgaben**
- Prüfungen für Profilformat, leere Pflichtfelder und doppelte Tags mit konfigurierbarem Geltungsbereich vorsehen.
- Beziehungen prüfen: Referenzen auf vorhandene Objekte, Anschlusskompatibilität, nicht verbundene Rohrenden, verwaiste Zuordnungen und ungültige Szenario-/Schrittbezüge.
- Ergebnisse als verständliche Fehler und Warnungen mit Navigation zum betroffenen Objekt anzeigen.
- Fehler und Warnungen unterscheiden; fachlich plausible Sonderfälle dürfen durch Warnungen kenntlich bleiben.
- Prüfstatus in Freigabe und Export nachvollziehbar machen. Eine technische Prüfung ist keine fachliche Anlagenfreigabe.

**Ergebnis / Abnahme**
- Tests decken gültige Fälle, Fehler, Warnungen, Grenzfälle und Projekte ohne Profil ab.
- Die Prüfung ändert keine Projektwerte automatisch und blockiert nicht pauschal bestehende Projekte.
- Die Freigabe wird bei relevanten Engineering-Änderungen weiterhin widerrufen; der PDF-Hash wird geprüft.

## Arbeitspaket 4 – Strukturierter Tabellenimport und -export

**Zweck:** Einen risikoarmen, praktisch nutzbaren Datenaustausch schaffen, bevor ein vollständiges Anlagenmodell integriert wird.

**Aufgaben**
- CSV-/XLSX-Import und -Export für einen klar abgegrenzten Umfang definieren, zunächst Betriebsmittel und Aktoren.
- Spaltenzuordnung, Vorschau und Validierung vor dem Übernehmen anbieten; Dubletten und unbekannte Werte nicht stillschweigend verwerfen.
- Import als überprüfbare Transaktion gestalten: Zuordnungen anzeigen, Fehler adressierbar machen und Übernahme rückgängig machen können.
- Bestehenden Aktorenexport beibehalten und dokumentieren, welche Szenario-/Schrittzustände enthalten sind.

**Ergebnis / Abnahme**
- Ein Roundtrip-Test mit Beispieldaten weist nach, welche Felder verlustfrei übertragbar sind und welche nicht.
- Fehlerhafte oder teilweise unbekannte Zeilen bleiben nachvollziehbar; ein abgebrochener Import verändert das Projekt nicht.

## Arbeitspaket 5 – DEXPI-Austausch bewerten und gegebenenfalls prototypisieren

**Zweck:** Prüfen, ob ein standardisierter P&ID-Datenaustausch für FlowSketch konkreten Mehrwert bringt.

**Aufgaben**
- DEXPI-Version und Spezifikation anhand der festgelegten Austauschfälle evaluieren; Klassen, Verbindungen, Geometrie und Metadaten mit dem FlowSketch-Modell abgleichen.
- Eine Zuordnungsmatrix für vollständig unterstützte, teilweise unterstützte und nicht abbildbare Inhalte erstellen.
- Zunächst einen begrenzten Prototyp mit nicht vertraulichen Testdaten prüfen. Unbekannte Daten erhalten oder explizit melden, nicht stillschweigend entfernen.
- Szenarien und Schrittüberschreibungen separat betrachten: Sie sind nicht automatisch Teil einer verlustfreien P&ID-Übergabe.
- Erst nach erfolgreicher fachlicher Abnahme Umfang und Wartungsaufwand einer dauerhaften Integration entscheiden.

**Ergebnis / Abnahme**
- Eine dokumentierte Go/No-go-Entscheidung einschließlich Versionsbindung, Lizenz-/Nutzungsfragen und Grenzen liegt vor.
- Ein Import wird nur freigegeben, wenn Zuordnungen, Verluste und Fehlerfälle reproduzierbar getestet sind.

## Arbeitspaket 6 – Review, Nachvollziehbarkeit und Teamabläufe

**Zweck:** Reviews für mehrere Beteiligte sicherer und leichter nachvollziehbar machen.

**Aufgaben**
- Review-Pakete um Projekt-/PDF-Identität, ausgewählten Szenario-/Schrittstand und Prüfhinweise ergänzen.
- Änderungen an fachlichen Daten nachvollziehbar machen; eine Audit-Historie getrennt vom vorhandenen Undo/Redo bewerten.
- Nutzerrollen, gemeinsame Ablage und Konfliktbehandlung nur dann planen, wenn konkrete Mehrbenutzeranforderungen belegt sind.
- Export und Offline-Viewer anhand identischer Szenario-/Schrittwerte vergleichen.

**Ergebnis / Abnahme**
- Reviewer können Referenz, Projektstand und dargestellten Ablauf eindeutig zuordnen.
- Veränderungen an Referenz-PDF oder Overlay sind erkennbar; Exporte geben keine nicht geprüfte Anlagenfreigabe vor.
- Kollaboration wird nicht durch parallele Bearbeitung ohne Konfliktregeln vorgetäuscht.

## Arbeitspaket 7 – Bedienbarkeit, Kompatibilität und Auslieferungsqualität

**Zweck:** Die fachlichen Erweiterungen mit verlässlicher Bedienung und klaren Releases absichern.

**Aufgaben**
- Aufgabenbasierte Abnahme mit repräsentativen Plänen durchführen: PDF öffnen, Overlay bearbeiten, Ablauf prüfen, exportieren und Ergebnisse wiederfinden.
- Relevante Windows-, DPI- und Mehrmonitorfälle praktisch prüfen; gespeicherte Prüfbilder und bildschirmfreie Modelltests ergänzen, aber nicht mit echter Bedienabnahme gleichsetzen.
- Regressionen für Altprojekte, PDF-Hash, Undo/Redo, Kopieren, statischen Export und Review-Viewer pflegen.
- Versionierte Auslieferungen mit aktualisierter Hilfe, Beispielen und dokumentierten Grenzen bereitstellen; frühere Versionsordner erhalten.

**Ergebnis / Abnahme**
- Build, Modell-/Regressionstests, gespeicherte Renderbilder und echte Bedienabnahme werden getrennt dokumentiert.
- Ein Release enthält keine ungeprüften Behauptungen zu Normkonformität, Simulation oder Anlagenfreigabe.

## Reihenfolge und Entscheidungstore

1. **Zuerst:** Arbeitspaket 1, damit Nutzerbedarf, Prüfkorpus und Austauschziele feststehen.
2. **Danach parallelisierbar:** Arbeitspaket 2 und ein erster, eng begrenzter Entwurf für Arbeitspaket 4.
3. **Auf geprüften Profilen aufbauen:** Arbeitspaket 3; Validatoren nicht vorab auf unbestätigte Regeln festlegen.
4. **Nach dem Tabellen-Roundtrip entscheiden:** Arbeitspaket 5 nur weiterführen, wenn DEXPI gegenüber CSV/XLSX einen belegten Zusatznutzen bietet.
5. **Querschnitt und Release:** Arbeitspakete 6 und 7 begleiten jede nutzerrelevante Erweiterung, nicht erst den Abschluss.

## Nicht Teil dieses Plans

- SPS-/HMI-Verbindungen, Steuerungslogik oder automatische Ausführung von Szenarien.
- Hydraulische, thermische oder dynamische Simulation.
- Behauptete Vollständigkeit oder Normkonformität ohne geprüfte Normausgabe, Rechteklärung und fachliche Abnahme.
- Automatische Konvertierung beliebiger Referenz-PDFs in ein vollständiges Anlagenmodell.
