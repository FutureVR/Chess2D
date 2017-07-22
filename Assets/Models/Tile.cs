using System;
using UnityEngine;
using System.Collections.Generic;

public class Tile
{
    Dictionary<string, Sprite> spriteForTheme;

    private GameBoard gameBoard_;
    public int xIndex_;
    public int yIndex_;
    public bool isWhite_;

    private bool isPossibleMove_;

    public bool IsPossibleMove
    {
        get
        {
            return this.isPossibleMove_;
        }
        set
        {
            this.isPossibleMove_ = value;

            if (cbPossibleMove != null)
                cbPossibleMove(this);
        }
    }

    public Piece myPiece_;
    private Action<Tile> cbPossibleMove;

    public Tile(GameBoard gameBoard, bool isWhite, int xIndex, int yIndex)
    {
        gameBoard_ = gameBoard;
        xIndex_ = xIndex;
        yIndex_ = yIndex;
        myPiece_ = null;
        isWhite_ = isWhite;
    }

    public void registerPossibleMoveChangedCallback(Action<Tile> callback)
    {
        cbPossibleMove += callback;
    }

    public void unregisterPossibleMoveChangedCallback(Action<Tile> callback)
    {
        cbPossibleMove -= callback;
    }
}