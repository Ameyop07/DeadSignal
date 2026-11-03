using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CompoundFPS
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get;private set;}
        static bool startOnLoad;
        public PlayerController player;
        public PlayerHealth playerHealth;
        public WeaponManager weapons;
        public Camera playerCamera, menuCamera;
        public Transform spawn;
        public ObjectiveManager objectives;
        public string missionName="OUTPOST 17";
        public int LevelNumber=>SceneManager.GetActiveScene().buildIndex+1;
        public bool HasNextLevel=>LevelNumber<SceneManager.sceneCountInBuildSettings;
        public bool SettingsOpen {get;set;}
        public float MissionTime {get;private set;}
        GameState appliedState=(GameState)(-1);
        void Awake()
        {
            Instance=this;CombatEvents.State=GameState.Menu;Time.timeScale=1;
            Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;AudioListener.volume=GameSettings.Master;
        }
        void Start(){if(startOnLoad){startOnLoad=false;Play();}else ApplyState();}
        void Update()
        {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if(SettingsOpen)SettingsOpen=false;
                else if(CombatEvents.State==GameState.Playing)SetState(GameState.Paused);
                else if(CombatEvents.State==GameState.Paused)SetState(GameState.Playing);
            }
            if(CombatEvents.Playing)
            {
                MissionTime+=Time.deltaTime;
                if(player.transform.position.y<-10)playerHealth.TakeDamage(999,player.transform.position);
            }
            if(appliedState!=CombatEvents.State)ApplyState();
        }
        public void Play(){player.Teleport(spawn.position,spawn.rotation);SetState(GameState.Playing);}
        public void Resume(){SettingsOpen=false;SetState(GameState.Playing);}
        public void SetState(GameState state){CombatEvents.State=state;ApplyState();}
        void ApplyState()
        {
            appliedState=CombatEvents.State;bool playing=CombatEvents.Playing;
            Time.timeScale=playing?1:0;
            Cursor.lockState=playing?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!playing;
            bool menu=CombatEvents.State==GameState.Menu;
            menuCamera.enabled=menu;playerCamera.enabled=!menu;
        }
        public void Restart(){Reload(true);}
        public void MainMenu(){Reload(false);}
        public void NextLevel(){startOnLoad=true;Time.timeScale=1;SceneManager.LoadScene(Mathf.Min(LevelNumber,SceneManager.sceneCountInBuildSettings-1));}
        public void SelectNextLevel(){startOnLoad=false;Time.timeScale=1;SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex+1)%SceneManager.sceneCountInBuildSettings);}
        void Reload(bool play){startOnLoad=play;Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
        public void Quit(){PlayerPrefs.Save();Application.Quit();}
        void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;if(Instance==this)Instance=null;}
    }
}
