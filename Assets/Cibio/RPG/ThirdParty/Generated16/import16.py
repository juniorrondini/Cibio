from pathlib import Path
from PIL import Image
import numpy as np
from scipy import ndimage
import json,hashlib

S=Path(__file__).parent
A=S.parent.parent/'Resources/RpgArt' if S.parent.name=='ThirdParty' else S/'Final'
A.mkdir(parents=True,exist_ok=True)
jobs=json.loads((S/'assets16.json').read_text(encoding='utf-8-sig'))
def atlas(im,cols):
 data=np.array(im.convert('RGBA'));labels,n=ndimage.label(data[:,:,3]>45);areas=np.bincount(labels.ravel());groups=[[] for _ in range(4)]
 for k,sl in enumerate(ndimage.find_objects(labels),1):
  if sl is None or areas[k]<1500:continue
  yy,xx=sl;row=min(3,int((yy.start+yy.stop)*.5/im.height*4));groups[row].append((xx.start,k,yy,xx))
 cells=[];heights=[];left=[];right=[]
 for group in groups:
  assert len(group)==cols,(len(group),cols)
  for _,k,yy,xx in sorted(group):
   x0=max(0,xx.start-3);y0=max(0,yy.start-3);x1=min(im.width,xx.stop+3);y1=min(im.height,yy.stop+3)
   crop=data[y0:y1,x0:x1].copy();own=ndimage.binary_dilation(labels[y0:y1,x0:x1]==k,iterations=3);crop[~own]=0
   band=(np.indices(crop.shape[:2])[0]>=crop.shape[0]*.75)&(crop[:,:,3]>100)
   rgb=crop[:,:,:3].astype(float);low=rgb.min(axis=2);high=rgb.max(axis=2);dark=band&(high<105)&((high-low)/(high+1)<.48)
   py,px=np.where(dark);anchor=float(px.min()+px.max())*.5 if len(px)>12 else crop.shape[1]*.5
   cells.append((Image.fromarray(crop),anchor));heights.append(crop.shape[0]-6);left.append(anchor);right.append(crop.shape[1]-anchor)
 scale=min(152/np.median(heights[::cols]),168/max(heights),120/max(left),120/max(right))
 out=Image.new('RGBA',(cols*256,1024))
 for i,(crop,anchor) in enumerate(cells):
  small=crop.resize((round(crop.width*scale),round(crop.height*scale)),Image.Resampling.LANCZOS);x=round(128-anchor*scale);y=230-small.height
  assert x>=2 and x+small.width<255 and y>=2,(i,x,y,small.size)
  out.alpha_composite(small,((i%cols)*256+x,(i//cols)*256+y))
 return out
audit=[]
for job in jobs:
 out=atlas(Image.open(S/job['path']),job['cols']);path=A/('BoneroCompanheiro16_'+job['key']+'.png');out.save(path)
 # Every cell must contain a single complete silhouette and empty surrounding gutter.
 for row in range(4):
  for col in range(job['cols']):
   a=np.array(out.crop((col*256,row*256,(col+1)*256,(row+1)*256)))[:,:,3];lab,n=ndimage.label(a>100);areas=np.bincount(lab.ravel())[1:]
   assert sum(areas>30)==1,(job['key'],row,col,'stray fragments')
   assert a[:2,:].max()==a[-2:,:].max()==a[:,:2].max()==a[:,-2:].max()==0
 audit.append(dict(name=path.name,size=out.size,frames=job['cols']*4,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(S/'Import-Audit16.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print('Imported',sum(j['frames'] for j in audit),'complete split-identity frames; no foreign fragments')
