"""Reproducible CC0 recording layers and original DSP, no franchise audio."""
from pathlib import Path
import sys,json,numpy as np
W=Path(__file__).parent;R=W.parent.parent;sys.path.insert(0,str(W/'audio_libs'));import soundfile as sf
O=R/'Resources/RpgAudio/Designed07';O.mkdir(parents=True,exist_ok=True);old=R/'Resources/RpgAudio/Designed05';SR=48000;rng=np.random.default_rng(707);report=[]
def time(d):return np.arange(round(d*SR))/SR
def noise(d,lo,hi):
 n=round(d*SR);f=np.fft.rfftfreq(n,1/SR);a=np.fft.rfft(rng.normal(size=n));a*=np.minimum(1,f/max(1,lo))**3/(1+(f/hi)**6);x=np.fft.irfft(a,n);return x/(np.std(x)+1e-9)
def read(name,d,rate=1):
 x,s=sf.read(old/(name+'.wav'));x=x.mean(axis=1);y=np.interp(np.arange(round(d*SR))*s*rate/SR,np.arange(len(x)),x,left=0,right=0);return y
def env(t,a,d):return np.minimum(1,t/a)*np.exp(-t/d)
def save(name,x,loop=False):
 x=np.tanh(x*.85);x-=x.mean();x*=.78/(np.max(abs(x))+1e-9);y=np.column_stack((x,.88*x+.12*np.roll(x,43)))
 if loop:
  n=4096;u=np.linspace(0,1,n)[:,None];y[-n:]=y[-n:]*(1-u)+y[0]*u
 else:
  n=240;y[:n]*=np.linspace(0,1,n)[:,None];y[-480:]*=np.linspace(1,0,480)[:,None]
 sf.write(O/(name+'.wav'),y,SR,subtype='PCM_16');report.append(dict(name=name,seconds=len(x)/SR,peak=float(abs(y).max()),rms=float(np.sqrt((y*y).mean())),seam=float(abs(y[0]-y[-1]).max()) if loop else None))
t=time(6);save('flight_wind',noise(6,100,2100)*(.17+.035*np.sin(2*np.pi*1.5*t))+.06*np.sin(2*np.pi*66*t),True)
for name,d,up in [('flight_launch',.7,True),('flight_land',.55,False),('wave_windup',.22,True),('power_surge',.48,False)]:
 t=time(d);sweep=np.sin(2*np.pi*(75*t+(240 if up else -35)*t*t));x=noise(d,180,3200)*np.sin(np.pi*t/d)**1.3*.24+sweep*env(t,.02,.13)*.13
 if name=='flight_land':x+=read('punch_hit0',d,.7)*.3
 save(name,x)
for i in range(3):
 d=.42+i*.06;t=time(d);p=read('punch_hit'+str(i),d,1.13);snap=noise(d,1200,6500)*env(t,.0007,.013);chest=np.sin(2*np.pi*(63*t+110*.021*(1-np.exp(-t/.021))))*env(t,.001,.065)
 save('punch_hit'+str(i),p*.7+snap*.11+chest*.2)
 d=.64+i*.09;t=time(d);base=read('power_hit'+str(i),d,1.03);roar=noise(d,180,1800)*env(t,.006,.12+i*.03);sub=np.sin(2*np.pi*(49*t+80*.03*(1-np.exp(-t/.03))))*env(t,.001,.12)
 x=base*.7+roar*.22+sub*.23
 for k,g in [(int(SR*.047),.16),(int(SR*.091),.08)]:x[k:]+=x[:-k]*g
 save('power_hit'+str(i),x)
t=time(1.8);x=read('wave_blast',1.8,.8)*.7+noise(1.8,75,2400)*env(t,.006,.3)*.28+np.sin(2*np.pi*(43*t+160*.04*(1-np.exp(-t/.04))))*env(t,.002,.2)*.26
save('wave_awakened',x)
t=time(3.0);u=np.maximum(0,t-.24);gate=t>=.24;x=read('transform_ignite',3)*.8+noise(3,170,1700)*env(u,.005,.5)*gate*.19+np.sin(2*np.pi*58*u)*env(u,.003,.23)*gate*.18
save('transform_ignite',x)
(O/'Measurements.json').write_text(json.dumps(report,indent=2),encoding='utf-8');assert max(p['peak'] for p in report)<.8;assert all(p['seam']<.0001 for p in report if p['seam'] is not None)
print(json.dumps(report,indent=2))
