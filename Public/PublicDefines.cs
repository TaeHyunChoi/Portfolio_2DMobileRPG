using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PublicDefines
{
    public enum ePawnType
    {
        Player = 0,
        Fellow1,
        Fellow2,
        Monster
    }
    public enum NowAction
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
