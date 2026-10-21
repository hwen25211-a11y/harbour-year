using System.Collections.Generic;
using UnityEngine;
// 轻松版声音：开始页面/开场/建档一首音乐，房间另一首（短暂淡出淡入）；看旅行风景时暂停。默认关闭。
namespace HarbourYear.Lite
{
    public class LiteAudio:MonoBehaviour
    {
        public class Prefs{public bool enabled;public float bgm=0.25f,sfx=0.5f;}
        class Slot{public string file;public float gain;public int priority;public Slot(string f,float g,int p){file=f;gain=g;priority=p;}}
        static readonly Dictionary<string,Slot> Slots=new Dictionary<string,Slot>{
            {"bgm_room",new Slot("room_bgm",1,0)},{"bgm_menu",new Slot("start_intro_bgm",1,0)},{"sfx_open",new Slot("ui_open",0.8f,2)},{"sfx_close",new Slot("ui_close",0.8f,2)},
            {"sfx_click",new Slot("ui_open",0.35f,1)},{"sfx_page",new Slot("page_gain10db",0.9f,1)},{"sfx_event",new Slot("ui_open",0.6f,3)},{"sfx_money_up",new Slot("money_up",0.7f,3)},{"sfx_money_down",new Slot("ui_close",0.45f,3)}};
        const string PrefKey="HarbourYear.Lite.Audio";const float Fade=0.32f;
        public static LiteAudio Instance;public Prefs prefs=new Prefs();
        AudioSource music;readonly List<AudioSource> pool=new List<AudioSource>();readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();readonly Dictionary<string,float> resume=new Dictionary<string,float>(),last=new Dictionary<string,float>();
        string scene="menu",track,pending;float level=1,target=1;bool hidden;int lastPriority=-1;float lastAt=-10;
        public static LiteAudio Boot(GameObject host){if(Instance==null)Instance=host.AddComponent<LiteAudio>();return Instance;}
        void Awake(){Instance=this;music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;LoadPrefs();}
        void OnDestroy(){if(Instance==this)Instance=null;}
        AudioClip Clip(string slot){var f=Slots[slot].file;AudioClip c;if(!clips.TryGetValue(f,out c)){c=Resources.Load<AudioClip>("Lite/audio/"+f);clips[f]=c;}return c;}
        void LoadPrefs()
        {
            try{var o=Json.Parse(PlayerPrefs.GetString(PrefKey,"null")) as JObj;if(o!=null&&o.I("v")==2){prefs.enabled=o.B("enabled");prefs.bgm=Mathf.Clamp01((float)o.D("bgm"));prefs.sfx=Mathf.Clamp01((float)o.D("sfx"));}}catch{}
        }
        public void SavePrefs(){var o=new JObj();o["v"]=2.0;o["enabled"]=prefs.enabled;o["bgm"]=(double)prefs.bgm;o["sfx"]=(double)prefs.sfx;PlayerPrefs.SetString(PrefKey,Json.Write(o));PlayerPrefs.Save();Apply();}
        public void SetScene(string s){if(s==scene)return;scene=s;Apply();}
        void OnApplicationFocus(bool f){hidden=!f&&!Application.isEditor;Apply();}
        void OnApplicationPause(bool p){hidden=p;Apply();}
        string Wanted{get{return scene=="menu"?"bgm_menu":"bgm_room";}}
        void Apply()
        {
            bool on=prefs.enabled&&!hidden&&scene!="outdoor";
            if(!prefs.enabled||hidden)foreach(var s in pool)s.Stop();else foreach(var s in pool)if(s.isPlaying&&s.clip!=null)s.volume=prefs.sfx*GainOf(s);
            if(!on){if(music.isPlaying){if(track!=null)resume[track]=music.time;music.Pause();}pending=null;level=1;target=1;if(scene!="outdoor"&&track!=Wanted)Swap(Wanted,false);Volume();return;}
            if(track==null){Swap(Wanted,true);level=1;target=1;Volume();return;}
            if(track!=Wanted){if(!music.isPlaying){Swap(Wanted,true);level=0;target=1;}else{pending=Wanted;target=0;}return;}
            pending=null;target=1;if(!music.isPlaying)music.UnPause();if(!music.isPlaying)music.Play();Volume();
        }
        float GainOf(AudioSource s){foreach(var kv in Slots)if(Clip(kv.Key)==s.clip)return kv.Value.gain;return 1;}
        void Swap(string slot,bool play)
        {
            if(track!=null&&music.clip!=null)resume[track]=music.time;var c=Clip(slot);track=slot;music.clip=c;if(c==null)return;
            float t;music.time=resume.TryGetValue(slot,out t)&&t<c.length?t:0;if(play)music.Play();
        }
        void Volume(){if(track!=null)music.volume=prefs.bgm*Slots[track].gain*level;}
        void Update()
        {
            if(Mathf.Approximately(level,target)){if(pending!=null&&level<=0){Swap(pending,true);pending=null;target=1;}return;}
            level=Mathf.MoveTowards(level,target,Time.unscaledDeltaTime/Fade);Volume();
        }
        public bool Play(string slot)
        {
            if(!prefs.enabled||hidden||prefs.sfx<=0)return false;var s=Slots[slot];float now=Time.unscaledTime;float l;
            if(last.TryGetValue(slot,out l)&&now-l<0.09f)return false;if(lastPriority>s.priority&&now-lastAt<0.25f)return false;var c=Clip(slot);if(c==null)return false;
            AudioSource src=null;foreach(var p in pool)if(!p.isPlaying){src=p;break;}if(src==null){src=gameObject.AddComponent<AudioSource>();src.playOnAwake=false;pool.Add(src);}
            src.clip=c;src.volume=prefs.sfx*s.gain;src.Play();last[slot]=now;if(s.priority>=lastPriority||now-lastAt>=0.25f){lastPriority=s.priority;lastAt=now;}return true;
        }
        public static void Sfx(string slot){if(Instance!=null)Instance.Play(slot);}
    }
}
