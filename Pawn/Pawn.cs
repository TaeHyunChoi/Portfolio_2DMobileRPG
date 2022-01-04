using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    #region 필드
    //Other class
    protected BattleManager _bttlMNG;
    protected Animator _animeCtrl;

    //From DataTable
    public delegate LowBase GetFromTable(eSheet_Pawn info);
    public GetFromTable _dbGetData;

    //Pawn Info, Stats
    [SerializeField] protected int _pawnIndex;
    public int PawnIndex { get { return _pawnIndex; } }
    [SerializeField] protected int _level;
    public int PawnLevel { get { return _level; } }

    [SerializeField] protected PublicDefines.ePawnType _type;
    public PublicDefines.ePawnType PawnType { get { return _type; } }
    public PawnStats Stats;

    //Pawn State
    public PublicDefines.NowAction NowAct { get; set; }
    public bool NowHit { get; set; }
    public bool IsDead { get; set; }

    //Animation
    public PublicDefines.NowAction InputState { get; set; }
    protected bool _isLeft;
    public bool IsLeft { get { return _isLeft; } }

    //Sound
    public delegate GameObject SoundCtrl(EffectSoundIndex type, bool mute = false, bool isLoop = false);
    public SoundCtrl PlaySound { get; set; }

    //Skills
    public SkillInfos _skill;
    public bool InputSkill { get; set; }

    //Normal Attack
    public int NowCombo { get; set; }
    public bool Targeting { get; set; }
    protected Collider2D _hitBox;
    [SerializeField] float _atkDist = 1.5f;
    protected float AtkDist { get { return _atkDist; } }
    protected float _nowTime_Attack { get; set; }
    protected float _atkDelayTime = 2.5f;

    //Move
    protected Vector3 _inputDir;
    protected float _moveSpeed = 8f;                            //슬쩍 건드려볼까
    public float MoveSpeed { get { return _moveSpeed; } }
    public Queue<Vector3> _followPath { get; set; }
    #endregion

    //[초기화] 능력치, 스킬 정보
    protected void InitPawn_Stat(int index)
    {
        string name = _dbGetData(eSheet_Pawn.Pawn).ToString(index, eTableIndex_Pawn.Name.ToString());
        int level = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.Level.ToString());
        int maxHP = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.MaxHp.ToString());
        int atk = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.Atk.ToString());
        int def = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.Def.ToString());
        int spAtk = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.SpecAtk.ToString());
        int spDef = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.SpecDef.ToString());
        int speed = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.Speed.ToString());
        int pvEXP = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.ProvideExp.ToString());
        int maxEXP = _dbGetData(eSheet_Pawn.Pawn).ToInteger(index, eTableIndex_Pawn.MaxExp.ToString());

        Stats = new PawnStats(index, name, level, maxHP, atk, def, spAtk, spDef, speed, pvEXP, maxEXP);
    }
    protected void InitPawn_Skill(int index)
    {
        _skill = new SkillInfos();
        _skill.Index = index;
        _skill.AnimationIndex = _dbGetData(eSheet_Pawn.Skill).ToString(index, eTableIndex_Skill.AnimeIndex.ToString());
        _skill.SKillName = _dbGetData(eSheet_Pawn.Skill).ToString(index, eTableIndex_Skill.SkillName.ToString());
        _skill.TargetNumber = _dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.TargetNumber.ToString());
        _skill.TargetPawnType = (TargetType)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.TargetType.ToString());
        _skill.fRange = _dbGetData(eSheet_Pawn.Skill).ToFloat(index, eTableIndex_Skill.Range.ToString());
        _skill.SkillCategory = (SkillCategory)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.SkillCategory.ToString());
        _skill.AffectStat = (AffectStat)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.AffectStat.ToString());
        _skill.iPowerPerHit = _dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.PowerPerHit.ToString());
        _skill.iAccuracy = _dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.Accuracy.ToString());
        _skill.EffectType = (eEffectType)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.EffectType.ToString());
        string EffectRscName = _dbGetData(eSheet_Pawn.Skill).ToString(index, eTableIndex_Skill.EffectRscName.ToString()); //얘는 GameObject 걸어야 함
        _skill.PrbEffect = Resources.Load("SkillEffects/" + EffectRscName) as GameObject;
        _skill.SkillExplain = _dbGetData(eSheet_Pawn.Skill).ToString(index, eTableIndex_Skill.Explain.ToString());

        _skill.fNowCoolTime = 0;
        _skill.fCondTime = _dbGetData(eSheet_Pawn.Skill).ToFloat(index, eTableIndex_Skill.CondTime.ToString());

        _skill.Cond_RefStat = (RefStat)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.CondRefStat.ToString());
        _skill.Cond_RefPawn = (RefPawnType)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.CondRefType.ToString());
        _skill.Cond_fRefValue = _dbGetData(eSheet_Pawn.Skill).ToFloat(index, eTableIndex_Skill.CondRefType.ToString());
        _skill.CondHP_Compare = (eCompOper)_dbGetData(eSheet_Pawn.Skill).ToInteger(index, eTableIndex_Skill.CondRefType.ToString());

        _skill.Meet_CoolTime = _dbGetData(eSheet_Pawn.Skill).ToBool(index, eTableIndex_Skill.MeetCondCool.ToString());
        _skill.Meet_Condition = _dbGetData(eSheet_Pawn.Skill).ToBool(index, eTableIndex_Skill.MeetCondHP.ToString());
        _skill.PossibleUse = _dbGetData(eSheet_Pawn.Skill).ToBool(index, eTableIndex_Skill.PossibleUse.ToString());

        InputSkill = false;
    }

    //[행동 입력] 다른 액션 애니메이션 재생 가능한지 여부
    protected bool bCanPlayNewAnime;

    //[행동 입력] 내부 알고리즘이 아닌 외부에서 입력을 넣었는지 여부
    protected bool bInputExternal;


    //[이동] 탄젠트를 사용하여 이동 간격(거리)을 통일함.
    protected float GetAngle(Vector2 start, Vector2 end)
    {
        Vector2 v2 = end - start;
        return Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
    }

    //[이동] 다음 캐릭터가 경로를 따라올 수 있도록 위치 정보를 Queue에 전달
    protected void FellowGetMoveDelta(PawnFellower fellower, Vector3 moveDelta)
    {
        fellower._followPath.Enqueue(moveDelta);
    }

    //[행동] 행동을 마친 후 대기 상태로 전환 및 관련 정보 초기화
    protected virtual void Reset_NowAction()    
    {
        bCanPlayNewAnime = true;
        InputState = PublicDefines.NowAction.IDLE;

        if (NowAct == PublicDefines.NowAction.SKILL)
        {
            _skill.Reset();
            _bttlMNG._skillTargets.Clear();
        }
    }

    //[행동] 애니메이션 이벤트 : 캐릭터 사망 시 처리
    public void InAnime_Dead()  
    {
        IsDead = true;

        //인게임이 갖고 있던 캐릭터 리스트에서 정보 삭제
        if (PawnType != PublicDefines.ePawnType.Monster)
        {
            IngameManager._instance._ltPartyPawns.Remove(this);
        }
        else if (PawnType == PublicDefines.ePawnType.Monster)
        {
            PawnMonster monster = this.transform.GetComponent<PawnMonster>();
            monster.DropItems();
            IngameManager._instance._ltMonstersInRoom.Remove(this);
        }

        //애니메이션 끝났으니 없앱시다.
        transform.gameObject.SetActive(false);
    }


    //[스킬] 스킬 타겟 설정
    public virtual void SetSkillTargets(SkillInfos skill)
    {
        if (PawnType != PublicDefines.ePawnType.Monster) //플레이어 파티라면
        {
            switch (skill.SkillCategory)
            {
                case SkillCategory.Attack:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltMonstersInRoom);
                    break;
                case SkillCategory.Heal:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltPartyPawns);
                    break;
                case SkillCategory.Buff:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltPartyPawns);
                    break;
                case SkillCategory.Calling:
                    //애니메이션으로 빼서 타이밍 맞춤.
                    break;
            }
        }
        else //몬스터 파티 소속이라면
        {
            switch (skill.SkillCategory)
            {
                case SkillCategory.Attack:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltPartyPawns);
                    break;
                case SkillCategory.Heal:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltMonstersInRoom);
                    break;
                case SkillCategory.Buff:
                    _bttlMNG._skillTargets = _bttlMNG._skCheckSkillTargets(this, skill, IngameManager._instance._ltMonstersInRoom);
                    break;
                case SkillCategory.Calling:
                    //애니메이션으로 빼서 타이밍 맞추는게 좋겠다.
                    break;
            }
        }

        if (_bttlMNG._skillTargets.Count > 0)
        {
            InputState = PublicDefines.NowAction.SKILL;
        }
    }

    //[스킬] 각 Pawn에서 대상Pawn을 받아서 실행시킨다.
    protected virtual void InAnime_PlaySkillEffect()
    {
        
    }

    //[스킬] 다른 캐릭터(Pawn)을 소환할 때의 명령어
    protected virtual void CallSpawn_InAnime()
    {
        _bttlMNG._skCallOtherPawns(this, _skill);
    }


    //[공격] 공격 범위에 상대가 있는지, 얼마나 있는지 확인
    protected RaycastHit2D[] CheckCompInAtkArea(bool left)
    {
        Vector2 _posCenter = new Vector2(transform.position.x, transform.position.y) + new Vector2(_hitBox.offset.x, _hitBox.offset.y);
        Vector2 dir = left ? Vector2.left : Vector2.right;

        int lMask = -1;
        if (_type == PublicDefines.ePawnType.Player || _type == PublicDefines.ePawnType.Fellow1 || _type == PublicDefines.ePawnType.Fellow2)
        {
            lMask = 1 << LayerMask.NameToLayer("Monsters");
        }
        else if (_type == PublicDefines.ePawnType.Monster)
        {
            lMask = 1 << LayerMask.NameToLayer("Players");
        }

        return Physics2D.RaycastAll(_posCenter, dir, AtkDist, lMask);
    }

    //[공격] 근접 공격 시 Raycast로 상대 확인 및 데미지 계산
    protected void CalcDamageByRaycast(PublicDefines.AttakType UseSkill)
    {
        RaycastHit2D[] hitbox = CheckCompInAtkArea(_isLeft);
        for (int n = 0; n < hitbox.Length; n++)
        {
            BattleManager._instance.CalcDamage(this, hitbox[n].collider.transform.GetComponent<Pawn>(), UseSkill);
        }
    }

    //[공격] 원거리 공격 시 스킬 이펙트에 있는 Trigger로 상대 확인 및 데미지 계산
    public void CalcDamageByTrigger(Pawn atk, Pawn hit, PublicDefines.AttakType UseSkill)
    {
        BattleManager._instance.CalcDamage(atk, hit, UseSkill);
    }


    //[효과음]
    protected virtual void InAnime_PlaySound_Effect(EffectSoundIndex type)  //애니메이션 이벤트. 효과음 재생
    {
        PlaySound(type);
    }
}
