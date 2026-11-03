using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Linq;
namespace CompoundFPS
{
    public sealed class MainMenu : MonoBehaviour
    {
        public TMP_FontAsset font;
        GameObject menu,pause,lost,won,settings;
        TextMeshProUGUI result;
        GameManager game;
        void Start()
        {
            game=GameManager.Instance;UIFactory.Font=font;
            if(!EventSystem.current){var e=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));e.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            menu=Page("Main Menu","DEAD\nSIGNAL","OPERATION  /  "+game.missionName,false);
            var navigation=GetComponent<ArmoryScreen>();
            var buttons=UIFactory.Stack(menu.transform,"Actions",new Vector2(0,0),new Vector2(100,95),new Vector2(600,390));
            UIFactory.Button(buttons,"PLAY  /  DEPLOY",game.Play);UIFactory.Button(buttons,"MISSIONS  /  SELECT LEVEL",()=>navigation.Show(false));UIFactory.Button(buttons,"ARMORY  /  GUN STORE",()=>navigation.Show(true));UIFactory.Button(buttons,"SETTINGS",()=>game.SettingsOpen=true);UIFactory.Button(buttons,"QUIT",game.Quit);
            var brief=UIFactory.Panel("Briefing",menu.transform,new Vector2(1,.5f),new Vector2(-100,0),new Vector2(550,380),UIFactory.Ink);
            UIFactory.Text("Brief title",brief.transform,"MISSION BRIEF",23,new Vector2(0,1),new Vector2(35,-35),new Vector2(480,40),UIFactory.Accent);
            UIFactory.Text("Brief",brief.transform,$"{game.missionName} has gone dark.\n\nBreach the gate. Neutralize {game.objectives.requiredEnemies} guards. Reach extraction.\n\nMISSION {game.LevelNumber} / 3\nEarn credits. Expand your armory.",25,new Vector2(0,1),new Vector2(35,-95),new Vector2(470,260));
            pause=Page("Pause","MISSION\nPAUSED",game.missionName+"  /  STANDBY",true);
            var ps=UIFactory.Stack(pause.transform,"Actions",new Vector2(0,0),new Vector2(100,100),new Vector2(550,310));
            UIFactory.Button(ps,"RESUME",game.Resume);UIFactory.Button(ps,"SETTINGS",()=>game.SettingsOpen=true);UIFactory.Button(ps,"RESTART",game.Restart);UIFactory.Button(ps,"MAIN MENU",game.MainMenu);
            lost=Page("Game Over","SIGNAL\nLOST","OPERATOR DOWN",true);
            var ls=UIFactory.Stack(lost.transform,"Actions",new Vector2(0,0),new Vector2(100,160),new Vector2(550,160));UIFactory.Button(ls,"RESTART",game.Restart);UIFactory.Button(ls,"MAIN MENU",game.MainMenu);
            won=Page("Mission Complete","MISSION\nCOMPLETE","EXTRACTION CONFIRMED",true);
            result=UIFactory.Text("Result",won.transform,"",26,new Vector2(0,.5f),new Vector2(105,-100),new Vector2(700,80));
            var ws=UIFactory.Stack(won.transform,"Actions",new Vector2(0,0),new Vector2(100,130),new Vector2(550,240));if(game.HasNextLevel)UIFactory.Button(ws,"NEXT LEVEL",game.NextLevel);UIFactory.Button(ws,"RESTART",game.Restart);UIFactory.Button(ws,"MAIN MENU",game.MainMenu);
            BuildSettings();ApplySavedGraphics();
        }
        GameObject Page(string name,string title,string eyebrow,bool dim)
        {
            var p=UIFactory.Panel(name,transform,Vector2.zero,Vector2.zero,Vector2.zero,new Color(.02f,.04f,.05f,dim?.85f:.28f));UIFactory.Fill(p.rectTransform);
            p.gameObject.AddComponent<UIFade>();
            UIFactory.Panel("Left shade",p.transform,new Vector2(0,.5f),Vector2.zero,new Vector2(830,1080),new Color(.025f,.05f,.06f,.88f));
            UIFactory.Panel("Accent",p.transform,new Vector2(0,1),new Vector2(100,-110),new Vector2(80,5),UIFactory.Accent);
            UIFactory.Text("Eyebrow",p.transform,eyebrow,22,new Vector2(0,1),new Vector2(100,-145),new Vector2(700,50),UIFactory.Accent);
            var t=UIFactory.Text("Title",p.transform,title,100,new Vector2(0,1),new Vector2(95,-230),new Vector2(780,280));t.fontStyle=FontStyles.Bold;t.lineSpacing=-10;
            UIFactory.Text("Footer",p.transform,"DEAD SIGNAL    /    TACTICAL OPERATIONS    /    CAMPAIGN",18,new Vector2(0,0),new Vector2(100,35),new Vector2(950,35),new Color(.6f,.7f,.7f));
            return p.gameObject;
        }
        void BuildSettings()
        {
            settings=UIFactory.Panel("Settings",transform,Vector2.zero,Vector2.zero,Vector2.zero,UIFactory.Ink).gameObject;UIFactory.Fill((RectTransform)settings.transform);
            UIFactory.Text("Title",settings.transform,"SETTINGS",60,new Vector2(.5f,1),new Vector2(0,-80),new Vector2(800,80));
            var stack=UIFactory.Stack(settings.transform,"Settings rows",new Vector2(.5f,.5f),new Vector2(0,-20),new Vector2(800,650),15);
            AddSlider(stack,"MOUSE SENSITIVITY",.03f,.25f,GameSettings.Sensitivity,v=>GameSettings.Sensitivity=v);
            AddSlider(stack,"MASTER VOLUME",0,1,GameSettings.Master,v=>GameSettings.Master=v);
            AddSlider(stack,"MUSIC / AMBIENCE",0,1,GameSettings.Music,v=>GameSettings.Music=v);
            AddSlider(stack,"SFX VOLUME",0,1,GameSettings.SFX,v=>GameSettings.SFX=v);
            UnityEngine.UI.Button quality=null;quality=UIFactory.Button(stack,"",()=>{int i=(GraphicsPresets.Current+1)%3;GraphicsPresets.Apply(i,game.catalog);quality.GetComponentInChildren<TextMeshProUGUI>().text="QUALITY  /  "+GraphicsPresets.Names[i];});quality.GetComponentInChildren<TextMeshProUGUI>().text="QUALITY  /  "+GraphicsPresets.Names[GraphicsPresets.Current];
            UnityEngine.UI.Button full=null;full=UIFactory.Button(stack,"FULLSCREEN  /  "+Screen.fullScreen,()=>{bool value=!Screen.fullScreen;Screen.fullScreen=value;PlayerPrefs.SetInt("FPS.Fullscreen",value?1:0);full.GetComponentInChildren<TextMeshProUGUI>().text="FULLSCREEN  /  "+value;});
            var resolutions=Screen.resolutions.Select(r=>new Vector2Int(r.width,r.height)).Distinct().Where(r=>r.x>=1280).ToArray();if(resolutions.Length==0)resolutions=new[]{new Vector2Int(1920,1080)};
            int selected=System.Array.FindIndex(resolutions,r=>r.x==Screen.width && r.y==Screen.height);if(selected<0)selected=0;
            UnityEngine.UI.Button resolution=null;resolution=UIFactory.Button(stack,$"RESOLUTION  /  {Screen.width} x {Screen.height}",()=>{selected=(selected+1)%resolutions.Length;var r=resolutions[selected];Screen.SetResolution(r.x,r.y,Screen.fullScreen);PlayerPrefs.SetInt("FPS.Width",r.x);PlayerPrefs.SetInt("FPS.Height",r.y);resolution.GetComponentInChildren<TextMeshProUGUI>().text=$"RESOLUTION  /  {r.x} x {r.y}";});
            UIFactory.Button(stack,"BACK",()=>{game.SettingsOpen=false;PlayerPrefs.Save();});
        }
        void AddSlider(Transform parent,string label,float min,float max,float initial,UnityEngine.Events.UnityAction<float> changed)
        {
            var row=UIFactory.Rect(label,parent,Vector2.zero,Vector2.zero,new Vector2(800,55));row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=55;
            UIFactory.Text("Label",row,label,20,new Vector2(0,.5f),new Vector2(0,0),new Vector2(330,40));
            var bg=UIFactory.Panel("Slider",row,new Vector2(1,.5f),Vector2.zero,new Vector2(430,18),new Color(.15f,.23f,.24f));bg.raycastTarget=true;
            var fill=UIFactory.Panel("Fill",bg.transform,Vector2.zero,Vector2.zero,new Vector2(430,18),UIFactory.Accent);UIFactory.Fill(fill.rectTransform);
            var handle=UIFactory.Panel("Handle",bg.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(18,32),Color.white);handle.raycastTarget=true;
            var slider=bg.gameObject.AddComponent<UnityEngine.UI.Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=min;slider.maxValue=max;slider.value=initial;slider.onValueChanged.AddListener(changed);
        }
        void ApplySavedGraphics()
        {
            GraphicsPresets.Apply(GraphicsPresets.Current,game.catalog,false);
            if(!Application.isEditor && PlayerPrefs.HasKey("FPS.Width"))Screen.SetResolution(PlayerPrefs.GetInt("FPS.Width"),PlayerPrefs.GetInt("FPS.Height"),PlayerPrefs.GetInt("FPS.Fullscreen",1)==1);
            else if(!Application.isEditor && PlayerPrefs.HasKey("FPS.Fullscreen"))Screen.fullScreen=PlayerPrefs.GetInt("FPS.Fullscreen")==1;
        }
        void Update()
        {
            if(!game || !menu)return;
            var state=CombatEvents.State;bool s=game.SettingsOpen;
            menu.SetActive(!s && !game.NavigationOpen && state==GameState.Menu);pause.SetActive(!s && state==GameState.Paused);lost.SetActive(!s && state==GameState.Lost);won.SetActive(!s && state==GameState.Won);settings.SetActive(s);
            if(state==GameState.Won)result.text=$"{game.objectives.Eliminated} GUARDS NEUTRALIZED   /   {game.MissionTime:0}s";
        }
    }
}
