using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FModAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class GunSounds
    {
        public string Name;
        public EventReference Shot;
        public EventReference EmptyShot;
        public EventReference MagIn;
        public EventReference MagOut;
        public EventReference Cock; // lol
    }
    [SerializeField] private List<GunSounds> gunsSounds = new List<GunSounds>();
    private static readonly Dictionary<string, GunSounds> GunSoundsDictionary = new Dictionary<string, GunSounds>();

    [System.Serializable]
    public class Sound
    {
        public string name;
        public EventReference clip;
    }
    [SerializeField] private List<Sound> sounds = new List<Sound>();
    private static readonly Dictionary<string, EventReference> SoundDictionary = new Dictionary<string, EventReference>();

    public static Dictionary<SoundInstanceType, EventInstance> SoundInstancesDictionary
    {
        get => _soundInstancesDictionary;
        private set => _soundInstancesDictionary = value;
    }
    private static Dictionary<SoundInstanceType, EventInstance> _soundInstancesDictionary = new Dictionary<SoundInstanceType, EventInstance>();

    private void Awake()
    {
        // Populate the sound dictionary
        foreach (var sound in sounds)
        {
            if (!SoundDictionary.ContainsKey(sound.name))
                SoundDictionary.Add(sound.name, sound.clip);
        }
        
        // Populate the gun sound dictionary
        foreach (var sound in gunsSounds)
        {
            if (!GunSoundsDictionary.ContainsKey(sound.Name))
                GunSoundsDictionary.Add(sound.Name, sound);
        }
    }

    public static void PlaySound(string soundName, Vector3 worldPosition)
    {
        if (SoundDictionary.TryGetValue(soundName, out EventReference sound))
            RuntimeManager.PlayOneShot(sound, worldPosition);
        else
            Debug.LogWarning("Sound not found: " + soundName);
    }
    
    public static void PlayGunSound(string gunName ,GunSoundType type, Vector3 worldPosition)
    {
        if (!GunSoundsDictionary.TryGetValue(gunName, out var gunSounds))
            Debug.LogWarning("Gun sounds not found: " + gunName);
        
        RuntimeManager.PlayOneShot(GetGunSoundByType(gunSounds), worldPosition);

        EventReference GetGunSoundByType(GunSounds gunSounds)
        {
            switch (type)
            {
                case GunSoundType.Shot:
                    return gunSounds.Shot;
                case GunSoundType.EmptyShot:
                    return gunSounds.EmptyShot;
                case GunSoundType.MahIn:
                    return gunSounds.MagIn;
                case GunSoundType.MagOut:
                    return gunSounds.MagOut;
                case GunSoundType.Cock:
                    return gunSounds.Cock;
                default:
                    return gunSounds.EmptyShot;
            }
        }
    }

    public static void CreateSoundInstance(SoundInstanceType type, string soundName, bool startInstance = true)
    {
        if (SoundInstancesDictionary.ContainsKey(type))
        {
            Debug.LogError("This instance is already playing, but you are trying to call it again: " + type);
            return;
        }

        if (!SoundDictionary.TryGetValue(soundName, out var sound))
        {
            Debug.LogError("Couldn't find the sound by the name: " + soundName);
            return;
        }

        var eventInstance = RuntimeManager.CreateInstance(sound);
        SoundInstancesDictionary.Add(type, eventInstance);

        if(startInstance)
            StartSoundInstance(type);
    }

    public static void StartSoundInstance(SoundInstanceType type)
    {
        if (!SoundInstancesDictionary.TryGetValue(type, out var instance))
        {
            Debug.LogError("Couldn't start the sound instance. It is not initialized. Type: " + type);
            return;
        }

        instance.getPlaybackState(out var state);

        if (state != PLAYBACK_STATE.STOPPED)
        {
            Debug.LogError($"Couldn't start the sound instance. Sound instance state: {state}. Type: {type}");
            return;
        }

        instance.start();
    }
    
    public static void StopSoundInstance(SoundInstanceType type)
    {
        if (!SoundInstancesDictionary.TryGetValue(type, out var instance))
        {
            Debug.LogError("Couldn't stop the sound instance. It is not initialized. Type: " + type);
            return;
        }

        instance.getPlaybackState(out var state);

        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
        {
            Debug.LogError($"Couldn't stop the sound instance. Sound instance state: {state}. Type: {type}");
            return;
        }

        instance.stop(STOP_MODE.ALLOWFADEOUT);
    }
}

public enum SoundInstanceType
{
    Walk
}

public enum GunSoundType
{
    Shot,
    EmptyShot,
    MahIn,
    MagOut,
    Cock
}
