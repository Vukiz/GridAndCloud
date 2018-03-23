using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IslandServer.GameJson;
using NetMQ;
using NetMQ.Sockets;

namespace IslandServer
{
  class ServerController
  {
    private const int Size = 4;
    private List<List<List<int>>> map;
    private DateTime lastFrameSendTime;
    private const float MoveTimeSize = 100;

    private readonly List<string> PlayerMoves = new List<string>() { "RIGHT", "LEFT", "DOWN", "UP" };
    private bool PlayersReady { get; set; }
    public ServerController()
    {
      InitializeMap();
    }

    public void HandleMessage(string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      Console.WriteLine("Recevied message: " + message);
      PlayerMove playerMove = message.FromJson<PlayerMove>();
      MovePlayers(playerMove.Move, playerMove.PlayerId);
    }

    private GameState GetGameState()
    {
      GameState gameState = new GameState {CurrentGameState = 1, PlayerOne = new Player(), PlayerTwo = new Player()};
      for (int i = 0; i < map.Capacity; i++)
      {
        for (int j = 0; j < map.Capacity; j++)
        {
          if (map[i][j].Any(e => e != 0))
          {
            if (map[i][j].Contains(1))
            {
              gameState.PlayerOne.X = i;
              gameState.PlayerOne.Y = j;
            }
            if (map[i][j].Contains(2))
            {
              gameState.PlayerTwo.X = i;
              gameState.PlayerTwo.Y = j;
            }
          }
        }
      }
      if (gameState.PlayerOne.X == gameState.PlayerTwo.X && gameState.PlayerOne.Y == gameState.PlayerTwo.Y)
      {
        gameState.CurrentGameState = 2;
      }
      return gameState;
    }

    private void MovePlayers(string playerMove, int playerId)
    {
      if (playerId == 0)
      {
        Debug.Assert(false,"PlayerID cannot be 0");
        return;
      }
      bool playerOneMoved = false;
      bool playerTwoMoved = false;
      for (int i = 0; i < map.Capacity; i++)//TODO map[i][j]
      {
        for (int j = 0; j < map.Capacity; j++)
        {
          if (map[i][j].All(e => e == 0))
          {
            continue;
          }
          if (map[i][j].Contains(playerId))
          {
            if (playerOneMoved)
            {
              continue;
            }
            MovePlayer(i, j, playerMove, playerId);
            playerOneMoved = true;
          }
          else
          {
            if (playerTwoMoved)
            {
              continue;
            }
            Random rand = new Random();
            string randomizedMove = PlayerMoves[rand.Next(0, PlayerMoves.Count)];
            Console.WriteLine("Randomly moved to :"+ randomizedMove);
            MovePlayer(i, j, randomizedMove, map[i][j].FirstOrDefault(e=> e!= 0));
            playerTwoMoved = true;
          }
        }
      }
      if (playerOneMoved && playerTwoMoved)
      {
        PlayersReady = true;
      }
      else
      {
        Debug.Assert(false, "Not all players made their move");
      }
    }

    private void InitializeMap()
    {
      map = new List<List<List<int>>>(Size);
      for (var i = 0; i < Size; i++)
      {
        map.Add(new List<List<int>>(Size));
        for (var j = 0; j < Size; j++)
        {
          map[i].Add(new List<int>());
          map[i][j].Add(0);
        }
      }
      map[1][1].Add(1);;
      map[3][3].Add(2);
    }
    private void MovePlayer(int x, int y, string playerMove, int playerId)
    {
      int stepValue = 1;
      if (playerMove.Equals("RIGHT", StringComparison.InvariantCultureIgnoreCase))
      {
        map[x][y].Remove(playerId);
        map[(x + Size + stepValue) % Size][y].Add(playerId);
      }
      else if (playerMove.Equals("LEFT", StringComparison.InvariantCultureIgnoreCase))
      {
        map[x][y].Remove(playerId);
        map[(x + Size - stepValue) % Size][y].Add(playerId);
      }
      else if (playerMove.Equals("DOWN", StringComparison.InvariantCultureIgnoreCase))
      {
        map[x][y].Remove(playerId);
        map[x][(y + Size - stepValue) % Size].Add(playerId);
      }
      else if (playerMove.Equals("UP", StringComparison.InvariantCultureIgnoreCase))
      {
        map[x][y].Remove(playerId);
        map[x][(y + Size + stepValue) % Size].Add(playerId);
      }
    }

    public void SendFrameIfNeeded(PublisherSocket publisherSocket)
    {
      if (PlayersReady && lastFrameSendTime.AddMilliseconds(MoveTimeSize) < DateTime.Now)
      {
        lastFrameSendTime = DateTime.Now;
        PlayersReady = false;
        string gameStateJson = GetGameState().ToJson();
        publisherSocket.SendFrame(gameStateJson);
        Console.WriteLine("Send gamestate: " + gameStateJson);
      }
    }
  }
}
