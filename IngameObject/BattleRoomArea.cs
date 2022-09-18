using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoomArea : MonoBehaviour
{
    private int EnterCount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어, 동료AI는 레이어 인덱스로 판별
        if (collision.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            ++EnterCount;
        }

        //입장 수 >= (플레이어 + 생존 동료 수)
        IngameManager.instance.RemainPanwns(Defines.ePawnType.Fellow, out List<PawnBase> _fellows);
        if (EnterCount >= 1 + _fellows.Count)
        {
            //몬스터 정보 등록
            IngameManager.instance.Monsters = new List<PawnBase>(transform.GetComponentsInChildren<Pwn_Monster>());

            //전투 상태 선언
            IngameManager.instance.SetGameState(Defines.eGameState.Battle);

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
