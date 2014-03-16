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
    internal class ClientStateConnected : AClientState
    {
        public ClientStateConnected(Client client) : base(client)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Client.Receiver.Clear();
            Client.ReceiveAsync();
        }

        public override void Leave()
        {
            base.Leave();

            Client.CloseTcpClientAndRemoveFromServerSocket();
        }

        public override void SendRequested(string message)
        {
            Client.Sender.Send(message, Client.TcpClient.GetStream());
        }

        public override void CloseRequested()
        {
            Client.UpdateState(this, new ClientStateDisconnected(Client));
        }

        public override void NotifyReceived(byte[] buff, int receivedSize)
        {
            if (receivedSize == 0)
            {
                Client.DispatchDisconnected();
                Client.UpdateState(this, new ClientStateDisconnected(Client));
                return;
            }

            String[] messages = Client.Receiver.Push(buff, receivedSize);
            foreach (String message in messages)
            {
                Client.DispatchReceived(message);
            }

            Client.ReceiveAsync();
        }

        public override void NotifyReceiveError(Exception e)
        {
            Client.DispatchError(e);
            Client.DispatchDisconnected();
            Client.UpdateState(this, new ClientStateDisconnected(Client));
        }
    }
}
