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

    private bool isMoving = false;

    void Start()
    {
        handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
            return;

        XRHand rightHand = handSubsystem.rightHand;
        XRHand leftHand = handSubsystem.leftHand;

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
            MoveInDirectionOfLeftHand(leftHand);
        }

        // Optional: Camera switch based on left hand
    }

    void MoveInDirectionOfLeftHand(XRHand leftHand)
    {
        if (leftHand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose leftPalmPose))
        {
            Vector3 direction = leftPalmPose.forward; // You can also use another joint for more precise pointing
            Vector3 movement = direction.normalized * Time.deltaTime * 20f; // Movement speed
            transform.position += new Vector3(-movement.x, movement.y, -movement.z); // Move horizontally only
        }
    }


}
