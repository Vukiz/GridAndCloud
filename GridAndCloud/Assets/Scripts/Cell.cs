using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public class Cell : MonoBehaviour
  {
    public void Initialize(int x, int y, Transform trans)
    {
      transform.position = new Vector2(x, y);
      transform.parent = trans;
      transform.name = "Cell[" + x + "][" + y + "]";
    }
  }
}