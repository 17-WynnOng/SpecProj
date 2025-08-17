using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [SerializeField] private Slider bgmSlider, sfxSlider;
    [SerializeField] private AudioMixer bgmMixer, sfxMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Start()
    {

        if (LoadoutManager.Instance != null && LoadoutManager.Instance.myAudioSource != null)
        {
            bgmSource = LoadoutManager.Instance.myAudioSource;
        }

        LoadAudioSettings();

        if (bgmMixer != null)
        SetBGMVolume(bgmSlider.value);
    }

    // ===== MUSIC =====
    public void PlayBGM(int index, bool loop = true)
    {
        if (index < 0 || index >= bgmClips.Length) return;
        bgmSource.clip = bgmClips[index];
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGM(string clipName, bool loop = true)
    {
        AudioClip clip = System.Array.Find(bgmClips, c => c.name == clipName);
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayOneShotSFXByName(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        // Find the clip in the array
        AudioClip clip = System.Array.Find(sfxClips, c => c != null && c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
            return;
        }

        // Play it with current SFX volume
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXLoopByName(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        AudioClip clip = System.Array.Find(sfxClips, c => c != null && c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
            return;
        }

        // Assign clip and enable looping
        sfxSource.clip = clip;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
        sfxSource.loop = false;
        sfxSource.clip = null;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBGMVolume(float value)
    {
        if (value < 0.0001f) value = 0.0001f; // avoid log(0)
        bgmMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20f);
        SaveAudioSettings();
    }

    public void SetBGMVolumeFromSlider()
    {
        SetBGMVolume(bgmSlider.value);
    }

    public void SetSFXVolume(float value)
    {
        if (value < 0.0001f) value = 0.0001f; // avoid log(0)
        sfxMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20f);
        SaveAudioSettings();
    }

    public void SetSFXVolumeFromSlider()
    {
        SetSFXVolume(sfxSlider.value);
    }

    public void RefreshBGMSlider(float _value)
    {
        bgmSlider.value = _value;
    }

    private void SaveAudioSettings()
    {
        AudioSettingsData data = new AudioSettingsData()
        {
            bgmVolume = bgmSlider.value,
            sfxVolume = sfxSlider.value,
        };

        SaveSystem.Save("audioSettings.json", data);
    }

    private void LoadAudioSettings()
    {
        var data = SaveSystem.Load<AudioSettingsData>("audioSettings.json");
        if (data != null)
        {
            bgmSlider.value = data.bgmVolume;   
            sfxSlider.value = data.sfxVolume;
        }
    }

    public void GetBGMSliderReference(Slider slider)
    {
        bgmSlider = slider;
    }

    public void GetSFXSliderReference(Slider slider)
    {
        sfxSlider = slider;
    }
}
