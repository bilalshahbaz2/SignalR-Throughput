using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRThroughput
{
    public interface IBufferClass
    {
        Task ReadAsync();
    }
}
