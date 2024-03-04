using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Services;

public class TaskAssigner(TrackerDbContext db)
{
    private Popug[]? _inMemoryPopugs;
    
    public async Task<Guid> GetAssignee()
    {
        _inMemoryPopugs ??= await db.Popugs.AsNoTracking()
            .Where(x => x.Position.Equals(PopugPositionsEnum.Worker))
            .ToArrayAsync();

        if (_inMemoryPopugs.Length == 0)
            throw new Exception("Нет попугов, на которых можно назначить задачу");
            
        var selectedPopug = BigBrotherSelectPopugMethod(_inMemoryPopugs.Length);
        return _inMemoryPopugs[selectedPopug].Id;
    }

    private static int BigBrotherSelectPopugMethod(int popugsCount) => Random.Shared.Next(0, popugsCount - 1);
}