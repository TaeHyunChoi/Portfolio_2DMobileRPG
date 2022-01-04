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
public struct SkillInfos
{
    #region Field
    //Basic Info
    public int Index;                                
    public string AnimationIndex;                    
    public string SKillName;                         
    public int TargetNumber;                         
    public TargetType TargetPawnType;                
    public float fRange;                             
    public SkillCategory SkillCategory;              
    public AffectStat AffectStat;                    
    public int iPowerPerHit;                         
    public int iAccuracy;                            
    public eEffectType EffectType;                   
    public GameObject PrbEffect;                     
    public string SkillExplain;                      

    //Trigger Condition
    public float fNowCoolTime;                       
    public float fCondTime;                          

    public RefStat Cond_RefStat;                     
    public RefPawnType Cond_RefPawn;                 
    public float Cond_fRefValue;                     
    public eCompOper CondHP_Compare;                 

    public bool Meet_CoolTime;                       
    public bool Meet_Condition;                      
    public bool PossibleUse;                         
    #endregion
    #region Method
    //[발동 조건] 스킬 조건 확인
    public void Check_Condition(PawnStats stat)
    {
        Check_CoolTime(stat);
        Check_CondStat(stat);

        if (Meet_CoolTime && Meet_Condition)
        {
            PossibleUse = true;
        }
        else
        {
            PossibleUse = false;
        }
    }
    //[발동 조건] 쿨타임 확인
    public void Check_CoolTime(PawnStats stat)
    {
        if (fNowCoolTime >= fCondTime)
        {
            Meet_CoolTime = true;
        }
        else
        {
            fNowCoolTime += Time.deltaTime;
        }
    }

    //[발동 조건] 스탯 기준일 경우 확인
    public void Check_CondStat(PawnStats stat)
    {
        switch (Cond_RefStat)
        {
            case RefStat.NOWHP:
                switch (CondHP_Compare)
                {
                    case eCompOper.OVER:
                        Meet_Condition = ((stat.NowHp / stat.MaxHp) > Cond_fRefValue) ? true : false;
                        break;
                    case eCompOper.AND_OVER:
                        Meet_Condition = ((stat.NowHp / stat.MaxHp) >= Cond_fRefValue) ? true : false;
                        break;
                    case eCompOper.SAME:
                        Meet_Condition = ((stat.NowHp / stat.MaxHp) == Cond_fRefValue) ? true : false;
                        break;
                    case eCompOper.AND_UNDER:
                        Meet_Condition = ((stat.NowHp / stat.MaxHp) <= Cond_fRefValue) ? true : false;
                        break;
                    case eCompOper.UNDER:
                        Meet_Condition = ((stat.NowHp / stat.MaxHp) < Cond_fRefValue) ? true : false;
                        break;
                }
                break;
            default:
                Meet_Condition = true;
                break;
        }
    }

    //스킬이 발동된 후 발동 조건을 초기화
    public void Reset()
    {
        fNowCoolTime = 0;
        Meet_CoolTime = false;
        Meet_Condition = false;
        PossibleUse = false;
    }
    #endregion
}

//스킬 조건 확인을 위한 경우의 수
public enum RefStat
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
public enum RefPawnType
{
    Self = 0,
    OurParty,
    Monsters,
    NONE
}
public enum SkillCategory
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
public enum AffectStat
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
    public string _mainKey = "Index";
    public override void Load(string jsonData) 
    {
        JSONNode node = JSONNode.Parse(jsonData);
        for (int n = 0; n < (int)eTableIndex_Skill.max_cnt; n++)
        {
            eTableIndex_Skill subKey = (eTableIndex_Skill)n;
            if (string.Compare(_mainKey, subKey.ToString()) != 0)
            {
                for (int m = 0; m < node[0].AsArray.Count; m++) //(엑셀 기준) 한 행씩 저장한다.
                {
                    Add(node[0][m][_mainKey], subKey.ToString(), node[0][m][subKey.ToString()].Value);
                }
            }
        }
    }
}
