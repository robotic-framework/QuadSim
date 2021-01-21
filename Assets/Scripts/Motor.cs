using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{

    private const int StandbyCommand = 1000;
    private const int MinCommand = 1150;
    private const int MaxCommand = 1850;

	private Rigidbody _rigidbody;

    private int _throttle;

    public bool Inverse;

    public int Throttle
    {
        get
        {
            return _throttle;
        }

        set
        {
            _throttle = Mathf.Clamp(value, MinCommand, MaxCommand);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
	    _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float force = _throttle / 100F - 7;
	    _rigidbody.AddRelativeForce(new Vector3(0, 1, 0) * force);

        Vector3 torque = Vector3.up;
        if (Inverse)
        {
            torque = Vector3.down;
        }
        _rigidbody.AddRelativeTorque(torque * force);
    }
}
