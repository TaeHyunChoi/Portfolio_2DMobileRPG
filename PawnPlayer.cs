using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PawnPlayer : Pawn
{
    #region 필드
    private static PawnPlayer _uniqueInstance;
    public static PawnPlayer _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    private InputManager _input;

    private float _mx, _my;
    private PawnFellower _fellower;

    public bool NowTouchingATK { get; set; }
    public bool NowTouchingSKILL { get; set; }
    #endregion
    private void Start()
    {
        StartCoroutine(IE_PlayerControl());
    }

    //[초기화] 플레이어 캐릭터 정보 초기화
    public void Init_Player()
    {
        _uniqueInstance = this;
        _bttlMNG = BattleManager._instance;
        _dbGetData = DataTableManager._instance.GetTableInfo;
        PlaySound = SoundManager._instance.PlaySound_Effect;

        _type = PublicDefines.ePawnType.Player;
        _animeCtrl = transform.GetComponent<Animator>();
        _hitBox = transform.GetComponent<Collider2D>();

        _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();

        PlaySound = SoundManager._instance.PlaySound_Effect;

        InitPawn_Stat(_pawnIndex * 100 + _level);
        InitPawn_Skill(_pawnIndex);

        IngameManager._instance._ltPartyPawns.Add(this);
        _fellower = GameObject.FindGameObjectWithTag("Friend1").GetComponent<PawnFellower>();
    }

    //[행동 제어]
    IEnumerator IE_PlayerControl()
    {
        bCanPlayNewAnime = true;
        NowCombo = 0;
        _nowTime_Attack = _atkDelayTime;

        while (!IsDead)
        {
            //공격 딜레이 갱신
            _nowTime_Attack += Time.deltaTime;

            //입력값 결정 + bCanPlayNewAnime 재설정
            InputState = Set_InputState();

            //AI 처리
            NowAct = Set_PlayerAction();

            //애니메이션 실행
            Play_Anime(NowAct);                        

            //벽과의 충돌 시 지진 현상을 막기 위해 Collider 연산 후 갱신
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    //[행동 입력] 공격
    public void InputAttack(bool input)
    {
        NowTouchingATK = input;
        if (!input) //공격 입력을 취소한다면 >> 입력 Reset
        {
            NowCombo = 0;
            //애니메이션 캔슬이 나니까 Reset은 애니메이션 맨마지막에서만 처리하자.
        }
    }

    //[행동 입력] 콤보 공격을 위해 다음 공격 입력
    public void InputNextAttack()
    {
        if (NowTouchingATK)
        {
            //때리고 난 뒤에 NowCombo 후처리
            NowCombo = (NowCombo + 1) > 2 ? 0 : NowCombo + 1;

            //공격 가능한 상태로 전환, 공격 딜레이도 함께 초기화
            _nowTime_Attack = (NowCombo == 0) ? 0 : _atkDelayTime;
            bCanPlayNewAnime = true;
            //터치로 공격 버튼을 누르고 있으므로(NowTouching) 상태 입력값은 ATTACK으로 변경된다.
        }
    }

    //[행동 입력] 조건에 따른 입력값 설정
    private PublicDefines.NowAction Set_InputState()
    {
        if (NowTouchingATK && _nowTime_Attack >= _atkDelayTime)
        {
            InputState = PublicDefines.NowAction.ATTACK;
        }
        else if (InputState != PublicDefines.NowAction.SKILL)
        {
            InputState = PublicDefines.NowAction.IDLE;
        }

        switch (InputState)
        {
            case PublicDefines.NowAction.DEAD:
                {
                    bCanPlayNewAnime = true;
                    return PublicDefines.NowAction.DEAD;
                }
            case PublicDefines.NowAction.HIT:
                {
                    float hitRate = UnityEngine.Random.Range(0, 1);
                    if (hitRate >= 0.2f)
                    {
                        bCanPlayNewAnime = true;
                        return PublicDefines.NowAction.HIT;
                    }
                    else
                    {
                        return InputState;
                    }
                }
            case PublicDefines.NowAction.SKILL:
            case PublicDefines.NowAction.ATTACK:
                if (IngameManager._instance.NowBattle)
                {
                    return InputState;
                }
                else
                {
                    return PublicDefines.NowAction.IDLE;
                }
            default:
                {
                    if (_input.GetInputVector() != Vector3.zero)
                    {
                        return PublicDefines.NowAction.MOVE;
                    }
                    else
                    {
                        return PublicDefines.NowAction.IDLE;
                    }
                }
        }
    }

    //[행동 입력] 행동별, 조건별 입력값 변경, 처리
    private PublicDefines.NowAction Set_PlayerAction()
    {
        switch (InputState)
        {
            case PublicDefines.NowAction.IDLE:
            case PublicDefines.NowAction.MOVE:
                Vector2 move = _input.GetInputVector();
                if (move != Vector2.zero)
                {
                    float degree = GetAngle(Vector2.zero, move);
                    float radian = degree * Mathf.PI / 180;

                    _mx = Mathf.Cos(radian);
                    _my = Mathf.Sin(radian);
                    _inputDir = new Vector3(_mx, _my).normalized;
                    transform.position += _inputDir * _moveSpeed * Time.deltaTime; //move delta

                    FellowGetMoveDelta(_fellower, transform.position);
                    bCanPlayNewAnime = true;
                    return PublicDefines.NowAction.MOVE;
                }
                else
                {
                    return PublicDefines.NowAction.IDLE;
                }
            case PublicDefines.NowAction.ATTACK:
                if (IngameManager._instance.NowBattle)
                {
                    if (bCanPlayNewAnime && _nowTime_Attack >= _atkDelayTime)
                    {
                        if (NowCombo == 0) //첫번째 콤보 공격이면 공격 딜레이 초기화.
                        {
                            _nowTime_Attack = 0;
                        }
                        return PublicDefines.NowAction.ATTACK;
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
            case PublicDefines.NowAction.SKILL:
                if (IngameManager._instance.NowBattle)
                {
                    NowCombo = 0;
                    SetSkillTargets(_skill);
                    return PublicDefines.NowAction.SKILL;
                }
                else
                {
                    return PublicDefines.NowAction.IDLE;
                }
            case PublicDefines.NowAction.HIT:
                NowCombo = 0;
                return PublicDefines.NowAction.HIT;
            case PublicDefines.NowAction.DEAD:
                return PublicDefines.NowAction.DEAD;
        }
        return InputState;
    }

    //[행동 입력] 상태값에 따른 애니메이션 호출
    private void Play_Anime(PublicDefines.NowAction nowAct)
    {
        if (bCanPlayNewAnime)
        {
            //바라보는 방향 결정(왼쪽? 오른쪽?)
            if (_mx < 0)
            {
                _isLeft = true;
            }
            else if (_mx > 0)
            {
                _isLeft = false;
            }

            //애니메이션 태그 설정
            string dir = (_isLeft) ? "Left" : "Right";
            string tag = string.Format($"Ataho-{NowAct}-{dir}");
            switch (nowAct)
            {
                case PublicDefines.NowAction.IDLE:
                case PublicDefines.NowAction.MOVE:
                    break;
                case PublicDefines.NowAction.ATTACK:
                    bCanPlayNewAnime = false;
                    tag += string.Format("-{0}", NowCombo);
                    break;
                case PublicDefines.NowAction.SKILL:
                    bCanPlayNewAnime = false;
                    tag += "-" + _skill.AnimationIndex;
                    break;
                case PublicDefines.NowAction.HIT:
                case PublicDefines.NowAction.DEAD:
                    break;
            }

            //애니메이션 재생 호출
            _animeCtrl.Play(tag);
        }
    }

    //[스킬] 애니메이션 이벤트. 스킬 이펙트 생성
    protected override void InAnime_PlaySkillEffect()
    {
        _bttlMNG._skSkillEffect(this, _bttlMNG._skillTargets, _skill);
    }
}
