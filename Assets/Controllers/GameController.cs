using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public enum PieceTypes { Pawn, Knight, Bishop, Rook, Queen, King }

    BoardController boardController;
    MouseController mouseController;

    enum SceneState { LOADING, PLAYING, MENU }
    SceneState currentState = SceneState.PLAYING;

    string targetLevel;

    public Preferences myPrefs;
    BoardInfo boardInfo;

    List<string> BoardThemes;
    public Dictionary< string, Dictionary<string, Sprite> > themeToSpriteForPiece;
    public Dictionary< string, Dictionary<string, Sprite> > tileToSpriteForTheme;
    public Dictionary< string, Sprite> backgroundForTheme;
    public Dictionary<string, Sprite> boardForTheme;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void Start()
    {
        themeToSpriteForPiece = new Dictionary<string, Dictionary<string, Sprite>>();
        tileToSpriteForTheme = new Dictionary<string, Dictionary<string, Sprite>>();
        backgroundForTheme = new Dictionary<string, Sprite>();
        boardForTheme = new Dictionary<string, Sprite>();

        setupDefaultPrefs();

        loadAllAssets();

        currentState = SceneState.MENU;
        onMainLoaded();
    }

    void Update()
    {
        //Debug.Log(myPrefs.freePieceMovement);

        if (currentState == SceneState.PLAYING)
        {
            mouseController.update();
        }
        else if (currentState == SceneState.LOADING)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // Extend within this if statement to 
            // account for each possible level
            if (currentSceneName == targetLevel)
            {
                if (currentSceneName == "Arena")
                    onArenaLoaded();
                if (currentSceneName == "Options")
                    onOptionsLoaded();
                if (currentSceneName == "Main")
                    onMainLoaded();
            }
        }
    }


    public void startLoadingLevel(string levelName)
    {
        currentState = SceneState.LOADING;
        StartCoroutine(loadLevel(levelName));
        targetLevel = levelName;
    }

    void setupDefaultPrefs()
    {
        boardInfo = new BoardInfo(new Vector2(8, 8), 8, 8);
        myPrefs = new Preferences();

        /*boardInfo.boardSize = new Vector2(8, 8);
        boardInfo.boardColumns = 8;
        boardInfo.boardRows = 8;*/

        myPrefs.theme = "Standard"; 
        myPrefs.highlightTiles = true;
        myPrefs.freePieceMovement = true;
    }


    void loadAllAssets()
    {
        string[] pieceNames = { "WhitePawn", "WhiteKnight", "WhiteBishop", "WhiteRook",
                                "WhiteQueen", "WhiteKing", "BlackPawn", "BlackKnight",
                                "BlackBishop", "BlackRook", "BlackQueen", "BlackKing"};
        foreach (string pieceName in pieceNames)
        {
            themeToSpriteForPiece.Add(pieceName, returnThemeToSpriteForType(pieceName, "Pieces"));
        }
        

        List<string> themeNames = returnAllThemeNames();
        foreach (string theme in themeNames)
        {
            tileToSpriteForTheme.Add(theme, loadAssetsFromFolder(theme, "Tiles"));

            Sprite background = Resources.Load("Sets/" + theme + "/" + "Background") as Sprite;
            backgroundForTheme.Add(theme, background);

            string boardPath = "Sets/" + theme + "/" + "Board";
            if (File.Exists(boardPath))
            {
                Sprite board = Resources.Load("Sets/" + theme + "/" + "Board") as Sprite;
                boardForTheme.Add(theme, board);
            }
        }
    }

    Dictionary<string, Sprite> returnThemeToSpriteForType(string spriteName, string type)
    {
        List<string> themeNames = returnAllThemeNames();

        // For each name in the directory, add to the dictionary
        Dictionary<string, Sprite> themeToSprite = new Dictionary<string, Sprite>();

        foreach (string themeName in themeNames)
        {
            Object mySprite = Resources.Load("Sets/" + themeName 
                                + "/" + type + "/" + spriteName, typeof(Sprite));
            themeToSprite.Add(themeName, (Sprite)mySprite);
        }

        return themeToSprite;
    }

    List<string> returnAllThemeNames()
    {
        List<string> themeNames = new List<string>();

        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Resources/Sets/");
        DirectoryInfo[] info = dir.GetDirectories("*.*");
        foreach (DirectoryInfo directory in info)
        {
            themeNames.Add(directory.Name);
        }

        return themeNames;
    }


    Dictionary<string, Sprite> loadAssetsFromFolder(string folder, string type)
    {
        Dictionary<string, Sprite> pieceNameToSprite = new Dictionary<string, Sprite>();
        Object[] pieceSprites = Resources.LoadAll("Sets/" + folder + "/" + type, typeof(Sprite));

        foreach (Object sprite in pieceSprites)
        {
            pieceNameToSprite.Add(sprite.name, (Sprite)sprite);
        }

        return pieceNameToSprite;
    }


    void onArenaLoaded()
    {
        currentState = SceneState.PLAYING;

        mouseController = new MouseController();

        boardController = new BoardController();
        boardController.beginChessGame(boardInfo);
    }

    // If the current scene is for options,
    // then change boardInfo and myPrefs accordingly
    void saveOptions()
    {

    }

    void onOptionsLoaded()
    {
        currentState = SceneState.MENU;

        OptionsHandler optionsHandler = 
            GameController.FindObjectOfType<OptionsHandler>();
        optionsHandler.startOptionsMenu(myPrefs, this);

    }

    void onMainLoaded()
    {
        /*Vector2 size = new Vector2();
        float orthoSize = Camera.main.orthographicSize;
        float aspect = Camera.main.aspect;

        if (aspect >= 1)
            size = new Vector2(2 * orthoSize, 2 * orthoSize);
        else
            size = new Vector2(2 * orthoSize * aspect, 2 * orthoSize * aspect);
        
        BoardInfo menuBoardInfo = new BoardInfo(2 * size, 16, 16);

        boardController = new BoardController();
        boardController.beginChessGame(menuBoardInfo);*/
    }

    public void quit()
    {
        Application.Quit();
    }

    IEnumerator loadLevel(string levelName)
    {

        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }
    }
}


