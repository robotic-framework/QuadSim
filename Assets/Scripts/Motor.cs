using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{

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
            _throttle = Mathf.Clamp(value, RCController.MinThrottleCommand, RCController.MaxThrottleCommand);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
	    _rigidbody = GetComponent<Rigidbody>();
	    _rigidbody.angularDrag = 0;
	    _rigidbody.maxAngularVelocity = 50;
    }

    // Update is called once per frame
    void Update()
    {
        float force = _throttle / 100F - 7;
	    _rigidbody.AddRelativeForce(new Vector3(0, 1, 0) * force);

        var direction = Inverse ? -1 : 1;
        _rigidbody.angularVelocity = transform.up * (_throttle * direction * Time.deltaTime);
    }
}
