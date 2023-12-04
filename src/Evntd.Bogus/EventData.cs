namespace Evntd.Bogus;

// EVENTSTORE

public class EventData
{
    public string Id { get; set; }
    public string Type { get; set; }
    public object Data { get; set; }
    public Metadata Metadata { get; set; }
}
