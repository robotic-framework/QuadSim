using UnityEngine;
using UnityEngine.UI;

public class MotorController
{
	private RCController _rc;
	private Motor _motor1;
	private Motor _motor2;
	private Motor _motor3;
	private Motor _motor4;

	private Slider _sliderMotor1;
	private Slider _sliderMotor2;
	private Slider _sliderMotor3;
	private Slider _sliderMotor4;
	private Text _textMotor1;
	private Text _textMotor2;
	private Text _textMotor3;
	private Text _textMotor4;
	public ushort[] motors { get; set; }

	public static MotorController instance { get; } = new MotorController();

	private MotorController()
	{
		motors = new ushort[8];
		_rc = GameObject.Find("RC").GetComponent<RCController>();

		_motor1 = GameObject.Find("Motor1").GetComponent<Motor>();
		_motor2 = GameObject.Find("Motor2").GetComponent<Motor>();
		_motor3 = GameObject.Find("Motor3").GetComponent<Motor>();
		_motor4 = GameObject.Find("Motor4").GetComponent<Motor>();

		_sliderMotor1 = GameObject.Find("sliderMotor1").GetComponent<Slider>();
		_sliderMotor2 = GameObject.Find("sliderMotor2").GetComponent<Slider>();
		_sliderMotor3 = GameObject.Find("sliderMotor3").GetComponent<Slider>();
		_sliderMotor4 = GameObject.Find("sliderMotor4").GetComponent<Slider>();

		_textMotor1 = GameObject.Find("textMotor1").GetComponent<Text>();
		_textMotor2 = GameObject.Find("textMotor2").GetComponent<Text>();
		_textMotor3 = GameObject.Find("textMotor3").GetComponent<Text>();
		_textMotor4 = GameObject.Find("textMotor4").GetComponent<Text>();
	}

	public void Update()
	{
		_motor1.Throttle = motors[0];
		_motor2.Throttle = motors[1];
		_motor3.Throttle = motors[2];
		_motor4.Throttle = motors[3];

		_textMotor1.text = _motor1.Throttle.ToString();
		_textMotor2.text = _motor2.Throttle.ToString();
		_textMotor3.text = _motor3.Throttle.ToString();
		_textMotor4.text = _motor4.Throttle.ToString();

		_sliderMotor1.SetValueWithoutNotify(_throttleScale(_motor1.Throttle));
		_sliderMotor2.SetValueWithoutNotify(_throttleScale(_motor2.Throttle));
		_sliderMotor3.SetValueWithoutNotify(_throttleScale(_motor3.Throttle));
		_sliderMotor4.SetValueWithoutNotify(_throttleScale(_motor4.Throttle));
	}

	private float _throttleScale(int throttle)
	{
		return (throttle - RCController.MinThrottleCommand) /
		       (float) (RCController.MaxThrottleCommand - RCController.MinThrottleCommand);
	}
}
