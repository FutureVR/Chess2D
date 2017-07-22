using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController {

    // The following three values can be freely
    // set to any numbers
    Vector2 boardSize;
    int boardColumns;
    int boardRows;

    // These values are calculated from other variables
    Vector2 boardPosition;
    Vector2 tileSize;

    // Sprites for tiles
    Sprite blackTileSprite;
    Sprite whiteTileSprite;
    Sprite tintedBlackTileSprite;
    Sprite tintedWhiteTileSprite;

    public GameBoard gameBoard_ { get; protected set; }
    public static BoardController instance { get; protected set; }


    GameObject wholeChessSetEmpty;
    GameObject boardEmpty;
    [HideInInspector]
    public GameObject allPiecesEmpty;

    public bool playerHasWon = false;

    Preferences myPref;


    public BoardController( )
    {
        instance = this;
    }

    public void beginChessGame(BoardInfo boardInfo)
    {
        //Set parameters to variables
        this.boardSize = boardInfo.boardSize;
        boardRows = boardInfo.boardRows;
        boardColumns = boardInfo.boardColumns;

        // Get and calculate size and position of board
        tileSize.x = boardSize.x / boardColumns;
        tileSize.y = boardSize.y / boardRows;
        boardPosition = calculateBoardPositionFromCam(Camera.main);

        // Gameboard creation needs to go after board position is found
        gameBoard_ = new GameBoard(boardRows, boardColumns);

        // TODO: Fix names for tiles to make it more flexible for other themes
        string theme = GameController.instance.myPrefs.theme;
        Dictionary<string, Sprite> tileToSprite = GameController.instance.tileToSpriteForTheme[theme];

        blackTileSprite = tileToSprite["BlackTile"];
        whiteTileSprite = tileToSprite["WhiteTile"];
        tintedBlackTileSprite = tileToSprite["BlackTileTinted"];
        tintedWhiteTileSprite = tileToSprite["WhiteTileTinted"];

        // Create a gameobject to hold all tiles and pieces
        wholeChessSetEmpty = new GameObject();
        wholeChessSetEmpty.name = "Chess_Set";
        wholeChessSetEmpty.transform.position = new Vector3(0, 0, 0);

        // Create a gameobject that is the parent of all tiles
        boardEmpty = new GameObject();
        boardEmpty.name = "Board";
        boardEmpty.transform.SetParent(wholeChessSetEmpty.transform);
        boardEmpty.transform.position = new Vector3(0, 0, 0);

        GameController gc = GameController.instance;
        string themeName = gc.myPrefs.theme;
        
        if (gc.boardForTheme.ContainsKey(themeName))
        {
            boardEmpty.AddComponent<SpriteRenderer>().sprite =
                gc.boardForTheme[themeName];
            boardEmpty.transform.localScale = boardSize;
        }

        // Create a gameobject that holds all pieces
        allPiecesEmpty = new GameObject();
        allPiecesEmpty.name = "Pieces";
        allPiecesEmpty.transform.SetParent(wholeChessSetEmpty.transform);
        allPiecesEmpty.transform.position = new Vector3(0, 0, 0);

        for (int x = 0; x < gameBoard_.width; x++)
        {
            for (int y = 0; y < gameBoard_.height; y++)
            {
                //Create the tile game object
                Tile tile_data = gameBoard_.getTileAtIndex(new Vector2(x, y));

                Vector2 pos = getPositionOfTile(tile_data);
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                GameObject tile_go = createTileGameObjectFromData(tile_data, 
                    boardEmpty, "Tile_" + x + "_" + y, "Board", 
                    worldPos, tile_data.isWhite_);        

                //Register the tile_data
                tile_data.registerPossibleMoveChangedCallback((tile) 
                    => { onTilePossibleMoveChanged(tile, tile_go); });

                //Create the piece game object, if it exists
                Piece piece_data = gameBoard_.getTileAtIndex(new Vector2(x, y)).myPiece_;

                if (piece_data != null)
                {
                    //Create the gameobject from the piece_data
                    string name = "Piece_" + x + "_" + y;
                    Vector3 worldPosition = new Vector3(piece_data.Position_.x, piece_data.Position_.y, 0);
                    GameObject piece_go = createPieceFromData(piece_data, allPiecesEmpty, name, worldPosition);

                    //Register piece_data for callbacks 
                    registerPieceCallbacks(piece_data, piece_go);
                }
            }
        }
    }

    Vector2 calculateBoardPositionFromCam(Camera cam)
    {
        Vector2 middleInScreenCoord = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        Vector2 middleInWorldCoord = cam.ScreenToWorldPoint(middleInScreenCoord);
        boardPosition = middleInWorldCoord - boardSize / 2;
        boardPosition += new Vector2( tileSize.x / 2, tileSize.y / 2);
        return boardPosition;
    }

    public GameObject createPieceFromData(Piece piece_data, 
        GameObject parent, string name, Vector3 worldPosition)
    {
        GameObject piece_go = new GameObject();
        piece_go.name = name;
        piece_go.transform.position = worldPosition;
        piece_go.AddComponent<SpriteRenderer>();

        Preferences myPref = GameController.instance.myPrefs;
        piece_go.GetComponent<SpriteRenderer>().sprite 
            = piece_data.spriteForTheme[myPref.theme];


        piece_go.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";
        piece_go.transform.SetParent(parent.transform);
        piece_go.transform.localScale = tileSize;

        return piece_go;
    }

    GameObject createTileGameObjectFromData(Tile tile_data, GameObject parent, 
            string name, string sortingLayer, Vector3 position, bool isWhite)
    {
        GameObject tile_go = new GameObject();
        tile_go.name = name;
        tile_go.transform.SetParent(parent.transform);
        tile_go.transform.position = position;

        GameController gc = GameController.instance;
        string themeName = gc.myPrefs.theme;
        if (gc.boardForTheme.ContainsKey(themeName) == false)
        {
            tile_go.AddComponent<SpriteRenderer>();
            tile_go.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            tile_go.transform.localScale = tileSize;

            if (tile_data.isWhite_)
                tile_go.GetComponent<SpriteRenderer>().sprite = whiteTileSprite;
            else
                tile_go.GetComponent<SpriteRenderer>().sprite = blackTileSprite;
        }

        return tile_go;
    }

    public void registerPieceCallbacks(Piece piece_data, GameObject piece_go)
    {
        piece_data.registerPiecePlacementChangedCallback((piece) => { onPieceMoved(piece_data, piece_go); });
        piece_data.registerPieceCapturedCallback((piece) => { onPieceCaptured(piece_data, piece_go); });
    }



    public void makeMove(Tile startTile, Tile endTile)
    {
        Debug.Log(playerHasWon);
        if (playerHasWon == false)
        {

            // Get the starting piece, if it exists
            Piece startPiece = startTile.myPiece_;

            if (startPiece == null)
            {
                Debug.LogError("Trying to move non-existant piece");
                return;
            }

            // Destroy target piece if it exists
            Piece endPiece = endTile.myPiece_;
            if (endPiece != null)
                gameBoard_.destroyPiece(endPiece, endTile);

            // Move the pieces
            startTile.myPiece_ = null;
            endTile.myPiece_ = startPiece;
            startPiece.hasMoved_ = true;

            startPiece.Position_ = getPositionOfTile(endTile);

            // Check and report if piece reached the end of the board
            int endTileHeight = (int)getIndexOfTile(endTile).y;
            if (endTileHeight == 0 || endTileHeight == gameBoard_.height - 1)
                startPiece.reachedEndOfBoard(getIndexOfTile(endTile));
        }
    }

    public void changeTurn()
    {
        gameBoard_.changeTurn();
    }


    public Vector2 worldCoordAtIndex(Vector2 index)
    {
        Vector2 offset = boardPosition;
        Vector2 relativePosition = 
            new Vector2(tileSize.x * index.x, tileSize.y * index.y);

        return offset + relativePosition;
    }

    public Vector2 indexAtWorldCoord(Vector2 worldCoord)
    {
        Vector2 offset = boardPosition;
        Vector2 relativePosition = worldCoord - offset;
        Vector2 index = new Vector2(relativePosition.x / tileSize.x,
            relativePosition.y / tileSize.y);
        return index;
    }

    public Vector2 getPositionOfTile(Tile tile)
    {
        Vector2 index = new Vector2(tile.xIndex_, tile.yIndex_);
        return worldCoordAtIndex(index);
    }

    public Vector2 getIndexOfTile(Tile tile)
    {
        return new Vector2(tile.xIndex_, tile.yIndex_);
    }

    public Tile getTileAtWorldCoord(Vector3 position)
    {
        Vector2 offset = boardPosition;
        Vector2 relativePosition = 
            new Vector2(position.x, position.y) - offset;

        int xIndexTile;
        int yIndexTile;
        xIndexTile = Mathf.FloorToInt(relativePosition.x / tileSize.x + .5f);
        yIndexTile = Mathf.FloorToInt(relativePosition.y / tileSize.y + .5f);

        return gameBoard_.getTileAtIndex(new Vector2(xIndexTile, yIndexTile));
    }


    //Set the position of the piece_go to the position of the piece_data
    void onPieceMoved(Piece piece_data, GameObject piece_go)
    {
        piece_go.transform.position = new Vector3(piece_data.Position_.x, piece_data.Position_.y, 0);
    }

    //If tile_data.isPossibleMove, Add green tint, else default to regular tint
    void onTilePossibleMoveChanged(Tile tile_data, GameObject tile_go)
    {
        Preferences myPrefs = GameController.instance.myPrefs;
        if (myPrefs.highlightTiles  &&  playerHasWon == false)
        {
            SpriteRenderer renderer = tile_go.GetComponent<SpriteRenderer>();

            if (tile_data.IsPossibleMove)
            {
                if (tile_data.isWhite_) renderer.sprite = tintedWhiteTileSprite;
                else renderer.sprite = tintedBlackTileSprite;
            }
            else
            {
                if (tile_data.isWhite_) renderer.sprite = whiteTileSprite;
                else renderer.sprite = blackTileSprite;
            }
        }
    }


    void onPieceCaptured(Piece piece_data, GameObject piece_go)
    {
        //Destroy(piece_go);
        Object.Destroy(piece_go);
        Debug.Log("Capturing Piece");
    }

    // Returns 0 if white has won, 1 if black has won, and -1 if no one has
    // TODO: This will not work exactly, because loser will have to move once
    // before the game realizes that the player has lost, but this will work for now
    // this should be called before the turn is changed
    // You should be checking if the other person (not the current player) 
    // has lost, but for now
    // we are checking if the opposite player has lost, who just took their turn
    public int checkWinConditionsForAll()
    {
        List<Piece> necessaryWhitePieces = gameBoard_.necessaryWhitePieces;
        List<Piece> necessaryBalckPieces = gameBoard_.necessaryBlackPieces;

        int currentPlayer = gameBoard_.currentPlayer;

        //Check if the current player, white, has lost
        if (currentPlayer == 0)
        {
            bool whiteLost = checkIfPlayerLost(necessaryWhitePieces, 0);
            if (whiteLost == true) return 1;
        }
        //Check if the current player, black, has lost
        else if (currentPlayer == 1)
        {
            bool blackLost = checkIfPlayerLost(necessaryBalckPieces, 1);
            if (blackLost == true) return 0;
        }

        //If the program has reached this point, neither has lost
        return -1;

        /*bool whiteLost = checkIfPlayerLost(necessaryWhitePieces, currentPlayer);
        bool blackLost = checkIfPlayerLost(necessaryBalckPieces, currentPlayer);

        if (whiteLost)
            return 1;
        else if (blackLost)
            return 0;
        else
            return -1;*/
    }

    bool checkIfPlayerLost(List<Piece> necessaryPieces, int playerTurn)
    {
        bool lost = true;
        foreach (Piece piece in necessaryPieces)
        {
            Tile myTile = getTileAtWorldCoord(piece.Position_);
            Vector2 myIndex = getIndexOfTile(myTile);

            if (piece.Captured == false)
            {
                if (playerTurn == 0)
                {
                    if (gameBoard_.blackAttackersAtTile[(int)myIndex.x, (int)myIndex.y] == 0)
                        lost = false;
                }
                else if (playerTurn == 1)
                {
                    if (gameBoard_.whiteAttackersAtTile[(int)myIndex.x, (int)myIndex.y] == 0)
                        lost = false;
                }
                
            }
        }

        return lost;
    }


    public void endGame()
    {
        playerHasWon = true;
        Debug.Log("Ending Game");
    }
}
