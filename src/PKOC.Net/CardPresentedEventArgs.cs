using System;
using PKOC.Net.MessageData;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PKOC.Net
{
    public class CardPresentedEventArgs : EventArgs
    {
        public Guid ConnectionId { get; }
        
        public byte Address { get; }
        
        public CardPresentResponseData CardPresentResponseData { get; }

        public CardPresentedEventArgs(Guid connectionId, byte address, CardPresentResponseData cardPresentResponseData)
        {
            ConnectionId = connectionId;
            Address = address;
            CardPresentResponseData = cardPresentResponseData;
        }
    }
}