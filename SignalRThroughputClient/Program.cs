using System;
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

            signalRHub.RecieveUpdates();
            await signalRHub.Execute(200);
            

            Console.ReadLine();
        }
    }
}
