using System;

namespace QueueSender
{
    public class DataAvailableModel
    {
        public string Uuid { get; set; }
        public string Recipient { get; set; }
        public string MessageType { get; set; }
        public string Origin { get; set; }
        public bool SupportsBundling { get; set; }
        public int RelativeWeight { get; set; }

        public static Energinet.DataHub.PostOffice.Contracts.DataAvailable CreateProtoContract(string messageId, string recipient, string origin)
        {
            return new()
            {
                UUID = messageId,
                Recipient = recipient,
                MessageType = "DataAvailable",
                Origin = origin,
                SupportsBundling = false,
                RelativeWeight = 1,
            };
        }
    }
}
