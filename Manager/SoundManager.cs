using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BGM, 효과음 인덱스
public enum BgmIndex
{
    DevilsVelly,
    BattleTheme1,
    BattleTheme2,
    ChapterStart,
    Victory
}
public enum EffectSoundIndex
{
    Attack_Weapon,
    Attack_Fist,
    Attack_FistHard,
    Attack_Miss,
    Attack_Critical,
    Attack_Straight,
    Skill_Fire,
    Skill_Ice,
    SKill_TigerRoar,
    Object_GetCoin
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager _uniqueInstance;
    public static SoundManager _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    AudioSource _bgmPlayer;
    [SerializeField] AudioClip[] _bgmClips;

    AudioSource _effectPlayer;
    [SerializeField] AudioClip[] _effectClips;

    private float _bgmVolume;
    private bool _bgmLoop;
    private float _effectVolume;

    //[초기화] 사운드 매니저 초기화
    public void Init_SoundManager()
    {
        _uniqueInstance = this;
        DontDestroyOnLoad(this.gameObject);

        //Init Sound Player
        _bgmPlayer = this.transform.GetComponent<AudioSource>();
        _bgmVolume = 0.5f;
        _effectVolume = 0.1f;
        _bgmLoop = true;
    }

    //[재생] 배경 음악 재생
    public void PlaySound_BGM(BgmIndex type, bool mute = false)
    {
        if (!mute)
        {
            _bgmPlayer.clip = _bgmClips[(int)type];
            _bgmPlayer.loop = _bgmLoop;
            _bgmPlayer.mute = mute;
            _bgmPlayer.volume = _bgmVolume;
            _bgmPlayer.Play();

        }
        else
        {
            _bgmPlayer.Stop();
        }
    }

    //[재생] 효과음 재생 오브젝트 생성
    public GameObject PlaySound_Effect(EffectSoundIndex type, bool mute = false, bool isLoop = false) //이펙트 필요할 때마다 여기서 호출하는건가보네
    {
        if (!mute)
        {
            _effectPlayer = new GameObject("EffectSoundPlayer").AddComponent<AudioSource>();
            _effectPlayer.transform.parent = this.transform; //SoundManager 자식 오브젝트로 effectPlayer가 붙는다.

            //초기화 정보를 _effectPlayer에게 전달한다.
            _effectPlayer.minDistance = 150;
            _effectPlayer.clip = _effectClips[(int)type];
            _effectPlayer.volume = _effectVolume;
            _effectPlayer.mute = mute;
            _effectPlayer.loop = mute;

            //설정 다했으면 재생시킨다.
            _effectPlayer.Play();

            //이펙트 효과음이 끝나면 > 생성한 오브젝트를 없앤다
            Destroy(_effectPlayer.gameObject, 1);

            return _effectPlayer.gameObject;
        }
        else
        {
            return null;
        }
    }
}
