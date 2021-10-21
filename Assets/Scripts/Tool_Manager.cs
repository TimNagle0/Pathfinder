using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tool_Manager : MonoBehaviour
{
    public GameObject MoveTool;     // Add the Move_Tool_elements childobject in inspector
    public GameObject BrushTool;    // Add the Brush_Tool_elements childobject in inspector
    public GameObject ActiveTool;   // Variable for storing the current tool

    [SerializeField] Layer_manager LayerManager; // Reference to the layer manager in the inspector.

    public int toolChanges;     // Variable for tracking the amount of tool changes a player makes, only needed for gathering player data

    //Dictionary with tools and their respective UI elements. To expand, add childobjects to the tool with the elements.
    public Dictionary<GameObject,List<SpriteRenderer>> ToolDictionary = new Dictionary<GameObject, List<SpriteRenderer>>();

    void Start()
    {
        toolChanges = 0;
        ToolDictionary.Add(MoveTool, MoveTool.GetComponentsInChildren<SpriteRenderer>().ToList());
        ToolDictionary.Add(BrushTool, BrushTool.GetComponentsInChildren<SpriteRenderer>().ToList());
        
        //Disable each of the Tool UI elements.
        foreach (var spriteList  in ToolDictionary.Values)
        {
            UpdateSprites(spriteList, false);
        }
        
    }
    // Change the active tool to the passed tool, also updates UI sprites.
    public void ActivateTool(GameObject tool)
    {
        if (tool != null)
        {
            toolChanges++;
            DeActivateTool(ActiveTool);
            ActiveTool = tool;
            UpdateSprites(ToolDictionary[ActiveTool], true);
        }
    }

    // Deactivate the given tool
    public void DeActivateTool(GameObject tool)
    {
        if(tool != null)
        {
            ActiveTool = null;
            LayerManager.ResetBoxColliders();
            UpdateSprites(ToolDictionary[tool], false);
        }
        
    }

    private void UpdateSprites(List<SpriteRenderer> SpriteList, bool spriteState)
    {
        foreach (SpriteRenderer sprite in SpriteList)
        {
            sprite.enabled = spriteState;
        }
    }
}
