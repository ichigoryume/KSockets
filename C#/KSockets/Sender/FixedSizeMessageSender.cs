//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Sender
{
    public class FixedSizeMessageSender : ISender
    {
        private int MessageSize { get; set; }

        public FixedSizeMessageSender(int messageSize)
        {
            MessageSize = messageSize;
        }

        void ISender.Send(string message, System.IO.Stream stream)
        {
            byte[] payload = new byte[MessageSize];
            Array.Clear(payload, 0, MessageSize);

            byte[] data = Encoding.UTF8.GetBytes(message);
            Array.Copy(data, payload, Math.Min(data.Length, MessageSize));

            stream.Write(payload, 0, MessageSize);
            stream.Flush();
        }
    }
}
