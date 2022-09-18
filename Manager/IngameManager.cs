using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    public static IngameManager instance;

    public static Defines.eGameState nowState { get; private set; }
    public PawnBase player;

    public List<PawnBase> Fellows = new List<PawnBase>();
    public List<PawnBase> Monsters = new List<PawnBase>();

    public void Awake()
    {
        instance = this;

        nowState = Defines.eGameState.Usual;
    }

    //상황 설정
    public void SetGameState(Defines.eGameState _state)
    {
       nowState = _state;
    }

    //전투
    public void RemainPanwns(Defines.ePawnType _type, out List<PawnBase> _targets)
    {
        _targets = new List<PawnBase>();

        if(_type == Defines.ePawnType.Player
        || _type == Defines.ePawnType.Fellow)
        {
            for(int i = 0 ; i < Monsters.Count; i++)
            {
                if(!Monsters[i].IsDead)
                {
                    _targets.Add(IngameManager.instance.Monsters[i]);
                }
            }
        }
        else if(_type == Defines.ePawnType.Monster)
        {
            for(int i = 0 ; i < IngameManager.instance.Fellows.Count; i++)
            {
                if(!Fellows[i].IsDead)
                {
                    _targets.Add(Fellows[i]);
                }
            }
            
            _targets.Add(player);
        }
    }
    public static void CalcDamage(PawnBase _atk, PawnBase _hit, bool _isNormalAtk)
    {
        int _damage = _isNormalAtk ? 15 : _atk.Skill.PowerPerHit;  //데미지 임의 설정

        if(_hit.Stat.CalcNowHP(_atk.Stat, _damage) <= 0)
        {
            _hit.InputAction(Defines.eAct.DEAD);  

            if(_atk.TargetPawn != null) //Player는 target을 따로 설정X => null 여부 확인
            {
                _atk.Reset_Target();
            }
        }
        else
        {
            _hit.InputAction(Defines.eAct.HIT);
        }

        //UI Manager에게 Display 요청
        UIManager.UpdateUI_HP(_hit);
    }

    //비전투
    public int Init_FollowPath(PawnBase _target, Pwn_Fellow _fellow)
    {
        //최초 거리만큼 FollowPath 설정
        float degree = _target.GetNormalDirection(transform.position, _target.transform.position);
        float radian = degree * (Mathf.PI / 180);
        Vector3 moveDelta = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian)).normalized * _fellow.MoveSpeed * Time.deltaTime;

        //간격만큼 Queue에 저장한다.
        int _count, _frame;
        _frame = _count = (int)((_target.transform.position - transform.position).magnitude / moveDelta.magnitude);
        Vector3 _nextPos = transform.position + moveDelta;

        while (--_count >= 0)
        {
            _fellow.EnqueuePath(_nextPos);
            _nextPos += moveDelta;
        }

        return _frame;
    }
    public static void Update_FollowPath(Vector3 _moveDelta)
    {
        for (int i = 0; i < instance.Fellows.Count; i++)
        {
            instance.Fellows[i].GetComponent<Pwn_Fellow>().EnqueuePath(instance.Fellows[i].transform.position + _moveDelta);
        }
    }
    public void Reset_FollowPath(Pwn_Fellow _fellow)
    {
        _fellow.ClearPath();
        Init_FollowPath(_fellow.Prev, _fellow);
    }
}
