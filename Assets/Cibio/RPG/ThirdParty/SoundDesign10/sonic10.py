from pathlib import Path
import sys,json,shutil,numpy as np
import soundfile as sf
R=Path(__file__).resolve().parents[2];O=R/'Resources/RpgAudio/Designed10';O.mkdir(parents=True,exist_ok=True);S=R/'ThirdParty/SoundDesign10';S.mkdir(parents=True,exist_ok=True)
SR=48000;rng=np.random.default_rng(1010);report=[]
def noise(t,lo,hi):
 f=np.fft.rfftfreq(len(t),1/SR);a=np.fft.rfft(rng.normal(size=len(t)));a*=np.minimum(1,f/lo)**3/(1+(f/hi)**8);x=np.fft.irfft(a,len(t));return x/(x.std()+1e-9)
def env(t,attack,decay):return np.minimum(1,np.maximum(0,t)/attack)*np.exp(-np.maximum(0,t)/decay)*(t>=0)
def save(name,x,loop=False):
 x=np.tanh(x*1.15);x-=x.mean();x*=.94/(abs(x).max()+1e-9);y=np.column_stack([x,x*.86+np.roll(x,61)*.14])
 if loop:
  n=4096;u=np.linspace(0,1,n)[:,None];y[-n:]=y[-n:]*(1-u)+y[0]*u
 else:y[:48]*=np.linspace(0,1,48)[:,None];y[-1440:]*=np.linspace(1,0,1440)[:,None]
 sf.write(O/(name+'.wav'),y,SR,subtype='PCM_16');report.append(dict(name=name,seconds=len(x)/SR,peak=float(abs(y).max()),rms=float(np.sqrt((y*y).mean())),first_250ms_rms=float(np.sqrt((y[:12000]**2).mean())),loop=loop))
t=np.arange(SR*2)/SR;x=np.zeros_like(t)
for delay,gain in [(0,1),(.042,.78)]:
 u=t-delay;sub=np.sin(2*np.pi*(33*u+25*.11*(1-np.exp(-np.maximum(0,u)/.11))))
 chest=np.sin(2*np.pi*(92*u+45*.035*(1-np.exp(-np.maximum(0,u)/.035))))
 x+=gain*(noise(t,1600,11000)*env(u,.0005,.022)*.36+noise(t,90,2600)*env(u,.0015,.34)*.74+sub*env(u,.001,.52)*.72+chest*env(u,.001,.17)*.32)
x+=noise(t,40,800)*env(t-.065,.04,.62)*.25+noise(t,400,4200)*env(t-.10,.07,.30)*.15
for delay,gain in [(int(.117*SR),.12),(int(.23*SR),.065)]:x[delay:]+=x[:-delay]*gain
save('sonic_boom',x)
t=np.arange(SR*4)/SR;save('sonic_wind',noise(t,100,2000)*(.22+.025*np.sin(2*np.pi*3*t))+.10*np.sin(2*np.pi*57*t),True)
(O/'Measurements.json').write_text(json.dumps(report,indent=2));(S/'README.md').write_text('Sons originais por síntese DSP, CC0 1.0. Ruptura com dois transientes separados por42ms, subgrave descendente58→33Hz, corpo grave, estalo agudo e reverberação curta; loop de vento supersônico separado. 48kHz estéreo; seed1010. Sem áudio extraído de franquias. O jogo destaca o boom no mix por320ms e mantém o limiter. Receita requer numpy e soundfile.\n',encoding='utf-8');print(json.dumps(report,indent=2))
