from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json,sys,shutil
W=Path(__file__).parent;from scipy import ndimage
S=W;R=S.parent.parent;A=R/'Resources/RpgArt'
jobs=json.loads((W/'assets12.json').read_text(encoding='utf-8'));audit=[];contact=[]
for j in jobs:
 im=Image.open(S/j['path']).convert('RGBA');arr=np.array(im);alpha=arr[:,:,3];assert (alpha==0).mean()>.15,(j['key'],'Background not transparent')
 labels,n=ndimage.label(alpha>100);areas=np.bincount(labels.ravel());groups=[[] for _ in range(4)];cols={'idle':4,'walk':9,'slash':6,'spellcast':7,'hurt':6}[j['key']]
 for k,sl in enumerate(ndimage.find_objects(labels),1):
  if sl is None or areas[k]<1500:continue
  yy,xx=sl;d=min(3,int((yy.start+yy.stop)*.5/im.height*4));groups[d].append((areas[k],xx.start,im.crop((max(0,xx.start-1),max(0,yy.start-1),min(im.width,xx.stop+1),min(im.height,yy.stop+1)))))
 atlas=Image.new('RGBA',(cols*128,512))
 for d,row in enumerate(groups):
  assert len(row)>=cols,(j['key'],d,len(row));row=sorted(sorted(row,reverse=True,key=lambda x:x[0])[:cols],key=lambda x:x[1]);frames=[x[2] for x in row]
  # One scale for a direction/action preserves authored crouching and collapse.
  scale=min(104/frames[0].height,120/max(f.width for f in frames),108/max(f.height for f in frames));rowaudit=[]
  for c,f in enumerate(frames):
   f=f.resize((round(f.width*scale),round(f.height*scale)),Image.Resampling.LANCZOS);cell=Image.new('RGBA',(128,128));cell.alpha_composite(f,((128-f.width)//2,108-f.height));atlas.alpha_composite(cell,(c*128,d*128));a=np.array(cell)[:,:,3];ys,xs=np.where(a>100)
   assert xs.min()>0 and xs.max()<127 and ys.min()>=0 and ys.max()<127
   rowaudit.append(dict(direction=d,frame=c,bounds=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],source_height=frames[c].height,scale=scale))
  audit.extend(dict(action=j['key'],**x) for x in rowaudit)
 atlas.save(A/('Companheiro12_'+j['key']+'.png'));None
 preview=Image.new('RGBA',atlas.size,(42,48,53,255));preview.alpha_composite(atlas);contact.append((j['key'],preview.convert('RGB')))
 j['path']=j['key']+'-original.png'
(S/'assets12.json').write_text(json.dumps(jobs,indent=2,ensure_ascii=False),encoding='utf-8');(S/'Import-Audit12.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
sheet=Image.new('RGB',(9*128+150,sum(i.height+30 for _,i in contact)),(33,39,43));draw=ImageDraw.Draw(sheet);y=0
for name,im in contact:draw.text((10,y+12),name,fill='white');sheet.paste(im,(150,y+30));y+=im.height+30
sheet.save(W/'Companheiro-Contact12.jpg',quality=95)
print('Imported',len(audit),'frames, genuine alpha, consistent per-action scales')
