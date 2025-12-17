using UnityEngine;
using Unity.XR.CoreUtils;

public class BoatRideManager : MonoBehaviour
{
    [Header("Ref-uri de bază")]
    public XROrigin xrOrigin;
    public BoatController boatController;

    Transform originalParent;
    bool isOnBoat;

    void Start()
    {
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();

        if (boatController == null)
            boatController = FindObjectOfType<BoatController>();
    }

    public void BoardBoat()
    {
        if (isOnBoat || xrOrigin == null || boatController == null) return;

        Debug.Log("BoardBoat() called");

        isOnBoat = true;

        originalParent = xrOrigin.transform.parent;
        xrOrigin.transform.SetParent(boatController.transform);

        boatController.isControlled = true;
    }

    public void LeaveBoat(Transform exitPoint)
    {
        if (!isOnBoat || xrOrigin == null || boatController == null) return;

        Debug.Log("LeaveBoat() called");

        isOnBoat = false;

        xrOrigin.transform.SetParent(originalParent);

        // dacă ai un punct de ieșire (pe ponton/plank)
        if (exitPoint != null)
        {
            xrOrigin.MoveCameraToWorldLocation(exitPoint.position);

            Vector3 rot = xrOrigin.transform.eulerAngles;
            rot.y = exitPoint.eulerAngles.y;
            xrOrigin.transform.eulerAngles = rot;
        }

        boatController.isControlled = false;
    }
}