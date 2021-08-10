using UnityEditor;
using UnityEngine;

namespace Net.Protocol
{
	public class CommonHandler : IHandler
	{
		private readonly ImuController _imu;

		public CommonHandler()
		{
			_imu = GameObject.Find("IMU").GetComponent<ImuController>();
		}

		public MessageResponseSimImu msgSimImuHandler(MessageRequestSimImu request)
		{
			var msg = new MessageResponseSimImu
			{
				Acc = {X = 1, Y = 2, Z = 3},
				Gyro = {X = _imu.GyroArray[0], Y = _imu.GyroArray[1], Z = _imu.GyroArray[2]},
				Mag = {X = 7, Y = 8, Z = 9},
				Ct = 1000,
				Cp = 10000,
				Ccp = 20000,
				Att = {X = _imu.EularArray[0], Y = _imu.EularArray[1], Z = _imu.EularArray[2]},
				Alt = _imu.Altitude,
				Vario = _imu.VelZ
			};

			return msg;
		}

		public void msgSimControlHandler(MessageRequestControl request)
		{
			MotorController.instance.motors = request.Motors;
		}
	}
}
