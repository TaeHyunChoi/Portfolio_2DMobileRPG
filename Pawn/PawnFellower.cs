using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PawnFellower : PawnAI
{
    Pawn _prevPawn;
    PawnFellower _fellower;
    public Vector3 _stdMoveDelta;

    protected int distFrameCount;
    protected Vector3 _nextPos;
    IEnumerator SceneAsUsual;
    IEnumerator ReturnPos;

    private void Start() //최초 생성 시 평소 상태로 설정(AsUsual)
    {
        SceneAsUsual = AsUsual(_prevPawn.transform.position);
        StartCoroutine(SceneAsUsual);
    }

    //[초기화] 동료 캐릭터 정보 초기화
    public void Init_Fellow()
    {
        _ingameMNG = IngameManager._instance;
        _bttlMNG = BattleManager._instance;

        _dbGetData = DataTableManager._instance.GetTableInfo;
        _animeCtrl = this.transform.GetComponent<Animator>();
        _hitBox = transform.GetComponent<Collider2D>();

        //본인 정보 생성 (from 데이터테이블)
        InitPawn_Stat(_pawnIndex * 100 + _level);
        InitPawn_Skill(_pawnIndex);

        //Ingame Manager에게 정보 전달
        _ingameMNG._ltPartyPawns.Add(this);

        //Effect Sound
        PlaySound = SoundManager._instance.PlaySound_Effect;

        //UI 연결
        IngameManager._instance._uiDisplayNowHP(this.Stats);

        //파티원 정보 저장 + 앞선 캐릭터 확인
        GetPartyMemberInfos();

        //MOVE Value
        _followPath = new Queue<Vector3>();
        _nowTime_Attack = _atkDelayTime;

        //Battle Value 
        NowHit = false;
    }

    //[초기화] 평상 시의 이동을 위해 앞순서의 캐릭터 정보(위치)를 가져옴
    private void GetPartyMemberInfos()
    {
        if (PawnType == PublicDefines.ePawnType.Fellow1)
        {
            _prevPawn = GameObject.FindGameObjectWithTag("Player").GetComponent<PawnPlayer>();
            _fellower = GameObject.FindGameObjectWithTag("Friend2").GetComponent<PawnFellower>();
        }
        else if (PawnType == PublicDefines.ePawnType.Fellow2)
        {
            _prevPawn = GameObject.FindGameObjectWithTag("Friend1").GetComponent<PawnFellower>();
        }
        _posGoal = _prevPawn.transform.position;
    }

    //[알고리즘] 평상시 알고리즘
    public IEnumerator AsUsual(Vector3 prevPawnPos)
    {

        if (distFrameCount == 0)
        {
            distFrameCount = InitFollowQueue(prevPawnPos);
        }
        else
        {
            ResetFollowQueue(prevPawnPos);
        }

        while (true)
        {
            if (_followPath.Count > distFrameCount)
            {
                _nextPos = _followPath.Dequeue();

                //LR 정하고
                if (_nextPos.x < transform.position.x)
                {
                    _isLeft = true;
                }
                else if (_nextPos.x > transform.position.x)
                {
                    _isLeft = false;
                }
                //위치 이동
                transform.position = _nextPos;

                //Fellow가 있다면 이동 경로 전달
                if (_fellower != null)
                {
                    FellowGetMoveDelta(_fellower, transform.position);
                }

                //입력값 확정
                NowAct = PublicDefines.NowAction.MOVE;
            }
            else
            {
                NowAct = _prevPawn.NowAct;
            }

            string dir = (_isLeft) ? "Left" : "Right";
            string tagAnime = string.Format($"{Stats.Name}-{NowAct}-{dir}");
            _animeCtrl.Play(tagAnime);
            yield return null;
        }
    }

    //[알고리즘] 전투 알고리즘
    public override void StartBattle()
    {
        StopCoroutine(SceneAsUsual);

        SceneBattle = Battle();
        StartCoroutine(SceneBattle);
    }

    //[알고리즘] 전투 후 평상시 상태로 복귀
    public void ReturnUsual(Vector3 prevPos, Vector3 goalPos)
    {
        //전투 종료
        StopCoroutine(SceneBattle);

        //평상시의 위치로 복귀
        ReturnPos = ReturnMovingTo(prevPos, goalPos);

        //평상시의 알고리즘 시작
        StartCoroutine(ReturnPos);
    }

    //[이동] 전투 후 평상시 위치로 복귀
    protected IEnumerator ReturnMovingTo(Vector3 prevPawnPos, Vector3 targetPos)
    {
        //moveDelta 만들고
        float degree = GetAngle(transform.position, targetPos);
        float radian = degree * (Mathf.PI / 180);
        Vector3 moveDelta = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian)).normalized * _moveSpeed * Time.deltaTime;

        //이동 애니메이션 실행
        string dirTag = (moveDelta.x < 0) ? "Left" : "Right";
        string tag = string.Format($"{Stats.Name}-{PublicDefines.NowAction.MOVE}-{dirTag}");
        _animeCtrl.Play(tag);

        //간격만큼 이동하고
        int count = (int)((targetPos - transform.position).magnitude / moveDelta.magnitude);
        while (--count >= 0)
        {
            transform.position += moveDelta;
            yield return null;
        }

        //도착했으면 IDLE로 변경하고
        tag = string.Format($"{Stats.Name}-{PublicDefines.NowAction.IDLE}-{dirTag}");
        _animeCtrl.Play(tag);

        //코루틴 종료
        SceneAsUsual = AsUsual(prevPawnPos);
        yield return StartCoroutine(SceneAsUsual);
    }

    //[이동] 플레이어 이동이 없을 경우(ex.최초 실행) 그만큼 이동을 위한 이동 정보 생성
    private int InitFollowQueue(Vector3 targetPos)
    {
        float degree = GetAngle(transform.position, targetPos);
        float radian = degree * (Mathf.PI / 180);
        Vector3 moveDelta = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian)).normalized * _moveSpeed * Time.deltaTime;

        //간격만큼 Queue에 저장한다.
        int count, result;
        count = result = (int)((targetPos - transform.position).magnitude / moveDelta.magnitude);
        Vector3 nextPos = transform.position + moveDelta;

        _followPath.Clear();
        while (--count >= 0)
        {
            _followPath.Enqueue(nextPos);
            nextPos += moveDelta;
        }
        return result;
    }

    //[이동] 전투 후 이전의 이동 Queue를 초기화하고 새로운 위치 정보를 입력
    private void ResetFollowQueue(Vector3 targetPos)
    {
        //이전의 위치 정보 Queue를 지운다.
        _followPath.Clear();

        //동일한 간격만큼 거리를 나누어 Queue에 저장한다.
        Vector3 diff = (targetPos - transform.position);
        Vector3 moveDelta = new Vector3(diff.x / distFrameCount, diff.y / distFrameCount);
        Vector3 nextPos = transform.position + moveDelta;
        while (Vector3.Distance(targetPos, nextPos) >= 0.01f)
        {
            _followPath.Enqueue(nextPos);
            nextPos += moveDelta;
        }
    }

    //[사운드] 애니메이션 이벤트. 효과음 재생
    protected override void InAnime_PlaySound_Effect(EffectSoundIndex type)
    {
        if(!_targetPawn.IsDead)
        { 
            PlaySound(type);
        }
    }
}
