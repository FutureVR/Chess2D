using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInfo {

    public Vector2 boardSize;
    public int boardColumns;
    public int boardRows;

    public BoardInfo(Vector2 boardSize_, int cols_, int rows_)
    {
        boardSize = boardSize_;
        boardColumns = cols_;
        boardRows = rows_;
    }
}
