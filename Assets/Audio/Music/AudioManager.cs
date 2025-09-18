using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;

        [HideInInspector] public AudioSource source;
    }

    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;
    
    [Header("Background Music")]
    public AudioClip[] backgroundMusic;
    public AudioSource musicSource;
    
    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private Dictionary<string, SoundEffect> soundDictionary = new Dictionary<string, SoundEffect>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create dictionary for fast lookup
        foreach (SoundEffect sound in soundEffects)
        {
            // Create audio source for each sound
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * sfxVolume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            
            // Add to dictionary
            if (!soundDictionary.ContainsKey(sound.name))
                soundDictionary.Add(sound.name, sound);
        }

        // Setup music source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    void Start()
    {
        // Play random background music if available
        if (backgroundMusic.Length > 0)
            PlayRandomMusic();
    }

    // Play a sound by name
    public void PlaySound(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }

    // Play a sound at a specific position (for 3D sound)
    public void PlaySoundAtPosition(string name, Vector3 position)
    {
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }

    // Stop a sound
    public void StopSound(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            sound.source.Stop();
        }
    }

    // Play specific background music
    public void PlayMusic(int index)
    {
        if (backgroundMusic.Length > 0 && index < backgroundMusic.Length)
        {
            musicSource.clip = backgroundMusic[index];
            musicSource.Play();
        }
    }

    // Play random background music
    public void PlayRandomMusic()
    {
        if (backgroundMusic.Length > 0)
        {
            int index = Random.Range(0, backgroundMusic.Length);
            musicSource.clip = backgroundMusic[index];
            musicSource.Play();
        }
    }

    // Update volume settings
    public void UpdateVolume()
    {
        // Update music volume
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;

        // Update SFX volume
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.source != null)
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
        }
    }

    // Toggle music on/off
    public void ToggleMusic(bool isOn)
    {
        if (musicSource != null)
            musicSource.mute = !isOn;
    }

    // Toggle SFX on/off
    public void ToggleSFX(bool isOn)
    {
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.source != null)
                sound.source.mute = !isOn;
        }
    }
}