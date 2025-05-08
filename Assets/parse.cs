using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using TMPro;


[RequireComponent(typeof(LineRenderer))] // Ensure LineRenderer is present
public class parse : MonoBehaviour
{
	public TextAsset file;
	public GameObject wayPoint;

    Vector3[] positions;

    [Header("Arrow Customization")]
    public Material lineMaterial; // Assign a particle or unlit material here!
    public Color arrowColor = Color.yellow;
    public float lineWidth = 0.05f;
    public float arrowHeadAngle = 20.0f;
    public float arrowHeadLength = 0.25f;

    [Header("Text Display")]
    public Color textColor = Color.white;
    public float textSize = 1.0f;
    public Vector3 textOffset = new Vector3(0, 0.2f, 0); // Offset text slightly above the midpoint
    public bool makeTextFaceCamera = true; // Should the text always face the camera?

    // Components
    private LineRenderer lineRenderer;
    private TextMeshPro textMeshPro;
    private GameObject textObject; // Reference to the child object holding the TMP component
    private Camera mainCamera;

    public GameObject XRrig;

    public Collider machupicchu;

    public Material reached;

    public Vector3[] lastCheckpoint = new Vector3[1];
    public Vector3[] currentCheckpoint = new Vector3[1];
    public int currentIndex = 0;

    public HandGestureLocomotion handGestureLocomotion;
    public Timer timer;
    public Vector3 startPoint;
    public Vector3 endPoint;
    void ParseFile()
	{
		float ScaleFactor = 1.0f / 39.37f;
		
		string content = file.ToString();
		string[] lines = content.Split('\n');
        positions = new Vector3[lines.Length];
        for (int i = 0; i < lines.Length; i++)
		{
			string[] coords = lines[i].Split(' ');
			Vector3 pos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
			positions[i] = pos * ScaleFactor;
		}
        currentCheckpoint[0] = positions[0];
        startPoint = positions[0];
        endPoint = positions[positions.Length-1];
        //return positions;
    }


    void Start()
	{	
        ParseFile();
		foreach (Vector3 pos in positions)
		{

            Debug.Log(pos);
			Instantiate(wayPoint, pos, Quaternion.identity);
            
        }
        //Debug.DrawLine(wayPoint.transform.position, positions[0], Color.green, 5f);
        //LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        //lineRenderer.startWidth = 0.1f;
        //lineRenderer.endWidth = 0.1f;
        //lineRenderer.SetPosition(0, positions[0]); // Start point
        //lineRenderer.SetPosition(1, positions[1]); // End point
        // --- Setup Line Renderer ---
        lineRenderer = GetComponent<LineRenderer>();

        // Assign material if provided, otherwise use a default particle material
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // Fallback to a common particle shader material if none assigned
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        // Set parameters
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 5; // Start, End, HeadTip1, End, HeadTip2
        lineRenderer.useWorldSpace = true; // Arrow moves with objects

        // --- Setup TextMeshPro ---
        // Create a child GameObject to hold the TextMeshPro component
        textObject = new GameObject("Arrow Text");
        textObject.transform.SetParent(this.transform, false); // Attach to this object, keep local orientation

        textMeshPro = textObject.AddComponent<TextMeshPro>();
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.fontSize = textSize;
        textMeshPro.color = textColor;
        // Optional: Disable Mesh Renderer casting shadows/receiving shadows if desired
        // MeshRenderer tmProRenderer = textObject.GetComponent<MeshRenderer>();
        // if (tmProRenderer != null) {
        //     tmProRenderer.receiveShadows = false;
        //     tmProRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // }

        // Get main camera reference
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("RuntimeArrowDisplay: Main Camera not found! Text cannot face camera.", this);
        }

        XRrig.transform.position = positions[0];
        Vector3 up = new Vector3(0, 1.0f, 0);
        XRrig.transform.rotation = Quaternion.LookRotation(positions[1] - positions[0], up);
        //timer.StartTimer();
        //Debug.Log("Timer starting");
    }

    void Update()
    {

        // Get positions
        Vector3 camPos = Camera.main.transform.position;
        Vector3 startPos = camPos - new Vector3(0, 0.5f, 0);
        Vector3 endPos = currentCheckpoint[0];

        // Calculate direction and distance
        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;

        // --- Update Line Renderer ---
        if (distance > 0.01f) // Only draw if distance is significant
        {
            lineRenderer.startColor = arrowColor;
            lineRenderer.endColor = arrowColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            // Calculate arrowhead points
            Vector3 normalizedDirection = direction.normalized;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
            Vector3 headTip1 = endPos + right * arrowHeadLength;
            Vector3 headTip2 = endPos + left * arrowHeadLength;

            // Set Line Renderer positions
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            lineRenderer.SetPosition(2, headTip1);
            lineRenderer.SetPosition(3, endPos); // Go back to the endpoint for the second head line
            lineRenderer.SetPosition(4, headTip2);
        }
        else
        {
            // If distance is too small, just draw a tiny segment or nothing
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos); // Effectively hides it
            // Or disable it: lineRenderer.enabled = false; (handled above)
        }


        // --- Update Text Mesh Pro ---
        if (textMeshPro != null)
        {
            // Calculate midpoint and apply offset
            textMeshPro.transform.position = startPos + direction * 0.1f + textOffset;

            // Set text content
            string distanceText = $"Dist: {distance:F2}";
            textMeshPro.text = $"{distanceText}";

            // Update text properties
            textMeshPro.fontSize = textSize;
            textMeshPro.color = textColor;

            // Make text face the camera (optional)
            if (makeTextFaceCamera && mainCamera != null)
            {
                // Rotate the text object to face the camera
                textMeshPro.transform.rotation = Quaternion.LookRotation(textMeshPro.transform.position - mainCamera.transform.position);

                // Alternative (Billboard effect - often looks better for text):
                // textMeshPro.transform.rotation = mainCamera.transform.rotation;
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.transform.position == startPoint && other.transform.position == currentCheckpoint[0])
        {
            timer.StartTimer();
        }
        if (other.transform.position == endPoint && other.transform.position == currentCheckpoint[0])
        {
            timer.StopTimer();
        }
        if (other.transform.position == currentCheckpoint[0] && other.gameObject.name == "WayPoint(Clone)")
        {
            Debug.Log("Trigger with " + other.name);
            Debug.Log(other.name);

            other.gameObject.GetComponent<Renderer>().material = reached;


            Debug.Log("Position count: " + positions.Length);
            lastCheckpoint[0] = positions[currentIndex];
            currentCheckpoint[0] = positions[++currentIndex];

            Debug.Log("Last Checkpoint: " + lastCheckpoint[0]);
            Debug.Log("Current Checkpoint: " + currentCheckpoint[0]);
        }
        else if (other == machupicchu)
        {
            Debug.Log("Hit terrain with " + other.name);
            XRrig.transform.position = lastCheckpoint[0];
            XRrig.transform.rotation = Quaternion.LookRotation(positions[currentIndex] - lastCheckpoint[0]);

        }
    }

}