using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PawnAI : Pawn
{
    #region 필드
    private static PawnAI _uniqueInstance;
    public static PawnAI _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    protected IngameManager _ingameMNG;
    public Pawn _targetPawn;
    protected Vector3 _posGoal;

    private float _moveDist = 1.1f;
    public float MoveDist { get { return _moveDist; } }
    protected bool _nowMoving;

    protected float _actDelayTime;
    protected float _nowTime_Action;

    protected IEnumerator SceneBattle;
    [SerializeField] bool _modePuppet = false;
    protected bool PuppetMode { get { return _modePuppet; } }
    #endregion
    private void Awake()
    {
        _uniqueInstance = this;
        _dbGetData = DataTableManager._instance.GetTableInfo;

        bCanPlayNewAnime = true;
        bInputExternal = false;

        _nowTime_Attack = 0.5f;
        _actDelayTime = _nowTime_Action = 1.5f;
        _nowTime_Attack = _atkDelayTime;

        SceneBattle = Battle();
    }

    //Skill
    protected override void InAnime_PlaySkillEffect()
    {
        if (_bttlMNG == null) //이렇게 말고 달리 해결할 방법이 없나? 흠..
        {
            _bttlMNG = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        }
        _bttlMNG._skSkillEffect(this, _bttlMNG._skillTargets, _skill);
    }

    //[전투] 전투 상태 진입 선언 (전투 알고리즘 실행)
    public virtual void StartBattle()
    {
        SceneBattle = Battle();
        StartCoroutine(SceneBattle);
    }

    //[전투] 전투 시의 알고리즘
    protected virtual IEnumerator Battle()
    {
        InputState = PublicDefines.NowAction.IDLE;
        _nowTime_Action = _actDelayTime;
        if (PawnType != PublicDefines.ePawnType.Monster)
        {
            _nowTime_Attack = _atkDelayTime;
        }
        else
        {
            _nowTime_Attack = _atkDelayTime * 0.25f;
        }

        bCanPlayNewAnime = true;

        while (!IsDead)
        {
            if (bInputExternal) //외부 입력이었다면
            {
                //들어온값 그대로 넘기고
                bInputExternal = false;     //외부 입력 초기화.     
            }
            else if (bCanPlayNewAnime)      //외부 입력이 아닌데 + 새로운 애니메이션 재생도 가능하다면
            {
                InputState = Input_ByRaycast();    //(내부의) Raycast로 감지하여 상태값 생성.
            }

            if (Stats.NowHp <= 0)
            {
                InputState = PublicDefines.NowAction.DEAD;
            }
            if (bCanPlayNewAnime)
            {
                Set_AIBattleAction();   //[posGoal] 상태값에 따른 처리
                PlayAnime_Battle();     //[isLeft] [bCanPlayNewAnime] 애니메이션 실행
            }
            yield return null;
        }
        yield break;
    }

    //[상태 입력] 내부 알고리즘이 아닌 다른 객체/입력일 경우
    public void Input_External(PublicDefines.NowAction state)
    {
        InputState = state;             //상태값 전달

        bInputExternal = true;          // [외부 입력] Raycast입력을 하지 않습니다.
        if (state == PublicDefines.NowAction.DEAD)
        {
            _hitBox.enabled = false;            // 다른 입력은 이제 못 들어옵니다.
            bCanPlayNewAnime = true;            // [강제 재생] 바로 재생 가능합니다.
        }
        else if (state == PublicDefines.NowAction.SKILL)
        {
            bCanPlayNewAnime = true;            // [강제 재생] 바로 재생 가능합니다.
        }
        else if (state == PublicDefines.NowAction.HIT && NowAct != PublicDefines.NowAction.SKILL) //HIT인데 + 현재 스킬 사용 중이 아닙니다 : 조건 걸릴까...?
        {
            //이거 의미가 없는거 같은데 뭐지 : 공격 시에만 의미가 있는거 같다?
            float hitRate = Random.Range(0, 1f);
            if (PawnType == PublicDefines.ePawnType.Monster)            //몬스터라면 : 5% 확률로 피격 애니메이션을 재생한다.
            {
                if (hitRate <= 0.25f)
                {
                    bCanPlayNewAnime = true;        // [강제 재생] 바로 재생 가능합니다.
                }
            }
            else if (PawnType != PublicDefines.ePawnType.Monster)       //플레이어 파티라면 : 90% 확률로 피격 애니메이션을 재생한다.
            {
                if (hitRate <= 0.75f)
                {
                    bCanPlayNewAnime = true;        // [강제 재생] 바로 재생 가능합니다.
                }
            }
        }
    }

    //[상태 입력] Raycast에 의한 내부 알고리즘으로 입력값 생성할 경우
    protected virtual PublicDefines.NowAction Input_ByRaycast()
    {
        RaycastHit2D[] hitLeft = CheckCompInAtkArea(true);
        RaycastHit2D[] hitRight = CheckCompInAtkArea(false);

        if (hitLeft.Length > 0)  //왼쪽O
        {
            _nowTime_Action = 0;
            _targetPawn = hitLeft[0].collider.GetComponent<Pawn>();

            //타겟이 죽었다면 > 새로운 타겟 설정으로 넘겨야 함.
            if (_targetPawn.IsDead)
            {
                _targetPawn = null;
                return PublicDefines.NowAction.IDLE;
            }
            else //왼쪽에 있는 타겟이 살아있다면
            {
                if (_nowTime_Attack >= _atkDelayTime)
                {
                    _nowTime_Attack = 0;
                    return PublicDefines.NowAction.ATTACK;
                }
                else
                {
                    _nowTime_Attack += Time.deltaTime;
                    return PublicDefines.NowAction.IDLE;
                }
            }
        }
        else if (hitRight.Length > 0) //왼쪽X 오른쪽O
        {
            _nowTime_Action = 0;
            _targetPawn = hitRight[0].collider.GetComponent<Pawn>();

            if (_targetPawn.IsDead)
            {
                _targetPawn = null;
                return PublicDefines.NowAction.IDLE;
            }
            else
            {
                if (_nowTime_Attack >= _atkDelayTime)
                {
                    _nowTime_Attack = 0;
                    return PublicDefines.NowAction.ATTACK;
                }
                else
                {
                    _nowTime_Attack += Time.deltaTime;
                    return PublicDefines.NowAction.IDLE;
                }
            }
        }
        else //왼쪽X, 오른쪽X
        {
            if (_nowTime_Action < _actDelayTime)
            {
                _nowTime_Action += Time.deltaTime;      //액션 대기 시간이 남았다면 + 마저 대기하라.
                return PublicDefines.NowAction.IDLE;
            }

            if (_targetPawn != null)
            {
                if (_targetPawn.IsDead)
                {
                    _targetPawn = null;
                    return PublicDefines.NowAction.IDLE;
                }
                return PublicDefines.NowAction.MOVE;    //시간 되면 이동하라.                    
            }
            else if (_targetPawn == null) //대상 그룹에 생존자가 있는가? >> 이게 안 들어왔네
            {
                if (CheckRemainTarget() > 0)
                {
                    SetTargetPawn(out _targetPawn);         //가장 가까이 있는 TargetPawn을 찾아서
                    if (!_targetPawn.IsDead) //그새 죽었을까봐 한 번 더 챙겨드립니다^^!
                    {
                        return PublicDefines.NowAction.MOVE;    //시간 되면 이동하라.                    
                    }
                    else
                    {
                        return PublicDefines.NowAction.IDLE;
                    }
                }
                else
                {
                    return PublicDefines.NowAction.IDLE;
                }
            }
            return NowAct; //입력 조건 안맞으면 이전 상태값 반환
        }
    }

    //[전투] 타겟 설정
    private int CheckRemainTarget()
    {
        if (PawnType == PublicDefines.ePawnType.Fellow1 || PawnType == PublicDefines.ePawnType.Fellow2)
        {
            return _ingameMNG._ltMonstersInRoom.Count;
        }
        else if (PawnType == PublicDefines.ePawnType.Monster)
        {
            return _ingameMNG._ltPartyPawns.Count;
        }
        return 0;
    }

    //[전투] 공격 상대를 타겟팅
    protected void SetTargetPawn(out Pawn targetPawn)
    {
        //상대 그룹 설정
        List<Pawn> targetGroup = new List<Pawn>();

        if (PawnType == PublicDefines.ePawnType.Fellow1 || PawnType == PublicDefines.ePawnType.Fellow2)
        {
            targetGroup = _ingameMNG._ltMonstersInRoom;
            QuickSortPawnAsc(transform.position, ref targetGroup, 0, _ingameMNG._ltMonstersInRoom.Count - 1);

            if (PawnType == PublicDefines.ePawnType.Fellow1)
            {
                targetPawn = targetGroup[0];
            }
            else
            {
                int index = (int)(_ingameMNG._ltMonstersInRoom.Count * 0.5f);
                targetPawn = targetGroup[index];
            }
        }
        else //Player는 이 함수에 걸지도 않으니까 그냥 else로 Monster 처리합시다.
        {
            targetGroup = _ingameMNG._ltPartyPawns;
            //_ingameMNG.QuickSortPawnAsc(transform.position, ref targetGroup, 0, _ingameMNG._ltPartyPawns.Count - 1); //굳이?
            targetPawn = targetGroup[Random.Range(0, targetGroup.Count)];
        }
    }

    //[전투] 상대에게 접근할 때의 목적지 설정
    protected void SetApproachingPosition(out Vector3 goal)
    {
        //이건 내가 가고자 하는 위치구나.
        Vector3 left = _targetPawn.transform.position + Vector3.left * AtkDist * 0.75f;
        Vector3 right = _targetPawn.transform.position + Vector3.right * AtkDist * 0.75f;

        if (Vector3.Distance(transform.position, left) < Vector3.Distance(transform.position, right))
        {
            goal = left;
        }
        else
        {
            goal = right;
        }
    }

    //[전투] 상대 그룹에 대하여 가까운 순으로 정렬
    protected void QuickSortPawnAsc(Vector3 myPos, ref List<Pawn> targetGroup, int start, int end)
    {
        Vector3 temp = Vector3.zero;
        //Pawn tempPawn = new Pawn();
        float distSTD = Vector3.Distance(myPos, targetGroup[start].transform.position);

        int left = start, right = end;
        float distRight = Vector3.Distance(myPos, targetGroup[right].transform.position);
        float distLeft = Vector3.Distance(myPos, targetGroup[left].transform.position);

        while (left < right)
        {
            //오른쪽 비교
            while (left < right && distSTD <= distRight)
            {
                right--;
            }
            if (left > right)
            {
                break;
            }

            //왼쪽 비교
            while (left < right && distSTD >= distLeft)
            {
                left++;
            }
            if (left > right)
            {
                break;
            }

            //이건 왜 swap하는거지? 반복해서 left, right 인덱스 구하는거구나
            temp = targetGroup[right].transform.position;
            targetGroup[right].transform.position = targetGroup[left].transform.position;
            targetGroup[left].transform.position = temp;
        }

        //비교 반복이 끝나면 시작값과 왼쪽값을 서로 바꾼다.
        Pawn tempPawn = targetGroup[start];
        targetGroup[start] = targetGroup[left];
        targetGroup[left] = tempPawn;

        //재귀함수를 쓰되 "조건을 한 번 더 나누어" 피벗값을 확실히 잡는 것이 더 좋다.
        //재귀 함수 들어가서 변수 다 넣어두고 while(조건문) 따지지 말고, 앞단에서 조건을 따져서 연산을 줄일 수 있다.
        if (start + 1 < left) //하나짜리를 걸러낸다.
            QuickSortPawnAsc(myPos, ref targetGroup, start, left - 1);
        if (end > right)
            QuickSortPawnAsc(myPos, ref targetGroup, left + 1, end);
    }

    //[전투] 입력 받은 상태값(InputState)에 따른 행동, 정보 처리
    protected void Set_AIBattleAction()
    {
        switch (InputState)
        {
            case PublicDefines.NowAction.IDLE:
                break;
            case PublicDefines.NowAction.MOVE:
                if (_targetPawn != null)
                {
                    SetApproachingPosition(out _posGoal);
                    Vector3 vec = (_posGoal - transform.position).normalized;
                    transform.position += vec * _moveSpeed * Time.deltaTime;
                }
                else
                {
                    InputState = PublicDefines.NowAction.IDLE;
                }
                break;
            case PublicDefines.NowAction.ATTACK:
                if (_targetPawn != null)
                {
                    _posGoal = _targetPawn.transform.position;
                    _nowTime_Attack = 0;
                }
                else
                {
                    InputState = PublicDefines.NowAction.IDLE;
                }
                break;
            case PublicDefines.NowAction.SKILL:
                SetSkillTargets(_skill);
                break;
            case PublicDefines.NowAction.HIT:
            case PublicDefines.NowAction.DEAD:
                break;
        }
    }

    //[전투] 전투 시 상태값에 따른 애니메이션 재생 호출
    protected void PlayAnime_Battle()
    {
        //현재 상태 확정
        NowAct = InputState;

        //PosGoal에 따라 LR 처리
        if (_posGoal.x < transform.position.x)
        {
            _isLeft = true;
        }
        else if (_posGoal.x > transform.position.x)
        {
            _isLeft = false;
        }

        //애니메이션 태그 생성
        string dir = (_isLeft) ? "Left" : "Right";
        string tagAnime = string.Format($"{Stats.Name}-{NowAct}-{dir}");

        switch (NowAct)
        {
            case PublicDefines.NowAction.IDLE:
                bCanPlayNewAnime = true;
                break;
            case PublicDefines.NowAction.MOVE:
                bCanPlayNewAnime = true;
                break;
            case PublicDefines.NowAction.SKILL:
                tagAnime += "-" + _skill.AnimationIndex;
                bCanPlayNewAnime = false;
                break;
            default:
                bCanPlayNewAnime = false;
                break;
        }
        _animeCtrl.Play(tagAnime);
    }

    //[초기화] 행동을 마친 후 대기 상태로 초기화, 관련 정보 초기화
    protected override void Reset_NowAction()
    {
        bCanPlayNewAnime = true;
        InputState = PublicDefines.NowAction.IDLE;
        if (NowAct == PublicDefines.NowAction.SKILL) //이게 좀 더 안전하게 입력되려나?
        {
            _skill.Reset();
            _bttlMNG._skillTargets.Clear();
        }
    }


    #region Gizmo
    /*
    private void OnDrawGizmos()
    {
        //위치 설정
        Vector2 _posCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        BoxCollider2D _collider = transform.GetComponent<BoxCollider2D>();
        _posCenter += _collider.offset;

        //피격 효과
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_posCenter, transform.GetComponent<BoxCollider2D>().size);

        //Raycast
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_posCenter, _posCenter + Vector2.left * fAtkDist);
        Gizmos.DrawLine(_posCenter, _posCenter + Vector2.right * fAtkDist);

        //머리 위에 위치 못 찍나.
        if (_targetPawn != null)
        {
            Handles.Label(transform.position + new Vector3(1, 1, 0), _targetPawn.gameObject.name);
        }
        else
        {
            Handles.Label(transform.position + new Vector3(1, 1, 0), "Null");
        }
    }
    //*/
    #endregion
}