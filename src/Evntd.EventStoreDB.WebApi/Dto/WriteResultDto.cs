using EventStore.Client;

namespace Evntd.EventStoreDB.WebApi.Dto
{
    public class WriteResultDto
    {
        public string? LogPosition { get; set; }

        public ulong NextExpectedStreamRevision { get; set; }

        public static WriteResultDto FromDomain(IWriteResult writeResult)
        {
            return new WriteResultDto
            {
                LogPosition = writeResult.LogPosition.ToString(),
                NextExpectedStreamRevision = writeResult.NextExpectedStreamRevision
            };
        }
    }
}
