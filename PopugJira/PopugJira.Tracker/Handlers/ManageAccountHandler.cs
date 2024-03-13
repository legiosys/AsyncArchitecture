using EventSchemaRegistry;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Handlers;

public class ManageAccountHandler(TrackerDbContext db) : IConsumer<KafkaEvent<Popug_Changed_V1>>
{
    public async Task Consume(ConsumeContext<KafkaEvent<Popug_Changed_V1>> context)
    {
        SchemeValidator.ValidateOrThrow(context.Message);
        
        var message = context.Message.Data;
        if (message.ChangeType != PopugChangedTypes.Created)
            return;
        
        if(await db.Popugs.AsNoTracking().AnyAsync(x => x.Id == message.Id))
            return;

        db.Popugs.Add(new Popug()
        {
            Id = message.Id,
            Position = message.Changes["Position"]
        });

        await db.SaveChangesAsync();
    }
}