"""Production import only: slice image_gen sheets, register feet, resize, pack native frames."""
from pathlib import Path
from PIL import Image
import json,shutil,numpy as np,sys
sys.path.insert(0,str(Path(__file__).parent/'audio_libs'))
from scipy import ndimage
W=Path(__file__).parent;R=W.parent.parent if W.name=='Generated05' else W/'Cibio05/Assets/Cibio/RPG';A=R/'Resources/RpgArt';S=R/'ThirdParty/Generated05';S.mkdir(exist_ok=True)
CELL=128
manifest=json.loads((W/'assets05.json').read_text(encoding='utf-8'))
report=[]
for item in manifest:
    action=item['action'];cols=item.get('source_columns',item['columns']);rows=item['rows'];source=Path(item['path']);source=source if source.is_absolute() else W/source;im=Image.open(source).convert('RGBA')
    if source.resolve()!=(S/(action+'-original.png')).resolve():shutil.copy2(source,S/(action+'-original.png'))
    # Generated sheets have uneven gutters and poses crossing nominal row boundaries.
    # Segment actual connected character silhouettes instead of cutting an assumed grid.
    rgba=np.array(im);labels,count=ndimage.label(rgba[:,:,3]>100);areas=np.bincount(labels.ravel());objects=ndimage.find_objects(labels);groups=[[] for _ in range(rows)]
    for i,slices in enumerate(objects,1):
        if not slices or areas[i]<1000:continue
        yy,xx=slices;row=min(rows-1,int((yy.start+yy.stop)/2/im.height*rows));pad=3
        x0=max(0,xx.start-pad);x1=min(im.width,xx.stop+pad);y0=max(0,yy.start-pad);y1=min(im.height,yy.stop+pad)
        crop=rgba[y0:y1,x0:x1].copy();keep=ndimage.binary_dilation(labels[y0:y1,x0:x1]==i,iterations=3);crop[:,:,3]*=keep
        blue=(crop[:,:,2]>crop[:,:,0]*1.12)&(crop[:,:,2]>crop[:,:,1]*.95)&(crop[:,:,3]>100)
        band=np.zeros_like(blue);band[round(crop.shape[0]*.45):round(crop.shape[0]*.85)]=True;ys,xs=np.where(blue&band)
        anchor=float(np.median(xs)) if len(xs)>20 else crop.shape[1]/2
        groups[row].append(((xx.start+xx.stop)/2,Image.fromarray(crop),anchor))
    for row in range(rows):
        groups[row].sort(key=lambda p:p[0]);assert len(groups[row])==cols,(action,row,'Expected',cols,'got',len(groups[row]))
    scale=min(104/max(f.height-6 for line in groups for _,f,_ in line),120/max(f.width-6 for line in groups for _,f,_ in line))
    frames=[]
    for row in range(rows):
        line=[]
        for col in range(cols):
            _,f,anchor=groups[row][col];size=(max(1,round(f.width*scale)),max(1,round(f.height*scale)));bottom=f.height-3;f=f.resize(size,Image.Resampling.LANCZOS)
            canvas=Image.new('RGBA',(CELL,CELL));canvas.alpha_composite(f,(round(64-anchor*scale),round(124-bottom*scale)))
            assert canvas.getchannel('A').getextrema()[1]>100,(action,row,col)
            line.append(canvas)
        frames.append(line)
    if 'select_columns' in item:
        frames=[[line[c] for c in item['select_columns']] for line in frames];cols=len(item['select_columns'])
    if action=='transition':
        out=Image.new('RGBA',(cols*CELL,rows*CELL))
        for row in range(rows):
            for col in range(cols):out.alpha_composite(frames[row][col],(col*CELL,row*CELL))
        out.save(A/'Transform_Sequence05.png')
        idle=Image.new('RGBA',(CELL*2,CELL*4))
        for row in range(4):
            for col in range(2):idle.alpha_composite(frames[row][4+col],(CELL*col,row*CELL))
        idle.save(A/'HeroAwakened_idle.png')
    else:
        if action=='walk':
            # Native walk includes a standing frame, followed by the eight-frame loop.
            idle=Image.open(A/'HeroAwakened_idle.png')
            frames=[[idle.crop((CELL,row*CELL,CELL*2,(row+1)*CELL))]+frames[row] for row in range(4)];cols=9
        if action=='hurt':frames=[frames[2]];rows=1
        out=Image.new('RGBA',(cols*CELL,rows*CELL))
        for row in range(rows):
            for col in range(cols):out.alpha_composite(frames[row][col],(col*CELL,row*CELL))
        out.save(A/('HeroAwakened_'+action+'.png'))
    report.append(dict(action=action,frames=cols*rows,dimensions=list(out.size),source_dimensions=list(im.size),scale=scale,nonempty=True))
(S/'Import-Report.json').write_text(json.dumps(report,indent=2))
if (W/'import_sprites05.py').resolve()!=(S/'import_sprites05.py').resolve():shutil.copy2(W/'import_sprites05.py',S/'import_sprites05.py')
for item in manifest:item['path']=item['action']+'-original.png'
(S/'assets05.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))

