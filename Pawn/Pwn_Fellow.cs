using System.Collections.Generic;
using UnityEngine;

public class Pwn_Fellow : PawnBase
{
    private int fellowIndex;
    private Vector3 goalPos;

    public PawnBase Prev { get; private set; }
    private Queue<Vector3> path;
    private int distFrameCount;


    public override void Init(Defines.ePawnType _type, int _pawnIndex)
    {
        base.Init(_type, _pawnIndex);

        gameObject.tag = "Fellow";

        //prev 설정 (비전투 시 prev 경로를 쫓아다닌다)
        int _count = IngameManager.instance.Fellows.Count;
        if (_count <= 0)
        {
            Prev = IngameManager.instance.player;
        }
        else
        {
            Prev = IngameManager.instance.Fellows[_count];
        }

        fellowIndex = _count;
        IngameManager.instance.Fellows.Add(this);

        distFrameCount = IngameManager.instance.Init_FollowPath(Prev, this);
        inputAct = nowAct = Defines.eAct.IDLE;

        actElapsedTime = 0;
        actDelayTime = 1.5f;            //임의 설정
        actElapsedTime = actDelayTime;  //임의 설정. 즉시 공격 가능하도록 
    }


    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        //전투
        if (IngameManager.nowState == Defines.eGameState.Battle)
        {
            //시간 갱신
            actElapsedTime += Time.deltaTime;       
            Skill.UpdateCoolTime();

            //타겟 정보 갱신
            if (TargetPawn != null)
            {
                TargetingMonster();
            }
            goalPos = TargetPawn.transform.position;


            inputAct = AI_SelectAct();

            //행동 변화가 있으면 사이에 딜레이를 준다.
            if (nowAct != inputAct                   
                && actElapsedTime < actDelayTime)
            {
                return;
            }

            //행동 실행
            AI_Act(goalPos);

            //루프 강제종료 (전투만 실행하도록)
            return;
        }

        //비전투 : prev pawn를 쫓아다닌다.
        if (path.Count > distFrameCount)
        {
            transform.position = path.Dequeue();
        }

        //inputAct를 LateUpdate()로 넘겨 애니메이션 실행
    }
    private void LateUpdate()
    {
        if (!IsDead)
        {
            AnimPlay();
        }
    }


    //## Battle
    private void TargetingMonster()
    {
        PawnBase _closest = null;

        List<PawnBase> _remains = IngameManager.instance.Monsters;
        float _closestDist = float.MaxValue;

        for (int i = 0; i < _remains.Count; i++)
        {
            if (_remains[i].IsDead == false
               && Vector3.Distance(transform.position, _remains[i].transform.position) < _closestDist)
            {
                _closest = _remains[i].GetComponent<PawnBase>();
            }
        }

        TargetPawn = _closest;
    }    


    //## Usual
    public void EnqueuePath(Vector3 _pos)
    {
        path.Enqueue(_pos);
    }
    public void ClearPath()
    {
        path.Clear();
    }
}
