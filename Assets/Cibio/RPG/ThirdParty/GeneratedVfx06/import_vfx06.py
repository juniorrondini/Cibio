"""Import image_gen animations: fixed 4x4 frames with transparent gutters."""
from pathlib import Path
from PIL import Image
import numpy as np,json,shutil
W=Path(__file__).parent;R=W.parent.parent if W.name=='GeneratedVfx06' else W/'Cibio06/Assets/Cibio/RPG';A=R/'Resources/RpgArt';S=R/'ThirdParty/GeneratedVfx06';S.mkdir(exist_ok=True)
items=json.loads((W/'vfx06.json').read_text(encoding='utf-8'));report=[]
for item in items:
    source=Path(item['path']);source=source if source.is_absolute() else W/source;im=Image.open(source).convert('RGBA')
    if source.resolve()!=(S/(item['name']+'-original.png')).resolve():shutil.copy2(source,S/(item['name']+'-original.png'))
    out=Image.new('RGBA',(1024,1024));stats=[]
    for row in range(4):
        for col in range(4):
            cell=im.crop((round(col*im.width/4),round(row*im.height/4),round((col+1)*im.width/4),round((row+1)*im.height/4)))
            alpha=np.array(cell.getchannel('A'));assert (alpha>20).sum()>80,(item['name'],row,col,'empty');assert (alpha<12).sum()>100,(item['name'],row,col,'opaque background')
            cell=cell.resize((208,208),Image.Resampling.LANCZOS);out.alpha_composite(cell,(col*256+24,row*256+24));stats.append(int((alpha>20).sum()))
    out.save(A/('Vfx_'+item['name']+'06.png'));arr=np.array(out.getchannel('A'));assert not arr[0].any() and not arr[-1].any();report.append(dict(name=item['name'],size=[1024,1024],frames=16,nonempty=stats,alpha_min=int(arr.min()),alpha_max=int(arr.max())))
(S/'Import-Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
if (W/'import_vfx06.py').resolve()!=(S/'import_vfx06.py').resolve():shutil.copy2(W/'import_vfx06.py',S/'import_vfx06.py')
for item in items:item['path']=item['name']+'-original.png'
(S/'vfx06.json').write_text(json.dumps(items,indent=2),encoding='utf-8')
(R/'ThirdParty/Prompts-VFX-06.txt').write_text('Ferramenta: image_gen integrada. Atlas RGBA originais; importação recorta e acrescenta margens transparentes sem redesenhar a arte.\n\n'+'\n\n'.join(x['name']+'\n'+x['prompt'] for x in items),encoding='utf-8')
print(json.dumps(report,indent=2))
