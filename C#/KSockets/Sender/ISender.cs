//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace KSockets.Sender
{
    public interface ISender
    {
        void Send(String message, Stream stream);
    }
}
