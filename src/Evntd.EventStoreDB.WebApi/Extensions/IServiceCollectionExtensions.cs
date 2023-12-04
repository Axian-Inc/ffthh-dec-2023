using EventStore.Client;
using Grpc.Core;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Evntd.EventStoreDB.WebApi.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreProblemDetails(this IServiceCollection services)
        {
            services
                .AddProblemDetails(options =>
                 {
                     options.IncludeExceptionDetails = (c, e) => false;
                     options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest); // too broad and inaccurate, but good enough for now.
                     options.MapToStatusCode<StreamNotFoundException>(StatusCodes.Status404NotFound);
                     options.MapToStatusCode<WrongExpectedVersionException>(StatusCodes.Status409Conflict);
                     options.MapToStatusCode<InvalidSchemeException>(StatusCodes.Status503ServiceUnavailable);
                     options.Map<RpcException>((c, e) =>
                         new ProblemDetails
                         {
                             Title = nameof(RpcException),
                             Status = StatusCodes.Status503ServiceUnavailable,
                             Detail = "No connection could be made to the EventStore."
                         });

                     options.MapToStatusCode<DiscoveryException>(StatusCodes.Status503ServiceUnavailable);
                     options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
                 })
                .AddControllers()
                .AddProblemDetailsConventions();

            return services;
        }
    }
}
