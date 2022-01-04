using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMonster : PawnAI
{
    //스킬에 의해 생성된 몬스터인지 여부
    public bool bSpawnBySkill { get; set; }

    private void Start()
    {
        Init_Monster();
    }
    
    //[초기화] 몬스터 정보 초기화
    private void Init_Monster()
    {
        _ingameMNG = IngameManager._instance;
        _bttlMNG = BattleManager._instance;

        _animeCtrl = this.transform.GetComponent<Animator>();
        _hitBox = transform.GetComponent<Collider2D>();
        InitPawn_Stat(_pawnIndex * 100 + _level);
        InitPawn_Skill(_pawnIndex);

        PlaySound = SoundManager._instance.PlaySound_Effect;
    }

    //[초기화] 스킬로 생성된 몬스터의 정보 초기화
    public void Init_CalledMonster(int pawnIndex, int level)
    {
        bSpawnBySkill = true;

        //인스펙터에서 Index, Level 직접 입력을 못했으므로 여기서 추가
        _pawnIndex = pawnIndex;
        _level = level;
        Init_Monster();

        //전투 알고리즘 실행
        SceneBattle = Battle();
        StartCoroutine(SceneBattle);
    }

    //[알고리즘] 전투 알고리즘
    protected override IEnumerator Battle()
    {
        InputState = PublicDefines.NowAction.IDLE;

        if (!bSpawnBySkill)
        {
            //초기에 세팅된 몬스터 : 딜레이 없이 즉각적인 행동 가능
            _nowTime_Action = _actDelayTime;
        }
        else
        {
            //스킬로 호출된 몬스터 : 약간의 딜레이 후에 행동 가능
            _nowTime_Action = _actDelayTime * 0.4f;
        }

        //공격 딜레이 임의 설정
        _nowTime_Attack = Random.Range(0, _atkDelayTime * 0.5f);

        //애니메이션 실행 : 가능
        bCanPlayNewAnime = true;

        //처음 생성시 : 외부 입력 없음
        bInputExternal = false;

        while (!IsDead)
        {
            // [eNowAct] 입력 처리
            if (bInputExternal) //외부 입력이었다면 (Hit, Dead)
            {
                bInputExternal = false;     //외부 입력 초기화.     
            }
            else if (bCanPlayNewAnime)      //외부 입력이 아닌데 + 새로운 애니메이션 재생도 가능하다면
            {
                if (!PuppetMode)
                {
                    if (_skill.PossibleUse) //사용 가능한 스킬이 있다면
                    {
                        InputState = PublicDefines.NowAction.SKILL;
                    }
                    else  //사용 가능한 스킬이 없다면
                    {
                        InputState = Input_ByRaycast();
                    }
                }
                else
                {
                    InputState = PublicDefines.NowAction.IDLE;
                }
            }
            if (Stats.NowHp <= 0)
            {
                InputState = PublicDefines.NowAction.DEAD;
            }
            if (bCanPlayNewAnime)
            {
                Set_AIBattleAction();   //태값에 따른 처리
                PlayAnime_Battle();     //애니메이션 실행
            }
            yield return null;
        }
        yield break;
    }

    //[인게임] 아이템 드롭
    public void DropItems() 
    {
        StartCoroutine(DropTime());
    }
    private IEnumerator DropTime()
    {
        int count = Random.Range(4, 7);
        while (--count >= 0)
        {
            DropItem item = Instantiate(IngameManager._instance._dropItem, this.transform.position, Quaternion.identity).GetComponent<DropItem>();
            item.Init_SpawnPos(this.transform.position);
        }
        yield break;
    }

    //[사운드] 애니메이션 이벤트 : 효과음 호출
    protected override void InAnime_PlaySound_Effect(EffectSoundIndex type)
    {
        if (!_targetPawn.IsDead)
        {
            PlaySound(type);
        }
    }
}
