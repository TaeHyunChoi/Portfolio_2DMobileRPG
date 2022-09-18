using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.IO;

//데이터 테이블 인덱스
public enum eSheet_Pawn
{
    Pawn,
    Skill
}
public class DataTableManager : TSingleton<DataTableManager>
{
    //테이블 정보이므로 static으로 설정
    private static Dictionary<eSheet_Pawn, LowBase> PawnSheet;      //캐릭터별, 레벨별 능력치
    private static Dictionary<eSheet_Pawn, LowBase> SkillSheet;     //캐릭터별 스킬 정보

    private void Awake()
    {
        base.Init();
    }

    //[불러오기] json 파일에서 정보 불러오기. 개별 sheet의 내용도 함께 저장.
    public void Init_LoadSheet()
    {
        PawnSheet = new Dictionary<eSheet_Pawn, LowBase>();
        LoadSheet_Pawn<PawnDataTable>(eSheet_Pawn.Pawn);

        SkillSheet = new Dictionary<eSheet_Pawn, LowBase>();
        LoadSheet_Skill<SkillDataTable>(eSheet_Pawn.Skill);
    }
    private LowBase LoadSheet_Pawn<T>(eSheet_Pawn _info) where T : LowBase, new()
    {
        if (PawnSheet.ContainsKey(_info))
        {
            LowBase lBase = PawnSheet[_info];
            return lBase;
        }
        TextAsset TextAsset = Resources.Load("Json/" + _info.ToString()) as TextAsset;
        if (TextAsset != null)
        {
            T tLow = new T();
            tLow.Load(TextAsset.text);
            PawnSheet.Add(_info, tLow);
        }
        return PawnSheet[_info];
    }
    private LowBase LoadSheet_Skill<T>(eSheet_Pawn _info) where T : LowBase, new()
    {
        if (SkillSheet.ContainsKey(_info))
        {
            LowBase lBase = SkillSheet[_info];
            return lBase;
        }
        TextAsset TextAsset = Resources.Load("Json/" + _info.ToString()) as TextAsset;
        if (TextAsset != null)
        {
            T tLow = new T();
            tLow.Load(TextAsset.text);
            SkillSheet.Add(_info, tLow);
        }
        return SkillSheet[_info];
    }

    //[가져오기] json에서 저장한 정보를 게임으로 가져오기
    public static LowBase GetTableInfo(eSheet_Pawn _info)
    {
        if (PawnSheet.ContainsKey(_info))
        {
            return PawnSheet[_info];
        }
        else if (SkillSheet.ContainsKey(_info))
        {
            return SkillSheet[_info];
        }

        return null;
    }
    public static PawnBase.PawnStat Init_PawnStat(int _index)
    {
        PawnBase.PawnStat _stat = new PawnBase.PawnStat();

        _stat.Index = _index;
        _stat.TypeIndex = _index / 100;
        _stat.Level = _index % 100;

        _stat.Name = GetTableInfo(eSheet_Pawn.Pawn).ToString(_index, eTableIndex_Pawn.Name.ToString());

        _stat.MaxHp = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.MaxHp.ToString());

        _stat.Atk = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.Atk.ToString());
        _stat.Def = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.Def.ToString());
        _stat.SpecAtk = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.SpecAtk.ToString());
        _stat.SpecDef = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.SpecDef.ToString());

        _stat.Speed = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.Speed.ToString());

        _stat.ProvideExp = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.ProvideExp.ToString());
        _stat.MaxExp = GetTableInfo(eSheet_Pawn.Pawn).ToInteger(_index, eTableIndex_Pawn.MaxExp.ToString());

        return _stat;
    }
    public static PawnBase.PawnSkill Init_PawnSkill(int _index)
    {
        PawnBase.PawnSkill _skill = new PawnBase.PawnSkill();
        _skill.UserTypeIndex = _index / 100;
        _skill.AnimationIndex = GetTableInfo(eSheet_Pawn.Skill).ToString(_index, eTableIndex_Skill.AnimeIndex.ToString());
        _skill.SKillName = GetTableInfo(eSheet_Pawn.Skill).ToString(_index, eTableIndex_Skill.SkillName.ToString());
        _skill.TargetNumber = GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.TargetNumber.ToString());
        _skill.TargetPawnType = (TargetType)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.TargetType.ToString());
        _skill.Range = GetTableInfo(eSheet_Pawn.Skill).ToFloat(_index, eTableIndex_Skill.Range.ToString());
        _skill.SkillCategory = (eSkillCategory)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.SkillCategory.ToString());
        _skill.AffectStat = (eAffectStat)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.AffectStat.ToString());
        _skill.PowerPerHit = GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.PowerPerHit.ToString());
        _skill.Accuracy = GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.Accuracy.ToString());
        _skill.EffectType = (eEffectType)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.EffectType.ToString());
        string EffectRscName = GetTableInfo(eSheet_Pawn.Skill).ToString(_index, eTableIndex_Skill.EffectRscName.ToString()); //얘는 GameObject 걸어야 함
        _skill.EffectPrefab = Resources.Load("SkillEffects/" + EffectRscName) as GameObject;
        _skill.SkillExplain = GetTableInfo(eSheet_Pawn.Skill).ToString(_index, eTableIndex_Skill.Explain.ToString());

        _skill.NowCoolTime = 0;
        _skill.CondTime = GetTableInfo(eSheet_Pawn.Skill).ToFloat(_index, eTableIndex_Skill.CondTime.ToString());

        _skill.Cond_RefStat = (eRefStat)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.CondRefStat.ToString());
        _skill.Cond_RefPawn = (eRefPawnType)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.CondRefType.ToString());
        _skill.Cond_RefValue = GetTableInfo(eSheet_Pawn.Skill).ToFloat(_index, eTableIndex_Skill.CondRefType.ToString());
        _skill.CondHP_Compare = (eCompOper)GetTableInfo(eSheet_Pawn.Skill).ToInteger(_index, eTableIndex_Skill.CondRefType.ToString());
    
        return _skill;
    }
}
