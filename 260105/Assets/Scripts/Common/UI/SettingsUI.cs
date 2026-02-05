using UnityEngine;
using UnityEngine.UI;

public class SettingsUIData : BaseUIData { }

public class SettingsUI : BaseUI
{
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        _bgmSlider.onValueChanged.RemoveAllListeners();
        _sfxSlider.onValueChanged.RemoveAllListeners();

        _bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
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
