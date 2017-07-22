using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour 
{

    GameController gameController;
    Preferences newPrefs;

    public Button onHighlighting;
    public Button offHighlighting;
    public Button onMovement;
    public Button offMovement;
    public Button themeStandard;
    public Button themeRealistic;

    public Button saveChangesButton;


    public void startOptionsMenu(Preferences currentPrefs, GameController gameController)
    {
        newPrefs = currentPrefs;
        this.gameController = gameController;

        onHighlighting.onClick.AddListener(turnOnHighlighting);
        offHighlighting.onClick.AddListener(turnOffHighlighting);
        onMovement.onClick.AddListener(turnOnMovement);
        offMovement.onClick.AddListener(turnOffMovement);
        themeStandard.onClick.AddListener(changeThemeStandard);
        themeRealistic.onClick.AddListener(changeThemeRealistic);
        saveChangesButton.onClick.AddListener(saveChanges);
    }

    void turnOnHighlighting()
    {
        newPrefs.highlightTiles = true;
    }

    void turnOffHighlighting()
    {
        newPrefs.highlightTiles = false;
    }

    void turnOnMovement()
    {
        newPrefs.freePieceMovement = true;
    }

    void turnOffMovement()
    {
        newPrefs.freePieceMovement = false;
    }

    void changeThemeStandard()
    {
        newPrefs.theme = "Standard";
    }

    void changeThemeRealistic()
    {
        newPrefs.theme = "Realistic";
    }

    void saveChanges()
    {
        gameController.myPrefs = newPrefs;
    }
}
