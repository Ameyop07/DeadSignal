using UnityEngine;

namespace CompoundFPS
{
    public static class GameSettings
    {
        public static float Sensitivity { get => PlayerPrefs.GetFloat("FPS.Sensitivity", 0.10f); set => PlayerPrefs.SetFloat("FPS.Sensitivity", value); }
        public static float Master { get => PlayerPrefs.GetFloat("FPS.Master", 0.8f); set { PlayerPrefs.SetFloat("FPS.Master", value); AudioListener.volume = value; } }
        public static float Music { get => PlayerPrefs.GetFloat("FPS.Music", 0.25f); set => PlayerPrefs.SetFloat("FPS.Music", value); }
        public static float SFX { get => PlayerPrefs.GetFloat("FPS.SFX", 0.8f); set => PlayerPrefs.SetFloat("FPS.SFX", value); }
    }
}
