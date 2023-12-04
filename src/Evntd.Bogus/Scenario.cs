using Bogus;

namespace Evntd.Bogus;

public class Scenario
{
    private readonly InMemoryStore _store;
    private readonly ScenarioOptions _options;
    private readonly Faker _faker;

    private readonly Dictionary<Guid, Address?> _customers = new Dictionary<Guid, Address?>();
    private readonly List<Employee> _employees = new List<Employee>();

    private readonly Faker<Metadata> _fakeMetadata;
    private readonly Faker<EventData> _fakeEventData;
    
    private readonly Faker<Address> _fakeAddress;
    private readonly Faker<Pizza> _fakePizza;
    private readonly Faker<Employee> _fakeEmployee;

    private readonly Faker<CustomerRegisteredEvent> _fakeCustomerRegistered;
    private readonly Faker<DineInOrderPlacedEvent> _fakeDineInOrderPlaced;
    private readonly Faker<CarryoutOrderPlacedEvent> _fakeCarryoutOrderPlaced;
    private readonly Faker<DeliveryOrderPlacedEvent> _fakeDeliveryOrderPlaced;


    public Scenario(InMemoryStore store, ScenarioOptions options)
    {
        _store = store;
        _options = options;
        _faker = new Faker();
        _fakeCustomerRegistered = new Faker<CustomerRegisteredEvent>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.EmailAddress, (f, x) => f.Internet.Email(x.FirstName, x.LastName))
            .RuleFor(x => x.PhoneNumber, (f, x) => f.Phone.PhoneNumber(options.Customer.PhoneNumberFormat))
            .RuleFor(x => x.PasswordHash, (f, x) => f.Random.Hash(options.Customer.PasswordHashLength))
            .RuleFor(x => x.PasswordSalt, (f, x) => f.Random.Hash(options.Customer.PasswordSaltLength));

        _fakeAddress = new Faker<Address>()
            .RuleFor(x => x.Street, f => f.Address.StreetAddress())
            .RuleFor(x => x.City, f => f.Address.City())
            .RuleFor(x => x.State, f => f.Address.State())
            .RuleFor(x => x.ZipCode, f => f.Address.ZipCode(_options.Customer.ZipCodeFormat));

        _fakeEmployee = new Faker<Employee>()
            .RuleFor(x => x.EmployeeId, f => f.Random.Uuid())
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName());

        _fakeMetadata = new Faker<Metadata>()
           .RuleFor(x => x.CorrelationId, f => f.Random.Uuid().ToString());

        _fakeEventData = new Faker<EventData>()
            .RuleFor(x => x.Id, f => f.Random.Uuid().ToString())
            .RuleFor(x => x.Metadata, f => _fakeMetadata.Generate());

        var pizzaOptions = _options.Pizza;
        _fakePizza = new Faker<Pizza>()
            .RuleFor(p => p.Size, f => f.PickWeighted(pizzaOptions.Sizes))
            .RuleFor(p => p.Sauce, f => f.PickWeighted(pizzaOptions.Sauces))
            .RuleFor(p => p.CheeseQuantity, f => f.PickWeighted(pizzaOptions.CheeseQuantities))
            /* gross */.RuleFor(p => p.Cheeses, (f, p) => p.CheeseQuantity == "None" ? Array.Empty<string>() : f.PickMultipleChoice(pizzaOptions.CheeseOptions))
            .RuleFor(p => p.Meats, f => f.PickMultipleChoice(pizzaOptions.MeatOptions))
            .RuleFor(p => p.Vegetables, f => f.PickMultipleChoice(pizzaOptions.VegetableOptions));

        _fakeDineInOrderPlaced = new Faker<DineInOrderPlacedEvent>();
        _fakeCarryoutOrderPlaced = new Faker<CarryoutOrderPlacedEvent>();
        _fakeDeliveryOrderPlaced = new Faker<DeliveryOrderPlacedEvent>();
    }

    public IEnumerable<HttpRequestMessage> Simulate()
    {
        InitializeEmployees();
        InitializeCustomerBase();
        var start = DateTimeOffset.Now.Add(TimeSpan.FromDays(-_options.Days));
        for (int i = 0; i < _options.Days; i++)
        {
            OperateDay(start.AddDays(i));
        }

        return _store.ToHttpRequests();
    }


    public void InitializeCustomerBase()
    {
        int initialCustomerBase = _faker.Random.Int(_options.Customer.MinInitialCustomerBase, _options.Customer.MaxInitialCustomerBase);

        DateTimeOffset now = _options.Customer.EarliestCustomerRegistration.Add(_faker.Date.Timespan(_options.Customer.MaxTimeBetweenRegistrations));

        for (int i=0; i < initialCustomerBase; i++)
        {
            RegisterCustomer(now);

            now = now.Add(_faker.Date.Timespan(_options.Customer.MaxTimeBetweenRegistrations));
        }
    }

    public void InitializeEmployees()
    {
        _employees.AddRange(_fakeEmployee.Generate(_options.Employee.NumEmployees));
    }


    public Guid RegisterCustomer(DateTimeOffset timestamp)
    {
        var birthDateProvided = _faker.Random.Bool(_options.Customer.PercentBirthDateProvided);
        var customerId = _faker.Random.Guid();

        var customerRegistered = birthDateProvided ?
            _fakeCustomerRegistered
                .RuleFor(x => x.BirthDate, f => f.Date.Between(_options.Customer.MinBirthDate, _options.Customer.MaxBirthDate))
                .Generate() : _fakeCustomerRegistered.Generate();

        var addressProvided = _faker.Random.Bool(_options.Customer.PercentSetDefaultAddress);
        var address = addressProvided ? _fakeAddress.Generate() : null;

        _customers.Add(customerId, address);

        var events = new List<EventData> { CreateEventData("CustomerRegistered", customerRegistered, timestamp) };

        if (addressProvided)
        {
            events.Add(SetDefaultAddress(customerId, address, timestamp.AddMilliseconds(_faker.Random.Double(0, 1000))));
        }

        _store.AppendEvents($"customer-{customerId.ToString("n")}", events);

        return customerId;
    }

    public EventData SetDefaultAddress(Guid customerId, Address address, DateTimeOffset timestamp)
    {
        var addressDefaulted = new AddressDefaultedEvent
        {
            Label = _faker.PickRandom(_options.Customer.DefaultAddressLabels),
            Street = address.Street,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode
        };

        return CreateEventData("AddressDefaulted", addressDefaulted, timestamp);
    }

    public void PlaceDineInOrder(int pizzas, DateTimeOffset timestamp)
    {
        var events = new List<EventData>();
        var orderId = _faker.Random.Guid();

        var order = _fakeDineInOrderPlaced
            .RuleFor(x => x.Pizzas, f => _fakePizza.Generate(pizzas).Select(ToOrdered).ToList())
            .Generate();

        events.Add(CreateEventData("DineInOrderPlaced", order, timestamp));

        var now = timestamp;

        foreach (var pizza in order.Pizzas)
        {
            now = now.Add(_faker.Date.Timespan(_options.Employee.PizzaPrepTime));
            events.Add(CreateEventData("PizzaPrepared", new PizzaPreparedEvent { PizzaId = pizza.PizzaId, PreparedBy = PickRandomEmployee() }, now));


            now = now.Add(_options.Employee.PizzaBakeTime);
            events.Add(CreateEventData("PizzaBaked", new PizzaBakedEvent { PizzaId = pizza.PizzaId, BakedBy = PickRandomEmployee() }, now));
        }

        now = now.Add(_faker.Date.Timespan(_options.Employee.OrderPrepTime));
        events.Add(CreateEventData("OrderPrepared", new OrderPreparedEvent { PreparedBy = PickRandomEmployee() }, now));

        now = now.Add(_faker.Date.Timespan(_options.Customer.CarryoutTime));
        events.Add(CreateEventData("OrderServed", new OrderServedEvent(), now));

        _store.AppendEvents($"order-{orderId.ToString("n")}", events);
    }

    private OrderedPizza ToOrdered(Pizza p, int i)
    {
        return new OrderedPizza
        {
            PizzaId = i + 1,
            Size = p.Size,
            Sauce = p.Sauce,
            Meats = p.Meats,
            CheeseQuantity = p.CheeseQuantity,
            Cheeses = p.Cheeses,
            Vegetables = p.Vegetables
        };
    }

    public void PlaceCarryoutOrder(Guid customerId, int pizzas, DateTimeOffset timestamp)
    {
        var events = new List<EventData>();

        var orderId = _faker.Random.Guid();

        var order = _fakeCarryoutOrderPlaced
            .RuleFor(x => x.CustomerId, f => customerId)
            .RuleFor(x => x.Pizzas, f => _fakePizza.Generate(pizzas).Select(ToOrdered).ToList())
            .Generate();

        order.CustomerId = customerId;

        events.Add(CreateEventData("CarryoutOrderPlaced", order, timestamp));

        var now = timestamp;

        foreach (var pizza in order.Pizzas)
        {
            now = now.Add(_faker.Date.Timespan(_options.Employee.PizzaPrepTime));
            events.Add(CreateEventData("PizzaPrepared", new PizzaPreparedEvent { PizzaId = pizza.PizzaId, PreparedBy = PickRandomEmployee() }, now));


            now = now.Add(_options.Employee.PizzaBakeTime);
            events.Add(CreateEventData("PizzaBaked", new PizzaBakedEvent { PizzaId = pizza.PizzaId, BakedBy = PickRandomEmployee() }, now));
        }

        now = now.Add(_faker.Date.Timespan(_options.Employee.OrderPrepTime));
        events.Add(CreateEventData("OrderPrepared", new OrderPreparedEvent { PreparedBy = PickRandomEmployee() }, now));

        now = now.Add(_faker.Date.Timespan(_options.Customer.CarryoutTime));
        events.Add(CreateEventData("OrderReceived", new OrderReceivedEvent(), now));

        _store.AppendEvents($"order-{orderId.ToString("n")}", events);
    }

    public void PlaceDeliveryOrder(Guid customerId, int pizzas, DateTimeOffset timestamp)
    {
        var events = new List<EventData>();

        var orderId = _faker.Random.Guid();

        var order = _fakeDeliveryOrderPlaced
            .RuleFor(x => x.CustomerId, f => customerId)
            .RuleFor(x => x.Pizzas, f => _fakePizza.Generate(pizzas).Select(ToOrdered).ToList())
            .RuleFor(x => x.DeliveryAddress, f => _customers[customerId] ?? _fakeAddress.Generate())
            .Generate();

        order.CustomerId = customerId;

        events.Add(CreateEventData("DeliveryOrderPlaced", order, timestamp));
        
        var now = timestamp; 

        foreach (var pizza in order.Pizzas)
        {
            now = now.Add(_faker.Date.Timespan(_options.Employee.PizzaPrepTime));
            events.Add(CreateEventData("PizzaPrepared", new PizzaPreparedEvent { PizzaId = pizza.PizzaId, PreparedBy = PickRandomEmployee() }, now));


            now = now.Add(_options.Employee.PizzaBakeTime);
            events.Add(CreateEventData("PizzaBaked", new PizzaBakedEvent { PizzaId = pizza.PizzaId, BakedBy = PickRandomEmployee() }, now));
        }

        now = now.Add(_faker.Date.Timespan(_options.Employee.OrderPrepTime));
        events.Add(CreateEventData("OrderPrepared", new OrderPreparedEvent { PreparedBy = PickRandomEmployee() }, now));

        now = now.Add(_faker.Date.Timespan(_options.Employee.DeliveryTime));
        events.Add(CreateEventData("OrderDelivered", new OrderDeliveredEvent { DeliveredBy = PickRandomEmployee() }, now));


        _store.AppendEvents($"order-{orderId.ToString("n")}", events);
    }

    public void PlaceOrder(DateTimeOffset timestamp)
    {
        Guid customerId;
        var now = timestamp;
        var isExistingCustomer = _faker.Random.Bool(_options.Orders.ExistingCustomer);
        if (!isExistingCustomer)
        {
            customerId = RegisterCustomer(now);
            now = now.Add(_faker.Date.Timespan(_options.Customer.RegistrationTime));
        }
        else
        {
            customerId = PickRandomCustomer();
        }

        string type = _faker.PickWeighted(_options.Orders.Type);
        int pizzas = Int32.Parse(_faker.PickWeighted(_options.Orders.Quantity));

        switch (type)
        {
            case "DineIn": 
                PlaceDineInOrder(pizzas, now); 
                break;
            case "Carryout":
                PlaceCarryoutOrder(customerId, pizzas, now);
                break;
            case "Delivery":
                PlaceDeliveryOrder(customerId, pizzas, now);
                break;
        }
    }


    public void OperateDay(DateTimeOffset timestamp)
    {
        var now = new DateTimeOffset(timestamp.Year, timestamp.Month, timestamp.Day, 11, 0, 0, TimeSpan.FromHours(-7));

        DateTimeOffset close = now.AddHours(11);

        while (now < close)
        {
            now = now.Add(_faker.Date.Timespan(TimeSpan.FromMinutes(10)));
            PlaceOrder(now);
        }
    }


    public Guid PickRandomEmployee()
    {
        return _faker.PickRandom(_employees).EmployeeId;
    }

    public Guid PickRandomCustomer()
    {
        return _faker.PickRandom(_customers.Keys.AsEnumerable());
    }

    private EventData CreateEventData(string type, object data, DateTimeOffset timestamp)
    {
        EventData eventData = _fakeEventData
            .RuleFor(x => x.Type, f => type)
            .RuleFor(x => x.Data, f => data);

        eventData.Metadata.CreatedAt = timestamp;

        return eventData;
    }
}