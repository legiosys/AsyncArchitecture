using Confluent.Kafka;
using EventSchemaRegistry;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PopugJira.Accounting.Contracts;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.Models;
using PopugJira.Accounting.Services;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Contracts;

namespace PopugJira.Accounting.Handlers;

public class ApplyTransactionHandler(AccountingDbContext dbContext, EventSender eventSender) : 
    IConsumer<KafkaEvent<Task_Added_V1>>, 
    IConsumer<KafkaEvent<Task_Assigned_V1>>,
    IConsumer<KafkaEvent<Task_Completed_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<Task_Added_V1>> context)
    {
        try
        {
            if(!SchemeValidator.IsValid(context.Message, out var errors))
                throw new Exception($"Invalid scheme: {string.Join(',',errors)}");

            await ApplyAssign(context.Message.Data.TaskId, context.Message.Data.Assignee, context.Message.SentTime);
        }
        catch (Exception e)
        {
            await HandleError(context.Message, e);
        }
    }

    public async Task Consume(ConsumeContext<KafkaEvent<Task_Assigned_V1>> context)
    {
        try
        {
            if(!SchemeValidator.IsValid(context.Message, out var errors))
                throw new Exception($"Invalid scheme: {string.Join(',',errors)}");
            
            await ApplyAssign(context.Message.Data.TaskId, context.Message.Data.Assignee, context.Message.SentTime);
        }
        catch (Exception e)
        {
            await HandleError(context.Message, e);
        }
    }

    public async Task Consume(ConsumeContext<KafkaEvent<Task_Completed_V1>> context)
    {
        try
        {
            if(!SchemeValidator.IsValid(context.Message, out var errors))
                throw new Exception($"Invalid scheme: {string.Join(',',errors)}");
            
            await ApplyComplete(context.Message.Data.TaskId, context.Message.Data.Assignee, context.Message.SentTime);
        }
        catch (Exception e)
        {
            await HandleError(context.Message, e);
        }
    }

    private async Task ApplyAssign(Guid taskId, Guid assignee, DateTimeOffset eventTime)
    {
        var (popug, task, cycle) = await GetAggregates(taskId, assignee);

        var transaction = new Transaction()
        {
            PopugId = popug.Id,
            TaskId = task.Id,
            TransactionType = TransactionTypes.Assign,
            Credit = task.AssignPrice,
            Debit = 0,
            Date = eventTime.ToUniversalTime(),
            BillingCycle = cycle.Id
        };
        popug.Balance -= task.AssignPrice;
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
        await eventSender.TransactionApplied(transaction, popug.ReplicationId, task);
    }
    
    private async Task ApplyComplete(Guid taskId, Guid assignee, DateTimeOffset eventTime)
    {
        var (popug, task, cycle) = await GetAggregates(taskId, assignee);

        var transaction = new Transaction()
        {
            PopugId = popug.Id,
            TaskId = task.Id,
            TransactionType = TransactionTypes.Complete,
            Credit = 0,
            Debit = task.CompletePrice,
            Date = eventTime.ToUniversalTime(),
            BillingCycle = cycle.Id
        };
        popug.Balance += task.CompletePrice;
        
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
        await eventSender.TransactionApplied(transaction, popug.ReplicationId, task);
    }

    private async Task<(Popug popug, PopugTask task, BillingCycle billingCycle)> GetAggregates(Guid taskId, Guid assignee)
    {
        var popug = await dbContext.Popugs.FirstOrDefaultAsync(x => x.ReplicationId == assignee);
        if (popug == null)
        {
            popug = new Popug()
            {
                ReplicationId = assignee,
                Position = PopugPositionsEnum.Worker
            };
            dbContext.Popugs.Add(popug);
        }

        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.ReplicationId == taskId);
        if (task == null)
        {
            var (assign, complete) = PriceCalculator.Calc();
            task = new PopugTask()
            {
                ReplicationId = taskId,
                AssignPrice = assign,
                CompletePrice = complete
            };
            dbContext.Tasks.Add(task);
        }

        var billingCycle = await dbContext.BillingCycles.FirstOrDefaultAsync(x => x.End == null);
        if (billingCycle == null)
        {
            billingCycle = new BillingCycle()
            {
                Start = DateTimeOffset.UtcNow
            };
            dbContext.BillingCycles.Add(billingCycle);
        }

        return (popug, task, billingCycle);
    }
    private async Task HandleError(object message, Exception e)
    {
        dbContext.HandleErrors.Add(new HandleError()
        {
            Message = JsonConvert.SerializeObject(message),
            Error = e.Message
        });
        await dbContext.SaveChangesAsync();
    }
}