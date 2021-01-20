using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMUController : MonoBehaviour
{
	private const float GYRO_LSB = 14.375F;
	private Vector3 _eular;
	private int[] _eularArray = { 0, 0, 0 };
	private float[] _lastAngles = { 0, 0, 0 };

	private int[] _gyroArray = { 0, 0, 0 };

    private float _lastAltitude;
    private float _velVerticle = 0F; // m/s
    private float _lastTime = 0F;

	// Start is called before the first frame update
	public int[] EularArray
	{
		get
		{
			return _eularArray;
		}
	}

	public int[] GyroArray
	{
		get
		{
			return _gyroArray;
		}
	}

    public int Altitude
    {
        get
        {
            return (int)(transform.position.y * 100);
        }
    }

    public int VelZ
    {
        get
        {
            return (int)(_velVerticle * 100); // cm/s
        }
    }

    void Start()
    {
        _lastTime = Time.time;
    }
    

	// Update is called once per frame
	void Update()
	{
		_eular = transform.rotation.eulerAngles;
		if (_eular.x >= 180)
		{
			_eular.x -= 360;
		}
		if (_eular.y >= 180)
		{
			_eular.y -= 360;
		}
		if (_eular.z >= 180)
		{
			_eular.z -= 360;
		}
		_eularArray[0] = -(int)(_eular.z * 10); // Roll
		_eularArray[1] = (int)(_eular.x * 10); // Pitch
		_eularArray[2] = (int)(_eular.y * 10); // Yaw

		float[] angles = { 0, 0, 0 };
        angles[0] = -_eular.z;
        angles[1] = _eular.x;
        angles[2] = _eular.y;

        float offset;
		for (int axis = 0; axis < 3; axis++)
		{
			offset = angles[axis] - _lastAngles[axis];
			float gyro = offset / Time.deltaTime;
			_gyroArray[axis] = (int)(gyro * GYRO_LSB);
            _lastAngles[axis] = angles[axis];
		}

        float curTime = Time.time;
        offset = transform.position.y - _lastAltitude;
        if (offset != 0)
        {
            _velVerticle = offset / (curTime - _lastTime);
            _lastTime = curTime;
        }

        _lastAltitude = transform.position.y;
	}
}
