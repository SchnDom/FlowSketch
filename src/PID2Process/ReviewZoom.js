// One transformed sheet keeps background, symbols, labels and pipe overlays aligned.
const ReviewZoom = {
  mount(document, pdf) {
    const el=id=>document.getElementById(id), viewport=el('viewport'), stage=el('stage'), paper=el('paper-size');
    let zoom=1, baseWidth=0, pan=null;
    const aspect=()=>pdf.pageAspectRatios?.[Number(el('page').value)-1]||.75;
    const fitWidth=()=>Math.max(1,Math.min(viewport.clientWidth-30,(viewport.clientHeight-30)/aspect()));
    function setZoom(value, x=viewport.clientWidth/2, y=viewport.clientHeight/2) {
      const next=Math.max(.25,Math.min(8,value)), width=fitWidth();
      const ratio=width*next/Math.max(1,baseWidth*zoom);
      const left=(viewport.scrollLeft+x-15)*ratio-x+15, top=(viewport.scrollTop+y-15)*ratio-y+15;
      zoom=next;baseWidth=width;
      stage.style.width=width+'px';stage.style.height=width*aspect()+'px';stage.style.transform='scale('+zoom+')';
      paper.style.width=width*zoom+'px';paper.style.height=width*aspect()*zoom+'px';
      viewport.scrollLeft=Math.max(0,left);viewport.scrollTop=Math.max(0,top);
      el('zoom-level').textContent=Math.round(zoom*100)+' %';
      el('zoom-in').disabled=zoom>=8;el('zoom-out').disabled=zoom<=.25;
    }
    function fit(){setZoom(1);viewport.scrollLeft=viewport.scrollTop=0;}
    el('zoom-in').onclick=()=>setZoom(zoom*1.25);
    el('zoom-out').onclick=()=>setZoom(zoom/1.25);
    el('zoom-fit').onclick=fit;
    viewport.addEventListener('wheel',e=>{
      e.preventDefault();const r=viewport.getBoundingClientRect();
      const delta=e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?viewport.clientHeight:1);
      setZoom(zoom*Math.exp(-Math.max(-120,Math.min(120,delta))*.002),e.clientX-r.left,e.clientY-r.top);
    },{passive:false});
    viewport.onpointerdown=e=>{
      if(e.button!==1&&(e.button!==0||e.target.closest?.('.device')))return;
      e.preventDefault();viewport.focus();pan={id:e.pointerId,x:e.clientX,y:e.clientY,left:viewport.scrollLeft,top:viewport.scrollTop};
      viewport.setPointerCapture(e.pointerId);viewport.classList.add('panning');
    };
    viewport.onpointermove=e=>{if(pan&&e.pointerId===pan.id){viewport.scrollLeft=pan.left+pan.x-e.clientX;viewport.scrollTop=pan.top+pan.y-e.clientY;}};
    function stop(e){if(pan&&e.pointerId===pan.id){pan=null;viewport.classList.remove('panning');if(viewport.hasPointerCapture(e.pointerId))viewport.releasePointerCapture(e.pointerId);}}
    viewport.onpointerup=stop;viewport.onpointercancel=stop;viewport.onlostpointercapture=stop;
    viewport.onkeydown=e=>{if(e.target!==viewport)return;if(['+','=','-','0'].includes(e.key)){e.preventDefault();if(e.key==='0')fit();else setZoom(e.key==='-'?zoom/1.25:zoom*1.25);}};
    if(typeof ResizeObserver!=='undefined')new ResizeObserver(()=>setZoom(zoom)).observe(viewport);
    fit();return {fit};
  }
};
if(typeof module!=='undefined')module.exports=ReviewZoom;
