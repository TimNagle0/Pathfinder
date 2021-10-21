using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// This script handles all the layer information and manipulation
// A layer consists of a layer tile, colliders, and a layer selection sprite on the right of the screen
public class Layer_manager : MonoBehaviour
{   
    // Create lists for storing information
    public List<Layer> layers = new List<Layer>(); // All the layers currently in the scene
    public Sprite[] textures = new Sprite[4];   // Sprites for the layer selection gameobjects (each layer needs its own sprite)

    // Dictionary to store the connections between the layer selection and the layer tile of each layer 
    public Dictionary<GameObject, GameObject> layerConnections = new Dictionary<GameObject, GameObject>();

    //Variables for storing information
    public int amountOfLayerSelection;  // Data for the experiment
    private Layer currentSelectedLayer; // Keeps track of the current working layer

    // Instances of other scripts
    Player_Controller PlayerController;
    Tool_Assistant_Script ToolAssistant;
    
    Color selectedColor = new Color(0.7f,0.7f,0.7f);


    void Start()
    {
        // Instantiating all objects & scripts
        GameObject assistant = GameObject.Find("Tool_Assistant");
        if (assistant != null)
        {
            ToolAssistant = assistant.GetComponent<Tool_Assistant_Script>();
        }
        PlayerController = GameObject.Find("Ball").GetComponent<Player_Controller>();

        // Find all layers in the scene, assign the layer tile and selection object, and assign a sprite to the selection object
        // Then create a layer object and add it to the list of layers in the scene
        List<GameObject> templayers;
        templayers = GameObject.FindGameObjectsWithTag("Layer").ToList();
        amountOfLayerSelection = 0;
        foreach (GameObject layer in templayers)
        {
           GameObject layerSelection = layer.transform.GetChild(1).gameObject;
           GameObject layerTile = layer.transform.GetChild(0).gameObject;
           layerSelection.GetComponent<SpriteRenderer>().sprite = textures[templayers.IndexOf(layer)];
           if (layerTile != null && layerSelection != null)
            {
                layers.Add(new Layer(layer, layerTile, layerSelection));
            }
            
        }

        SetBoxColliders(new Vector2(0,0));
        UpdateLayerOrder();
        
    }

    // Class to combine all the layer elements into one object
    public class Layer
    {
        public GameObject layer;
        public GameObject layerTile;
        public GameObject layerSelection;
        public int layerPosition;

        public Layer (GameObject l, GameObject lt, GameObject ls)
        {
            layer = l;
            layerTile = lt;
            layerSelection = ls;
        }
    }

    // Whenever the order of the layers if being changed, this function is called
    // It determines which layer was moved to which position and changes the positions of all other layers accordingly in the list on the right of the screen
    public void UpdateLayerPositions(int oldPosition, int newPosition)
    {
        if(ToolAssistant != null && ToolAssistant.assistentTool == Tool_Assistant_Script.Tool.Layer)
        {
            ToolAssistant.showAssistance = false;
        }
        Experiment_Manager.amountOfMoves++; // Data for the experiment
        
        // Determine if the layer was moved up or down in the list on the right side of the screen
        // If it was moved up, place the layer and move all the other sprites down & vice versa for the other direction
        List<Layer> tempLayers = layers;
        if (newPosition > oldPosition)
        {
            if (newPosition >= tempLayers.Count)
            {
                newPosition = tempLayers.Count;
            }
            tempLayers.Insert(newPosition, layers[oldPosition]);
            tempLayers.RemoveAt(oldPosition);
        }
        else if (newPosition < oldPosition)
        {
            tempLayers.Insert(newPosition, layers[oldPosition]);
            tempLayers.RemoveAt(oldPosition + 1);
        }
        else
        {
            return;
        }
        // After all the layers are moved, set the changes and update the order of the layer tiles accordingly
        layers = tempLayers;
        UpdateLayerOrder();    
    }

    // This updates the order of the layer tiles according to the positions of the layer selecions on the right side of the screen
    void UpdateLayerOrder()
    {
        // Get the current position of each layer and move if back or forward based on its position in the layer selections
        foreach (Layer currentLayer in layers)
        {
            currentLayer.layer.GetComponentInChildren<layer_movement>().setPosition(layers.IndexOf(currentLayer));
            Vector3 zPosLayer = currentLayer.layerTile.transform.position;
            zPosLayer.z = 10 + (layers.IndexOf(currentLayer) * 0.1f);
            currentLayer.layerTile.transform.position = zPosLayer;
        }
    }

    // The box colliders of the layer tiles need to change between different tools
    // These functions take care of this by changing the size or resetting them to the original image
    // Sets a size for the box collider || for the move tool this is very large
    public void SetBoxColliders(Vector2 newSize)
    {
        foreach (Layer currentLayer in layers)
        {
            currentLayer.layerTile.GetComponent<BoxCollider2D>().size = newSize;
        }
    }

    // Resets the box collider of a tile to the original size of the sprite, used for the brush tool and during play
    public void ResetBoxColliders()
    {
        foreach(Layer currentLayer in layers)
        {
            Vector2 spriteSize = currentLayer.layerTile.GetComponent<SpriteRenderer>().bounds.size;
            BoxCollider2D currentCollider = currentLayer.layerTile.GetComponent<BoxCollider2D>();
            currentCollider.size = spriteSize;
        }
    }

    // Reset all the edge colliders of the layers
    public void ResetEdgeColliders(bool enableCollision)
    {
        foreach (Layer layer in layers)
        {
            PlayerController.CollisionUpdate(layer.layerTile, false);
        }
            
    }

    // Whenever a player selects a layer by clicking on the layer selection, this is called
    // Sets the current selected layer variable to the right layer, disables the collision for the old layer tile, and enables it for the current tile.
    // Also updates the data for the experiment and changes the color of the sprite on the right of the screen

    public void SelectLayer(int layerSlot)
    {
        // If the layer is not currently selected add a selection to the data for the experiment
        if(currentSelectedLayer != layers[layerSlot])
        {
            amountOfLayerSelection++;
        }

        //Turn off currently selected layer and box collider of the associated tile
        if(currentSelectedLayer != null)
        {
            UpdateSelectedLayer(currentSelectedLayer, Color.white, false);
        }
        //Turn on the new layer and box collider
        currentSelectedLayer = layers[layerSlot];
        UpdateSelectedLayer(currentSelectedLayer, selectedColor, true);
    }

    // Toggles the collision for the layer tile of a layer, also changes the color for the layer selection sprite
    void UpdateSelectedLayer(Layer layer,Color color, bool isSelected)
    {
        layer.layerTile.GetComponent<Tool_Behaviour>().toggleCollision(layer.layerTile.GetComponent<BoxCollider2D>(), isSelected);
        layer.layerSelection.GetComponent<SpriteRenderer>().color = color;
    }


    // This function returns the top layer of the scene
    // The passed variable is a list of all the layers that the player object is currently overlapping
    // To get the top layer, the list of layers (which is from top to bottom based on the right side of the screen) is compared with the passed list of layers
    // The first match between the lists, and thus the top layer is returned
    public GameObject getTopLayer(List<GameObject> currentLayers)
    {
        GameObject tempLayer = null;
        foreach (Layer layer in layers)
        {
            if (currentLayers.Contains(layer.layerTile))
            {
                tempLayer = layer.layerTile;
                return tempLayer;
            }
        }
        return tempLayer;
    }

    // Set all the boxcolliders to trigger || used when pressing play
    public void SetCollisionsToTrigger(bool setTrigger)
    {
        foreach (Layer layer in layers)
        {
            layer.layerTile.GetComponent<BoxCollider2D>().isTrigger = setTrigger;
        }
    }

    // Resetting all the boxcolliders
    public void EnableColliders(bool setEnable)
    {
        foreach(Layer layer in layers)
        {
            layer.layerTile.GetComponent<BoxCollider2D>().enabled = setEnable;
        }
    }
}
