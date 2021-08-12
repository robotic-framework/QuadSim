namespace Net.Protocol
{
	public interface IHandler
	{
		MessageResponseSimImu msgSimImuHandler();
		void msgSimControlHandler(MessageRequestControl request);
		MessageResponseCommand msgSimCommandHandler();
	}
}
