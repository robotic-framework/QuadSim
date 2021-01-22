using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GPSSimulator : MonoBehaviour
{
	public double Lat;
	public double Lon;
	public float Bearing;
	public int[] Angle = { 0, 0 };

	private float _lastPosX;
	private float _lastPosY;
	private float _homePosX;
	private float _homePosY;
	private float _lastTime;

	// earth radius in meters
	private const float earthRadius = 6371010.0f;
	private InfoText _info;

	void Start()
	{
		_resetHome();
		_resetLast();

		_info = GameObject.Find("InfoText").GetComponent<InfoText>();
	}

	// Update is called once per frame
	void Update()
	{
		// 10Hz
		if (Time.time - _lastTime < 0.1)
		{
			return;
		}

		// pos
		_calcPos();

		// GPS bearing
		_calcBearing();
	}

	private void _calcPos()
	{
		float offsetX = transform.position.x - _homePosX;
		float offsetY = transform.position.z - _homePosY;
        if (offsetX == 0 && offsetY == 0)
        {
            return;
        }
        
		float distance = Mathf.Sqrt(Mathf.Pow(offsetX, 2) + Mathf.Pow(offsetY, 2));
		float bearing = -Mathf.Atan2(offsetY, offsetX);
		bearing = bearing / Mathf.PI * 180 + 90;
		if (bearing < 0)
		{
			bearing += 360;
		}
		float bearingRad = bearing / 180 * Mathf.PI;
		double distRatio = distance / earthRadius;
		double distRatioSin = Math.Sin(distRatio);
		double distRatioCos = Math.Cos(distRatio);

		double startLatRad = Lat / 180 * Mathf.PI;
		double startLonRad = Lon / 180 * Mathf.PI;
		double startLatSin = Math.Sin(startLatRad);
		double startLatCos = Math.Cos(startLatRad);

		double endLatRad = Math.Asin((startLatSin * distRatioCos) + (startLatCos * distRatioSin * Math.Cos(bearingRad)));
		double endLonRad = startLonRad +
			Math.Atan2(Math.Sin(bearingRad) * distRatioSin * startLatCos,
			distRatioCos - startLatSin * Math.Sin(endLatRad));

		Lat = endLatRad / Math.PI * 180;
		Lon = endLonRad / Math.PI * 180;
		_info.GPSLat = Lat;
		_info.GPSLon = Lon;
	}

	private void _calcBearing()
	{
		float offsetX = transform.position.x - _lastPosX;
		float offsetY = transform.position.z - _lastPosY;
		if (offsetX < 0.2 && offsetY < 0.2)
		{
			return;
		}
		Bearing = -Mathf.Atan2(offsetY, offsetX);
		Bearing = Bearing / Mathf.PI * 180 + 90;
		if (Bearing < 0)
		{
			Bearing += 360;
		}
		_info.GPSBearing = Bearing;

		_resetLast();
	}

	private void _resetLast()
	{
		_lastPosX = transform.position.x;
		_lastPosY = transform.position.z;
		_lastTime = Time.time;
	}

	private void _resetHome()
	{
		_homePosX = transform.position.x;
		_homePosY = transform.position.z;
	}
}
