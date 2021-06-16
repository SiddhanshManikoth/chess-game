using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SChessman : MonoBehaviour
{

    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    public bool isWhitechess;

    public void chessSetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] chessPossibleMoves()
    {
        return new bool[8, 8];
    }

    public bool chessMove(int x, int y, ref bool[,] r)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            SChessman c = BoardManager.Instance.SChessmans[x, y];
            if (c == null)
                r[x, y] = true;
            else
            {
                if (isWhitechess != c.isWhitechess)
                    r[x, y] = true;
                return true;
            }
        }
        return false;
    }
}
