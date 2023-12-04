using EventStore.Client;
using System.Net.Mime;
using System.Text.Json;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class ResolvedEventDto
    {
        public EventRecordDto? Event { get; set; }

        public EventRecordDto? Link { get; set; }

        public string? OriginalPosition { get; set; }

        public static ResolvedEventDto FromDomain(ResolvedEvent resolvedEvent)
        {
            return new ResolvedEventDto
            {
                Event = EventRecordDto.FromDomain(resolvedEvent.Event),
                Link = EventRecordDto.FromDomain(resolvedEvent.Link),
                OriginalPosition = resolvedEvent.OriginalPosition.ToString()
            };
        }

        public object? ScopeTo(string? scope)
        {
            if (Scopes.Event == scope)
            {
                return Event;
            }
            else if (Scopes.Data == scope)
            {
                if (Event != null)
                {
                    if (Event.Data != null)
                    {
                        if (Event.ContentType == MediaTypeNames.Application.Json)
                        {
                            try
                            {
                                return JsonSerializer.Deserialize<JsonDocument>(Event.Data);
                            }
                            catch (JsonException)
                            {
                                // No big deal, just return it raw.
                            }
                        }

                        return Event.Data;
                    }
                }
            }
            else if (Scopes.Metadata == scope)
            {
                if (null != Event?.Metadata)
                {
                    try
                    {
                        return JsonSerializer.Deserialize<JsonDocument>(Event.Metadata);
                    }
                    catch (JsonException)
                    {
                        return Event.Metadata;
                    }
                }
            }

            return this;
        }
    }
}
