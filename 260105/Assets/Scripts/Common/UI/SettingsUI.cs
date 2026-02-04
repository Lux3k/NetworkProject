using UnityEngine;
using UnityEngine.UI;

public class SettingsUIData : BaseUIData { }

public class SettingsUI : BaseUI
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        bgmSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        //AudioManager.Instance?.SetBGMVolume(value);
    }

    void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        //AudioManager.Instance?.SetSFXVolume(value);
    }
}
