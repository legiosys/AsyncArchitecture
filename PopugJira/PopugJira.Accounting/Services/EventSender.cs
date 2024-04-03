using EventSchemaRegistry;
using MassTransit;
using PopugJira.Accounting.Contracts;
using PopugJira.Accounting.Models;

namespace PopugJira.Accounting.Services;

public class EventSender(ITopicProducer<KafkaEvent<FinTransaction_Applied_V1>> transAppliedProducer,
    ITopicProducer<KafkaEvent<Payment_Completed_V1>> paymentProducer,
    ITopicProducer<KafkaEvent<TaskPrice_Calculated_V1>> priceProducer)
{
    private const string Producer = "accounting";
    
    public async Task PriceCalculated(PopugTask task)
    {
        var message = new KafkaEvent<TaskPrice_Calculated_V1>(
            new TaskPrice_Calculated_V1(task.ReplicationId, task.AssignPrice, task.CompletePrice), Producer);
        
        SchemeValidator.ValidateOrThrow(message);
        await priceProducer.Produce(message);
    }

    public async Task TransactionApplied(Transaction transaction, Guid popugId, PopugTask? task = null)
    {
        var message = new KafkaEvent<FinTransaction_Applied_V1>(
            new FinTransaction_Applied_V1(transaction.PublicId, transaction.TransactionType, transaction.Credit,
                transaction.Debit, popugId, task?.ReplicationId),
            Producer);
        SchemeValidator.ValidateOrThrow(message);
        await transAppliedProducer.Produce(message);
    }

    public async Task PaymentCompleted(Transaction transaction, Popug popug)
    {
        var message = new KafkaEvent<Payment_Completed_V1>(
            new Payment_Completed_V1(transaction.PublicId, transaction.Credit, popug.ReplicationId),
            Producer);
        SchemeValidator.ValidateOrThrow(message);
        await paymentProducer.Produce(message);
    }
}