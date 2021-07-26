using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NavState
{
	NavStateNone,
	NavStateRthStart,
	NavStateRthEnroute,
	NavStateHoldInfinity,
	NavStateHoldTimed,
	NavStateWpEnroute,
	NavStateProcessNext,
	NavStateDoJump,
	NavStateLandStart,
	NavStateLandInProgress,
	NavStateLanded,
	NavStateLandSettle,
	NavStateLandStartDescent
}

public enum GpsMode
{
	GpsModeNone,
	GpsModeHold,
	GpsModeRth,
	GpsModeNav
}

public class GpsSimulator : MonoBehaviour
{
	public const short LAT = 0;
	public const short LON = 1;
	private const float KmPerDegree = 111.318845f;

	public double Lat;
	public double Lon;
	public float Bearing;
	public int[] Angle = { 0, 0 };
	public bool nav { get; set; }
	public GpsMode gpsMode { get; set; }
	private NavState _navState;

	private float _lastPosX;
	private float _lastPosY;
	private float _homePosX;
	private float _homePosY;
	private float _lastTime;

	float scaleLonDown;
    bool isSetHome = false;
	private int[] _posGps = { 0, 0 };
	private int[] _homeGps = { 0, 0 };
	private int[] _prevGps = { 0, 0 };
	private int[] _currentWp = { 0, 0 };
	private int[] _errorDistance = { 0, 0 };
	private int _distanceToHome;
	private int _directionToHome;
	private int _distanceToWp;   // cm
	private int _directionToWp;  // degrees * 100
	private int[] _calcSpeed = { 0, 0 };

	// earth radius in meters
	private const float EarthRadius = 6371010.0f;
	private InfoText _info;

	void Start()
	{
		_info = GameObject.Find("InfoText").GetComponent<InfoText>();
        _posGps[LAT] = (int)(Lat * 10000000UL);
        _posGps[LON] = (int)(Lon * 10000000UL);
	}

	private void resetGPS()
	{
		_resetHome();
		_resetLast();
	}

	public void StartNav()
	{
		resetGPS();
		nav = true;
	}

	// Update is called once per frame
	void Update()
	{
		_gpsUpdate();
		_navUpdate();
	}

	private void _gpsUpdate()
	{
		// 10Hz
		if (Time.time - _lastTime < 0.1)
		{
			return;
		}


		if (_isDistanceDeadband(0.1f)) return;
		// pos
		_calcPos();

		// GPS bearing
		_calcBearing();

		_resetLast();
	}

	private void _calcPos()
	{
		float offsetX = transform.position.x - _homePosX;
		float offsetY = transform.position.z - _homePosY;

		float distance = _getDistance(_homePosX, _homePosY, transform.position.x, transform.position.z);
		float bearing = _getBearing(_homePosX, _homePosY, transform.position.x, transform.position.z);
		float bearingRad = bearing / 180 * Mathf.PI;
		double distRatio = distance / EarthRadius;
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
        _posGps[LAT] = (int)(Lat * 10000000UL);
        _posGps[LON] = (int)(Lon * 10000000UL);
	}

	private bool _isDistanceDeadband(float deadband)
	{
		float distance = _getDistance(_lastPosX, _lastPosY, transform.position.x, transform.position.z);
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
		_homeGps[LAT] = _posGps[LAT];
		_homeGps[LON] = _posGps[LON];

        _calcLongitudeScaling(_posGps[LAT]);
	}

	private void _calcLongitudeScaling(int lat)
	{
		scaleLonDown = Mathf.Cos(lat * 1.0e-7f * Mathf.PI / 180);
	}

    private static float _getBearing(float targetX, float targetY, float sourceX, float sourceY)
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

    private int _getBearingByCoord(int targetLat, int targetLon, int sourceLat, int sourceLon)
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

    private static float _getDistance(float targetX, float targetY, float sourceX, float sourceY)
    {
        float offsetX = sourceX - targetX;
		float offsetY = sourceY - targetY;

		return Mathf.Sqrt(Mathf.Pow(offsetX, 2) + Mathf.Pow(offsetY, 2));
    }

    private static int _getDistanceByCoord(int targetLat, int targetLon, int sourceLat, int sourceLon)
    {
        return (int)(_getDistance(targetLat, targetLon, sourceLat, sourceLon) * KmPerDegree / 100);
    }

	private void _navUpdate()
	{
		if (!nav)
		{
			return;
		}

        _directionToHome = _getBearingByCoord(_posGps[LAT], _posGps[LON], _homeGps[LAT], _homeGps[LON]);
        _distanceToHome = _getDistanceByCoord(_posGps[LAT], _posGps[LON], _homeGps[LAT], _homeGps[LON]);

        _info.DistanceToHome = (float)_distanceToHome / 100;
        _info.DirectionToHome = (float)_directionToHome / 100;
	}
}
