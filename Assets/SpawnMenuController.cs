using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SpawnMenuController : MonoBehaviour
{
    public GameObject spawnMenu;
    public InputDeviceCharacteristics controllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    public Transform headLocation;
    public float distanceFromCamera = 1.5f;

    private InputDevice controller;
    private bool menuVisible = false;
    // Start is called before the first frame update
    void Start()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);
        if (devices.Count > 0)
        {
            controller = devices[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!controller.isValid)
        {
            Start();
        }
        bool gripPressed = false;
        bool triggerPressed = false;

        controller.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
        controller.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);

        if (gripPressed && triggerPressed && !menuVisible)
        {
            ShowMenu();
        }
    }

    public void ShowMenu()
    {
        Vector3 forward = new Vector3(headLocation.forward.x, 0, headLocation.forward.z).normalized;
        spawnMenu.transform.position = headLocation.position + forward * distanceFromCamera;

        spawnMenu.transform.rotation = Quaternion.LookRotation(forward);

        spawnMenu.SetActive(true);
        menuVisible = true;
    }

    public void HideMenu()
    {
        spawnMenu.SetActive(false);
        menuVisible = false;
    }
}
