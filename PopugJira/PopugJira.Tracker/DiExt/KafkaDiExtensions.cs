using EventSchemaRegistry;
using MassTransit;
using MassTransit.KafkaIntegration.Serializers;
using MassTransit.Serialization;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Contracts;
using PopugJira.Tracker.Handlers;

namespace PopugJira.Tracker.DiExt;

public static class KafkaDiExtensions
{
    public static void AddKafka(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.UsingInMemory();
            cfg.AddRider(rider =>
            {
                rider.AddConsumer<ManageAccountHandler>();
                rider.AddProducer<KafkaEvent<Task_Changed_V1>>("tasks-streaming");
                rider.AddProducer<KafkaEvent<Task_Changed_V1>>("tasks-streaming.v2");
                rider.AddProducer<KafkaEvent<Task_Added_V1>>("tasks-lifecycle.added.v1");
                rider.AddProducer<KafkaEvent<Task_Assigned_V1>>("tasks-lifecycle.assigned.v1");
                rider.AddProducer<KafkaEvent<Task_Completed_V1>>("tasks-lifecycle.completed.v1");
                rider.UsingKafka(((context, kafka) =>
                {
                    kafka.Host("localhost:9092");
                    kafka.TopicEndpoint<KafkaEvent<Popug_Changed_V1>>("popug-streaming", "tracker", e =>
                    {
                        e.ConfigureConsumer<ManageAccountHandler>(context);
                    });
                }));
            });
        });
    }
}