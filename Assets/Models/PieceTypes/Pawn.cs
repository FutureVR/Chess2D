using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece  {

    public Pawn(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        this.spriteForTheme = spriteForTheme;
    }

    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        HashSet<Vector2> possibleMoveIndices = new HashSet<Vector2>();
        GameBoard gameBoard = BoardController.instance.gameBoard_;

        // Add all possible displacements from original position
        // to the set allIndexDisplacements
        HashSet<Vector2> allIndexDisplacements = new HashSet<Vector2>();

        // White's piece
        int direction = 1;
        if (owner_ == 0)
            direction = 1;
        else if (owner_ == 1)
            direction = -1;

        if (myIndex.y + direction < gameBoard.height && myIndex.y + direction >= 0)
        {
            //Handle direct forward movement
            if (gameBoard.getTileAtIndex(new Vector2(0, 1 * direction) + myIndex).myPiece_ == null)
            {
                allIndexDisplacements.Add(new Vector2(0, 1 * direction));

                //Handle double movement if pawn has not moved
                if (hasMoved_ == false && myIndex.y + 2 * direction <= gameBoard.height &&
                    0 <= myIndex.y + 2 * direction)
                {
                    if (gameBoard.getTileAtIndex(new Vector2(0, 2 * direction) + myIndex).myPiece_ == null)
                        allIndexDisplacements.Add(new Vector2(0, 2 * direction));
                }
            }

            //Handle diagonal captures, if not on edge

            HashSet<Vector2> diagonalIndices = returnDiagonalDisplacements(myIndex, direction);

            if (diagonalIndices != null)
            {
                foreach (Vector2 index in diagonalIndices)
                {
                    if (gameBoard.getTileAtIndex(index + myIndex).myPiece_ != null)
                        allIndexDisplacements.Add(index);
                }
            }
        }


        // Use displacements to get index positions
        foreach (Vector2 displacement in allIndexDisplacements)
            checkAndAddMovementAtTile(myIndex + displacement, possibleMoveIndices, gameBoard, true, false);

        return possibleMoveIndices;
    }


    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        HashSet<Vector2> diagonalDisplacements = returnDiagonalDisplacements(myIndex, owner_ == 0 ? 1 : -1);
        HashSet<Vector2> diagonalIndices = new HashSet<Vector2>();

        foreach (Vector2 displacement in diagonalDisplacements)
            diagonalIndices.Add(displacement + myIndex);

        //Debug.Log(diagonalIndices.Count);
        return diagonalIndices;
    }

    // Returns diagonal tiles if they exist on the board
    // Does not require there to be a piece on the diagonal tile
    HashSet<Vector2> returnDiagonalDisplacements(Vector2 myIndex, int direction)
    {
        GameBoard gameBoard = BoardController.instance.gameBoard_;
        HashSet<Vector2> diagonalIndices = new HashSet<Vector2>();

        if (myIndex.y != 0 && myIndex.y != gameBoard.height - 1)
        {
            Vector2 diagonalIndexRight = new Vector2(1, 1 * direction);
            Vector2 diagonalIndexLeft = new Vector2(-1, 1 * direction);

            if (myIndex.x != gameBoard.width - 1)
            {
                diagonalIndices.Add(diagonalIndexRight);
            }
            if (myIndex.x != 0)
            {
                diagonalIndices.Add(diagonalIndexLeft);
            }
        }

        return diagonalIndices;
    }
    

    public override void reachedEndOfBoard(Vector2 myIndex)
    {

        BoardController bc = BoardController.instance;

        Piece piece_data = bc.gameBoard_.createPieceAtIndex(GameBoard.PieceTypes.Queen, owner_, myIndex);
        Tile myTile = bc.gameBoard_.getTileAtIndex(myIndex);

        string name = "TransformedPawn_" + myIndex.x;
        Vector2 worldPosPlane = BoardController.instance.worldCoordAtIndex(myIndex);
        Vector3 worldPosition = new Vector3( worldPosPlane.x, worldPosPlane.y, 0 );

        GameObject pieceParent = bc.allPiecesEmpty;
        GameObject piece_go = bc.createPieceFromData(piece_data, pieceParent, name, worldPosition);
        BoardController.instance.registerPieceCallbacks(piece_data, piece_go);

        //Replace this pawn's board's piece with the new piece, and destroy this
        myTile.myPiece_ = piece_data;
        BoardController.instance.gameBoard_.destroyPiece(this, myTile);
    }

}
