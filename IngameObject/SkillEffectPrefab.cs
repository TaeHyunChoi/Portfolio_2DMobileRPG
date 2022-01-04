using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectPrefab : MonoBehaviour
{
    Pawn _user, _target;
    eEffectType _type;
    float _moveSpeed = 15f;

    public void Update()
    {
        //이펙트 생성 도중에 타겟이 이미 사망한 상태라면 : 이펙트 삭제
        if (_target.IsDead)
        {
            Destroy(this.gameObject);
        }

        //스킬 이펙트가 투사체 타입이라면
        if (_type == eEffectType.Projectile)
        {
            Vector3 offsetTarget = new Vector3(_target.transform.GetComponent<Collider2D>().offset.x, _target.transform.GetComponent<Collider2D>().offset.y*0.5f);
            Vector3 vec = ((_target.transform.position + offsetTarget) - (this.transform.position)).normalized;
            transform.Translate(vec * _moveSpeed * Time.deltaTime);
        }
        //스킬 이펙트가 타겟 위치에서 생성되는 타입이라면
        else if (_type == eEffectType.Place)
        {
            Destroy(this.gameObject, 1);
        }
    }

    //[초기화] 스킬을 사용한 캐릭터, 대상 캐릭터, 스킬 타입 초기화 → 데미지 계산에 사용
    public void PlayEffect(Pawn user, Pawn target, eEffectType type)
    {
        _user = user;
        _target = target;
        _type = type;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        //스킬과 부딪힌 상대가 타겟이라면
        if (collision.transform.GetComponent<Pawn>() == _target)
        {
            //인게임 매니저에게 데미지 계산을 요청
            _user.CalcDamageByTrigger(_user, _target, PublicDefines.AttakType.SKILL);

            //충돌 후 이펙트 삭제
            Destroy(this.gameObject, 0.2f);
        }
    }
}
