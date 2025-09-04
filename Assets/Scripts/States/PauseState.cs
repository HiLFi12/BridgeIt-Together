using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseState : GameState
{
    public override void Enter(GameManager gameManager)
    {
        Time.timeScale = 0f;
        gameManager.ActivePauseOverlay();
        Debug.Log("Pausing game");
    }

    public override void Exit(GameManager gameManager)
    {
        Time.timeScale = 1f;
        gameManager.DeactivatePauseOverlay();
        Debug.Log("Leaving pause");
    }

    public override void Update(GameManager gameManager)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.ChangeGameStatus(new GameplayState());
        }
    }
}

