using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Experiment_Manager : MonoBehaviour
{
    //BOF STUFF
    // Imports of JS functions defined in the .jslib in Plugins folder 
    [DllImport("__Internal")]
    public static extern void RedirectBOF();
    [DllImport("__Internal")]
    public static extern void RequestConfigFromJS();

    // A list to store all the tries until they can be send to the database
    public List<SingleTry> DataEntries = new List<SingleTry>();

    // For keeping the time of each try
    private float startTime;
    private float endTime;

    //Researchversion for evaluation tries 1 = No coins, 2 = coins
    private int rv = 2;

    // Database entries -- name doesn't change
    public string dataBaseName = "GameLog";
    public int effort;
    public int enjoyment;
    public string startDateTime;
    public string endDateTime;
    public static int amountOfMoves;

    // Intstances of all other scripts in the scene
    UI_Button_Controller UIController;  //UI & buttons
    Layer_manager LayerManager; // Layer order & selections
    Tool_Manager ToolManager;   // Tool selections & usages
    GameObject canvas;  // UI Canvas

    // Controllers for the evaluation after each stage
    Slider effortSlider;
    Slider enjoymentSlider;
    Button continueButton;

    // Keeps track if all the tries have been sent to the database
    public int amountFormsSend;

    void Start()
    {
        // Setting all start values
        amountFormsSend = 0;
        effort = 0;
        enjoyment = 0;
        amountOfMoves = 0;

        // Creating instances of all objects & scripts based on the scene
        // Evaluation scenes and normal scenes have different tries and objects so this prevents reference errors
        canvas = GameObject.Find("Canvas");
        if(canvas.tag == "EvaluationCanvas")
        {
            StartEvaluationTry();
            effortSlider = canvas.transform.GetChild(1).gameObject.GetComponent<Slider>();
            enjoymentSlider = canvas.transform.GetChild(2).gameObject.GetComponent<Slider>();
            continueButton = canvas.transform.GetChild(3).gameObject.GetComponent<Button>();

        }
        else
        {
            UIController = canvas.GetComponent<UI_Button_Controller>();
            LayerManager = GameObject.Find("Layer_elements").GetComponent<Layer_manager>();
            ToolManager = GameObject.Find("Photoshop_Tools").GetComponent<Tool_Manager>();
            StartTry();
        }
    }

    // Class for storing all the data of a single try in one object
    public class SingleTry
    {
        public string dbName { get; set; }
        public int ResearchVersion { get; set; }
        public int Level { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public float Time { get; set; }
        public int ToolSwitches { get; set; }
        public int LayerSwitches { get; set; }
        public int Moves { get; set; }
        public int Score { get; set; }
        public string Outcome { get; set; }
        public int Effort { get; set; }
        public int Enjoyment { get; set; }

        public SingleTry(string dbName,int ResearchVersion,int Level,string startTime, string endTime, float Time,int ToolSwitches,int LayerSwitches,int Moves,int Score,string Outcome,int Effort,int Enjoyment)
        {
            this.dbName = dbName;
            this.ResearchVersion = ResearchVersion;
            this.Level = Level;
            this.startTime = startTime;
            this.endTime = endTime;
            this.Time = Time;
            this.ToolSwitches = ToolSwitches;
            this.LayerSwitches = LayerSwitches;
            this.Moves = Moves;
            this.Score = Score;
            this.Outcome = Outcome;
            this.Effort = Effort;
            this.Enjoyment = Enjoyment;
        }
    }

    // Start a normal try for a regular level
    public void StartTry()
    {
        //Reset all the values that persist after the "reset" button is pressed
        ToolManager.toolChanges = 0;
        effort = 0;
        enjoyment = 0;
        amountOfMoves = 0; 
        startTime = Time.time;
        startDateTime = System.DateTime.Now.ToString();

    }

    // Start the try for the evaluation scene
    public void StartEvaluationTry()
    {
        startTime = Time.time;
        startDateTime = System.DateTime.Now.ToString();
    }

    // Set the timer for a particular try.
    public void SetTryTimer()
    {
        endTime = Time.time - startTime;
        endDateTime = System.DateTime.Now.ToString();
    }

    // End the evaluation try that happens after each stage
    public void EndEvaluationTry()
    {
        //These tries are the only ones with effort and enjoyment. 
        //They also automatically load the next level when the button is pressed
        SetTryTimer();
        effort = (int)effortSlider.value;
        enjoyment = (int)enjoymentSlider.value;
        var singleTry = new SingleTry(dataBaseName,
                                      rv,
                                      SceneManager.GetActiveScene().buildIndex,
                                      startDateTime,
                                      endDateTime,
                                      endTime, 
                                      0, 
                                      0, 
                                      0, 
                                      0, 
                                      "Continue", 
                                      effort, 
                                      enjoyment);
        DataEntries.Add(singleTry);

        //Uncomment this for a real experiment
        // Depending on testing or deploying use on of the two ways to load the next level
        AddTriesToLog(true); // Sends all the data of the current recorded tries to the database
        //UIController.LoadNextLevel(); // Immediately load the next level
    }
    
    // End the current try by creating a try instance and adding it to the DataEntries List. This list is sent whenever AddTriesToLog is called
    public void EndTry()
    {
        var singleTry = new SingleTry(dataBaseName,
                                      UIController.researchVersion,
                                      SceneManager.GetActiveScene().buildIndex,
                                      startDateTime,
                                      endDateTime,
                                      endTime,
                                      ToolManager.toolChanges, 
                                      LayerManager.amountOfLayerSelection, 
                                      amountOfMoves, 
                                      UIController.score, 
                                      UIController.retryReason, 
                                      effort, 
                                      enjoyment);
        DataEntries.Add(singleTry);
    }

    //Send all tries to database with BOF system and indicate if the next level needs to be loaded
    public void AddTriesToLog(bool loadNextLevel)
    {
        foreach (SingleTry row in DataEntries)
        {
            StartCoroutine(SendFiles(row,loadNextLevel));
        }
    }


    //Create a form containing all the fields needed for the database and send it.
    //Once all forms are send (re)load the appropriate level
    public IEnumerator SendFiles(SingleTry row,bool loadNextlevel)
    {
        // Instead of the URL we can use # to get to the same route as the game was delivered on
        // Alternatively specify the URL of the server with port and route
        // var url = "http://127.0.0.1:5000/game"
        var url = "#";

        //Create a new form and add the right field to it
        WWWForm form = new WWWForm();
        form.AddField("dataBaseName", row.dbName);
        form.AddField("Version", row.ResearchVersion);
        form.AddField("Level", row.Level);
        form.AddField("startTime", row.startTime);
        form.AddField("endTime", row.endTime);
        form.AddField("Time", (int)row.Time);
        form.AddField("Tool Changes", row.ToolSwitches);
        form.AddField("Layer Selections", row.LayerSwitches);
        form.AddField("Moves", row.Moves);
        form.AddField("Score", row.Score);
        form.AddField("Retry Reason", row.Outcome);
        form.AddField("Effort", row.Effort);
        form.AddField("Enjoyment", row.Enjoyment);
        
        //send form
        UnityWebRequest UWRPost = UnityWebRequest.Post(url, form);
        yield return UWRPost.SendWebRequest();

        // The coroutine will handle the process in the background and then continue here if there is an error or if it's done
        // If it succeeded add to the counter for total amount of forms, once all forms are sent, load the next level
        //If an error occurs, load the next level anyway, to prevent players getting stuck
        if (UWRPost.isNetworkError || UWRPost.isHttpError)
        {
            Debug.Log(UWRPost.error);
            LoadLevel(loadNextlevel);
        }
        else
        {
            amountFormsSend++;
            if(amountFormsSend == DataEntries.Count)
            {
                LoadLevel(loadNextlevel);
            }
            
        }
    }

    //If the amount of forms sent is the same as the amount of forms in DataEntries, load the next level
    private void LoadLevel(bool nextLevel)
    {
        if (nextLevel)
        {
            LoadNextLevel();
        }
        else
        {
            ReloadLevel();
        }
    }

    //reload the current level
    private void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Load the next level
    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex+1;
        if(SceneManager.sceneCountInBuildSettings <= nextSceneIndex)
        {
            StopExperiment();
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        
       
    }

    //Stop the experiment and redirect the participant to the next page in BOF
    void StopExperiment()
    {
        RedirectBOF();
    }
}