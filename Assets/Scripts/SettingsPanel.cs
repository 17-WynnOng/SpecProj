using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider, sfxSlider;

    void Awake()
    {
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.GetBGMSliderReference(bgmSlider);
            AudioManager.Instance.GetSFXSliderReference(sfxSlider);

            bgmSlider.onValueChanged.AddListener(_ => AudioManager.Instance.SetBGMVolumeFromSlider());
            sfxSlider.onValueChanged.AddListener(_ => AudioManager.Instance.SetSFXVolumeFromSlider());
        }
    }
}
