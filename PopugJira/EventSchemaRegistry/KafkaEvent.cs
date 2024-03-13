namespace EventSchemaRegistry;

public class KafkaEvent<T> where T : class
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset SentTime { get; init; } = DateTimeOffset.Now;
    public int Version { get; init; }
    public string Producer { get; init; }
    public string EventName { get; init; }
    public T Data { get; init; }

    public KafkaEvent(T data, string producer)
    {
        Data = data;
        Producer = producer;
        EventName = typeof(T).Name;
        Version = int.Parse(EventName.Split('_').Last().Trim('V'));
    }
    
}