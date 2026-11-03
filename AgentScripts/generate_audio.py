"""Original synthesized placeholders; replace WAV references with recordings for realism."""
import math, random, wave, struct
from pathlib import Path
root=Path('Assets/Audio/Designed');root.mkdir(parents=True,exist_ok=True)
rate=32000
def write(name,duration,fn,seed=1):
    rng=random.Random(seed);low=0;out=[]
    for i in range(int(duration*rate)):
        t=i/rate;n=rng.uniform(-1,1);low=low*.86+n*.14
        value=fn(t,n,low)*min(1,t*1500)*min(1,(duration-t)*100)
        out.append(int(max(-.98,min(.98,value))*32767))
    with wave.open(str(root/(name+'.wav')),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(struct.pack('<'+'h'*len(out),*out))
def pulse(t,at,width):return math.exp(-abs(t-at)*width)
for gun,freq,tail in [('Rifle',86,.42),('Pistol',150,.3),('SMG',120,.24),('DMR',63,.65)]:
    write(gun+'Fire',tail,lambda t,n,l,f=freq:(n*.65*math.exp(-t*110)+l*1.4*math.exp(-t*12)+math.sin(2*math.pi*(f*t-25*t*t))*.6*math.exp(-t*23)+n*.16*pulse(t,.07,45)),freq)
    duration=2.0 if gun=='Rifle' else 1.45 if gun=='Pistol' else 1.65 if gun=='SMG' else 2.5
    write(gun+'Reload',duration,lambda t,n,l,d=duration,f=freq:n*.6*(pulse(t,.08,110)+pulse(t,d*.55,80)+pulse(t,d*.84,110))+math.sin(t*2*math.pi*f*6)*.12*pulse(t,d*.84,65),freq+1)
    write(gun+'Empty',.13,lambda t,n,l,f=freq:(n*.65+math.sin(t*2*math.pi*f*8)*.18)*math.exp(-t*75),freq+2)
    write(gun+'Switch',.28,lambda t,n,l,f=freq:l*.7*pulse(t,.08,28)+(n*.4+math.sin(t*2*math.pi*f*5)*.12)*pulse(t,.2,90),freq+3)
for surface in ['Concrete','Grass','Metal']:
    for variant in range(5):
        def step(t,n,l,s=surface,v=variant):
            thud=math.sin(t*2*math.pi*(65+v*5))*.4*math.exp(-t*27)
            grit=n*.18*(math.exp(-t*37)+.35*pulse(t,.09,35))
            if s=='Grass':return thud*.55+l*.8*pulse(t,.045,25)+n*.15*math.exp(-t*15)
            if s=='Metal':return thud+grit+sum(math.sin(t*2*math.pi*f)*a*math.exp(-t*dec) for f,a,dec in [(430,.2,12),(710,.11,18),(1170,.08,26)])
            return thud+grit
        write(surface+'Step'+str(variant),.35,step,200+variant)
for name,f in [('ConcreteImpact',240),('MetalImpact',870),('GrassImpact',95),('FleshImpact',55),('Landing',75),('Hurt',95),('EnemyDeath',72),('Button',660)]:
    write(name,.32,lambda t,n,l,f=f:(n*.25+math.sin(t*2*math.pi*f)*.38+l*.45)*math.exp(-t*(12 if f==870 else 32)),f)
write('Ambience',12,lambda t,n,l:(l*.35+math.sin(t*2*math.pi*55)*.07+math.sin(t*2*math.pi*82.5)*.03)*(.8+.2*math.cos(t*math.pi/6)),14)
print('Generated',len(list(root.glob('*.wav'))),'original audio clips')
