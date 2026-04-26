namespace GalacticFleetCommand.Api.Domain;

public interface IVersionedEntity
{
    string Id { get; }
    int Version { get; set; }
}