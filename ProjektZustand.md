# Projektzustand FlowSketch

Stand: 20.10.2026. Neue Ausgabe **0.7.0**, `bin/0.7.0/FlowSketch.exe`. Der vom Nutzer gewählte Programmname ersetzt PID2Process; Projektdateien und frühere Versionsordner bleiben erhalten.

## Repository und Projektablage

- Projektordner auf Nutzerauftrag nach `C:\01_Files\03_Repositories\FlowSketch` umgezogen; bisheriger Name `PID2Process` bleibt nur in internen Solution-/Quellpfaden und dem bestehenden Dateiformat erhalten.
- GitHub: `https://github.com/SchnDom/FlowSketch`, Hauptzweig `main`. Quellen, Dokumentation, Markenbilder und eingebettete Demo werden versioniert. Referenz-PDFs und persönliche Projekte unter `_PIDs`, Buildausgaben und Testartefakte bleiben lokal erhalten und sind ausgeschlossen.
- README als Repository-Einstieg mit Einrichtung, Funktionen, Build-/Prüfbefehlen, Verzeichnisstruktur und Grenzen aktualisiert. Anwendungsversion bleibt 0.7.0; dieser Umzug ändert keine Programmfunktionen.
- Umzug geprüft: 1.863 Dateien mit identischen relativen Pfaden und Größen sowie 26 SHA-256-Prüfsummen für Nutzerprojekte, Referenzdateien und Markenbilder unverändert. Am alten Pfad verbleiben nur durch die laufende Codex-Sitzung gesperrte leere Ordner; sämtliche Projektdateien liegen am neuen Pfad. Diese leeren Reste können nach Schließen der alten Sitzung entfernt werden.
- Erster Quellstand auf `origin/main` veröffentlicht; README-Verweise und Git-Ausschlüsse geprüft. Release-Quellbuild am neuen Pfad erfolgreich (0 Fehler, NU1900 wegen nicht erreichbarer NuGet-Sicherheitsdaten). Node-Viewer-, Zoom- und Offline-Hilfeprüfungen am neuen Pfad bestanden. Keine erneute Mausprüfung und kein neuer funktioneller Release für den reinen Umzug.

## Aktueller Stand – 0.7.0

- Dunkle ComboBox-Vorlage übernimmt die ItemTemplateSelector-/DisplayMemberPath-Darstellung: obere Szenario-/Schrittauswahl zeigt Namen statt .NET-Typnamen. Editierbare Combos behalten ihre Texteingabe. Menüseparatoren über den speziellen WPF-Menü-Style-Key korrigiert: voller Inhaltsbereich mit passender Einrückung, einheitlicher Farbton #52677F und vertikale Trenner auf 20 px abgestimmt.
- Programm-, Fenster-, Titel- und Taskleistenicon aus `_media/CS26001-1-Icon-4c.png` in 16/24/32/48/64/128/256 px abgeleitet, Transparenz erhalten. Keine Neuzeichnung der Marke. Hilfe/Über uns sowie Offline-Hilfe zeigen `_media/CS26001-1-Logo-4c.png`, unverzerrt auf hellem Grund. Original-PNGs bleiben unverändert.
- Gerätedaten mit den Gruppen Kennzeichnung, Objekteigenschaften und Farben und Größen. Zwei kompakte Feldspalten; lange Beschreibung und Sensortabellen über volle Breite. Verdeckte Felder hinterlassen keine Lücken, Checkboxen tragen ihre Beschriftung direkt. Abbrechen/Validierung bleiben transaktional.
- Gesamte vorhandene Dialogfamilie geprüft und dunkel gestaltet: Gerätedaten/Zeichnen/Projekt/Alarm, Sammeländerung, Medien/Farbpalette, Legende, Szenario-/Schritt-/Alarmtabellen, Farbrad, Hilfe und Über uns. Dunkle Eingaben, Auswahllisten, Tabellenüberschriften/Zellen, Gruppen, Expander und Scrollleisten. Native Dialogtitel erhalten auf unterstützten Windows-Versionen dunkle DWM-Farben. Formlayout orientiert sich an Microsoft Forms/Grouping (`learn.microsoft.com/windows/apps/design/controls/forms`, `learn.microsoft.com/windows/win32/uxguide/vis-layout`).
- Rechter Tab Abläufe vereint Szenario (oben) und zugehörige Schritte (unten), mit verstellbarem Trennsteg. Betriebsmittel und Alarme bleiben eigene Tabs; Planübersicht bleibt erhalten. Auswahl synchronisiert den Plan, Szenariowechsel wählt den ersten Schritt. Letzter Schritt eines Szenarios vor Löschen geschützt; bestehende verwaiste Schritte bleiben in den Daten erhalten.
- Rückschlagarmatur/-klappe in den Gerätedaten über Zugelassene Flussrichtung spiegelbar. Vorhandene Kontur gespiegelt, Ports bleiben geometrisch links/rechts, keine stille Rohr-Neuzuordnung. Gerichtete Netzweitergabe und Darstellung nativ/SVG berücksichtigen die Einstellung. Grün nur auf tatsächlich durchflossenem Quelle-Ziel-Weg, sonst grau; kein gespeicherter abgeleiteter Zustand. JSON, Kopieren und Undo übernehmen die neue Objekteigenschaft.
- Auslieferung 0.7.0: Release-Build/Publish ohne Compilerfehler/-warnungen; zusätzlich NU1900-Hinweis wegen nicht erreichbarer NuGet-Sicherheitsdaten. EXE-Dateiversion 0.7.0.0. Ausgelieferte EXE: **413 PASS, kein FAIL, Rückgabecode 0**. Neue Prüfungen umfassen echte Namensdarstellung im VisualTree, Ablauf-/Schrittfilter und Auswahl, kompakte Gerätedaten/Abbruch, editierbare Auswahl, vollständige Wortmarke, beide Rückschlagtypen/Richtungen/Quellseiten und fehlende Ziele sowie JSON. Node-Viewer-/Zoomtests und Offline-Hilfe inklusive unveränderter Marken-PNG bestanden.
- Gespeicherte Renderbilder geprüft: `flowsketch-0.7.0.png`, `device-dialog-0.7.0.png`, `scenario-dialog-0.7.0.png`, Menü- und Über-uns-Bilder; Sensor-/Medienansichten zusätzlich erzeugt. Hilfebilder aktualisiert. Keine erneute Bildschirmübernahme nach dem vorherigen Esc-Abbruch; echte Maus-/Fenster-/Taskleistenabnahme dieser Version bleibt separat offen. Frühere Builds, Nutzerdateien und Referenz-PDFs erhalten.

## Bisheriger Stand – 0.6.1

- Feste Planwerkzeugleiste ohne irreführende Ziehpunkte und ohne hellen nativen Überlaufbereich. Bei geringerer Breite bleiben alle Werkzeuge durch Zeilenumbruch erreichbar.
- Einheitliche Trennlinienfarbe #45505D in Titel-, Plan- und Menübereichen. Zusätzlicher Abstand vor Szenario; Hilfe und README aktualisiert. Keine Änderung an Dokumentgeometrie, Zuständen oder PDF-Referenzen.
- Gruppenrahmen werden über der PDF-Unterlage, aber hinter Symbolen und ihren Textbeschriftungen gezeichnet; keine grüne Linie über Text.
- Auslieferung 0.6.1: Build/Publish erfolgreich, keine Compilerfehler; NU1900-Hinweis wegen nicht erreichbarer NuGet-Sicherheitsdaten. EXE-Dateiversion 0.6.1.0. Ausgelieferte EXE: 370 PASS, kein FAIL, Rückgabecode 0, einschließlich Ebenenreihenfolge des Gruppenrahmens. Node-Viewer-/Zoomtests und Offline-Hilfe bestanden. Gespeichertes Prüfbild `test-results/toolbar-0.6.1.png` betrachtet: keine Griffpunkte/kein heller Überlauf, einheitliche Trennlinien und freie Gruppenbeschriftungen. Keine erneute Bildschirmübernahme; echte Mausprüfung separat offen. Frühere Versionen und Nutzerinstanzen erhalten.

## Bisheriger Stand – 0.6.0

- Integrierte dunkle Titelleiste mit WPF WindowChrome: Logo, Datei, Projekt, Hilfe, Trennlinie, Speichern-Symbol, Trennlinie, Export-Symbol, Trennlinie, Rückgängig/Wiederholen. Projektname mittig, Fensteraktionen rechts. Menü und Buttons in der Caption-Zone bedienbar; native Resize-/Verschiebe-/Systemmenü-Bereiche erhalten. Export-Schnellaktion bietet die bestehenden Excel-/Review-/PDF-/JPG-Varianten. Eigene Vektorsymbole, vorhandenes Logo.
- Kontextmenüs mit hellerem Zeilenhintergrund auf dunklem Rahmen. Haupt-/Unter-/Kontextmenüs verwenden vollständige Inhaltsanzeige ohne ScrollViewer oder feste Maximalhöhe; Dropdown-Auswahlfelder sind davon unabhängig.
- Gruppierte Symbole, Rohre und Wirklinien je Blatt mit einem hellgrünen gestrichelten Rahmen um alle Mitglieder; auch ohne Auswahl sichtbar. Reine Editorhilfe, nicht in Export oder Projektzuständen gespeichert.
- Rückgängig/Wiederholen als Schnellaktionen und STRG+Z / STRG+Y bzw. STRG+UMSCHALT+Z. Bis zu 100 bestätigte Änderungen je Dokument; neue Änderungen löschen den Redo-Zweig. Verlauf umfasst Projektgeometrie, Gruppen, Geräte-/Medien-/Szenario-/Schritt-/Alarmdaten. Kein Verlauf für Zoom/Ansicht, keine Speicherung des Verlaufs in Projektdateien, keine Wiederherstellung einer Engineering-Freigabe. Texteingaben und offene Dialoge werden nicht durch globales Undo überschrieben.
- Online-Referenzen: Microsoft Windows-Menükonventionen (`learn.microsoft.com/windows/win32/uxguide/cmd-menus`) und WPF WindowChrome (`learn.microsoft.com/dotnet/api/system.windows.shell.windowchrome`). Letzte Nutzervorgabe mit drei Textmenüs und Schnellaktionen hat Vorrang vor zusätzlichem Bearbeiten-Menü.
- .NET 10 bleibt unverändert, Auslieferung weiterhin mit separat installierter Desktop Runtime. Auf Nutzerfrage nur Größenordnung für eine mögliche eigenständige Ausgabe genannt; keine Umstellung beauftragt. Größen-Testpublish auf Nutzerwunsch abgebrochen.
- Auslieferung 0.6.0: Release-Quellbuild 0 Compilerwarnungen/0 Fehler. Publish erfolgreich mit NU1900-Hinweis (NuGet-Sicherheitsdaten nicht erreichbar). EXE-Dateiversion 0.6.0.0. Ausgelieferte EXE: 369 PASS, kein FAIL, Rückgabecode 0; inklusive Undo-/Redo-Verlauf, Dokumenttrennung, Freigabesperre, Gruppenrahmen, Titel-/Schnellaktionsaufbau und scrollfreien Menüvorlagen. Node-Viewer-/Zoomregression und Offline-Hilfeprüfung bestanden. Gespeicherte Bilder `test-results/desktop-chrome-0.6.0.png` und `test-results/context-menu-0.6.0.png` betrachtet; Hilfe mit neuer Oberfläche aktualisiert. Keine erneute Bildschirmübernahme nach Esc, echte Fensterbedienung (Ziehen/Resize/Minimieren/Maximieren/Schließen) noch separat abzunehmen. Frühere Ausgaben und laufende Nutzerinstanzen erhalten.

## Bisheriger Stand – 0.5.1

- Abwählen entfernt Größen-/Endpunkt-/Sensorgriffe und Auswahlrahmen sofort. Bei Wechsel zwischen einzelnen Symbolen bleiben keine Griffe am vorherigen Symbol. Klick außerhalb des Blatts hebt die Auswahl ebenfalls auf.
- Demo zusätzlich in die EXE eingebettet; Hilfe → Demo öffnen benötigt keine externe Begleitdatei und öffnet weiterhin eine unabhängige Arbeitskopie. Beispielprojekt und frühere Ausgaben unverändert erhalten.
- Planübersicht verwendet das vollständige Dokument unabhängig vom Hauptzoom; bei Zoom unter Einpassen bleibt die Vorschau vollständig eingepasst und der Sichtfensterrahmen auf das Blatt begrenzt. Aktualisierung auch nach PDF-Nachrendern.
- Plan-Kontextmenüs vereinheitlicht nach leerer Fläche, Einzel- und Mehrfachauswahl. Objekt bearbeiten / Objekte bearbeiten (gemeinsame Eigenschaften), Gruppenaktionen bei Mehrfachauswahl, Kopieren/Einfügen/Löschen und Linienwerkzeuge in gewünschter Reihenfolge mit Trennlinien. Aktionen ohne Auswahl sichtbar deaktiviert.
- Betriebsmittelsuche mit Lupe im Feld; aktiver Filter zeigt ein anklickbares Löschkreuz. Wildcards * und ? je Tag, Funktion oder Einbauort, unabhängig von Groß-/Kleinschreibung; ohne Wildcards Teiltextsuche.
- Hilfe und README aktualisiert. Bildschirmfreie Integrationsprüfungen für Auswahlwechsel, Menüaufbau, Wildcards und Filter-Rücksetzen, eingebettete Demo und Übersicht unter Einpassen ergänzt. Keine erneute Bildschirmübernahme nach dem vorherigen Esc-Abbruch; echte Bedienabnahme dieser Änderungen separat offen.
- Prüfung/Auslieferung: Release-Quellbuild ohne Compilerwarnungen/-fehler; Publish erfolgreich mit NU1900-Hinweis, da NuGet-Sicherheitsdaten nicht erreichbar waren. EXE-Dateiversion 0.5.1.0. Ausgelieferte EXE: 349 PASS, kein FAIL, Rückgabecode 0. Node-Viewer-/Zoomtests und Offline-Hilfeprüfung bestanden. Gespeichertes Prüfbild `test-results/editor-corrections-0.5.1.png` betrachtet: Hauptplan unter Einpassen, vollständige feste Übersicht und Suchlupe sichtbar. Keine Nutzerinstanz beendet, Referenz-PDFs und Nutzerprojekt unverändert.

## Bisheriger Stand – 0.5.0

- Aktives Ziel: moderne WPF-Oberfläche nach Nutzerreferenz CustomTkinter, Speichern unter, Symbolvorschauen/verbreiterbare Bibliothek, geometrisch neu gezeichnete Grafiken mit Endpunktgriffen, Zeichenvorgaben per Bibliotheks-Kontextmenü, Planübersicht und projektübergreifendes STRG-C/V. Bestehende Funktionen erhalten; Referenz-PDFs und Nutzerdateien unverändert.
- Quellstand kompiliert; vorläufig 336 Modelltests bestanden, einschließlich Grafikstärken/Objektgrenzen und Mehrfachkopie mit Gruppen, internen/externen Rohrbindungen sowie Szenario-/Schrittwerten. Native und SVG-Zeichengeometrie werden verglichen. `bin/0.4.3/graphics.pid2process.json` gefunden, schreibgeschützt gelesen und als `test-results/graphics-corrected.png` gerendert.
- Erste echte UI-Nachweise: Bibliothek verbreitert; Bibliotheks-Kontextmenü öffnet Zeichenpalette, Farbe und 5-px-Vorgabe greifen für neuen Pfeil. Endpunkt einzeln verschoben, Linie als Ganzes bewegt. Pfeil + Rechteck per Auswahlrahmen markiert, STRG+C, neues Blatt, STRG+V: beide Objekte im neuen Projekt sichtbar.
- Sichtprüfung deckte zu schwache Kontraste, zu dünne Zeichenvorschauen und einen am Hauptviewport abgeschnittenen VisualBrush der Übersicht auf. Kontraste und Vorschauen angepasst, Übersicht auf unabhängig gerendertes Vollplanbild umgestellt.
- Nutzerkorrektur: Menüs einschließlich Untermenüs und Hervorhebung mit expliziter dunkler Vorlage und heller Schrift. Szenario-/Schritt-Auswahl oben einschließlich Auswahllisten dunkel; Betriebsmittel, Szenarien, Schritte und Alarme rechts einschließlich Suche dunkel. Gespeichertes Editorbild nach Korrektur betrachtet; Modelltest 336 PASS, kein FAIL. Der Nutzer stoppte die echte Bildschirmprüfung mit Esc; diese wurde nicht erneut gestartet. Abschließende echte Bedienprüfung der korrigierten Menü-Popups und Übersicht bleibt offen.
- Neue transparente Vektor-Iconmarke mit Frames 16–256 px erzeugt, Firmenwortmarke unverändert. Moderne Oberfläche und Grafikbeispiele als Prüfbilder gespeichert. Hilfe und Übergabe werden nach endgültiger Prüfung vervollständigt.
- Auslieferung 0.5.0: Release-Publish erfolgreich ohne Warnungen/Fehler; EXE-Dateiversion 0.5.0.0. Ausgelieferte EXE mit abgewartetem Prozessende: Rückgabecode 0, 336 PASS, kein FAIL. Node-Viewer-Regression, Review-Zoom und Offline-Hilfe (19 Kapitel, 14 Bilder) bestanden. Hilfe mit aktuellem dunklem Editorbild und Grafikbeispielen aktualisiert. Frühere Versionsordner erhalten; keine laufende Nutzerinstanz beendet.

## Bisheriger Stand – 0.4.5

- Review-Paket mit Mausradzoom zur Mausposition, Plus/Minus, Prozentanzeige und Einpassen. 25–800 % relativ zur vollständig eingepassten Seite. Verschieben über mittlere Maustaste oder freie Fläche mit linker Maustaste; Geräteklicks bleiben erhalten. Tastatur +/−/0 auf fokussierter Planfläche.
- Hintergrund, Symbolkonturen, Beschriftungen, Sensor-, Wirk- und Rohrlinien liegen auf einer gemeinsam transformierten Zeichenfläche. Szenario-/Schrittwechsel erhalten die Ansicht; Seitenwechsel passt die neue Seite ein. Offline ohne zusätzliche Abhängigkeit; vorhandene Pakete neu exportieren. Rasterunterlage bleibt auf die Exportauflösung begrenzt.
- Hilfe und README aktualisiert. Release-Publish erfolgreich ohne Warnungen/Fehler; EXE-Dateiversion 0.4.5.0. Ausgelieferte EXE: **314 PASS, kein FAIL** im bildschirmfreien Modelltest. Node-Viewer-Regression, gezielte Zoomprüfung (Mausverankerung, Grenzen, Pan/Abbruch, Geräteklicks, Seitenformate) und Offline-Hilfeprüfung erfolgreich.
- Echter Browser mit erzeugtem Review-Testpaket: Plus 125 %, Mausrad 159 %, linkes Ziehen verschiebt den Plan; Szenario-/Schrittwechsel behalten 159 %, Seitenwechsel setzt auf Einpassen/100 %. Minus und Einpassen funktionieren. Prüfbild `test-results/review-zoom-0.4.5.png`, Protokoll `test-results/Review-Zoom-0.4.5.md`. Keine neue WPF-Mausprüfung erforderlich; Änderung betrifft den exportierten HTML-Viewer. Laufende Version 0.4.4 und frühere Versionsordner erhalten.

## Bisheriger Stand – 0.4.4

- Jedes Szenario besitzt mindestens einen Schritt. Wechsel im Plan, Szenarioeditor und Review-Viewer öffnet den ersten Schritt in natürlicher Namensreihenfolge. Keine auswählbare Szenario-Grundstellung; letzter Szenarioschritt ist vor Löschen geschützt. Leere Altszenarien bekommen Schritt 1, bestehende Vorgaben bleiben vererbbar, alte Einzel-Schritte bleiben ohne automatische Zuordnung. Eine solche Engineering-Ergänzung widerruft die Freigabe.
- Sensor-Prozessanschluss: Ausgeblendet/Unten/Links/Rechts/Oben, Vorgabe Unten. Beim Einfügen grafische Linie erzeugen und passendes Rohr in Anschlussrichtung fangen. Blauer Endgriff bindet an Segment/Anteil eines Rohrpfads; Nachführung bei Sensor-/Rohränderungen, geometrieschonendes Lösen beim Rohrlöschen. Keine Mediennetz-Kante.
- Sensoralarme wie Referenz: LL unten rot, L unten gelb, H oben gelb, HH oben rot. HH/LL haben je Halbkreis Vorrang. Aktive Grenzwerte mit Kennung/Einheit unter dem Sensor; ohne aktiven Alarm kein Zusatztext. Native Darstellung und SVG-Viewer verwenden gleiche Auswertung.
- SHIFT-Größenänderung korrigiert: absolute Mausposition relativ zum Ziehbeginn statt aufsummierter Thumb-Differenzen. Stetige Projektion auf das ursprüngliche Seitenverhältnis, Begrenzung am Blattrand und größerer Griff. Keine Größen-Rückkopplung durch mitbewegten Griff.
- Neue Bibliothek Zeichnen: Linie, Kreis/Ellipse, Rechteck, Textfeld, Pfeil, Doppelpfeil. Aufziehen mit linker Maustaste, SHIFT hält gleiche Seiten bei Rechteck/Ellipse auch an Grenzen; Doppelklick bearbeitet Farbe, Stärke, Füllung und Text. Reine Anzeige ohne Betriebsmittel-, Zustands- oder Alarmzuordnung sowie ohne Rohr-/Wirklinienports. Auswahl, Kopieren, Gruppieren, Größenänderung und Exporte unterstützt.
- Hilfe auf diese Bedienung aktualisiert: 19 Kapitel, vierzehn Bilder. Demo mit grafischen Überschriften und Sensor-Prozessanschlüssen aktualisiert; alle 29 Rohrpfade bleiben rechtwinklig.
- Release-Build: 0 Warnungen/0 Fehler. Bildschirmfreier Modelltest: **314 PASS, kein FAIL**; einschließlich Migration/Schrittpflicht, Sensorfarben, Bindung/Nachführung/Lösen, JSON-Roundtrip, stabile SHIFT-Skalierung und reine Zeichenobjekte. Node-Viewer prüft Sensorwerte, Anschlussgeometrie und Zeichenpfade gegen C# sowie tatsächliche SVG-Ausgaben.
- Gespeicherte Alarm-, Zeichnungs- und Demobilder betrachtet. Am 20.10.2026 Computer Use erfolgreich neu initialisiert und ausgelieferte EXE 0.4.4 bedient: Rechteck aufziehen, am Eckgriff vergrößern, verschieben, per Doppelklick Grafikdialog öffnen und Fülloption übernehmen. Zeichenobjekt erscheint nicht als Betriebsmittel. Demo über Hilfe geöffnet und eingepasst; Szenariowechsel wählt ersten Schritt und aktualisiert Plan. Sensor-Endgriff auf Rohrsegment gezogen und Sensor verschoben: Endpunkt bleibt am Rohr. LL unten rot, H oben gelb, HH oben rot und Alarmtext-Rücknahme bei Normalwert im laufenden Plan beobachtet. Details: `test-results/Bedienpruefung-0.4.4.md`.
- Nutzerabnahme am 20.10.2026: „SHIFT+Ziehen funktioniert wie angenommen“. Damit ist die kombinierte SHIFT-Ziehgeste durch den Nutzer bestätigt. Die automatisierte Mausprüfung umfasste normale Eckgriff-Skalierung; die Computer-Use-API bietet keinen gehaltenen Tastenzustand für `drag`. Keine vollständige Bedienabnahme aller Funktionen beansprucht.
- Auslieferung am 20.10.2026 nach `bin/0.4.4`; frühere Ausgaben erhalten. Publish erfolgreich mit NuGet-Hinweis NU1900: Sicherheitsrisikodaten der Paketquelle waren nicht erreichbar. Der vorherige Release-Quellbuild hatte keine Compilerwarnungen. Für die Windows-PDF-Rasterisierung benötigt der bildschirmfreie Modelltest Zugriff außerhalb der Sandbox.

## Bisheriger Stand – 0.4.3

- Szenarioeditor mit Option **Szenario- und Schrittauswahl in aktueller Ansicht anzeigen**. Standardmäßig aktiv; synchronisiert beide oberen Plan-Dropdowns während der Vorschau, einschließlich neu angelegter Szenarien/Schritte und Szenario-Grundzustand.
- Dropdowns werden gemeinsam bei unterdrückten Zwischenereignissen aktualisiert. Übernehmen bindet die Auswahl an die bestätigten Projektobjekte; Ausschalten stellt die Auswahl vor dem Öffnen wieder her. Abbrechen stellt Auswahl und Option wieder her und verwirft Änderungen.
- Die Option bleibt je geöffnetem Dokument während der Sitzung erhalten; keine Änderung am Projektformat. Bestehende unmittelbare Planvorschau bleibt erhalten.
- Demo unter Hilfe → Demo öffnen: unabhängige A2-Arbeitskopie mit 37 Symbolen, 29 rechtwinkligen Rohrpfaden, zwei Wirklinien, sieben Szenarien und 26 Schritten. Vier getrennte Anlagenwege; aufgelöste Geometrie auf horizontale/vertikale Segmente und Rohrkreuzungen geprüft. Wasserfüllung, Tankversorgung/Mediumwechsel, Gasförderung, Feststoff, Filter/Ablass, Wärmetauscherkreise und Sensoralarme enthalten. Projekt, PNG-Vorschau, Anleitung und Excel mit 390 Aktorzeilen unter Beispiele.
- Hilfe vollständig auf die neuen Menüs, Medienfilter, Szenario-/Schrittbedienung, Ansichtsoption, Alarmzuordnung, Excel-Export und Demo aktualisiert. Veraltete Angaben zu Legendenfeldern und Behälterversorgung korrigiert. 19 Kapitel und zwölf aktuelle Bilder; HTML-Titel FlowSketch.
- Release-Build: 0 Warnungen/0 Fehler. Bildschirmfreier Modelltest: **289 PASS, kein FAIL**, darunter zehn neue Integrationsprüfungen für Synchronisierung, Ausschalten, Übernehmen, Abbrechen, Grundzustand, neue Schritte und Beibehalten der Dokumentoption. Node-Viewer-Regression erfolgreich.
- Gespeichertes Editorbild mit sichtbarer Option betrachtet und Hilfe aktualisiert. Keine echte Maus-/Tastatur-Bedienabnahme dieser Version.

## Bisheriger Stand – 0.4.2

- Medienfilter korrigiert: feste Dropdowns liefern den ausgewählten Eintrag statt des im SelectionChanged-Ereignis noch alten Anzeigetexts. Typwechsel zeigt ausschließlich passende Medien, bei leerer Typgruppe bleibt kein alter Name stehen. Übernahme prüft Name und Typ gemeinsam; Medien-Typbindung im Projekteditor explizit mit unmittelbarer Rückschreibung.
- Szenarioeditor nach Bildvorlage zweigeteilt: Szenario links, untergeordneter Schritt rechts; jeweils Name, Beschreibung und Anlegen/Löschen. Neue Szenarien erhalten einen ersten Schritt. Szenariogrundzustand bleibt als explizite Auswahl verfügbar. Szenariowechsel verschiebt keinen vorhandenen Schritt.
- Tabs Schritte (Übersicht mit Beschreibung/Voraussetzungen/Aktionen/Übergang), Symbolzustände und Alarme. Doppelclick auf einen Schritt wählt ihn zur Bearbeitung. Betriebsmittelalarme lassen sich bearbeiten und Szenarien zuordnen. Szenario-Löschung erhält vorhandene Schritte ohne automatische Neuzuordnung.
- Zustands-, Text-, Medium- und Rohwertänderungen stoßen direkt die Planvorschau an; Timer entfernt. Änderungen und Alarmzuordnungen bleiben bis Übernehmen in der Arbeitskopie, Abbrechen stellt den Originalplan wieder her. Schrittbeschreibung wird als optionales Feld mitgespeichert.
- Doppelte obere Menüpunkte Betriebsmittel/Szenarien/Schritte/Alarme entfernt, rechte Tabs erhalten. Auswahl in Betriebsmittel selektiert das Symbol mit Rahmen und zeigt bei Bedarf seine Seite.
- Release-Build: 0 Warnungen/0 Fehler. Bildschirmfreier Modelltest: **266 PASS, kein FAIL**; einschließlich vier Mediengruppen, leerer Gruppe, unmittelbarer Vorschau, Szenariowechsel, Schrittanlage, Alarmzuordnung, Abbruchschutz, JSON-Roundtrip und Betriebsmittelauswahl. Node-Viewer-Regression erfolgreich, XLSX weiterhin unabhängig lesbar.
- Gespeicherte Layoutbilder aller drei Editortabs betrachtet; zu schmale Schrittspalten korrigiert und erneut geprüft. Hilfe inklusive Szenarioeditorbild aktualisiert. Keine echte Maus-/Tastatur-Bedienabnahme dieser Version; Modell-/Layoutnachweise sind davon getrennt.

## Bisheriger Stand – 0.4.1

- Datei → Exportieren... → Aktoren als Excel-Tabelle...: echte .xlsx-Arbeitsmappe mit Szenario, Schritt, Aktor und Zustand. Keine Medienangaben, keine Excel-Installation nötig; Export arbeitet projektweit unabhängig von PDF, Seite und Druckrahmen und verändert weder Vorgaben noch Freigabe.
- Natürliche aufsteigende Sortierung nach Szenarioname, zugehörigem Schrittnamen und Aktortag (2 vor 10). Je Schritt alle betätigten Ventile inklusive Handantrieb, Pumpen/Motoren, Kompressoren und Ventilatoren; passive Rückschlagarmaturen ausgenommen. Schritt überschreibt Szenario, sonst gleicher Gerätegrundzustand wie im Plan.
- Szenarien ohne Schritte zeigen ihren Grundzustand; Schritte ohne gültige Zuordnung erscheinen separat, ohne stille Zuordnung. Vier Textspalten verhindern Formelausführung bei Namen mit „=“. Filterbare Tabelle, Streifenformat und fixierte Kopfzeile; leeres Projekt liefert Kopfzeile.
- Export schreibt zunächst temporär und ersetzt das Ziel erst nach vollständiger Erstellung. Hilfe und README aktualisiert. Vorherige Versionsordner bleiben erhalten.
- Release-Build: 0 Warnungen/0 Fehler. Bildschirmfreier Modelltest: **246 PASS, kein FAIL**, einschließlich acht Excel-Prüfungen. Unabhängiger openpyxl-Lesetest: Arbeitsmappe, Tabelle, Sortierung, Vererbung, Sonderzeichen und leeres Projekt erfolgreich. Node-Viewer-Regression erfolgreich.
- Keine neue Windows-/Excel-Bedienprüfung; Dateiinhalt und Struktur wurden programmgesteuert geprüft.

## Bisheriger Stand – 0.4.0

- Projektmenü mit Projekteinstellungen, Revision prüfen, Freigeben, Medien und Legende; Dateimenü gemäß gewünschter Reihenfolge, Tab-Schließen mit Speicherabfrage. Mauszoom hält den Punkt unter dem Mauszeiger fest, auch bei zentriertem Blatt. Neue Medienquellen halbiert.
- Projektmedien mit stabilen IDs, Bezeichnung, Typ und Farbe. Standardpalette Leer #808080, Flüssig #0000FF, Fest #FF00FF, Gas #FFAA33, Gemisch separat einstellbar. Übernehmen synchronisiert Quellen, automatische Legenden und abgeleitete Rohrfarben; explizite Rohrfarbe bleibt vorrangig.
- Mediumtyp steht im Gerätedialog vor der gefilterten Medienauswahl. Größen in logischen px, weiterhin normiert gespeichert. Auch gemeinsame Größenangaben verwenden px.
- Legenden verwenden Projektmedien. Quellen werden auf Wunsch beim Übernehmen angelegt, bestehende Quellen pro Medium/Seite nicht dupliziert. Keine Funktions-/Einbauort-/Beschreibungsfelder im Legendendialog.
- Generisches Sensorsymbol: geteilter Kreis, Signalkennung oben und Tagnummer unten. Rohbereich 0–27648, Skalierung 0–100 %, vier aktivierte Grenzwerte HH 99 / H 95 / L 5 / LL 1. Tabellen Skalierung und Alarme sind standardmäßig eingeklappt. Rohwerte je Szenario/Schritt; ohne Vorgabe keine abgeleiteten Alarme. Aktive Alarme werden angezeigt und nicht als manuelle Alarmdatensätze gespeichert.
- Szenario-/Schritt-Editor mit Schrittwahl, Neuanlage und Löschen in einer Arbeitskopie; Planvorschau im Hintergrund. Abbrechen verwirft die Kopie. Die alten separaten Füll-/Druckspalten sind entfernt; Behälterzustand Befüllt/Leer und Medium werden unabhängig zugewiesen.
- Rohrabschnitte speichern jetzt eine abgeleitete Durchflussrichtung. Quelle → Tank → Ausgang wird ausgewertet. Ohne erreichbare Quelle kann ein befüllter Tank jedes vorgegebene Medium liefern. Zwischen zwei Behältern ohne Quelle keine Animation. Zufluss verändert den manuell vorgegebenen Füllzustand nicht automatisch.
- Bestehende PDF-Hashprüfung, Nutzerdateien und Projekt-IDs erhalten. Historische Füll-/Druckwerte bleiben lesbar; neue Behältervorgaben verwenden Befüllt/Leer.
- Release-Build erfolgreich: 0 Warnungen, 0 Fehler. Bildschirmfreier `--model-test`: **238 PASS, kein FAIL**; einschließlich eingeklappter Sensortabellen, transaktionaler Schrittwechsel sowie Schrittanlage/-löschung.
- Node-Viewerprüfung erfolgreich: bestehende 69 Symbol-/Netzwerkfälle sowie 9 neue Richtungs-/Tank-/Sensorfälle stimmen zwischen C# und JavaScript überein. Offline-Hilfe geprüft: 19 Kapitel, Inhaltsverzeichnis, Suche und neun vorhandene Bilder. Gespeichertes Prüfbild `test-results/flowsketch-media-sensor.png` betrachtet; geteilter Sensorkreis, Ventilarme und Medienlegende sichtbar.
- Echte Bedienabnahme von 0.4.0 bleibt offen: Computer-Use-Helfer konnte seine Node-Laufzeit auch nach Neustart nicht starten (Pfad nicht gefunden). Keine Windows-UI-Automation als Ersatz verwendet. Vorherige Bediennachweise gelten ausdrücklich nur für 0.3.2.

## Bisheriger Stand und Nachweise

Stand: 14.10.2026. Aktuelle Ausgabe: **0.3.2**, `bin/0.3.2/PID2Process.exe`. Vorherige Version `bin/0.3.1` erhalten. Vorherige Version `bin/0.3.0` erhalten. Vorherige Ausgabe `bin/Symbols` erhalten. Vorherige Ausgaben `bin/Selection` und `bin/Updated` erhalten. Build, Modelltests und Sichtprüfung getrennt bewertet; die vollständige physische STRG+Drag-Geste bleibt offen. Keine laufenden Nutzerinstanzen automatisch ersetzen oder schließen.

## Aktuelle Sitzung – Version 0.3.2, leeres Standardblatt und Layout

- Normaler Programmstart öffnet sofort ein leeres A4-Zeichenblatt im Querformat. Datei → Neues leeres Blatt legt einen weiteren unabhängigen Tab an. Symbole, Rohr- und Wirklinien arbeiten ohne PDF; Speichern/Öffnen verlangen keine Ersatz-PDF.
- Neues optionales `blankSheet` mit Format, Ausrichtung, Arbeitsflächengröße in mm und Druck-/Exportrahmen. Fehlendes Feld bleibt PDF-Projekt; fehlende PDF-Dateien bleiben zur Zuordnung markiert. Format 2.1 bleibt abwärtslesbar.
- A4/A3/A2 mit Hoch-/Querformat. Formfreie Arbeitsfläche ohne feste Blattgrenze: Pan über einen Rand erweitert die Fläche. Geometrie, Geräte-IDs, Rohr-/Wirklinienbindungen und Rahmen bleiben erhalten. Kein riesiges Ersatz-PDF wird erzeugt.
- Formfrei erfordert einen mit der Maus aufgezogenen Druck-/Exportrahmen. PDF/JPG/Review geben dessen Ausschnitt aus; Export arbeitet mit einem Snapshot, übrige Zeichnung bleibt erhalten. PDF-Blattmaße entsprechen dem Rahmen in mm.
- Blattfreigabe prüft die eigene Dokumentrevision; Engineering-Änderungen widerrufen sie weiterhin. Vorhandene PDF-Hashprüfung bleibt aktiv.
- Vollständige bebilderte Hilfe: 19 Kapitel zu Oberfläche, Blättern/PDFs, Layout/Exportrahmen, Symbolen, Auswahl, Gruppen, Rohr-/Wirklinien, Geräte-/Medienzuständen, Szenarien/Schritten, Alarmen, Freigabe, Export und Problembehebung. WPF-Kapitelauswahl und offline HTML mit Inhaltsverzeichnis/Suche verwenden dieselbe Markdown-Quelle; neun Bilder eingebettet und im Dokumentationsordner.
- Neues Legendensymbol mit editierbarem Titel, Text-/Farbzeilen und gemeinsamen Geräteangaben. Hinzufügen/Entfernen/Zurücksetzen im transaktionalen Dialog. Keine Medienports oder Wirklinienanschlüsse; Kopien besitzen unabhängige Texte; native und SVG-Darstellung mit Pfeilen.
- Vorlage aus den Medienlegenden beider vorhandener PIDs: Luft, Druckluft, Dampf (Wasserdampf), Wasser, Silikonöl, Wasser-Glykol Mischung. Linienfarben aus PDF-Vektordaten übernommen: #939598, #139B48, #FF00FF, #0000FF, #F26722, #00FFFF. Referenz-PDFs unverändert.
- Release-Publish erfolgreich, 0 Warnungen/0 Fehler. Gebündelte EXE: 0.3.2.0.
- Bildschirmfreier `--model-test`: **217 PASS, kein FAIL**; Leerblatt/Layout, physische Formatmaße, formfreie Erweiterung, Anschluss-/Rahmennachführung, Rahmenabbruch, Ausschnitt-JSON/PDF/JPG/Review, transaktionale Legendenbearbeitung und unabhängige Kopien geprüft.
- `node tests/review-scene.test.cjs` erfolgreich: bestehende 69 Netzwerkfälle plus tatsächliche Leerblatt-/Legenden-SVG-Ausgaben. `node tests/help.test.cjs` erfolgreich: 19 Kapitel, Inhaltsverzeichnis/Suche und neun Bildverweise im ausgelieferten HTML.
- Gespeicherte aktuelle Editor-, Rahmen-, Legenden-/Dialogbilder betrachtet; Leerblatt-PDF mit Poppler gerendert und betrachtet. Keine Bildschirmübernahme, sichtbaren Selbsttests oder echten Bediengesten. Echte Bedien-/Druckabnahme bleibt separat offen.

## Echte Bedienprüfung von 0.3.2 am 14.10.2026

Auf ausdrücklichen Nutzerauftrag mit Computer-Use-Skill und separater sichtbarer Testinstanz geprüft. Keine vorhandenen Nutzerprojekte bearbeitet.

- Programmstart: automatisch sichtbares leeres A4-Querformat, Zeichenfläche und Bibliothek sofort verfügbar.
- Blattlayout-Dialog: A4/A3/A2/Formfrei sichtbar; Formfrei ausgewählt und übernommen. Die tatsächlichen Wechsel auf A3/A2 wurden in diesem UI-Lauf nicht ausgeführt.
- Legende aus Bibliothek auf leerem Blatt platziert. Titel und erster Zeilentext über echte Dialogbedienung geändert und übernommen; Änderungen erscheinen korrekt auf dem Blatt.
- Druck-/Exportrahmen mit echter linker Maus-Ziehgeste aufgezogen: 149,568 × 89,741 mm. Orange Umrandung korrekt dargestellt.
- R aktiviert das Rohrwerkzeug ohne PDF. Zwei Segmente mit drei Klickpunkten gezeichnet und per Enter abgeschlossen.
- Bebilderte Hilfe geöffnet; Kapitelwahl inklusive Legendenkapitel und Scrollen geprüft. Logo, Anleitung und eingebettetes Bild sichtbar und lesbar.
- Testprojekt über Windows-Speicherdialog separat unter `test-results/ui-abnahme-0.3.2-20261014.pid2process.json` gespeichert. JSON danach auf Titel, Zeilentext, drei Rohrpunkte und Exportrahmen geprüft.
- PDF-Einzelexport über echte Menü-/Dateidialogbedienung nach `test-results/ui-abnahme-0.3.2-20261014.pdf` ausgeführt. Maße aus PDF: 149,568253 × 89,741022 mm; entsprechen dem Rahmen. Mit Poppler gerendert und betrachtet: Legende und Rohrpfad korrekt, Rahmen nicht exportiert.
- Auf diesen Bedienwegen kein Fehler gefunden. Testinstanz nach dem Speichern geschlossen; Testdateien erhalten. Keine Löschaktionen über die UI ausgeführt.
- Offen bleiben echte Kombinationen mit gedrückt gehaltener mittlerer Maustaste beziehungsweise SHIFT/STRG beim Ziehen, vollständige Geräte-/Szenario-Bedienabnahme und ein physischer Druck. Die verfügbare `sky.drag`-API unterstützt keine gehaltenen Modifier oder mittleren Drag; Modellnachweise hierfür bleiben getrennt.

## Vorherige Sitzung – Version 0.3.1, Logo und Programmordner

- Bereitgestelltes `_media/favicon.ico` als Windows-EXE-Ressource und Fenstericon. PNG-Logo in Hilfe und Über uns.
- `companyInfo.txt` vollständig und unverändert eingebettet. Über uns enthält Version, Logo, Kontakt und Firmenangaben; neue Bedienhilfe enthält zusätzlich die eingebettete README. Die Angabe zur Website-Implementierung bleibt eine Website-Angabe, kein Softwareentwicklernachweis.
- Kompakte Windows-x64-Ausgabe: gebündelte EXE im Versionsordner, Dokumentation und Debugsymbole in eigenen Unterordnern. .NET 10 Desktop Runtime bleibt Voraussetzung. Wiederholbar über `scripts/Publish-Release.ps1`. Frühere Ausgaben und Nutzerdateien bleiben erhalten.
- Release-Publish erfolgreich: 0 Warnungen, 0 Fehler. Benötigte Publish-Pakete konnten außerhalb der Sandbox wiederhergestellt werden. EXE-Metadaten: 0.3.1.0, Firma cs-systems GmbH; Icon aus der fertigen EXE extrahiert und mit Vorlage betrachtet.
- Gebündelte EXE selbst bildschirmfrei mit `--model-test` geprüft: **177 PASS, kein FAIL**, Exitcode 0. Neue Prüfungen sichern eingebettete Firmenangaben, Icon, Hilfe/Über-uns-Tabs und aktuelle Bedienhilfe ab.
- Node-Viewer-Tests erfolgreich. Gespeicherte Dialoginhalte auf Layout und Lesbarkeit geprüft; kein Bildschirmzugriff und keine sichtbaren Selbsttests. Echte Dialogbedienung ist damit nicht nachgewiesen.

## Vorherige Sitzung – Version 0.3.0, Zustandsfarben, Bedienung und Wirklinien

- Ventile mit gefüllten Konturen nach Nutzerbildern: Offen grün (#00FF00), Geschlossen grau (#808080), Störung rot (#FF0000). Dreiwegearmaturen färben A/B/C entsprechend der gewählten Verbindung einzeln. Antriebe übernehmen den Zustand; Ein/Aus-Geräte verwenden dieselben Farben. Editor, Export und SVG-Viewer verwenden dieselben Formteile.
- Große Zustandstexte entfernt. Standardmäßig ausgeblendet; über „Zustandstext anzeigen“ in den Eigenschaften klein zuschaltbar. Zustand bleibt in Detailinformationen verfügbar.
- Regelventil aus der Platzierungsbibliothek entfernt; Altdaten weiterhin lesbar.
- SHIFT bei Bewegung beschränkt auf eine waagrechte oder senkrechte Achse. SHIFT beim Skalieren erhält das ursprüngliche Seitenverhältnis. STRG-Ziehen kopiert Symbole samt unabhängigen Szenario-/Schrittzuweisungen und neuem Tag; Rohrlinien behalten ihren Abzweigmechanismus.
- Neue Wirklinie/Regellinie: dünne grafische Linie mit Klammern, mehrteiliger Entwurf, Auswahl/Gruppen/Eigenschaften/Löschen, Darstellung im Editor sowie PDF/JPG und Review-Viewer. Getrennte Datenliste `signalLines`, keine Wirkung im Mediengraph und keine Steuerlogik.
- Wirklinienanschlüsse liegen an beliebigen Stellen der Symbolumrandung. Eigene Bindung über Equipment-ID, Rechteckseite und relativen Anteil; folgt Bewegung/Skalierung. Keine Bindung an Rohrports oder Rohrleitungen. Beim Entfernen eines Symbols bleibt der geometrische Endpunkt erhalten.
- Versionsnummer in Projektdatei, EXE-Metadaten, Fenstertitel und Über uns. Release-Ausgabe unter `bin/0.3.0`; neue Builds erhalten künftig eine neue Versionsnummer. Nutzerinstanzen und ältere Ausgaben erhalten.

Nachweise und Grenzen:

- Release-Build: 0 Fehler, 1 NU1900-Warnung wegen nicht erreichbarer NuGet-Auditinformationen. Vorhandene Abhängigkeiten konnten verwendet werden; auch der erneute Restore konnte die Online-Auditinformationen nicht abrufen.
- Abschließender bildschirmfreier `--model-test`: **170 PASS, kein FAIL**, Exitcode 0. Einschließlich Umrandungsbindung mit relativem Anteil, Nachführung bei Skalierung, getrennten Wirklinien-/Rohrdaten, JSON-Roundtrip, Kopien/Zuweisungen und bestehenden Modell-/Exportprüfungen.
- `node tests/review-scene.test.cjs`: erfolgreich. 69 C#/Viewer-Netzwerkfälle, 19 exportierte Symbole/Antriebe, Anschlussfüllfarben und optionale Texte, C#/JS-Wirkliniengeometrie und tatsächliches exportiertes SVG geprüft.
- Gespeicherte `test-results/valve-colors.png` und `signal-line-poppler.png` betrachtet. Zweites Bild aus PDF mit Poppler gerendert; Bindung endet sichtbar auf dem Rechteckrand abseits der Rohranschlüsse. Original-PDF-Hash bleibt unverändert.
- Auf aktuellen ausdrücklichen Wunsch **keine Bildschirmübernahme**, keine sichtbaren Selbsttests und keine echten Maus-/Tastatureingaben. Die Bedienabnahme für SHIFT/STRG und Wirklinien bleibt separat offen; direkte Funktionen in nicht angezeigter WPF-Instanz und reine Geometrieprüfungen sind kein physischer Bediennachweis.

## Vorherige Sitzung – Symbole, Rohrzeichnung und Medienweitergabe

Umgesetzt:

- „Rohr einfügen“ in den Kontextmenüs der Planfläche, Symbole, Rohre und Mehrfachauswahl; R bleibt zusätzlich und wird bei Texteingaben abgefangen.
- STRG-Abzweig: Loslassen übernimmt den ersten Eckpunkt und lässt den Entwurf aktiv. Weitere Punkte, Anschlussfang, Doppelklick/Enter und Esc verwenden dieselben Funktionen wie das reguläre Rohrwerkzeug. Elternbindung bleibt über Segment und Anteil bestehen.
- Gemeinsame `SymbolDefinitions.json` als eingebettete Ressource für WPF, PDF/JPG und SVG-Viewer. Gezeichnete Konturen nach Nutzerbildern: Behälter, beide Wärmetauscher, Filter, Flüssigkeitspumpe, Kompressor/Vakuumpumpe, Ventilator, gerade/Eck-/Dreiwege-Absperr- und Kugelarmaturen, Rückschlagarmatur/-klappe. Antriebe handbetätigt, Elektromotor, Kolben und Membran mittig oben in Zustandsfarbe.
- Typbezogene Anschlüsse: Behälter acht, Wärmetauscher vier mit strikt getrennten Kreisen, Filter oben/unten, Fördereinrichtungen links/rechts, Eckarmaturen links/unten, Dreiwege A links/B rechts/C unten. Offen verbindet alle drei; A-B/A-C/B-C nur das jeweilige Paar.
- Medienverfügbarkeit und Durchfluss getrennt. Versorgte Sackleitungen bleiben statisch; nur einfache Wege zu freien Enden oder fremden Behältern animieren. Elternrohr-Enden mit angebundenem Abzweig gelten nicht mehr als freie Enden. Rohrpfade werden an Abzweigstellen in Auswertungsabschnitte unterteilt; Kreise ohne Ziel bleiben statisch. Animierte Abschnitte folgen Geometrieänderungen sofort und bleiben unter den Symbolen.
- Filterzustand Durchlässig/Verstopft; Rückschlagrichtung ab Kugelseite; Pumpen ausschließlich Flüssigkeit, Kompressoren/Ventilatoren ausschließlich Gas. Fördereinrichtungen arbeiten links nach rechts; AUS sperrt, EIN ohne geeignetes Eingangsmedium zeigt einen abgeleiteten Fehler.
- Behälter besitzen Medienangaben und getrennte Vorgaben Befüllt/Unter Druck. Szenario und Schritt können beide Werte unabhängig setzen beziehungsweise erben. Behälter liefern gemäß Vorgabe Medium und dienen auch als Zuflussziel; Zufluss verändert keine Vorgabe automatisch. Eigener Behälter zählt nicht als eigenes Ziel.
- Format 2.1; 2.0 bleibt lesbar. Manuelle/elektrische Altantriebe zugeordnet; unklare Altantriebe bleiben als Auswahl markiert. Unpassende Anschlüsse geometrisch erhalten, orange markiert und fachlich gesperrt. Ungeklärte Anschlüsse beziehungsweise fehlende Antriebsbauarten verhindern Freigabe.
- Gerätedaten erst nach erfolgreicher vollständiger Validierung übernehmen; doppelte Tags erzeugen keine Teiländerungen. Behälterzuweisungen werden beim Entfernen eines Symbols bereinigt. Statische Schritt-Exporte übernehmen auch Behälterwerte.

Nachweise und Grenzen:

- Release-Build des Projekts nach `bin/Symbols`: erfolgreich mit 0 Warnungen und 0 Fehlern.
- Bildschirmfreier `--model-test`: 127 PASS, kein FAIL; neue Symbol-/Netzwerkfälle und bestehende Auswahl-, Zustands-, Daten-, Hash-, PDF/JPG- und Nutzer-PID-Prüfungen. Die Windows-PDF-API benötigt außerhalb der Sandbox Zugriff auf die lokalen Dateien; keine sichtbaren Fenster oder Eingaben in diesem Lauf.
- 69 unabhängige Netzwerkfälle werden als `test-results/symbol-network-cases.json` exportiert und durch den Node-Test vollständig gegen C# verglichen: beide geschlossenen Ventile, offene Hauptwege/gesperrte Äste, Dreiwegezustände mit allen Eingängen, Rückschlagrichtung, Filter, alle Mediumtypen und Förderzustände, Behälterversorgung/Zufluss, getrennte Wärmetauscherkreise, Ringe und Seitenbegrenzung.
- Abzweigfunktionen direkt in einer nicht angezeigten WPF-Instanz geprüft: aktiver Entwurf nach Loslassen, mehrere Punkte, Elternbindung, Anschlussfang und vollständiger Abbruch. Kontextmenüs der Planfläche/Auswahl und gezeichneten Symbole/Rohre geprüft. Diese Tests simulieren keine physische STRG+Drag-Geste.
- Symbolatlas und PDF-Export erzeugt, PDF mit Poppler gerendert und betrachtet. Kugelarmaturen verdecken die inneren Kreuzlinien; Rückschlagpunkte sind gefüllt. Prüflayout korrigiert, damit Kreise rund bleiben und Antriebe/Status/Tags nicht abgeschnitten oder überlagert sind. Artefakte: `test-results/symbols-atlas.png`, `symbols-atlas.pdf`, `symbols-atlas-poppler.png`, `symbols-review/viewer.html`.
- Nach ausdrücklicher neuer Erlaubnis sichtbare Testinstanz mit eigener Test-PDF geöffnet. R und Rohr-Kontextmenü „Rohr einfügen“ samt Aktivierung des Werkzeugs visuell bestätigt. Ein Rohr wurde gezeichnet; ein weiterer Entwurf wurde abgebrochen. Eigene UI-Prüfdatei separat unter `test-results/ui-symbols.pid2process.json` gespeichert und Testinstanz geschlossen. Keine Nutzerdateien überschrieben oder gelöscht.
- Vollständige echte Bedienabnahme aller neuen Geräte-/Szenariodialoge und die physische Kombination STRG+Drag stehen noch aus. Viewer durch DOM-Testgerüst geprüft, nicht als vollständigen Browser-Bedientest darstellen. Keine Anlagen-/Feldfreigabe behaupten.

## Vorherige Sitzung – Mehrfachauswahl und Medienangaben


Nutzerauftrag:

- Manche Rohrleitungen lassen sich nicht mit ENTF entfernen; R als Shortcut zum Rohrzeichnen.
- Auswahlrahmen durch Ziehen: mehrere Objekte löschen, gruppieren und Funktion, Einbauort, Farbe mit Farbrad, Deckkraft, Breite und Höhe gemeinsam bearbeiten.
- Medienquelle: Beschreibung und sichtbare Temperatur, Druck und Durchflussgeschwindigkeit.

Bereits umgesetzt:

- Neue `MainWindow.Selection.cs`: Auswahlrahmen auf freier Planfläche, Auswahlumrisse, Gruppenwahl, Gruppieren/Aufheben, Mehrfachverschieben und Rechtsklickmenü für gemeinsame Eigenschaften. `GroupId` wird an Symbolen und Rohren gespeichert.
- ENTF-Behandlung auf Fenster-Ebene, mit Schutz vor Eingriffen während Texteingaben; R aktiviert das Rohrwerkzeug. Rohre haben zusätzlich eine kontinuierliche unsichtbare 18-Pixel-Klickfläche, damit gestrichelte Lücken leichter auswählbar sind. Die tatsächliche Ursache des Nutzerfalls ist noch nicht durch eine erneute echte Bedienprüfung bestätigt.
- Neue `OverlaySelection.cs`: gemeinsame Auswahl-/Änderungs-/Löschlogik. Mehrfachlöschen löst Anschlüsse an entfernten Objekten/Elternrohren geometrieschonend und bereinigt Szenario-/Schrittzuweisungen. Funktion, Einbauort, Farbe und Deckkraft gelten auch für Rohre.
- Neue `BatchPropertiesEditor.cs`: nur angehakte Eigenschaften werden übernommen; leere Textwerte können bewusst löschen. Breite/Höhe sind normierte Maße je Symbol, nicht Rohrpfadmaße. Diese Einschränkung wurde dem Nutzer während der Arbeit mitgeteilt; Rohrpfade bleiben über ihre Eckpunkte bearbeitbar.
- Neue `ColorPicker.cs`: Hex-Eingabe, Farbvorschau, HSV-Farbrad mit Helligkeitsregler. Eingebaut in gemeinsame Eigenschaften sowie Geräte-Linien-/Hintergrundfarbe. Semantische Symbol-Zustandsfarben bleiben vorrangig; explizite Rohrfarbe überschreibt die Medienfarbe.
- Medienquelle: optionale numerische `TemperatureCelsius`, `PressureBar`, `FlowVelocityMetersPerSecond`; Zahlenvalidierung und Einheiten °C/bar/m/s. Beschreibung und Werte erscheinen im Symbol und im Review-Viewer. Neue Medienquellen bekommen größere Standardmaße. Kein Livewert und keine physikalische Berechnung.
- Native Darstellung und Review-JavaScript berücksichtigen Rohrfarbe/Deckkraft. JSON speichert die neuen Eigenschaften und Gruppenzuordnung.

Nachweise dieser Sitzung:

- Release-Build nach den Änderungen erfolgreich: 0 Warnungen, 0 Fehler, Ausgabe `bin/Selection`.
- `--model-test` erfolgreich, einschließlich neuer `SelectionSelfTest.cs`: Auswahlrahmen mit Symbolen/Rohren, Seitenbegrenzung, Gruppenerweiterung, Batch-Validierung und Änderungen, Gruppenverschiebung, anschlussschonendes Mehrfachlöschen, JSON-Roundtrip, HSV-Farbrad und Medienwerte. Bestehende Modell-/Exporttests und beide Nutzer-Test-PIDs ebenfalls erfolgreich.
- `node tests/review-scene.test.cjs` erfolgreich: bisheriger Editor-/Viewer-Netzwerkvergleich und DOM-Testgerüst. Neue Medienangaben/Rohrstil noch nicht mit zusätzlichen Viewer-Assertions abgesichert.
- Gespeicherte Prüfbilder `test-results/batch-properties.png` und `test-results/color-wheel.png` betrachtet. Das Farbrad-Testbild zeigt den Farbbereich, aber der gewählte Testausschnitt enthält nicht die Dialogbuttons; kein Nachweis für die vollständige Dialogbedienung.
- PDF-Skill und Computer-Use-Skill gelesen; PDF-Authoring-Marker einmal erfolgreich vor den Testexporten ausgeführt. Neue Exporte wurden erzeugt, aber die abschließende PDF-Sichtprüfung für die Medienangaben ist noch offen.
- Keine neue Bildschirmübernahme in dieser Sitzung ausgeführt. Der Nutzer hat die Bildschirmübernahme zwischenzeitlich wieder erlaubt; vor dem geplanten echten UI-Test hat er ausdrücklich für heute gestoppt. Diese damalige Stop-Anweisung wurde durch den Fortsetzungsauftrag vom 14.10.2026 aufgehoben. Es wurde keine neue Auswahl-Testinstanz für die Bildschirmprüfung gestartet.

Nächste Arbeit beim ausdrücklichen Fortsetzen:

1. Diese Datei und betroffene Quellen lesen; den neuesten Zwischenstand `bin/Selection` verwenden, keine älteren Instanzen verwechseln.
2. Tastatur- und Auswahlverhalten prüfen: ENTF auf gestrichelten/verbundenen Rohren und nach Endpunkt-Drag; R auf Planfläche, nicht während Texteingabe; Auswahlrahmen, Kontextmenü, Gruppe und Gruppenverschiebung. UI-Löschaktionen benötigen nach Computer-Use-Skill eine Bestätigung unmittelbar vor dem Löschen; alternativ Löschlogik ausschließlich automatisiert prüfen.
3. Gemeinsame Eigenschaften und Farbrad im echten Dialog prüfen, einschließlich Übernehmen/Abbrechen und vernünftiger Fensterhöhe. Medien-Gerätedialog sowie sichtbare Beschreibung/T/p/v überprüfen.
4. Code-Review: ENTF nicht versehentlich auf alte Canvas-Auswahl anwenden, wenn Fokus in rechter Liste liegt; Gruppenauswahl nach Seiten-/Tabwechsel; Vorschau-/Auswahlhinweise und Mouse-Capture bei Abbruch; Größenänderungen mit Rohranschlüssen. In `EditEquipment` werden Medienzahlen aktuell vor der nachgelagerten Tag-Eindeutigkeitsprüfung zugewiesen: Reihenfolge/Transaktionsschutz überprüfen, damit ungültiger Tag mit anschließendem Abbrechen keine Teiländerung hinterlässt.
5. Review-Tests um Medienwerte und Rohrfarbe/Deckkraft ergänzen, aktuelle PDF-/JPG-Exportbilder ansehen. Danach Build, automatisierte Tests und echte Bedienprüfung getrennt dokumentieren und README/AGENTS anpassen, soweit nötig.

Offene Punkte sind nicht als fertige Bedienabnahme darstellen. Test-PDFs und Nutzeränderungen erhalten.

## Anwendung und Bedienung

- .NET 10 WPF, Windows 10 ab Build 19041. Kompakte obere Menüzeile: Datei einschließlich Export, Projekt, Betriebsmittel, Szenarien, Schritte, Alarme, Hilfe/Über uns. Schmale kategorisierte Symbolbibliothek links, Dokument-Tabs zentral; Hauptfenster startet maximiert.
- Mehrere PDFs werden unabhängig in Tabs bearbeitet. Navigation pro PDF-Seite; neu platzierte Symbole werden automatisch der aktuellen Seite zugeordnet. Kein Planseiten-Eingabedialog.
- Original-PDF wird über die Windows-PDF-API gerendert. Zoom rendert aus der Originaldatei in passender Auflösung neu; logische Seitengröße und Overlay-Koordinaten sind unabhängig von Rasterauflösung. Keine eingebettete PDF-Reader-Oberfläche. Beim Seitenladen ist die Platzierung gesperrt. Ressourcenlimit: 48 Megapixel je Rasterbild.
- Symbolvorschau am Mauszeiger, Gerätedaten-Dialog nach Klick, Drag-and-drop, Resize-Griff, Gerätedaten per Doppelklick. Mittlere Maustaste greift die Ansicht; rechte öffnet Kontextmenü. Kopieren/Einfügen und Löschen vorhanden.
- Armaturen: gerade, Eckform, Dreiwege, Kugel und Rückschlagvarianten mit Antrieben; Filter, Wärmetauscher, Pumpen/Motoren, Kompressoren und Ventilatoren; Temperatur, Druck, Durchfluss, Feuchte, Füllstand, Leitfähigkeit, pH, Drehzahl, Position, generische Messstelle; Behälter/Aggregate.
- Gerätedaten: Tag, Beschreibung, mechanische Bauform/Messgröße, Betätigung, NC/NO/AB/letzte Position, Rückmeldungen mit Tags, DOL/SS/FU sowie Farbe, Deckkraft und normierte Größe. Unpassende Gerätefelder werden ausgeblendet. Dialoge passen sich Inhalt und Bildschirmhöhe an; Verwaltungslisten wachsen mit ihren Einträgen.
- Rechte Seitenleiste mit Betriebsmittel/Szenarien/Schritte/Alarme; Breite per Splitter veränderbar. Menüeinträge wechseln auf diese Tabs. Szenarien enthalten mehrere Schritte, referenziert über `ControlStep.ScenarioId`. Schrittauswahl ist auf das aktuelle Szenario begrenzt. Bearbeiten bewahrt die vorhandene Reihenfolge; alte unzugeordnete Schritte bleiben erhalten.
- Szenario-/Schritteditor: virtualisierte Tabelle, Suche nach Tag/Typ/Funktion/Einbauort, kombinierter Funktionsfilter und Mehrfachauswahl mit Sammelzuweisung. Dialoghöhe wächst bis zu einer Bildschirmgrenze mit der Objektzahl; Filter ändern die Fenstergröße nicht. Änderungen erfolgen auf einer Kopie, Abbrechen verändert das Projekt nicht. Szenarioeditor hat einen zweiten Tab für enthaltene Schritte; deren Änderungen werden erst mit dem äußeren Dialog übernommen.
- Alle Symbole haben Funktion und Einbauort (+). Medienquelle mit frei wählbarem Namen und Gasförmig/Flüssig/Fest/Gemisch-Mehrphasig. Textfelder mit Grundtext, Szenariotext und optionaler Schrittüberschreibung; ausgeblendete Texte werden auch im Export verborgen. Ventile verwenden grüne/graue/rote Füllungen, graue Konturen und optionalen kompakten Zustandstext; Dreiwegearme werden einzeln gefärbt.
- Rohre als mehrteilige normierte Pfade mit transparenter Mausvorschau. Klicken für Eckpunkte, Doppelklick/Enter für Ende, Esc für Abbruch; Symbolanschlüsse werden beim Zeichnen sichtbar und gefangen. STRG+Ziehen von einem vorhandenen Rohr erzeugt einen Abzweig. Anschlüsse speichern Symbol/Port oder Rohr/Segment/Anteil; End-/Eckpunkte lassen sich mit Griffen bewegen, Abzweige folgen. Zyklische Geometrieanschlüsse werden beim Fangen ausgeschlossen. Entfernte Objekte/Elternrohre lösen die Anbindung, ohne die letzte Rohrgeometrie zu verlieren.
- Medienverteilung trennt graphbasierte Verfügbarkeit und Durchfluss: Gas druckbeschlagen, Flüssigkeit befüllt, Feststoff Materialführung, mehrere Medien Gemisch. Nur versorgte Wege zu freien Enden oder fremden Behältern animieren; Sackäste bleiben statisch. Gerichtete Geräteverbindungen und getrennte Wärmetauscherkreise siehe aktuelle Sitzung. Unverbundene Rohre behalten manuelle Szenario-/Schrittzustände ohne Animation. Keine hydraulische Simulation, Restdruck- oder Entleerungsberechnung. Verbindungen gelten innerhalb einer PDF-Seite.
- Export unter Datei: Review-Paket (Viewer, JSON, hochauflösende Seitenbilder, Referenz-PDF), Gesamtexport PDF/JPG, Einzelexport PDF/JPG für aktuelle Seite. JPG gesamt erzeugt eine Datei je Seite. PDF übernimmt originale PDF-Seiten und fügt das Overlay hinzu; originale Beschriftungen bleiben erhalten. PDF-Seitenrotation wird berücksichtigt. Aktuell ausgewählter Szenario-/Schrittzustand wird in statische Exporte übernommen.
- Freigabe gebunden an PDF-Hash; Engineering-Änderungen widerrufen Freigabe. Keine SPS-/HMI-Verbindung oder Steuerlogik. Referenz-PDF wird nicht verändert.

## Nachweise

- Release-Build erfolgreich: 0 Warnungen, 0 Fehler. Damals geprüfter Build: `bin/Updated/PID2Process.exe` (separat von der bereits laufenden älteren Version).
- `--self-test` prüft echte zweitseitige PDF, beide Nutzer-Test-PIDs, unabhängige Tabs, Seitennavigation, Geräte-/Grafik-Roundtrip, höhere Renderauflösung beim Zoom und stabile Overlay-Geometrie.
- Sichtbarer WPF-Selbsttest vor dem Wunsch nach Verzicht auf Bildschirmübernahme erfolgreich: vier rechte Tabs, Szenarioübernahme mit mehreren Schritten, 100-Symbol-Filter, transaktionale Dialoge, Rohrvorschau, Pfad und Endpunkt-Griffe. Gerätedialoge für Ventile, Motoren und Sensorik geprüft. Frühere Fensterbilder unter `test-results`.
- Danach ausschließlich bildschirmfreier `--model-test`: kein Show/ShowDialog, keine Vordergrundaktivierung oder Eingabe. Prüft Medienarten, geschlossene Ventile, inaktive Quelle, Anschlussnachführung, Vererbung, Such-/Funktionsfilter mit 100 Symbolen, JSON-Roundtrip, PDF-/JPG-Export und Hashsperre. Auch die zwei Nutzer-Test-PIDs werden aus Originaldateien mit 2000/6000 Pixeln gerendert und nicht verändert. Daten und Prüfbilder werden eigens erzeugt.
- `tests/review-scene.test.cjs`: Netzwerkzustände des exportierten Viewers gegen C#-Ergebnisse für Szenario/offenen/geschlossenen Schritt verglichen. Alle Medienarten und Abzweiggeometrie geprüft; tatsächliches Export-JavaScript in einem DOM-Testgerüst ausgeführt, inklusive Szenario-/Schrittauswahl und Schritttext. Kein vollständiger Browser-Bedientest.
- Echte Mausprüfung mit Wolkenkammer-PID: Platzierungsvorschau, Klick/Gerätedialog, Übernehmen, Drag-and-drop, Resize-Griff, Doppelklick und Rechtsklick ohne Verschieben visuell bestätigt.
- Pan separat als WPF-Integrationstest: Mitteltastenereignis, native Maus-Erfassung, tatsächliche Scrollverschiebung und Loslassen geprüft. Der gehaltene Hardware-Tastenzustand wird im Test simuliert; nicht als durchgeführte physische Mitteltasten-Geste darstellen.
- PDF-/JPG-Gesamt-/Einzelexport geprüft; Referenzhash unverändert. Rotierter Test-PID-Export zusätzlich mit Poppler gerendert und auf richtige Overlayposition und Beschriftungsorientierung visuell geprüft. PDF-Skill für Export-Sichtprüfung verwendet.
- Neue Medien-/Textdarstellung und Wechsel zwischen Szenario und Schritt vor dem Bildschirmverzicht mit echten Klicks visuell geprüft; Rohr-Endpunkt per echter Maus gezogen, Abzweig-Nachführung bestätigt. STRG-Abzweig ist implementiert, aber die physische Kombination STRG+Drag wurde nicht ausgeführt. Nach dem Bildschirmverzicht keine UI-Automation mehr.
- Ergebnisse: `test-results/results.txt` (sichtbarer Lauf), `test-results/headless-results.txt` (abschließender bildschirmfreier Lauf), `test-results/headless-state-editor.png` und `test-results/headless-poppler.png` (Sichtprüfung gespeicherter Bilder). Exporte sind Prüfartefakte und keine freigegebenen Anlagenunterlagen.
- Historischer Stand: Projektformat 2.0. Ab 14.10.2026 Format 2.1; Altformat wird gelesen: alte Rohr-Endkoordinaten dienen als Fallback, fehlende neue Felder bekommen Standardwerte. Keine automatische fachliche Zuordnung alter Schritte.

## Weiterentwicklung

Fachliche Symbolnormung, Benutzerrollen, Mehrbenutzerablage und Freigabeprozesse können anschließend erweitert werden. Keine Feld-/Anlagenvalidierung oder Freigabe der Beispieldaten behaupten.

`AGENTS.md` enthält Arbeitsregeln. Diese Datei ist die aktuelle veränderliche Übergabequelle.
