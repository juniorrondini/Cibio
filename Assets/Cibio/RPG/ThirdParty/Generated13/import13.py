from pathlib import Path
from PIL import Image
import numpy as np,json,shutil
W=Path(__file__).parent;S=W;R=S.parent.parent;A=R/'Resources/RpgArt';A.mkdir(parents=True,exist_ok=True);jobs=json.loads((W/'assets13.json').read_text(encoding='utf-8-sig'));audit=[]
def fit(im,w,h):
 scale=min(w/im.width,h/im.height);return im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.LANCZOS)
for j in jobs:
 source=S/j['path'];im=Image.open(source).convert('RGBA');None
 if j['key']=='title':
  # Preserve the complete authored composition, including the quiet left area for the menu.
  im.convert('RGB').save(A/'Title_Vila13.png');audit.append(dict(name='Title_Vila13',size=im.size,aspect=im.width/im.height));continue
 assert (np.array(im)[:,:,3]<8).mean()>.1,j['key']
 if j['key']=='icon':
  alpha=np.array(im)[:,:,3];ys,xs=np.where(alpha>8);im=im.crop((xs.min(),ys.min(),xs.max()+1,ys.max()+1));f=fit(im,216,216);out=Image.new('RGBA',(256,256));out.alpha_composite(f,((256-f.width)//2,(256-f.height)//2));out.save(A/'UI_Palma13.png');audit.append(dict(name='UI_Palma13',size=out.size));continue
 out=Image.new('RGBA',(1024,1024));cells=[]
 for r in range(4):
  for c in range(4):
   part=im.crop((round(c*im.width/4),round(r*im.height/4),round((c+1)*im.width/4),round((r+1)*im.height/4)));f=fit(part,220,220);cell=Image.new('RGBA',(256,256));cell.alpha_composite(f,((256-f.width)//2,(256-f.height)//2));out.alpha_composite(cell,(c*256,r*256));alpha=np.array(cell)[:,:,3];ys,xs=np.where(alpha>8);assert len(xs)>16,(j['key'],r,c);assert xs.min()>=16 and ys.min()>=16 and xs.max()<240 and ys.max()<240
   cells.append(dict(frame=r*4+c,bounds=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],nonempty_pixels=int(len(xs))))
 name='Vfx_PalmaFlight13' if j['key']=='projectile' else 'Vfx_PalmaImpact13';out.save(A/(name+'.png'));audit.append(dict(name=name,size=out.size,cells=cells))
for j in jobs:
 j['path']=j['key']+'-original.png'
 if 'refs' in j:j['references']=[Path(p).name for p in j.pop('refs')]
(S/'assets13.json').write_text(json.dumps(jobs,indent=2,ensure_ascii=False),encoding='utf-8');(S/'Import-Audit13.json').write_text(json.dumps(audit,indent=2),encoding='utf-8');print('Imported title, dedicated icon and32 padded VFX frames')
