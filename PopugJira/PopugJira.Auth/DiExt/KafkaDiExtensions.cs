using MassTransit;
using PopugJira.Auth.Contracts;

namespace PopugJira.Auth.DiExt;

public static class KafkaDiExtensions
{
    public static void AddKafka(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.UsingInMemory();
            cfg.AddRider(rider =>
            {
                rider.AddProducer<PopugChanged>("popug-streaming");
                rider.AddProducer<PopugRegistered>("popug-registrations");
                rider.UsingKafka(((context, kafka) =>
                {
                    kafka.Host("localhost:9092");
                }));
            });
        });
    }
}