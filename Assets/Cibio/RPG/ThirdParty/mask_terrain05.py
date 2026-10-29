from pathlib import Path
import numpy as np
from PIL import Image
W=Path(__file__).parent;R=W.parent/'Resources/RpgArt' if W.name=='ThirdParty' else W/'Cibio05/Assets/Cibio/RPG/Resources/RpgArt'
h,w=768,1024
y,x=np.mgrid[0:h,0:w];x=x/(w-1)*48-24;y=(1-y/(h-1))*36-18
edge=.09*np.sin(x*6+y*3)+.07*np.sin(y*11-x*7)+.08*np.sin(x*2-y*4)
road=np.minimum(abs(y-(-2+.65*np.sin(x*.19)))-1.15,abs(x-(.65*np.sin(y*.24)))-.98)
road=np.minimum(road,abs(y-(7.2+.65*np.sin(x*.22)))-.88)
def segment(ax,ay,bx,by,width):
    u=np.clip(((x-ax)*(bx-ax)+(y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
    return np.sqrt((x-ax-u*(bx-ax))**2+(y-ay-u*(by-ay))**2)-width
for points in [(-10,-2,-11,-10,.75),(-1,-10,13,-10,.72),(13,-10,13,-5,.72),(-18,7,-14,2,.7)]:road=np.minimum(road,segment(*points))
theta=np.arctan2((y+.6)/4.3,x/5.8)
plaza=(np.sqrt((x/5.8)**2+((y+.6)/4.3)**2)-1)*4.3+.13*np.sin(theta*5)+.08*np.sin(theta*9)+edge
smooth=lambda z:np.clip(z,0,1)**2*(3-2*np.clip(z,0,1))
stone=smooth((.18-plaza)/.36)
earth=np.maximum(smooth((.24-road-edge)/.48),smooth((.58-plaza)/.7))
out=np.stack((earth,stone,np.zeros_like(x),np.ones_like(x)),axis=2)
Image.fromarray((out*255).astype('uint8')).save(R/'Terrain_Mask05.png')
print('Organic mask',out.shape,'stone pixels',int((stone>.5).sum()))
