using Microsoft.AspNetCore.SignalR;
using SignalRThroughput.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SignalRThroughput.Hubs
{
    public class CollectorHub : Hub
    {
        private BufferClass bufferClass;

        public CollectorHub(BufferClass bufferClass)
        {
            this.bufferClass = bufferClass;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("--> Connection Established " + Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ReceivedConnID", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Disconnected - {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public Task<ChannelReader<OutgoingMessage>> FeedHandler(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<OutgoingMessage>(new UnboundedChannelOptions { SingleWriter = true });
            _ = WriteItemsAsync(channel.Writer, cancellationToken);
            return Task.FromResult(channel.Reader);
        }
        /* does nothing
        public void Run()
        {
            Task.Run(async () =>
            {
                await bufferClass.runAsync(1000);
            }).Wait();
        }

        */

        private async Task WriteItemsAsync(ChannelWriter<OutgoingMessage> writer, CancellationToken cancellationToken)
        {
            Exception localException = null;
            int size = 2000;
            long maxDelta = 0;

            try
            {

                // 1000
                // iternations = 10
                // each iteration = 100
                OutgoingMessage _out = null;
                var watch = Stopwatch.StartNew();
                long projectedMS = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    int iternationChunkSize = size / 10;

                    for (int chunkCounter = 0; chunkCounter < 10; chunkCounter++)
                    {
                        for (int i = 0; i < iternationChunkSize; i++)
                        {
                            _out = MessageHelper.CreateOutgoing(i);
                            _out.Block = $"{size} - {i}";
                            await writer.WriteAsync(_out);
                        }

                        var milisec = watch.ElapsedMilliseconds;
                        var delta = milisec - projectedMS;
                        maxDelta = Math.Max(delta, maxDelta);

                        projectedMS += 100;

                        if (projectedMS % 2000 == 0)
                        {
                            Console.WriteLine($"Publishing : {_out.Block} -- {_out.Key} , publish ms:{delta}  , max delta ms:{maxDelta}");
                        }
                        if (projectedMS < milisec)
                        {
                            Console.WriteLine($"!!!!Publishing locked up,  publish ms:{delta}");
                            //   throw new Exception("Publishing locked up");
                        }
                        else await Task.Delay((int)(projectedMS - milisec));

                    }
                    //Console.WriteLine($"Publishing : {message.Block} -- {message.Key}");
                }
            }
            catch (Exception ex)
            {
                localException = ex;
                Console.WriteLine($"Publisher exception : {ex}");
            }
            finally
            {
                writer.Complete(localException);
            }
        }
    }
}
