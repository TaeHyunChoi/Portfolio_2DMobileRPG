using UnityEngine;

public static class Defines
{
    public enum eGameState
    {
        Usual,
        Battle
    }
    public enum ePawnType
    {
        Player = 0,
        Fellow,
        Monster
    }
    public enum eAct
    { 
        IDLE = 0,
        MOVE,
        ATTACK,
        SKILL,
        HIT,
        DEAD
    }
    public enum AttakType
    { 
        NORMAL = 0,
        SKILL = 1
    }
    public enum InputDirection
    {
        UP = 0,
        DOWN,
        LEFT,
        RIGHT
    }
}
