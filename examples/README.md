# FlowSketch-Demo

Über **Hilfe → Demo öffnen** wird eine Arbeitskopie auf einem A2-Blatt geöffnet. **Datei → Speichern...** speichert diese unter einem eigenen Dateinamen. Alternativ die Projektdatei über Datei → Öffnen... laden.

Die Demo enthält 43 Symbole, 32 ausschließlich rechtwinklige Rohrpfade, zwei grafische Wirklinien, vier Medien, sieben Szenarien und 26 Schritte. PNG-Vorschau und Excel-Aktorenliste liegen daneben. REF-101, REF-102 und REF-103 verbinden über QV000001 drei Wasserleitungen. REF-201 und REF-202 verwenden QV000002 für ein separates Druckluftpaar.

Rechts im Tab **Abläufe** ein Szenario doppelklicken, dann rechts im Editor einen Schritt wählen. **Szenario- und Schrittauswahl in aktueller Ansicht anzeigen** synchronisiert die obere Werkzeugleiste. Zustände sind manuelle Vorgaben; Schritte laufen nicht automatisch ab.

| Szenario | Gezeigte Funktionen |
| --- | --- |
| 01 Sicherer Stillstand | Grundstellungen und Schrittfolge |
| 02 Wasserbehälter füllen | Versorgte Pumpe, Filter, Dreiwegeventil A-B und Tank als Ziel |
| 03 Behälter als Versorgung | Befüllt/Leer, Ausgang aus dem Tank, Mediumwechsel auf Emulsion |
| 04 Druckluft und Ventilator | Kompressor, Rückschlagrichtung, Tankversorgung, ungeeignetes Medium |
| 05 Filter und Ablass | Verstopft/Durchlässig, A-C-Ablass, Feststoffbehälter |
| 06 Wärmetauscherkreise | Nur Primärkreis, beide Kreise, nur Sekundärkreis, aus |
| 07 Sensorwerte und Alarme | Prozesswerte mit Einheit, Low/Low-Low, Normal, High/High-High |

Zusätzlich: automatische Medienlegende, elektrische Armaturantriebe, zwei frei beschriftete Sensoren, grafische Regelzuordnung, wechselnder Text im Plan und manuelle Betriebsmittelalarme. Der Excel-Export enthält alle Szenarien/Schritte mit Aktorzuständen.

Die Demo erfordert keine PDF und steuert keine reale Anlage. Tankfüllstände ändern sich ausschließlich durch die Schrittvorgabe. Die bereitgestellte Arbeitsmappe ist eine Ausgabe, kein Projektimport.

Die Projektdatei lässt sich im Quellverzeichnis reproduzierbar mit `python scripts/Create-Demo.py` erzeugen. Der bildschirmfreie Modelltest prüft Geometrie und ausgewählte Prozesszustände und erzeugt Vorschau sowie Excel-Datei.

Die separate Kompositions-Demo ist über Datei → PDF-Komposition → Demo laden erreichbar. Sie erzeugt zwei Beispiel-PDFs mit unterschiedlich gedrehten Seiten und zeigt ausgewählte Inhalte ohne Schriftkopf. Anordnung, Mauszeiger-Zoom, Hilfslinien, Export und Übernehmen in den Haupteditor können daran ausprobiert werden.
