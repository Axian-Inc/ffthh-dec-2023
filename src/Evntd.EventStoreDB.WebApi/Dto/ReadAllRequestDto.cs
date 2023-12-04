using EventStore.Client;
using Evntd.EventStoreDB.WebApi.Model;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class ReadAllRequestDto
    {
        public string? Direction { get; set; }
        public string? Position { get; set; }
        public string? MaxCount { get; set; }
        public string? ResolveLinkTos { get; set; }
        public string? Deadline { get; set; }
        public string? Scope { get; set; }

        public static ReadAllRequest ToDomain(ReadAllRequestDto dto)
        {
            Direction? direction = Enum.TryParse(dto.Direction, ignoreCase: true, out Direction d) ? d : null;
            Position? position = dto.Position != null && EventStore.Client.Position.TryParse(dto.Position, out Position? p) ? p : null;
            long? maxCount = long.TryParse(dto.MaxCount, out long c) ? c : null;
            bool? resolveLinkTos = bool.TryParse(dto.ResolveLinkTos, out bool x) ? x : null;
            TimeSpan? deadline = TimeSpan.TryParse(dto.Deadline, out TimeSpan t) ? t : null;
            return new ReadAllRequest(direction, position, maxCount, resolveLinkTos, deadline);
        }
    }
}