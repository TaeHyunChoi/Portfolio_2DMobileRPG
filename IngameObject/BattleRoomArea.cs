using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoomArea : MonoBehaviour
{
    public int RoomIndex { get; set; }

    private IngameManager _ingameMNG;
    private int EnterCount;
    public int MonsterCount { get; set; }

    private void Start()
    {
        _ingameMNG = IngameManager._instance;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어, 동료AI는 레이어 인덱스로 판별
        if (collision.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            ++EnterCount;
        }

        //전투 지역에 속한 몬스터 정보를 생성 후 전투 상태 선언
        if (EnterCount >= _ingameMNG._ltPartyPawns.Count)
        {
            int count = transform.GetChild(0).childCount;
            _ingameMNG._ltMonstersInRoom = new List<Pawn>();
            for (int n = 0; n < count; n++)
            {
                _ingameMNG._ltMonstersInRoom.Add(transform.GetChild(0).GetChild(n).transform.GetComponent<PawnMonster>());
            }
            BattleManager._instance.SetBattle(true);

            //번거로운 충돌을 없애기 위해 Collider, Trigger Off.
            transform.GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //캐릭터가 들어오다가 퇴장할 경우 → 입장 카운트 감소
        if (collision.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            --EnterCount;
        }
    }
}
