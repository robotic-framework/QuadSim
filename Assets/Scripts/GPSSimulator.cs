using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GPSSimulator : MonoBehaviour
{
	public double Lat;
	public double Lon;

	private float _lastPosX;
	private float _lastPosY;
	private float _startPosX;
	private float _startPosY;
	private float _lastTime;

	// earth radius in meters
	private const float earthRadius = 6371010.0f;
	private InfoText _info;

	void Start()
	{
		_resetLast();
		_startPosX = transform.position.x;
		_startPosY = transform.position.z;

		GameObject info = GameObject.Find("InfoText");
		_info = info.GetComponent<InfoText>();
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
		float offsetX = transform.position.x - _startPosX;
		float offsetY = transform.position.z - _startPosY;
		float distance = Mathf.Sqrt(Mathf.Pow(offsetX, 2) + Mathf.Pow(offsetY, 2));
		float bearing = -Mathf.Atan2(offsetY, offsetX);
		bearing = bearing / Mathf.PI * 180 + 90;
		if (bearing < 0)
		{
			bearing += 360;
		}
		float bearingRad = bearing / 180 * Mathf.PI;
		_info.GPSBearing = bearing;

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

		// GPS bearing
		// offsetX = transform.position.x - _lastPosX;
		// offsetY = transform.position.z - _lastPosY;
		// if (offsetX < 0.2 && offsetY < 0.2)
		// {
		//     return;
		// }
		// bearing = Mathf.Atan2(offsetY, offsetX);
		// _info.GPSBearing = bearing / Mathf.PI * 180;

		_resetLast();
	}

	private void _resetLast()
	{
		_lastPosX = transform.position.x;
		_lastPosY = transform.position.z;
		_lastTime = Time.time;
	}
}
