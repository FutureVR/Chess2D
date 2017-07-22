using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController {

    Piece currentlySelectedPiece = null;
    Tile startTile = null;
    HashSet<Tile> possibleMoveTiles;
    bool leftMouseButtonHeldDown = false;

    // TODO: Split update into multiple functions to reduce complexity
    public void update()
    {

        if (Input.GetMouseButtonDown(0) && leftMouseButtonHeldDown == false)
        {
            //Convert screen coordinates to world coordinates, then convert to tile
            leftMouseButtonHeldDown = true;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startTile = BoardController.instance.getTileAtWorldCoord(mouseWorldPos);

            if (startTile == null)
            {
                Debug.Log("Clicked outside of range");
            }
            else
            {
                currentlySelectedPiece = startTile.myPiece_;

                if (currentlySelectedPiece == null)
                {
                    Debug.Log("No piece was clicked");
                }
                else
                {
                    if (currentlySelectedPiece.owner_ == BoardController.instance.gameBoard_.currentPlayer)
                    {
                        //Get the set of tiles that the current piece can move to
                        HashSet<TileMove> otherMoves = new HashSet<TileMove>();
                        possibleMoveTiles = currentlySelectedPiece.returnPossibleMoveTiles(startTile, otherMoves);
                        //Debug.Log(otherMoves.Count);

                        //Set each tile in set as a possible move
                        if (possibleMoveTiles.Count != 0)
                        {
                            foreach (Tile tile in possibleMoveTiles)
                            {
                                if (tile != null)
                                    tile.IsPossibleMove = true;
                            }
                        }
                    }
                }
            }
        }


        if (leftMouseButtonHeldDown == true)
        {
            Preferences myPrefs = GameController.instance.myPrefs;
            if (myPrefs.freePieceMovement)
            {
                if (currentlySelectedPiece != null)
                {
                    currentlySelectedPiece.Position_ =
                        Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            leftMouseButtonHeldDown = false;

            if (currentlySelectedPiece != null  &&  startTile != null)
            {
                Vector3 endTileWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Tile endTile = BoardController.instance.getTileAtWorldCoord(endTileWorldCoord);

                if (endTile == null)
                {
                    Debug.Log("Cannot Move a piece off the board");
                    setPiecePositionAtTile(currentlySelectedPiece, startTile);
                }
                else
                {
                    if (endTile.IsPossibleMove  && BoardController.instance.playerHasWon == false)
                    {
                        BoardController.instance.makeMove(startTile, endTile);
                        int result = BoardController.instance.checkWinConditionsForAll();
                        BoardController.instance.changeTurn();

                        if (result == 0 || result == 1)
                        {
                            if (result == 0) Debug.Log("White won");
                            else if (result == 1) Debug.Log("Black won");
                            BoardController.instance.endGame();
                        }

                        setPiecePositionAtTile(currentlySelectedPiece, endTile);
                    }
                    else
                    {
                        Debug.Log("Move is invalid");
                        setPiecePositionAtTile(currentlySelectedPiece, startTile);
                    }
                }

                //Set tile and piece variables back to original state
                //Set all the tiles back to normal tint
                if (possibleMoveTiles != null)
                {
                    if (possibleMoveTiles.Count != 0)
                    {
                        foreach (Tile tile in possibleMoveTiles)
                        {
                            tile.IsPossibleMove = false;
                        }
                    }
                }
            }

            currentlySelectedPiece = null;
            startTile = null;
            
            if (possibleMoveTiles != null)
                possibleMoveTiles.Clear();
        }
    }

    //Does not change a piece's tile, only its world position 
    void setPiecePositionAtTile(Piece piece, Tile tile)
    {
        if (tile == null) Debug.LogError("Setting piece to non-existent tile");
        piece.Position_ = BoardController.instance.getPositionOfTile(tile);
    }
}
