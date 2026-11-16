// AI-FORK: quote-first map-trader contracts. Placeholder routes had no working write contract.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class TradeLineRequest
    {
        public int ThingId { get; set; }
        public int Count { get; set; }
    }

    public class TradeQuoteRequest
    {
        public int MapId { get; set; }
        public int TraderPawnId { get; set; }
        public string TraderShipId { get; set; }
        public int? TraderWorldObjectId { get; set; }
        public int? CaravanId { get; set; }
        public int? CommsConsoleId { get; set; }
        public string NegotiatorPawnId { get; set; }
        public List<TradeLineRequest> Buy { get; set; }
        public List<TradeLineRequest> Sell { get; set; }
        public bool DryRun { get; set; }
        public bool GiftMode { get; set; }
    }

    public class TradeGiftRequest
    {
        public string Action { get; set; }
        public int MapId { get; set; }
        public int TraderPawnId { get; set; }
        public string NegotiatorPawnId { get; set; }
        public List<TradeLineRequest> Sell { get; set; }
        public List<TradeLineRequest> Buy { get; set; }
        public string QuoteId { get; set; }
        public string RequestId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class TradeExecuteRequest
    {
        public string QuoteId { get; set; }
        public string RequestId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}

// AI-FORK: C-38 faction comms (Building_CommsConsole -> FactionDialogMaker).
public sealed class FactionCommsRequest
{
    public int MapId { get; set; }
    public string FactionId { get; set; }
    public string NegotiatorPawnId { get; set; }
    public int CommsConsoleId { get; set; }
    /// <summary>open_job, trader, orbital_trader, military_aid or ai_core.</summary>
    public string RequestType { get; set; }
    public string TraderKind { get; set; }
    public bool Confirm { get; set; }
    public bool DryRun { get; set; }
}
