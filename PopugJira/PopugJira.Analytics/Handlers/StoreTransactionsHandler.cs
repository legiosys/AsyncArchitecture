using Confluent.Kafka;
using EventSchemaRegistry;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PopugJira.Accounting.Contracts;
using PopugJira.Analytics.Db;
using PopugJira.Analytics.Models;

namespace PopugJira.Analytics.Handlers;

public class StoreTransactionsHandler(AnalyticsDbContext db) : IConsumer<KafkaEvent<FinTransaction_Applied_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<FinTransaction_Applied_V1>> context)
    {
        SchemeValidator.ValidateOrThrow(context.Message);

        var task = await db.PopugTasks.AsNoTracking()
            .FirstAsync(x => x.ReplicationId == context.Message.Data.TaskId);
        switch (context.Message.Data.Type)
        {
            case TransactionTypes.Assign:
                db.TopsBalanceChanges.Add(new TopsBalanceChange()
                {
                    Debit = task.AssignPrice,
                    Credit = 0,
                    Date = context.Message.SentTime
                });
                db.PopugBalanceChanges.Add(new PopugBalanceChange()
                {
                    Date = context.Message.SentTime,
                    PopugId = context.Message.Data.PopugId,
                    Credit = task.AssignPrice,
                    Debit = 0
                });
                break;
            case TransactionTypes.Complete:
                db.TopsBalanceChanges.Add(new TopsBalanceChange()
                {
                    Debit = 0,
                    Credit = task.CompletePrice,
                    Date = context.Message.SentTime
                });
                db.PopugBalanceChanges.Add(new PopugBalanceChange()
                {
                    Date = context.Message.SentTime,
                    PopugId = context.Message.Data.PopugId,
                    Credit = 0,
                    Debit = task.CompletePrice
                });
                break;
            case TransactionTypes.Payment:
                db.PopugBalanceChanges.Add(new PopugBalanceChange()
                {
                    Date = context.Message.SentTime,
                    PopugId = context.Message.Data.PopugId,
                    Credit = context.Message.Data.Credit,
                    Debit = 0,
                    ZeroBalance = true
                });
                break;
        }

        await db.SaveChangesAsync();
    }
}