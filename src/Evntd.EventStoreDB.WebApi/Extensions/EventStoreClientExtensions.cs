using EventStore.Client;
using Evntd.EventStoreDB.WebApi.Model;

namespace Evntd.EventStoreDB.WebApi.Extensions
{
    public static class EventStoreClientExtensions
    {
        public static Task<IWriteResult> AppendToStreamAsync(this EventStoreClient esc, AppendToStreamRequest request, CancellationToken cancellationToken)
        {
            if (request.ExpectedRevision != null)
            {
                return esc.AppendToStreamAsync(
                       request.StreamName,
                       request.ExpectedRevision.Value,
                       request.EventData,
                       configureOperationOptions: null,
                       deadline: null,
                       userCredentials: null,
                       cancellationToken);
            }

            return esc.AppendToStreamAsync(
                    request.StreamName,
                    request.ExpectedState.GetValueOrDefault(StreamState.Any),
                    request.EventData,
                    configureOperationOptions: null,
                    deadline: null,
                    userCredentials: null,
                    cancellationToken);
        }

        public static IAsyncEnumerable<ResolvedEvent> ReadStreamAsync(this EventStoreClient esc, ReadStreamRequest request, CancellationToken cancellationToken)
        {
            return esc.ReadStreamAsync(
                        request.Direction,
                        request.StreamName,
                        request.Revision,
                        request.MaxCount,
                        request.ResolveLinkTos,
                        request.Deadline,
                        null,//TODO: Credentials
                        cancellationToken);
        }

        public static IAsyncEnumerable<ResolvedEvent> ReadAllAsync(this EventStoreClient esc, ReadAllRequest request, CancellationToken cancellationToken)
        {
            return esc.ReadAllAsync(
                        request.Direction,
                        request.Position,
                        request.MaxCount,
                        request.ResolveLinkTos,
                        request.Deadline,
                        null,//TODO: Credentials
                        cancellationToken);
        }
    }

}