using System;
using System.Collections.Generic;
using UnityEngine;

public class Piece
{
    public Dictionary<string, Sprite> spriteForTheme;
    public bool hasMoved_ = false;
    public int owner_;
    private bool captured = false;
    private Action<Piece> cbPieceCaptured;
    private Action<Piece> cbPiecePlacementChanged;
    private Vector2 position_;
    protected bool isCastleable = false;

    public Piece(int owner, Vector2 position)
    {
        owner_ = owner;
        position_ = position;
    }

    public bool Captured
    {
        get { return this.captured; }
        set
        {
            this.captured = value;

            if (cbPieceCaptured != null)
                cbPieceCaptured(this);
        }
    }

    public Vector2 Position_
    {
        get { return this.position_; }
        set
        {
            this.position_ = value;

            if (cbPiecePlacementChanged != null)
                cbPiecePlacementChanged(this);
        }
    }
    public virtual void reachedEndOfBoard(Vector2 myIndex)
    {
    }

    public void registerPieceCapturedCallback(Action<Piece> callback)
    {
        cbPieceCaptured += callback;
    }

    public void registerPiecePlacementChangedCallback(Action<Piece> callback)
    {
        cbPiecePlacementChanged += callback;
    }

    public virtual HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        return null;
    }

    public virtual HashSet<Tile> returnAttackTiles(Tile myTile)
    {
        Vector2 myIndex = BoardController.instance.getIndexOfTile(myTile);
        HashSet<Vector2> possibleAttackIndices = returnAttackIndices(myIndex);
        return convertVectorSetToTileSet(possibleAttackIndices);
    }

    public HashSet<Vector2> returnMoveIndicesFromAttackIndices(Vector2 myIndex)
    {
        HashSet<Vector2> attackIndices = returnAttackIndices(myIndex);
        HashSet<Vector2> moveIndices = new HashSet<Vector2>();
        GameBoard gameBoard = BoardController.instance.gameBoard_;

        foreach (Vector2 index in attackIndices)
            checkAndAddMovementAtTile(index, moveIndices, gameBoard, true, false);

        return moveIndices;
    }

    //Check for out of bounds errors, so that there are possibly zero moves
    public virtual HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        HashSet<Vector2> possibleMoveIndices = new HashSet<Vector2>();

        if (owner_ == 0)
            possibleMoveIndices.Add(myIndex + new Vector2(1, 1));
        else if (owner_ == 1)
            possibleMoveIndices.Add(myIndex + new Vector2(-1, -1));

        return possibleMoveIndices;
    }

    // TODO: Alter this using sets to allow for kings to castle and eventually 
    //       for checker pieces to move
    // TODO: Alter this so that a piece cannot move away, allowing for the king
    //       to be in check
    // TODO: If a king is in check, but has not lost, then the only possible 
    //       moves are those that prevent the king from staying in check
    public HashSet<Tile> returnPossibleMoveTiles(Tile myTile, HashSet<TileMove> otherMoves)
    {
        if (myTile == null)
        {
            Debug.LogError("Trying to get possible moves from null tile");

            return null;
        }

        Vector2 myIndex = BoardController.instance.getIndexOfTile(myTile);
        //Debug.Log(otherMoves.Count);
        HashSet<IndexMove> otherMoveIndices = convertTileMoveSetToIndexMove(otherMoves);

        HashSet<Vector2> possibleMoveIndices = returnPossibleMoveIndices(myIndex, otherMoveIndices);

        return convertVectorSetToTileSet(possibleMoveIndices);
    }

    public void unregisterPieceCapturedCallback(Action<Piece> callback)
    {
        cbPieceCaptured -= callback;
    }

    public void unregisterPiecePlacementChangedCallback(Action<Piece> callback)
    {
        cbPiecePlacementChanged -= callback;
    }

    // Returns true if there is a piece on this spot
    // Mode determines which pieces are included in the HashSet
    protected bool checkAndAddMovementAtTile(Vector2 currentIndex, HashSet<Vector2> possibleMoves,
        GameBoard gameBoard, bool includeEnemies, bool includeFriends)
    {
        // Exit if the currentIndex is out of bounds
        if (currentIndex.x < 0 || currentIndex.x >= gameBoard.width ||
            currentIndex.y < 0 || currentIndex.y >= gameBoard.height)
        {
            return false;
        }

        Piece pieceOnCurrentTile = gameBoard.getTileAtIndex(currentIndex).myPiece_;

        if (pieceOnCurrentTile == null)
        {
            possibleMoves.Add(currentIndex);
        }
        else //running into another piece
        {
            // If piece is a friend
            if (owner_ == pieceOnCurrentTile.owner_)
            {
                if (includeFriends) possibleMoves.Add(currentIndex);
            }
            else  // If piece is an enemy
            {
                if (includeEnemies) possibleMoves.Add(currentIndex);
            }

            return true;
        }
        return false;
    }

    protected HashSet<Vector2> returnAttackIndices_Diagonal(Vector2 myIndex)
    {
        GameBoard gameBoard = BoardController.instance.gameBoard_;
        HashSet<Vector2> possibleMoves = new HashSet<Vector2>();

        for (int dist = 1; myIndex.x + dist < gameBoard.width &&
            myIndex.y + dist < gameBoard.height; dist++)
        {
            Vector2 currentIndex = new Vector2(myIndex.x + dist, myIndex.y + dist);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int dist = 1; myIndex.x - dist >= 0 &&
            myIndex.y - dist >= 0; dist++)
        {
            Vector2 currentIndex = new Vector2(myIndex.x - dist, myIndex.y - dist);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int dist = 1; myIndex.x + dist <= gameBoard.width &&
            myIndex.y - dist >= 0; dist++)
        {
            Vector2 currentIndex = new Vector2(myIndex.x + dist, myIndex.y - dist);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int dist = 1; myIndex.x - dist >= 0 &&
            myIndex.y + dist <= gameBoard.height; dist++)
        {
            Vector2 currentIndex = new Vector2(myIndex.x - dist, myIndex.y + dist);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        return possibleMoves;
    }

    protected HashSet<Vector2> returnAttackIndices_Straight(Vector2 myIndex)
    {
        GameBoard gameBoard = BoardController.instance.gameBoard_;
        HashSet<Vector2> possibleMoves = new HashSet<Vector2>();

        for (int x = (int)myIndex.x + 1; x < gameBoard.width; x++)
        {
            Vector2 currentIndex = new Vector2(x, myIndex.y);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int x = (int)myIndex.x - 1; x >= 0; x--)
        {
            Vector2 currentIndex = new Vector2(x, myIndex.y);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int y = (int)myIndex.y + 1; y < gameBoard.height; y++)
        {
            Vector2 currentIndex = new Vector2(myIndex.x, y);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        for (int y = (int)myIndex.y - 1; y >= 0; y--)
        {
            Vector2 currentIndex = new Vector2(myIndex.x, y);
            if (checkAndAddMovementAtTile(currentIndex, possibleMoves, gameBoard, true, true)) break;
        }

        return possibleMoves;
    }

    protected HashSet<Vector2> returnPossibleMoveIndices_Diagonal(Vector2 myIndex)
    {
        return returnAttackIndices_Diagonal(myIndex);
    }

    protected HashSet<Vector2> returnPossibleMoveIndices_Straight(Vector2 myIndex)
    {
        return returnAttackIndices_Straight(myIndex);
    }

    /*protected void setPieceSprite(string whiteSprite, string blackSprite)
    {
        if (owner_ == 0)
            mySprite_ = Resources.Load<Sprite>(whiteSprite) as Sprite;
        else if (owner_ == 1)
            mySprite_ = Resources.Load<Sprite>(blackSprite) as Sprite;
    }*/

    private HashSet<Tile> convertVectorSetToTileSet(HashSet<Vector2> vectorSet)
    {
        HashSet<Tile> vecSet = new HashSet<Tile>();

        foreach (Vector2 vec in vectorSet)
        {
            Tile tile = BoardController.instance.gameBoard_.getTileAtIndex(vec);
            vecSet.Add(tile);
        }

        return vecSet;
    }

    HashSet<IndexMove> convertTileMoveSetToIndexMove(HashSet<TileMove> otherMoves)
    {
        HashSet<IndexMove> indexMoveSet = new HashSet<IndexMove>();

        foreach (TileMove tileMove in otherMoves)
            indexMoveSet.Add(tileMoveToIndexMove(tileMove));

        return indexMoveSet;
    }

    IndexMove tileMoveToIndexMove(TileMove tileMove)
    {
        Vector2 startIndex = BoardController.instance.getIndexOfTile(tileMove.tileStart);
        Vector2 endIndex = BoardController.instance.getIndexOfTile(tileMove.tileEnd);

        IndexMove indexMove = new IndexMove(startIndex, endIndex);
        return indexMove;
    }
}