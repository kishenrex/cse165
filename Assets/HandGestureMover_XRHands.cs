using UnityEngine;
using UnityEngine.XR.Hands; // Required for XR Hands
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management; // For XROrigin

public class HandGestureMover_XRHands : MonoBehaviour
{
    [Header("Tracking Setup")]
    [SerializeField] private Handedness trackedHand = Handedness.Left; // Which hand triggers movement
    [SerializeField] private Transform xrOriginTransform; // Assign the XR Origin's Transform

    [Header("Pinch Detection")]
    [SerializeField] private float pinchThreshold = 0.03f; // Distance in meters to consider a pinch

    [Header("Movement Settings")]
    [SerializeField] private float movementScale = 1.0f;
    [SerializeField] private bool useInvertedMovement = true; // True for "pulling" the world movement

    private XRHandSubsystem handSubsystem;
    private bool isPinching = false;
    private Vector3 pinchAnchorPosition; // World position where pinch started/last updated
    private XRHandJointID thumbTipID = XRHandJointID.ThumbTip;
    private XRHandJointID indexTipID = XRHandJointID.IndexTip;
    private XRHandJointID palmID = XRHandJointID.Palm; // Use Palm or Root for movement anchor

    void Start()
    {
        if (xrOriginTransform == null)
        {
            Debug.LogWarning($"{nameof(HandGestureMover_XRHands)}: XR Origin Transform not assigned. Attempting find.");
            // A simple way to find it if not assigned, assuming only one XROrigin
            var xrOrigin = FindObjectOfType<XROrigin>(); // From Unity.XR.CoreUtils
            if (xrOrigin != null)
            {
                xrOriginTransform = xrOrigin.transform;
            }
            else
            {
                Debug.LogError($"{nameof(HandGestureMover_XRHands)}: Could not find XROrigin in the scene!");
                enabled = false; // Disable script if no origin
                return;
            }
        }

        InitializeHandSubsystem();
    }

    void InitializeHandSubsystem()
    {
        // Find the active XRHandSubsystem
        var subsystems = new List<XRHandSubsystem>();
        XRHandSubsystem m_Subsystem =
            XRGeneralSettings.Instance?
                .Manager?
                .activeLoader?
                .GetLoadedSubsystem<XRHandSubsystem>();
        Debug.Log($"Unity Doc XRHandSubsystem found: {m_Subsystem != null}");

        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
            Debug.Log($"XRHandSubsystem found: {handSubsystem.running}");
            // Optional: Subscribe to events for more robust handling if needed
            // handSubsystem.trackingAcquired += OnTrackingAcquired;
            // handSubsystem.trackingLost += OnTrackingLost;
            // handSubsystem.updatedHands += OnUpdatedHands;
        }
        else
        {
            Debug.LogError($"{nameof(HandGestureMover_XRHands)}: No XRHandSubsystem found!");
            enabled = false;
        }
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            Debug.Log("Waiting for XRHandSubsystem to start...");
            return; // Wait for subsystem
        }

        // Get the correct hand data
        XRHand hand = (trackedHand == Handedness.Left) ? handSubsystem.leftHand : handSubsystem.rightHand;
        Debug.Log($"Tracking {trackedHand} hand: {hand.isTracked}");

        if (!hand.isTracked)
        {
            Debug.Log("START PINCHING");
            // If hand tracking is lost while pinching, cancel the pinch
            if (isPinching)
            {
                isPinching = false;
                Debug.Log("Pinch cancelled - Hand tracking lost");
            }
            return; // Hand not tracked
        }

        // --- Pinch Detection ---
        bool pinchDetectedThisFrame = CheckPinch(hand);

        // --- State Management & Movement ---
        Vector3 currentAnchorPosition = GetAnchorPosition(hand); // Get stable position (e.g., Palm)

        if (pinchDetectedThisFrame && !isPinching)
        {
            // Pinch Started
            isPinching = true;
            pinchAnchorPosition = currentAnchorPosition; // Store starting position
            Debug.Log("Pinch Started");
        }
        else if (pinchDetectedThisFrame && isPinching)
        {
            // Pinch Held - Calculate and Apply Movement
            Debug.Log("You are pinching!");
            if (xrOriginTransform != null && currentAnchorPosition != Vector3.zero && pinchAnchorPosition != Vector3.zero)
            {
                Vector3 handDelta = currentAnchorPosition - pinchAnchorPosition;
                Vector3 movement = useInvertedMovement ? -handDelta : handDelta;

                // Optional: Zero out vertical movement
                // movement.y = 0;

                xrOriginTransform.position += movement * movementScale;

                // IMPORTANT: Update the anchor for the *next* frame's delta calculation
                // This makes it feel like continuously dragging from the current position
                pinchAnchorPosition = currentAnchorPosition;
            }
        }
        else if (!pinchDetectedThisFrame && isPinching)
        {
            // Pinch Released
            isPinching = false;
            Debug.Log("Pinch Released");
        }
    }

    // Checks if the specified hand is currently pinching
    private bool CheckPinch(XRHand hand)
    {
        // Get Thumb Tip joint
        var thumbTipJoint = hand.GetJoint(thumbTipID);
        // Get Index Tip joint
        var indexTipJoint = hand.GetJoint(indexTipID);

        // Try get poses (position and rotation)
        bool thumbPoseValid = thumbTipJoint.TryGetPose(out Pose thumbPose);
        bool indexPoseValid = indexTipJoint.TryGetPose(out Pose indexPose);

            if (thumbPoseValid && indexPoseValid)
            {
                // Calculate distance between the tips
                float distance = Vector3.Distance(thumbPose.position, indexPose.position);
                // Return true if distance is below threshold
                return distance < pinchThreshold;
            }
        // If data is invalid or untracked, assume not pinching
        return false;
    }

    // Gets a stable anchor position from the hand (e.g., Palm or Root) in world space
    private Vector3 GetAnchorPosition(XRHand hand)
    {
        // Using Palm joint is usually a good balance of stability and representation
        var anchorJoint = hand.GetJoint(palmID);
        // Alternative: Use the hand's root pose
        // if (hand.rootPose.HasValue) return hand.rootPose.Value.position;


        if (anchorJoint.TryGetPose(out Pose anchorPose))
        {
            return anchorPose.position; // Pose position is already in world space
        }

        // Fallback or if anchor joint isn't tracked
        return Vector3.zero; // Indicates an invalid position
    }


    // --- Optional Event Handlers (Uncomment if needed) ---
    // private void OnDestroy() {
    //     if (handSubsystem != null) {
    //         handSubsystem.trackingAcquired -= OnTrackingAcquired;
    //         handSubsystem.trackingLost -= OnTrackingLost;
    //         handSubsystem.updatedHands -= OnUpdatedHands;
    //     }
    // }

    // private void OnTrackingAcquired(XRHand hand) {
    //     Debug.Log($"Tracking acquired: {hand.handedness}");
    // }

    // private void OnTrackingLost(XRHand hand) {
    //     Debug.Log($"Tracking lost: {hand.handedness}");
    //     // If the pinching hand loses tracking, ensure pinch state is reset
    //     if (hand.handedness == trackedHand && isPinching) {
    //         isPinching = false;
    //         Debug.Log("Pinch cancelled - Tracking lost event");
    //     }
    // }

    // // Called every time hand data is updated - can be performance intensive
    // private void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType) {
    //     // Can check updateSuccessFlags and updateType if needed
    //     // Usually, polling in Update() as shown above is sufficient for this use case.
    // }
}