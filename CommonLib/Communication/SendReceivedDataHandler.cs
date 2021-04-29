using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonLib.Communication
{
    public class SendReceivedDataHandler
    {
        private int remainingRetryTime;

        private SendReceivedDataHandlerPair currentPair;

        private readonly System.Timers.Timer _timer;

        private readonly Queue<SendReceivedDataHandlerPair> _pairQueue = new Queue<SendReceivedDataHandlerPair>();

        private readonly Action<object> _send;

        /// <summary>
        /// Received Handler
        /// </summary>
        public Action<object> ReceivedHandler { get; }
        public event Action WaitTimeout;

        /// <summary>
        /// Initialize an instance of SendReceivedHandler class.
        /// </summary>
        /// <param name="send">Send Method</param>
        /// <param name="interval">Interval between two sending</param>
        /// <param name="retry">Retry time if failed or time is out</param>
        public SendReceivedDataHandler(Action<object> send, int interval = 500)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));

            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = interval
            };
            _timer.Elapsed += Timer_Elapsed;

            ReceivedHandler = o => HandleReceivedData(o);
        }

        private void HandleReceivedData(object data)
        {
            if (!_timer.Enabled)         //Time is out, application can't handle data.
            {
                WaitTimeout?.Invoke();
                return;
            }

            _timer.Stop();

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
                _pairQueue.Enqueue(pair);
            }
        }

        /// <summary>
        /// Removed all the pairs from PairQueue.
        /// </summary>
        public void RemoveAllPair()
        {
            while (_pairQueue?.Count > 0)
                _pairQueue.Dequeue();
        }

        /// <summary>
        /// Start send code.
        /// </summary>
        public void Start()
        {
            if (_pairQueue?.Count > 0)
            {
                currentPair = _pairQueue.Dequeue();
                SendCurrentCode();
                remainingRetryTime = currentPair.RetryTime;
            }
        }

        private void SendCurrentCode()
        {
            _send(currentPair.Data);
            _timer.Start();
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
            _timer.Stop();
            Retry();
        }
    }
}
