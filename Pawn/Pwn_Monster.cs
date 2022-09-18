using System.Collections.Generic;
using UnityEngine;

public class Pwn_Monster : PawnBase
{
    private Vector3 goalPos;

    public override void Init(Defines.ePawnType _type, int _pawnIndex)
    {
        base.Init(_type, _pawnIndex);
        
        gameObject.tag = "Monster";
        IngameManager.instance.Monsters.Add(this);

        actElapsedTime = 0;
        actDelayTime = 2f;            //임의 설정
        actElapsedTime = actDelayTime;  //임의 설정. 즉시 공격 가능하도록 
    }


    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        //시간 갱신
        actElapsedTime += Time.deltaTime;
        Skill.UpdateCoolTime();

        //공격 대상 및 위치 설정
        if (TargetPawn != null)                     
        {
            TargetingPlayers();
        }
        goalPos = TargetPawn.transform.position;

        //행동 결정
        inputAct = AI_SelectAct();
        if (nowAct != inputAct                  //행동 변화가 생기면 
            && actElapsedTime < actDelayTime)   //행동 간 딜레이를 둔다.
        {
            return;
        }

        //행동 실행
        AI_Act(goalPos);
    }
    private void LateUpdate()
    {
        if (canPlay)
        {
            AnimPlay();
        }
    }


    private void TargetingPlayers()
    {
        PawnBase _closest = null;

        //Fellow 선탐색
        List<PawnBase> _fellows = IngameManager.instance.Fellows;
        float _closestDist = float.MaxValue;

        for (int i = 0; i < _fellows.Count; i++)
        {
            if (_fellows[i].IsDead == false
               && Vector3.Distance(transform.position, _fellows[i].transform.position) < _closestDist)
            {
                _closest = _fellows[i].GetComponent<PawnBase>();
            }
        }

        //플레이어와의 거리도 판단
        if (Vector3.Distance(transform.position, IngameManager.instance.player.transform.position) <= _closestDist)
        {
            _closest = IngameManager.instance.player.GetComponent<PawnBase>();
        }

        TargetPawn = _closest;
    }
}
