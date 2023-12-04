using EventStore.Client;

namespace Evntd.EventStoreDB.WebApi.Model
{
    public class AppendToStreamRequest
    {
        public AppendToStreamRequest(string streamName, EventData[] eventData, StreamState? expectedState, StreamRevision? expectedRevision)
        {
            StreamName = streamName;
            EventData = eventData;
            ExpectedState = expectedState;
            ExpectedRevision = expectedRevision;
        }

        public string StreamName { get; }
        public StreamRevision? ExpectedRevision { get; }
        public StreamState? ExpectedState { get; }
        public EventData[] EventData { get; }
    }
}
