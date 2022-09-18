using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    //베지어 곡선 구현
    private Vector3 P1;
    private Vector3 P2;
    private Vector3 P3;
    private Vector3 P4;
    private float t;

    private bool isDropped;
    private float moveSpeed = 17;
    private float waitMoveTime = 0.75f;
    private float nowTime;

    private Vector3 posPlayer;
    private float distGet = 2.5f;

    private void FixedUpdate()
    {
        if (!isDropped)
        {
            //아이템이 지정 위치에 떨어지면(도착하면)
            if (Vector3.Distance(P4, transform.position) <= 0.1f)
            {
                //드롭 상태로 전환
                isDropped = true;
            }
            else
            {
                //아직 드롭이 끝나지 않았다면 계속 이동
                transform.position = BezierCurve(P1, P2, P3, P4, t);
                t += Time.deltaTime * 4;
            }
        }
        else
        {
            //아이템 먹기 전 일정 시간 딜레이 걸었음
            if (nowTime <= waitMoveTime)
            {
                nowTime += Time.deltaTime;
            }
            else
            {
                //플레이어 위치를 실시간으로 갱신
                posPlayer = GameObject.FindGameObjectWithTag("Player").transform.position;

                if (Vector3.Distance(posPlayer, transform.position) <= distGet)
                {
                    Vector3 _dir = (posPlayer - transform.position).normalized;
                    transform.position += _dir * moveSpeed * Time.deltaTime;

                    if (Vector3.Distance(posPlayer, transform.position) <= 0.5f)
                    {
                        SoundManager.PlaySound_Effect(EffectSoundIndex.Object_GetCoin);
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }

    //[초기화] 베지어 곡선 사용을 위한 4개의 점 설정
    public void Init_SpawnPos(Vector3 _pos)
    {
        P1 = _pos;
        P2 = P1 + Vector3.up;
        float _x = Random.Range(-2, 2);
        float _y = Random.Range(-2, 2);
        P4 = P1 + new Vector3(_x, _y);

        float _y3 = (P4.y > 0.5f) ? P4.y + 0.25f : P4.y + 0.5f;
        P3 = new Vector3(P4.x, _y3);
    }

    //베지어 커브 실행
    public Vector3 BezierCurve(Vector3 _p1, Vector3 _p2, Vector3 _p3, Vector3 _p4, float _t)
    {
        Vector3 t1 = Vector3.Lerp(_p1, _p2, _t);
        Vector3 t2 = Vector3.Lerp(_p2, _p3, _t);
        Vector3 t3 = Vector3.Lerp(_p3, _p4, _t);

        Vector3 tt1 = Vector3.Lerp(t1, t2, _t);
        Vector3 tt2 = Vector3.Lerp(t2, t3, _t);

        return Vector3.Lerp(tt1, tt2, _t);
    }
}
