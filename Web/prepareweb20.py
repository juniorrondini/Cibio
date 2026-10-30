from pathlib import Path
import gzip,hashlib,json,shutil,argparse
W=Path(__file__).parent
a=argparse.ArgumentParser();a.add_argument('--build-root',type=Path,default=W.parent/'outputs/Cibio_Web_20');a.add_argument('--site-root',type=Path,default=W.parent/'sites/cibio-brasil/dist');a.add_argument('--build-log',type=Path,default=W/'buildweb20fresh.log');a.add_argument('--audit',type=Path,default=W/'Web20-Audit.json');args=a.parse_args()
O=args.build_root;D=args.site_root;B=D/'Build';B.mkdir(exist_ok=True)
sha=lambda b:hashlib.sha256(b).hexdigest()
log=args.build_log.read_text(encoding='utf-8-sig');assert 'CIBIO_20_WEB_BUILD_OK' in log and 'CIBIO_20_WEB_SOURCE_SETTINGS_RESTORED' in log and 'Exiting batchmode successfully' in log
previous=json.loads((D/'game-manifest.json').read_text()) if (D/'game-manifest.json').exists() else None
manifest={'version':20,'engine':'Unity 6000.6.7f1','transport':'gzip chunks; SHA256 verified; decompressed in browser','textureFormat':'DXT5 (WebGL build only)'}
for kind,suffix,mime in [('data','.data','application/octet-stream'),('wasm','.wasm','application/wasm')]:
    source=next((O/'Build').glob('*'+suffix));raw=source.read_bytes()
    assert raw.startswith(b'UnityWebData1.0') if kind=='data' else raw.startswith(b'\x00asm')
    compressed=gzip.compress(raw,compresslevel=9,mtime=0);entry={'rawSize':len(raw),'compressedSize':len(compressed),'rawSha256':sha(raw),'mime':mime,'parts':[]}
    for index,start in enumerate(range(0,len(compressed),8*1024*1024)):
        chunk=compressed[start:start+8*1024*1024];digest=sha(chunk);name=f'{kind}-{index:03}-{digest[:16]}.bin';(B/name).write_bytes(chunk)
        entry['parts'].append({'url':'Build/'+name,'size':len(chunk),'sha256':digest})
    assert gzip.decompress(b''.join((D/p['url']).read_bytes() for p in entry['parts']))==raw
    manifest[kind]=entry
for kind,suffix in [('loader','.loader.js'),('framework','.framework.js')]:
    source=next((O/'Build').glob('*'+suffix));name=sha(source.read_bytes())[:16]+suffix;shutil.copy2(source,B/name);manifest[kind]='Build/'+name
licenses=D/'StreamingAssets/Cibio-Licenses';licenses.mkdir(parents=True,exist_ok=True)
for source in (O/'StreamingAssets/Cibio-Licenses').rglob('*'):
    if source.is_file() and source.suffix.lower() not in ['.zip','.meta']:
        dest=licenses/source.relative_to(O/'StreamingAssets/Cibio-Licenses');dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,dest)
shutil.copy2(licenses/'CREDITOS.txt',D/'credits.txt')
note='\n\nVERSÃO WEB 20: originais, fontes e arquivos ZIP de licenças permanecem disponíveis no repositório público:\nhttps://github.com/juniorrondini/Cibio/tree/main/Assets/Cibio/RPG/ThirdParty\nhttps://github.com/juniorrondini/Cibio/tree/main/Assets/StreamingAssets/Cibio-Licenses\nA distribuição Web conserva os textos de crédito e licença; os ZIPs completos não são baixados para jogar.\n'
for source in [D/'credits.txt',licenses/'FONTES-E-LICENCAS.txt']:
    s=source.read_text(encoding='utf-8-sig');source.write_text(s+note if 'VERSÃO WEB 20:' not in s else s,encoding='utf-8')
(D/'game-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(D/'_headers').write_text('/Build/*\n  Cache-Control: public, max-age=31536000, immutable\n/game-manifest.json\n  Cache-Control: no-cache\n',encoding='utf-8')
if previous:
    def refs(m):return {m['loader'],m['framework']}|{item['url'] for k in ['data','wasm'] for item in m[k]['parts']}
    for ref in refs(previous)-refs(manifest):
        obsolete=(D/ref).resolve();assert obsolete.is_relative_to(B.resolve())
        if obsolete.exists():obsolete.unlink()
all_files=[p for p in D.rglob('*') if p.is_file()];assert all(p.stat().st_size<25*1024*1024 for p in all_files)
report={'files':len(all_files),'static_total_bytes':sum(p.stat().st_size for p in all_files),'game_download_bytes':sum(manifest[k]['compressedSize'] for k in ['data','wasm']),'all_files_below_25MiB':True,'chunks_reconstruct_original_data_and_wasm':True,'source_archive_included':False,'sources_public_github':True}
args.audit.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
