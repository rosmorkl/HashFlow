namespace HashFlow.Application.Common.Interfaces;

public interface IMessageProducer
{
    void SendBatchToQueueAsync(IEnumerable<string> batch);
}