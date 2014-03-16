//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using KSockets.Extensions;
using KSockets.Sender;

namespace KSockets.Receiver
{
    public class VariableSizeMessageReceiver : IReceiver
    {
        private ReceiveBuffer ReceiveBuffer { get; set; }

        public VariableSizeMessageReceiver()
        {
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
            while (ReceiveBuffer.Length() >= 4)
            {
                int messageSize = ReceiveBuffer.ReadFirst4BytesAsInt();
                if (ReceiveBuffer.Length() < 4 + messageSize)
                {
                    break;
                }

                String message = Encoding.UTF8.GetString(ReceiveBuffer.GetBytes(messageSize + 4), 4, messageSize);
                messages.Add(message);
            }

            return messages.ToArray();
        }
    }

    [TestClass]
    public class VariableSizeMessageReceiverTest
    {
        [TestMethod]
        public void Push1Message()
        {
            {
                IReceiver receiver = new VariableSizeMessageReceiver();

                byte[] buff = CreatePayload("hoge");
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual<String>("hoge", messages[0]);
            }

            {
                IReceiver receiver = new VariableSizeMessageReceiver();

                byte[] buff = CreatePayload("日本語hoge");
                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual<String>("日本語hoge", messages[0]);
            }

        }

        [TestMethod]
        public void PushEmptyMessage()
        {
            IReceiver receiver = new VariableSizeMessageReceiver();

            byte[] buff = CreatePayload("");
            String[] messages = receiver.Push(buff, buff.Length);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("", messages[0]);
        }

        [TestMethod]
        public void Push2Messages()
        {
            {
                IReceiver receiver = new VariableSizeMessageReceiver();

                byte[] buff1 = CreatePayload("１２３");
                byte[] buff2 = CreatePayload("日本語");
                byte[] buff = new byte[buff1.Length + buff2.Length];
                buff1.CopyTo(buff, 0);
                buff2.CopyTo(buff, buff1.Length);

                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(2, messages.Length);
                Assert.AreEqual<String>("１２３", messages[0]);
                Assert.AreEqual<String>("日本語", messages[1]);
            }

            {
                IReceiver receiver = new VariableSizeMessageReceiver();

                byte[] buff1 = CreatePayload("");
                byte[] buff2 = CreatePayload("");
                byte[] buff = new byte[buff1.Length + buff2.Length];
                buff1.CopyTo(buff, 0);
                buff2.CopyTo(buff, buff1.Length);

                String[] messages = receiver.Push(buff, buff.Length);
                Assert.AreEqual<int>(2, messages.Length);
                Assert.AreEqual<String>("", messages[0]);
                Assert.AreEqual<String>("", messages[1]);
            }
        }

        [TestMethod]
        public void Push1MessageDividedInto2Fragments()
        {
            // divided at message size area
            {
                IReceiver receiver = new VariableSizeMessageReceiver();
                byte[] buff = CreatePayload("hoge");

                byte[] buff1 = new byte[2];
                Array.Copy(buff, buff1, 2);
                String[] messages = receiver.Push(buff1, buff1.Length);
                Assert.AreEqual<int>(0, messages.Length);

                byte[] buff2 = new byte[6];
                Array.Copy(buff, 2, buff2, 0, 6);
                messages = receiver.Push(buff2, buff2.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual("hoge", messages[0]);
            }

            // divided between message size and message
            {
                IReceiver receiver = new VariableSizeMessageReceiver();
                byte[] buff = CreatePayload("hoge");

                byte[] buff1 = new byte[4];
                Array.Copy(buff, buff1, 4);
                String[] messages = receiver.Push(buff1, buff1.Length);
                Assert.AreEqual<int>(0, messages.Length);

                byte[] buff2 = new byte[4];
                Array.Copy(buff, 4, buff2, 0, 4);
                messages = receiver.Push(buff2, buff2.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual("hoge", messages[0]);
            }

            // divided at message area
            {
                IReceiver receiver = new VariableSizeMessageReceiver();
                byte[] buff = CreatePayload("hoge");

                byte[] buff1 = new byte[6];
                Array.Copy(buff, buff1, 6);
                String[] messages = receiver.Push(buff1, buff1.Length);
                Assert.AreEqual<int>(0, messages.Length);

                byte[] buff2 = new byte[2];
                Array.Copy(buff, 6, buff2, 0, 2);
                messages = receiver.Push(buff2, buff2.Length);
                Assert.AreEqual<int>(1, messages.Length);
                Assert.AreEqual("hoge", messages[0]);
            }
        }

        [TestMethod]
        public void Push3MessagesDividedInto4Fragments()
        {
            IReceiver receiver = new VariableSizeMessageReceiver();
            byte[] message1 = CreatePayload("hoge");
            byte[] message2 = CreatePayload("foo");
            byte[] message3 = CreatePayload("ho");
            byte[] allMessage = new byte[21];
            Array.Copy(message1, 0, allMessage, 0, 8);
            Array.Copy(message2, 0, allMessage, 8, 7);
            Array.Copy(message3, 0, allMessage, 15, 6);

            byte[] fragment1 = new byte[7];
            byte[] fragment2 = new byte[7];
            byte[] fragment3 = new byte[7];

            Array.Copy(allMessage, 0, fragment1, 0, 7);
            Array.Copy(allMessage, 7, fragment2, 0, 7);
            Array.Copy(allMessage, 14, fragment3, 0, 7);

            String[] messages = receiver.Push(fragment1, fragment1.Length);
            Assert.AreEqual<int>(messages.Length, 0);

            messages = receiver.Push(fragment2, fragment2.Length);
            Assert.AreEqual<int>(messages.Length, 1);
            Assert.AreEqual<String>("hoge", messages[0]);

            messages = receiver.Push(fragment3, fragment3.Length);
            Assert.AreEqual<int>(messages.Length, 2);
            Assert.AreEqual<String>("foo", messages[0]);
            Assert.AreEqual<String>("ho", messages[1]);
        }

        private byte[] CreatePayload(String message)
        {
            ISender sender = new VariableSizeMessageSender();
            using (MemoryStream stream = new MemoryStream())
            {
                sender.Send(message, stream);

                byte[] result = new byte[stream.Length];
                Array.Copy(stream.GetBuffer(), result, stream.Length);

                return result;
            }
        }
    }
}
