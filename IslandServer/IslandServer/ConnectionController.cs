using System;
using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace IslandServer
{
  public class ConnectionController
  {
    private readonly Thread publishWorker;
    private readonly Thread listenerWorker;
    private ServerController server;
    private bool cancelled;
    
    private string PublishConnectionString => "tcp://127.0.0.1:12345";
    private string ListenerConnectionString => "tcp://127.0.0.1:12346";
    
    private void PublisherWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var publisherSocket = new PublisherSocket())
      {
        publisherSocket.Options.SendHighWatermark = 1000;
        publisherSocket.Bind(PublishConnectionString);
        Console.WriteLine("Binded Publisher to " + PublishConnectionString);
        while (!cancelled)
        {
          server.SendFrameIfNeeded(publisherSocket);
        }
      }
      NetMQConfig.Cleanup();
    }

    private void ListenerWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var subSocket = new SubscriberSocket())
      {
        subSocket.Options.ReceiveHighWatermark = 1000;
        subSocket.Connect(ListenerConnectionString);
        subSocket.Subscribe("Server");
        Console.WriteLine("Subscribed Listener to " + ListenerConnectionString);
        while (!cancelled)
        {
          string frameString;
          if (!subSocket.TryReceiveFrameString(out frameString)) continue;
          server.HandleMessage(frameString.Replace("Server", ""));
        }
        subSocket.Close();
      }
      NetMQConfig.Cleanup();
    }

    public ConnectionController()
    {
      publishWorker = new Thread(PublisherWork);
      listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
      cancelled = false;
      server = new ServerController();
      publishWorker.Start();
      listenerWorker.Start();
    }

    public void Stop()
    {
      cancelled = true;
      publishWorker.Join();
      listenerWorker.Join();
    }
  }
}
