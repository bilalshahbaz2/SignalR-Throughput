using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRThroughputClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press Key when server is up ");
            Console.ReadLine();

            var signalRHub = new SignalRCollectorClient("http://localhost:5000/hubs/collector", new System.Threading.CancellationTokenSource(), 200);
           // await signalRHub.InitAsync();
            await Task.Delay(200);
            //await signalRHub.Execute();
            signalRHub.RecieveUpdates();
            //for(int i=0; i<=10; i++)
            //{
            //    signalRHub.trigger();
            //    await Task.Delay(5000);
            //}


            Console.ReadLine();
        }
    }
}
