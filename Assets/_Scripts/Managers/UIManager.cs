using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject hotkeyList;
    public Button hotkeyList_ShowBtn;
    public Button hotkeyList_HideBtn;

    void Start()
    {
        hotkeyList.SetActive(false);
        hotkeyList_HideBtn.gameObject.SetActive(false);
        hotkeyList_ShowBtn.gameObject.SetActive(true);
        
    }

    public void ShowHotkeyListBtn(){
        hotkeyList.SetActive(true);
        hotkeyList_HideBtn.gameObject.SetActive(true);
        hotkeyList_ShowBtn.gameObject.SetActive(false);
    }

    public void HideHotkeyListBtn(){
        hotkeyList.SetActive(false);
        hotkeyList_HideBtn.gameObject.SetActive(false);
        hotkeyList_ShowBtn.gameObject.SetActive(true);
    }
    
}
