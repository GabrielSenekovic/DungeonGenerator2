using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [SerializeField] Light DirectionalLight;
    [SerializeField] LightingPreset Preset;
    [SerializeField, Range(0, 24)] float TimeOfDay;
    [SerializeField] float timeSpeed;

    void Update() 
    {
        if(Preset == null)
        {
            return;
        }

        if(Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * timeSpeed;
            TimeOfDay %= 24;
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) -90f, 170f, 0));
        }
    }
    public void Sleep()
    {
        TimeOfDay = 7;
        UpdateLighting(TimeOfDay / 24f);
    }

    void OnValidate() 
    {
        if(DirectionalLight != null){return;}
        if(RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light l in lights)
            {
                if(l.type == LightType.Directional)
                {
                    DirectionalLight = l;
                    return;
                }
            }
        }
    }
}
