from pathlib import Path
from PIL import Image
import numpy as np,json,sys,shutil
from import09 import segment
S=Path(__file__).parent;R=S.parents[1];A=R/'Resources/RpgArt';manifest=json.loads((S/'assets09.json').read_text(encoding='utf-8'));items={x['action']:x for x in manifest};CELL=192;report=[]
for item in manifest:item['path']=str(S/item['path'])

def select(groups,cols):
 return [[row[c] for c in np.round(np.linspace(0,len(row)-1,cols)).astype(int)] for row in groups]
def pack(groups,cols,rows,scale=None):
 out=Image.new('RGBA',(cols*CELL,rows*CELL));scales=[]
 for d in range(rows):
  for c in range(cols):
   f=groups[d][c][1];p=np.array(f);blue=(p[:,:,2]>p[:,:,0]*1.12)&(p[:,:,2]>p[:,:,1]*.95)&(p[:,:,3]>100);band=np.zeros_like(blue);band[round(f.height*.48):round(f.height*.87)]=True;yy,xx=np.where(blue&band);anchor=float(np.median(xx)) if len(xx)>20 else f.width/2
   target=90+14*c/7 if cols==8 and action=='transition' else 104
   k=min(target/max(1,f.height-4),89/max(anchor,f.width-anchor));scales.append(k);f=f.resize((max(1,round(f.width*k)),max(1,round(f.height*k))),Image.Resampling.LANCZOS);tile=Image.new('RGBA',(CELL,CELL));tile.alpha_composite(f,(round(96-anchor*k),round(188-f.height+2*k)));out.alpha_composite(tile,(c*CELL,d*CELL))
 return out,float(np.median(scales))
action='transition';transition=select(segment(items['transition']['path'],4,8),8);north=segment(items['north']['path'],2,4);back=north[0]+north[1];transition[0]=[transition[0][0]]+back[1:];atlas,scale=pack(transition,8,4);atlas.save(A/'Transform_Ascendant09.png');idle=Image.new('RGBA',(CELL*2,CELL*4))
for d in range(4):
 for c in range(2):idle.alpha_composite(atlas.crop(((c+6)*CELL,d*CELL,(c+7)*CELL,(d+1)*CELL)),(c*CELL,d*CELL))
idle.save(A/'HeroAscendant_idle.png');report.append(dict(action='transition',scale=scale,frames=32))
for item in manifest:
 action=item['action']
 if action in ['transition','north','sonic','icons']:continue
 cols=item['columns'];groups=select(segment(item['path'],4,cols),cols);sheet,scale=pack(groups,cols,4)
 if action=='walk':
  out=Image.new('RGBA',(CELL*9,CELL*4))
  for d in range(4):out.alpha_composite(idle.crop((0,d*CELL,CELL,(d+1)*CELL)),(0,d*CELL));out.alpha_composite(sheet.crop((0,d*CELL,cols*CELL,(d+1)*CELL)),(CELL,d*CELL))
  sheet=out
 if action=='hurt':sheet=sheet.crop((0,CELL*2,cols*CELL,CELL*3))
 sheet.save(A/('HeroAscendant_'+action+'.png'));report.append(dict(action=action,scale=scale,width=sheet.width,height=sheet.height))
im=Image.open(items['sonic']['path']).convert('RGBA');out=Image.new('RGBA',(1024,1024))
for row in range(4):
 for col in range(4):
  tile=im.crop((round(col*im.width/4),round(row*im.height/4),round((col+1)*im.width/4),round((row+1)*im.height/4)));tile.thumbnail((208,208),Image.Resampling.LANCZOS);out.alpha_composite(tile,(col*256+(256-tile.width)//2,row*256+(256-tile.height)//2))
out.save(A/'Vfx_Sonic09.png')
icons=Image.open(S/'icons-original.png').convert('RGBA');atlas=Image.new('RGBA',(512,256))
for r in range(2):
 for c in range(4):
  f=icons.crop((round(c*icons.width/4),round(r*icons.height/2),round((c+1)*icons.width/4),round((r+1)*icons.height/2)));f=f.crop(f.getbbox());f.thumbnail((112,112),Image.Resampling.LANCZOS);atlas.alpha_composite(f,(c*128+(128-f.width)//2,r*128+(128-f.height)//2))
atlas.save(A/'UI_Skills09.png')
for item in manifest:item['path']=item['action']+'-original.png'
(S/'assets09.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8');(S/'Import-Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report,indent=2))
