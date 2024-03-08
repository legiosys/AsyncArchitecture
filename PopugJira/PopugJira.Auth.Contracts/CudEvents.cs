using System.Collections.Immutable;

namespace PopugJira.Auth.Contracts;

//CUD
public record PopugChanged(Guid Id, string ChangeType, Dictionary<string, string> Changes);

public static class PopugChangedTypes
{
    public const string Created = "created";
}