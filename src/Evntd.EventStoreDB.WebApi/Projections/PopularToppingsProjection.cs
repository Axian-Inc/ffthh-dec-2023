using System.Text;
using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.Caching.Memory;

namespace Evntd.EventStoreDB.WebApi.Projections;

public class PopularToppingsProjection : BackgroundService
{
    const string CheckpointCacheKey = "CHECKPOINT/PopularToppings";
    public const string ProjectionStateCacheKey = "PROJECTION/PopularToppings";
    const string SubscribedStreamName = "$ce-order";

    private readonly EventStoreClient _esdb;
    private readonly IMemoryCache _cache;    

    public PopularToppingsProjection(EventStoreClient esdb, IMemoryCache cache)
    {
        this._esdb = esdb;
        this._cache = cache;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        FromStream revision = FromStream.Start;
        _cache.TryGetValue(CheckpointCacheKey, out revision);
        await _esdb.SubscribeToStreamAsync(SubscribedStreamName, revision, OnEventAppeared, resolveLinkTos: true, OnSubscriptionDropped, userCredentials: null, cancellationToken);
    }

    private async Task OnEventAppeared(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        switch (resolvedEvent.Event.EventType)
        {
            case "DineInOrderPlaced":
            case "CarryoutOrderPlaced":
            case "DeliveryOrderPlaced":
                var state = LoadProjectionState();

                var order = JsonSerializer.Deserialize<OrderPlacedEvent>(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span));
                var toppings = order?.pizzas.SelectMany(p => Enumerable.Union(p.cheeses,p.meats).Union(p.vegetables)) ?? Enumerable.Empty<string>();
                

                foreach (var topping in toppings) 
                {
                    if (!state.TryGetValue(topping, out int count))
                    {
                        state.Add(topping, 1);
                    }
                    else 
                    {
                        state[topping] = count + 1;
                    }
                }


                break;
        }

        _cache.Set(CheckpointCacheKey, resolvedEvent.Link.EventNumber);
    }

    private void OnSubscriptionDropped(StreamSubscription subscription, SubscriptionDroppedReason droppedReason, Exception? error)
    {
        // Log the error, do stuff to resubscribe if needed
    }

    private Dictionary<string,int> LoadProjectionState()
    {
        if (!_cache.TryGetValue(ProjectionStateCacheKey, out Dictionary<string,int> state)) 
        {
            state = new Dictionary<string,int>();
            _cache.Set(ProjectionStateCacheKey, state);
        }

        return state;
    }

    public class OrderPlacedEvent 
    {
        public List<Pizza> pizzas { get; set; }
    }

    public class Pizza
    {
        public string[] cheeses { get; set; }
        public string[] meats { get; set; }
        public string[] vegetables { get; set; }
    }
}