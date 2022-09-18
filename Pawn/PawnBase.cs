using System.Collections.Generic;
using UnityEngine;

public class PawnBase : MonoBehaviour
{
    public struct PawnStat
    {
        public int Index;
        public int TypeIndex;
        public string Name;
        public int Level;
        public int NowHp;
        public int MaxHp;
        public int Atk;
        public int Def;
        public int SpecAtk;
        public int SpecDef;
        public int Speed;
        public int ProvideExp;
        public int NowExp;
        public int MaxExp;

        public int CalcNowHP(PawnStat attacker, int power)
        {
            //데미지 계산
            float CalcStatValue = ((float)((Level * 2) + 10) / 250) * ((float)attacker.Atk / Def);
            int damage = Mathf.RoundToInt((CalcStatValue) * (power + 2) * Random.Range(0.85f, 1f));

            //HP 반영
            NowHp = (NowHp - damage) <= 0 ? 0 : (NowHp - damage);
            return damage;
        }
    }
    public struct PawnSkill
    {
        //Basic Info
        public int UserTypeIndex;
        public string AnimationIndex;
        public string SKillName;
        public int TargetNumber;
        public TargetType TargetPawnType;
        public float Range;
        public eSkillCategory SkillCategory;
        public eAffectStat AffectStat;
        public int PowerPerHit;
        public int Accuracy;
        public eEffectType EffectType;
        public GameObject EffectPrefab;
        public string SkillExplain;

        //Trigger Condition
        public float NowCoolTime;
        public float CondTime;

        public eRefStat Cond_RefStat;
        public eRefPawnType Cond_RefPawn;
        public float Cond_RefValue;
        public eCompOper CondHP_Compare;
        //[발동 조건] 스킬 조건 확인
        public bool Check_Condition(PawnStat _stat)
        {
            //쿨타임 체크
            if (CoolTimeEnd() == false)
            {
                return false;
            }

            //스탯 조건 체크
            if (Check_CondStat(_stat) == false)
            {
                return false;
            }


            return true;
        }

        //[발동 조건] 쿨타임 확인
        public bool CoolTimeEnd()
        {
            if (NowCoolTime >= CondTime)
            {
                return true;
            }
            else
            {
                NowCoolTime += Time.deltaTime;
                return false;
            }
        }
        public float RemainCoolTime()
        {
            return CondTime - NowCoolTime;
        }
        public void ResetCoolTime()
        {
            NowCoolTime = 0;
        }
        public void UpdateCoolTime()
        {
            NowCoolTime += Time.deltaTime;
        }

        //[발동 조건] 스탯 기준일 경우 확인
        public bool Check_CondStat(PawnStat stat)
        {
            if (Cond_RefStat == eRefStat.NOWHP)
            {
                switch (CondHP_Compare)
                {
                    case eCompOper.OVER:
                        return ((stat.NowHp / stat.MaxHp) > Cond_RefValue) ? true : false;
                    case eCompOper.AND_OVER:
                        return ((stat.NowHp / stat.MaxHp) >= Cond_RefValue) ? true : false;
                    case eCompOper.SAME:
                        return ((stat.NowHp / stat.MaxHp) == Cond_RefValue) ? true : false;
                    case eCompOper.AND_UNDER:
                        return ((stat.NowHp / stat.MaxHp) <= Cond_RefValue) ? true : false;
                    case eCompOper.UNDER:
                        return ((stat.NowHp / stat.MaxHp) < Cond_RefValue) ? true : false;
                }
            }

            return false;
        }

        //스킬이 발동된 후 발동 조건을 초기화
        public void Reset()
        {
            NowCoolTime = 0;
        }
    }

    //기본 정보 (외부 입력 처리)
    [SerializeField] public Defines.ePawnType type;
    [SerializeField] public int Index;

    public PawnStat Stat { get; private set; }
    public PawnSkill Skill { get; private set; }
    public PawnBase TargetPawn { get; protected set; }

    //상태
    public bool IsDead { get; protected set; }
    protected Defines.eAct nowAct;

    //입력
    public Defines.eAct inputAct;

    //공격, 피격
    public int comboIndex;
    public Collider2D hitBox;
    private int compLayerMask;
    public float atkDist = 1.5f;
    public float actElapsedTime;
    public float actDelayTime;

    //스킬
    protected List<PawnBase> skillTargets = new List<PawnBase>();

    //이동    //이동 좌표 (x,y)
    public float _mx { get; protected set; }
    public float _my { get; protected set; }
    public Vector3 inputDir;
    public float MoveSpeed = 8f;

    //애니메이션
    public Animator animCtrl;
    public bool canPlay;    //다른 액션 애니메이션 재생 가능한지 여부
    public bool isLeft;     //바라보는 방향을 계속 유지하기 위해 필요

    private void Awake()
    {
        Init(type, Index);
    }

    public virtual void Init(Defines.ePawnType _type, int _index)
    {
        _type = type;
        if (type == Defines.ePawnType.Player
        || type == Defines.ePawnType.Fellow)
        {
            compLayerMask = 1 << LayerMask.NameToLayer("Monsters");
        }
        else
        {
            compLayerMask = 1 << LayerMask.NameToLayer("Players");
        }

        animCtrl = transform.GetComponent<Animator>();
        hitBox = transform.GetComponent<Collider2D>();

        Index = _index;
        Stat = DataTableManager.Init_PawnStat(_index);
        Skill = DataTableManager.Init_PawnSkill(_index);

        canPlay = true;
        comboIndex = 0;
        actElapsedTime = actDelayTime;

        IsDead = false;
        canPlay = true;
    }
    public void InputAction(Defines.eAct _act)
    {
        inputAct = _act;
    }
    protected bool IsTargetInAtkArea(PawnBase _target)
    {
        Vector3 _origin = transform.position + new Vector3(hitBox.offset.x, hitBox.offset.y, transform.position.z);
        Vector3 _dir = (transform.position.x < _target.transform.position.x) //타겟 위치 > 좌우 판단
                         ? Vector3.left : Vector3.right;

        int lMask = 0;
        if (type == Defines.ePawnType.Fellow)
        {
            lMask = 1 << LayerMask.NameToLayer("Monsters");
        }
        else if (type == Defines.ePawnType.Monster)
        {
            lMask = 1 << LayerMask.NameToLayer("Players");
        }

        RaycastHit2D[] _hit = Physics2D.RaycastAll(_origin, _dir, atkDist, lMask);
        for (int i = 0; i < _hit.Length; i++)
        {
            if (_hit[i].transform.GetComponent<PawnBase>() == _target)
            {
                return true;
            }
        }

        return false;
    }

    //For AI
    //레이캐스트를 쐈을 때 >> Target이 있는지 확인
    protected Defines.eAct AI_SelectAct()
    {
        if (TargetPawn == null)
        {
            return Defines.eAct.IDLE;
        }

        //스킬 사용 조건(1) 스탯 관련
        if (Skill.Check_Condition(Stat))                 
        {
            //스킬 사용 조건(2) 대상 수
            skillTargets = SkillManager.TargetCount(this);
            if (skillTargets.Count > 0)                 
            {
                return Defines.eAct.SKILL;
            }
        }

        //일반 공격 > 대상이 범위에 있는지 Raycast로 판별
        else if (IsTargetInAtkArea(TargetPawn))
        {
            if (actElapsedTime >= actDelayTime)
            {
                return Defines.eAct.ATTACK;
            }
            else
            {
                return Defines.eAct.IDLE;
            }
        }

        return Defines.eAct.MOVE;
    }
    protected void AI_Act(Vector3 _goalPos)
    {
        switch (inputAct)
        {
            case Defines.eAct.MOVE:
                Vector3 vec = (_goalPos - transform.position).normalized;
                transform.position += vec * MoveSpeed * Time.deltaTime;
                break;
            default:
                //이외는 애니메이션 태그로 처리
                break;
        }
    }
    protected RaycastHit2D[] CheckCompInAtkArea(Vector3 _pos, Vector2 _dir)
    {
        Vector2 _posCenter = new Vector2(_pos.x, _pos.y) + new Vector2(hitBox.offset.x, hitBox.offset.y);

        int lMask = -1;
        if (type == Defines.ePawnType.Player || type == Defines.ePawnType.Fellow)
        {
            lMask = 1 << LayerMask.NameToLayer("Monsters");
        }
        else if (type == Defines.ePawnType.Monster)
        {
            lMask = 1 << LayerMask.NameToLayer("Players");
        }

        return Physics2D.RaycastAll(_posCenter, _dir, atkDist, lMask);
    }
    public void Reset_Target()
    {
        TargetPawn = null;
    }


    //탄젠트를 사용하여 이동 거리 통일
    public float GetNormalDirection(Vector2 _start, Vector2 _end)
    {
        Vector2 dir = _end - _start;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    //애니메이션 재생
    protected void AnimPlay()
    {
        //애니메이션 중복 호출을 막는다.
        if (!canPlay)
        {
            return;
        }

        //애니메이션 태그 설정
        if (_mx < 0)
        {
            isLeft = true;
        }
        else if (_mx > 0)
        {
            isLeft = false;
        }
        string dir = (isLeft) ? "Left" : "Right";
        string tag = string.Format($"{Stat.Name}-{nowAct}-{dir}");

        switch (inputAct)
        {
            case Defines.eAct.IDLE:
            case Defines.eAct.MOVE:
                break;
            case Defines.eAct.ATTACK:
                canPlay = false;
                tag += string.Format("-{0}", comboIndex);
                break;
            case Defines.eAct.SKILL:
                canPlay = false;
                tag += "-" + Skill.AnimationIndex;
                break;
            case Defines.eAct.HIT:
            case Defines.eAct.DEAD:
                break;
        }

        animCtrl.Play(tag); //애니메이션 재생 호출
        nowAct = inputAct;  //재생 후 상태값 변경
    }


    //## 애니메이션 이벤트 태그로 걸 함수
    public void AnimEvent_NormalAtk()
    {
        IngameManager.CalcDamage(this, TargetPawn, true);
    }
    public void AnimEvent_NextCombo()
    {
        comboIndex = (comboIndex + 1) % 3;
        actElapsedTime = (comboIndex == 0) ? 0 : actDelayTime;
        canPlay = true;
    }
    public void AnimEvent_UseAtkSkill()
    {
        SkillManager.UseAtkSkill(this, skillTargets);
    }
    public void AnimEvent_UseSpawnSkill()
    {
        SkillManager.UseSpawnSkill(this);
    }
    public void AnimEvent_ResetState()
    {
        if (nowAct == Defines.eAct.SKILL)
        {
            Skill.Reset();
        }

        canPlay = true;
        inputAct = Defines.eAct.IDLE;
    }
    public void AnimEvent_Dead()
    {
        IsDead = true;

        //페이드 아웃
        UnityEngine.UI.Image _pawnColor = transform.GetComponent<UnityEngine.UI.Image>();
        float _fade = 1;
        while (_fade >= 0)
        {
            _pawnColor.color = new Color(1f, 1f, 1f, _fade);
            _fade -= Time.deltaTime;
        }
        gameObject.SetActive(false);
    }
}