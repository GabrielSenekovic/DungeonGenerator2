using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IMenu
{
    void OnOpen();
    void OnClose();
    CanvasGroup GetCanvas();
}
