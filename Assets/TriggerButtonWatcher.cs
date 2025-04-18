using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]

public class TriggerButtonWatcher : MonoBehaviour
{
    public TriggerButtonEvent TriggerButtonPress;

    private bool lastButtonState = false;
    private List<InputDevice> devicesWithTriggerButton;

    private void Awake()
    {
        if (TriggerButtonPress == null)
        {
            TriggerButtonPress = new TriggerButtonEvent();
        }

        devicesWithTriggerButton = new List<InputDevice>();
    }

    void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
        Debug.Log("Devices Enabled");
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        devicesWithTriggerButton.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out discardedValue))
        {
            devicesWithTriggerButton.Add(device); // Add any devices that have a primary button.
            Debug.Log("Devices Connected");
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (devicesWithTriggerButton.Contains(device))
            devicesWithTriggerButton.Remove(device);
    }

    void Update()
    {
        bool tempState = false;
        foreach (var device in devicesWithTriggerButton)
        {
            bool triggerButtonState = false;
            tempState = device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonState) // did get a value
                        && triggerButtonState // the value we got
                        || tempState; // cumulative result from other controllers
        }

        if (tempState != lastButtonState) // Button state changed since last frame
        {
            TriggerButtonPress.Invoke(tempState);
            lastButtonState = tempState;
        }
    }
}

public class TriggerButtonEvent : UnityEvent<bool> { }

