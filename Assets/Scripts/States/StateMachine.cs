using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private GameState currentState;

    public void ChangeState(GameState newState, GameManager gameManager)
    {
        if (currentState != null)
        {
            currentState.Exit(gameManager);
        }

        currentState = newState;
        currentState.Enter(gameManager);
    }

    public void Update(GameManager gameManager)
    {
        if (currentState != null)
        {
            currentState.Update(gameManager);
        }
    }
}

