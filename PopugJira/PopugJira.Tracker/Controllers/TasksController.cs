using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using PopugJira.Auth.Contracts;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.Models;
using PopugJira.Tracker.Services;
using PopugJira.Tracker.ViewModels.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PopugJira.Tracker.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class TasksController : ControllerBase
{
    [HttpPost]
    public async Task Add([FromBody] AddTaskDto request,
        [FromServices] TrackerDbContext db, 
        [FromServices] TaskAssigner assigner,
        [FromServices] EventSender eventSender)
    {
        var task = new PopugTask()
        {
            Id = Guid.NewGuid(),
            Assignee = await assigner.GetAssignee(),
            Description = request.Description,
            JiraId = request.JiraId,
            Status = PopugTaskStatus.Active
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        await eventSender.TaskCreated(task);
    }

    [HttpPost]
    [Authorize("manager")]
    public async Task Shuffle([FromServices] TrackerDbContext db, 
        [FromServices] TaskAssigner assigner,
        [FromServices] EventSender eventSender)
    {
        var tasks = await db.Tasks
            .Where(x => x.Status.Equals(PopugTaskStatus.Active))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.Assignee = await assigner.GetAssignee();
            await eventSender.TaskAssigned(task);
        }

        await db.SaveChangesAsync();
    }
    
    [HttpPost]
    [Authorize("worker")]
    public async Task Complete([FromBody] CompleteTaskDto request,
        [FromServices] TrackerDbContext db,
        [FromServices] EventSender eventSender)
    {
        var task = await db.Tasks.FirstAsync(x => x.Id == request.Id);
        task.Status = PopugTaskStatus.Completed;
        await db.SaveChangesAsync();
        await eventSender.TaskCompleted(task);
    }

    [HttpGet]
    public async Task<List<PopugTask>> List([FromServices] TrackerDbContext db)
    {
        var user = HttpContext.User;
        var popugId = Guid.Parse(user.GetClaim(Claims.Subject) ?? string.Empty);
        var query = db.Tasks.AsNoTracking().Where(x => x.Status.Equals(PopugTaskStatus.Active));
        if (user.HasClaim(Claims.Role, PopugPositionsEnum.Worker))
            query = query.Where(x => x.Assignee == popugId);
        return await query.ToListAsync();
    }
}