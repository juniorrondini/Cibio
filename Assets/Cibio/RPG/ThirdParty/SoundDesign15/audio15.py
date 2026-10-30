from pathlib import Path
import numpy as np
import wave, json
S=Path(__file__).parent
R=S.parent.parent
A=R/'Resources/RpgAudio/Designed15' if S.parent.name=='ThirdParty' else S/'FinalAudio';A.mkdir(parents=True,exist_ok=True)
S.mkdir(parents=True,exist_ok=True)
rate=48000;rng=np.random.default_rng(1515);audit=[]
def horn(t,freq):
    phase=np.cumsum(freq if isinstance(freq,np.ndarray) else np.full(len(t),freq))/rate*2*np.pi
    signal=sum(np.sin(phase*k+.035*k*np.sin(t*31))*(1/k**1.35) for k in range(1,14))
    breath=rng.normal(0,.035,len(t));return signal*.65+breath
def save(name,length,kind):
    t=np.arange(round(length*rate))/rate;x=np.zeros(len(t))
    if kind=='rise':
        env=(1-np.exp(-t*16))*np.exp(-t*1.8);x=horn(t,55+100*(1-np.exp(-t*5)))*env
        x+=rng.normal(0,.16,len(t))*np.exp(-t*5)+np.sin(2*np.pi*(40*t+8*np.exp(-t*8)))*np.exp(-t*3)
    elif kind=='reveal':
        env=np.minimum(1,t/.8)*np.minimum(1,(length-t)/1.4);x=(horn(t,98)+horn(t,147)*.4+horn(t,196)*.25)*env*.65
        x+=np.sin(2*np.pi*42*t)*env*.25
    elif kind=='blast':
        env=(1-np.exp(-t*70))*np.exp(-t*2.7);x=(horn(t,116+5*np.sin(t*11))+horn(t,173)*.27)*env
        x+=rng.normal(0,.17,len(t))*np.exp(-t*8)
    elif kind=='windup':x=horn(t,80+60*t/length)*np.sin(np.pi*t/length)**1.6*.7
    elif kind=='lap':x=(np.sin(2*np.pi*587*t)*np.exp(-t*10)+np.sin(2*np.pi*880*t)*np.exp(-t*13))*.4
    elif kind=='victory':
        for start,f in [(0,196),(.25,247),(.5,294),(.9,392)]:
            u=np.maximum(0,t-start);x+=horn(t,f)*np.where(t>=start,(1-np.exp(-u*30))*np.exp(-u*1.5),0)*.3
    else:x=horn(t,72+15*np.sin(t*4))*(1-np.exp(-t*40))*np.exp(-t*3)
    # Local reflections and a stereo tail; soft saturation and a fixed ceiling.
    dry=x.copy()
    for delay,gain in [(.071,.18),(.127,.12),(.223,.07)]:
        n=int(delay*rate);x[n:]+=dry[:-n]*gain
    x=np.tanh(x*.8);fade=np.minimum(1,t/.012)*np.minimum(1,(length-t)/.08);x*=fade
    left=x;right=np.roll(x,31)*.97;right[:31]=0;audio=np.column_stack([left,right]);audio*=.82/max(.82,float(np.max(np.abs(audio))))
    with wave.open(str(A/(name+'.wav')),'wb') as out:out.setnchannels(2);out.setsampwidth(2);out.setframerate(rate);out.writeframes((audio*32767).astype('<i2').tobytes())
    audit.append(dict(name=name,seconds=length,peak=float(np.abs(audio).max()),rms=float(np.sqrt(np.mean(audio**2)))))
for args in [('world_break15',2.4,'rise'),('world_reveal15',7.2,'reveal'),('orbit_lap15',.45,'lap'),('fusion_entry15',2.0,'blast'),('fusion_combo15',.65,'thud'),('horn_windup15',1.1,'windup'),('horn_blast15',1.6,'blast'),('fusion_dash15',1.1,'rise'),('fusion_phase15',2.1,'reveal'),('fusion_victory15',3.6,'victory')]:save(*args)
(S/'Audio-Audit15.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
(S/'README.md').write_text('# Sons da Convergência\n\nDez efeitos originais por síntese DSP, sem samples externos: série harmônica de instrumento de sopro, turbulência, subgrave, envelopes e reflexões curtas. Receita audio15.py, seed 1515, 48 kHz/16-bit estéreo. Pico máximo 0,82, preservando o limiter existente. Artefatos CC0. Não inclui áudio extraído de franquias.\n',encoding='utf-8')
# Recipe already lives with its sources.
print('Designed10 original sound effects:',len(audit))
