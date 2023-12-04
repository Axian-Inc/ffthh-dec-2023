using EventStore.Client;
using System.Text;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class EventRecordDto
    {
        public string? ContentType { get; set; }
        public DateTime? Created { get; set; }
        public string? Data { get; set; }
        public Guid EventId { get; set; }
        public ulong EventNumber { get; set; }
        public string? EventType { get; set; }
        public string? EventStreamId { get; set; }
        public string? Metadata { get; set; }
        public string? Position { get; set; }

        public static EventRecordDto? FromDomain(EventRecord? eventRecord)
        {
            if (eventRecord == null)
            {
                return null;
            }

            return new EventRecordDto
            {
                ContentType = eventRecord.ContentType,
                Created = eventRecord.Created,
                Data = Encoding.UTF8.GetString(eventRecord.Data.Span),
                EventId = eventRecord.EventId.ToGuid(),
                EventNumber = eventRecord.EventNumber,
                EventStreamId = eventRecord.EventStreamId,
                EventType = eventRecord.EventType,
                Metadata = Encoding.UTF8.GetString(eventRecord.Metadata.Span),
                Position = eventRecord.Position.ToString()
            };
        }

    }
}
