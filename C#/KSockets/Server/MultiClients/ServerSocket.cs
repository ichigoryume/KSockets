//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

using KSockets.Sender;
using KSockets.Receiver;

namespace KSockets.Server.MultiClients
{
    public class ServerSocket
    {
        /*-------------------------------------------------------------------------------
         * Events
         *-------------------------------------------------------------------------------*/
        public delegate void AcceptedHandler(Client client);
        public delegate void ErrorHandler(String errorMessage);

        public event AcceptedHandler    Accepted;
        public event ErrorHandler       Error;


        /*-------------------------------------------------------------------------------
         * Public Properties
         *-------------------------------------------------------------------------------*/
        public int Port { get; internal set; }
        public IEnumerable<Client> Clients
        {
            get { return ClientList; }
        }


        /*-------------------------------------------------------------------------------
         * Internal Properties
         *-------------------------------------------------------------------------------*/
        internal TcpListener TcpListener { get; private set; }
        internal List<Client> ClientList { get; set; }
        internal Dispatcher Dispatcher { get; set; }


        /*-------------------------------------------------------------------------------
         * Private Properties
         *-------------------------------------------------------------------------------*/
        private AServerSocketState State { get; set; }

        public delegate ISender SenderCreatorDelegate();
        private SenderCreatorDelegate SenderCreator { get; set; }

        public delegate IReceiver ReceiverCreatorDelegate();
        private ReceiverCreatorDelegate ReceiverCreator { get; set; }


        /*-------------------------------------------------------------------------------
         * Creation Methods
         *-------------------------------------------------------------------------------*/
        public static ServerSocket CreateVariableMessageSocket()
        {
            ServerSocket serverSocket = new ServerSocket
            (
                () => { return new VariableSizeMessageSender(); },
                () => { return new VariableSizeMessageReceiver(); }
            );

            return serverSocket;
        }

        public static ServerSocket CreateFixedSizeMessageSocket(int messageSize)
        {
            ServerSocket serverSocket = new ServerSocket
            (
                () => { return new FixedSizeMessageSender(messageSize); },
                () => { return new FixedSizeMessageReceiver(messageSize); }
            );

            return serverSocket;
        }

        public static ServerSocket CreateTerminatedMessageSocket(String terminator)
        {
            ServerSocket serverSocket = new ServerSocket
            (
                () => { return new TerminatedMessageSender(terminator); },
                () => { return new TerminatedMessageReceiver(terminator); }
            );

            return serverSocket;
        }

        public ServerSocket(SenderCreatorDelegate senderCreator, ReceiverCreatorDelegate receiverCreator)
        {
            SenderCreator = senderCreator;
            ReceiverCreator = receiverCreator;

            Dispatcher = Dispatcher.CurrentDispatcher;
            ClientList = new List<Client>();
            State = new ServerSocketStateIdle(this);

            State.Enter();
        }
        
        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public void Start(int port)
        {
            State.StartRequested(port);
        }

        public void Close()
        {
            State.CloseRequested();
        }


        /*-------------------------------------------------------------------------------
         * Internal Methods
         *-------------------------------------------------------------------------------*/
        internal void UpdateState(AServerSocketState requester, AServerSocketState nextState)
        {
            if (State != requester)
            {
                return;
            }

            State.Leave();
            State = nextState;
            State.Enter();
        }

        internal void CreateTcpListener()
        {
            if (TcpListener != null)
            {
                throw new Exception("TcpListener != null");
            }

            TcpListener = new TcpListener(IPAddress.Any, Port);
        }

        internal void CloseTcpListenerAndAllClients()
        {
            if (TcpListener != null)
            {
                TcpListener.Stop();
                TcpListener = null;
            }

            Client[] clients = ClientList.ToArray(); // イテレーション中に要素が削除されてもいいようにコピーを
            foreach (Client client in clients)
            {
                client.Close(); // Client側でServer.ClientListから削除する
            }
        }

        internal async void AcceptAsync()
        {
            try
            {
                TcpListener.Start();
                TcpClient tcpClient = await TcpListener.AcceptTcpClientAsync();
                State.NotifyAccepted(tcpClient);
            }
            catch (Exception e)
            {
                State.NotifyAcceptError(e);
            }
        }

        // event dispatch method 群
        internal void DispatchAccepted(TcpClient tcpClient)
        {
            Client client = new Client(this, tcpClient, SenderCreator.Invoke(), ReceiverCreator.Invoke());
            ClientList.Add(client);

            Dispatcher.BeginInvoke(new Action(() => OnAccepted(client)), null);
        }

        internal void DispatchError(Exception e)
        {
            Dispatcher.BeginInvoke(new Action(() => OnError(e.Message)), null);
        }


        /*-------------------------------------------------------------------------------
         * Private Methods
         *-------------------------------------------------------------------------------*/

        private void OnAccepted(Client client)
        {
            if (Accepted == null)
            {
                return;
            }

            try
            {
                Accepted(client);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnError(String message)
        {
            if (Error == null)
            {
            }

            try
            {
                Error(message);
            }
            catch
            {
            }
        }

    }
}
