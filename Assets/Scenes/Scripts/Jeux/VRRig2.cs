using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class VRMap2
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public VRMap2(Transform vrTarget, Transform rigTarget, Vector3 trackingPositionOffset, Vector3 trackingRotationOffset)
    {
        this.rigTarget = rigTarget;
        this.vrTarget = vrTarget;
        this.trackingPositionOffset = trackingPositionOffset;
        this.trackingRotationOffset = trackingRotationOffset;
    }
    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}
public class VRRig2 : MonoBehaviour
{
    public VRMap2 head;
    public VRMap2 leftHand;
    public VRMap2 rightHand;

    public Transform headConstraint;
    private Vector3 headBodyOffest;
    public float turnSmoothness;

    // Start is called before the first frame update
    void Start()
    {
        headBodyOffest = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = headConstraint.position + headBodyOffest;

        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();

    }
}