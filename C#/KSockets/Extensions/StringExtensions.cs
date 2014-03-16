//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToUTF8Array(this String value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] ToUTF8Array(this String message, int size)
        {
            byte[] buff = new byte[size];
            byte[] messageBuff = message.ToUTF8Array();
            Array.Copy(messageBuff, buff, Math.Min(size, messageBuff.Length));

            return buff;
        }

    }
}
