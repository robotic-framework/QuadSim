using UnityEngine;
using UnityEngine.UI;
using Net;
using Net.Protocol;

public class RCController : MonoBehaviour
{
	public const short Roll = 0;
	public const short Pitch = 1;
	public const short Yaw = 2;
	public const short Throttle = 3;
	public const int StandbyCommand = 1000;
	public const int MinThrottleCommand = 1150;
	public const int MaxThrottleCommand = 1850;
	public const int MaxAttitudeCommand = 500;

    private IMUController _imu;
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
	private int[] _rcCommand = { 0, 0, 0, 0 };

	public int[] RCCommand
	{
		get
		{
			return _rcCommand;
		}
		set
		{
			_rcCommand = value;
		}
	}

	public bool AltHold { get; set; }
	public bool VelZHold { get; set; }
	// cm
	public int DesiredAltHold { get; set; }
	// cm/s
	public int DesiredVelZ { get; set; }

	void Start()
	{
		AltHold = false;
		VelZHold = false;
        DesiredAltHold = 0;
        DesiredVelZ = 50;

        _imu = GameObject.Find("IMU").GetComponent<IMUController>();
        _gps = GameObject.Find("GPS").GetComponent<GpsSimulator>();
		_throttle = GameObject.Find("sliderThrottle").GetComponent<Slider>();
		_roll = GameObject.Find("sliderRoll").GetComponent<Slider>();
		_pitch = GameObject.Find("sliderPitch").GetComponent<Slider>();
		_yaw = GameObject.Find("sliderYaw").GetComponent<Slider>();

		_altHold = GameObject.Find("ToggleAltHold").GetComponent<Toggle>();
		_velZHold = GameObject.Find("ToggleVelZHold").GetComponent<Toggle>();

        _desiredAltHold = GameObject.Find("TextDesiredAltHold").GetComponent<Text>();
        _desiredVelZHold = GameObject.Find("TextDesiredVelZHold").GetComponent<Text>();

        var protocol = new ProtocolMsp();
        ServerSocket.Instance.ParseFunc = protocol.ReceiveStream;
        ServerSocket.Instance.PacketFunc = protocol.PacketStream;
        ServerSocket.Instance.Start();
	}

	// Update is called once per frame
	void Update()
	{
		calcCommand();

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
	}

	private void calcCommand()
	{
		float throttleScale = Input.GetAxis("Throttle");
		_throttle.SetValueWithoutNotify(throttleScale);
		float offset = (MaxThrottleCommand - MinThrottleCommand) * throttleScale;
		RCCommand[Throttle] = MinThrottleCommand + (int)offset;

		float scale = Input.GetAxis("Horizontal");
		_roll.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[Roll] = (int)offset;

		scale = Input.GetAxis("Vertical");
		_pitch.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[Pitch] = (int)offset;

		scale = Input.GetAxis("Yaw");
		_yaw.SetValueWithoutNotify((scale + 1) / 2);
		offset = MaxAttitudeCommand * scale;
		RCCommand[Yaw] = -(int)offset;
	}
}
