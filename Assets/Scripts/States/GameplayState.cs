using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayState : GameState
{
    public override void Enter(GameManager gameManager)
    {
        AsyncScenesManager asyncScenesManager = ServiceLocator.Instance.GetService<AsyncScenesManager>();

        if (!asyncScenesManager.IsPermanentSceneLoaded())
        {
            asyncScenesManager.LoadPermanentSceneAsync();
        }

        asyncScenesManager.UnloadSceneAsync("Menu");

        if (!asyncScenesManager.IsLevelLoaded(gameManager.currentLevel))
        {
            gameManager.StartCoroutine(LoadGameLevel(gameManager, asyncScenesManager));
        }
        else
        {
            Debug.Log("Level already loaded, resuming gameplay");
            gameManager.audioManager.PlayBGM(1);
        }
    }

    public override void Exit(GameManager gameManager)
    {
        Debug.Log("Leaving game");
    }

    public override void Update(GameManager gameManager)
    {
        // ESC es manejado por GameConditionManager.Update(), no aquí
        // Comentado para evitar conflictos:
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     gameManager.ChangeGameStatus(new PauseState());
        // }
    }

    private IEnumerator LoadGameLevel(GameManager gameManager, AsyncScenesManager asyncScenesManager)
    {
        yield return null;

        asyncScenesManager.LoadNewLevel(gameManager.currentLevel);

        gameManager.audioManager.PlayBGM(1);

        Debug.Log("Joining game");
    }
}
