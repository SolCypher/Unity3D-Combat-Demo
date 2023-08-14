using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    #region Singleton
    public static SceneController instance;

    void Awake(){
        instance = this;
    }

    #endregion

    public string mainScene = "MainScene";

    public void RestartMainScene(){
        SceneManager.LoadScene(mainScene);

    }

}
