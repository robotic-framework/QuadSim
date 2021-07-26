namespace Net.Protocol
{
	public struct MessageSimImu : IMessageSerializer
	{
		public Vector3 Acc;
		public Vector3 Gyro;
		public Vector3 Mag;
		public short Ct;
		public int Cp;
		public int Ccp;

		public void Decode(byte[] payload, ushort length)
		{
		}

		public ushort Encode(byte[] payload, ushort maxLength)
		{
			return 0;
		}
	}

	public struct MessageSimAcc : IMessageSerializer
	{
		public Vector3 Acc;

		public void Decode(byte[] payload, ushort length)
		{
		}

		public ushort Encode(byte[] payload, ushort maxLength)
		{
			return 0;
		}
	}

	public struct MessageSimGyro : IMessageSerializer
	{
		public Vector3 Gyro;

		public void Decode(byte[] payload, ushort length)
		{
		}

		public ushort Encode(byte[] payload, ushort maxLength)
		{
			return 0;
		}
	}
}
