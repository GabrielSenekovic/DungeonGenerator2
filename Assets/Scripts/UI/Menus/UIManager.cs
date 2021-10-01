using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIManager : MonoBehaviour
{
    static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    [SerializeField] Menu mainMenu;
    [SerializeField] GameObject HUD;
    public enum UIScreen
    {
        MainMenu = 0
    }
    public GraphemeDatabase graphemeDatabase;

    List<CanvasGroup> openMenus = new List<CanvasGroup>();

    public Volume volume;

    public Counter moneyCounter;

    public Currency currency;

    public Counter keys;

    public CommandBox commandBox;
    public MiniMap miniMap;

    [System.Serializable]public class UIColorManager
    {
        public Color openMenuColor;
        public Color primary;

        public void SetPrimaryColor(Color newColor, UIManager UI)
        {
            primary = newColor;
            UI.mainMenu.GetComponent<Menu>().ChangeColor(newColor);
            UI.HUD.GetComponentInChildren<HPBar>().background.color = newColor;
        }
    }
    public UIColorManager UIColor;

    void Awake()
    {
        instance = this;
    }

    private void Start() 
    {
        Instance.mainMenu.GetComponent<Menu>().Initialize(this, GetComponent<AudioSource>());
        if(Instance.mainMenu.canvas.alpha == 1){Instance.mainMenu.GetComponent<Menu>().SwitchMenu(0);}
    }

    public void OpenCommandBox()
    {
        commandBox.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    public bool CloseCommandBox()
    {
        if(commandBox.gameObject.activeSelf)
        {
            commandBox.gameObject.SetActive(false);
            Time.timeScale = 1;
            return true;
        }
        return false;
    }

    public static Currency GetCurrency()
    {
        return instance.currency;
    }
    public void OpenOrClose(UIScreen screen)
    {
        switch(screen)
        {
            case UIScreen.MainMenu: 

            OpenOrClose(mainMenu.canvas);
            if(mainMenu.canvas.alpha == 0)
            {
                EmptyMenus();
            }
            mainMenu.SwitchMenu(0); break;
        }
    }
    static public void OpenOrClose(CanvasGroup screen)
    {
        screen.alpha = screen.alpha > 0 ? 0 : 1;
        screen.blocksRaycasts = !(screen.blocksRaycasts); //!  = true ? false : true;
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        //cursor.gameObject.SetActive(cursor.gameObject.activeSelf ? false: true);
        if(screen.GetComponentInChildren<Options>())
        {
            screen.GetComponentInChildren<Options>().Activate(screen.alpha == 1);
        }
    }

    static public void ToggleHUD()
    {
        UIManager.Instance.HUD.SetActive(!UIManager.Instance.HUD.activeSelf);
        ColorAdjustments colorAdjustments;
        UIManager.Instance.volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        colorAdjustments.colorFilter.value = UIManager.Instance.HUD.activeSelf ? Color.white : Instance.UIColor.openMenuColor;
        DepthOfField depthOfField;
        UIManager.Instance.volume.profile.TryGet<DepthOfField>(out depthOfField);
        depthOfField.focusDistance.value = UIManager.Instance.HUD.activeSelf ? 1.8f : 4.5f;
        depthOfField.focalLength.value = UIManager.Instance.HUD.activeSelf ? 50 : 300;
    }

    public void AddMenu(CanvasGroup menu)
    {
        openMenus.Add(menu);
    }

    public void EmptyMenus()
    {
        for(int i = 0; i < openMenus.Count; i++)
        {
            OpenOrClose(openMenus[i]);
        }
        openMenus.Clear();
    }
}
