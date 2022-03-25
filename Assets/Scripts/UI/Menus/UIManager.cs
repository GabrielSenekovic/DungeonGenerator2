using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public GameObject saveLocationNameBox;
    public MiniMap miniMap;
    public DialogBox dialogBox;

    public SavedLocations savedLocations;

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
    public Texture2D currentMap;
    public UIColorManager UIColor;

    void Awake()
    {
        if(instance == null)
        {instance = this; Debug.Log("UIManager instance set to this");}
        else
        {
            Destroy(this);
        }
    }

    private void Start() 
    {
        Debug.Log("Initialising main menu");
        Instance.mainMenu.GetComponent<Menu>().Initialize(this, GetComponent<AudioSource>());
        if(Instance.mainMenu.canvas.alpha == 1){Instance.mainMenu.GetComponent<Menu>().SwitchMenu(0);}
        savedLocations.text.Initialize(graphemeDatabase.fonts[0], true);
    }

    public void OpenCommandBox()
    {
        commandBox.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    public bool CloseCommandBox()
    {
        if(commandBox.gameObject.activeSelf && !saveLocationNameBox.gameObject.activeSelf)
        {
            commandBox.gameObject.SetActive(false);
            Time.timeScale = 1;
            return true;
        }
        return false;
    }
    public void OpenSaveLocationNameBox()
    {
        saveLocationNameBox.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    public void CloseSaveLocationNameBox()
    {
        savedLocations.AddLocation(saveLocationNameBox.GetComponent<InputField>().text, currentMap, DunGenes.Instance.gameData.levelConstructionSeed, DunGenes.Instance.gameData.levelDataSeed);
        saveLocationNameBox.gameObject.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public static Currency GetCurrency()
    {
        if(instance == null)
        {
            Debug.Log("Instance of UIManager was null");
        }
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
        instance.HUD.SetActive(!instance.HUD.activeSelf);
        ColorAdjustments colorAdjustments;
        instance.volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        colorAdjustments.colorFilter.value = instance.HUD.activeSelf ? Color.white : Instance.UIColor.openMenuColor;
        DepthOfField depthOfField;
        instance.volume.profile.TryGet<DepthOfField>(out depthOfField);
        depthOfField.focusDistance.value = instance.HUD.activeSelf ? 1.8f : 4.5f;
        depthOfField.focalLength.value = instance.HUD.activeSelf ? 50 : 300;
    }

    static public void StartDialog(Manuscript.Dialog dialog)
    {
        ToggleHUD();
        instance.dialogBox.gameObject.SetActive(true);
        instance.dialogBox.InitiateDialog(dialog);
        Time.timeScale = 0;
    }
    static public void EndDialog()
    {
        ToggleHUD();
        instance.dialogBox.gameObject.SetActive(false);
        Time.timeScale = 1;
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
