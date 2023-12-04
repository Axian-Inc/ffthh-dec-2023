using EventStore.Client;
using Evntd.EventStoreDB.WebApi.Model;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class AppendToStreamRequestDto
    {
        public string? StreamName { get; set; }
        public string? ExpectedState { get; set; }
        public string? ExpectedRevision { get; set; }
        public EventDataDto[]? EventData { get; set; }

        public static AppendToStreamRequest ToDomain(AppendToStreamRequestDto dto)
        {
            string streamName = dto.StreamName ?? throw new ArgumentException($"A {nameof(StreamName)} is required.", nameof(dto));
            EventData[] eventData = dto.EventData?.Select(EventDataDto.ToDomain).ToArray() ?? Array.Empty<EventData>();
            StreamState? expectedState = ParseStreamState(dto.ExpectedState);
            StreamRevision? expectedRevision = long.TryParse(dto.ExpectedRevision, out long result) ? StreamRevision.FromInt64(result) : null;
            return new AppendToStreamRequest(streamName, eventData, expectedState, expectedRevision);
        }

        private static StreamState? ParseStreamState(string? streamState)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (string.Equals("NoStream", streamState, comparison))
            {
                return StreamState.NoStream;
            }
            if (string.Equals("Any", streamState, comparison))
            {
                return StreamState.Any;
            }
            if (string.Equals("StreamExists", streamState, comparison))
            {
                return StreamState.StreamExists;
            }

            return null;
        }
    }
}
