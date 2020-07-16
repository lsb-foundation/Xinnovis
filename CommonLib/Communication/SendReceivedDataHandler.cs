using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonLib.Communication
{
    public class SendReceivedDataHandler
    {
        private int remainingRetryTime;

        private SendReceivedDataHandlerPair currentPair;

        private System.Timers.Timer timer;

        private Queue<SendReceivedDataHandlerPair> PairQueue = new Queue<SendReceivedDataHandlerPair>();

        private Action<object> Send;

        /// <summary>
        /// Received Handler
        /// </summary>
        public Action<object> ReceivedHandler { get; }

        /// <summary>
        /// Initialize an instance of SendReceivedHandler class.
        /// </summary>
        /// <param name="send">Send Method</param>
        /// <param name="interval">Interval between two sending</param>
        /// <param name="retry">Retry time if failed or time is out</param>
        public SendReceivedDataHandler(Action<object> send, int interval = 500)
        {
            Send = send ?? throw new ArgumentNullException(nameof(send));

            timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = interval
            };
            timer.Elapsed += Timer_Elapsed;

            ReceivedHandler = o => HandleReceivedData(o);
        }

        private void HandleReceivedData(object data)
        {
            if (!timer.Enabled)         //Time is out, application can't handle data.
                return;

            timer.Stop();

            if (!currentPair.ReceivedHandler(data))     //Handle failed.
            {
                Retry();
                return;
            }

            Start();
        }

        /// <summary>
        /// Add Pair List to PairQueue.
        /// </summary>
        /// <param name="pairList"></param>
        public void AddPair(IList<SendReceivedDataHandlerPair> pairList)
        {
            foreach (var pair in pairList)
            {
                PairQueue.Enqueue(pair);
            }
        }

        /// <summary>
        /// Removed all the pairs from PairQueue.
        /// </summary>
        public void RemoveAllPair()
        {
            while (PairQueue?.Count > 0)
                PairQueue.Dequeue();
        }

        /// <summary>
        /// Start send code.
        /// </summary>
        public void Start()
        {
            if (PairQueue?.Count > 0)
            {
                currentPair = PairQueue.Dequeue();
                SendCurrentCode();
                remainingRetryTime = currentPair.RetryTime;
            }
        }

        private void SendCurrentCode()
        {
            Send(currentPair.Data);
            timer.Start();
        }

        private void Retry()
        {
            if (remainingRetryTime > 0)
            {
                Debug.WriteLine($"Failed or timeout, retrying to send.");
                SendCurrentCode();
                remainingRetryTime--;
            }
            else Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Time is out.
            timer.Stop();
            Retry();
        }
    }
}
