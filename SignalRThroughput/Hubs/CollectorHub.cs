using Microsoft.AspNetCore.SignalR;
using SignalRThroughput.Models;
using System;
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
        private readonly BufferBlock<OutgoingMessage> buffer;

        private readonly Timer timer1;
        private readonly Timer timer2;
        private readonly Timer timer3;
        private readonly Timer timer4;


        public CollectorHub()
        {
            channelBuffer = Channel.CreateUnbounded<OutgoingMessage>();
            buffer = new BufferBlock<OutgoingMessage>();

            //int x = 1;
            var stopwatch = Stopwatch.StartNew();

            timer1 = new Timer(InjectData, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2));

            timer2 = new Timer(InjectData, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

            timer3 = new Timer(InjectData, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(3));

            timer4 = new Timer(InjectData, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));



        }

        private void InjectData(object state)
        {

            Console.WriteLine($"Running Block");

            for (int i = 0; i < 10000; i++)
            {
                var _out = MessageHelper.CreateOutgoing(i);

                this.channelBuffer.Writer.WriteAsync(_out);
            }
            Console.WriteLine($"Executed Block");
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
                    Console.WriteLine($"Publishing :  -- {message.Key}");
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
