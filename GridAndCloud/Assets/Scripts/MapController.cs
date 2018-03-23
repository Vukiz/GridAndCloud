using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

namespace Assets.Scripts
{
  public class MapController : MonoBehaviour
  {
    private GameObject cellPrefab;
    private List<List<Cell>> map;
    public const int Size = 4;
    public List<List<Cell>> Map
    {
      get { return MapInitialize(); }
      set { map = value; }
    }

    private List<List<Cell>> MapInitialize()
    {
      if (map != null) return map;
      map = new List<List<Cell>>(Size);
      for (var i = 0; i < Size; i++)
      {
        map.Add(new List<Cell>(Size));
        for (var j = 0; j < Size; j++)
        {
          map[i].Add(Instantiate(cellPrefab).GetComponent<Cell>());
          map[i][j].Initialize(i + (int) transform.position.x, j + (int) transform.position.y, transform);
        }
      }
      return map;
    }

    // Use this for initialization
    void Start()
    {
      cellPrefab = Resources.Load<GameObject>("Prefabs/CellPrefab");
      MapInitialize();

    }
  }
}
