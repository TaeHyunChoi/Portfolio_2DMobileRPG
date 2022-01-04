using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _uniqueInstance;
    public static UIManager _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    [SerializeField] Transform _tfPartyStatus;
    Slider[] _sldNowHP;

    [SerializeField] Transform _tfSkillBttns;
    Image[] _imgTimeCap;
    Text[] _txtRemainTime;
    private void Awake()
    {
        _uniqueInstance = this;
        Init_UIManager();
    }
    private void Update()
    {
        DisplayTime();
    }

    //[초기화] UI 관련 정보 초기화
    private void Init_UIManager()
    {
        _sldNowHP = new Slider[_tfPartyStatus.childCount];
        for (int n = 0; n < _tfPartyStatus.childCount; n++)
        {
            _sldNowHP[n] = _tfPartyStatus.GetChild(n).GetChild(2).GetComponent<Slider>();
        }

        _imgTimeCap = new Image[_tfSkillBttns.childCount];
        _txtRemainTime = new Text[_tfSkillBttns.childCount];

        for (int m = 0; m < _tfSkillBttns.childCount; m++)
        {
            _imgTimeCap[m] = _tfSkillBttns.GetChild(m).GetChild(1).GetChild(0).GetComponent<Image>();
            _txtRemainTime[m] = _tfSkillBttns.GetChild(m).GetChild(1).GetChild(1).GetComponent<Text>();
        }
    }

    //[표시] 플레이어, 동료의 남은 HP 표시
    private void DisplayTime()
    {
        for (int n = 0; n < IngameManager._instance._ltPartyPawns.Count; n++)
        {
            //현재 스킬 사용이 가능하다면
            if (IngameManager._instance._ltPartyPawns[n]._skill.PossibleUse)
            {
                //Cap을 벗긴다.
                _imgTimeCap[n].enabled = _txtRemainTime[n].enabled = false;
            }
            else    //사용이 불가하다면
            {
                //Cap을 씌우고
                _imgTimeCap[n].enabled = true;
                _txtRemainTime[n].enabled = true;

                //남은 시간을 보여준다
                int remainTime = (int)(IngameManager._instance._ltPartyPawns[n]._skill.fCondTime - IngameManager._instance._ltPartyPawns[n]._skill.fNowCoolTime) + 1;
                _txtRemainTime[n].text = remainTime.ToString();
            }
        }
    }

    //[표시] 플레이어, 동료의 스킬 쿨타임 표시
    public void DisplayPartyHP(PawnStats partyStats)
    {
        //인덱스 번호가 일치하지 않았다는거지...? 얘를 맞추면 되는건데.. How?
        //PlayerParty의 n번째입니다! 라고 맞추면 좋을텐데
        //아니면 다른 방법이 있다?

        int sldIndex = (partyStats.Index - 1000) / 100;
        _sldNowHP[sldIndex].value = (float)partyStats.NowHp / partyStats.MaxHp;
    }

    //[표시] 몬스터의 남은 HP 표시
    public void DisplayMonsterHP(Pawn monster)
    {
        float ratioHP = (float)monster.Stats.NowHp / monster.Stats.MaxHp; //아 맞다 얘네 int형이구나; 잊지 말자 형변환 캐스팅!
        Slider sld = monster.transform.GetChild(1).GetChild(0).GetComponent<Slider>();
        sld.value = ratioHP;
    }
}
