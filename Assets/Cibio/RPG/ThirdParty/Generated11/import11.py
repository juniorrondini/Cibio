from pathlib import Path
from PIL import Image
import numpy as np,json,sys,shutil
from scipy import ndimage
S=Path(__file__).parent;R=S.parents[1];A=R/'Resources/RpgArt';manifest=json.loads((S/'assets11.json').read_text(encoding='utf-8'));jobs={x['key']:x for x in manifest};report=[]
for j in manifest:j['path']=str(S/j['path'])

def parts(path,rows,cols):
 im=Image.open(path).convert('RGBA');p=np.array(im);labels,n=ndimage.label(p[:,:,3]>100);areas=np.bincount(labels.ravel());groups=[[] for _ in range(rows)]
 for k,sl in enumerate(ndimage.find_objects(labels),1):
  if sl is None or areas[k]<1000:continue
  yy,xx=sl;row=min(rows-1,int((yy.start+yy.stop)*.5/im.height*rows));groups[row].append((int(areas[k]),xx.start,im.crop((max(0,xx.start-2),max(0,yy.start-2),min(im.width,xx.stop+2),min(im.height,yy.stop+2)))))
 out=[]
 for row in groups:
  assert len(row)>=cols,(path,len(row),cols);row=sorted(sorted(row,reverse=True,key=lambda x:x[0])[:cols],key=lambda x:x[1]);out.append([x[2] for x in row])
 return out
def fit(im,cell,width,height,bottom):
 scale=min(width/im.width,height/im.height);size=(round(im.width*scale),round(im.height*scale));f=im.resize(size,Image.Resampling.LANCZOS);out=Image.new('RGBA',(cell,cell));out.alpha_composite(f,((cell-f.width)//2,bottom-f.height));return out
def save(im,name):im.save(A/(name+'.png'));report.append(dict(name=name,width=im.width,height=im.height))
groups=parts(jobs['davi']['path'],4,8);atlas=Image.new('RGBA',(1024,512))
for d,row in enumerate(groups):
 for c,f in enumerate(row):atlas.alpha_composite(fit(f,128,112,102,124),(c*128,d*128))
save(atlas,'Davi11')
for i,f in enumerate([f for row in parts(jobs['buildings']['path'],2,2) for f in row]):save(fit(f,384,344,350,374),'House11_'+str(i))
natural=[f for row in parts(jobs['nature-clean']['path'],2,3) for f in row]
for i in range(3):save(fit(natural[i],384,320,350,374),'Tree11_'+str(i))
save(fit(natural[3],256,216,208,246),'Shrub11');save(fit(natural[4],192,164,144,184),'Stone11');save(fit(natural[5],192,148,170,184),'Reed11')
pond=Image.open(jobs['pond']['path']).convert('RGBA');p=np.array(pond);ys,xs=np.where(p[:,:,3]>100);pond=pond.crop((max(0,xs.min()-2),max(0,ys.min()-2),min(pond.width,xs.max()+3),min(pond.height,ys.max()+3)));save(fit(pond,512,456,346,500),'Pond11')
im=Image.open(jobs['ground']['path']).convert('RGB')
for name,c,r in [('Grass',0,0),('Earth',1,0),('Stone',0,1),('Damp',1,1)]:save(im.crop((round(c*im.width/2),round(r*im.height/2),round((c+1)*im.width/2),round((r+1)*im.height/2))).resize((512,512),Image.Resampling.LANCZOS),'Terrain_'+name+'11')
props=[f for row in parts(jobs['props']['path'],2,3) for f in row]
for i,(name,cell,width,height,bottom) in enumerate([('Barrel11',192,136,164,184),('Bench11',256,220,170,246),('Lamp11',256,100,236,246),('Stall11',384,320,320,374),('Planter11',192,128,164,184),('Well11',256,212,220,246)]):save(fit(props[i],cell,width,height,bottom),name)
for j in manifest:
 j['path']=j['key']+'-original.png'
 if 'referenced_image_paths' in j:j['references']=[Path(x).name for x in j.pop('referenced_image_paths')]
(S/'assets11.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8');(S/'Import-Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(report)
