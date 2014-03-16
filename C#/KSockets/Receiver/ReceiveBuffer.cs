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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using KSockets.Extensions;

namespace KSockets.Receiver
{
    /// <summary>
    /// 受信データを一旦格納するバッファを管理するクラス。
    /// バッファにはMemoryStreamを使用する。バッファ容量は可変。
    /// 
    /// Fixed, VariableSize などの各プロトコルに応じた受信処理から
    /// バッファ管理に関する処理を切り出しクラス化したもの。
    /// 各プロトコル向けに便利なヘルパー関数も提供する。
    /// </summary>
    public class ReceiveBuffer
    {
        /// <summary>
        /// 受信データを格納するストリーム。
        /// Streamは、書き込んだデータを部分的に削除する方法がないのでどんどん肥大化していく。
        /// ので適当なタイミングでDisposeし、新しいStreamと入れ替える。
        /// </summary>
        internal MemoryStream Stream { get; private set; }

        public ReceiveBuffer()
        {
            Stream = new MemoryStream();
        }

        public void Clear()
        {
            Stream.Dispose();
            Stream = new MemoryStream();
        }


        /// <summary>
        /// バッファの末尾にdataを追加。
        /// </summary>
        public void Push(byte[] data, int size)
        {
            // Stream.Positionは常に次の読み込み開始位置を指しておきたいが
            // Streamへの書き込みでPositionは動いてしまうので、
            // 書き込み前に一旦退避して、書き込み後に戻す。
            long position = Stream.Position;
            Stream.Position = Stream.Length;
            Stream.Write(data, 0, size);
            Stream.Position = position;
        }

        /// <summary>
        /// 読み出し可能なデータサイズ。
        /// Streamのバッファサイズではなく、まだGetBytes()で読み込んでない部分のサイズ。
        /// </summary>
        public long Length()
        {
            return Stream.Length - Stream.Position;
        }

        /// <summary>
        /// size分のデータをバッファから読み出す。
        /// コールするたびに Length() で得られる値が変わる（読み込んだ分読み込み開始位置をシークする）。
        /// </summary>
        public byte[] GetBytes(int size)
        {
            byte[] returnBytes = new byte[size];
            Stream.Read(returnBytes, 0, size);

            // バッファ内の全データをこのメソッドで返した -> Dispose()してメモリを解放し、Streamを作り直す
            if (Stream.Position >= Stream.Length)
            {
                Stream.Dispose();
                Stream = new MemoryStream();
            }

            return returnBytes;
        }

        /// <summary>
        /// 現在の読み込み位置から4バイトのデータを int 型として読み込む。
        /// このメソッドのコール前後でLength()のコール結果は変わらない（読み込み開始位置をシークしない）。
        /// </summary>
        public int ReadFirst4BytesAsInt()
        {
            if (Length() < 4)
            {
                return -1;
            }

            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Stream.GetBuffer(), (int)Stream.Position));
        }

        /// <summary>
        /// value で指定したものと同じbyte配列が、まだGetBytes()で返していない領域の何バイト目にあるか検索する。
        /// 見つからない場合は -1 を返す。
        /// </summary>
        public int IndexOf(byte[] value)
        {
            byte[] buffer = Stream.GetBuffer();
            for(int i = (int)Stream.Position; i <= (int)Stream.Length - value.Length; i++)
            {
                if(IsEqual(buffer, i, value))
                {
                    return i - (int)Stream.Position;
                }
            }

            return -1;
        }

        // data1 の data1Offset から data2.Length 分のデータが data2 と全て等しければ True
        private Boolean IsEqual(byte[] data1, long data1Offset, byte[] data2)
        {
            for (int i = 0; i < data2.Length; i++)
            {
                if (data1[data1Offset + i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }


    [TestClass]
    public class ReceiveBufferTest
    {
        [TestMethod]
        public void ClearTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();

            Stream stream = buffer.Stream;
            buffer.Clear();
            Assert.AreNotSame(stream, buffer.Stream);

            stream = buffer.Stream;
            buffer.Push("hoge".ToUTF8Array(), 4);
            Assert.AreSame(stream, buffer.Stream);
            Assert.AreEqual<int>((int)stream.Length, 4);

            buffer.Clear();
            Assert.AreNotSame(stream, buffer.Stream);
            Assert.AreEqual<int>((int)buffer.Stream.Length, 0);
        }

        [TestMethod]
        public void PushTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();

            buffer.Push("foo".ToUTF8Array(), 3);
            Assert.AreEqual<long>(3, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);
            Assert.AreEqual<String>("foo", buffer.Stream.GetBuffer().ToUTF8String(3));

            buffer.Push(Encoding.UTF8.GetBytes("日本語"), 9);
            Assert.AreEqual<long>(12, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);
            Assert.AreEqual<String>("foo日本語", buffer.Stream.GetBuffer().ToUTF8String(12));
        }

        [TestMethod]
        public void LengthTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();
            Assert.AreEqual<long>(0, buffer.Length());
            Assert.AreEqual<long>(0, buffer.Stream.GetBuffer().Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);

            buffer.Push(new byte[4], 4);
            Assert.AreEqual<long>(4, buffer.Length());
            Assert.AreEqual<long>(4, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);

            buffer.Push(new byte[4], 4);
            Assert.AreEqual<long>(8, buffer.Length());
            Assert.AreEqual<long>(8, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);

            buffer.GetBytes(3);
            Assert.AreEqual<long>(5, buffer.Length());
            Assert.AreEqual<long>(8, buffer.Stream.Length);
            Assert.AreEqual<long>(3, buffer.Stream.Position);

            buffer.Push(new byte[4], 4);
            Assert.AreEqual<long>(9, buffer.Length());
            Assert.AreEqual<long>(12, buffer.Stream.Length);
            Assert.AreEqual<long>(3, buffer.Stream.Position);

            buffer.GetBytes(9);
            Assert.AreEqual<long>(0, buffer.Length());
            Assert.AreEqual<long>(0, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);

            buffer.Push(new byte[4], 4);
            Assert.AreEqual<long>(4, buffer.Length());
            Assert.AreEqual<long>(4, buffer.Stream.Length);
            Assert.AreEqual<long>(0, buffer.Stream.Position);
        }

        [TestMethod]
        public void GetBytesTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();

            Stream stream = buffer.Stream;
            buffer.Push("abc".ToUTF8Array(), 3);

            Assert.AreEqual<String>("abc", buffer.GetBytes(3).ToUTF8String());
            Assert.AreNotSame(stream, buffer.Stream);

            stream = buffer.Stream;
            buffer.Push("abcdefghi".ToUTF8Array(), 9);

            Assert.AreEqual<String>("abc", buffer.GetBytes(3).ToUTF8String());
            Assert.AreSame(stream, buffer.Stream);

            Assert.AreEqual<String>("de", buffer.GetBytes(2).ToUTF8String());
            Assert.AreSame(stream, buffer.Stream);

            Assert.AreEqual<String>("fghi", buffer.GetBytes(4).ToUTF8String());
            Assert.AreNotSame(stream, buffer.Stream);
        }

        [TestMethod]
        public void ReadFirst4BytesAsIntTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();
            Assert.AreEqual<int>(-1, buffer.ReadFirst4BytesAsInt());

            buffer.Push(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1)), 4);
            buffer.Push(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(2)), 4);
            buffer.Push(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(3)), 4);
            buffer.Push(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(4)), 4);

            Assert.AreEqual<int>(1, buffer.ReadFirst4BytesAsInt());
            buffer.GetBytes(4);
            Assert.AreEqual<int>(2, buffer.ReadFirst4BytesAsInt());
            buffer.GetBytes(4);
            Assert.AreEqual<int>(3, buffer.ReadFirst4BytesAsInt());
            buffer.GetBytes(4);
            Assert.AreEqual<int>(4, buffer.ReadFirst4BytesAsInt());
            buffer.GetBytes(4);
            Assert.AreEqual<int>(-1, buffer.ReadFirst4BytesAsInt());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();

            buffer.Push("abc日本語abcd".ToUTF8Array(), 16);
            Assert.AreEqual<int>(0, buffer.IndexOf("abc".ToUTF8Array()));
            Assert.AreEqual<int>(1, buffer.IndexOf("bc".ToUTF8Array()));
            Assert.AreEqual<int>(2, buffer.IndexOf("c".ToUTF8Array()));
            Assert.AreEqual<int>(2, buffer.IndexOf("c日本".ToUTF8Array()));
            Assert.AreEqual<int>(12, buffer.IndexOf("abcd".ToUTF8Array()));
            Assert.AreEqual<int>(15, buffer.IndexOf("d".ToUTF8Array()));

            buffer.GetBytes(2);
            Assert.AreEqual<int>(10, buffer.IndexOf("abc".ToUTF8Array()));
            Assert.AreEqual<int>(11, buffer.IndexOf("bc".ToUTF8Array()));
            Assert.AreEqual<int>(0, buffer.IndexOf("c".ToUTF8Array()));
            Assert.AreEqual<int>(0, buffer.IndexOf("c日本".ToUTF8Array()));
            Assert.AreEqual<int>(10, buffer.IndexOf("abcd".ToUTF8Array()));
            Assert.AreEqual<int>(13, buffer.IndexOf("d".ToUTF8Array()));
        }

        [TestMethod]
        public void BigSizeTest()
        {
            ReceiveBuffer buffer = new ReceiveBuffer();

            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);
            buffer.Push(new byte[1024 * 1024], 1024 * 1024);

            buffer.GetBytes(1024 * 1024 * 10);

            Assert.AreEqual<long>(0, buffer.Length());
            Assert.AreEqual<long>(0, buffer.Stream.Length);
        }
    }
}
