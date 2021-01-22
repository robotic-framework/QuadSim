using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PIDController : MonoBehaviour
{
	private const int GYRO_P_MAX = 300;
	private const int GYRO_I_MAX = 250;

	private Motor _motor1;
	private Motor _motor2;
	private Motor _motor3;
	private Motor _motor4;
	private IMUController _imu;
	private GPSSimulator _gps;
	private RCController _rc;
	private InfoText _info;
    private Slider _sliderMotor1;
    private Slider _sliderMotor2;
    private Slider _sliderMotor3;
    private Slider _sliderMotor4;
    private Text _textMotor1;
    private Text _textMotor2;
    private Text _textMotor3;
    private Text _textMotor4;

	private int[] _eularArray = { 0, 0, 0 };
	private int[] _gyroArray = { 0, 0, 0 };
	private int[] errorAngleI = { 0, 0, 0 };
	private int errorAltI = 0;
	private int errorVelZI = 0;
	private int errorGyroI_YAW = 0;
	private int[] lastGyro = { 0, 0, 0 };
	private int[] lastErrorAngle = { 0, 0, 0 };
	private int[,] deltaGyro = { { 0, 0, 0 }, { 0, 0, 0 } };
	private int[] commandOffset = { 0, 0, 0, 0, 0 }; // roll, pitch, yaw, althold, velZ

	// x = Roll, y = Yaw, z = Pitch
	public int PLevel;
	public int ILevel;
	public int DLevel;
	public int PAltHold;
	public int IAltHold;
	public int DAltHold;
	public int PVelZ;
	public int IVelZ;
	public int DVelZ;

	// Start is called before the first frame update
	void Start()
	{
		_motor1 = GameObject.Find("Motor1").GetComponent<Motor>();
		_motor2 = GameObject.Find("Motor2").GetComponent<Motor>();
		_motor3 = GameObject.Find("Motor3").GetComponent<Motor>();
		_motor4 = GameObject.Find("Motor4").GetComponent<Motor>();

		_imu = GameObject.Find("IMU").GetComponent<IMUController>();
		_gps = GameObject.Find("GPS").GetComponent<GPSSimulator>();
		_rc = GameObject.Find("RC").GetComponent<RCController>();

		_info = GameObject.Find("InfoText").GetComponent<InfoText>();

        _sliderMotor1 = GameObject.Find("sliderMotor1").GetComponent<Slider>();
        _sliderMotor2 = GameObject.Find("sliderMotor2").GetComponent<Slider>();
        _sliderMotor3 = GameObject.Find("sliderMotor3").GetComponent<Slider>();
        _sliderMotor4 = GameObject.Find("sliderMotor4").GetComponent<Slider>();

        _textMotor1 = GameObject.Find("textMotor1").GetComponent<Text>();
        _textMotor2 = GameObject.Find("textMotor2").GetComponent<Text>();
        _textMotor3 = GameObject.Find("textMotor3").GetComponent<Text>();
        _textMotor4 = GameObject.Find("textMotor4").GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		_eularArray = _imu.EularArray;
		_gyroArray = _imu.GyroArray;

		_info.Eular = _eularArray;
		_info.Gyro = _gyroArray;
		_info.Altitude = _imu.Altitude;
		_info.VelZ = _imu.VelZ;

		apply();
		mixMotor();
	}

	private void apply()
	{
		int PTerm = 0, ITerm = 0, DTerm = 0, PTermACC, ITermACC;
		int limit = 0;
		int prop = Mathf.Min(Mathf.Max(Mathf.Abs(_rc.RCCommand[RCController.Pitch]), Mathf.Abs(_rc.RCCommand[RCController.Roll])), 512);
		// Roll and Pitch
		for (int axis = 0; axis < 2; axis++)
		{
			int rcLevel = _rc.RCCommand[axis] << 1;
			int errorAngle = Mathf.Clamp(rcLevel + _gps.Angle[axis], -500, 500) - _eularArray[axis];
			errorAngleI[axis] = Mathf.Clamp(errorAngleI[axis] + errorAngle, -10000, 10000);

			PTermACC = (errorAngle * PLevel) >> 7;
			limit = DLevel * 5;
			PTermACC = Mathf.Clamp(PTermACC, -limit, limit);

			ITermACC = (errorAngleI[axis] * ILevel) >> 12;

			ITerm = ITermACC + ((ITerm - ITermACC) * prop >> 9);
			PTerm = PTermACC + ((PTerm - PTermACC) * prop >> 9);

			DTerm = (errorAngle - lastErrorAngle[axis]) * DLevel >> 7;
			lastErrorAngle[axis] = errorAngle;

			// int delta = _gyroArray[axis] - lastGyro[axis];
			// lastGyro[axis] = _gyroArray[axis];
			// DTerm = (deltaGyro[0, axis] + deltaGyro[1, axis] + delta) >> 7;
			// deltaGyro[1, axis] = deltaGyro[0, axis];
			// deltaGyro[0, axis] = delta;

			commandOffset[axis] = PTerm + ITerm + DTerm;
		}

		// Yaw
		{
			int rcLevel = (_rc.RCCommand[RCController.Yaw] * 30) >> 5;
			int error = rcLevel - _gyroArray[2];
			errorGyroI_YAW += error * ILevel;
			errorGyroI_YAW = Mathf.Clamp(errorGyroI_YAW, 2 - (1 << 28), -2 + (1 << 28));
			if (Mathf.Abs(rcLevel) > 50)
			{
				errorGyroI_YAW = 0;
			}

			PTerm = (error * PLevel) >> 6;
			limit = GYRO_P_MAX - DLevel;
			PTerm = Mathf.Clamp(PTerm, -limit, limit);
			ITerm = Mathf.Clamp(errorGyroI_YAW >> 13, -GYRO_I_MAX, GYRO_I_MAX);
			commandOffset[RCController.Yaw] = PTerm + ITerm;
		}

		// AltHold
        if (_rc.AltHold)
		{
			int errorAlt = Mathf.Clamp(_rc.DesiredAltHold - _imu.Altitude, -300, 300);
			errorAlt = _applyDeadband(errorAlt, 10);
			PTerm = Mathf.Clamp((PAltHold * errorAlt) >> 7, -150, 150);

			errorAltI += (IAltHold * errorAlt) >> 6;
			errorAltI = Mathf.Clamp(errorAltI, -30000, 30000);
			ITerm = errorAltI >> 9;

			int velZ = _imu.VelZ;
			DTerm = Mathf.Clamp(DAltHold * velZ >> 4, -150, 150);
			commandOffset[RCController.Throttle] = PTerm + ITerm - DTerm;
		}

		// VelZ
		if (_rc.VelZHold && (Mathf.Abs(_rc.DesiredAltHold - _imu.Altitude) > 100))
		{
			int velZ = _imu.VelZ;
			int errorVel = Mathf.Clamp(_rc.DesiredVelZ - Mathf.Abs(velZ), -500, 500);
			PTerm = Mathf.Clamp((PVelZ * errorVel) >> 6, -200, 200);

			errorVelZI += (IVelZ * errorVel) >> 5;
			errorVelZI = Mathf.Clamp(errorVelZI, -30000, 30000);
			ITerm = errorVelZI >> 8;

			commandOffset[4] = PTerm + ITerm;
			if (_imu.VelZ < 0)
			{
				commandOffset[4] = -commandOffset[4];
			}
		}

	}

	private void mixMotor()
	{
		_motor1.Throttle = _pidMix(-1, 1, -1);
		_motor2.Throttle = _pidMix(-1, -1, 1);
		_motor3.Throttle = _pidMix(1, 1, 1);
		_motor4.Throttle = _pidMix(1, -1, -1);

		_textMotor1.text = _motor1.Throttle.ToString();
		_textMotor2.text = _motor2.Throttle.ToString();
		_textMotor3.text = _motor3.Throttle.ToString();
		_textMotor4.text = _motor4.Throttle.ToString();

        _sliderMotor1.SetValueWithoutNotify(_throttleScale(_motor1.Throttle));
        _sliderMotor2.SetValueWithoutNotify(_throttleScale(_motor2.Throttle));
        _sliderMotor3.SetValueWithoutNotify(_throttleScale(_motor3.Throttle));
        _sliderMotor4.SetValueWithoutNotify(_throttleScale(_motor4.Throttle));
	}

	private int _pidMix(int x, int y, int z)
	{
		return _rc.RCCommand[RCController.Throttle] +
            commandOffset[RCController.Roll] * x +
            commandOffset[RCController.Pitch] * y +
            commandOffset[RCController.Yaw] * z +
            commandOffset[RCController.Throttle] +
            commandOffset[4];
	}

    private float _throttleScale(int throttle)
    {
        return (float)(throttle - RCController.MinThrottleCommand) / (float)(RCController.MaxThrottleCommand - RCController.MinThrottleCommand);
    }

	private int _applyDeadband(int val, int deadband)
	{
		if (Mathf.Abs(val) < deadband)
		{
			return 0;
		}
		else if (val > 0)
		{
			return val - deadband;
		}
		else if (val < 0)
		{
			return val + deadband;
		}
		return val;
	}
}
