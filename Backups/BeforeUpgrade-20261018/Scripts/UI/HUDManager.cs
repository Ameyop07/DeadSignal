using UnityEngine;
using TMPro;
namespace CompoundFPS
{
    public sealed class HUDManager : MonoBehaviour
    {
        public TMP_FontAsset font;
        GameManager game;
        GameObject hud;
        TextMeshProUGUI health,ammo,weapon,objective,reload,hit,damageDirection,extraction;
        RectTransform[] cross=new RectTransform[4];
        UnityEngine.UI.Image[] damageEdges=new UnityEngine.UI.Image[4];
        float hitUntil,nextText;bool kill;
        void OnEnable(){CombatEvents.HitConfirmed+=Confirm;}
        void OnDisable(){CombatEvents.HitConfirmed-=Confirm;}
        void Confirm(bool isKill){hitUntil=Time.unscaledTime+.18f;kill=isKill;}
        void Start()
        {
            game=GameManager.Instance;UIFactory.Font=font;
            hud=UIFactory.Rect("HUD",transform,Vector2.zero,Vector2.zero,Vector2.zero).gameObject;UIFactory.Fill((RectTransform)hud.transform);
            var op=UIFactory.Panel("Objective panel",hud.transform,new Vector2(0,1),new Vector2(45,-45),new Vector2(660,125),UIFactory.Ink);
            UIFactory.Text("Sector",op.transform,game.missionName+"   /   LIVE OPERATION",18,new Vector2(0,1),new Vector2(22,-16),new Vector2(620,30),UIFactory.Accent);
            objective=UIFactory.Text("Objective",op.transform,"",25,new Vector2(0,1),new Vector2(22,-56),new Vector2(620,60));
            var hp=UIFactory.Panel("Health panel",hud.transform,new Vector2(0,0),new Vector2(45,45),new Vector2(245,100),UIFactory.Ink);
            health=UIFactory.Text("Health",hp.transform,"",36,new Vector2(.5f,.5f),Vector2.zero,new Vector2(205,65));
            var ap=UIFactory.Panel("Ammo panel",hud.transform,new Vector2(1,0),new Vector2(-45,45),new Vector2(360,130),UIFactory.Ink);
            weapon=UIFactory.Text("Weapon",ap.transform,"",18,new Vector2(0,1),new Vector2(22,-15),new Vector2(325,32),UIFactory.Accent);
            ammo=UIFactory.Text("Ammo",ap.transform,"",44,new Vector2(0,0),new Vector2(22,15),new Vector2(325,65));
            reload=UIFactory.Text("Reload",hud.transform,"",24,new Vector2(.5f,0),new Vector2(0,170),new Vector2(420,45));reload.alignment=TextAlignmentOptions.Center;
            UIFactory.Text("Controls",hud.transform,"WASD MOVE   SHIFT SPRINT   SPACE JUMP   CTRL CROUCH   RMB AIM   R RELOAD   1 / 2 SWITCH",17,new Vector2(.5f,0),new Vector2(0,15),new Vector2(1180,30));
            for(int i=0;i<4;i++)cross[i]=UIFactory.Panel("Crosshair",hud.transform,new Vector2(.5f,.5f),Vector2.zero,i<2?new Vector2(8,2):new Vector2(2,8),Color.white).rectTransform;
            hit=UIFactory.Text("Hit marker",hud.transform,"X",30,new Vector2(.5f,.5f),Vector2.zero,new Vector2(45,45));hit.alignment=TextAlignmentOptions.Center;
            damageEdges[0]=UIFactory.Panel("Damage left",hud.transform,new Vector2(0,.5f),Vector2.zero,new Vector2(18,1080),Color.clear);
            damageEdges[1]=UIFactory.Panel("Damage right",hud.transform,new Vector2(1,.5f),Vector2.zero,new Vector2(18,1080),Color.clear);
            damageEdges[2]=UIFactory.Panel("Damage top",hud.transform,new Vector2(.5f,1),Vector2.zero,new Vector2(1920,18),Color.clear);
            damageEdges[3]=UIFactory.Panel("Damage bottom",hud.transform,new Vector2(.5f,0),Vector2.zero,new Vector2(1920,18),Color.clear);
            damageDirection=UIFactory.Text("Damage direction",hud.transform,"^",48,new Vector2(.5f,.5f),new Vector2(0,120),new Vector2(60,60),Color.red);damageDirection.alignment=TextAlignmentOptions.Center;
            extraction=UIFactory.Text("Extraction waypoint",hud.transform,"",22,new Vector2(.5f,.5f),Vector2.zero,new Vector2(280,55),UIFactory.Accent);extraction.alignment=TextAlignmentOptions.Center;
        }
        void Update()
        {
            if(!hud || !game)return;hud.SetActive(CombatEvents.Playing);if(!CombatEvents.Playing)return;
            var w=game.weapons.Current;if(!w || w.Ammo==null)return;
            if(Time.unscaledTime>nextText)
            {
                nextText=Time.unscaledTime+.1f;health.text=$"+  {game.playerHealth.Health:0}";ammo.text=$"{w.Ammo.magazine:00} <size=25>/ {w.Ammo.reserve}</size>";weapon.text=w.definition.displayName;objective.text=game.objectives.CurrentObjective;
                reload.text=w.Reloading?$"RELOADING  {w.ReloadProgress*100:0}%":w.Ammo.magazine==0?"R  /  RELOAD":"";
            }
            float gap=w.Aiming?3:7+game.player.Speed*1.1f+(game.player.Grounded?0:7)+w.ShotBloom*8;
            foreach(var piece in cross)piece.gameObject.SetActive(!(w.Aiming && w.definition.hasScope));
            cross[0].anchoredPosition=new Vector2(-gap,0);cross[1].anchoredPosition=new Vector2(gap,0);cross[2].anchoredPosition=new Vector2(0,gap);cross[3].anchoredPosition=new Vector2(0,-gap);
            hit.enabled=Time.unscaledTime<hitUntil;hit.color=kill?UIFactory.Accent:Color.white;
            float alpha=game.playerHealth.DamageFlash;foreach(var edge in damageEdges)edge.color=new Color(.85f,.08f,.03f,alpha*.75f);
            damageDirection.enabled=alpha>.05f;
            Vector3 d=game.playerHealth.LastDamageOrigin-game.player.transform.position;float angle=Vector3.SignedAngle(game.player.transform.forward,d,Vector3.up)*Mathf.Deg2Rad;
            damageDirection.rectTransform.anchoredPosition=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*120;damageDirection.rectTransform.localRotation=Quaternion.Euler(0,0,-angle*Mathf.Rad2Deg);
            extraction.enabled=game.objectives.Eliminated>=game.objectives.requiredEnemies;
            if(extraction.enabled)
            {
                Vector3 target=game.objectives.extraction.position+Vector3.up*1.6f;
                Vector3 viewport=game.playerCamera.WorldToViewportPoint(target);if(viewport.z<0){viewport.x=1-viewport.x;viewport.y=.15f;}
                var size=((RectTransform)hud.transform).rect.size;
                extraction.rectTransform.anchoredPosition=new Vector2((Mathf.Clamp(viewport.x,.12f,.88f)-.5f)*size.x,(Mathf.Clamp(viewport.y,.15f,.85f)-.5f)*size.y);
                extraction.text=$"EXTRACT\n{Vector3.Distance(game.player.transform.position,target):0} m";
            }
        }
    }
}
