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
    public class TerminatedMessageSender : ISender
    {
        private String Terminator { get; set; }

        public TerminatedMessageSender(String terminator)
        {
            Terminator = terminator;
        }

        void ISender.Send(string message, System.IO.Stream stream)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message + Terminator);
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }
    }
}
