using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class parse : MonoBehaviour
{
	public TextAsset file;
	public GameObject wayPoint;

    List<Vector3> positions;

    void ParseFile()
	{
		float ScaleFactor = 1.0f / 39.37f;
		positions = new List<Vector3>();
		string content = file.ToString();
		string[] lines = content.Split('\n');
		for (int i = 0; i < lines.Length; i++)
		{
			string[] coords = lines[i].Split(' ');
			Vector3 pos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
			positions.Add(pos * ScaleFactor);
		}
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
        Debug.DrawLine(wayPoint.transform.position, positions[0], Color.green, 5f);
    }

	void Update()
	{
        Debug.DrawLine(wayPoint.transform.position, positions[0], Color.green, 5f);
    }
    void OnTriggerEnter(Collider other)
    {
        //Destroy(gameObject);
		Debug.Log(other.name);
        Destroy(other.gameObject);
        positions.RemoveAt(0);

    }
    //  void OnCollisionEnter(Collision collision)
    //  {
    //      foreach (ContactPoint contact in collision.contacts)
    //      {
    //          Debug.DrawRay(contact.point, contact.normal, Color.white);
    //      }
    //Debug.Log("Collision with " + collision.gameObject.name);
    //  }

}