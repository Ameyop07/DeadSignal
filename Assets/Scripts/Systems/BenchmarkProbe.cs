using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Profiling;
using Unity.Profiling;
namespace CompoundFPS
{
    // Runs only when explicitly launched with -fps-benchmark. Uses an isolated save profile.
    public sealed class BenchmarkProbe : MonoBehaviour
    {
        readonly List<double> frames=new List<double>(16000),cpu=new List<double>(16000),gpu=new List<double>(16000);
        readonly List<string> checks=new List<string>();
        readonly FrameTiming[] timing=new FrameTiming[1];
        ProfilerRecorder main,gc,draw,memory;long maxGc,maxDraw,maxMemory;bool sampling;string folder;int preset=1;double loadStart;
        Keyboard testKeyboard;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Bootstrap()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-fps-benchmark")<0)return;
            var g=new GameObject("Standalone verification");DontDestroyOnLoad(g);g.AddComponent<BenchmarkProbe>();
        }
        void Check(bool b,string text){string result=(b?"PASS: ":"FAIL: ")+text;checks.Add(result);File.AppendAllText(Path.Combine(folder,"checks.txt"),result+"\n");if(!b)Debug.LogError("Verification: "+text);}
        void Start()
        {
            folder=Path.Combine(Application.dataPath,"../Verification");Directory.CreateDirectory(folder);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-fps-quality");if(at>=0 && at+1<args.Length)int.TryParse(args[at+1],out preset);preset=Mathf.Clamp(preset,0,2);
            PlayerProgress.UseTestProfile();Screen.SetResolution(1920,1080,FullScreenMode.Windowed);Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            testKeyboard=InputSystem.AddDevice<Keyboard>();testKeyboard.MakeCurrent();File.WriteAllText(Path.Combine(folder,"checks.txt"),"");
            main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);draw=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);memory=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory",1);
            StartCoroutine(Run());
        }
        void LateUpdate()
        {
            if(!sampling)return;frames.Add(Time.unscaledDeltaTime*1000);if(main.Valid)cpu.Add(main.LastValue/1e6);if(gc.Valid)maxGc=Math.Max(maxGc,gc.LastValue);if(draw.Valid)maxDraw=Math.Max(maxDraw,draw.LastValue);if(memory.Valid)maxMemory=Math.Max(maxMemory,memory.LastValue);
            FrameTimingManager.CaptureFrameTimings();if(FrameTimingManager.GetLatestTimings(1,timing)>0 && timing[0].gpuFrameTime>0)gpu.Add(timing[0].gpuFrameTime);
        }
        IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(3);
            var catalog=GameManager.Instance.catalog;var progress=PlayerProgress.Data;var premium=catalog.weapons[2];
            Check(catalog.weapons.All(w=>w.preview && w.shootingSound && w.reloadSound && w.emptySound && w.switchSound),"Every weapon has preview, fire/reload/empty/switch clips");
            Check(catalog.weapons.Select(w=>w.shootingSound).Distinct().Count()==catalog.weapons.Length,"Distinct firing audio per weapon");
            Check(!PlayerProgress.Buy(premium) && progress.money==0,"Insufficient funds do not buy or subtract");
            PlayerProgress.Award(premium.price);Check(PlayerProgress.Buy(premium) && progress.money==0 && progress.Owns(premium),"Purchase deducts exact price and grants ownership");
            Check(!PlayerProgress.Buy(premium) && progress.money==0,"Duplicate purchase rejected");
            Check(PlayerProgress.Equip(premium),"Owned weapon can be equipped");Check(!PlayerProgress.Equip(catalog.weapons[3]),"Unowned weapon cannot be equipped");
            string json=JsonUtility.ToJson(progress);const string key="DeadSignal.Verification.RoundTrip";PlayerPrefs.SetString(key,json);PlayerPrefs.Save();var restored=JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(key));PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();Check(restored.Owns(premium) && restored.equippedWeapon==premium.id && restored.money==0,"Save/load preserves money, ownership and loadout");
            GameManager.Instance.LoadLevel(1,false);Check(!SceneTransition.Busy,"Locked level cannot load");PlayerProgress.UseDefault();
            yield return InspectUI();
            for(int level=0;level<catalog.levels.Length;level++)
            {
                var game=GameManager.Instance;GraphicsPresets.Apply(preset,game.catalog,false);Application.targetFrameRate=60;
                Check(game.LevelNumber==level+1,"Correct level loaded "+(level+1));game.Play();yield return null;
                Check(game.weapons.Current.definition==game.level.defaultWeapon,"Configurable default weapon for level "+(level+1));
                var enemies=FindObjectsByType<EnemyAI>();Check(enemies.All(e=>e.GetComponent<NavMeshAgent>().isOnNavMesh),"All guards on baked NavMesh");
                if(level==0)
                {
                    foreach(var enemy in enemies){enemy.enabled=false;enemy.GetComponent<EnemyWeapon>().enabled=false;}
                    int before=AudioManager.Instance.FootstepsPlayed;yield return new WaitForSecondsRealtime(.4f);Check(before==AudioManager.Instance.FootstepsPlayed,"No footsteps while idle");
                    foreach(var surface in new[]{SurfaceKind.Concrete,SurfaceKind.Grass,SurfaceKind.Metal})
                    {
                        float x=surface==SurfaceKind.Grass?-6:surface==SurfaceKind.Metal?6:0;game.player.Teleport(new Vector3(x,.15f,-28),Quaternion.identity);
                        testKeyboard.MakeCurrent();InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.W));yield return new WaitForSecondsRealtime(1.1f);InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return null;
                        Check(AudioManager.Instance.LastSurface==surface && AudioManager.Instance.FootstepsPlayed>before,"Walking footsteps on "+surface+"; steps="+(AudioManager.Instance.FootstepsPlayed-before)+"; position="+game.player.transform.position);before=AudioManager.Instance.FootstepsPlayed;
                    }
                    game.player.Teleport(new Vector3(0,.15f,-28),Quaternion.identity);yield return new WaitForSecondsRealtime(.2f);
                    int sprintSteps=AudioManager.Instance.FootstepsPlayed;testKeyboard.MakeCurrent();InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.W,Key.LeftShift));yield return new WaitForSecondsRealtime(1.2f);InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return null;Check(AudioManager.Instance.FootstepsPlayed-sprintSteps>=3,"Running increases footstep cadence");
                    game.player.Teleport(new Vector3(0,.15f,-28),Quaternion.identity);yield return new WaitForSecondsRealtime(.2f);
                    int landings=AudioManager.Instance.LandingsPlayed;testKeyboard.MakeCurrent();InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.Space));yield return null;InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.2f);before=AudioManager.Instance.FootstepsPlayed;yield return new WaitForSecondsRealtime(.15f);Check(before==AudioManager.Instance.FootstepsPlayed,"No footsteps airborne");yield return new WaitForSecondsRealtime(.7f);Check(AudioManager.Instance.LandingsPlayed>landings,"Landing sound on return to ground");
                    PlayerProgress.Award(1000);PlayerProgress.Buy(catalog.weapons[3]);
                    foreach(var definition in catalog.weapons)
                    {
                        PlayerProgress.Equip(definition);game.weapons.EquipLoadout();yield return null;var weapon=game.weapons.Current;int rounds=weapon.Ammo.magazine;
                        Check(weapon.TryFire() && weapon.Ammo.magazine==rounds-1,"Fire and ammo consumption: "+definition.id);
                        Check(weapon.BeginReload(),"Reload starts: "+definition.id);yield return new WaitForSecondsRealtime(definition.reloadDuration+.2f);
                        Check(!weapon.Reloading && weapon.Ammo.magazine==definition.magazineCapacity,"Timed reload completed: "+definition.id);
                        weapon.Ammo.magazine=0;Check(!weapon.TryFire() && weapon.Ammo.magazine==0,"Empty magazine prevents firing: "+definition.id);weapon.BeginReload();yield return new WaitForSecondsRealtime(definition.reloadDuration+.2f);
                    }
                    PlayerProgress.UseDefault();game.weapons.EquipLoadout();
                    foreach(var enemy in enemies){enemy.enabled=true;enemy.GetComponent<EnemyWeapon>().enabled=true;}
                }
                game.player.enabled=false;game.player.GetComponentInChildren<PlayerLook>().enabled=false;
                yield return new WaitForSecondsRealtime(1);sampling=true;
                foreach(var enemy in enemies)
                {
                    if(!enemy || enemy.GetComponent<EnemyHealth>().Dead)continue;
                    Vector3 desired=enemy.transform.position-enemy.transform.forward*6;
                    if(NavMesh.SamplePosition(desired,out var nav,5,NavMesh.AllAreas))game.player.Teleport(nav.position,Quaternion.identity);
                    float until=Time.realtimeSinceStartup+3.2f;float fireAt=Time.realtimeSinceStartup+1;
                    while(Time.realtimeSinceStartup<until && !enemy.GetComponent<EnemyHealth>().Dead)
                    {
                        game.playerHealth.Restore();game.playerCamera.transform.LookAt(enemy.transform.position+Vector3.up*1.2f);
                        if(Time.realtimeSinceStartup>fireAt){var w=game.weapons.Current;if(w.Ammo.magazine==0)w.BeginReload();else w.TryFire();}
                        yield return null;
                    }
                }
                sampling=false;foreach(var e in enemies)if(e)e.GetComponent<EnemyHealth>().TakeDamage(999,game.player.transform.position);
                game.player.Teleport(game.objectives.extraction.position,Quaternion.identity);yield return new WaitForSecondsRealtime(.3f);
                Check(CombatEvents.State==GameState.Won,"Mission completion "+(level+1));Check(PlayerProgress.Data.unlockedLevel>=Math.Min(level+1,2),"Next mission unlock saved");
                if(level<2){loadStart=Time.realtimeSinceStartupAsDouble;game.NextLevel();while(SceneTransition.Busy)yield return null;checks.Add("MEASURE: transition "+(level+1)+" = "+(Time.realtimeSinceStartupAsDouble-loadStart).ToString("F3")+" s");yield return new WaitForSecondsRealtime(.5f);}
            }
            PlayerProgress.Equip(premium);GameManager.Instance.Restart();while(SceneTransition.Busy)yield return null;yield return new WaitForSecondsRealtime(.5f);Check(GameManager.Instance.weapons.Current.definition.id==premium.id,"Explicit owned loadout overrides level default after reload");
            Finish();
        }
        static double Percentile(List<double> values,double p){if(values.Count==0)return 0;var sorted=values.ToArray();Array.Sort(sorted);return sorted[(int)((sorted.Length-1)*p)];}
        IEnumerator InspectUI()
        {
            var game=GameManager.Instance;var screen=FindAnyObjectByType<ArmoryScreen>();
            foreach(var resolution in new[]{new Vector2Int(1280,720),new Vector2Int(1280,960),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(resolution.x,resolution.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);screen.Show(true);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();
                var corners=new Vector3[4];bool fits=true;foreach(var button in FindObjectsByType<UnityEngine.UI.Button>()){((RectTransform)button.transform).GetWorldCorners(corners);foreach(var c in corners)if(c.x<-.5f || c.x>Screen.width+.5f || c.y<-.5f || c.y>Screen.height+.5f)fits=false;}
                Check(fits,"Armory controls fit "+Screen.width+"x"+Screen.height);
                if(resolution.x==1280 && resolution.y==720)ScreenCapture.CaptureScreenshot(Path.Combine(folder,"armory-1280x720.png"));
                screen.Show(false);yield return new WaitForSecondsRealtime(.3f);
                if(resolution.y==960)ScreenCapture.CaptureScreenshot(Path.Combine(folder,"missions-1280x960.png"));
            }
            screen.Show(true);yield return new WaitForSecondsRealtime(.3f);
            var card=GameObject.Find("Weapon dmr");var purchase=card.GetComponentInChildren<UnityEngine.UI.Button>();purchase.onClick.Invoke();Check(!PlayerProgress.Data.Owns(game.catalog.weapons[3]),"Store button rejects unaffordable purchase");
            PlayerProgress.Award(game.catalog.weapons[3].price);purchase.onClick.Invoke();Check(PlayerProgress.Data.Owns(game.catalog.weapons[3]),"Store Buy button grants weapon");purchase.onClick.Invoke();Check(PlayerProgress.Data.equippedWeapon=="dmr","Store Equip button selects owned gun");PlayerProgress.UseDefault();
            game.NavigationOpen=false;game.SettingsOpen=true;yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"settings-1920x1080.png"));game.SettingsOpen=false;
        }
        void Finish()
        {
            string summary=$"Preset: {GraphicsPresets.Names[preset]}\nGPU: {SystemInfo.graphicsDeviceName}\nCPU: {SystemInfo.processorType}\nResolution: {Screen.width}x{Screen.height}\nCombat frames: {frames.Count}\nAverage FPS: {(frames.Count>0?1000/frames.Average():0):F2}\nFrame time p50/p95/p99 ms: {Percentile(frames,.5):F2} / {Percentile(frames,.95):F2} / {Percentile(frames,.99):F2}\nMain thread p95 ms: {Percentile(cpu,.95):F2} (valid={main.Valid}; includes frame-cap wait)\nGPU p95 ms: {Percentile(gpu,.95):F2} (samples={gpu.Count}; zero means unavailable)\nPeak draw calls: {maxDraw} (valid={draw.Valid}; zero is not evidence of no rendering)\nPeak used memory MB: {maxMemory/1048576d:F1}\nPeak GC allocation per frame bytes: {maxGc} (valid={gc.Valid})\n";
            File.WriteAllText(Path.Combine(folder,"benchmark-"+GraphicsPresets.Names[preset]+".txt"),summary+string.Join("\n",checks));Debug.Log(summary);Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        void OnDestroy(){main.Dispose();gc.Dispose();draw.Dispose();memory.Dispose();}
    }
}
