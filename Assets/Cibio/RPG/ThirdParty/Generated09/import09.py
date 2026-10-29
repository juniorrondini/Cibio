from pathlib import Path
from PIL import Image
import numpy as np,json,sys,shutil
from scipy import ndimage

def segment(path,rows,cols):
 im=Image.open(path).convert('RGBA');rgba=np.array(im);labels,n=ndimage.label(rgba[:,:,3]>100);areas=np.bincount(labels.ravel());groups=[[] for _ in range(rows)]
 for i,sl in enumerate(ndimage.find_objects(labels),1):
  if sl is None or areas[i]<1000:continue
  yy,xx=sl;row=min(rows-1,int((yy.start+yy.stop)/2/im.height*rows));y0=max(0,yy.start-2);y1=min(im.height,yy.stop+2);x0=max(0,xx.start-2);x1=min(im.width,xx.stop+2);f=rgba[y0:y1,x0:x1].copy();f[:,:,3]*=ndimage.binary_dilation(labels[y0:y1,x0:x1]==i,iterations=2);groups[row].append((xx.start,Image.fromarray(f)))
 for row in groups:row.sort(key=lambda v:v[0]);assert len(row)>=cols,(str(path),len(row),cols)
 return groups
def pack(groups,cols,rows,reference_scale=None):
 # One form-wide scale, anchored at pelvis and sole; no per-frame rescaling.
 scale=reference_scale or min(104/max(f.height-4 for row in groups for _,f in row),117/max(f.width-4 for row in groups for _,f in row))
 out=Image.new('RGBA',(cols*128,rows*128));bounds=[]
 for d in range(rows):
  for c in range(cols):
   f=groups[d][c][1];pix=np.array(f);blue=(pix[:,:,2]>pix[:,:,0]*1.12)&(pix[:,:,2]>pix[:,:,1]*.95)&(pix[:,:,3]>100);band=np.zeros_like(blue);band[round(f.height*.50):round(f.height*.85)]=True;ys,xs=np.where(blue&band);anchor=float(np.median(xs)) if len(xs)>20 else f.width/2
   f=f.resize((max(1,round(f.width*scale)),max(1,round(f.height*scale))),Image.Resampling.LANCZOS);tile=Image.new('RGBA',(128,128));tile.alpha_composite(f,(round(64-anchor*scale),round(124-f.height+2*scale)));out.alpha_composite(tile,(c*128,d*128));bounds.append(tile.getbbox())
 return out,scale,bounds
