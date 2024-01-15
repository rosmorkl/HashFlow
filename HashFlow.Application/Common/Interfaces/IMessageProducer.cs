namespace HashFlow.Application.Common.Interfaces;

public interface IMessageProducer
{
    void SendBatchToQueue(IEnumerable<string> batch);
}