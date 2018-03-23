namespace IslandGateway
{
  class Program
  {
    private static ConnectionController connectionController;

    static void Main(string[] args)
    {
      connectionController = new ConnectionController();
      connectionController.Start();
    }
  }
}
