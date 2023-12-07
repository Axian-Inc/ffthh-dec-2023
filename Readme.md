# Evntd

### Evntd.EventStoreDB.WebApi

C# Minimal WebApi which wraps EventStore.Client.Grpc.Streams to get easy access to an EventStore.



### EventStore DB Cluster

Start the EventStore DB cluster.


```
docker-compose -f ./src/EventStoreDB/docker-compose.yml up -d
```

For Mac user's with ARM use the following
```
docker-compose -f ./src/EventStreoDB/docker-compose-arm.yml up -d
```

This will open localhost ports [1111-1113] to TCP and [2111-2113] to HTTPS.

Go to the [EventStore dashboard](http://localhost:2113) and check it out.

```
# EventStoreDB default credentials
Username: Admin
Password: Changeit
```

#### Enable Projections

1. Go to the [Projections page](http://127.0.0.1:2113/web/index.html#/projections)
2. Click the Enable All button


### HTTP WebApi

First verify that you have a valid .NET development certificate setup.

```
dotnet dev-certs https --check --trust
```

If you don't have a valid certificate. Run the following.
```
dotnet dev-certs https --trust
```

Launch the Evntd.EventStoreDB.WebApi using an IDE or via the command line.
```
dotnet run --project ./src/Evntd.EventStoreDB.WebApi --launch-profile https
```

Open a browser and navigate to the following URI.
```
https://localhost:7177
```

Review the documentation, then try out some manual requests with Insomnia.

Import `/insomnia/Insomnia_2023-11-22.yaml` to get starter requests. Some requests have missing parameter values which you'll need to modify yourself.

⚠️ Disable Certificate Validation
In Insomnia open **Application > Preferences**, on the **General** tab disable **Validate certificates** in the Request/Response section.

### Pizza Delivery

Run the Evntd.Bogus project to populate the EventStore with a bunch of streams to play with.

```
cd ./src/Evntd.Bogus
dotnet run
```

Review the following stream events that are generated.


#### Customer stream events

Stream: `category-{id}`

##### CustomerRegistered
```
{
    "firstName": string
    "lastName": string
    "emailAddress": string
    "phoneNumber": string
    "birthDate": string
    "passwordHash": string
    "passwordSalt": string
}
```

##### AddressDefaulted
```
{
    "label": string
    "street": string
    "city": string
    "state": string
    "zipCode": string
}
```


#### Order stream events

Stream: `order-{id}`

##### CarryoutOrderPlaced
```
{
    "customerId": string,
    "pizzas": [
        "pizzaId": number,
        "size": string,
        "sauce"; string,
        "cheeseQuantity": string,
        "cheeses": Array<string>,
        "meats": Array<string>,
        "vegetables": Array<string>
    ]
}
```

##### DeliveryOrderPlaced
```
{
    "customerId": string,
    "deliveryAddress": {
        "street": string,
        "city": string,
        "state": string,
        "zipCode": string
    },
    "pizzas": [
        "pizzaId": number,
        "size": string,
        "sauce"; string,
        "cheeseQuantity": string,
        "cheeses": Array<string>,
        "meats": Array<string>,
        "vegetables": Array<string>
    ]
}
```

##### DineInOrderPlaced
```
{
    "pizzas": [
        "pizzaId": number,
        "size": string,
        "sauce"; string,
        "cheeseQuantity": string,
        "cheeses": Array<string>,
        "meats": Array<string>,
        "vegetables": Array<string>
    ]
}
```

##### PizzaPrepared
```
{
    "pizzaId": number,
    "preparedBy": string
}
```

##### PizzaBaked
```
{
    "pizzaId": number,
    "bakedBy": string
}
```

##### OrderPrepared
```
{
    "preparedBy": string
}
```

##### OrderReceived
```
{
}
```
##### OrderDelivered
```
{
    "deliveredBy": string
}
```
##### OrderServed
```
{
    "servedBy": string
}
```

### Next...

- Build a read model

    - How many Pepporoni pizzas were sold on any given day?

    - Who are your top customers by visit?

    - Who are your top customers by pizza count?

    - What are the most popular toppings?

    - Which employee delivers the quickest on average?

    - How many oddly topped pizzas are ordered?
        - Hint: You'll need to define "odd" here.

- Explore EventStoreDB projections and create your own.  See [User defined projections](https://developers.eventstore.com/server/v5/projections.html#user-defined-projections)