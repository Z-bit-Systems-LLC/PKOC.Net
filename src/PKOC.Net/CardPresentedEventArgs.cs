using System;
using PKOC.Net.MessageData;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PKOC.Net
{
    /// <summary>
    /// Event arguments containing information about a card that has been presented to the PKOC control panel.
    /// </summary>
    public class CardPresentedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="CardPresentedEventArgs"/> class.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="address">The address of the device.</param>
        /// <param name="cardPresentResponseData"></param>
        public CardPresentedEventArgs(Guid connectionId, byte address, CardPresentResponseData cardPresentResponseData)
        {
            ConnectionId = connectionId;
            Address = address;
            CardPresentResponseData = cardPresentResponseData;
        }
        
        /// <summary>
        /// Gets the unique identifier for the connection.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets the address of the device.
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// Represents a response data for a card present transaction.
        /// </summary>
        public CardPresentResponseData CardPresentResponseData { get; }
    }
}