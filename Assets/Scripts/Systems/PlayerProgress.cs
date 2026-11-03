using System;
using System.Collections.Generic;
using UnityEngine;
namespace CompoundFPS
{
    [Serializable] public sealed class ProgressData
    {
        public int version=1, money, unlockedLevel;
        public string equippedWeapon="";
        public List<string> owned=new List<string>();
        public bool Owns(WeaponDefinition w)=>w && (w.initiallyOwned || owned.Contains(w.id));
        public bool Buy(WeaponDefinition w){if(!w || Owns(w) || w.price<0 || money<w.price)return false;money-=w.price;owned.Add(w.id);return true;}
        public void Award(int value){money=(int)Math.Min(int.MaxValue,(long)money+Math.Max(0,value));}
        public bool Equip(WeaponDefinition w){if(!Owns(w))return false;equippedWeapon=w.id;return true;}
    }
    public static class PlayerProgress
    {
        const string Key="DeadSignal.Progress.v1";
        static ProgressData data;
        public static event Action Changed;
        public static ProgressData Data {get {if(data==null)Load();return data;}}
        public static bool IsIsolated {get;private set;}
        public static void UseTestProfile(){data=new ProgressData();IsIsolated=true;Changed?.Invoke();}
        public static void RestoreProfile(){IsIsolated=false;data=null;Load();Changed?.Invoke();}
        public static void Load()
        {
            try{data=JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(Key,""))??new ProgressData();}catch{data=new ProgressData();}
            data.money=Math.Max(0,data.money);data.unlockedLevel=Math.Max(0,data.unlockedLevel);data.owned=data.owned??new List<string>();
        }
        public static void Save(){if(!IsIsolated){PlayerPrefs.SetString(Key,JsonUtility.ToJson(Data));PlayerPrefs.Save();}Changed?.Invoke();}
        public static bool Buy(WeaponDefinition w){if(!Data.Buy(w))return false;Save();return true;}
        public static bool Equip(WeaponDefinition w){if(!Data.Equip(w))return false;Save();return true;}
        public static void UseDefault(){Data.equippedWeapon="";Save();}
        public static void Award(int value){Data.Award(value);Save();}
        public static void Complete(LevelConfiguration level,int count){Data.Award(level.completionReward);Data.unlockedLevel=Math.Max(Data.unlockedLevel,Math.Min(count-1,level.buildIndex+1));Save();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset(){data=null;Changed=null;IsIsolated=false;}
    }
}
