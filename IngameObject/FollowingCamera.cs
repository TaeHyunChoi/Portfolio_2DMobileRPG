using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    PawnPlayer _player;
    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PawnPlayer>();
    }

    //카메라는 플레이어를 쫓아다닌다.
    //나중에 카메라 시점을 바꿔 다른 연출을 할 수 있으므로 카메라를 플레이어 캐릭터 자식 오브젝트로 넣지 않았음.
    private void Update()
    {
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, this.transform.position.z);
    }
}