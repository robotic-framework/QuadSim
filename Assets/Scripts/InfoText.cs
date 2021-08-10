using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
	// Start is called before the first frame update
	private Text text;

	public short[] Eular = { 0, 0, 0 };
	public short[] Gyro = { 0, 0, 0 };
	public int Altitude = 0;
	public int VelZ = 0;
	public float GPSBearing = 0;
	public double GPSLat = 0;
	public double GPSLon = 0;
    public float DistanceToHome;
    public float DirectionToHome;

	void Start()
	{
		text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		text.text = string.Format("Eular:\nRoll: {0}\nPitch: {1}\nYaw: {2}\n\nAltitude: {3} m\n\nGyro:\nRoll: {4}\nPitch: {5}\nYaw: {6}\n\nVelZ: {7} m/s\n\n GPS:\nBearing: {8}\nLat: {9}\nLon: {10}\nDistanceToHome: {11}\nDirectionToHome: {12}",
		Eular[0] / 10F,
		Eular[1] / 10F,
		Eular[2] / 10F,
		Altitude / 100F,
		Gyro[0],
		Gyro[1],
		Gyro[2],
		VelZ / 100F,
		GPSBearing,
		GPSLat,
		GPSLon,
        DistanceToHome,
        DirectionToHome);
	}
}
