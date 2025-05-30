using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTrackingScript : MonoBehaviour
{
    public Camera sceneCamera;
    public OVRHand leftHand;
    public OVRHand rightHand;
    public OVRSkeleton skeleton;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float step;
    private bool isIndexFingerPinching;

    private LineRenderer line;
    private Transform p0;
    private Transform p1;
    private Transform p2;

    private Transform handIndexTipTransform;
    // Add these fields at the top of the class
    public GameObject bigVegas; // Assign in Inspector
    private Animator bigVegasAnimator;
    // Start is called before the first frame update
    void Start()
    {

        // Set initial cube's position in front of user
        transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 1.0f;

        // Assign the LineRenderer component of the cube GameObject to line
        line = GetComponent<LineRenderer>();

        if (bigVegas != null)
        {
            bigVegasAnimator = bigVegas.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Define step value for animation
        step = 5.0f * Time.deltaTime;

        // If left hand is tracked
        if (leftHand.IsTracked)
        {
            // Gather info whether left hand is pinching
            isIndexFingerPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            // Proceed only if left hand is pinching
            if (isIndexFingerPinching)
            {
                // Show the Line Renderer
                line.enabled = true;

                // Animate cube smoothly next to left hand
                pinchCube();

                // Loop through all the bones in the skeleton
                foreach (var b in skeleton.Bones)
                {
                    // If bone is the the hand index tip
                    if (b.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                    {
                        // Store its transform and break the loop
                        handIndexTipTransform = b.Transform;
                        break;
                    }
                }

                // p0 is the cube's transform and p2 the left hand's index tip transform
                // These are the two edges of the line connecting the cube to the left hand index tip
                p0 = transform;
                p2 = handIndexTipTransform;

                // This is a somewhat random point between the cube and the index tip
                // Need to reference as the point that "bends" the curve
                p1 = sceneCamera.transform;
                p1.position += sceneCamera.transform.forward * 0.8f;

                // Draw the line that connects the cube to the user's left index tip and bend it at p1
                DrawCurve(p0.position, p1.position, p2.position);
            }
            // If the user is not pinching
            else
            {
                // Don't display the line at all
                line.enabled = false;
            }
        }
        // Add this after the rightHand.IsTracked check in Update()
        if (rightHand.IsTracked && leftHand.IsTracked)
        {
            {
                Transform rightPoint = rightHand.GetPointerRayTransform();
                if (rightPoint != null && bigVegas != null)
                {
                    // Use the forward direction of the pointer as the walking direction
                    MoveBigVegasTowards(rightPoint.forward);
                }
                else if (bigVegasAnimator != null && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                {
                    bigVegasAnimator.SetBool("IsWalking", false);
                }
            }
        }
        else if (bigVegasAnimator != null)
        {
            bigVegasAnimator.SetBool("IsWalking", false);
        }
    }



    // Add this helper method at the end of the class
    private void MoveBigVegasTowards(Vector3 targetDirection)
    {
        if (bigVegas == null || bigVegasAnimator == null)
            return;

        // Calculate target position a few meters in the pointing direction, but ignore Y (vertical) movement
        Vector3 start = bigVegas.transform.position;
        Vector3 direction = targetDirection.normalized;
        direction.y = 0f; // Prevent vertical movement
        direction = direction.normalized;

        Vector3 targetPos = start + direction * 2.0f; // Move 2 meters in pointing direction

        // Move BigVegas towards the target position, only on the XZ plane
        float moveSpeed = 1.5f; // meters per second
        Vector3 nextPos = Vector3.MoveTowards(start, targetPos, moveSpeed * Time.deltaTime);
        nextPos.y = start.y; // Keep original Y position
        bigVegas.transform.position = nextPos;

        // Rotate BigVegas to face the movement direction, only on the XZ plane
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
            bigVegas.transform.rotation = Quaternion.Slerp(bigVegas.transform.rotation, lookRot, 0.2f);
        }

        // Set walking animation
        bigVegasAnimator.SetBool("IsWalking", true);
    }



    void DrawCurve(Vector3 point_0, Vector3 point_1, Vector3 point_2)
    /***********************************************************************************
    # Helper function that draws a curve between point_0 and point_2, bending at point_1.
    # Gradually draws a line as Quadratic Bézier Curve that consists of 200 segments.
    #
    # Bézier curve draws a path as function B(t), given three points P0, P1, and P2.
    # B, P0, P1, P2 are all Vector3 and represent positions.
    #
    # B = (1 - t)^2 * P0 + 2 * (1-t) * t * P1 + t^2 * P2
    #
    # t is 0 <= t <= 1 representing size / portion of line when moving to the next segment.
    # For example, if t = 0.5f, B(t) is halfway from point P0 to P2.
    ***********************************************************************************/
    {
        // Set the number of segments to 200
        line.positionCount = 200;
        Vector3 B = new Vector3(0, 0, 0);
        float t = 0f;

        // Draw segments
        for (int i = 0; i < line.positionCount; i++)
        {
            // Move to next segment
            t += 0.005f;

            B = (1 - t) * (1 - t) * point_0 + 2 * (1 - t) * t * point_1 + t * t * point_2;
            line.SetPosition(i, B);
        }
    }

    void pinchCube()
    // Places and rotates cube smoothly next to user's left hand
    {
        targetPosition = leftHand.transform.position - leftHand.transform.forward * 0.4f;
        targetRotation = Quaternion.LookRotation(transform.position - leftHand.transform.position);

        transform.position = Vector3.Lerp(transform.position, targetPosition, step);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
    }
}