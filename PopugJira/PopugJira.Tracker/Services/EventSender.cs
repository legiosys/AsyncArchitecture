using EventSchemaRegistry;
using MassTransit;
using PopugJira.Tracker.Contracts;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Services;

public class EventSender(
    ITopicProducer<KafkaEvent<Task_Added_V1>> taskAddedProducer, 
    ITopicProducer<KafkaEvent<Task_Assigned_V1>> taskAssignedProducer, 
    ITopicProducer<KafkaEvent<Task_Completed_V1>> taskCompletedProducer, 
    ITopicProducer<KafkaEvent<Task_Changed_V1>> taskChangedProducer)
{
    private const string Producer = "tracker";

    public async Task TaskCreated(PopugTask task)
    {
        var messageCud = new KafkaEvent<Task_Changed_V1>(
            new Task_Changed_V1(task.Id, task.Description, task.Assignee, task.Status, TaskChangedTypes.Created),
            Producer);
        SchemeValidator.ValidateOrThrow(messageCud);
        await taskChangedProducer.Produce(messageCud);
        
        var messageCudV2 = new KafkaEvent<Task_Changed_V2>(
            new Task_Changed_V2(task.Id, task.Description, task.JiraId, task.Assignee, task.Status, TaskChangedTypes.Created),
            Producer);
        SchemeValidator.ValidateOrThrow(messageCudV2);
        await taskChangedProducer.Produce(messageCudV2);

        var messageBusiness = new KafkaEvent<Task_Added_V1>(new Task_Added_V1(task.Id, task.Assignee), Producer);
        SchemeValidator.ValidateOrThrow(messageBusiness);
        await taskAddedProducer.Produce(messageBusiness);
    }

    public async Task TaskAssigned(PopugTask task)
    {
        var message = new KafkaEvent<Task_Assigned_V1>(new Task_Assigned_V1(task.Id, task.Assignee), Producer);
        SchemeValidator.ValidateOrThrow(message);
        await taskAssignedProducer.Produce(message);   
    }
    
    public async Task TaskCompleted(PopugTask task)
    {
        var message = new KafkaEvent<Task_Completed_V1>(new Task_Completed_V1(task.Id, task.Assignee), Producer);
        SchemeValidator.ValidateOrThrow(message);
        await taskCompletedProducer.Produce(message);    
    }
}