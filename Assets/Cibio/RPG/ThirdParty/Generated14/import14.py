from pathlib import Path
from PIL import Image
import numpy as np,json,sys,shutil
W=Path(__file__).parent;from scipy import ndimage
S=W;R=S.parent.parent;A=R/'Resources/RpgArt';A.mkdir(parents=True,exist_ok=True);jobs=json.loads((W/'assets14.json').read_text(encoding='utf-8-sig'));audit=[]
def parts(im,rows,cols):
 labels,n=ndimage.label(np.array(im)[:,:,3]>100);areas=np.bincount(labels.ravel());groups=[[] for _ in range(rows)]
 for k,sl in enumerate(ndimage.find_objects(labels),1):
  if sl is None or areas[k]<1500:continue
  yy,xx=sl;r=min(rows-1,int((yy.start+yy.stop)*.5/im.height*rows))
  # Extract only this atlas island; adjacent assets can overlap its bounding rectangle.
  box=(max(0,xx.start-2),max(0,yy.start-2),min(im.width,xx.stop+2),min(im.height,yy.stop+2))
  sprite=np.array(im.crop(box));own=ndimage.binary_dilation(labels[box[1]:box[3],box[0]:box[2]]==k,iterations=2)
  sprite[~own]=0;groups[r].append((int(areas[k]),xx.start,Image.fromarray(sprite)))
 out=[]
 for row in groups:
  assert len(row)>=cols,(len(row),cols);out.extend([x[2] for x in sorted(sorted(row,key=lambda v:v[0],reverse=True)[:cols],key=lambda v:v[1])])
 return out
def savefit(im,name,cell,w,h,bottom):
 scale=min(w/im.width,h/im.height);f=im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.LANCZOS);out=Image.new('RGBA',cell);out.alpha_composite(f,((cell[0]-f.width)//2,bottom-f.height));out.save(A/(name+'.png'));a=np.array(out)[:,:,3];ys,xs=np.where(a>8);assert xs.min()>0 and xs.max()<cell[0]-1 and ys.min()>0 and ys.max()<cell[1]-1; audit.append(dict(name=name,size=cell,bounds=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)]))
for j in jobs:
 im=Image.open(S/j['path']).convert('RGBA');None
 if j['key']=='ground':
  for name,c,r in [('Sand',0,0),('Sea',1,0),('Wood',0,1),('CoastRock',1,1)]:im.crop((round(c*im.width/2),round(r*im.height/2),round((c+1)*im.width/2),round((r+1)*im.height/2))).convert('RGB').resize((512,512),Image.Resampling.LANCZOS).save(A/f'Terrain_{name}14.png')
 elif j['key']=='buildings':
  p=parts(im,1,2);savefit(p[0],'Lighthouse14',(512,768),464,704,750);savefit(p[1],'Cabin14',(512,512),464,414,498)
 else:
  p=parts(im,2,3)
  for index,name,cell,w,h,bottom in [(0,'Boat14',(384,256),344,206,246),(1,'Palm14',(384,512),344,462,498),(2,'CoastRock14',(256,256),224,208,246),(3,'Pier14',(512,256),480,218,250),(4,'Cache14_closed',(128,128),112,88,122),(5,'Cache14_open',(128,128),112,118,122)]:savefit(p[index],name,cell,w,h,bottom)
for j in jobs:
 j['path']=j['key']+'-original.png'
 if 'refs' in j:j['references']=[Path(p).name for p in j.pop('refs')]
(S/'assets14.json').write_text(json.dumps(jobs,indent=2,ensure_ascii=False),encoding='utf-8');(S/'Import-Audit14.json').write_text(json.dumps(audit,indent=2),encoding='utf-8');print('Imported12 coastal assets')
