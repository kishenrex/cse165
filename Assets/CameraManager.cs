using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCam;
    public Camera thirdpersonCam;

    public KeyCode switchKey = KeyCode.C;

    private bool isThirdPersonView = false;
    private void Awake()
    {
       if(mainCam == null || thirdpersonCam == null)
        {
            Debug.Log("Cams not assigned");
            enabled = false;
            return;
        }
    }
    void ActivateMainCam()
    {
        if (mainCam != null)
        {
            mainCam.enabled = true;
            mainCam.gameObject.SetActive(true);
        }
        if (thirdpersonCam != null)
        {
            thirdpersonCam.enabled = false;
        }
    }
    void Start()
    {

        ActivateMainCam();
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            ToggleCam();
        }
    }
    void ToggleCam()
    {
            isThirdPersonView = !isThirdPersonView;
            if (isThirdPersonView)
            {
                ActivateThirdCam();
            }
            else
            {
                ActivateMainCam();
            }
    }


        void ActivateThirdCam()
        {
            if(mainCam != null)
            {
                mainCam.gameObject.SetActive(false);
            }
            if(thirdpersonCam != null)
            {
                thirdpersonCam.gameObject.SetActive(true);
                thirdpersonCam.enabled = true;
            }
        }
}
