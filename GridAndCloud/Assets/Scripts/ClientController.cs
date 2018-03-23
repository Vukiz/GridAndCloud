using System;
using Assets.Scripts.GameJson;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class ClientController : MonoBehaviour
  {
    private ServerController serverListener;
    private MapController map;
    private GameObject playerOne;
    private GameObject playerTwo;
    private int size;

    public int WolfCount = 1;//should be loaded from stats
    public int RabbitCount = 0;
    public string MessageToSend;
    private GameStates currentGameState;
    private Text gameOverText;
    public int PlayerId { get; set; }

    private void HandleMessage(string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      GameState gameStep = message.FromJson<GameState>();
      switch (gameStep.CurrentGameState)
      {
        case 1:
        {
          UpdatePlayersPosition(gameStep.PlayerOne, gameStep.PlayerTwo);
          break;
        }
        case 2:
        {
          UpdatePlayersPosition(gameStep.PlayerOne, gameStep.PlayerTwo);
          EndGame();
          break;
        }
      }
    }

    private void EndGame()
    {
      currentGameState = GameStates.Ended;
      gameOverText.enabled = true;
    }

    private void UpdatePlayersPosition(Player gameStepPlayerOne, Player gameStepPlayerTwo)
    {
      playerOne.transform.position = new Vector3(gameStepPlayerOne.X, gameStepPlayerOne.Y, -1);
      playerTwo.transform.position = new Vector3(gameStepPlayerTwo.X, gameStepPlayerTwo.Y, -1);
    }

    private void Start()
    {
      gameOverText = GameObject.Find("GameOverText").GetComponent<Text>();
      gameOverText.enabled = false;
      serverListener = new ServerController(HandleMessage);
      serverListener.Start(this);
      map = GameObject.Find("GameController").GetComponent<MapController>();
      size = MapController.Size;
      playerOne = GameObject.Find("PlayerOne");
      playerTwo = GameObject.Find("PlayerTwo");
    }

    private void Update()
    {
      if (currentGameState != GameStates.Ended && Input.anyKey)
      {
        PlayerMove playerMove = new PlayerMove
        {
          PlayerId = PlayerId
        };
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
          playerMove.Move = "DOWN";
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
          playerMove.Move = "UP";
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
          playerMove.Move = "LEFT";
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
          playerMove.Move = "RIGHT";
        }
        if (!string.IsNullOrEmpty(playerMove.Move))
        {
          MessageToSend = playerMove.ToJson();
        }
      }
      serverListener?.Update();
    }

    private void OnDestroy()
    {
      serverListener.Stop();
    }
  }
}

