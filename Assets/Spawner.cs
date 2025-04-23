using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject tool;
    public Transform spawnPoint;
    public SpawnMenuController menuController;
    
    public void SpawnTool()
    {
        Instantiate(tool, spawnPoint.position, spawnPoint.rotation);
        menuController.HideMenu();
    }
}
