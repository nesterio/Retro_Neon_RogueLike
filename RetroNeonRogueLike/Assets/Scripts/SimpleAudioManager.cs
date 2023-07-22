using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }
    [SerializeField] private List<Sound> sounds = new List<Sound>();

    private static Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Populate the sound dictionary
        foreach (var sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound.clip);
            }
        }
    }

    public static void PlaySound(string soundName, AudioSource source)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            source.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    public static void SetSourceVolume(AudioSource source, float volume)=>
        source.volume = Mathf.Clamp01(volume);
    

    public static void FadeOutAudio(AudioSource source, float duration, Action callback = null) =>
        RoomManager.Instance.StartCoroutine(FadeOut(source, duration, callback));
    private static IEnumerator FadeOut(AudioSource audioSource, float duration, Action callback)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        
        callback?.Invoke();
    }

    public static void FadeInAudio(AudioSource source, float duration, float targetVolume, Action callback = null) =>
        RoomManager.Instance.StartCoroutine(FadeIn(source, duration, targetVolume, callback));
    private static IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume, Action callback)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / duration;
            yield return null;
        }
        
        callback?.Invoke();
    }
}