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
	private const int StandbyCommand = 1000;
	private const int MinThrottleCommand = 1150;
	private const int MaxThrottleCommand = 1850;
	private const int MaxAttitudeCommand = 500;

	private Slider _throttle;

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

	void Awake()
	{
		GameObject sliderThrottle = GameObject.Find("sliderThrottle");
		_throttle = sliderThrottle.GetComponent<Slider>();
	}

	// Update is called once per frame
	void Update()
	{
		calcCommand();
	}

	private void calcCommand()
	{
		float throttleScale = (Input.GetAxis("Throttle") + 1) / 2;
		_throttle.SetValueWithoutNotify(throttleScale);
		float offset = (MaxThrottleCommand - MinThrottleCommand) * throttleScale;
		RCCommand[Throttle] = MinThrottleCommand + (int)offset;

        float scale = Input.GetAxis("Horizontal");
        offset = MaxAttitudeCommand * scale;
        RCCommand[Roll] = (int)offset;

        scale = Input.GetAxis("Vertical");
        offset = MaxAttitudeCommand * scale;
        RCCommand[Pitch] = (int)offset;

        scale = Input.GetAxis("Yaw");
        offset = MaxAttitudeCommand * scale;
        RCCommand[Yaw] = (int)offset;
	}
}
