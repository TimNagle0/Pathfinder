using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_Behaviour : MonoBehaviour
{
    public Vector3 mousePosition;
    public Vector3 offset;
    // Start is called before the first frame update
    Tool_Manager ToolManager;
    public GameObject linePrefab;
    public GameObject currentLine;

    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;
    public List<Vector2> mousePositions;
    public BoxCollider2D boxCollider;
    public Vector2 lastMousePos;
    void Start()
    {
        ToolManager = GameObject.Find("Photoshop_Tools").GetComponent<Tool_Manager>();
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        toggleCollision(boxCollider, false);
    }

    private void OnMouseDown()
    {
        Experiment_Manager.amountOfMoves++;
        if (ToolManager.ActiveTool == ToolManager.MoveTool)
        {
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
        }else if (ToolManager.ActiveTool == ToolManager.BrushTool)
        {
            Vector2 tempMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CreateLine();
        }
        
    }
    private void OnMouseDrag()
    {
        if (ToolManager.ActiveTool == ToolManager.MoveTool)
        {
            Vector3 curScreenpoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenpoint) + offset;
            transform.position = curPosition;
        }else if(ToolManager.ActiveTool == ToolManager.BrushTool)
        {
          
            Vector2 tempMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distanceToLastPos = Vector2.Distance(tempMousePos, mousePositions[mousePositions.Count - 1]);
            if (isWithinBounds(tempMousePos.x, tempMousePos.y, boxCollider))
            {
                if(distanceToLastPos > 0.1f)
                {
                    UpdateLine(tempMousePos);
                }
            }
        }
        
    }

    public void toggleCollision(Collider2D collider2D, bool isCollision)
    {
        collider2D.enabled = isCollision;
    }

    private bool isWithinBounds(float xPos, float yPos, Collider2D collider)
    {
        float halfWidth = collider.bounds.extents.x;
        float halfHeight = collider.bounds.extents.y;
        bool isWithin = false;
        if ((xPos > (collider.bounds.center.x  - halfWidth)) & (xPos < collider.bounds.center.x + halfWidth))
        {
            if(yPos < (collider.bounds.center.y + halfHeight) & yPos > (collider.bounds.center.y - halfHeight))
            {
                isWithin = true;
            }
        }
        return isWithin;
    }
    private void CreateLine()
    {
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        currentLine.transform.parent = gameObject.transform;
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        edgeCollider = currentLine.GetComponent<EdgeCollider2D>();
        mousePositions.Clear();
        mousePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        lineRenderer.SetPosition(0, mousePositions[0]);
        lineRenderer.SetPosition(1, mousePositions[1]);
        edgeCollider.points = mousePositions.ToArray();
    }

    private void UpdateLine(Vector2 newMousePosition)
    {
        if(currentLine != null)
        {
            mousePositions.Add(newMousePosition);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, newMousePosition);
            edgeCollider.points = mousePositions.ToArray();
        }
        
    }
}
