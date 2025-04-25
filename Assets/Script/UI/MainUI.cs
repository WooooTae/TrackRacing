using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    public Button StartBtn;

    void Start()
    {
        StartBtn.onClick.AddListener(() => { Debug.Log("Click"); SceneManager.LoadScene("Game"); }); 
    }
}
