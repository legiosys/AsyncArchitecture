using MassTransit;
using PopugJira.Tracker.Contracts;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Services;

public class EventSender(ITopicProducer<TaskOperationPerformed> taskOperationProducer, 
    ITopicProducer<TaskChanged> taskChangedProducer)
{
    public async Task TaskCreated(PopugTask task)
    {
        await taskChangedProducer.Produce(new TaskChanged(task.Id, task.Description, task.Assignee, task.Status, TaskChangedTypes.Created));
        await taskOperationProducer.Produce(new TaskOperationPerformed(task.Id, TaskOperationTypes.AddedNew, task.Assignee));
    }

    public async Task TaskAssigned(PopugTask task)
    {
        await taskChangedProducer.Produce(new TaskChanged(task.Id, task.Description, task.Assignee, task.Status, TaskChangedTypes.Assigned));
        await taskOperationProducer.Produce(new TaskOperationPerformed(task.Id, TaskOperationTypes.Assigned, task.Assignee));
    }
    
    public async Task TaskCompleted(PopugTask task)
    {
        await taskChangedProducer.Produce(new TaskChanged(task.Id, task.Description, task.Assignee, task.Status, TaskChangedTypes.Completed));
        await taskOperationProducer.Produce(new TaskOperationPerformed(task.Id, TaskOperationTypes.Completed, task.Assignee));
    }
}