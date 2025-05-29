using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

public class RaySpatialAnchors : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public ARAnchorManager anchorManager;

    // Start is called before the first frame update  
    void Start()
    {
        rayInteractor.selectEntered.AddListener(SpawnAnchor);
    }

    // Update is called once per frame  
    void Update()
    {

    }

    public void SpawnAnchor(BaseInteractionEventArgs args)
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Pose hitPose = new Pose(hit.point, Quaternion.LookRotation(-hit.normal));
            ARAnchor anchor = anchorManager.AddAnchor(hitPose);

            if (anchor == null)
            {
                Debug.LogError("Failed to create anchor.");
            }
        }
        else
        {
            Debug.LogError("Raycast did not hit any surface.");
        }
    }
}
