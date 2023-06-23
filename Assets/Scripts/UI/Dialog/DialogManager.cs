using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] DialogBox dialogBox;
    public bool DialogDone() => dialogBox.dialogDone;
    public bool DialogActive() => dialogBox.gameObject.activeSelf;
    public void StartDialog(Manuscript.Dialog dialog)
    {
        UIManager.ToggleHUD();
        dialogBox.gameObject.SetActive(true);
        dialogBox.InitiateDialog(dialog);
        Time.timeScale = 0;
    }
    public void ContinueDialog()
    {
        dialogBox.ContinueDialog();
    }
    public void EndDialog()
    {
        UIManager.ToggleHUD();
        dialogBox.gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}
