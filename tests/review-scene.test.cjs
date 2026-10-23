const checkCases=JSON.parse(require('node:fs').readFileSync('test-results/check-valve-cases.json','utf8'));
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const scene=require('../src/PID2Process/ReviewScene.js');
const pipeStyleCases=JSON.parse(fs.readFileSync('test-results/pipe-style-cases.json','utf8'));
for(const c of pipeStyleCases.cases){const actual=scene.pipeAppearance(pipeStyleCases.project,c.pipe,{state:c.state,mediumType:'Liquid',mediumName:'Wasser'});assert.equal(actual.color,c.color);assert.equal(actual.width,c.width);assert.deepEqual(actual.dash,c.dash);assert.equal(actual.scaled,true);}
assert.deepEqual(scene.pipeAppearance({mediaColors:{empty:'#808080'}},{color:'#123456'},{state:'Leer'}),{color:'#123456',width:5,dash:[10,10],scaled:false});
console.log('PASS: Alle neuen Rohrstrichtypen und Zustandsfarben stimmen mit C# überein; Altstil bleibt erhalten');
for(const c of JSON.parse(fs.readFileSync('test-results/drawing-geometry-cases.json','utf8'))){const numbers=path=>(path.match(/-?\d+(?:\.\d+)?/g)||[]).map(Number),actual=numbers(scene.drawingGeometry(c.item,c.width,c.height)),expected=numbers(c.path);assert.equal(actual.length,expected.length);for(let i=0;i<actual.length;i++)assert.ok(Math.abs(actual[i]-expected[i])<.000002,c.item.variant+' geometry '+i);}
console.log('PASS: Richtungsunabhängige Pfeilspitzen und tatsächliche Zeichengeometrie stimmen mit C# überein');
const sensorDrawing=JSON.parse(fs.readFileSync('test-results/sensor-drawing-cases.json','utf8'));
for(const c of sensorDrawing.cases){assert.deepEqual(scene.sensorDisplay(c.item,c.values),c.expected);assert.deepEqual(scene.sensorConnection(c.project,c.item),c.points);}
for(const c of sensorDrawing.connectionCases)assert.deepEqual(scene.sensorConnection(c.project,c.item),c.points);
for(const x of sensorDrawing.drawings.equipment){assert.deepEqual(scene.ports(x),[]);if(sensorDrawing.paths[x.id])assert.equal(scene.drawingPath(x),sensorDrawing.paths[x.id]);}
console.log('PASS: Sensor-Halbkreisalarme, Prozesslinien und Zeichenpfade stimmen zwischen Editor und Viewer überein');
const p=JSON.parse(fs.readFileSync('test-results/process-fixture.pid2process.json','utf8'));
const cases=JSON.parse(fs.readFileSync('test-results/scene-cases.json','utf8'));
for(const c of cases){const actual=scene.evaluate(p,scene.values(p.scenarios.find(s=>s.id===c.scenarioId),p.controlSteps.find(t=>t.id===c.stepId)));assert.deepEqual(actual,c.expected);console.log('PASS: Editor/Viewer-Netzwerkgleichheit '+c.name);}
const source=p.equipment.find(d=>d.type==='MediaSource'),down=p.pipes.find(d=>d.tag==='L-NACH'),branch=p.pipes.find(d=>d.tag==='L-ABZ'),open=p.controlSteps.find(t=>t.name==='Füllen starten'),scenario=p.scenarios.find(s=>s.id===open.scenarioId);
for(const[type,state]of Object.entries({Gas:'Druckbeschlagen',Liquid:'Befüllt',Solid:'Materialführung',Mixed:'Gemisch'})){source.mediumType=type;assert.equal(scene.evaluate(p,scene.values(scenario,open))[branch.id].state,state);}source.mediumType='Gas';
const before=scene.resolve(p,branch)[0].y;down.points.at(-1).y+=.1;assert.ok(Math.abs(scene.resolve(p,branch)[0].y-before-.05)<1e-9);
console.log('PASS: Alle Mediumtypen und beweglicher mittiger Abzweig im Viewer');
// Minimal DOM harness exercises the actual exported script without a web server.
class Element{
 constructor(name){this.name=name;this.children=[];this.style={};this.attrs={};this.value='';this.namespaceURI='http://www.w3.org/2000/svg';this.clientWidth=1200;this.clientHeight=900;this.scrollLeft=0;this.scrollTop=0;}
 addEventListener(name,fn){this['on'+name]=fn;}
 append(x){x.parent=this;this.children.push(x);}add(x){this.append(x);if(this.children.length===1)this.value=String(x.value);}replaceChildren(){this.children=[];this.value='';}setAttribute(k,v){this.attrs[k]=v;}remove(){this.parent.children=this.parent.children.filter(x=>x!==this);}querySelectorAll(){return this.children.filter(x=>x.className==='device'||x.name==='svg');}
}
const elements=Object.fromEntries(['page','scenario','step','stage','background','revision','warning','name','description','details','steps','alarms','viewport','paper-size','zoom-in','zoom-out','zoom-fit','zoom-level'].map(id=>[id,new Element(id)]));
elements.stage.append(elements.background);
const document={getElementById:id=>elements[id],createElement:n=>new Element(n),createElementNS:(_,n)=>new Element(n)};
const html=fs.readFileSync('test-results/review/viewer.html','utf8'),script=html.match(/<script>([\s\S]*?)<\/script>/)[1];
assert.ok(!script.includes('__DATA__')&&!script.includes('__SCENE_SCRIPT__'));
vm.runInNewContext(script,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
const exported=JSON.parse(fs.readFileSync('test-results/review/project-data.json','utf8')),s=exported.scenarios.find(s=>s.name==='Befüllen');
assert.ok(html.includes('id="zoom-in"')&&html.includes('id="paper-size"'));
elements['zoom-in'].onclick();const zoomBeforeScenario=elements.stage.style.transform;
elements.scenario.value=s.id;elements.scenario.onchange();assert.equal(elements.step.children.length,2);
const t=exported.controlSteps.find(t=>t.name==='Füllen starten');elements.step.value=t.id;elements.step.onchange();
assert.ok(elements.stage.children.some(x=>x.textContent==='Befüllung läuft'));
assert.ok(elements.stage.children.some(x=>x.style.background==='#FFDF9A'));
assert.equal(elements.steps.children.length,2);
elements.scenario.value=exported.scenarios.find(s=>s.name==='Regelbetrieb').id;elements.scenario.onchange();assert.equal(elements.step.children.length,1);assert.equal(elements.step.value,exported.controlSteps.find(t=>t.scenarioId===exported.scenarios.find(s=>s.name==='Regelbetrieb').id).id);
assert.equal(elements.stage.style.transform,zoomBeforeScenario);assert.equal(elements['zoom-level'].textContent,'125 %');
elements.page.onchange();assert.equal(elements.stage.style.transform,'scale(1)');
console.log('PASS: Exportierter Viewer behält Zoom bei Szenario-/Schrittwechsel und passt neue Seiten ein');
console.log('PASS: Exportiertes HTML schaltet Szenarien, zugehörige Schritte, Ventilfarben und Schritttexte');
const symbolCases=JSON.parse(fs.readFileSync('test-results/symbol-network-cases.json','utf8'));
for(const c of symbolCases){assert.deepEqual(scene.evaluateScene(c.project,scene.values(c.scenario,null)),c.expected,c.name);}
assert.equal(symbolCases.length,69);
console.log('PASS: '+symbolCases.length+' Symbol-/Netzwerkfälle stimmen zwischen C# und Viewer überein');
const catalog=scene.catalog;
assert.equal(Object.keys(catalog.Vessel.ports).length,8);
assert.equal(Object.keys(catalog['HeatExchanger|M-Form'].ports).length,4);
assert.deepEqual(catalog['HeatExchanger|M-Form'].ports.PrimaryIn,[1,.35]);
assert.deepEqual(catalog['IsolationValve|3-Wege'].ports.Bottom,[.5,1]);
assert.deepEqual(catalog.Filter.ports,{Top:[.5,0],Bottom:[.5,1]});
console.log('PASS: Gemeinsame Symboldefinition mit erwarteten Anschlüssen');
const atlasHtml=fs.readFileSync('test-results/symbols-review/viewer.html','utf8'),atlasScript=atlasHtml.match(/<script>([\s\S]*?)<\/script>/)[1];
const atlas=JSON.parse(fs.readFileSync('test-results/symbols-review/project-data.json','utf8'));
for(const element of Object.values(elements))element.replaceChildren();
elements.stage.append(elements.background);
vm.runInNewContext(atlasScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
elements.scenario.value=atlas.scenarios[0].id;elements.scenario.onchange();
const devices=elements.stage.children.filter(x=>x.className==='device');
assert.equal(devices.length,atlas.equipment.length);
for(let i=0;i<devices.length;i++){const x=atlas.equipment[i],svg=devices[i].children.find(c=>c.name==='svg');assert.ok(svg,x.variant);assert.equal(svg.attrs.viewBox,'0 0 100 100');const expected=scene.definition(x).shapes.length+(scene.valve(x)&&!scene.check(x)?scene.catalog['Actuator|'+x.actuator].shapes.length:0);assert.equal(svg.children.length,expected,x.variant+' / '+x.actuator);}
assert.ok(devices.some(d=>d.children.some(c=>c.name==='svg'&&c.children.some(c=>c.name==='ellipse'&&c.attrs.fill==='#00FF00'))));
console.log('PASS: Alle '+devices.length+' Symbole und Antriebe werden im tatsächlich exportierten Viewer als SVG gezeichnet');
for(const state of ['Offen','Geschlossen','A-B','A-C','B-C','Störung']){const x={type:'IsolationValve',variant:'3-Wege'},active={'Offen':['Left','Right','Bottom'],'Geschlossen':[],'A-B':['Left','Right'],'A-C':['Left','Bottom'],'B-C':['Right','Bottom']}[state]||[];for(const part of ['Left','Right','Bottom'])assert.equal(scene.fillColor(x,{part},state),state==='Störung'?'#FF0000':active.includes(part)?'#00FF00':'#808080');}
assert.ok(devices.filter((d,i)=>scene.colored(atlas.equipment[i])).every(d=>!d.children.some(c=>String(c.className).startsWith('caption'))));
const captionItem=atlas.equipment.find(scene.valve);captionItem.showStateText=true;
const enabledScript=atlasScript.replace(/const PROJECT=[\s\S]*?;\s*const SYMBOLS=/,'const PROJECT='+JSON.stringify(atlas)+';\nconst SYMBOLS=');
for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
vm.runInNewContext(enabledScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
elements.scenario.value=atlas.scenarios[0].id;elements.scenario.onchange();
const enabledDevice=elements.stage.children.filter(x=>x.className==='device')[atlas.equipment.indexOf(captionItem)];
assert.ok(enabledDevice.children.some(c=>c.className==='caption compact'&&c.textContent==='Offen'));
console.log('PASS: Anschlussfarben und standardmäßig ausgeblendete Zustandstexte');
const signalCase=JSON.parse(fs.readFileSync('test-results/signal-scene.json','utf8')),signal=signalCase.project.signalLines[0],signalPoints=scene.signalResolve(signalCase.project,signal);
assert.deepEqual(signalPoints,signalCase.points);
const markers=scene.signalMarkers(signalPoints,1400,1050);
assert.equal(markers.length,signalCase.markers.length);
for(let i=0;i<markers.length;i++)for(let j=0;j<markers[i].length;j++){assert.ok(Math.abs(markers[i][j].x-signalCase.markers[i][j].x)<1e-12);assert.ok(Math.abs(markers[i][j].y-signalCase.markers[i][j].y)<1e-12);}
const withoutSignal={...signalCase.project,signalLines:[]};assert.deepEqual(scene.evaluate(signalCase.project,scene.values(null,null)),scene.evaluate(withoutSignal,scene.values(null,null)));
for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
const signalHtml=fs.readFileSync('test-results/signal-review/viewer.html','utf8'),signalScript=signalHtml.match(/<script>([\s\S]*?)<\/script>/)[1];
vm.runInNewContext(signalScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
const exportedSvg=elements.stage.children.find(x=>x.name==='svg'),signalPaths=exportedSvg.children.filter(x=>x.attrs.class==='signal-line');
assert.equal(signalPaths.length,4);assert.ok(signalPaths.every(x=>x.attrs['stroke-width']===1.3&&!x.attrs['stroke-dasharray']));
console.log('PASS: Wirkliniengeometrie und Klammern wie C#, unabhängig vom Mediennetz, im Export-Viewer dargestellt');
const blankProject=JSON.parse(fs.readFileSync('test-results/blank-review/project-data.json','utf8'));
assert.equal(blankProject.blankSheet.format,'Ausschnitt');
assert.equal(blankProject.blankSheet.widthMm,297);
assert.equal(blankProject.blankSheet.heightMm,210);
assert.equal(blankProject.pdf.filePath,'');
assert.equal(blankProject.pdf.pageCount,1);
for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
const blankScript=fs.readFileSync('test-results/blank-review/viewer.html','utf8').match(/<script>([\s\S]*?)<\/script>/)[1];
vm.runInNewContext(blankScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
assert.equal(elements.background.src,'page-1.png');
assert.equal(elements.stage.children.filter(x=>x.className==='device').length,blankProject.equipment.length);
assert.ok(elements.stage.children.find(x=>x.name==='svg').children.some(x=>x.attrs.class==='signal-line'));
console.log('PASS: Leerblatt-Exportrahmen im tatsächlich exportierten Viewer ohne PDF gerendert');
const legendProject=JSON.parse(fs.readFileSync('test-results/legend-review/project-data.json','utf8'));
for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
const legendScript=fs.readFileSync('test-results/legend-review/viewer.html','utf8').match(/<script>([\s\S]*?)<\/script>/)[1];
vm.runInNewContext(legendScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
const legendDevice=elements.stage.children.find(x=>x.className==='device');
const legendSvg=legendDevice.children.find(x=>x.name==='svg');
assert.deepEqual(legendSvg.children.filter(x=>x.name==='text').map(x=>x.textContent),['Legende',...legendProject.equipment[0].legendEntries.map(x=>x.text)]);
assert.deepEqual(legendSvg.children.filter(x=>x.name==='line').map(x=>x.attrs.stroke),legendProject.equipment[0].legendEntries.map(x=>x.color));
assert.equal(legendSvg.children.filter(x=>x.name==='polyline').length,6);
assert.deepEqual(scene.definition(legendProject.equipment[0]).ports,{});
console.log('PASS: Editierbare PID-Legende mit Texten, Linienfarben und Pfeilen im SVG-Export');
const enhancementCases=JSON.parse(fs.readFileSync('test-results/flowsketch-cases.json','utf8'));
for(const sample of enhancementCases){const values=scene.values(sample.scenario,null);assert.deepEqual(scene.evaluate(sample.project,values),sample.expected,sample.name);assert.equal(scene.sensorAlarms(sample.project,values).length,sample.alarms,sample.name);}
console.log('PASS: '+enhancementCases.length+' neue Richtungs-, Tank- und Sensorfälle stimmen zwischen Editor und Viewer überein');
for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
const drawingHtml=fs.readFileSync('test-results/drawing-review/viewer.html','utf8'),drawingScript=drawingHtml.match(/<script>([\s\S]*?)<\/script>/)[1];
vm.runInNewContext(drawingScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
const annotationDevices=elements.stage.children.filter(x=>x.className==='device');
assert.equal(annotationDevices.length,6);
assert.ok(annotationDevices.some(x=>x.textContent==='Nur Grafik'));
assert.ok(annotationDevices.every(x=>!x.children.some(c=>c.className?.startsWith('tag')||c.className?.startsWith('caption'))));
assert.ok(annotationDevices.some(x=>x.children.some(c=>c.name==='svg'&&c.children.some(n=>n.name==='ellipse'))));
console.log('PASS: Reine Zeichenobjekte im tatsächlich exportierten SVG-Viewer ohne Betriebsmittelbeschriftungen');









for(const test of checkCases){const n=scene.evaluateScene(test.project,test.values),x=test.project.equipment.find(x=>x.id===test.id);assert.equal(n.checkValves[test.id],test.expected);const status=scene.status(x,test.values,n);assert.equal(status,test.expected?'Durchflossen':'Gesperrt');assert.equal(scene.fillColor(x,{part:'State'},status),test.expected?'#00FF00':'#808080');}
console.log('PASS: Gespiegelte Rückschlagarmaturen: Richtung, gesperrte Wege und echte Durchflussfarben stimmen mit C# überein');
for(const c of pipeStyleCases.cases){
 const project=JSON.parse(JSON.stringify(pipeStyleCases.project)),water=project.media.find(m=>m.name==='Wasser');
 const source=project.equipment.find(x=>x.type==='MediaSource');Object.assign(source,{mediumId:water.id,mediaName:water.name,mediumType:water.type});
 const pipe=JSON.parse(JSON.stringify(c.pipe));pipe.startConnection={equipmentId:source.id,port:'Right'};pipe.endConnection=null;
 project.equipment=[source];project.pipes=[pipe];project.signalLines=[];project.alarms=[];project.controlSteps=[];
 project.scenarios=[{id:'pipe-style',name:'Rohrstil',equipmentStates:{[source.id]:c.state==='Leer'?'Inaktiv':'Aktiv'},pipeStates:{},textValues:{},sensorValues:{},tankStates:{}}];
 const pipeScript=script.replace(/const PROJECT=[\s\S]*?;\s*const SYMBOLS=/,'const PROJECT='+JSON.stringify(project)+';\nconst SYMBOLS=');
 for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
 vm.runInNewContext(pipeScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
 elements.scenario.value='pipe-style';elements.scenario.onchange();
 const line=elements.stage.children.find(x=>x.name==='svg').children.find(x=>x.name==='polyline');
 assert.equal(line.attrs.stroke,c.color);assert.equal(line.attrs['stroke-width'],c.width);assert.equal(line.attrs['stroke-dasharray'],c.dash?.join(' '));assert.equal(line.attrs['vector-effect'],undefined);
}
console.log('PASS: Tatsächlich exportiertes SVG verwendet neue Rohrfarben, Breiten und Strichtypen in beiden Zuständen');
for(const shown of [false,true]){
 const project=JSON.parse(JSON.stringify(p)),item=project.equipment.find(x=>x.type==='Instrument');
 Object.assign(item,{page:1,showTag:shown,tagOffsetX:.01,tagOffsetY:.02,backgroundTransparent:true,linesTransparent:true});
 project.equipment=[item];project.pipes=[];project.signalLines=[];project.alarms=[];project.scenarios=[];project.controlSteps=[];
 const displayScript=script.replace(/const PROJECT=[\s\S]*?;\s*const SYMBOLS=/,'const PROJECT='+JSON.stringify(project)+';\nconst SYMBOLS=');
 for(const element of Object.values(elements))element.replaceChildren();elements.stage.append(elements.background);
 vm.runInNewContext(displayScript,{document,Option:class{constructor(text,value){this.text=text;this.value=value;}},console});
 const device=elements.stage.children.find(x=>x.className==='device');assert.equal(device.style.background,'transparent');
 const symbol=device.children.find(x=>x.name==='svg');assert.ok(symbol.children.filter(x=>x.name!=='text').every(x=>x.attrs.stroke==='transparent'));
 const label=device.children.find(x=>x.name==='span'&&x.textContent===item.tag);assert.equal(Boolean(label),shown);
 if(shown){assert.equal(label.style.left,(.01/item.width*100)+'%');assert.ok(label.style.top.includes(String(.02/item.height*100)+'%'));}
}
console.log('PASS: Tatsächlich exportierter Viewer berücksichtigt Taganzeige, normierten Versatz und unabhängige Transparenz');
