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
    /// <summary>
    /// 固定長メッセージ受信クラス
    /// 
    /// 固定長メッセージの受信におけるバッファ処理を行う。
    /// バッファにはReceiveBufferを使用する。
    /// </summary>
    public class FixedSizeMessageReceiver : IReceiver
    {
        private int MessageSize { get; set; }
        private ReceiveBuffer ReceiveBuffer { get; set; }

        public FixedSizeMessageReceiver(int messageSize)
        {
            MessageSize = messageSize;
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
            while (ReceiveBuffer.Length() >= MessageSize)
            {
                byte[] buff = ReceiveBuffer.GetBytes(MessageSize);
                String message = Encoding.UTF8.GetString(buff, 0, lastNonNullCharIndex(buff));
                messages.Add(message);
            }

            return messages.ToArray();
        }

        private int lastNonNullCharIndex(byte[] buff)
        {
            for (int i = buff.Length; i > 0; i--)
            {
                if (buff[i - 1] != (byte)0)
                {
                    return i;
                }
            }
            return 0;
        }
    }

    [TestClass]
    public class FixedSizeMessageReceiverTest
    {
        [TestMethod]
        public void Push1Message()
        {
            // check case of 32 bytes
            {
                IReceiver receiver = new FixedSizeMessageReceiver(32);

                String[] messages = receiver.Push("hoge".ToUTF8Array(32), 32);
                Assert.AreEqual<int>(messages.Length, 1);
                Assert.AreEqual(messages[0], "hoge");

                messages = receiver.Push("12345678901234567890123456789012".ToUTF8Array(32), 32);
                Assert.AreEqual<int>(messages.Length, 1);
                Assert.AreEqual(messages[0], "12345678901234567890123456789012");
            }

            // check case of 16 bytes
            {
                IReceiver receiver = new FixedSizeMessageReceiver(16);
                String[] messages = receiver.Push("hoge".ToUTF8Array(16), 16);
                Assert.AreEqual<int>(messages.Length, 1);
                Assert.AreEqual(messages[0], "hoge");

                messages = receiver.Push("1234567890123456".ToUTF8Array(16), 16);
                Assert.AreEqual<int>(messages.Length, 1);
                Assert.AreEqual(messages[0], "1234567890123456");
            }

            // check case of 1 bytes
            {
                IReceiver receiver = new FixedSizeMessageReceiver(1);
                String[] messages = receiver.Push("h".ToUTF8Array(1), 1);
                Assert.AreEqual<int>(messages.Length, 1);
                Assert.AreEqual(messages[0], "h");
            }
        }

        [TestMethod]
        public void PushEmptyMessage()
        {
            IReceiver receiver = new FixedSizeMessageReceiver(8);
            byte[] buff = new byte[8];
            Array.Clear(buff, 0, buff.Length);

            String[] messages = receiver.Push(buff, 8);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("", messages[0]);
        }

        [TestMethod]
        public void Push1JPMessage()
        {
            // check case of 16 bytes
            IReceiver receiver = new FixedSizeMessageReceiver(24);
            String[] messages = receiver.Push("日本語".ToUTF8Array(24), 24);
            Assert.AreEqual<int>(messages.Length, 1);
            Assert.AreEqual(messages[0], "日本語");

            byte[] messageBuff = "１２３４５６７８".ToUTF8Array(24);

            messages = receiver.Push(messageBuff, 24);
            Assert.AreEqual<int>(messages.Length, 1);
            Assert.AreEqual(messages[0], "１２３４５６７８");
        }

        [TestMethod]
        public void Push2Messages()
        {
            // check case of 3 bytes
            {
                IReceiver receiver = new FixedSizeMessageReceiver(3);
                String[] messages = receiver.Push("123字".ToUTF8Array(6), 6);

                Assert.AreEqual<int>(2, messages.Length);
                Assert.AreEqual<String>("123", messages[0]);
                Assert.AreEqual<String>("字", messages[1]);
            }

            // check case of 1 bytes
            {
                IReceiver receiver = new FixedSizeMessageReceiver(1);
                String[] messages = receiver.Push("123".ToUTF8Array(3), 3);

                Assert.AreEqual<int>(3, messages.Length);
                Assert.AreEqual<String>("1", messages[0]);
                Assert.AreEqual<String>("2", messages[1]);
                Assert.AreEqual<String>("3", messages[2]);
            }

            {
                IReceiver receiver = new FixedSizeMessageReceiver(1);
                byte[] buff = new byte[3];
                String[] messages = receiver.Push(buff, 3);

                Assert.AreEqual<int>(3, messages.Length);
                Assert.AreEqual<String>("", messages[0]);
                Assert.AreEqual<String>("", messages[1]);
                Assert.AreEqual<String>("", messages[2]);
            }
        }

        [TestMethod]
        public void Push1MessagesDividedInto2Fragments()
        {
            IReceiver receiver = new FixedSizeMessageReceiver(4);

            String[] messages = receiver.Push("ho".ToUTF8Array(), 2);
            Assert.AreEqual<int>(0, messages.Length);

            messages = receiver.Push("ge".ToUTF8Array(), 2);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("hoge", messages[0]);
        }

        [TestMethod]
        public void Push3MessagesDividedInto4Fragments()
        {
            IReceiver receiver = new FixedSizeMessageReceiver(3);

            String[] messages = receiver.Push("fo".ToUTF8Array(), 2);
            Assert.AreEqual<int>(0, messages.Length);

            messages = receiver.Push("obarb".ToUTF8Array(), 5);
            Assert.AreEqual<int>(2, messages.Length);
            Assert.AreEqual<String>("foo", messages[0]);
            Assert.AreEqual<String>("bar", messages[1]);

            messages = receiver.Push("a".ToUTF8Array(), 1);
            Assert.AreEqual<int>(0, messages.Length);

            messages = receiver.Push("z".ToUTF8Array(), 1);
            Assert.AreEqual<int>(1, messages.Length);
            Assert.AreEqual<String>("baz", messages[0]);
        }

    }
}
