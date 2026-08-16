using System;

namespace Whalebone.Gameplay.D3
{
    /// <summary>
    /// D3 路线表现快照。它只把 D3 已经裁决的状态映射给 HUD/测试，不能反向写入经济、航程或存档。
    /// </summary>
    [Serializable]
    public sealed class D3RouteFeedback
    {
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string VoyageStatus { get; private set; }
        public string LocationId { get; private set; }
        public long SaveRevision { get; private set; }
        public int Sequence { get; private set; }

        public D3RouteFeedback(string code, string message, string voyageStatus,
            string locationId, long saveRevision, int sequence)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            VoyageStatus = voyageStatus ?? string.Empty;
            LocationId = locationId ?? string.Empty;
            SaveRevision = saveRevision;
            Sequence = sequence;
        }

        public D3RouteFeedback Clone()
        {
            return new D3RouteFeedback(Code, Message, VoyageStatus, LocationId, SaveRevision, Sequence);
        }
    }
}
