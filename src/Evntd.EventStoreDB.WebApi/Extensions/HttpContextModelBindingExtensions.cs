using Evntd.EventStoreDB.WebApi.Dto;

namespace Evntd.EventStoreDB.WebApi.Extensions
{
    public static class HttpContextModelBindingExtensions
    {
        public static async Task<AppendToStreamRequestDto> ToAppendToStreamRequestDto(this HttpContext httpContext)
        {
            return new AppendToStreamRequestDto
            {
                StreamName = (string?)httpContext.GetRouteValue("stream"),
                ExpectedRevision = httpContext.FromLastHeader("ExpectedRevision"),
                ExpectedState = httpContext.FromLastHeader("ExpectedState"),
                EventData = await httpContext.Request.ReadFromJsonAsync<EventDataDto[]>(httpContext.RequestAborted)
            };
        }

        public static ReadAllRequestDto ToReadAllRequestDto(this HttpContext httpContext)
        {
            return new ReadAllRequestDto
            {
                Direction = FromLastQueryParam(httpContext, "d", "direction"),
                Position = FromLastQueryParam(httpContext, "p", "position"),
                MaxCount = FromLastQueryParam(httpContext, "c", "count"),
                ResolveLinkTos = FromLastQueryParam(httpContext, "l", "resolvelinks"),
                Deadline = FromLastQueryParam(httpContext, "deadline"),
                Scope = FromLastQueryParam(httpContext, "s", "scope")
            };
        }

        public static ReadStreamRequestDto ToReadStreamRequestDto(this HttpContext httpContext)
        {
            return new ReadStreamRequestDto
            {
                StreamName = (string?)httpContext.GetRouteValue("stream"),
                Direction = FromLastQueryParam(httpContext, "d", "direction"),
                Revision = FromLastQueryParam(httpContext, "r", "revision"),
                MaxCount = FromLastQueryParam(httpContext, "c", "count"),
                ResolveLinkTos = FromLastQueryParam(httpContext, "l", "resolvelinks"),
                Deadline = FromLastQueryParam(httpContext, "deadline"),
                Scope = FromLastQueryParam(httpContext, "s", "scope")
            };
        }

        public static ReadStreamRequestDto ToReadEventRequestDto(this HttpContext httpContext)
        {
            return new ReadStreamRequestDto
            {
                StreamName = (string?)httpContext.GetRouteValue("stream"),
                Revision = (string?)httpContext.GetRouteValue("revision"),
                MaxCount = "1",
                ResolveLinkTos = FromLastQueryParam(httpContext, "l", "resolvelinks"),
                Deadline = FromLastQueryParam(httpContext, "deadline"),
                Scope = FromLastQueryParam(httpContext, "s", "scope")
            };
        }

        public static string? FromLastHeader(this HttpContext httpContext, params string[] headers)
        {
            return headers.SelectMany(h => httpContext.Request.Headers[h]).LastOrDefault();
        }

        public static string? FromLastQueryParam(this HttpContext httpContext, params string[] args)
        {
            return args.SelectMany(arg => httpContext.Request.Query[arg]).LastOrDefault();
        }
    }
}
