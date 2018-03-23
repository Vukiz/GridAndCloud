using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace IslandGateway
{
  class ConnectionController
  {
    private readonly Thread publishWorker;
    private readonly Thread serverListenerWorker;
    private readonly Thread clientListenerWorker;
    private bool cancelled;

    private GatewayController gateway;
    private string ResponseConnectionString => "tcp://127.0.0.1:12347";
    private string PublishConnectionString => "tcp://127.0.0.1:12346";
    private string ServerListenerConnectionString => "tcp://127.0.0.1:12345";
    private string ClientListenerConnectionString => "tcp://127.0.0.1:12344";

    public ConnectionController()
    {
      publishWorker = new Thread(PublisherWork);
      serverListenerWorker = new Thread(ServerListenerWork);
      clientListenerWorker = new Thread(ClientListenerWork);
    }
    private void PublisherWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var responseSocket = new ResponseSocket(ResponseConnectionString))
      {
        string message = string.Empty;
        Console.WriteLine("Waiting for clients to connect");
        while (string.IsNullOrEmpty(message))
        {
          if(responseSocket.TryReceiveFrameString(out message))
          {
            string playerID = gateway.GivePlayerId(message);
            Console.WriteLine("Player id is : "+ playerID);
            if (string.IsNullOrEmpty(playerID))
            {
              message = string.Empty;
            }
            else
            {
              responseSocket.SendFrame(playerID);
            }
          }
        }

      }
      using (var publisherSocket = new PublisherSocket())
      {
        publisherSocket.Options.SendHighWatermark = 1000;
        publisherSocket.Bind(PublishConnectionString);
        Console.WriteLine("Binded Publisher to " + PublishConnectionString);
        while (!cancelled)
        {
          gateway.SendFrameIfNeeded(publisherSocket);
        }
      }
      NetMQConfig.Cleanup();
    }

    private void ServerListenerWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var subSocket = new SubscriberSocket())
      {
        subSocket.Options.ReceiveHighWatermark = 1000;
        subSocket.Connect(ServerListenerConnectionString);
        subSocket.Subscribe("");
        Console.WriteLine("Subscribed Server Listener to " + ServerListenerConnectionString);
        while (!cancelled)
        {
          string frameString;
          if (!subSocket.TryReceiveFrameString(out frameString)) continue;
          gateway.HandleServerMessage(frameString);
        }
        subSocket.Close();
      }
      NetMQConfig.Cleanup();
    }
    private void ClientListenerWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var subSocket = new SubscriberSocket())
      {
        subSocket.Options.ReceiveHighWatermark = 1000;
        subSocket.Connect(ClientListenerConnectionString);
        subSocket.Subscribe("");
        Console.WriteLine("Subscribed Client Listener to " + ClientListenerConnectionString);
        while (!cancelled)
        {
          string frameString;
          if (!subSocket.TryReceiveFrameString(out frameString)) continue;
          gateway.HandleClientMessage(frameString);
        }
        subSocket.Close();
      }
      NetMQConfig.Cleanup();
    }
    public void Start()
    {
      cancelled = false;
      gateway = new GatewayController();
      publishWorker.Start();
      serverListenerWorker.Start();
      clientListenerWorker.Start();
    }

    public void Stop()
    {
      cancelled = true;
      publishWorker.Join();
      serverListenerWorker.Join();
      clientListenerWorker.Join();
    }
  }
}