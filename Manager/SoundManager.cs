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
    public static SoundManager instance { get; private set; }

    private AudioSource bgmPlayer;
    public AudioClip[] bgmClips;

    private AudioSource effectPlayer;
    public AudioClip[] effectClips;

    private float maxBGMVal = 1f;
    private bool bgmLoop;
    private float maxEffectVal = 0.2f;

    //[초기화] 사운드 매니저 초기화
    public void Init_SoundManager()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        //Init Sound Player
        bgmPlayer = this.transform.GetComponent<AudioSource>();
        bgmLoop = true;
    }

    //[재생] 배경 음악 재생
    public void PlaySound_BGM(BgmIndex _type, bool _mute = false)
    {
        if (!_mute)
        {
            bgmPlayer.clip = bgmClips[(int)_type];
            bgmPlayer.loop = bgmLoop;
            bgmPlayer.mute = _mute;
            bgmPlayer.volume = maxBGMVal * 0.5f;
            bgmPlayer.Play();

        }
        else
        {
            bgmPlayer.Stop();
        }
    }
    public void UpButton_BGM()
    {
        bgmPlayer.volume = (bgmPlayer.volume >= maxBGMVal) ? maxBGMVal : bgmPlayer.volume + (maxBGMVal * 0.01f);
    }
    public void UpButton_Effect()
    {
        effectPlayer.volume = (effectPlayer.volume >= maxEffectVal) ? maxEffectVal : effectPlayer.volume + (maxEffectVal * 0.01f);
    }

    public void Slider_BGM(int _input)
    {
        bgmPlayer.volume += (maxBGMVal * _input * 0.01f);
    }

    //[재생] 효과음 재생 오브젝트 생성
    public static GameObject PlaySound_Effect(EffectSoundIndex _type, bool _mute = false, bool _isLoop = false) //이펙트 필요할 때마다 여기서 호출하는건가보네
    {
        if (!_mute)
        {
            AudioSource _effectPlayer = new GameObject("EffectSoundPlayer").AddComponent<AudioSource>();
            _effectPlayer.transform.parent = instance.transform; //SoundManager 자식 오브젝트로 effectPlayer가 붙는다.

            //초기화 정보를 _effectPlayer에게 전달한다.
            _effectPlayer.minDistance = 150;
            _effectPlayer.clip = instance.effectClips[(int)_type];
            _effectPlayer.volume = instance.maxEffectVal * 0.5f;
            _effectPlayer.mute = _mute;
            _effectPlayer.loop = _mute;

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
