using System.Text.Json;
using System.Text;
using System.Net.Mime;

namespace Evntd.Bogus;

public class InMemoryStore
{
    private readonly List<(string, EventData)> _events = new List<(string, EventData)>();
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    
    public void AppendEvents(string stream, IEnumerable<EventData> events)
    {
        _events.AddRange(events.Select(e => (stream, e)));
    }

    public IEnumerable<HttpRequestMessage> ToHttpRequests()
    {
        return _events
            .OrderBy(x => x.Item2.Metadata.CreatedAt)
            .Select(x => ToHttpRequest($"https://localhost:7177/streams/{x.Item1}", x.Item2));
    }

    private HttpRequestMessage ToHttpRequest(string url, EventData evnt)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        var httpRequestBody = JsonSerializer.Serialize(new[] { evnt }, _serializerOptions);
        httpRequest.Content = new StringContent(httpRequestBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        return httpRequest;
    }
}
