using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
	// Start is called before the first frame update
	private Text text;

	public int[] Eular = { 0, 0, 0 };
	public int[] Motors = { 0, 0, 0, 0 };
	public int[] Gyro = { 0, 0, 0 };
	public int Altitude = 0;
	public int VelZ = 0;
    public float GPSBearing = 0;
    public double GPSLat = 0;
    public double GPSLon = 0;

	void Start()
	{
		text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		text.text = string.Format("Eular:\nRoll: {0}\nPitch: {1}\nYaw: {2}\n\nMotor1: {3}\nMotor2: {4}\nMotor3: {5}\nMotor4: {6}\n\nAltitude: {7} m\n\nGyro:\nRoll: {8}\nPitch: {9}\nYaw: {10}\n\nVelZ: {11} m/s\n\n GPS:\nBearing: {12}\nLat: {13}\nLon: {14}",
		Eular[0] / 10F,
		Eular[1] / 10F,
		Eular[2] / 10F,
		Motors[0],
		Motors[1],
		Motors[2],
		Motors[3],
		Altitude / 100F,
		Gyro[0],
		Gyro[1],
		Gyro[2],
        VelZ / 100F,
        GPSBearing,
        GPSLat,
        GPSLon);
	}
}
