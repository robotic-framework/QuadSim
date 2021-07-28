using System;

namespace Net.Protocol
{
	public class MessageRequestSimImu : MessageSerializer
	{
		public MessageRequestSimImu() : base(MsgType.TypeSimImu)
		{
		}

		public override void Decode(byte[] payload, ushort length)
		{
		}

		public override byte Encode(byte[] payload, ushort maxLength)
		{
			return 0;
		}
	}

	public class MessageSimImu : MessageSerializer
	{
		public Vector3 Acc;
		public Vector3 Gyro;
		public Vector3 Mag;
		public short Ct;
		public int Cp;
		public int Ccp;

		public override void Decode(byte[] payload, ushort length)
		{
			if (length < 28)
			{
				return;
			}

			Acc.X = BitConverter.ToInt16(payload, 0);
			Acc.Y = BitConverter.ToInt16(payload, 2);
			Acc.Z = BitConverter.ToInt16(payload, 4);

			Gyro.X = BitConverter.ToInt16(payload, 6);
			Gyro.Y = BitConverter.ToInt16(payload, 8);
			Gyro.Z = BitConverter.ToInt16(payload, 10);

			Mag.X = BitConverter.ToInt16(payload, 12);
			Mag.Y = BitConverter.ToInt16(payload, 14);
			Mag.Z = BitConverter.ToInt16(payload, 16);

			Ct = BitConverter.ToInt16(payload, 18);
			Cp = BitConverter.ToInt32(payload, 20);
			Ccp = BitConverter.ToInt32(payload, 24);
		}

		public override byte Encode(byte[] payload, ushort maxLength)
		{
			byte offset = 0;
			Array.Copy(BitConverter.GetBytes(Acc.X), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Acc.Y), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Acc.Z), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Gyro.X), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Gyro.Y), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Gyro.Z), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Mag.X), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Mag.Y), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Mag.Z), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Ct), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Cp), 0, payload, offset, 4);
			offset += 4;

			Array.Copy(BitConverter.GetBytes(Ccp), 0, payload, offset, 4);
			offset += 4;

			return offset;
		}

		public MessageSimImu() : base(MsgType.TypeSimImu)
		{
		}
	}

	public class MessageSimAcc : MessageSerializer
	{
		public Vector3 Acc;

		public override void Decode(byte[] payload, ushort length)
		{
			if (length < 6)
			{
				return;
			}

			Acc.X = BitConverter.ToInt16(payload, 0);
			Acc.Y = BitConverter.ToInt16(payload, 2);
			Acc.Z = BitConverter.ToInt16(payload, 4);
		}

		public override byte Encode(byte[] payload, ushort maxLength)
		{
			byte offset = 0;
			var bytesArray = BitConverter.GetBytes(Acc.X);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			bytesArray = BitConverter.GetBytes(Acc.Y);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			bytesArray = BitConverter.GetBytes(Acc.Z);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			return offset;
		}

		public MessageSimAcc() : base(MsgType.TypeSimAcc)
		{
		}
	}

	public class MessageSimGyro : MessageSerializer
	{
		public Vector3 Gyro;

		public override void Decode(byte[] payload, ushort length)
		{
			if (length < 6)
			{
				return;
			}

			Gyro.X = BitConverter.ToInt16(payload, 0);
			Gyro.Y = BitConverter.ToInt16(payload, 2);
			Gyro.Z = BitConverter.ToInt16(payload, 4);
		}

		public override byte Encode(byte[] payload, ushort maxLength)
		{
			byte offset = 0;
			var bytesArray = BitConverter.GetBytes(Gyro.X);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			bytesArray = BitConverter.GetBytes(Gyro.Y);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			bytesArray = BitConverter.GetBytes(Gyro.Z);
			Array.Copy(bytesArray, 0, payload, offset, 2);
			offset += 2;

			return offset;
		}

		public MessageSimGyro() : base(MsgType.TypeSimGyro)
		{
		}
	}
}
