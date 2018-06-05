﻿using System;
using System.Linq;
using System.Threading;
using static Secs4Net.Item;

namespace Secs4Net
{
    public sealed class PrimaryMessageWrapper : IDisposable
    {
        private int _isReplied = 0;
        private readonly WeakReference<SecsGem> _secsGem;
        private MessageHeader _header;
        public SecsMessage Message { get; }
        public int MessageId => _header.SystemBytes;

        internal PrimaryMessageWrapper(SecsGem secsGem, MessageHeader header, SecsMessage msg)
        {
            _secsGem = new WeakReference<SecsGem>(secsGem);
            _header = header;
            Message = msg;
        }

        /// <summary>
        /// Each PrimaryMessageWrapper can invoke Reply method once.
        /// Since message replied, method return false.
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="autoDispose">auto disposes <paramref name="replyMessage"/></param>
        /// <returns>ture, if reply message sent.</returns>
        public bool Reply(SecsMessage replyMessage, bool autoDispose = true)
        {
            SecsGem secsGem;
            if (Interlocked.Exchange(ref _isReplied, 1) == 1
                || !Message.ReplyExpected
                || !_secsGem.TryGetTarget(out secsGem))
            {
                if (autoDispose)
                    replyMessage.Dispose();

                return false;
            }

            if (replyMessage == null)
            {
                replyMessage = new SecsMessage(9, 7, false, "Unknown Message", B(_header.EncodeTo(new byte[10])));
            }
            else
            {
                replyMessage.ReplyExpected = false;
            }

            secsGem.SendDataMessageAsync(replyMessage,
                replyMessage.S == 9 ? secsGem.NewSystemId : _header.SystemBytes, autoDispose);

            return true;
        }

        ~PrimaryMessageWrapper()
        {
            Message.Dispose();
        }

        public override string ToString() => Message.ToString();
        public void Dispose() => Message.Dispose();
    }
}
