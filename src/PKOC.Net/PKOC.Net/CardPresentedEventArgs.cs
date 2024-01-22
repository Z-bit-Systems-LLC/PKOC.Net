using System;
using PKOC.Net.MessageData;

namespace PKOC.Net
{
    public class CardPresentedEventArgs : EventArgs
    {
        public CardPresentResponseData CardPresentResponseData { get; }

        public CardPresentedEventArgs(CardPresentResponseData cardPresentResponseData)
        {
            CardPresentResponseData = cardPresentResponseData;
        }
    }
}