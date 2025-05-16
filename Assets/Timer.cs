using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Threading;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Reference to the main timer TextMeshProUGUI component
    public TextMeshProUGUI penaltyTimerText; // Reference to the penalty countdown TextMeshProUGUI component
    private float elapsedTime = 0f; // Time elapsed since the start of the game
    public bool isTimerRunning = false; // Flag to control the main timer
    public bool isPenaltyActive = false; // Flag to control the penalty countdown
    public float penaltyTimeRemaining = 0f; // Time remaining for the penalty countdown
    public GameObject penaltyPanel; // Reference to the penalty panel GameObject
    public AudioManager audioManager; // Reference to the AudioManager for playing sounds

    // Update is called once per frame
    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime; // Increment elapsed time
            UpdateTimerText(); // Update the displayed timer text
        }

        if (isPenaltyActive)
        {
            penaltyTimeRemaining -= Time.deltaTime; // Decrease penalty time
            UpdatePenaltyTimerText(); // Update the penalty countdown text

            if (penaltyTimeRemaining <= 0f)
            {
                EndPenalty(); // End the penalty when time runs out
            }
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            elapsedTime = 0f; // Reset elapsed time
            isTimerRunning = true; // Start the timer
        }
    }
    public void StopTimer()
    {
        if (isTimerRunning)
        {
            isTimerRunning = false; // Stop the timer
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

    private void UpdatePenaltyTimerText()
    {
        // Format the penalty time as seconds
        int seconds = Mathf.FloorToInt(penaltyTimeRemaining);
        penaltyTimerText.text = string.Format("Countdown: {0:D2}", seconds); // Update the penalty text component
    }

    public void ResetTimer()
    {
        elapsedTime = 0f; // Reset elapsed time
        isTimerRunning = false; // Stop the timer
        UpdateTimerText(); // Update the displayed timer text
    }

    public void PenaltyTimer()
    {
        if (isTimerRunning && !isPenaltyActive)
        {
            //audioManager.PlayCrashSound(); // Play crash sound
            isPenaltyActive = true; // Activate penalty
            penaltyTimeRemaining = 3f; // Set penalty duration to 3 seconds
            StartCoroutine(DisablePlayerMovementAndGrayScreen(penaltyPanel, penaltyTimeRemaining)); // Disable player movement and gray out screen
        }
    }

    public IEnumerator DisablePlayerMovementAndGrayScreen(GameObject penaltyPanel, float penaltyTimeRemaining)
    {
        // Disable player movement
        HandGestureLocomotion handGestureLocomotion = FindObjectOfType<HandGestureLocomotion>();
        if (handGestureLocomotion != null)
        {
            handGestureLocomotion.enabled = false;
        }

        // Enable the penalty panel
        if (penaltyPanel != null)
        {
            penaltyPanel.SetActive(true); // Show the panel
        }

        // Wait for the penalty duration
        yield return new WaitForSeconds(penaltyTimeRemaining);

        // Re-enable player movement
        if (handGestureLocomotion != null)
        {
            handGestureLocomotion.enabled = true;
        }

        // Disable the penalty panel
        if (penaltyPanel != null)
        {
            penaltyPanel.SetActive(false); // Hide the panel
        }
    }

    private void EndPenalty()
    {
        isPenaltyActive = false; // Deactivate penalty
        penaltyTimerText.text = ""; // Clear the penalty countdown text
    }
}
