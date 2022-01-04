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

    private bool _isDropped;
    private float _moveSpeed = 17;
    private float _waitMoveTime = 0.75f;
    private float _nowTime;

    private Vector3 _posPlayer;
    private float _distGet = 2.5f;

    private void FixedUpdate()
    {
        if (!_isDropped)
        {
            //아이템이 지정 위치에 떨어지면(도착하면)
            if (Vector3.Distance(P4, transform.position) <= 0.1f)
            {
                //드롭 상태로 전환
                _isDropped = true;
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
            if (_nowTime <= _waitMoveTime)
            {
                _nowTime += Time.deltaTime;
            }
            else
            {
                //플레이어 위치를 실시간으로 갱신
                _posPlayer = GameObject.FindGameObjectWithTag("Player").transform.position;

                if (Vector3.Distance(_posPlayer, transform.position) <= _distGet)
                {
                    Vector3 dir = (_posPlayer - transform.position).normalized;
                    transform.position += dir * _moveSpeed * Time.deltaTime;

                    if (Vector3.Distance(_posPlayer, transform.position) <= 0.5f)
                    {
                        SoundManager._instance.PlaySound_Effect(EffectSoundIndex.Object_GetCoin);
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }

    //[초기화] 베지어 곡선 사용을 위한 4개의 점 설정
    public void Init_SpawnPos(Vector3 pos)
    {
        P1 = pos;
        P2 = P1 + Vector3.up;
        float x = Random.Range(-2, 2);
        float y = Random.Range(-2, 2);
        P4 = P1 + new Vector3(x, y);

        float y3 = (P4.y > 0.5f) ? P4.y + 0.25f : P4.y + 0.5f;
        P3 = new Vector3(P4.x, y3);
    }

    //베지어 커브 실행
    public Vector3 BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
        Vector3 t1 = Vector3.Lerp(p1, p2, t);
        Vector3 t2 = Vector3.Lerp(p2, p3, t);
        Vector3 t3 = Vector3.Lerp(p3, p4, t);

        Vector3 tt1 = Vector3.Lerp(t1, t2, t);
        Vector3 tt2 = Vector3.Lerp(t2, t3, t);

        return Vector3.Lerp(tt1, tt2, t);
    }
}
