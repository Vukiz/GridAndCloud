using System;
using System.Collections.Concurrent;
using System.Threading;
using Assets.Scripts.GameJson;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
namespace Assets.Scripts
{
  public class ServerController
  {
    private readonly Thread listenerWorker;
    private readonly Thread publisherWorker;

    private bool cancelled;

    private ClientController client;
    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate messageDelegate;
    private readonly ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    

    private string RequestConnectionString => "tcp://127.0.0.1:12347";
    private string ListenerConnectionString => "tcp://127.0.0.1:12346";
    private string PublishConnectionString => "tcp://127.0.0.1:12344";
    private void PublisherWork()
    {
      AsyncIO.ForceDotNet.Force();
      using (var requestSocket = new RequestSocket(RequestConnectionString))
      {
        requestSocket.SendFrame(client.WolfCount + " " + client.RabbitCount);
        Debug.Log("Requested player id from gateway");
        string message = string.Empty;
        while (string.IsNullOrEmpty(message))
        {
          if (requestSocket.TryReceiveFrameString(out message))
          {
            if (message.Equals("rabbit", StringComparison.InvariantCultureIgnoreCase))
            {
              client.RabbitCount++;
              client.PlayerId = 1;
            }
            if (message.Equals("wolf", StringComparison.InvariantCultureIgnoreCase))
            {
              client.WolfCount++;
              client.PlayerId = 2;
            }
          }
        }
      }
      using (var publisherSocket = new PublisherSocket())
      {
        publisherSocket.Options.SendHighWatermark = 1000;
        publisherSocket.Bind(PublishConnectionString);
        Debug.Log("Binded Publisher to " + PublishConnectionString);
        while (!cancelled)
        {
          if (!string.IsNullOrEmpty(client.MessageToSend))
          {
            Debug.Log("SendingFrame with " + client.MessageToSend);
            publisherSocket.SendFrame(client.MessageToSend);
            client.MessageToSend = string.Empty;
            Thread.Sleep(100);
          }
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
        subSocket.Subscribe("Client");

        Debug.Log("Subscribed Listener to " + ListenerConnectionString);
        while (!cancelled)
        {
          string frameString;
          if (!subSocket.TryReceiveFrameString(out frameString)) continue;
          messageQueue.Enqueue(frameString.Replace("Client",""));
          Thread.Sleep(100);
        }
        subSocket.Close();
      }
      NetMQConfig.Cleanup();
    }

    public void Update()
    {
      while (!messageQueue.IsEmpty)
      {
        string message;
        if (messageQueue.TryDequeue(out message))
        {
          messageDelegate(message);
        }
        else
        {
          Debug.Log("Cannot dequeue message");
          break;
        }
      }
    }

    public ServerController(MessageDelegate messageDelegate)
    {
      this.messageDelegate = messageDelegate;
      listenerWorker = new Thread(ListenerWork);
      publisherWorker = new Thread(PublisherWork);
    }

    public void Start(ClientController clientController)
    {
      cancelled = false;
      client = clientController;
      listenerWorker.Start();
      publisherWorker.Start();
    }

    public void OnApplicationQuit()
    {
      Stop();
    }
    public void Stop()
    {
      cancelled = true;
      listenerWorker.Join();
      publisherWorker.Join();
    }
  }
}
