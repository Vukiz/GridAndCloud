using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IslandServer
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
