using MassTransit;
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
                rider.AddProducer<TaskChanged>("tasks-streaming");
                rider.AddProducer<TaskOperationPerformed>("tasks-operations");
                rider.UsingKafka(((context, kafka) =>
                {
                    kafka.Host("localhost:9092");
                    kafka.TopicEndpoint<PopugChanged>("popug-streaming", "tracker", e =>
                    {
                        e.ConfigureConsumer<ManageAccountHandler>(context);
                    });
                }));
            });
        });
    }
}