using UnityEngine;

public class Pwn_Player : PawnBase
{
    private bool cannotMove;
    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        actElapsedTime += Time.deltaTime;
        Move(out Vector2 _move);
        DefineState(InputManager.instance.inputAtk, _move);
    }
    private void LateUpdate()
    {
        if (!IsDead)
        {
            AnimPlay();
        }
    }


    //입력 > 상태 설정
    private void Move(out Vector2 _move)
    {
        _move = InputManager.GetInputVector();
        
        if (cannotMove 
            || _move == Vector2.zero)
        {
            return;
        }
        
        float degree = GetNormalDirection(Vector2.zero, _move);
        float radian = degree * Mathf.PI / 180;

        _mx = Mathf.Cos(radian);
        _my = Mathf.Sin(radian);
        Vector3 _moveDelta = new Vector3(_mx, _my).normalized * MoveSpeed * Time.deltaTime;
        transform.position += _moveDelta;
        IngameManager.Update_FollowPath(_moveDelta);
    }
    private void DefineState(bool _inputAtk, Vector2 _move)
    {
        //## 상태값 결정
        if (_inputAtk
        && actElapsedTime >= actDelayTime)
        {
            inputAct = Defines.eAct.ATTACK;
        }

        switch (inputAct)
        {
            case Defines.eAct.DEAD:
                canPlay = true;
                break;
            case Defines.eAct.HIT:
                canPlay = (Random.Range(0, 1) > 0.2f);
                break;
            case Defines.eAct.SKILL:
                if (!Skill.Check_Condition(Stat))
                {
                    inputAct = Defines.eAct.IDLE;
                    return;
                }

                skillTargets = SkillManager.TargetCount(this);
                if (skillTargets.Count <= 0)
                {
                    inputAct = Defines.eAct.IDLE;
                    return;
                }
                break;
            default:
                break;
        }
    }
    public void Reset_TargetPawn()
    {
        TargetPawn = null;
    }


    //애니메이션
    public void AnimEvent_NextAtk()
    {
        if (nowAct != Defines.eAct.ATTACK)
        {
            return;
        }

        comboIndex = (comboIndex + 1) % 3;  //3타 기본
        actElapsedTime = (comboIndex == 0) ? 0 : actDelayTime;
        canPlay = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        cannotMove = (other.gameObject.layer == LayerMask.GetMask("MapWall"));
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        cannotMove = !(other.gameObject.layer == LayerMask.GetMask("MapWall"));
    }
}
