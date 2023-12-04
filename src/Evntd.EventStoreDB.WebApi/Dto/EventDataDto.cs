using EventStore.Client;
using System.Text.Json;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class EventDataDto
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public object? Data { get; set; }
        public object? Metadata { get; set; }

        public static EventData ToDomain(EventDataDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Type))
            {
                throw new ArgumentException("requires a type", nameof(dto));
            }

            Uuid eventId = Guid.TryParse(dto.Id, out Guid guid) ? Uuid.FromGuid(guid) : Uuid.NewUuid();

            ReadOnlyMemory<byte> data = dto.Data != null ? JsonSerializer.SerializeToUtf8Bytes(dto.Data) : new byte[0];
            ReadOnlyMemory<byte> metadata = dto.Metadata != null ? JsonSerializer.SerializeToUtf8Bytes(dto.Metadata) : new byte[0];

            return new EventData(eventId, dto.Type, data, metadata);
        }
    }
}
