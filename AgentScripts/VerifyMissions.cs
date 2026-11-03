using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using CompoundFPS;
public static class VerifyMissions
{
    static int stage,level;static double next;static float reloadDeadline;static string report="";
    static void Check(bool b,string label){if(!b)throw new Exception(label);report+="PASS: "+label+"\n";}
    public static string Main(){stage=0;level=1;report="";next=0;EditorApplication.update+=Tick;return "Mission verification running; report: Temp/mission-verification.txt";}
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+.4;
        try
        {
            var g=GameManager.Instance;
            if(stage==0)
            {
                Check(g.LevelNumber==level,"Loaded mission "+level);g.Play();
                var enemies=UnityEngine.Object.FindObjectsByType<EnemyAI>();Check(enemies.Length==6+level*2,"Guard count "+enemies.Length);
                foreach(var ai in enemies){var agent=ai.GetComponent<NavMeshAgent>();Check(agent.isOnNavMesh,"Guard navigable "+ai.name);ai.enabled=false;ai.GetComponent<EnemyWeapon>().enabled=false;agent.isStopped=true;}
                Check(g.weapons.Current.definition.hasScope,"Rifle has scope");
                g.weapons.Equip(1);Check(!g.weapons.Current.definition.hasScope,"Pistol iron sights");g.weapons.Equip(0);
                g.SetState(GameState.Paused);Check(Time.timeScale==0,"Pause freezes time");g.Resume();
                var w=g.weapons.Current;var target=enemies[0].GetComponent<EnemyHealth>();
                var cam=g.playerCamera;var oldpos=cam.transform.position;var oldrot=cam.transform.rotation;
                cam.transform.position=target.transform.position+Vector3.up*.95f+target.transform.forward*2;
                cam.transform.LookAt(target.transform.position+Vector3.up*.95f);
                float hp=target.Health;Check(w.TryFire(),"Rifle fires");Check(target.Health<hp,"Bullet damages character");
                cam.transform.SetPositionAndRotation(oldpos,oldrot);
                int marks=0;foreach(Transform child in UnityEngine.Object.FindFirstObjectByType<ImpactPool>().transform)if(child.gameObject.activeSelf && child.name=="Blood mark")marks++;
                Check(marks>0,"Blood splatter activated after hit");Check(w.BeginReload(),"Reload starts");reloadDeadline=Time.time+w.definition.reloadDuration+.2f;stage=1;
            }
            else if(stage==1)
            {
                if(Time.time<reloadDeadline)return;
                Check(!g.weapons.Current.Reloading && g.weapons.Current.Ammo.magazine==g.weapons.Current.definition.magazineCapacity,"Timed reload completes");
                foreach(var h in UnityEngine.Object.FindObjectsByType<EnemyHealth>())h.TakeDamage(999,g.player.transform.position);
                Check(g.objectives.Eliminated==g.objectives.requiredEnemies,"Eliminations tracked");g.player.Teleport(g.objectives.extraction.position,Quaternion.identity);stage=2;
            }
            else if(stage==2)
            {
                Check(CombatEvents.State==GameState.Won,"Extraction wins mission "+level);
                if(level<3){Check(g.HasNextLevel,"Next mission available");g.NextLevel();level++;stage=0;next=EditorApplication.timeSinceStartup+1;}
                else{Check(!g.HasNextLevel,"Final mission ends campaign");g.Restart();stage=3;next=EditorApplication.timeSinceStartup+1;}
            }
            else if(stage==3){Check(CombatEvents.Playing && g.playerHealth.Health==100,"Restart resets player");g.playerHealth.TakeDamage(999,Vector3.zero);stage=4;}
            else if(stage==4){Check(CombatEvents.State==GameState.Lost,"Player death opens loss state");g.MainMenu();stage=5;next=EditorApplication.timeSinceStartup+1;}
            else {Check(CombatEvents.State==GameState.Menu,"Return to main menu");File.WriteAllText("Temp/mission-verification.txt",report+"ALL CHECKS PASSED\n");EditorApplication.update-=Tick;}
        }
        catch(Exception e){File.WriteAllText("Temp/mission-verification.txt",report+"FAILED: "+e);EditorApplication.update-=Tick;Debug.LogException(e);}
    }
}
