from pathlib import Path
import sys,json,numpy as np
import soundfile as sf
R=Path(__file__).resolve().parents[2];O=R/'Resources/RpgAudio/Designed09';O.mkdir(parents=True,exist_ok=True)
SR=48000;rng=np.random.default_rng(909);report=[]
def noise(t,lo,hi):
 f=np.fft.rfftfreq(len(t),1/SR);a=np.fft.rfft(rng.normal(size=len(t)));a*=np.minimum(1,f/lo)**3/(1+(f/hi)**6);x=np.fft.irfft(a,len(t));return x/(x.std()+1e-9)
def env(t,a,d):return np.minimum(1,np.maximum(0,t)/a)*np.exp(-np.maximum(0,t)/d)*(t>=0)
def write(name,x):
 x=np.tanh(x);x-=x.mean();x*=.78/(abs(x).max()+1e-9);y=np.column_stack([x,x*.9+np.roll(x,37)*.1]);y[:240]*=np.linspace(0,1,240)[:,None];y[-960:]*=np.linspace(1,0,960)[:,None];sf.write(O/(name+'.wav'),y,SR,subtype='PCM_16');report.append(dict(name=name,seconds=len(x)/SR,peak=float(abs(y).max()),rms=float(np.sqrt((y*y).mean()))))
t=np.arange(SR*3)/SR;u=t-.55
x=noise(t,220,4400)*np.minimum(t/.55,1)*np.exp(-t/2)*.08+noise(t,65,2200)*env(u,.008,.62)*.38+np.sin(2*np.pi*(47*u+180*.025*(1-np.exp(-np.maximum(u,0)/.025))))*env(u,.004,.45)*.3
x+=noise(t,2200,10500)*env(u,.001,.045)*.14+noise(t,90,1800)*env(t-.91,.01,.5)*.18
for delay,gain in [(int(.093*SR),.14),(int(.181*SR),.09)]:x[delay:]+=x[:-delay]*gain
write('ascend_ignite',x)
t=np.arange(round(SR*1.35))/SR;x=np.zeros_like(t)
for at,gain in [(0,1),(.055,.65)]:
 u=t-at;x+=gain*(noise(t,650,10000)*env(u,.0006,.019)*.22+noise(t,65,1600)*env(u,.002,.16)*.42+np.sin(2*np.pi*49*u)*env(u,.001,.18)*.27)
x+=noise(t,240,3400)*env(t-.025,.05,.32)*.15;write('sonic_boom',x)
(O/'Measurements.json').write_text(json.dumps(report,indent=2));print(report)
