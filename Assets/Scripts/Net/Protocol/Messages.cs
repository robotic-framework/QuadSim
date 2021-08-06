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

	public class MessageResponseSimImu : MessageSerializer
	{
		public Vector3 Acc;
		public Vector3 Gyro;
		public Vector3 Mag;
		public short Ct;
		public int Cp;
		public int Ccp;
		public Vector3 Att;
		public int Alt;
		public short Vario;

		public override void Decode(byte[] payload, ushort length)
		{
			if (length < 40)
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

			Att.X = BitConverter.ToInt16(payload, 28);
			Att.Y = BitConverter.ToInt16(payload, 30);
			Att.Z = BitConverter.ToInt16(payload, 32);

			Alt = BitConverter.ToInt32(payload, 34);
			Vario = BitConverter.ToInt16(payload, 38);
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

			Array.Copy(BitConverter.GetBytes(Att.X), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Att.Y), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Att.Z), 0, payload, offset, 2);
			offset += 2;

			Array.Copy(BitConverter.GetBytes(Alt), 0, payload, offset, 4);
			offset += 4;

			Array.Copy(BitConverter.GetBytes(Vario), 0, payload, offset, 2);
			offset += 2;

			return offset;
		}

		public MessageResponseSimImu() : base(MsgType.TypeSimImu)
		{
		}
	}

	public class MessageRequestControl : MessageSerializer
	{
		public ushort[] Motors = new ushort[8];

		public MessageRequestControl() : base(MsgType.TypeSimControl)
		{
		}

		public override void Decode(byte[] payload, ushort length)
		{
			if (length < Motors.Length * 2)
			{
				return;
			}

			for (int i = 0; i < Motors.Length; i++)
			{
				Motors[i] = BitConverter.ToUInt16(payload, i * 2);
			}
		}

		public override byte Encode(byte[] payload, ushort maxLength)
		{
			for (int i = 0; i < Motors.Length; i++)
			{
				Array.Copy(BitConverter.GetBytes(Motors[i]), 0, payload, i * 2, 2);
			}

			return (byte)(Motors.Length * 2);
		}
	}

}
