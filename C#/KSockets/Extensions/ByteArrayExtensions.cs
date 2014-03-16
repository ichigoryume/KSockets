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
    public static class ByteArrayExtensions
    {
        public static String ToUTF8String(this byte[] array)
        {
            return Encoding.UTF8.GetString(array, 0, array.Length);
        }

        public static String ToUTF8String(this byte[] array, int size)
        {
            return Encoding.UTF8.GetString(array, 0, size);
        }
    }
}
