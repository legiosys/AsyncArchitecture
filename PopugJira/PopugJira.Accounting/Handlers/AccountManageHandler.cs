using Confluent.Kafka;
using EventSchemaRegistry;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.Models;
using PopugJira.Auth.Contracts;

namespace PopugJira.Accounting.Handlers;

public class AccountManageHandler(AccountingDbContext db) : IConsumer<KafkaEvent<Popug_Changed_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<Popug_Changed_V1>> context)
    {
        if(!SchemeValidator.IsValid(context.Message, out var errors))
            Console.WriteLine(string.Join(',',errors));
        var message = context.Message.Data;
        if (message.ChangeType != PopugChangedTypes.Created)
            return;
        
        if(await db.Popugs.AsNoTracking().AnyAsync(x => x.ReplicationId == message.Id))
            return;

        db.Popugs.Add(new Popug()
        {
            ReplicationId = message.Id,
            Position = message.Changes["Position"]
        });

        await db.SaveChangesAsync();
    }
}