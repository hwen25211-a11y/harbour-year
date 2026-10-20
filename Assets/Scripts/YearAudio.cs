using UnityEngine;
namespace HarbourYear
{
    public class YearAudio:MonoBehaviour
    {
        static YearAudio instance;AudioSource music,click;
        public static bool Muted{get{return PlayerPrefs.GetInt("HarbourYear.AudioMuted",0)==1;}}
        public static void Start(GameObject parent){if(instance==null)instance=parent.AddComponent<YearAudio>();}
        void Awake()
        {
            instance=this;music=gameObject.AddComponent<AudioSource>();click=gameObject.AddComponent<AudioSource>();music.loop=true;music.volume=.12f;click.volume=.16f;
            const int rate=22050;int length=rate*32;var data=new float[length];int[] notes={60,64,67,71,57,60,64,67,53,57,60,64,55,59,62,67};
            for(int i=0;i<length;i++){float t=(float)i/rate;int beat=(int)(t*2);float local=t-(float)beat/2;float freq=440*Mathf.Pow(2,(notes[(beat/2)%16]-69)/12f);float env=Mathf.Exp(-local*7)*Mathf.Min(1,local*90);data[i]=.24f*env*(Mathf.Sin(2*Mathf.PI*freq*local)+.22f*Mathf.Sin(4*Mathf.PI*freq*local));}
            music.clip=AudioClip.Create("Original notebook study loop",length,1,rate,false);music.clip.SetData(data,0);music.Play();Apply();
            var sound=new float[1800];for(int i=0;i<sound.Length;i++)sound[i]=.12f*Mathf.Sin(2*Mathf.PI*660*i/rate)*Mathf.Exp(-i/300f);click.clip=AudioClip.Create("Original paper tap",sound.Length,1,rate,false);click.clip.SetData(sound,0);
        }
        void Apply(){music.mute=Muted;click.mute=Muted;}
        public static void Toggle(){PlayerPrefs.SetInt("HarbourYear.AudioMuted",Muted?0:1);PlayerPrefs.Save();if(instance!=null)instance.Apply();}
        public static void Tap(){if(instance!=null&&!Muted)instance.click.Play();}
    }
}
