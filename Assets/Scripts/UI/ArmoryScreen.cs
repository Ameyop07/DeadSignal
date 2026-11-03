using UnityEngine;
using TMPro;
namespace CompoundFPS
{
    public sealed class ArmoryScreen : MonoBehaviour
    {
        GameManager game;GameObject store,missions;TextMeshProUGUI balance,menuBalance,message;
        TextMeshProUGUI[] statuses,actions;UnityEngine.UI.Button[] buttons;bool shop;
        void OnEnable(){PlayerProgress.Changed+=Refresh;}
        void OnDisable(){PlayerProgress.Changed-=Refresh;}
        void Start()
        {
            game=GameManager.Instance;if(!game.catalog)return;
            menuBalance=UIFactory.Text("Persistent balance",transform,"",24,new Vector2(1,1),new Vector2(-55,-30),new Vector2(470,40),UIFactory.Accent);menuBalance.alignment=TextAlignmentOptions.Right;
            store=Page("ARMORY","SELECT YOUR ADVANTAGE");missions=Page("MISSIONS","CHOOSE YOUR NEXT OPERATION");
            balance=UIFactory.Text("Armory balance",store.transform,"",32,new Vector2(1,1),new Vector2(-100,-65),new Vector2(500,60),UIFactory.Accent);balance.alignment=TextAlignmentOptions.Right;
            int count=game.catalog.weapons.Length;statuses=new TextMeshProUGUI[count];actions=new TextMeshProUGUI[count];buttons=new UnityEngine.UI.Button[count];
            for(int i=0;i<count;i++)
            {
                int index=i;var w=game.catalog.weapons[i];float x=(i-(count-1)*.5f)*410;
                var card=UIFactory.Panel("Weapon "+w.id,store.transform,new Vector2(.5f,.5f),new Vector2(x,0),new Vector2(385,625),new Color(.07f,.11f,.15f));
                UIFactory.Panel("Accent",card.transform,new Vector2(0,1),Vector2.zero,new Vector2(385,4),UIFactory.Accent);
                UIFactory.Text("Weapon name",card.transform,w.displayName,27,new Vector2(0,1),new Vector2(25,-28),new Vector2(335,70));
                var preview=UIFactory.Panel("Weapon preview",card.transform,new Vector2(.5f,1),new Vector2(0,-105),new Vector2(335,175),Color.white);preview.sprite=w.preview;preview.preserveAspect=true;
                UIFactory.Text("Statistics",card.transform,$"DAMAGE                 {w.damage:0}\nFIRE RATE              {w.fireRate:0.#} / s\nMAGAZINE               {w.magazineCapacity}\nRELOAD                  {w.reloadDuration:0.#} s",22,new Vector2(0,1),new Vector2(25,-305),new Vector2(335,150));
                statuses[i]=UIFactory.Text("Ownership",card.transform,"",21,new Vector2(0,0),new Vector2(25,105),new Vector2(335,55),UIFactory.Accent);
                buttons[i]=UIFactory.Button(card.transform,"",()=>PurchaseOrEquip(index));var r=(RectTransform)buttons[i].transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,25);r.sizeDelta=new Vector2(335,65);actions[i]=buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            }
            var auto=UIFactory.Button(store.transform,"USE LEVEL DEFAULT",()=>{PlayerProgress.UseDefault();game.weapons.EquipLoadout();message.text="Loadout follows each mission's default weapon.";});Place(auto,new Vector2(1,0),new Vector2(-100,55),new Vector2(400,65));
            message=UIFactory.Text("Store status",store.transform,"",22,new Vector2(.5f,0),new Vector2(0,70),new Vector2(700,50));message.alignment=TextAlignmentOptions.Center;
            for(int i=0;i<game.catalog.levels.Length;i++)
            {
                int index=i;var level=game.catalog.levels[i];bool unlocked=i<=PlayerProgress.Data.unlockedLevel;
                var card=UIFactory.Panel("Mission "+i,missions.transform,new Vector2(.5f,.5f),new Vector2((i-1)*520,0),new Vector2(475,580),new Color(.07f,.11f,.15f));
                UIFactory.Text("Number",card.transform,$"0{i+1}",100,new Vector2(0,1),new Vector2(30,-20),new Vector2(400,130),unlocked?UIFactory.Accent:new Color(.4f,.45f,.5f));
                UIFactory.Text("Title",card.transform,level.title,32,new Vector2(0,1),new Vector2(30,-160),new Vector2(415,85));
                UIFactory.Text("Briefing",card.transform,$"{level.briefing}\n\nDEFAULT: {level.defaultWeapon.displayName}\nELIMINATION: +{level.killReward} CR\nCOMPLETION: +{level.completionReward} CR",23,new Vector2(0,1),new Vector2(30,-255),new Vector2(415,210));
                var b=UIFactory.Button(card.transform,unlocked?"SELECT MISSION":"LOCKED / COMPLETE PREVIOUS",()=>{if(game.LevelNumber==index+1)game.NavigationOpen=false;else game.LoadLevel(index,false);});Place(b,new Vector2(.5f,0),new Vector2(0,25),new Vector2(415,65));b.interactable=unlocked;
            }
            store.SetActive(false);missions.SetActive(false);Refresh();
        }
        GameObject Page(string title,string subtitle)
        {
            var p=UIFactory.Panel(title,transform,Vector2.zero,Vector2.zero,Vector2.zero,UIFactory.Ink);UIFactory.Fill(p.rectTransform);p.raycastTarget=true;p.gameObject.AddComponent<UIFade>();
            UIFactory.Text("Heading",p.transform,title,55,new Vector2(0,1),new Vector2(100,-60),new Vector2(800,80));UIFactory.Text("Subtitle",p.transform,subtitle,21,new Vector2(0,1),new Vector2(104,-145),new Vector2(1000,40),UIFactory.Accent);
            var back=UIFactory.Button(p.transform,"BACK",()=>game.NavigationOpen=false);Place(back,new Vector2(0,0),new Vector2(100,55),new Vector2(280,65));return p.gameObject;
        }
        static void Place(UnityEngine.UI.Button b,Vector2 anchor,Vector2 position,Vector2 size){var r=(RectTransform)b.transform;r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;}
        void PurchaseOrEquip(int i)
        {
            var w=game.catalog.weapons[i];if(!PlayerProgress.Data.Owns(w)){if(!PlayerProgress.Buy(w)){message.text="Not enough money. Complete missions to earn credits.";return;}message.text="Purchased "+w.displayName+". Select Equip to use it.";}
            else{PlayerProgress.Equip(w);game.weapons.EquipLoadout();message.text="Equipped "+w.displayName+" for future missions.";}Refresh();
        }
        public void Show(bool armory){shop=armory;game.NavigationOpen=true;Refresh();}
        void Refresh()
        {
            if(!game || statuses==null)return;balance.SetText("{0:0}  CR",PlayerProgress.Data.money);menuBalance.SetText("BALANCE  /  {0:0} CR",PlayerProgress.Data.money);
            for(int i=0;i<statuses.Length;i++){var w=game.catalog.weapons[i];bool owned=PlayerProgress.Data.Owns(w),equipped=PlayerProgress.Data.equippedWeapon==w.id;statuses[i].text=owned?"OWNED":$"PRICE  /  {w.price} CR";actions[i].text=equipped?"EQUIPPED":owned?"EQUIP":PlayerProgress.Data.money>=w.price?"BUY":"NOT ENOUGH MONEY";buttons[i].interactable=!equipped;}
        }
        void Update(){if(!game || !store)return;store.SetActive(game.NavigationOpen && shop);missions.SetActive(game.NavigationOpen && !shop);menuBalance.gameObject.SetActive(CombatEvents.State==GameState.Menu && !game.NavigationOpen && !game.SettingsOpen);}
    }
}
