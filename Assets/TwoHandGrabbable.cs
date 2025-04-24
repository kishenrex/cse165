using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TwoHandGrabbable : XRGrabInteractable
{
    private XRBaseInteractor firstInteractor = null;
    private XRBaseInteractor secondInteractor = null;

    protected override void OnSelectEntered(XRBaseInteractor interactor)
    {
        if (firstInteractor == null)
        {
            firstInteractor = interactor;
        }
        else if (secondInteractor == null)
        {
            secondInteractor = interactor;

            base.OnSelectEntered(firstInteractor);
        }
    }

    protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        if (interactor == firstInteractor)
        {
            firstInteractor = null;
        }
        else if (interactor == secondInteractor)
        {
            secondInteractor = null;
        }

        base.OnSelectExited(interactor);
    }

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        return (firstInteractor == null || secondInteractor == null || base.IsSelectableBy(interactor));
    }
}
