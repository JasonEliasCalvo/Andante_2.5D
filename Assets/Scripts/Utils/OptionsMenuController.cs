using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    public Slider volumeSlider, musicSlider, sfxSlider;

    [SerializeField] private AudioMixer masterMixer;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    void Start()
    {
        InitializeOptions();

        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }


    private void SetResolution(int index)
    {
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
    }

    private void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private float NormalizeToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
    }

    private void SetMasterVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    private void SetMusicVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    private void SetSFXVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }


    private void InitializeOptions()
    {
        // Valores predeterminados
        float defaultMasterVol = 1.0f; 
        float defaultMusicVol = 0.5f; 
        float defaultSfxVol = 0.5f;

        float masterVol = PlayerPrefs.HasKey(MasterVolumeKey) ? PlayerPrefs.GetFloat(MasterVolumeKey) : defaultMasterVol;
        masterMixer.SetFloat("MasterVolume", NormalizeToDecibels(masterVol));
        volumeSlider.value = masterVol;

        float musicVol = PlayerPrefs.HasKey(MusicVolumeKey) ? PlayerPrefs.GetFloat(MusicVolumeKey) : defaultMusicVol;
        masterMixer.SetFloat("MusicVolume", NormalizeToDecibels(musicVol));
        musicSlider.value = musicVol;

        float sfxVol = PlayerPrefs.HasKey(SFXVolumeKey) ? PlayerPrefs.GetFloat(SFXVolumeKey) : defaultSfxVol;
        masterMixer.SetFloat("SFXVolume", NormalizeToDecibels(sfxVol));
        sfxSlider.value = sfxVol;
    }
}
