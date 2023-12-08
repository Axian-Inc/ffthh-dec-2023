using System.Text;
using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.Caching.Memory;

namespace Evntd.EventStoreDB.WebApi.Projections;

public class FrequentCustomersProjection : BackgroundService
{
    const string CheckpointCacheKey = "CHECKPOINT/FrequentCustomers";
    public const string ProjectionStateCacheKey = "PROJECTION/FrequentCustomers";
    const string SubscribedStreamName = "$ce-order";

    private readonly EventStoreClient _esdb;
    private readonly IMemoryCache _cache;    

    public FrequentCustomersProjection(EventStoreClient esdb, IMemoryCache cache)
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
            case "CarryoutOrderPlaced":
            case "DeliveryOrderPlaced":
                var state = LoadProjectionState();

                var order = JsonSerializer.Deserialize<OrderPlacedEvent>(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span));
                                
                CustomerInfo customerInfo = null;
                if (!state.TryGetValue(order.customerId, out customerInfo)) {
                    customerInfo = new CustomerInfo { customerId = order.customerId, pizzas = 0, visits = 0 };
                    state.Add(order.customerId, customerInfo);
                } 


                customerInfo.visits += 1;
                customerInfo.pizzas += order.pizzas.Count;

                break;
        }

        _cache.Set(CheckpointCacheKey, resolvedEvent.Link.EventNumber);
    }

    private void OnSubscriptionDropped(StreamSubscription subscription, SubscriptionDroppedReason droppedReason, Exception? error)
    {
        // Log the error, do stuff to resubscribe if needed
    }

    private Dictionary<string,CustomerInfo> LoadProjectionState()
    {
        if (!_cache.TryGetValue(ProjectionStateCacheKey, out Dictionary<string,CustomerInfo> state)) 
        {
            state = new Dictionary<string,CustomerInfo>();
            _cache.Set(ProjectionStateCacheKey, state);
        }

        return state;
    }

    class OrderPlacedEvent 
    {
        public string customerId {get; set; }
        public List<Pizza> pizzas { get; set; }
    }

    class Pizza
    {

    }

    public class CustomerInfo
    {
        public string customerId {get; set;}
        public int visits { get; set; }
        public int pizzas { get; set; }
    }
}