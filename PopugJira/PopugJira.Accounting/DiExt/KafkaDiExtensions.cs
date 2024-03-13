using EventSchemaRegistry;
using MassTransit;
using PopugJira.Accounting.Contracts;
using PopugJira.Accounting.Handlers;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Contracts;

namespace PopugJira.Accounting.DiExt;

public static class KafkaDiExtensions
{
    public static void AddKafka(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.UsingInMemory();
            cfg.AddRider(rider =>
            {
                rider.AddConsumer<CalcTaskPricesHandler>();
                rider.AddConsumer<AccountManageHandler>();
                rider.AddConsumer<ApplyTransactionHandler>();

                rider.AddProducer<KafkaEvent<TaskPrice_Calculated_V1>>("taskPrices-streaming.v1");
                rider.AddProducer<KafkaEvent<FinTransaction_Applied_V1>>("fin-transactions.applied.v1");
                rider.AddProducer<KafkaEvent<Payment_Completed_V1>>("payments.completed.v1");
                
                rider.UsingKafka(((context, kafka) =>
                {
                    kafka.Host("localhost:9092");
                    kafka.TopicEndpoint<KafkaEvent<Task_Changed_V1>>("tasks-streaming", "accounting", e =>
                    {
                        e.ConfigureConsumer<CalcTaskPricesHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<Popug_Changed_V1>>("popug-streaming", "accounting", e =>
                    {
                        e.ConfigureConsumer<AccountManageHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<Task_Added_V1>>("tasks-lifecycle.added.v1", "accounting", e =>
                    {
                        e.ConfigureConsumer<ApplyTransactionHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<Task_Assigned_V1>>("tasks-lifecycle.assigned.v1", "accounting", e =>
                    {
                        e.ConfigureConsumer<ApplyTransactionHandler>(context);
                    });
                    kafka.TopicEndpoint<KafkaEvent<Task_Completed_V1>>("tasks-lifecycle.completed.v1", "accounting", e =>
                    {
                        e.ConfigureConsumer<ApplyTransactionHandler>(context);
                    });
                }));
            });
        });
    }
}