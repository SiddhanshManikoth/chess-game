using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSHighlights : MonoBehaviour
{

    public static BoardSHighlights Instance { set; get; }

    public GameObject ShighlightPrefab;
    private List<GameObject> Shighlights;

    private void Start()
    {
        Instance = this;
        Shighlights = new List<GameObject>();
    }

    private GameObject GetHighLightObject()
    {
        GameObject go = Shighlights.Find(g => !g.activeSelf);

        if (go == null)
        {
            go = Instantiate(ShighlightPrefab);
            Shighlights.Add(go);
        }

        return go;
    }

    public void HighLightAllowedMoves(bool[,] moves)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (moves[i, j])
                {
                    GameObject go = GetHighLightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0.0001f, j + 0.5f);
                }
            }

        }
    }

    public void HideHighlights()
    {
        foreach (GameObject go in Shighlights)
            go.SetActive(false);
    }
}
