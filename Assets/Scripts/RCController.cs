using System;
using UnityEngine;
using UnityEngine.UI;
using Net;
using Net.Protocol;

enum RCAxis : short
{
	ROLL,
	PITCH,
	YAW,
	THROTTLE,
	AUX1,
	AUX2,
	AUX3,
	AUX4,
	AUX5,
	AUX6,
	AUX7,
	AUX8,
	RC_AXIS_COUNT
}

public class RCController : MonoBehaviour
{
	public const short StandbyCommand = 1000;
	public const short MinThrottleCommand = 1150;
	public const short MaxThrottleCommand = 1850;
	public const short MaxAttitudeCommand = 500;

	private ImuController _imu;
	private GpsSimulator _gps;
	private Slider _throttle;
	private Slider _roll;
	private Slider _pitch;
	private Slider _yaw;
	private Toggle _altHold;
	private Toggle _velZHold;
	private Text _desiredVelZHold;
	private Text _desiredAltHold;

	// interval [1000;2000] for THROTTLE and [-500;+500] for ROLL/PITCH/YAW

	public short[] RCCommand { get; set; } = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

	public bool AltHold { get; set; }

	public bool VelZHold { get; set; }

	// cm
	public int DesiredAltHold { get; set; }

	// cm/s
	public int DesiredVelZ { get; set; }

	private void Awake()
	{
		var protocol = new ProtocolMsp();
		ServerSocket.Instance.ParseFunc = protocol.ReceiveStream;
		ServerSocket.Instance.PacketFunc = protocol.PacketStream;
		ServerSocket.Instance.Start();

		RCCommand[(short) RCAxis.THROTTLE] = MinThrottleCommand;
	}

	private void Start()
	{
		AltHold = false;
		VelZHold = false;
		DesiredAltHold = 0;
		DesiredVelZ = 50;

		_imu = GameObject.Find("IMU").GetComponent<ImuController>();
		_gps = GameObject.Find("GPS").GetComponent<GpsSimulator>();
		_throttle = GameObject.Find("sliderThrottle").GetComponent<Slider>();
		_roll = GameObject.Find("sliderRoll").GetComponent<Slider>();
		_pitch = GameObject.Find("sliderPitch").GetComponent<Slider>();
		_yaw = GameObject.Find("sliderYaw").GetComponent<Slider>();

		_altHold = GameObject.Find("ToggleAltHold").GetComponent<Toggle>();
		_velZHold = GameObject.Find("ToggleVelZHold").GetComponent<Toggle>();

		_desiredAltHold = GameObject.Find("TextDesiredAltHold").GetComponent<Text>();
		_desiredVelZHold = GameObject.Find("TextDesiredVelZHold").GetComponent<Text>();
	}

	// Update is called once per frame
	private async void Update()
	{
		calcCommand();

		if (!AltHold)
		{
			DesiredAltHold = _imu.Altitude;
		}
		else
		{
			_desiredAltHold.text = DesiredAltHold.ToString();
		}

		if (VelZHold)
		{
			_desiredVelZHold.text = DesiredVelZ.ToString();
		}

		MotorController.instance.Update();
	}

	private void calcCommand()
	{
		var throttleScale = Math.Max(Input.GetAxis("Throttle"), 0); // cut to 0~1
		_throttle.SetValueWithoutNotify(throttleScale);
		var offset = (MaxThrottleCommand - MinThrottleCommand) * throttleScale;
		RCCommand[(short) RCAxis.THROTTLE] = (short) (MinThrottleCommand + Convert.ToInt16(offset));

		var scale = Input.GetAxis("Horizontal");
		_roll.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[(short) RCAxis.ROLL] = Convert.ToInt16(offset);

		scale = Input.GetAxis("Vertical");
		_pitch.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[(short) RCAxis.PITCH] = Convert.ToInt16(offset);

		scale = Input.GetAxis("Yaw");
		_yaw.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[(short) RCAxis.YAW] = (short) -Convert.ToInt16(offset);

		if (Input.GetKeyUp(KeyCode.H))
		{
			AltHold = !AltHold;
			_altHold.enabled = AltHold;
		}

		if (Input.GetKeyUp(KeyCode.V))
		{
			VelZHold = !VelZHold;
			_velZHold.enabled = VelZHold;
		}

		if (Input.GetKeyUp(KeyCode.N))
		{
			_gps.StartNav();
		}
	}
}
