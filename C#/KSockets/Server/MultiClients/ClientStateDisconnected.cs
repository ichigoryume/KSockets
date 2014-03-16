//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Server.MultiClients
{
    internal class ClientStateDisconnected : AClientState
    {
        public ClientStateDisconnected(Client client)
            : base(client)
        {
        }

        public override void SendRequested(string message)
        {
        }

        public override void CloseRequested()
        {
        }

        public override void NotifyReceived(byte[] buff, int receivedSize)
        {
        }

        public override void NotifyReceiveError(Exception e)
        {
        }
    }
}
