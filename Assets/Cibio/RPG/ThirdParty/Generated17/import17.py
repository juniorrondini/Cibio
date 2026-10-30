from pathlib import Path
from PIL import Image
import json,hashlib
S=Path(__file__).parent;A=S.parent.parent/'Resources/RpgArt' if S.parent.name=='ThirdParty' else S/'Final';A.mkdir(parents=True,exist_ok=True)
im=Image.open(S/'rift-original.png').convert('RGBA');out=Image.new('RGBA',(1024,1024))
for y in range(4):
 for x in range(4):
  tile=im.crop((round(x*im.width/4),round(y*im.height/4),round((x+1)*im.width/4),round((y+1)*im.height/4))).resize((224,224),Image.Resampling.LANCZOS)
  out.alpha_composite(tile,(x*256+16,y*256+16))
dest=A/'Vfx_ChronalRift17.png';out.save(dest);report=dict(size=out.size,frames=16,padding=16,sha256=hashlib.sha256(dest.read_bytes()).hexdigest())
(S/'Import-Audit17.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print('Imported16chronalruptureframes with16pxgutter')
