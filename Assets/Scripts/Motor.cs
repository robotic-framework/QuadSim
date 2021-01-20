using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{

    private const int StandbyCommand = 1000;
    private const int MinCommand = 1150;
    private const int MaxCommand = 1850;

	private Rigidbody _rigidbody;

    private int _command;

    public bool Inverse;

    public int Command
    {
        get
        {
            return _command;
        }

        set
        {
            _command = Mathf.Clamp(value, MinCommand, MaxCommand);
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
        float force = _command / 100F - 7;
	    _rigidbody.AddRelativeForce(new Vector3(0, 1, 0) * force);

        Vector3 torque = Vector3.up;
        if (Inverse)
        {
            torque = Vector3.down;
        }
        _rigidbody.AddRelativeTorque(torque * force);
    }
}
