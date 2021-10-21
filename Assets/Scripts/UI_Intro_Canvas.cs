using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//Script for the instruction screens for each stage
public class UI_Intro_Canvas : MonoBehaviour
{
    // Imports of JS functions defined in the .jslib in Plugins folder 
    [DllImport("__Internal")]
    public static extern void RedirectBOF();
    [DllImport("__Internal")]
    public static extern void RequestConfigFromJS();

    GameObject ContinueButton;

    void Start()
    {
        ContinueButton = gameObject.transform.GetChild(1).gameObject;
        HideButtons();
        
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Invoke("ShowButtons", 4f);
        }
        else
        {
            Invoke("ShowButtons", 4f);
        }
    }

    void StartExperiment(int participantID)
    {
        Invoke("ShowButtons", 4f);
    }
    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void HideButtons()
    {
        ContinueButton.SetActive(false);
    }
    void ShowButtons()
    {
        ContinueButton.SetActive(true);
    }

}
