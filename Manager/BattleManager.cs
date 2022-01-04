using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager _uniqueInstance;
    public static BattleManager _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }
    IngameManager _ingameMNG;

    //스킬 조건 확인
    public IEnumerator CheckSkillCondition()
    {
        _ingameMNG = IngameManager._instance;

        while (true)
        {
            Ckeck_SkillCanUse();
            yield return null;
        }
    }
    private void Ckeck_SkillCanUse()
    {
        //플레이어, 동료AI의 스킬 사용 여부 확인
        for (int n = 0; n < _ingameMNG._ltPartyPawns.Count; n++)
        {
            _ingameMNG._ltPartyPawns[n]._skill.Check_Condition(_ingameMNG._ltPartyPawns[n].Stats);
        }

        //몬스터 스킬 사용 여부 확인
        for (int n = 0; n < _ingameMNG._ltMonstersInRoom.Count; n++)
        {
            _ingameMNG._ltMonstersInRoom[n]._skill.Check_Condition(_ingameMNG._ltMonstersInRoom[n].Stats);
        }
    }
    #region Skill Delegate
    //대상 집단 중에서 스킬 발동 조건에 부합하는 대상을 찾는다.
    public delegate List<Pawn> SkillRelatedStats(Pawn skillUser, SkillInfos usedSkill, List<Pawn> listTargetPawns);
    public SkillRelatedStats _skCheckSkillTargets;

    //스킬 효과의 발동 조건에 맞는 대상을 최종적으로 저장한다.
    public List<Pawn> _skillTargets;

    //스킬 타겟으로 지정한 대상에게 스킬 애니메이션 재생 또는 스킬 효과를 발동한다.
    public delegate void PlaySkillEffect(Pawn skillUser, List<Pawn> targetParty, SkillInfos usedSkill);
    public PlaySkillEffect _skSkillEffect;

    //다른 캐릭터를 소환할 때에 어디에, 얼만큼 소환할지 결정한다.
    public delegate void SkillRelatedPosition(Pawn skillUser, SkillInfos usedSkill);
    public SkillRelatedPosition _skCallOtherPawns;

    public void InitSkillDelegates()
    {
        _skCheckSkillTargets = SkillOperation._ltPawns_StatsAffected;
        _skCallOtherPawns = SkillOperation.CallOtherPawns;
        _skSkillEffect = SkillOperation.PlaySkillEffect;
    }
    #endregion

    //[초기화] 전투 매니저 생성, 스킬 델리게이트 생성
    public void Init_BattleManager()
    {
        _uniqueInstance = this;
        _skillTargets = new List<Pawn>();
        InitSkillDelegates();
    }

    //[상황 제어] AI에게 상황별 정보 생성 및 알고리즘 실행
    public void SetBattle(bool battle)
    {
        if (battle)
        {
            _ingameMNG.NowBattle = true;

            //배틀모드 진입할 때에 문제가 있는건데 그러면
            for (int m = 0; m < _ingameMNG._ltMonstersInRoom.Count; m++)
            {
                PawnMonster monster = _ingameMNG._ltMonstersInRoom[m].GetComponent<PawnMonster>();
                monster.StartBattle();
            }
            for (int f = 1; f < _ingameMNG._ltPartyPawns.Count; f++)
            {
                PawnFellower friend = _ingameMNG._ltPartyPawns[f].GetComponent<PawnFellower>();
                friend.StartBattle();
            }
        }
        else
        {
            _ingameMNG.NowBattle = false;

            //Fellow1 설정
            PawnFellower friend1 = _ingameMNG._ltPartyPawns[1].GetComponent<PawnFellower>();
            friend1._targetPawn = null;

            Vector3 prevPos = IngameManager._instance._player.transform.position;
            Vector3 dirToGoal = (transform.position - prevPos).normalized;
            Vector3 goalPos = prevPos + (dirToGoal * friend1.MoveDist);        //여기가 friend1의 마지막 위치. >> friend2의 prevPos가 된다.
            friend1.ReturnUsual(prevPos, goalPos);

            //Fellow2 설정
            PawnFellower friend2 = _ingameMNG._ltPartyPawns[2].GetComponent<PawnFellower>();
            friend2._targetPawn = null;

            prevPos = goalPos;
            dirToGoal = (transform.position - prevPos).normalized;
            goalPos = prevPos + (dirToGoal * friend2.MoveDist);
            friend2.ReturnUsual(prevPos, goalPos);
        }
    }

    //[데미지 계산]
    public void CalcDamage(Pawn atk, Pawn hit, PublicDefines.AttakType skill = PublicDefines.AttakType.NORMAL)
    {
        int power;  //공격의 위력값
        if (skill > 0)                              //스킬 사용 여부 확인
        {
            power = atk._skill.iPowerPerHit;     //스킬 정보 구조체에서 위력값 가져오기
        }
        else
        {
            power = (atk.PawnType == PublicDefines.ePawnType.Monster) ? 10 : 25;     //임의로 정한 일반 공격 위력값
        }
        int damage = hit.Stats.CalcDamage(atk.Stats, power);    //스탯에 기반한 데미지 연산

        if (damage > 0)         //데미지가 0보다 클 경우
        {
            if (!hit.CompareTag("Player"))      //맞은 애가 AI인데
            {
                if (hit.Stats.NowHp <= 0)   //맞은 애는 죽었어요ㅠㅠㅠ
                {
                    hit.GetComponent<PawnAI>().Input_External(PublicDefines.NowAction.DEAD);
                }
                else
                {
                    hit.GetComponent<PawnAI>().Input_External(PublicDefines.NowAction.HIT);
                }
            }
            else    //맞은 애가 Player라면
            {
                if (hit.Stats.NowHp <= 0)
                {
                    hit.GetComponent<PawnPlayer>().NowAct = PublicDefines.NowAction.DEAD;

                    //플레이어를 죽이는건 (아직은) atk AI 뿐이므로
                    atk.GetComponent<PawnAI>()._targetPawn = null;
                }
            }

            //UI Manager에게 Display 요청
            if (hit.PawnType != PublicDefines.ePawnType.Monster)
            {
                UIManager._instance.DisplayPartyHP(hit.Stats);
            }
            else if (hit.PawnType == PublicDefines.ePawnType.Monster)
            {
                UIManager._instance.DisplayMonsterHP(hit);
            }
        }
    }
}
