using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Db;

namespace PopugJira.Tracker.Services;

public class TaskAssigner(TrackerDbContext db)
{

    public async Task<Guid> GetAssignee()
    {
        var popugs = await db.Popugs.AsNoTracking()
            .Where(x => x.Position.Equals(PopugPositionsEnum.Worker))
            .ToArrayAsync();

        var selectedPopug = BigBrotherSelectPopugMethod(popugs.Length);
        return popugs[selectedPopug].Id;
    }

    private static int BigBrotherSelectPopugMethod(int popugsCount) => Random.Shared.Next(0, popugsCount - 1);
}