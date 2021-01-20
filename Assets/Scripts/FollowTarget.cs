using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	public Transform target;

	private Vector3 offset = new Vector3(0, 50, -30);
    public float angle;
    private Vector3 velocityCameraFollow;

    void Awake()
    {
        offset = transform.position - target.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.transform.position + offset;
        // transform.position = Vector3.SmoothDamp(transform.position, target.transform.TransformPoint(offset) + Vector3.up * Input.GetAxis("Vertical"), ref velocityCameraFollow, 0.1f);
        // transform.rotation = Quaternion.Euler(new Vector3(angle, target.GetComponent<DroneMovementScript>().currentYRotation, 0));
    }
}
