using EventStore.Client;
using Evntd.EventStoreDB.WebApi.Model;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class ReadStreamRequestDto
    {
        public string? StreamName { get; set; }
        public string? Direction { get; set; }
        public string? Revision { get; set; }
        public string? MaxCount { get; set; }
        public string? ResolveLinkTos { get; set; }
        public string? Deadline { get; set; }
        public string? Scope { get; set; }

        public static ReadStreamRequest ToDomain(ReadStreamRequestDto dto)
        {
            string streamName = dto.StreamName ?? throw new ArgumentException($"A {nameof(StreamName)} is required.", "dto");
            Direction? direction = Enum.TryParse(dto.Direction, ignoreCase: true, out Direction d) ? d : null;
            StreamPosition? revision = long.TryParse(dto.Revision, out long result) ? StreamPosition.FromInt64(result) : null;
            long? maxCount = long.TryParse(dto.MaxCount, out result) ? result : null;
            bool? resolveLinkTos = dto.TryParseResolveLinkTosToBoolean();
            TimeSpan? deadline = dto.TryParseDeadlineAsTimeSpan();

            return new ReadStreamRequest(streamName, direction, revision, maxCount, resolveLinkTos, deadline);
        }

        private bool? TryParseResolveLinkTosToBoolean()
        {
            if (ResolveLinkTos == null) return null;
            if (int.TryParse(ResolveLinkTos, out int i)) return i != 0;
            if (bool.TryParse(ResolveLinkTos, out bool b)) return b;
            if (string.Equals("on", ResolveLinkTos, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals("off", ResolveLinkTos, StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }

        private TimeSpan? TryParseDeadlineAsTimeSpan()
        {
            if (Deadline == null)
                return null;

            if (long.TryParse(Deadline, out long x))
            {
                return TimeSpan.FromMilliseconds(x);
            }

            if (TimeSpan.TryParse(Deadline, out TimeSpan t))
            {
                return t;
            }

            return null;
        }
    }

    public static class Scopes
    {
        public static readonly string Resolved = "resolved";
        public static readonly string Event = "event";
        public static readonly string Data = "data";
        public static readonly string Metadata = "metadata";
    }
}