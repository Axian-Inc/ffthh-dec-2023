using EventStore.Client;

namespace Evntd.EventStoreDB.WebApi.Model
{
    public class ReadAllRequest
    {
        public Direction Direction { get; }
        public Position Position { get; }
        public long MaxCount { get; }
        public bool ResolveLinkTos { get; }
        public TimeSpan? Deadline { get; }

        public ReadAllRequest(Direction? direction, Position? position, long? maxCount, bool? resolveLinkTos, TimeSpan? deadline)
        {
            Direction = direction ?? Direction.Forwards;
            Position = position ?? (Direction == Direction.Forwards ? Position.Start : Position.End);
            MaxCount = maxCount ?? long.MaxValue;
            ResolveLinkTos = resolveLinkTos ?? true;
            Deadline = deadline;
        }
    }
}