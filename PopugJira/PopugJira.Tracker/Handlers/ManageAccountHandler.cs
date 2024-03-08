using MassTransit;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Handlers;

public class ManageAccountHandler(TrackerDbContext db) : IConsumer<PopugChanged>
{
    public async Task Consume(ConsumeContext<PopugChanged> context)
    {
        var message = context.Message;
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