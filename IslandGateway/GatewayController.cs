using System;
using System.Collections.Concurrent;
using NetMQ;
using NetMQ.Sockets;

namespace IslandGateway
{
  class GatewayController
  {
    private readonly ConcurrentQueue<string> toServerMessageQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<string> toClientMessageQueue = new ConcurrentQueue<string>();
    private bool wolfGiven;//should be in list for concrete session
    private bool rabbitGiven;

    public void SendFrameIfNeeded(PublisherSocket publisherSocket)
    {
      while (!toServerMessageQueue.IsEmpty)
      {
        string message;
        if (toServerMessageQueue.TryDequeue(out message))
        {
          publisherSocket.SendMoreFrame("Server").SendFrame(message);
          Console.WriteLine("Sended to server :"+ message);
        }
        else
        {
          Console.WriteLine("Cannot dequeue server message");
          break;
        }
      }
      while (!toClientMessageQueue.IsEmpty)
      {
        string message;
        if (toClientMessageQueue.TryDequeue(out message))
        {
          publisherSocket.SendMoreFrame("Client").SendFrame(message);
          Console.WriteLine("Sended to client :" + message);
        }
        else
        {
          Console.WriteLine("Cannot dequeue server message");
          break;
        }
      }
    }
    
    public string GivePlayerId(string playerStats)
    {
      var splitted = playerStats.Split(' ');
      if (splitted.Length <= 1)
      {
        return string.Empty;
      }
      int wolf;
      int rabbit;
      if (!int.TryParse(splitted[0], out wolf) || !int.TryParse(splitted[1], out rabbit))
      {
        return string.Empty;
      }
      if (wolfGiven || wolf > rabbit)
      {
        rabbitGiven = true;
        return "rabbit";
      }
      if (rabbitGiven || rabbit >= wolf)
      {
        wolfGiven = true;
        return "wolf";
      }
      return string.Empty;
    }

    public void HandleClientMessage(string frameString)
    {
      toServerMessageQueue.Enqueue(frameString);
    }

    public void HandleServerMessage(string frameString)
    {
      toClientMessageQueue.Enqueue(frameString);
    }
  }
}
