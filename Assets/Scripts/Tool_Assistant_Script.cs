using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_Assistant_Script : MonoBehaviour
{
    private float startTime;
    private float assistanceDelay;
    private const float period = 2f;
    public float transparencyFactor = 0;
    private Color curTransperency;
    private GameObject assistanceObject;
    private SpriteRenderer assistanceRenderer;
    public bool showAssistance;
    public enum Tool {Move,Brush,Layer};
    public Tool assistentTool;
    // Start is called before the first frame update
    void Start()
    {
        
        assistanceObject = gameObject.transform.GetChild(0).gameObject;
        assistanceRenderer = assistanceObject.GetComponent<SpriteRenderer>();
        assistanceRenderer.color = new Color(1f, 1f, 1f, 0f);
        assistanceDelay = 60f;
        showAssistance = true;
        StartCoroutine(BlinkAssistance(assistanceDelay));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator BlinkAssistance(float delay)
    {
        Debug.Log("Started Coroutine");
        yield return new WaitForSeconds(delay);
        while (showAssistance)
        {
            Debug.Log("Updating transperancy");
            float cycles = Time.time / period;
            const float tau = Mathf.PI * 2;
            float rawSineWave = Mathf.Sin(cycles * tau);
            transparencyFactor = (rawSineWave / 2f) + 0.5f;
            curTransperency = assistanceRenderer.color;
            curTransperency.a = transparencyFactor;
            assistanceRenderer.color = curTransperency;
            yield return new WaitForSeconds(0.05f);
            
        }
        StartCoroutine(DisableAssistance());    

    }
    IEnumerator DisableAssistance()
    {
        while (assistanceRenderer.color.a > 0.01f)
        {
            curTransperency = assistanceRenderer.color;
            float cycles = Time.time / period;
            const float tau = Mathf.PI * 2;
            float rawSineWave = Mathf.Sin(cycles * tau);
            transparencyFactor = (rawSineWave / 2f) + 0.5f;
            
            curTransperency.a = transparencyFactor;
            assistanceRenderer.color = curTransperency;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
