from pathlib import Path
import numpy as np
from scipy import signal
import wave,json,hashlib
S=Path(__file__).parent;R=S.parent.parent
A=R/'Resources/RpgAudio/Designed17' if S.parent.name=='ThirdParty' else S/'FinalAudio';A.mkdir(parents=True,exist_ok=True)
rate=48000;rng=np.random.default_rng(1717);audit=[]
def read(name):
 with wave.open(str(S/'Decoded'/(name+'.wav')),'rb') as w:
  assert w.getframerate()==rate and w.getsampwidth()==2
  a=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').reshape(-1,w.getnchannels()).astype(float)/32768
 return a.mean(axis=1)
def sample(name,n,stretch=1,reverse=False):
 a=read(name);a=a[::-1] if reverse else a;length=max(2,round(len(a)*stretch));a=np.interp(np.linspace(0,len(a)-1,length),np.arange(len(a)),a)
 out=np.zeros(n);out[:min(n,len(a))]=a[:n];return out
def band(a,lo,hi):return signal.sosfilt(signal.butter(3,[lo,hi],btype='bandpass',fs=rate,output='sos'),a)
def echoes(a,delays):
 out=a.copy()
 for sec,gain in delays:
  d=round(sec*rate);out[d:]+=a[:-d]*gain
 return out
def save(name,a,loop=False):
 a=np.asarray(a);a-=a.mean(axis=0);a=np.tanh(a*1.25)
 n=len(a);t=np.arange(n)/rate
 if a.ndim==1:
  right=np.roll(a,43)*.98;right[:43]=0;a=np.column_stack([a,right])
 if loop:
  # Fold overlap into both ends; force equal, zero-slope endpoints without a gap.
  k=round(rate*.18);fade=np.linspace(0,1,k)[:,None];seam=a[-k:]*(1-fade)+a[:k]*fade;a[:k]=seam;a[-k:]=seam
  a[-1]=a[0];a[-2]=a[0];a[1]=a[0]
 else:a*=np.minimum(1,t/.008)[:,None]*np.minimum(1,(n/rate-t)/.08)[:,None]
 a*=.84/max(.84,float(np.abs(a).max()));out=A/(name+'.wav');pcm=(a*32767).astype('<i2')
 with wave.open(str(out),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes(pcm.tobytes())
 audit.append(dict(name=name,seconds=n/rate,peak=float(np.abs(a).max()),rms=float(np.sqrt(np.mean(a*a))),dc=float(np.max(np.abs(a.mean(axis=0)))),loop=loop,seam=float(np.max(np.abs(a[-1]-a[0]))) if loop else 0,sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
 return a
def hit(length,large=False):
 t=np.arange(round(length*rate))/rate;n=len(t)
 sub=np.sin(2*np.pi*(42*t+2.8*(1-np.exp(-t*13))))*np.exp(-t*(3 if large else 6))*.8
 air=band(rng.normal(0,1,n),140,8200)*(1-np.exp(-t*160))*np.exp(-t*7)*.25
 x=sub+air+sample('lowFrequency_explosion_000',n,1.35 if large else .85)*.6
 x+=sample('forceField_002',n,1.2,True)*.24+sample('impactMetal_002',n,.6)*.1
 return echoes(x,[(.071,.14),(.137,.1),(.283,.07)])
entry=save('chronal_entry17',hit(2.2,True))
lap=save('chronal_lap17',hit(.95,False))
t=np.arange(round(1.5*rate))/rate;n=len(t);env=np.sin(np.pi*np.minimum(1,t/1.5))**1.3
rise=save('chronal_rise17',(sample('doorOpen_001',n,1.6,True)*.5+band(rng.normal(0,1,n),250,6500)*.18+np.sin(2*np.pi*(95*t+220*t*t))*.14)*env)
t=np.arange(round(6*rate))/rate;n=len(t)
texture=np.resize(read('spaceEngineLow_002'),n);field=np.resize(read('forceField_002')[::-1],n)
hum=sum(np.sin(2*np.pi*f*t+.18*np.sin(2*np.pi*2*t/6))*.13 for f in [36,54,72])
bed=(band(texture,45,1800)*.46+band(field,250,2600)*.18+hum)*(.78+.18*np.sin(2*np.pi*t/6))
save('chronal_loop17',np.column_stack([bed,np.roll(bed,211)+np.sin(2*np.pi*72.5*t)*.04]),True)
t=np.arange(round(4*rate))/rate;n=len(t);air=band(rng.normal(0,1,n),120,6100)*.13
engine=np.resize(read('engineCircular_002'),n);flange=air+np.roll(air,207)*.4
save('chronal_air17',np.column_stack([(flange+engine*.3)*(1+.24*np.sin(2*np.pi*t/4)),(np.roll(flange,391)+engine*.25)*(1-.24*np.sin(2*np.pi*t/4))]),True)
t=np.arange(round(1.2*rate))/rate;n=len(t)
save('chronal_exit17',(sample('thrusterFire_002',n,1.4,True)*.45+np.sin(2*np.pi*(140*t-40*t*t))*.25)*np.exp(-t*3))
t=np.arange(round(3*rate))/rate;n=len(t)
save('chronal_reveal17',hit(3,True)+sample('spaceEngineLarge_003',n,1.7)*np.exp(-t*2)*.42)
(S/'Audio-Audit17.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print('Designed17:',len(audit),'effects; peak',max(a['peak'] for a in audit))
