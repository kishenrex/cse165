using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Reference to the TextMeshProUGUI component
    private float elapsedTime = 0f; // Time elapsed since the start of the game
    private bool isTimerRunning = false; // Flag to control the timer

    // Update is called once per frame
    void Update()
    {
        if(isTimerRunning)
        {
            elapsedTime += Time.deltaTime; // Increment elapsed time
            UpdateTimerText(); // Update the displayed timer text
        }
    }

    public void StartTimer()
    {
        Debug.Log("Entering StartTimer()");
        if (!isTimerRunning)
        {
            elapsedTime = 0f; // Reset elapsed time if the timer is already 
            isTimerRunning = true; // Start the timer}
            Debug.Log("Timer started");
        }
    }
    public void StopTimer()
    {
        if (isTimerRunning)
        {
            isTimerRunning = false; // Stop the timer
            Debug.Log("Timer stopped");
        }
    }
    private void UpdateTimerText()
    {
        // Format the elapsed time as minutes and seconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000) % 1000);
        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds); // Update the text component
    }
    public void ResetTimer()
    {
        elapsedTime = 0f; // Reset elapsed time
        isTimerRunning = false; // Stop the timer
        UpdateTimerText(); // Update the displayed timer text
        Debug.Log("Timer reset");
    }
}
