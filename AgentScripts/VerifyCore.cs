using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CompoundFPS;
public static class VerifyCore
{
    static void Check(bool value,string label){if(!value)throw new Exception(label);}
    public static string Main()
    {
        var a=new AmmoState(30,7);for(int i=0;i<11;i++)Check(a.Spend(),"Spend");Check(a.magazine==19,"Magazine count");Check(a.Reload()==7 && a.magazine==26 && a.reserve==0,"Partial reserve transfer");Check(!a.CanReload,"Empty reserve");
        var b=new AmmoState(12,60);for(int i=0;i<12;i++)Check(b.Spend(),"Pistol spend");Check(!b.Spend() && b.magazine==0,"No negative ammunition");Check(b.Reload()==12 && b.magazine==12 && b.reserve==48,"Pistol reload conservation");
        var go=new GameObject("Health verification");
        try { var h=go.AddComponent<Damageable>();var head=new GameObject("Head");head.transform.SetParent(go.transform);var zone=head.AddComponent<HitZone>();zone.owner=h;zone.TakeDamage(20,Vector3.zero);Check(h.Health==60,"Headshot multiplier");Check(h.TakeDamage(100,Vector3.zero) && h.Dead,"Lethal damage");Check(!h.TakeDamage(100,Vector3.zero),"Death emitted once"); }
        finally{UnityEngine.Object.DestroyImmediate(go);}
        Check(NavMesh.SamplePosition(new Vector3(0,0,-28),out var start,2,NavMesh.AllAreas),"Spawn on NavMesh");
        Check(NavMesh.SamplePosition(new Vector3(0,0,29),out var exit,2,NavMesh.AllAreas),"Extraction on NavMesh");
        var path=new NavMeshPath();Check(NavMesh.CalculatePath(start.position,exit.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"Extraction reachable");
        Check(NavMesh.SamplePosition(new Vector3(-15,3,23),out var deck,2,NavMesh.AllAreas),"Deck on NavMesh");Check(NavMesh.CalculatePath(start.position,deck.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"Deck ramp reachable");
        int agents=0;foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAI>()){Check(ai.GetComponent<NavMeshAgent>().isOnNavMesh,"Guard on NavMesh");agents++;}
        Check(agents==8,"Eight guards");
        return "PASS: ammo depletion, partial/full reload conservation, headshots, single death, 8 guard NavMesh positions, complete paths to extraction and elevated deck.";
    }
}
