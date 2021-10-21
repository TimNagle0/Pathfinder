using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class layer_movement : MonoBehaviour
{
    public Vector3 mousePosition;   //Vector for storing the current mouse position
    public Vector3 offset;  // Vector for storing the offset between the mouse and the layer positions
    private const float layerDistance = 0.283f; // Distance / width of 1 layer
    private const float topLayerPosition = 3.2f;    // Worldposition of the top layer
    [SerializeField] public int layerSlot;  // The current position of a layer in the hierarchy
    private int lastLayerSlot;
    Layer_manager LayerManager;
    

    void Start()
    {
        LayerManager = GameObject.Find("Layer_elements").GetComponent<Layer_manager>();
        setPosition(layerSlot);
    }

    private void OnMouseUpAsButton()
    {
        LayerManager.SelectLayer(layerSlot);
    }

    private void OnMouseDown()
    {
        lastLayerSlot = layerSlot;
        //bringing the layer to the foreground
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        //determine offset between the mouse position and the layer
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(gameObject.transform.position.x, Input.mousePosition.y));
    }
    private void OnMouseDrag()
    {
        transform.position = GetCurrentPosition();
    }

    private void OnMouseUp()
    {
        Vector3 curPosition = GetCurrentPosition();
        layerSlot = Mathf.RoundToInt(Mathf.Abs(topLayerPosition - curPosition.y) / layerDistance);
        curPosition.y = topLayerPosition - layerSlot * layerDistance;
        curPosition.z = 0;
        transform.position = curPosition;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
        if (lastLayerSlot != layerSlot)
        {
            LayerManager.UpdateLayerPositions(lastLayerSlot, layerSlot);
        }
        
    }

    private Vector3 GetCurrentPosition()
    {
        //Getting the current y position of the layer and combining it with the offset
        Vector3 curScreenpoint = new Vector3(gameObject.transform.position.x, Input.mousePosition.y);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenpoint) + offset;
        return curPosition;
    }

    public void setPosition(int newLayerSlot)
    {
        Vector3 p = transform.position;
        layerSlot = newLayerSlot;
        p.y = topLayerPosition - layerSlot * layerDistance;
        transform.position = p;
    }
}
