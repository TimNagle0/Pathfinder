using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Player_Controller : MonoBehaviour
{
    //Variables for other scripts & objects
    UI_Button_Controller UIController; //Script for all the UI elements in a scene
    Layer_manager layerManager; // Script that manages layer order etc.
    Experiment_Manager ExperimentManager; // Experiment manager of the scene
    Coin_Manager CoinManager; // Manager for handeling the coins in a scene
    Rigidbody2D playerBody;  //rigidbody for the player obejct 

    public List<GameObject> enteredLayers = new List<GameObject>(); //List that holds all overlapping layers
    public Vector2 startPosition;   // Start position of the player
    GameObject oldTopLayer;     // Variable for collision
    
    void Start()
    {
        //Instantiate all the managers and objects
        ExperimentManager = GameObject.Find("Experiment_Manager").GetComponent<Experiment_Manager>();
        startPosition = gameObject.transform.position;
        layerManager = GameObject.Find("Layer_elements").GetComponent<Layer_manager>();
        playerBody = gameObject.GetComponent<Rigidbody2D>();
        UIController = GameObject.Find("Canvas").GetComponent<UI_Button_Controller>();
        
        //Turn off all the edge collider in the scene
        layerManager.ResetEdgeColliders(false);
        
    }

    // Instantiate the coin manager, done separately because the scene won't have one if the experiment doesn't use coins
    public void SetupCoinManager()
    {
        CoinManager = GameObject.Find("Coins_Manager").GetComponent<Coin_Manager>();
    }
    // Let the player object roll freely
    public void StartPlayer()
    {
        playerBody.bodyType = RigidbodyType2D.Dynamic;
    }
    // Reset the player at its start position
    public void ResetPlayer()
    {
        playerBody.bodyType = RigidbodyType2D.Static;
        gameObject.transform.position = startPosition;
    }

    /*Pausing player -- meant for debugging and taking pictures
    public void PausePlayer()
    {
        playerBody.bodyType = RigidbodyType2D.Static;
    }*/


    //--------------------------------------------------------------------------------------------------------
    // This is the trigger detection function for the player
    // Coins and other triggers are dealt with by their tags
    // The layer tiles are further processed
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Coins are added towards the score and deleted
        if(other.gameObject.tag == "Coin")
        {
            CoinManager.moveToScore(other.gameObject, UIController.score);
            UIController.score++;
            
            return;
        // DeathPlanes reset the player as if the reset button was pressed
        }else if(other.gameObject.tag == "Respawn")
        {
            UIController.retryReason = "Fail";
            UIController.Invoke("ResetLevel", 1f);
            return;
        }

        //If the trigger is a layer, add it to a list of layers that are currently overlapping eachother
        enteredLayers.Add(other.gameObject);
        GameObject currentTopLayer = layerManager.getTopLayer(enteredLayers);

        //Then get the top layer and check if it is not the same as before the trigger and call collision update
        if (currentTopLayer != oldTopLayer)
        {
            if (oldTopLayer != null)
            {
                CollisionUpdate(currentTopLayer, true);
                CollisionUpdate(oldTopLayer, false);
            }
            else
            {
                CollisionUpdate(currentTopLayer, true);
            }
            oldTopLayer = currentTopLayer;
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Ignore coins and other triggers
        if (collision.gameObject.tag == "Coin")
        {
            return;
        }

        // Remove the exited trigger from the list of overlapping layers and call collision updates
        enteredLayers.Remove(collision.gameObject);
        if (enteredLayers.Count > 0)
        {
            GameObject currentTopLayer = layerManager.getTopLayer(enteredLayers);
            if (currentTopLayer != oldTopLayer)
            {
                CollisionUpdate(currentTopLayer, true);
                CollisionUpdate(oldTopLayer, false);
                oldTopLayer = currentTopLayer;
            }
        }

    }
    //Detect if the finish line has been hit and show the end screen
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Finish")
        {
            playerBody.bodyType = RigidbodyType2D.Static;
            UIController.state = UI_Button_Controller.State.Finished;
            //UIController.ShowEndLevelScreen();
            UIController.Invoke("ShowEndLevelScreen", 1.5f);
            
        }

    }
    // The collision update for the EdgeColliders of the layers
    // Turns off/on the edge collision for all the children of a layer tile
    public void CollisionUpdate(GameObject layer, bool enableCollision)
    {
        List<EdgeCollider2D> children = layer.GetComponentsInChildren<EdgeCollider2D>().ToList();
        foreach (EdgeCollider2D child in children)
        {
            layer.GetComponent<Tool_Behaviour>().toggleCollision(child, enableCollision);
        }
        
    }
    

}
