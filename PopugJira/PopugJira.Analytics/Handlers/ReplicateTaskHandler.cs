using System.ComponentModel.DataAnnotations;
using Confluent.Kafka;
using EventSchemaRegistry;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PopugJira.Accounting.Contracts;
using PopugJira.Analytics.Db;
using PopugJira.Analytics.Models;
using PopugJira.Tracker.Contracts;

namespace PopugJira.Analytics.Handlers;

public class ReplicateTaskHandler(AnalyticsDbContext db) : 
    IConsumer<KafkaEvent<Task_Changed_V1>>,
    IConsumer<KafkaEvent<TaskPrice_Calculated_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<Task_Changed_V1>> context)
    {
        SchemeValidator.ValidateOrThrow(context.Message);

        var task = await GetTask(context.Message.Data.TaskId);

        task.Description = context.Message.Data.Description;
        await db.SaveChangesAsync();
    }

    public async Task Consume(ConsumeContext<KafkaEvent<TaskPrice_Calculated_V1>> context)
    {
        SchemeValidator.ValidateOrThrow(context.Message);
        
        var task = await GetTask(context.Message.Data.TaskId);
        task.AssignPrice = context.Message.Data.AssignPrice;
        task.CompletePrice = context.Message.Data.CompletePrice;
        await db.SaveChangesAsync();
    }

    private async Task<PopugTask> GetTask(Guid taskId)
    {
        var task = await db.PopugTasks.FirstOrDefaultAsync(x => x.ReplicationId == taskId);
        if (task == null)
        {
            task = new PopugTask()
            {
                ReplicationId = taskId
            };
            db.PopugTasks.Add(task);
        }

        return task;
    }
}