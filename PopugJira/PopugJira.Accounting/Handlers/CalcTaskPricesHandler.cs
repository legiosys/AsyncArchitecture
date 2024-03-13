using EventSchemaRegistry;
using MassTransit;
using Newtonsoft.Json;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.Models;
using PopugJira.Accounting.Services;
using PopugJira.Tracker.Contracts;

namespace PopugJira.Accounting.Handlers;

public class CalcTaskPricesHandler(AccountingDbContext dbContext, EventSender eventSender) : IConsumer<KafkaEvent<Task_Changed_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<Task_Changed_V1>> context)
    {
        try
        {
            var message = context.Message;
            if(!SchemeValidator.IsValid(message, out var errors))
               throw new Exception($"Invalid scheme: {string.Join(',',errors)}");
            
            var (assign, complete) = PriceCalculator.Calc();
            var task = new PopugTask()
            {
                ReplicationId = message.Data.TaskId,
                Description = message.Data.Description,
                AssignPrice = assign,
                CompletePrice = complete
            };
            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();
            await eventSender.PriceCalculated(task);
        }
        catch (Exception e)
        {
            dbContext.HandleErrors.Add(new HandleError()
            {
                Message = JsonConvert.SerializeObject(context.Message),
                Error = e.Message
            });
            await dbContext.SaveChangesAsync();
        }
    }

}