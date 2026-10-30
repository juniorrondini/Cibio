'use strict';
const $=id=>document.getElementById(id);
let unity=null,busy=false,blobUrls=[],controller=null;
const detail=$('load-detail'),progress=$('progress');
function showError(error){busy=false;controller?.abort();$('loading').hidden=true;$('cover').hidden=true;$('error').hidden=false;$('game').hidden=true;$('state').textContent='CARREGAMENTO INTERROMPIDO';$('error-message').textContent=error.message||'A conexão foi interrompida. Confira sua internet e tente novamente.';console.error('Cibio:',error);}
function supported(){if(!window.WebAssembly||!window.DecompressionStream)throw new Error('Use uma versão atual do Chrome, Edge, Firefox ou Safari em um computador.');const test=document.createElement('canvas');if(!test.getContext('webgl2'))throw new Error('Seu navegador precisa de WebGL 2 ativo para abrir o jogo. Verifique a aceleração gráfica.');}
async function unpack(file,onBytes){
  const buffers=new Array(file.parts.length);let next=0;
  async function worker(){while(next<file.parts.length){const index=next++,part=file.parts[index];const response=await fetch(part.url,{signal:controller.signal,cache:'force-cache'});if(!response.ok)throw new Error('Não foi possível baixar uma parte do jogo. Tente novamente.');const buffer=await response.arrayBuffer();if(buffer.byteLength!==part.size)throw new Error('O download chegou incompleto. Tente novamente.');const hash=Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',buffer)),n=>n.toString(16).padStart(2,'0')).join('');if(hash!==part.sha256)throw new Error('Uma parte do download está corrompida. Recarregue a página.');buffers[index]=buffer;onBytes(buffer.byteLength);}}
  await Promise.all(Array.from({length:Math.min(3,file.parts.length)},worker));
  const stream=new Blob(buffers).stream().pipeThrough(new DecompressionStream('gzip'));const blob=await new Response(stream,{headers:{"Content-Type":file.mime||"application/octet-stream"}}).blob();
  if(blob.size!==file.rawSize)throw new Error('Não foi possível preparar os arquivos do jogo. Tente novamente.');
  const url=URL.createObjectURL(blob);blobUrls.push(url);return url;
}
async function launch(){
  if(busy||unity)return;busy=true;$('error').hidden=true;$('cover').hidden=true;$('loading').hidden=false;$('game').hidden=true;progress.value=0;detail.textContent='Conferindo compatibilidade…';controller=new AbortController();
  try{
    supported();const response=await fetch('game-manifest.json',{cache:'no-cache',signal:controller.signal});if(!response.ok)throw new Error('Os arquivos do jogo não estão disponíveis. Tente novamente em instantes.');const manifest=await response.json();
    const total=manifest.data.compressedSize+manifest.wasm.compressedSize;let received=0;
    const update=bytes=>{received+=bytes;progress.value=received/total*.7;detail.textContent=`Baixando ${(received/1048576).toFixed(1)} de ${(total/1048576).toFixed(1)} MB`;$('state').textContent='PREPARANDO O JOGO';};
    const dataUrl=await unpack(manifest.data,update);detail.textContent='Preparando o cenário…';const codeUrl=await unpack(manifest.wasm,update);$('load-title').textContent='Entrando no Brasil da Vuvuzela…';detail.textContent='Iniciando o Unity. Isso pode levar um momento.';
    if(!window.createUnityInstance)await new Promise((resolve,reject)=>{const script=document.createElement('script');script.src=manifest.loader;script.onload=resolve;script.onerror=()=>reject(new Error('O inicializador do Unity não carregou. Tente novamente.'));document.body.append(script);});
    $('game').hidden=false;
    unity=await createUnityInstance($('game'),{dataUrl,codeUrl,frameworkUrl:manifest.framework,streamingAssetsUrl:'StreamingAssets',companyName:'Cibio',productName:'Cibio - Vila da Centelha',productVersion:'20',matchWebGLToCanvasSize:true,devicePixelRatio:Math.min(window.devicePixelRatio||1,1.5),showBanner:(message,type)=>{if(type==='error')console.error(message);}},p=>{progress.value=.7+p*.3;});
    $('loading').hidden=true;document.querySelector('.game-shell').classList.add('is-playing');$('fullscreen').disabled=false;$('state').textContent='JOGANDO · PROGRESSO NESTE NAVEGADOR';busy=false;$('game').focus();for(const url of blobUrls)URL.revokeObjectURL(url);blobUrls=[];
  }catch(error){for(const url of blobUrls)URL.revokeObjectURL(url);blobUrls=[];showError(error);}
}
$('play').addEventListener('click',launch);$('retry').addEventListener('click',()=>location.reload());
$('fullscreen').addEventListener('click',async()=>{try{await document.querySelector('.game-shell').requestFullscreen();$('game').focus();}catch{detail.textContent='O navegador não permitiu tela cheia.';}});
$('game').addEventListener('contextmenu',e=>e.preventDefault());
// Keyboard input belongs to the game only while its canvas has focus.
$('game').addEventListener('keydown',e=>{if(['ArrowUp','ArrowDown','ArrowLeft','ArrowRight',' '].includes(e.key))e.preventDefault();});
