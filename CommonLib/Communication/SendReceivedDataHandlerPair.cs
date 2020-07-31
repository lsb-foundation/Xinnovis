using System;

namespace CommonLib.Communication
{
    public class SendReceivedDataHandlerPair
    {
        /// <summary>
        /// Initialize an instance of SendReceivedHandlerPair class.
        /// </summary>
        /// <param name="data">Data that will be sended</param>
        /// <param name="receivedHandler">Handler when data is received</param>
        /// <param name="retryTime">Retry times</param>
        public SendReceivedDataHandlerPair(object data, Predicate<object> receivedHandler, int retryTime = 0)
        {
            if (data == null || receivedHandler == null)
                throw new ArgumentNullException();

            if (retryTime < 0)
                throw new Exception($"{nameof(retryTime)} must be greater than 0 or equals 0.");

            Data = data;
            ReceivedHandler = receivedHandler;
            RetryTime = retryTime;
        }

        /// <summary>
        /// Data that will be sended.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Handler when data is received, returns true if data is correct or returns false.
        /// </summary>
        public Predicate<object> ReceivedHandler { get; set; }

        /// <summary>
        /// Retry times if resolve received data failed or time is out.
        /// </summary>
        public int RetryTime { get; set; }
    }
}
