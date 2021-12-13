using SignalRThroughput.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SignalRThroughput
{
    public class BufferClass
    {
        private readonly Channel<OutgoingMessage> channelBuffer;

        public BufferClass()
        {
            channelBuffer = Channel.CreateUnbounded<OutgoingMessage>();
        }


        public async Task runAsync(int size)
        {
            int iternationChunkSize = size / 10;
            
            // 1000
            // iternations = 10
            // each iteration = 100

            for (int chunkCounter = 0; chunkCounter < 10; chunkCounter++)
            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < iternationChunkSize; i++)
                {
                    var _out = MessageHelper.CreateOutgoing(i);
                    _out.Block = $"{size} - {i}";
                    await channelBuffer.Writer.WriteAsync(_out);
                }

                var milisec = watch.ElapsedMilliseconds;
                if(milisec > 100)
                {
                    throw new Exception("out of bound value");
                }
                await Task.Delay(100- (int) milisec);
            }
        }

        public ValueTask<OutgoingMessage> ReadAsync()
        {
            return channelBuffer.Reader.ReadAsync();
        }
    }
}
