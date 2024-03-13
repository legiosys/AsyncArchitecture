using System.Collections.Immutable;
using EventSchemaRegistry;

namespace PopugJira.Auth.Contracts;

//CUD
public record Popug_Changed_V1(Guid Id, string ChangeType, Dictionary<string, string> Changes);

public static class PopugChangedTypes
{
    public const string Created = "created";
}