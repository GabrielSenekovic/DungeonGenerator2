using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TitleAnimator : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> letters = new List<TextMeshProUGUI>();
    [SerializeField]  List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();
    float letterSwitchTimer;
    [SerializeField] float letterSwitchTimerMax;
    float bloomTimer;
    [SerializeField] float bloomTimerMax;
    [SerializeField] AnimationCurve bloomCurve;
    [SerializeField] Volume volume;
    [SerializeField] List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();

    private void Update()
    {
        letterSwitchTimer += Time.deltaTime;
        if(letterSwitchTimer >= letterSwitchTimerMax)
        {
            letterSwitchTimer = 0;
            SwapFonts();
        }
        bloomTimer += Time.deltaTime;
        if(bloomTimer <= bloomTimerMax)
        {
            float bloomPercentage = bloomTimer / bloomTimerMax;
            volume.profile.TryGet(out Bloom bloom);
            bloom?.intensity.Override(bloomCurve.Evaluate(bloomPercentage));
        }
    }
    void SwapFonts()
    {
        for(int i = 0; i < letters.Count; i++)
        {
            if(Random.Range(0, 5) < 2)
            {
                letters[i].font = fonts.GetRandom();
            }
        }
    }
}
