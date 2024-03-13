using EventSchemaRegistry;
using MassTransit;
using PopugJira.Accounting.Contracts;
using PopugJira.Analytics.Handlers;
using PopugJira.Tracker.Contracts;

namespace PopugJira.Analytics.DiExt;

public static class KafkaDiExtensions
{
    public static void AddKafka(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.UsingInMemory();
            cfg.AddRider(rider =>
            {
                rider.AddConsumer<ReplicateTaskHandler>();
                rider.AddConsumer<StoreTransactionsHandler>();
                
                rider.UsingKafka(((context, kafka) =>
                {
                    kafka.Host("localhost:9092");
                    kafka.TopicEndpoint<KafkaEvent<Task_Changed_V1>>("tasks-streaming", "analytics", e =>
                    {
                        e.ConfigureConsumer<ReplicateTaskHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<TaskPrice_Calculated_V1>>("taskPrices-streaming.v1", "analytics", e =>
                    {
                        e.ConfigureConsumer<ReplicateTaskHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<FinTransaction_Applied_V1>>("fin-transactions.applied.v1", "analytics", e =>
                    {
                        e.ConfigureConsumer<StoreTransactionsHandler>(context);
                    });
                }));
            });
        });
    }
}