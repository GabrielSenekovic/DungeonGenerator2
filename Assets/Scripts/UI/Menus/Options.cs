using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Options : MonoBehaviour
{
    public Scrollbar SFXSlider;
    public Scrollbar MusicSlider;
    public Scrollbar VolumeSlider;

    public Scrollbar PrimaryMenuColor;
    public Scrollbar SaturationSlider;

    public void ChangeSFXVolume()
    {
        AudioManager.SFX_volume = SFXSlider.value;
    }
    public void ChangeMusicVolume()
    {
        AudioManager.music_volume = MusicSlider.value;
    }
    public void ChangeGlobalVolume()
    {
        AudioManager.global_volume = VolumeSlider.value;
    }
    public void ChangePrimaryMenuColor()
    {
        UIManager.Instance.UIColor.SetPrimaryColor(Color.HSVToRGB(PrimaryMenuColor.value, 1, 1), UIManager.Instance);
    }
    public void ChangeSaturation()
    {
        ColorAdjustments colorAdjustments;
        UIManager.Instance.volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        colorAdjustments.saturation.value = (SaturationSlider.value * 100) - 100;
    }
}
