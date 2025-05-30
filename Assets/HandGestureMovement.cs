using UnityEngine;
using System.Collections.Generic;

public class HandGestureMovement : MonoBehaviour
{
    [Header("Hand Tracking Setup")]
    // Assign OVRHandPrefab_Right from your OVRCameraRig
    [SerializeField] private OVRSkeleton _rightHandSkeleton;

    [Header("Character to Move")]
    // Assign your Mixamo character from the scene
    [SerializeField] private Animator _characterAnimator;
    [SerializeField] private float _characterSpeed = 2.0f;

    [Header("Gesture Detection")]
    // How close finger tips must be to the palm to be considered 'curled'
    [SerializeField] private float _fingerCurlThreshold = 0.06f;

    [Header("Movement Logic")]
    // Set to your ground/floor layer so the ray knows what to hit
    [SerializeField] private LayerMask _groundLayerMask;
    // Optional: A laser or marker to show where the user is pointing
    [SerializeField] private LineRenderer _laserPointer;

    // Private variables to hold joint transforms
    private Transform _rightIndexTip;
    private Transform _rightPalm;
    private Transform _rightMiddleTip;
    private Transform _rightRingTip;
    private Transform _rightPinkyTip;

    private bool _isInitialized = false;
    private Vector3 _targetDestination;
    private bool _isMoving = false;

    #region Initialization
    void Update()
    {
        // The hand skeleton can take a moment to initialize after the app starts.
        // We wait until it's ready before trying to access the joints.
        if (!_isInitialized && _rightHandSkeleton.IsInitialized)
        {
            InitializeHandJoints();
        }

        if (_isInitialized)
        {
            HandleGesture();
            MoveCharacter();
        }
    }

    private void InitializeHandJoints()
    {
        // Find and store the transforms of the key joints we need for our gesture logic.
        // This is more efficient than searching for them every frame.
        foreach (var bone in _rightHandSkeleton.Bones)
        {
            switch (bone.Id)
            {
                case OVRSkeleton.BoneId.Hand_IndexTip:
                    _rightIndexTip = bone.Transform;
                    break;
                case OVRSkeleton.BoneId.Hand_WristRoot: // A good proxy for the palm center
                    _rightPalm = bone.Transform;
                    break;
                case OVRSkeleton.BoneId.Hand_MiddleTip:
                    _rightMiddleTip = bone.Transform;
                    break;
                case OVRSkeleton.BoneId.Hand_RingTip:
                    _rightRingTip = bone.Transform;
                    break;
                case OVRSkeleton.BoneId.Hand_PinkyTip:
                    _rightPinkyTip = bone.Transform;
                    break;
            }
        }

        // Confirm all necessary joints were found
        _isInitialized = _rightIndexTip && _rightPalm && _rightMiddleTip && _rightRingTip && _rightPinkyTip;
        if (_isInitialized) Debug.Log("Hand Joints Initialized Successfully!");
        else Debug.LogError("Failed to initialize all necessary hand joints.");
    }
    #endregion

    #region Gesture Handling
    private void HandleGesture()
    {
        if (IsPointing())
        {
            _isMoving = true;
            DetermineDestination();
            UpdateLaser(true);
            // Tell the animator to switch to a walking/running animation
            _characterAnimator.SetBool("IsWalking", true);
        }
        else
        {
            _isMoving = false;
            UpdateLaser(false);
            // Tell the animator to switch back to the idle animation
            _characterAnimator.SetBool("IsWalking", false);
        }
    }

    private bool IsPointing()
    {
        // Condition 1: Check if the index finger is extended.
        // An extended finger's tip will be far from the palm. We give it a slightly larger threshold.
        bool isIndexExtended = Vector3.Distance(_rightIndexTip.position, _rightPalm.position) > _fingerCurlThreshold * 1.5f;

        // Condition 2: Check if the other fingers are curled.
        // A curled finger's tip will be close to the palm.
        bool isMiddleCurled = Vector3.Distance(_rightMiddleTip.position, _rightPalm.position) < _fingerCurlThreshold;
        bool isRingCurled = Vector3.Distance(_rightRingTip.position, _rightPalm.position) < _fingerCurlThreshold;
        bool isPinkyCurled = Vector3.Distance(_rightPinkyTip.position, _rightPalm.position) < _fingerCurlThreshold;

        // The gesture is active if the index is out and all other fingers are in.
        return isIndexExtended && isMiddleCurled && isRingCurled && isPinkyCurled;
    }
    #endregion

    #region Movement Logic
    private void DetermineDestination()
    {
        // We cast a ray forward from the index finger tip to find the target position.
        Ray ray = new Ray(_rightIndexTip.position, _rightIndexTip.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayerMask))
        {
            // The ray hit a valid surface. This is our destination.
            _targetDestination = hit.point;
        }
        else
        {
            // If we don't hit anything, just project a point a set distance forward.
            _targetDestination = ray.origin + ray.direction * 20f;
        }
    }

    private void MoveCharacter()
    {
        if (!_isMoving || _characterAnimator == null) return;

        // Get the character's transform
        Transform characterTransform = _characterAnimator.transform;

        // Make the character look at the target destination (but don't tilt up/down)
        Vector3 lookAtPosition = new Vector3(_targetDestination.x, characterTransform.position.y, _targetDestination.z);
        characterTransform.LookAt(lookAtPosition);

        // Move the character forward
        characterTransform.position += characterTransform.forward * _characterSpeed * Time.deltaTime;
    }
    #endregion

    #region Visuals
    private void UpdateLaser(bool isActive)
    {
        if (_laserPointer == null) return;
        _laserPointer.enabled = isActive;
        if (isActive)
        {
            _laserPointer.SetPosition(0, _rightIndexTip.position);
            _laserPointer.SetPosition(1, _targetDestination);
        }
    }
    #endregion
}
