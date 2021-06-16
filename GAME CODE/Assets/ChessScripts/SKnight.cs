using System.Collections;
using UnityEngine;

public class SKnight : SChessman
{
    public override bool[,] chessPossibleMoves()
    {
        bool[,] r = new bool[8, 8];

        // Up left
        chessMove(CurrentX - 1, CurrentY + 2, ref r);

        // Up right
        chessMove(CurrentX + 1, CurrentY + 2, ref r);

        // Down left
        chessMove(CurrentX - 1, CurrentY - 2, ref r);

        // Down right
        chessMove(CurrentX + 1, CurrentY - 2, ref r);


        // Left Down
        chessMove(CurrentX - 2, CurrentY - 1, ref r);

        // Right Down
        chessMove(CurrentX + 2, CurrentY - 1, ref r);

        // Left Up
        chessMove(CurrentX - 2, CurrentY + 1, ref r);

        // Right Up
        chessMove(CurrentX + 2, CurrentY + 1, ref r);

        return r;
    }

}
