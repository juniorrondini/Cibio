"""Original layered sound design. CC0 source recordings listed in Audio-05.txt."""
from pathlib import Path
import sys,json,wave
import numpy as np
sys.path.insert(0,str(Path(__file__).parent/'audio_libs'))
import soundfile as sf
W=Path(__file__).parent; S=W if W.name=='SoundDesign05' else W/'RpgResearch/Audio05'; R=W.parent.parent if W.name=='SoundDesign05' else W/'Cibio05/Assets/Cibio/RPG'; O=R/'Resources/RpgAudio/Designed05'; O.mkdir(parents=True,exist_ok=True)
SR=48000; rng=np.random.default_rng(51029); report=[]
def tsec(d):return np.arange(round(d*SR))/SR
def noise(d,lo,hi):
    n=round(d*SR); f=np.fft.rfftfreq(n,1/SR); spectrum=np.fft.rfft(rng.normal(size=n)); spectrum*=np.minimum(1,f/max(1,lo))**3/(1+(f/hi)**6); x=np.fft.irfft(spectrum,n);return x/(np.std(x)+1e-9)
def read(p,speed=1):
    x,s=sf.read(str(p),always_2d=True);x=x.mean(axis=1); idx=np.linspace(0,len(x)-1,round(len(x)/s*SR/speed));return np.interp(idx,np.arange(len(x)),x)
def fit(x,d):
    y=np.zeros(round(d*SR));y[:min(len(x),len(y))]=x[:len(y)];return y
def env(t,attack,decay):return np.minimum(1,t/attack)*np.exp(-t/decay)
def echo(x,delay=.065,gain=.24):
    y=x.copy()
    for j in range(1,5):
        n=round(delay*j*SR)
        if n<len(x):y[n:]+=x[:-n]*gain**j
    return y
def save(name,x,loop=False):
    # Frequency layers share headroom; smooth saturation rather than hard clipping.
    x=np.tanh(x*.85);x-=x.mean();x*=.89/(np.max(np.abs(x))+1e-9)
    if loop:
        # Periodic FFT noise + periodic modulations, enforce continuity in all layers.
        fade=round(.08*SR);a=np.linspace(0,1,fade); x[-fade:]=x[-fade:]*(1-a)+x[:fade]*a
        x[-1]=x[0]
    else:
        n=min(round(.008*SR),len(x)//4);x[:n]*=np.linspace(0,1,n);x[-n:]*=np.linspace(1,0,n)
    stereo=np.stack((x, np.roll(x,67)*.86+x*.14),axis=1)
    if loop:
        # Close the stereo microdelay boundary as well, over 2.7ms.
        a=np.linspace(0,1,128)[:,None];stereo[-128:]=stereo[-128:]*(1-a)+stereo[0]*a
    if not loop:stereo[:67,1]=x[:67]*.14;stereo[-400:]*=np.linspace(1,0,400)[:,None]
    sf.write(str(O/(name+'.wav')),stereo,SR,subtype='PCM_16')
    report.append(dict(name=name,seconds=len(x)/SR,peak=float(abs(stereo).max()),rms=float(np.sqrt(np.mean(stereo**2))),loop=loop,boundary=float(abs(stereo[0]-stereo[-1]).max())))
T=tsec(8);beat=(.78+.12*np.sin(2*np.pi*2*T)+.1*np.sin(2*np.pi*3.875*T))
save('charge_bass',(.28*np.sin(2*np.pi*55*T)+.18*np.sin(2*np.pi*82.5*T)+.09*np.sin(2*np.pi*110*T)+.17*noise(8,24,190))*beat,True)
save('charge_air',noise(8,130,2600)*(.25+.09*np.sin(2*np.pi*.625*T)+.06*np.sin(2*np.pi*5.125*T)),True)
crackle=read(S/'crackleelectricityloop.wav');crackle=np.tile(crackle,int(np.ceil(len(T)/len(crackle))))[:len(T)]
save('charge_electric',crackle*.42+noise(8,1300,6200)*(.025+.07*np.maximum(0,np.sin(2*np.pi*11*T))**8),True)
save('aura_sustain',noise(8,80,1800)*.13+.16*np.sin(2*np.pi*55*T)*(.8+.2*np.sin(2*np.pi*1.25*T)),True)
for i in range(3):
    d=.24+i*.025;t=tsec(d);sw=noise(d,350,3100)*np.sin(np.pi*np.minimum(t/d,1))**2
    save('punch_swing'+str(i),sw*.42)
    d=.62+i*.12;t=tsec(d);real=fit(read(S/'hits'/('hit%02d.mp3.flac'%([4,12,25][i])),.95-i*.06),d)
    # Remove leading silence to align impact exactly with the animated contact frame.
    ids=np.flatnonzero(abs(real)>.03)
    if len(ids):real=fit(real[max(0,ids[0]-240):],d)
    phase=2*np.pi*(45*t+135*.035*(1-np.exp(-t/.035)))
    thump=np.sin(phase)*env(t,.0015,.095+i*.018)
    crack=noise(d,700,6500)*env(t,.0008,.018)
    x=real*.72+thump*.46+crack*.16
    save('punch_hit'+str(i),echo(x,.038,.18))
    save('power_hit'+str(i),echo(x*1.3+noise(d,70,1600)*env(t,.003,.16)*.2+fit(read(S/'groundhit.wav',.8),d)*.22,.07,.25))
d=2.8;t=tsec(d);u=np.maximum(0,t-.24)
impact=np.sin(2*np.pi*(42*u+190*.04*(1-np.exp(-u/.04))))*env(u,.001,.24)*(t>=.24)
roar=noise(d,90,3200)*env(u,.025,.64)*(t>=.24)
vacuum=noise(d,450,2800)*np.minimum(1,t/.23)**2*(t<.24)
boom=fit(np.r_[np.zeros(round(.24*SR)),read(S/'deathboom.wav',1.25)],d)
save('transform_ignite',echo(.68*impact+.36*roar+.10*vacuum+.28*boom,.115,.32))
d=1.4;t=tsec(d);save('wave_blast',echo(noise(d,120,2200)*env(t,.02,.24)*.4+np.sin(2*np.pi*(48*t+75*.06*(1-np.exp(-t/.06))))*env(t,.003,.17)*.35,.09,.3))
d=.5;t=tsec(d);save('charge_release',noise(d,100,1200)*env(t,.01,.12)*.25)
(O/'Audio-Measurements.json').write_text(json.dumps(report,indent=2))
assert all(a['peak']<.95 for a in report)
assert all(a['boundary']<.0001 for a in report if a['loop'])
print(json.dumps(report,indent=2))
