using EventStore.Client;
using Evntd.EventStoreDB.WebApi.Dto;
using Evntd.EventStoreDB.WebApi.Extensions;
using Evntd.EventStoreDB.WebApi.Model;
using Evntd.EventStoreDB.WebApi.Projections;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEventStoreClient(builder.Configuration.GetConnectionString("EventStore")!);
builder.Services.AddEventStoreProblemDetails();

builder.Services.AddMemoryCache();
builder.Services.AddHostedService<PopularToppingsProjection>();
builder.Services.AddHostedService<FrequentCustomersProjection>();


var app = builder.Build();

app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseFileServer();

// WRITE OPERATIONS
app.MapPost(@"/streams/{stream:regex(^\$[A-Za-z0-9_-]*$)}", HandleAppendToSystemStream);
app.MapPost("/streams/{stream}", HandleAppendToStream);
                        
// READ OPERATIONS
app.MapGet("/streams/$all", HandleReadAll);
app.MapGet("/streams/{stream}", HandleReadStream);
app.MapGet("/streams/{stream}/{revision:long}", HandleReadEvent);
app.MapGet("/projections/by-category/{category}", HandleByCategory);
app.MapGet("/projections/by-correlation-id/{correlationId}", HandleByCorrelationId);
app.MapGet("/projections/by-event-type/{eventType}", HandleByEventType);
app.MapGet("/projections/stream-by-category/{category}", HandleStreamsByCategory);
app.MapGet("/projections/streams", HandleStreams);
           
// CUSTOM PROJECTIONS
app.MapGet("/popular-toppings", HandlePopularToppings);
app.MapGet("/most-frequent", HandleFrequentVisitors);
app.MapGet("/most-pizzas", HandleMostPizzas);

app.Run();

static IResult HandleAppendToSystemStream(HttpContext httpContext, string stream)
{
    return Results.Problem("Not allowed to append events to a EventStore system stream.", stream, StatusCodes.Status400BadRequest, "Invalid Append");
}

static async Task<IResult> HandleAppendToStream(EventStoreClient esdb, HttpContext httpContext, string stream)
{
    AppendToStreamRequestDto dto = await httpContext.ToAppendToStreamRequestDto();
    if (dto.EventData == null)
    {
        return Results.BadRequest();
    }

    AppendToStreamRequest request = AppendToStreamRequestDto.ToDomain(dto);
    IWriteResult writeResult = await esdb.AppendToStreamAsync(request, httpContext.RequestAborted);
    WriteResultDto writeResultDto = WriteResultDto.FromDomain(writeResult);
    return Results.Ok(writeResultDto);
}

static async Task<IResult> HandleReadAll(HttpContext httpContext, EventStoreClient esdb)
{
    ReadAllRequestDto dto = httpContext.ToReadAllRequestDto();
    ReadAllRequest request = ReadAllRequestDto.ToDomain(dto);

    List<ResolvedEventDto> eventDtos = await esdb.ReadAllAsync(request, httpContext.RequestAborted)
        .Select(ResolvedEventDto.FromDomain)
        .ToListAsync();

    return Results.Ok(eventDtos.Select(e => e.ScopeTo(dto.Scope)));
}

static async Task<IResult> HandleReadStream(HttpContext httpContext, EventStoreClient esdb)
{
    ReadStreamRequestDto dto = httpContext.ToReadStreamRequestDto();
    ReadStreamRequest request = ReadStreamRequestDto.ToDomain(dto);

    List<ResolvedEventDto> eventDtos = await esdb.ReadStreamAsync(request, httpContext.RequestAborted)
        .Select(ResolvedEventDto.FromDomain)
        .ToListAsync();

    return Results.Ok(eventDtos.Select(e => e.ScopeTo(dto.Scope)));
}

static async Task<IResult> HandleReadEvent(HttpContext httpContext, EventStoreClient esdb)
{
    ReadStreamRequestDto requestDto = httpContext.ToReadEventRequestDto();
    ReadStreamRequest request = ReadStreamRequestDto.ToDomain(requestDto);
                
    ResolvedEventDto? eventDto = await esdb.ReadStreamAsync(request, httpContext.RequestAborted)
        .Select(ResolvedEventDto.FromDomain)
        .FirstOrDefaultAsync(httpContext.RequestAborted);

    if (eventDto == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(eventDto.ScopeTo(requestDto.Scope));
}

static IResult HandleByCategory(HttpContext httpContext, string category) => Results.Redirect($"/streams/$ce-{category}{httpContext.Request.QueryString}");
static IResult HandleByCorrelationId(HttpContext httpContext, string correlationId) => Results.Redirect($"/streams/$bc-{correlationId}{httpContext.Request.QueryString}");
static IResult HandleByEventType(HttpContext httpContext, string eventType) => Results.Redirect($"/streams/$et-{eventType}{httpContext.Request.QueryString}");
static IResult HandleStreamsByCategory(HttpContext httpContext, string category) => Results.Redirect($"/streams/$category-{category}{httpContext.Request.QueryString}");
static IResult HandleStreams(HttpContext httpContext) => Results.Redirect($"/streams/$streams{httpContext.Request.QueryString}");



static IResult HandlePopularToppings(HttpContext httpContext, IMemoryCache cache) 
{
    var toppings = cache.Get<Dictionary<string,int>>(PopularToppingsProjection.ProjectionStateCacheKey);
    var top10 = toppings.OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key);
    return Results.Ok(top10);
}

static IResult HandleFrequentVisitors(HttpContext httpContext, IMemoryCache cache) 
{
    var customers = cache.Get<Dictionary<string,FrequentCustomersProjection.CustomerInfo>>(FrequentCustomersProjection.ProjectionStateCacheKey);
    var top10 = customers
        .OrderByDescending(kvp => kvp.Value.visits)
        .ThenByDescending(kvp => kvp.Value.pizzas)
        .Take(10)
        .Select(kvp => kvp.Value);

    return Results.Ok(top10);
}

static IResult HandleMostPizzas(HttpContext httpContext, IMemoryCache cache) 
{
    var customers = cache.Get<Dictionary<string,FrequentCustomersProjection.CustomerInfo>>(FrequentCustomersProjection.ProjectionStateCacheKey);
    var top10 = customers
        .OrderByDescending(kvp => kvp.Value.pizzas)
        .ThenBy(kvp => kvp.Value.visits)
        .Take(10)
        .Select(kvp => kvp.Value);
    return Results.Ok(top10);
}