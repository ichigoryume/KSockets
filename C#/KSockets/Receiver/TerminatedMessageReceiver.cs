//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using KSockets.Extensions;

namespace KSockets.Receiver
{
    public class TerminatedMessageReceiver : IReceiver
    {
        private byte[] Terminator { get; set; }
        private ReceiveBuffer ReceiveBuffer { get; set; }

        public TerminatedMessageReceiver(String terminator)
        {
            Terminator = Encoding.UTF8.GetBytes(terminator);
            ReceiveBuffer = new ReceiveBuffer();
        }

        void IReceiver.Clear()
        {
            ReceiveBuffer.Clear();
        }

        string[] IReceiver.Push(byte[] buffer, int receivedBytes)
        {
            ReceiveBuffer.Push(buffer, receivedBytes);

            List<String> messages = new List<string>();
            while (ReceiveBuffer.Length() >= Terminator.Length)
            {
                int terminatorIndex = ReceiveBuffer.IndexOf(Terminator);
                if (terminatorIndex < 0)
                {
                    break;
                }

                String message = Encoding.UTF8.GetString(ReceiveBuffer.GetBytes(terminatorIndex + Terminator.Length), 0, terminatorIndex);
                messages.Add(message);
            }

            return messages.ToArray();
        }

    }

    [TestClass]
    public class TerminatedMessageReceiverTest
    {
        [TestMethod]
        public void Push1Message()
        {
            {
                IReceiver receiver = new TerminatedMessageReceiver(Environment.NewLine);

                byte[] buff = Encoding.UTF8.GetBytes("hoge" + Environment.NewLine);
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual<String>("hoge", messages[0]);
            }

            {
                IReceiver receiver = new TerminatedMessageReceiver("終端");

                byte[] buff = Encoding.UTF8.GetBytes("hoge終端");
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual<String>("hoge", messages[0]);
            }

            {
                IReceiver receiver = new TerminatedMessageReceiver("\n");

                byte[] buff = Encoding.UTF8.GetBytes("メッセージ\n");
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual<String>("メッセージ", messages[0]);
            }
        }

        [TestMethod]
        public void PushEmptyMessage()
        {
            IReceiver receiver = new TerminatedMessageReceiver("\r");

            byte[] buff = Encoding.UTF8.GetBytes("\r");
            String[] messages = receiver.Push(buff, buff.Length);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("", messages[0]);
        }

        [TestMethod]
        public void Push1JPMessage()
        {
            IReceiver receiver = new TerminatedMessageReceiver("\n");
            byte[] buff = "日本語\n".ToUTF8Array();
            String[] messages = receiver.Push(buff, buff.Length);
            Assert.AreEqual<int>(messages.Length, 1);
            Assert.AreEqual(messages[0], "日本語");
        }

        [TestMethod]
        public void Push2Messages()
        {
            {
                IReceiver receiver = new TerminatedMessageReceiver("\n");
                byte[] buff = "１２３\n字\n".ToUTF8Array();
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(2, messages.Length);
                Assert.AreEqual<String>("１２３", messages[0]);
                Assert.AreEqual<String>("字", messages[1]);
            }

            {
                IReceiver receiver = new TerminatedMessageReceiver("\n");
                byte[] buff = "\n\n".ToUTF8Array();
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(2, messages.Length);
                Assert.AreEqual<String>("", messages[0]);
                Assert.AreEqual<String>("", messages[1]);
            }
        }

        [TestMethod]
        public void Push1MessageDividedInto2Fragments()
        {
            IReceiver receiver = new TerminatedMessageReceiver("\n");

            String[] messages = receiver.Push("ho".ToUTF8Array(), 2);
            Assert.AreEqual<int>(0, messages.Length);

            messages = receiver.Push("ge\n".ToUTF8Array(), 3);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual("hoge", messages[0]);
        }

        [TestMethod]
        public void Push3MessagesDividedInto4Fragments()
        {
            IReceiver receiver = new TerminatedMessageReceiver("\n");

            String[] messages = receiver.Push("fo".ToUTF8Array(), 2);
            Assert.AreEqual<int>(0, messages.Length);

            byte[] buff = "o\nhoge\n1".ToUTF8Array();
            messages = receiver.Push(buff, buff.Length);
            Assert.AreEqual<int>(2, messages.Length);
            Assert.AreEqual<String>("foo", messages[0]);
            Assert.AreEqual<String>("hoge", messages[1]);

            messages = receiver.Push("2345\n".ToUTF8Array(), 5);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("12345", messages[0]);
        }
    }
}
