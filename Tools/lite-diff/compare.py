import json,sys
js=json.load(open(sys.argv[1]));cs=json.load(open(sys.argv[2]))
def diff(a,b,path,out):
    if len(out)>12:return
    if isinstance(a,dict) and isinstance(b,dict):
        for k in sorted(set(a)|set(b)):
            diff(a.get(k),b.get(k),path+'.'+k,out)
    elif isinstance(a,list) and isinstance(b,list):
        if len(a)!=len(b):out.append((path,'len %d vs %d'%(len(a),len(b))));
        for i,(x,y) in enumerate(zip(a,b)):diff(x,y,path+'[%d]'%i,out)
    elif isinstance(a,(int,float)) and isinstance(b,(int,float)) and not isinstance(a,bool) and not isinstance(b,bool):
        if abs(a-b)>1e-9:out.append((path,'%r vs %r'%(a,b)))
    elif a!=b:out.append((path,'%r\n      vs %r'%(a,b)))
bad=0
for j,c in zip(js,cs):
    assert j['name']==c['name']
    if j['trace']!=c['trace']:
        bad+=1;n=next((i for i,(x,y) in enumerate(zip(j['trace'],c['trace'])) if x!=y),min(len(j['trace']),len(c['trace'])))
        print(j['name'],'TRACE differs at',n,j['trace'][max(0,n-2):n+3],c['trace'][max(0,n-2):n+3])
    for sj,sc in zip(j['snaps'],c['snaps']):
        out=[];diff(sj,sc,'',out)
        if out:
            bad+=1;print(j['name'],sj['tag'],'DIFF');[print('   ',p,m) for p,m in out];break
    if len(j['snaps'])!=len(c['snaps']):bad+=1;print(j['name'],'snap count',len(j['snaps']),len(c['snaps']))
print('IDENTICAL' if not bad else 'MISMATCHES %d'%bad)
