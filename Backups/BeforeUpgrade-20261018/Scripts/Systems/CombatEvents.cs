using System;
using UnityEngine;

namespace CompoundFPS
{
    public enum GameState { Menu, Playing, Paused, Lost, Won }
    public enum SoundCue { Rifle, Pistol, Reload, Empty, Footstep, EnemyShot, EnemyDeath, Hurt, Button }
    public static class CombatEvents
    {
        public static GameState State = GameState.Menu;
        public static bool Playing => State == GameState.Playing;
        public static event Action<Vector3, float> Noise;
        public static event Action<SoundCue, Vector3> Sound;
        public static event Action<Vector3, Vector3, bool> Impact;
        public static event Action<bool> HitConfirmed;
        public static event Action EnemyKilled;
        public static void EmitNoise(Vector3 p, float range) => Noise?.Invoke(p, range);
        public static void Play(SoundCue cue, Vector3 p) => Sound?.Invoke(cue, p);
        public static void Spark(Vector3 p, Vector3 normal, bool flesh) => Impact?.Invoke(p, normal, flesh);
        public static void Confirm(bool kill) => HitConfirmed?.Invoke(kill);
        public static void Kill() => EnemyKilled?.Invoke();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { State = GameState.Menu; Noise = null; Sound = null; Impact = null; HitConfirmed = null; EnemyKilled = null; }
    }
    public interface IDamageable { bool TakeDamage(float amount, Vector3 origin); }
}
