from pathlib import Path
from PIL import Image
import numpy as np
import json, shutil,sys
from scipy import ndimage

W=Path(__file__).parent
S=W
R=S.parent.parent
A=R/'Resources/RpgArt' if S.parent.name=='ThirdParty' else S/'Final';A.mkdir(parents=True,exist_ok=True)
S.mkdir(parents=True,exist_ok=True)
jobs=json.loads((S/'assets15.json').read_text(encoding='utf-8-sig'))
audit=[]
def atlas(im,cols,rows,cell):
    # Generated atlases can drift outside equal cells. Extract complete isolated
    # silhouettes first, without cutting feet or including a neighbouring frame.
    data=np.array(im);labels,n=ndimage.label(data[:,:,3]>45);areas=np.bincount(labels.ravel());groups=[[] for _ in range(rows)]
    for k,sl in enumerate(ndimage.find_objects(labels),1):
        if sl is None or areas[k]<1500:continue
        yy,xx=sl;row=min(rows-1,int((yy.start+yy.stop)*.5/im.height*rows));groups[row].append((xx.start,k,yy,xx))
    cells=[];heights=[];left=[];right=[]
    for group in groups:
        assert len(group)==cols,(len(group),cols)
        for _,k,yy,xx in sorted(group):
            x0=max(0,xx.start-3);y0=max(0,yy.start-3);x1=min(im.width,xx.stop+3);y1=min(im.height,yy.stop+3)
            crop=data[y0:y1,x0:x1].copy();own=ndimage.binary_dilation(labels[y0:y1,x0:x1]==k,iterations=3);crop[~own]=0
            # Register the dark boot/leg band rather than an extended horn arm.
            band=(np.indices(crop.shape[:2])[0]>=crop.shape[0]*.75)&(crop[:,:,3]>100)
            rgb=crop[:,:,:3].astype(float);low=rgb.min(axis=2);high=rgb.max(axis=2);dark=band&(high<105)&((high-low)/(high+1)<.48)
            py,px=np.where(dark);anchor=(float(px.min()+px.max())*.5) if len(px)>12 else (crop.shape[1]*.5)
            cells.append((Image.fromarray(crop),anchor));heights.append(crop.shape[0]-6);left.append(anchor);right.append(crop.shape[1]-anchor)
    scale=min(152/np.median(heights[::cols]),168/max(heights),(cell/2-8)/max(left),(cell/2-8)/max(right))
    out=Image.new('RGBA',(cols*cell,rows*cell))
    for i,(crop,anchor) in enumerate(cells):
        small=crop.resize((round(crop.width*scale),round(crop.height*scale)),Image.Resampling.LANCZOS)
        x=round(cell/2-anchor*scale);y=230-small.height
        assert x>=2 and x+small.width<cell-1 and y>=2,(i,x,y,small.size)
        out.alpha_composite(small,((i%cols)*cell+x,(i//cols)*cell+y))
    return out
def padded(im,cols,rows,cell,padding):
    out=Image.new('RGBA',(cols*cell,rows*cell))
    for y in range(rows):
        for x in range(cols):
            tile=im.crop((round(x*im.width/cols),round(y*im.height/rows),round((x+1)*im.width/cols),round((y+1)*im.height/rows))).resize((cell-2*padding,cell-2*padding),Image.Resampling.LANCZOS)
            out.alpha_composite(tile,(x*cell+padding,y*cell+padding))
    return out
for job in jobs:
    src=S/job['path'];im=Image.open(src).convert('RGBA')
    key=job['key']
    if key.startswith('boss_'):
        action=key[5:];action={'spell':'spellcast'}.get(action,action);cols={'idle':4,'walk':8,'slash':6,'spellcast':8,'hurt':6}[action]
        out=atlas(im,cols,4,256);name='BoneroCompanheiro15_'+action
    elif key in ('flight','hornfx'):
        out=padded(im,4,4,256,16);name='Vfx_WorldFlight15' if key=='flight' else 'Vfx_Horn15'
    elif key=='talents':out=padded(im,3,3,256,12);name='UI_Talents15'
    elif key=='fold':out=im.convert('RGB').resize((2048,2304),Image.Resampling.LANCZOS);name='WorldFold15'
    elif key=='arena':out=im.convert('RGB').resize((2048,1536),Image.Resampling.LANCZOS);name='Arena_Vuvuzela15'
    elif key=='world':out=im.convert('RGB').resize((2048,1152),Image.Resampling.LANCZOS);name='World_Vuvuzela15'
    else:raise ValueError(key)
    out.save(A/(name+'.png'));audit.append({'name':name,'source':key+'-original.png','size':out.size})
portable=[]
for j in jobs:portable.append({**{k:v for k,v in j.items() if k not in ('path','refs')},'path':j['key']+'-original.png','references':j.get('references',[])})
(S/'assets15.json').write_text(json.dumps(portable,ensure_ascii=False,indent=2),encoding='utf-8')
(S/'Import-Audit15.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print('Imported',len(audit),'original assets')
