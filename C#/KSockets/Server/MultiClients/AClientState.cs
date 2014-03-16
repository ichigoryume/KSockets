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
    internal abstract class AClientState
    {
        protected Client Client { get; private set; }

        public AClientState(Client client)
        {
            Client = client;
        }

        public virtual void Enter()
        {
        }

        public virtual void Leave()
        {
        }

        public abstract void SendRequested(String message);
        public abstract void CloseRequested();
        public abstract void NotifyReceived(byte[] buff, int receivedSize);
        public abstract void NotifyReceiveError(Exception e);
    }
}
