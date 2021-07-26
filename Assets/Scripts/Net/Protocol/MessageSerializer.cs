namespace Net.Protocol
{
	public interface IMessageSerializer
	{
		void Decode(byte[] payload, ushort length);
		ushort Encode(byte[] payload, ushort maxLength);
	}
}
