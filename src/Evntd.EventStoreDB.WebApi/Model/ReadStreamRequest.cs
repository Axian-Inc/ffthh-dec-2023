using EventStore.Client;

namespace Evntd.EventStoreDB.WebApi.Model
{
    public class ReadStreamRequest
    {
        public Direction Direction { get; }
        public string StreamName { get; }
        public StreamPosition Revision { get; }
        public long MaxCount { get; }
        public bool ResolveLinkTos { get; }
        public TimeSpan? Deadline { get; }

        public ReadStreamRequest(string streamName, Direction? direction, StreamPosition? revision, long? maxCount, bool? resolveLinkTos, TimeSpan? deadline)
        {
            Direction = direction ?? Direction.Forwards;
            StreamName = streamName;
            Revision = revision ?? (Direction == Direction.Forwards ? StreamPosition.Start : StreamPosition.End);
            MaxCount = maxCount ?? long.MaxValue;
            ResolveLinkTos = resolveLinkTos ?? true;
            Deadline = deadline;
        }
    }
}