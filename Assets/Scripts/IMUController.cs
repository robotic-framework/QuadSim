using System;
using UnityEngine;

public class ImuController : MonoBehaviour
{
	private const float GYRO_LSB = 14.375F;
	private Vector3 _eular;
	private short[] _eularArray = {0, 0, 0};
	private float[] _lastAngles = {0, 0, 0};

	private short[] _gyroArray = {0, 0, 0};

	private float _lastAltitude;
	private float _velVerticle; // m/s

	private InfoText _info;

	// Start is called before the first frame update
	public short[] EularArray
	{
		get { return _eularArray; }
	}

	public short[] GyroArray
	{
		get { return _gyroArray; }
	}

	public int Altitude
	{
		get { return Convert.ToInt32(_lastAltitude * 100); }
	}

	public short VelZ
	{
		get
		{
			return Convert.ToInt16(_velVerticle * 100); // cm/s
		}
	}

	void Start()
	{
		_info = GameObject.Find("InfoText").GetComponent<InfoText>();
		_lastAltitude = transform.position.y;
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

		_eularArray[0] = Convert.ToInt16(-(_eular.z * 10)); // Roll
		_eularArray[1] = Convert.ToInt16(_eular.x * 10); // Pitch
		_eularArray[2] = Convert.ToInt16(_eular.y * 10); // Yaw

		float[] angles = {0, 0, 0};
		angles[0] = -_eular.z;
		angles[1] = _eular.x;
		angles[2] = _eular.y;

		float offset;
		for (int axis = 0; axis < 3; axis++)
		{
			offset = angles[axis] - _lastAngles[axis];
			var deltaAngle = offset / Time.deltaTime;
			_gyroArray[axis] = Convert.ToInt16(deltaAngle * GYRO_LSB);
			_lastAngles[axis] = angles[axis];
		}

		offset = transform.position.y - _lastAltitude;
		if (offset != 0)
		{
			_velVerticle = offset / Time.deltaTime;
		}

		_lastAltitude = transform.position.y;

		_info.Eular = EularArray;
		_info.Gyro = GyroArray;
		_info.Altitude = Altitude;
		_info.VelZ = VelZ;
	}
}
