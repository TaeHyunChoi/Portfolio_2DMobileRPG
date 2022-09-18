using UnityEngine;

public class Obj_SkillEffect : MonoBehaviour
{
    private PawnBase user;
    private PawnBase target;
    private eEffectType type;
    private float moveSpeed = 15f;

    public void Init(PawnBase _user, PawnBase _target, eEffectType _type)
    {
        user = _user;
        target = _target;
        type = _type;

        //타겟 위치에서 생성되는 타입이라면
        if (type == eEffectType.Place)
        {
            Destroy(this.gameObject, 1);
        }
    }

    private void Update()
    {
        //정보 없으면 루틴X
        if (user == null || target == null)
        {
            return;
        }

        //이펙트 생성 도중에 타겟이 이미 사망한 상태라면 : 이펙트 삭제
        if (target.IsDead)
        {
            Destroy(this.gameObject);
        }

        //발사체 타입이라면
        if (type == eEffectType.Projectile)
        {
            Vector3 _offsetTarget = new Vector3(target.transform.GetComponent<Collider2D>().offset.x, target.transform.GetComponent<Collider2D>().offset.y * 0.5f);
            Vector3 _dir = ((target.transform.position + _offsetTarget) - (this.transform.position)).normalized;
            transform.Translate(_dir * moveSpeed * Time.deltaTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        //스킬과 부딪힌 상대가 타겟이라면
        if (other.transform.GetComponent<PawnBase>() == target)
        {
            //인게임 매니저에게 데미지 계산을 요청
            IngameManager.CalcDamage(user, target, false);

            //쿨타임 초기화
            user.Skill.ResetCoolTime();

            //충돌 후 이펙트 삭제
            Destroy(this.gameObject, 0.2f);
        }
    }
}
