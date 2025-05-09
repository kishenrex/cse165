using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Hands;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class HandGestureLocomotion : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public XRNode rightHandNode = XRNode.RightHand;
    public XRNode leftHandNode = XRNode.LeftHand;
    public float pinchThreshold = 0.02f; // ~2cm

    public bool isMoving = false;

    public XRHand leftHand;
    public Timer timer;
    public parse parse;

    public AudioManager audioManager;
    public GameObject drone;
    //public Vector3 direction = Camera.main.transform.forward;
    void Start()
    {
        handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
            return;

        XRHand rightHand = handSubsystem.rightHand;
        leftHand = handSubsystem.leftHand;

        if (rightHand.isTracked)
        {
            // Pinch detection
            Vector3 indexTip = rightHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var pose1) ? pose1.position : Vector3.zero;
            Vector3 thumbTip = rightHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var pose2) ? pose2.position : Vector3.zero;

            float pinchDistance = Vector3.Distance(indexTip, thumbTip);

            if (pinchDistance < pinchThreshold)
            {
                Debug.Log("Pinch Detected");
                isMoving = true;
            }
            else
            {
                Debug.Log("Pinch Released");
                isMoving = false;
            }
                
        }

        // Apply movement based on left hand
        if (isMoving && leftHand.isTracked)
        {
            MoveInDirectionOfLeftHand(20f);
            drone.GetComponent<AudioSource>().Play();
            Debug.Log("Moving in direction of left hand");
        }
        if (!isMoving)
        {
            MoveInDirectionOfLeftHand(5f);
            drone.GetComponent<AudioSource>().pitch = 0.5f; // Slow down the drone sound
            Debug.Log("Moving with base speed");
        }
        if(!leftHand.isTracked)
        {
            MoveInDirectionOfLeftHand(0f);
            drone.GetComponent<AudioSource>().Stop();
            Debug.Log("Moving in direction of camera");
        }

        StartRaceWithFistGesture();
        // Optional: Camera switch based on left hand
    }

    public void MoveInDirectionOfLeftHand(float speed)
    {
        if (leftHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTipPose))
        {
            // Transform the local forward vector to world space
            Vector3 direction = indexTipPose.rotation * Vector3.forward;

            // Normalize the direction and apply movement
            Vector3 movement = direction.normalized * Time.deltaTime * speed; // Movement speed
            transform.position += new Vector3(movement.x, movement.y, movement.z); // Move horizontally only
        }
    }

    public void MoveInDirectionOfCam()
    {
        Vector3 direction = Camera.main.transform.forward; // You can also use another joint for more precise pointing
        Vector3 movement = direction.normalized * Time.deltaTime * 0f; // Movement speed
        transform.position += new Vector3(movement.x, movement.y, movement.z); // Move horizontally only
    }
    private IEnumerator StartRaceWithFistGesture()
    {
        float fistHoldTime = 5f; // Time required to hold the fist
        float elapsedTime = 0f;
        
        while (elapsedTime < fistHoldTime)
        {
            if (handSubsystem == null || !handSubsystem.running)
                yield break;

            XRHand rightHand = handSubsystem.rightHand;

            if (rightHand.isTracked)
            {
                // Fist detection: Check if all fingers are curled
                bool isFist = true;
                foreach (XRHandJointID jointID in new[] { XRHandJointID.IndexTip, XRHandJointID.MiddleTip, XRHandJointID.RingTip, XRHandJointID.LittleTip })
                {
                    Vector3 jointPosition = rightHand.GetJoint(jointID).TryGetPose(out var pose) ? pose.position : Vector3.zero;
                    Vector3 palmPosition = rightHand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmPose) ? palmPose.position : Vector3.zero;

                    if (Vector3.Distance(jointPosition, palmPosition) > pinchThreshold)
                    {
                        isFist = false;
                        break;
                    }
                }

                if (isFist)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    elapsedTime = 0f; // Reset if fist is released
                    yield return null;
                }
            }
            else
            {
                elapsedTime = 0f; // Reset if hand is not tracked
                yield return null;
            }
        }

        Debug.Log("Race Started!");
        timer.StartTimer(); // Start the race timer
    }

}
