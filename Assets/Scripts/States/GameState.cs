using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState
{
    public abstract void Enter(GameManager gameManager);
    public abstract void Exit(GameManager gameManager);
    public abstract void Update(GameManager gameManager);
}
