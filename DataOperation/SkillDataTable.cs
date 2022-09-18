using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.IO;

public enum eTableIndex_Skill
{
    Index,
    AnimeIndex,
    SkillName,
    TargetNumber,
    TargetType,
    Range,
    SkillCategory,
    AffectStat,
    Duration,
    PowerPerHit,
    Hits,
    Accuracy,
    EffectType,
    EffectRscName,
    CondTime,
    CondRefStat,
    CondRefType,
    CondHPValue,
    CondHPCompOper,
    MeetCondCool,
    MeetCondHP,
    PossibleUse,
    Explain,
    max_cnt
}

//스킬 조건 확인을 위한 경우의 수
public enum eRefStat
{
    NONE = 0,
    INDEX,
    LEVEL,
    NOWHP,
    MAXHP,
    ATK,
    DEF,
    SPEC_ATK,
    SPEC_DEF,
    SPEED,
    PROVIDED_EXP,
    NOW_EXP,
    MAX_EXP
}
public enum TargetType
{
    SELF = 0,
    PLAYERS,
    MONSTERS,
    NONE
}
public enum eRefPawnType
{
    Self = 0,
    OurParty,
    Monsters,
    NONE
}
public enum eSkillCategory
{
    Attack,
    Heal,
    Buff,
    Calling
}
public enum eCompOper
{
    NOT_USED,
    OVER,
    AND_OVER,
    SAME,
    AND_UNDER,
    UNDER
}
public enum eEffectType
{
    None = 0,
    Place,
    Projectile,
    Front
}
public enum eAffectStat
{
    NowHp = 1,
    Atk = 2,
    Def = 4,
    SpecAtk = 8,
    SpecDef = 16,
    Speed = 32,
    ProvideExp = 64,
    NowExp = 128,
    Others
}

public class SkillDataTable : LowBase
{
    public string mainKey = "Index";
    public override void Load(string jsonData) 
    {
        JSONNode _node = JSONNode.Parse(jsonData);
        for (int n = 0; n < (int)eTableIndex_Skill.max_cnt; n++)
        {
            eTableIndex_Skill _subKey = (eTableIndex_Skill)n;
            if (string.Compare(mainKey, _subKey.ToString()) != 0)
            {
                for (int m = 0; m < _node[0].AsArray.Count; m++) //(엑셀 기준) 한 행씩 저장한다.
                {
                    Add(_node[0][m][mainKey], _subKey.ToString(), _node[0][m][_subKey.ToString()].Value);
                }
            }
        }
    }
}
