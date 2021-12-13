using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalRThroughput.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SignalRThroughputClient
{
    public class SignalRCollectorClient
    {
        private readonly String hubUrl;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private readonly HubConnection hubConnection;
        private readonly Stopwatch timer;
        private int i;
        private readonly int requestCount;
        private ConcurrentDictionary<int, List<long>> timeDictionary;
        private ConcurrentDictionary<int, double> StatsList;

        private BufferBlock<OutgoingMessage> messageBuffer;
        private ActionBlock<OutgoingMessage> actionBlock;

        private Timer time;
        private readonly List<OutgoingMessage> minData;
        private int currentSec;
        private bool flag;

        public SignalRCollectorClient(string url, CancellationTokenSource cts, int requestCount)
        {
            this.requestCount = requestCount;
            this.timer = new Stopwatch();
            this.hubUrl = url;
            this.cancellationTokenSource = cts;
            this.cancellationToken = this.cancellationTokenSource.Token;
            this.timeDictionary = new ConcurrentDictionary<int, List<long>>();
            this.StatsList = new ConcurrentDictionary<int, double>();

            this.minData = new List<OutgoingMessage>();
            this.currentSec = 1;
            this.flag = true;

            messageBuffer = new BufferBlock<OutgoingMessage>();
            actionBlock = new ActionBlock<OutgoingMessage>(item =>
            {
                //if (!this.timeDictionary.ContainsKey(item.Section))
                //{
                //    this.timeDictionary.TryAdd(item.Section, new List<long>());
                //}
                //this.timeDictionary[item.Section].Add(item.RecievedTime);
                //Console.WriteLine("Block Item " + item.Section);
                //Console.WriteLine("Block Item " + item.ResponseBag.Items[0].Security);
                //if (item.Key == 199)
                //{
                //    CalculateStat(item.Section);
                //    Console.WriteLine("-----------------------------------------------------");
                //}

            });

            //messageBuffer.LinkTo( actionBlock );

            //this.hubConnection = new HubConnectionBuilder().WithUrl(hubUrl)
            //   .WithAutomaticReconnect()
            //   .Build();

            this.hubConnection = new HubConnectionBuilder()
                        .WithUrl(hubUrl)
                        .WithAutomaticReconnect()
                        .AddMessagePackProtocol()
                        .Build();
            this.InitAsync().GetAwaiter().GetResult();
        }


        public  async Task InitAsync()
        {
            await this.hubConnection.StartAsync(this.cancellationToken);

            if (hubConnection.State == HubConnectionState.Connected)
            {
                hubConnection.On<OutgoingMessage>("OnRecieveMessage", (message) =>
                {
                    Console.WriteLine(message.Version);
                });
            }
            else throw new Exception($"Conneciton info {this.hubConnection.State}");
        }

        //public async Task Execute()
        //{
        //    await hubConnection.SendAsync("InjectData");
        //}


        private void StopTimer()
        {
            this.timer.Stop();
            TimeSpan timeTaken = this.timer.Elapsed;
            string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine(foo);
        }

        public void RecieveUpdates()
        {
            _ = Task.Factory.StartNew(async () =>
            {
                var stream = hubConnection.StreamAsync<OutgoingMessage>("FeedHandler", this.cancellationTokenSource.Token);
                var channel = await hubConnection.StreamAsChannelAsync<OutgoingMessage>("FeedHandler", cancellationToken);
                Console.WriteLine("Feedback Registed");
                while (await channel.WaitToReadAsync())
                {
                    OutgoingMessage message;
                    while (channel.TryRead(out message))
                    {
                        //Console.WriteLine("receiving");
                        //message.RecievedTime = DateTime.Now.Ticks;
                        //this.messageBuffer.Post(message);
                        if (flag )
                        { 
                            this.flag = false;
                            this.time = new Timer(minCount, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
                            Console.WriteLine(".");
                        }
                       // Console.WriteLine(".");
                        this.minData.Add(message);
                        
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        //private void CalculateStat(int section)
        //{
        //    var ticks = this.timeDictionary[section][199] - this.timeDictionary[section][0];
        //    var ms = ticks * 0.0001;
        //    var sec = ms * 0.001;

        //    this.StatsList.TryAdd(section, sec);
        //    Console.WriteLine($"Time in Seconds it took Section-{section} = {sec}");
        //}

        private void minCount(object state)
        {
            Console.WriteLine($"{this.currentSec}--{this.minData.Count}");
            this.minData.Clear();
            currentSec++;
        }

        public void trigger()
        {
            this.hubConnection.InvokeAsync("Run");
        }
    }
}
