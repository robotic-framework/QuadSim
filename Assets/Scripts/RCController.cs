using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        GameObject imu = GameObject.Find("IMU");
        _imu = imu.GetComponent<IMUController>();

		GameObject slider = GameObject.Find("sliderThrottle");
		_throttle = slider.GetComponent<Slider>();

		slider = GameObject.Find("sliderRoll");
		_roll = slider.GetComponent<Slider>();

		slider = GameObject.Find("sliderPitch");
		_pitch = slider.GetComponent<Slider>();

		slider = GameObject.Find("sliderYaw");
		_yaw = slider.GetComponent<Slider>();

		GameObject toggle = GameObject.Find("ToggleAltHold");
		_altHold = toggle.GetComponent<Toggle>();
		toggle = GameObject.Find("ToggleVelZHold");
		_velZHold = toggle.GetComponent<Toggle>();

        _desiredAltHold = GameObject.Find("TextDesiredAltHold").GetComponent<Text>();
        _desiredVelZHold = GameObject.Find("TextDesiredVelZHold").GetComponent<Text>();
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
		float throttleScale = (Input.GetAxis("Throttle") + 1) / 2;
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
