from pathlib import Path
import sys,json,shutil,numpy as np
from PIL import Image
W=Path(__file__).parent;R=W.parent.parent;A=R/'Resources/RpgArt';S=R/'ThirdParty/Generated07';S.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(W/'audio_libs'));from scipy import ndimage
original=S/'Flight-original.png'
im=Image.open(original).convert('RGBA');rgba=np.array(im);labels,n=ndimage.label(rgba[:,:,3]>100);areas=np.bincount(labels.ravel());groups=[[] for _ in range(4)]
for k,sl in enumerate(ndimage.find_objects(labels),1):
 if sl is None or areas[k]<1300:continue
 yy,xx=sl;row=min(3,int((yy.start+yy.stop)/2/im.height*4));crop=rgba[yy,xx].copy();crop[:,:,3]*=ndimage.binary_dilation(labels[yy,xx]==k,iterations=2);groups[row].append((xx.start,Image.fromarray(crop)))
for row in groups:row.sort(key=lambda p:p[0]);assert len(row)==8,len(row)
report=[]
for fam,offset,cell,height in [('Hero',0,64,49),('HeroAwakened',4,128,104)]:
 # Register to the pelvis, preserving poses and using one scale per form.
 scale=height/np.median([groups[d][offset+c][1].height for d in range(4) for c in (0,1)])
 out=Image.new('RGBA',(cell*4,cell*4))
 for d in range(4):
  for c in range(4):
   f=groups[d][offset+c][1];pix=np.array(f);blue=(pix[:,:,2]>pix[:,:,0]*1.13)&(pix[:,:,2]>pix[:,:,1]*1.02)&(pix[:,:,3]>150);yy,xx=np.where(blue);hipx=float(np.median(xx)) if len(xx) else f.width/2
   # Height and feet registration agree with existing action atlases.
   f=f.resize((max(1,round(f.width*scale)),max(1,round(f.height*scale))),Image.Resampling.LANCZOS);canvas=Image.new('RGBA',(cell,cell));canvas.alpha_composite(f,(round(cell*.5-hipx*scale),round(cell*.969-f.height)));out.alpha_composite(canvas,(c*cell,d*cell))
 out.save(A/(fam+'_flight.png'));report.append(dict(family=fam,frames=16,cell=cell,scale=scale))
(S/'Import-Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8');shutil.copy2(__file__,S/'import_flight07.py')
print(report)
