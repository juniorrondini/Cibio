from pathlib import Path
from PIL import Image
import json,hashlib
S=Path(__file__).parent;A=S.parent.parent/'Resources/RpgArt' if S.parent.name=='ThirdParty' else S/'Final';A.mkdir(parents=True,exist_ok=True)
audit=[]
for job in json.loads((S/'assets18.json').read_text(encoding='utf-8-sig')):
 im=Image.open(S/job['path']).convert('RGBA');bbox=im.getchannel('A').point(lambda a:255 if a>40 else 0).getbbox();assert bbox
 tile=im.crop(bbox);tile.thumbnail((448,448),Image.Resampling.LANCZOS);out=Image.new('RGBA',(512,512));out.alpha_composite(tile,((512-tile.width)//2,480-tile.height));p=A/(job['key']+'.png');out.save(p)
 audit.append(dict(name=p.name,size=out.size,ppu=192,padding_min=32,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
(S/'Import-Audit18.json').write_text(json.dumps(audit,indent=2),encoding='utf-8');print('Imported3 investigation props')
