using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
public class UI_Button_Controller : MonoBehaviour
{
    // All Gameobjects and scripts that are in the scene
    Rigidbody2D rigidBodyPlayer;
    Player_Controller playerController;
    Layer_manager layerManager;
    Tool_Manager toolManager;
    Tool_Assistant_Script ToolAssistant;
    Coin_Manager coinManager;
    Experiment_Manager ExperimentManager;
    GameObject EndLevelPanel;
    Toggle moveToolButton, brushToolButton;
    Button skipButton;

    //Keycodes for the tool shortcuts
    KeyCode moveKey, brushKey, pauseKey;

    public Vector2 boxColliderSize;
    public int researchVersion; //Research version (determines the required score and coins)
    int requiredScore; // Minimum score required for progressing to the next level
    public string retryReason; //String for storing the outcome of a try
    public int score = 0; // Current score

    //States for the tutorial
    public enum State {Setup, Playing, Finished, End};
    public State state;
   
    void Start()
    {
        researchVersion = 1;
        //Setting up all the gameobjects and scripts
        layerManager = GameObject.Find("Layer_elements").GetComponent<Layer_manager>();
        toolManager = GameObject.Find("Photoshop_Tools").GetComponent<Tool_Manager>();
        playerController = GameObject.Find("Ball").GetComponent<Player_Controller>();
        ExperimentManager = GameObject.Find("Experiment_Manager").GetComponent<Experiment_Manager>();
        EndLevelPanel = GameObject.Find("EndLevelPanel");

        //If the scene has an in-game instruction - load the tool assistant
        GameObject assistant = GameObject.Find("Tool_Assistant");
        if(assistant != null)
        {
            ToolAssistant = assistant.GetComponent<Tool_Assistant_Script>();
        }
       
        EndLevelPanel.SetActive(false); //Disabling the endscreen

        //Assigning the move and brush tool and adding listeners for callbacks
        moveToolButton = gameObject.transform.GetChild(2).GetComponent<Toggle>();
        moveToolButton.onValueChanged.AddListener(delegate { MoveToggleValueChanged(moveToolButton); });
        brushToolButton = gameObject.transform.GetChild(3).GetComponent<Toggle>();
        brushToolButton.onValueChanged.AddListener(delegate { BrushToggleValueChanged(brushToolButton); });

        skipButton = gameObject.transform.GetChild(4).GetComponent<Button>();
        skipButton.gameObject.SetActive(false);
        StartCoroutine(ShowSkipButton());
       
        //The keycodes for the shortcuts
        moveKey = KeyCode.M;
        brushKey = KeyCode.B;
        //pauseKey = KeyCode.P;
        
        //Setting up the scene and resetting some values
        state = State.Setup;
        SetUpResearchVersion(researchVersion);
        score = 0;
        retryReason = "";
    }

    void Update()
    {
        //look if shortcuts are pressed and if the game is in setup mode
        if(state == State.Setup)
        {
            if (Input.GetKeyDown(moveKey) && toolManager.ActiveTool != toolManager.MoveTool)
            {
                moveToolButton.isOn = true;
            }
            else if (Input.GetKeyDown(brushKey) && toolManager.ActiveTool != toolManager.BrushTool)
            {
               brushToolButton.isOn = true;
            }
        }


    }
    
    // If the play button is pressed change game state and turn off all the player controls (tools, layers, etc)
    // Reset all the boxcolliders of all the layer tiles to their original size and change them to triggers and enable the edge colliders for the ball to roll over
    public void OnPressPlayButton()
    {
        state = State.Playing;
        gameObject.GetComponent<ToggleGroup>().SetAllTogglesOff();
        ToggleInputs();

        
        if(toolManager.ActiveTool == toolManager.MoveTool && Experiment_Manager.amountOfMoves > 0)
        {
            Experiment_Manager.amountOfMoves--;
        }
        toolManager.DeActivateTool(toolManager.ActiveTool);
        layerManager.SetCollisionsToTrigger(true);
        layerManager.EnableColliders(true);
        layerManager.ResetBoxColliders();
        playerController.StartPlayer();
    }

    // If the player has not hit the finish block, reset
    // Workaround is needed to prevent cheating and breaking the game
    public void OnPressResetButton()
    {
        if(state == State.Finished)
        {

        }else if(state == State.Playing | state == State.End)
        {
            retryReason = "Retry";
            ResetLevel();
        }
    }

    // Skipt ahead to the next level (only shows after 3 minutes)
    public void OnPressSkipButton()
    {
        retryReason = "Skipped";
        ExperimentManager.EndTry();
        ExperimentManager.AddTriesToLog(true);
    }

    // Reset the level, submit a try to the experiment manager and start a new one
    public void ResetLevel()
    {
        if (state == State.End)
        {
            EndLevelPanel.SetActive(false);
        }
        else
        {
            ExperimentManager.SetTryTimer();
        }
        
        ExperimentManager.EndTry();
        ExperimentManager.StartTry();
        
        layerManager.SetCollisionsToTrigger(false);
        layerManager.EnableColliders(false);
        layerManager.SelectLayer(0);
        
        
        if (researchVersion == 2)
        {
            coinManager.ResetCoins();
            score = 0;
        }
        playerController.ResetPlayer();
        state = State.Setup;
        ToggleInputs();
    }

    // Listner for the move tool button callback, selects the move tool and changes the boxcolliders of the tiles accordingly
    void MoveToggleValueChanged(Toggle change)
    {
        if (moveToolButton.isOn)
        {
            if (ToolAssistant != null && ToolAssistant.assistentTool == Tool_Assistant_Script.Tool.Move)
            {
                DisableAssistance();
            }
            boxColliderSize = new Vector2(3840, 2160);
            toolManager.ActivateTool(toolManager.MoveTool);
            layerManager.SetBoxColliders(boxColliderSize);
        }
    }

    //disble the in-game assistance
    void DisableAssistance()
    {
        ToolAssistant.showAssistance = false;  
    }

    // Listner for the brush tool button callback, selects the move tool and changes the boxcolliders of the tiles accordingly
    void BrushToggleValueChanged(Toggle change)
    {
        if (brushToolButton.isOn)
        {
            if (ToolAssistant != null && ToolAssistant.assistentTool == Tool_Assistant_Script.Tool.Brush)
            {
                DisableAssistance();
            }
            toolManager.ActivateTool(toolManager.BrushTool);
            layerManager.ResetBoxColliders();
        }
    }

    // Allow or disallow the player to interact with the toggles
    public void ToggleInputs()
    {
        if (state == State.Setup)
        {
            moveToolButton.interactable = true;
            brushToolButton.interactable = true;
        }
        else
        {
            moveToolButton.interactable = false;
            brushToolButton.interactable = false;
        }
    }

    // Depending on the version initiate coins in a scene or not
    private void SetUpResearchVersion(int researchVersion)
    {
        coinManager = GameObject.Find("Coins_Manager").GetComponent<Coin_Manager>();
        if (researchVersion == 2)
        {
            playerController.SetupCoinManager();
            coinManager.SetupCoins();
            requiredScore = 1;
        }
        else
        {
            coinManager.DisableCoins();
            requiredScore = 0;
        }
    }
    
    // Show this screen if the player reaches the finish block
    // set the timer for a try and turn off all the interactable objects in a scene
    public void ShowEndLevelScreen()
    {
        state = State.End;
        ExperimentManager.SetTryTimer();
        layerManager.EnableColliders(false);
        EndLevelPanel.SetActive(true);
        GameObject nextLevelButton = EndLevelPanel.transform.GetChild(2).gameObject;
        GameObject textpanel1 = EndLevelPanel.transform.GetChild(3).gameObject;
        GameObject textpanel2 = EndLevelPanel.transform.GetChild(4).gameObject;
        
        //Determine if the player has passed the level and show the continue button (or not)
        if (score < requiredScore)
        {
            retryReason = "Fail";
            SetText(textpanel1, "Unfortunate!");
            SetText(textpanel2, "Gather at least one coin");
            nextLevelButton.SetActive(false);
        }
        else
        {
            SetText(textpanel1, "Congratulations!");
            SetText(textpanel2, "You completed the level");
            nextLevelButton.SetActive(true);
        }
    }

    // Set the text on the end level panel
    public void SetText(GameObject textPanel ,String text)
    {
        Text textcomponent = textPanel.GetComponent<Text>();
        textcomponent.text = text;
    }

    // Reload the scene after submitting a try
    public void OnRetryButton()
    {
        if (state != State.Finished)
        {
            if (state != State.End)
            {
                ExperimentManager.SetTryTimer();
            }
            retryReason = "Retry";
            ExperimentManager.EndTry();
            ReloadLevel();
        }
    }

    // Submit a try and load the next level
    public void OnNextLevelButton()
    {
        retryReason = "Continue";
        state = State.Setup;
        //LoadNextLevel();
        //Uncomment this for real experiment
        ExperimentManager.EndTry();
        ExperimentManager.AddTriesToLog(true);
    }

    // Reload the level
    private void ReloadLevel()
    {
        state = State.Setup;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //Uncomment this for real experiment
        ExperimentManager.AddTriesToLog(false);
    }


    public void LoadNextLevel()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex + 1);
    }

    //Coroutine that waits for the skip button
    IEnumerator ShowSkipButton()
    {
        yield return new WaitForSeconds(180);
        skipButton.gameObject.SetActive(true);
    }
}
