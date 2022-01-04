using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] GameObject _prbResultWnd;

    //출구 위치에 플레이어 캐릭터가 들어오면 : 맵 탐험 종료, 결과창 팝업
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        { 
            Instantiate(_prbResultWnd);
        }
    }

}
