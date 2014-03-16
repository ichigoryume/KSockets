//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Receiver
{
    public interface IReceiver
    {
        void Clear();
        String[] Push(Byte[] buffer, int receivedBytes);
    }
}
