"""Reproducible, PDF-free FlowSketch example. Coordinates below are millimetres."""
import json
import uuid
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
WIDTH, HEIGHT = 594, 420
DEFS = json.loads((ROOT / 'src/PID2Process/SymbolDefinitions.json').read_text(encoding='utf-8-sig'))

def ident(name):
    return str(uuid.uuid5(uuid.NAMESPACE_URL, 'flowsketch/demo/' + name))

p = dict(formatVersion='2.1', name='FlowSketch – Prozess-Demo',
         blankSheet=dict(format='A2', landscape=True, widthMm=WIDTH, heightMm=HEIGHT),
         pdf=dict(filePath='', fileName='', pageCount=1, pageAspectRatios=[HEIGHT / WIDTH]),
         media=[], equipment=[], pipes=[], signalLines=[], scenarios=[], controlSteps=[], alarms=[])
media = {}
for name, kind, color in [('Wasser', 'Liquid', '#0000FF'), ('Druckluft', 'Gas', '#FFAA33'),
                          ('Granulat', 'Solid', '#FF00FF'), ('Emulsion', 'Mixed', '#8000FF')]:
    media[name] = dict(id=ident(name), name=name, type=kind, color=color)
    p['media'].append(media[name])
items = {}

def equipment(tag, kind, x, cy, w=18, h=18, variant='', medium='Wasser', function='Demo'):
    definition = DEFS.get(kind + '|' + variant, DEFS.get(kind, {}))
    anchor = definition.get('ports', {}).get('Left1', definition.get('ports', {}).get('Left', [0, .5]))[1]
    if kind == 'HeatExchanger':
        anchor = .5
    item = dict(id=ident(tag), tag=tag, type=kind, variant=variant, page=1,
                x=x / WIDTH, y=(cy - anchor * h) / HEIGHT, width=w / WIDTH, height=h / HEIGHT,
                function=function, installationLocation='+DEMO', description=tag,
                actuator='Manual', failPosition='NC', color='#007E87', backgroundColor='#FFFFFF', opacity=1,
                mediumId=media[medium]['id'], mediaName=medium, mediumType=media[medium]['type'])
    if kind == 'IsolationValve':
        item.update(backgroundColor='#FFE4A8', actuator='Unspecified' if variant.startswith('Rückschlag') else 'ElectricMotor')
    if kind == 'MediaSource':
        item.update(color=media[medium]['color'])
    p['equipment'].append(item)
    items[tag] = item
    return tag

def port(tag, name):
    e = items[tag]
    d = DEFS.get(e['type'] + '|' + e['variant'], DEFS.get(e['type'], {}))
    a, b = d['ports'][name]
    return ((e['x'] + a * e['width']) * WIDTH, (e['y'] + b * e['height']) * HEIGHT)

def pipe(start, end=None, via=(), target=None):
    a, ap = start
    points = [port(a, ap)] + list(via)
    if end:
        b, bp = end
        points.append(port(b, bp))
    else:
        points.append(target)
    points = [q for i, q in enumerate(points) if i == 0 or q != points[i-1]]
    for q, r in zip(points, points[1:]):
        assert abs(q[0] - r[0]) < 1e-8 or abs(q[1] - r[1]) < 1e-8, (start, end, q, r)
    n = len(p['pipes']) + 1
    line = dict(id=ident('Rohr' + str(n)), tag=f'L-{n:03}', page=1,
                points=[dict(x=x/WIDTH, y=y/HEIGHT) for x, y in points],
                startConnection=dict(equipmentId=items[a]['id'], port=ap))
    if end:
        line['endConnection'] = dict(equipmentId=items[b]['id'], port=bp)
    p['pipes'].append(line)

def text(tag, content, x, y, w, h):
    equipment(tag, 'Text' if tag=='DEMO-STATUS' else 'Drawing', x, y+h/2, w, h, variant='Textfeld')
    items[tag].update(textContent=content, color='#203544', drawing=dict(fontSize=28 if tag=='DEMO-TITEL' else 18))

text('DEMO-TITEL', 'FlowSketch – Prozess-Demo', 15, 5, 360, 14)
text('DEMO-STATUS', 'Szenario und Schritt wählen', 390, 87, 175, 22)
for label, y in [('1  Wasser: Pumpe, Filter, Dreiwegeventil, Behälter', 40),
                 ('2  Druckluft: Verdichter, Rückschlag, Behälter, Ventilator', 130),
                 ('3  Feststoff: Filter und Behälter', 215),
                 ('4  Wärmetauscher: unabhängiger Primär- und Sekundärkreis', 290)]:
    text('TITEL-' + str(y), label, 170 if y==40 else 15, y, 380 if y != 40 else 240, 7)
text('DEMO-HINWEIS', 'Rechtwinklige Rohre · keine Anlagensteuerung · Zustände sind Vorgaben, keine Simulation', 15, 400, 560, 10)

# Water lane. Filter connections are vertical; deliberate doglegs avoid diagonal shortcuts.
equipment('Q-WASSER', 'MediaSource', 15, 70, 32, 12, medium='Wasser', function='Wasser')
equipment('XV-101', 'IsolationValve', 72, 70, function='Wasser')
equipment('P-101', 'PumpMotor', 115, 70, variant='Pumpe', function='Wasser')
equipment('F-101', 'Filter', 170, 70, 14, 20, function='Wasser')
equipment('XV-102', 'IsolationValve', 230, 70, 20, 20, variant='3-Wege', function='Wasser')
equipment('B-101', 'Vessel', 355, 70, 28, 38, function='Wasser')
equipment('XV-103', 'IsolationValve', 455, 70, function='Wasser')
equipment('XV-104', 'IsolationValve', 270, 102, variant='Kugelarmatur', function='Wasser / Ablass')
pipe(('Q-WASSER', 'Right'), ('XV-101', 'Left')); pipe(('XV-101', 'Right'), ('P-101', 'Left'))
pipe(('P-101', 'Right'), ('F-101', 'Top'), [(150, 70), (150, 50), (177, 50)])
pipe(('F-101', 'Bottom'), ('XV-102', 'Left'), [(177, 90), (208, 90), (208, 70)])
equipment('REF-101', 'Generic', 285, 70, 4, 4, variant='Querverweis', function='Wasser / Querverweis')
equipment('REF-102', 'Generic', 320, 70, 4, 4, variant='Querverweis', function='Wasser / Querverweis')
items['REF-101']['referenceId']=items['REF-102']['referenceId']='QV000001'
p['referenceNumber']=1
pipe(('XV-102', 'Right'), ('REF-101', 'Left'))
pipe(('REF-102', 'Right'), ('B-101', 'Left1'))
pipe(('REF-101', 'Bottom'), ('REF-102', 'Bottom'), [(287,85),(322,85)])
p['pipes'][-1]['tag']='L-REF-SCHLEIFE'
text('REF-HINWEIS', 'QV000001: gleicher Medienknoten. Zusatzrohr unten: Schleife, kein eigener Durchfluss.', 255, 24, 155, 12)
pipe(('B-101', 'Right1'), ('XV-103', 'Left'))
pipe(('XV-103', 'Right'), target=(545, 70))
pipe(('XV-102', 'Bottom'), ('XV-104', 'Left'), [(240, 102)])
pipe(('XV-104', 'Right'), target=(330, 102))

# Gas lane, check valve ball on inlet side.
for args in [('Q-LUFT','MediaSource',15,32,12,''), ('K-201','Compressor',75,18,18,''),
             ('XV-201','IsolationValve',125,18,18,''), ('RV-201','IsolationValve',180,18,18,'Rückschlagarmatur'),
             ('B-201','Vessel',260,28,38,''), ('V-201','Fan',350,18,18,''), ('XV-202','IsolationValve',435,18,18,'')]:
    tag, kind, x, w, h, variant = args
    equipment(tag, kind, x, 160, w, h, variant, 'Druckluft', 'Druckluft')
chain = ['Q-LUFT','K-201','XV-201','RV-201','B-201','V-201','XV-202']
for a, b in zip(chain, chain[1:]):
    pipe((a, 'Right1' if a == 'B-201' else 'Right'), (b, 'Left1' if b == 'B-201' else 'Left'))
pipe(('XV-202','Right'), target=(545,160))

# Solid material lane, with a separate vertical filter.
equipment('Q-GRANULAT','MediaSource',15,245,32,12,medium='Granulat',function='Feststoff')
equipment('XV-301','IsolationValve',75,245,medium='Granulat',function='Feststoff')
equipment('F-301','Filter',145,245,14,20,medium='Granulat',function='Feststoff')
equipment('B-301','Vessel',280,245,28,38,medium='Granulat',function='Feststoff')
equipment('XV-302','IsolationValve',400,245,medium='Granulat',function='Feststoff')
pipe(('Q-GRANULAT','Right'),('XV-301','Left'))
pipe(('XV-301','Right'),('F-301','Top'),[(125,245),(125,225),(152,225)])
pipe(('F-301','Bottom'),('B-301','Left1'),[(152,270),(200,270),(200,245)])
pipe(('B-301','Right1'),('XV-302','Left')); pipe(('XV-302','Right'),target=(545,245))

# Heat exchanger: two independent supplies and two disjoint outlets.
equipment('Q-PRIMAER','MediaSource',15,335,32,12,function='Wärmetauscher')
equipment('P-401','PumpMotor',75,335,variant='Pumpe',function='Wärmetauscher / Primär')
equipment('XV-401','IsolationValve',125,335,function='Wärmetauscher / Primär')
equipment('WT-401','HeatExchanger',190,335,36,36,variant='Kondensator',function='Wärmetauscher')
equipment('XV-402','IsolationValve',260,335,function='Wärmetauscher / Primär')
equipment('Q-SEKUNDAER','MediaSource',95,305,32,12,medium='Druckluft',function='Wärmetauscher / Sekundär')
equipment('XV-403','IsolationValve',260,375,medium='Druckluft',function='Wärmetauscher / Sekundär')
pipe(('Q-PRIMAER','Right'),('P-401','Left')); pipe(('P-401','Right'),('XV-401','Left'))
pipe(('XV-401','Right'),('WT-401','PrimaryIn')); pipe(('WT-401','PrimaryOut'),('XV-402','Left'))
pipe(('XV-402','Right'),target=(545,335))
pipe(('Q-SEKUNDAER','Right'),('WT-401','SecondaryIn'),[(208,305)])
pipe(('WT-401','SecondaryOut'),('XV-403','Left'),[(208,375)])
pipe(('XV-403','Right'),target=(545,375))

# Measurement and graphical control links bind to symbol borders, not pipe ports.
for tag, signal, number, x, cy, target in [('PE-101','PE','101',90,27,'XV-101'), ('TE-401','TE','401',320,305,'XV-402')]:
    equipment(tag,'Instrument',x,cy,12,12,function='Sensorik');e=items[tag];e['color']='#000000'
    e['sensor']=dict(signal=signal,number=number,rawMin=0,rawMax=27648,scaledMin=0,scaledMax=100,unit='%',
                     limits=[dict(name=name,enabled=True,value=value) for name,value in [('High-High',99),('High',95),('Low',5),('Low-Low',1)]])
    candidates=[]
    for line in p['pipes']:
        for i,(a,b) in enumerate(zip(line['points'],line['points'][1:])):
            ax,ay=a['x']*WIDTH,a['y']*HEIGHT;bx,by=b['x']*WIDTH,b['y']*HEIGHT
            if abs(ay-by)<1e-8 and min(ax,bx)<=x+6<=max(ax,bx) and ay>cy+6 and abs(bx-ax)>1e-8:
                candidates.append((ay,line['id'],i,(x+6-ax)/(bx-ax)))
    if candidates:
        y_end,line_id,index,fraction=min(candidates)
        e['sensor'].update(connectionSide='Bottom',connection=dict(pipeId=line_id,segment=index,fraction=fraction),connectionEnd=dict(x=(x+6)/WIDTH,y=y_end/HEIGHT))
    target_item=items[target];end=(target_item['x']*WIDTH+target_item['width']*WIDTH/2,target_item['y']*HEIGHT)
    start=(x+6,cy-6);points=[start,(start[0],start[1]-4),(end[0],start[1]-4),end]
    p['signalLines'].append(dict(id=ident('Wirk-'+tag),tag='Wirk-'+tag,page=1,color='#FF0000',
        points=[dict(x=a/WIDTH,y=b/HEIGHT) for a,b in points],
        startConnection=dict(equipmentId=e['id'],port='Top',fraction=.5),
        endConnection=dict(equipmentId=target_item['id'],port='Top',fraction=.5)))

equipment('LEGENDE','Legend',420,35,140,34)
items['LEGENDE'].update(automaticLegend=True,legendTitle='Medien',legendEntries=[dict(mediumId=m['id'],text=m['name'],color=m['color']) for m in media.values()])

base = {}
for tag,e in items.items():
    if e['type']=='MediaSource': base[tag]='Inaktiv'
    elif e['type']=='IsolationValve': base[tag]='Durchlässig' if e['variant'].startswith('Rückschlag') else 'Geschlossen'
    elif e['type'] in ['PumpMotor','Compressor','Fan']: base[tag]='Aus'
    elif e['type']=='Filter': base[tag]='Durchlässig'
    elif e['type']=='Vessel': base[tag]='Leer'

def scenario(number,name,states):
    s=dict(id=ident('S'+str(number)),name=f'{number:02} {name}',description='Eigenständiger Demoablauf – rechts im Szenarioeditor öffnen.',
           equipmentStates={items[k]['id']:v for k,v in (base|states).items()}, textValues={items['DEMO-STATUS']['id']:name},tankStates={},sensorValues={})
    p['scenarios'].append(s)
    return s

def step(s,name,states=None,medium=None,sample=None):
    n=sum(t['scenarioId']==s['id'] for t in p['controlSteps'])+1
    t=dict(id=ident(s['name']+'/'+str(n)),scenarioId=s['id'],name=f'{n:02} {name}',description=name,
           preconditions='Vorherigen Schritt fachlich prüfen; dies ist eine manuelle Zustandsvorgabe.',
           actions=name,transition='Nächsten Schritt im Editor auswählen.',tags=list((states or {}).keys()),
           equipmentStates={items[k]['id']:v for k,v in (states or {}).items()},
           textValues={items['DEMO-STATUS']['id']:s['name']+' / '+name},tankStates={},sensorValues={})
    if medium:
        tag,m=medium;t['tankStates'][items[tag]['id']]=dict(mediumId=media[m]['id'])
    if sample is not None:
        t['sensorValues']={items['PE-101']['id']:sample,items['TE-401']['id']:sample}
    p['controlSteps'].append(t)

s=scenario(1,'Sicherer Stillstand',{})
for name in ['Grundstellung prüfen','Medien und Armaturen prüfen','Bereit für Betrieb']:step(s,name)
s=scenario(2,'Wasserbehälter füllen',{'Q-WASSER':'Aktiv','XV-101':'Offen','P-101':'Ein','XV-102':'A-B'})
step(s,'Einlass öffnen',{'P-101':'Aus'});step(s,'Füllen mit freiem Zufluss');step(s,'Füllung beenden',{'P-101':'Aus','XV-101':'Geschlossen','B-101':'Befüllt'})
s=scenario(3,'Behälter als Versorgung',{'B-101':'Befüllt','XV-103':'Offen'})
step(s,'Auslass vorbereiten',{'XV-103':'Geschlossen'});step(s,'Aus Tank fördern');step(s,'Tank leer',{'B-101':'Leer'});step(s,'Mediumwechsel auf Emulsion',medium=('B-101','Emulsion'))
s=scenario(4,'Druckluft und Ventilator',{'Q-LUFT':'Aktiv','K-201':'Ein','XV-201':'Offen','V-201':'Ein','XV-202':'Offen'})
step(s,'Verdichter vorbereiten',{'K-201':'Aus','V-201':'Aus'});step(s,'Druckluft bereitstellen');step(s,'Behälter versorgt Ventilator',{'Q-LUFT':'Inaktiv','K-201':'Aus','B-201':'Befüllt'});step(s,'Ungeeignetes Medium am Ventilator',{'Q-LUFT':'Inaktiv','K-201':'Aus','B-201':'Befüllt'},medium=('B-201','Wasser'))
s=scenario(5,'Filter und Ablass',{'Q-WASSER':'Aktiv','XV-101':'Offen','P-101':'Ein','XV-102':'A-B','Q-GRANULAT':'Aktiv','XV-301':'Offen'})
step(s,'Filter verstopft',{'F-101':'Verstopft','F-301':'Verstopft'});step(s,'Filter durchlässig');step(s,'Wasser zum Ablass',{'XV-102':'A-C','XV-104':'Offen'});step(s,'Granulat aus Behälter',{'Q-GRANULAT':'Inaktiv','B-301':'Befüllt','XV-302':'Offen'})
s=scenario(6,'Wärmetauscherkreise',{'Q-PRIMAER':'Aktiv','P-401':'Ein','XV-401':'Offen','XV-402':'Offen','Q-SEKUNDAER':'Aktiv','XV-403':'Offen'})
step(s,'Nur Primärkreis',{'Q-SEKUNDAER':'Inaktiv'});step(s,'Beide Kreise unabhängig');step(s,'Nur Sekundärkreis',{'Q-PRIMAER':'Inaktiv','P-401':'Aus'});step(s,'Beide Kreise aus',{'Q-PRIMAER':'Inaktiv','P-401':'Aus','Q-SEKUNDAER':'Inaktiv'})
s=scenario(7,'Sensorwerte und Alarme',{})
for name,raw in [('Low und Low-Low',0),('Normalwert',13824),('High',26542),('High und High-High',27648)]:step(s,name,sample=raw)
for tag,name,trigger in [('P-101','Trockenlauf prüfen','Pumpe Ein ohne Eingangsmedium'),('F-101','Filterwartung','Filter Verstopft'),('V-201','Medium ungeeignet','Ventilator mit Flüssigkeit statt Gas')]:
    p['alarms'].append(dict(id=ident('Alarm-'+tag),tag=tag,name=name,priority='Warning',trigger=trigger,impact='Förderweg unterbrochen',operatorAction='Versorgung und Schrittvorgaben prüfen.',scenarioIds=[p['scenarios'][4 if tag=='F-101' else 3 if tag=='V-201' else 1]['id']]))

if '--stdout' in sys.argv:
    print(json.dumps(p,ensure_ascii=False,indent=2))
else:
    out=ROOT/'examples';out.mkdir(exist_ok=True)
    (out/'FlowSketch-Demo.pid2process.json').write_text(json.dumps(p,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f"Demo: {len(p['equipment'])} Symbole, {len(p['pipes'])} rechtwinklige Rohre, {len(p['scenarios'])} Szenarien, {len(p['controlSteps'])} Schritte")
