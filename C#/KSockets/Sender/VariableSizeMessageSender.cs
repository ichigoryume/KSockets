//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;

using KSockets.Extensions;

namespace KSockets.Sender
{
    public class VariableSizeMessageSender : ISender
    {
        public VariableSizeMessageSender()
        {
        }

        void ISender.Send(string message, System.IO.Stream stream)
        {
            byte[] payload = message.ToUTF8Array();
            byte[] size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));

            stream.Write(size, 0, 4);
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }
    }
}
