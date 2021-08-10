namespace Net.Protocol
{
	public interface IHandler
	{
		MessageResponseSimImu msgSimImuHandler(MessageRequestSimImu request);
		void msgSimControlHandler(MessageRequestControl request);
	}
}
