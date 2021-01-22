using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

enum NavState
{
	NAV_STATE_NONE,
	NAV_STATE_RTH_START,
	NAV_STATE_RTH_ENROUTE,
	NAV_STATE_HOLD_INFINIT,
	NAV_STATE_HOLD_TIMED,
	NAV_STATE_WP_ENROUTE,
	NAV_STATE_PROCESS_NEXT,
	NAV_STATE_DO_JUMP,
	NAV_STATE_LAND_START,
	NAV_STATE_LAND_IN_PROGRESS,
	NAV_STATE_LANDED,
	NAV_STATE_LAND_SETTLE,
	NAV_STATE_LAND_START_DESCENT
}

public enum GPSMode
{
	GPS_MODE_NONE,
	GPS_MODE_HOLD,
	GPS_MODE_RTH,
	GPS_MODE_NAV
}

public class GPSSimulator : MonoBehaviour
{
	public const short LAT = 0;
	public const short LON = 1;
    public const float KmPerDegree = 111.318845f;

	public double Lat;
	public double Lon;
	public float Bearing;
	public int[] Angle = { 0, 0 };
	public bool Nav { get; set; }
	public GPSMode GPSMode { get; set; }
	private NavState navState;

	private float _lastPosX;
	private float _lastPosY;
	private float _homePosX;
	private float _homePosY;
	private float _lastTime;

	float scaleLonDown;
    bool isSetHome = false;
	private int[] posGPS = { 0, 0 };
	private int[] homeGPS = { 0, 0 };
	private int[] prevGPS = { 0, 0 };
	private int[] currentWP = { 0, 0 };
	private int[] errorDistance = { 0, 0 };
	private int distanceToHome;
	private int directionToHome;
	private int distanceToWP;   // cm
	private int directionToWP;  // degrees * 100
	private int[] calcSpeed = { 0, 0 };

	// earth radius in meters
	private const float earthRadius = 6371010.0f;
	private InfoText _info;

	void Start()
	{
		_info = GameObject.Find("InfoText").GetComponent<InfoText>();
        posGPS[LAT] = (int)(Lat * 10000000UL);
        posGPS[LON] = (int)(Lon * 10000000UL);
	}

	private void resetGPS()
	{
		_resetHome();
		_resetLast();
	}

	public void StartNav()
	{
		resetGPS();
		Nav = true;
	}

	// Update is called once per frame
	void Update()
	{
		gpsUpdate();
		navUpdate();
	}

	private void gpsUpdate()
	{
		// 10Hz
		if (Time.time - _lastTime < 0.1)
		{
			return;
		}


		if (!_isDistanceDeadband(0.1f))
		{
			// pos
			_calcPos();

			// GPS bearing
			_calcBearing();

			_resetLast();
		}
	}

	private void _calcPos()
	{
		float offsetX = transform.position.x - _homePosX;
		float offsetY = transform.position.z - _homePosY;

		float distance = getDistance(_homePosX, _homePosY, transform.position.x, transform.position.z);
		float bearing = getBearing(_homePosX, _homePosY, transform.position.x, transform.position.z);
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
        posGPS[LAT] = (int)(Lat * 10000000UL);
        posGPS[LON] = (int)(Lon * 10000000UL);
	}

	private bool _isDistanceDeadband(float deadband)
	{
		float distance = getDistance(_lastPosX, _lastPosY, transform.position.x, transform.position.z);
		return distance <= deadband;
	}

	private void _calcBearing()
	{
		Bearing = -Mathf.Atan2(transform.position.z - _lastPosY, transform.position.x - _lastPosX);
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
		homeGPS[LAT] = posGPS[LAT];
		homeGPS[LON] = posGPS[LON];

        calcLongitudeScaling(posGPS[LAT]);
	}

	private void calcLongitudeScaling(int lat)
	{
		scaleLonDown = Mathf.Cos(lat * 1.0e-7f * Mathf.PI / 180);
	}

    private float getBearing(float targetX, float targetY, float sourceX, float sourceY)
    {
        float offsetX = sourceX - targetX;
		float offsetY = sourceY - targetY;

		float bearing = -Mathf.Atan2(offsetY, offsetX);
		bearing = bearing / Mathf.PI * 180 + 90;
		if (bearing < 0)
		{
			bearing += 360;
		}
        return bearing;
    }

    private int getBearingByCoord(int targetLat, int targetLon, int sourceLat, int sourceLon)
    {
        int offsetX = sourceLon - targetLon;
        int offsetY = (int)((sourceLat - targetLat) / scaleLonDown);

        float bearing = -Mathf.Atan2(offsetY, offsetX) / Mathf.PI * 180 + 90;
        if (bearing < 0)
        {
            bearing += 360;
        }

        return (int)(bearing * 100);
    }

    private float getDistance(float targetX, float targetY, float sourceX, float sourceY)
    {
        float offsetX = sourceX - targetX;
		float offsetY = sourceY - targetY;

		return Mathf.Sqrt(Mathf.Pow(offsetX, 2) + Mathf.Pow(offsetY, 2));
    }

    private int getDistanceByCoord(int targetLat, int targetLon, int sourceLat, int sourceLon)
    {
        return (int)(getDistance(targetLat, targetLon, sourceLat, sourceLon) * KmPerDegree / 100);
    }

	private void navUpdate()
	{
		if (!Nav)
		{
			return;
		}

        directionToHome = getBearingByCoord(posGPS[LAT], posGPS[LON], homeGPS[LAT], homeGPS[LON]);
        distanceToHome = getDistanceByCoord(posGPS[LAT], posGPS[LON], homeGPS[LAT], homeGPS[LON]);

        _info.DistanceToHome = (float)distanceToHome / 100;
        _info.DirectionToHome = (float)directionToHome / 100;
	}
}
