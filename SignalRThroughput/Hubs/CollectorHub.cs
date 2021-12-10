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
        private readonly Channel<OutgoingMessage> channelBuffer;

        private Timer timer;
        private readonly List<int> arr;
        private int i;


        private static int Interval = 1;
        private Stopwatch watch;



        public CollectorHub()
        {
            channelBuffer = Channel.CreateUnbounded<OutgoingMessage>();

            this.arr = new List<int>() { 1000, 2000, 3000, 4000, 5000 };
            this.i = 0;
            this.watch = new Stopwatch();
            this.timer = new Timer(Dump, null, TimeSpan.FromSeconds(0), TimeSpan.FromHours(1));
        }

        private async void Dump(object state)
        {
            //if(Interval <= 10)
            //{

            //    for (int q = 0; q < this.arr[0] / 10; q++)
            //    {
            //        var _out = MessageHelper.CreateOutgoing(q);
            //        //_out.Block = this.arr[q];
            //        Console.WriteLine(q);

            //        await this.channelBuffer.Writer.WriteAsync(_out);

            //    }

            //    Interval++;
            //}
            //else
            //{
            //    Interval = 1;

            //    Console.WriteLine("Waiting for 2 sec...");
            //    Thread.Sleep(2000);

            //}


            var count = this.arr.Count;
            for(int p=0; p<count; p++)
            {
                var interval = this.arr[p] / 10;
                for(int x=1; x<=10; x++)
                {
                    int q = 1;
                    watch.Restart();
                    while(watch.ElapsedMilliseconds <= 100 && q <= interval)
                    {
                        var _out = MessageHelper.CreateOutgoing(q);
                        await this.channelBuffer.Writer.WriteAsync(_out);
                        q++;
                    }
                }
                watch.Restart();
                while (watch.Elapsed.TotalSeconds <= 5)
                {

                }
            }

        }


        private void InjectData(int p)
        {
            Console.WriteLine("Running Block");

            var interval = this.arr[p] / 10;
            int x = 1;
            
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("--> Connection Established " + Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ReceivedConnID", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public Task<ChannelReader<OutgoingMessage>> FeedHandler(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<OutgoingMessage>(new UnboundedChannelOptions { SingleWriter = true });
            _ = WriteItemsAsync(channel.Writer, cancellationToken);
            return Task.FromResult(channel.Reader);
        }



        private async Task WriteItemsAsync(ChannelWriter<OutgoingMessage> writer, CancellationToken cancellationToken)
        {
            Exception localException = null;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await this.channelBuffer.Reader.ReadAsync();

                    await writer.WriteAsync(message, cancellationToken);
                    Console.WriteLine($"Publishing : {message.Block} -- {message.Key}");
                }
            }
            catch (Exception ex)
            {
                localException = ex;
            }
            finally
            {
                writer.Complete(localException);
            }
        }
    }
}
