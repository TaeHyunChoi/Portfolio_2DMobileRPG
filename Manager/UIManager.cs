using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    //버튼
    public static Color DefaultColor { get; private set; }
    public static Color PressedColor { get; private set; }

    [SerializeField] Transform tfPartyStatus;
    Slider[] sldNowHP;


    [SerializeField] Transform tfSkillBttns;
    [SerializeField] Image[] imgMoveButton;
    Image[] imgTimeCap;
    Text[] txtRemainTime;

    OptionWindow wndOption;
    [SerializeField] GameObject prfOptionWND;

    private void Awake()
    {
        Init_UIManager();

        DefaultColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        PressedColor = new Color(1, 1, 1, 0.5f);
    }
    private void Init_UIManager()
    {
        sldNowHP = new Slider[tfPartyStatus.childCount];
        for (int n = 0; n < tfPartyStatus.childCount; n++)
        {
            sldNowHP[n] = tfPartyStatus.GetChild(n).GetChild(2).GetComponent<Slider>();
        }

        imgTimeCap = new Image[tfSkillBttns.childCount];
        txtRemainTime = new Text[tfSkillBttns.childCount];

        for (int m = 0; m < tfSkillBttns.childCount; m++)
        {
            imgTimeCap[m] = tfSkillBttns.GetChild(m).GetChild(1).GetChild(0).GetComponent<Image>();
            txtRemainTime[m] = tfSkillBttns.GetChild(m).GetChild(1).GetChild(1).GetComponent<Text>();
        }
    }


    //버튼
    public void DisplayMoveButton(bool[] _input = null)
    {
        for (int i = 0; i < _input.Length; i++)
        {
            imgMoveButton[i].color = _input[i] ? PressedColor : DefaultColor;
        }
    }
    
    private void Update()
    {
        DisplayTime(0, IngameManager.instance.player.Skill);
        DisplayTime(1, IngameManager.instance.Fellows[0].Skill);
        DisplayTime(2, IngameManager.instance.Fellows[1].Skill);
    }
    //스킬 쿨타임 표시
    private void DisplayTime(int _index, PawnBase.PawnSkill _skill)
    {
        if (_skill.CoolTimeEnd())
        {
            imgTimeCap[_index].enabled = false;
            txtRemainTime[_index].enabled = false;
        }
        else
        {
            imgTimeCap[_index].enabled = true;
            txtRemainTime[_index].enabled = true;

            int remainTime = (int)_skill.RemainCoolTime() + 1;
            txtRemainTime[_index].text = remainTime.ToString();
        }
    }
    //플레이어, 동료의 남은 HP 표시
    public static void UpdateUI_HP(PawnBase _pawn)
    {
        if (_pawn.type == Defines.ePawnType.Player
            || _pawn.type == Defines.ePawnType.Fellow)
        {
            int sldIndex = (_pawn.Stat.Index - 1000) / 100;
            instance.sldNowHP[sldIndex].value = (float)_pawn.Stat.NowHp / _pawn.Stat.MaxHp;
        }
        else
        {
            Slider sld = _pawn.GetComponentInChildren<Slider>();
            sld.value = (float)_pawn.Stat.NowHp / _pawn.Stat.MaxHp;
        }
    }



    //옵션 윈도우
    public void OpenOptionWindow()
    {
        if (wndOption == null)
        {
            wndOption = Instantiate(prfOptionWND).GetComponent<OptionWindow>();
        }
        else
        {
            wndOption.gameObject.SetActive(true);
        }
    }
    public void CloseWindow(Transform _tfWindow)
    {
        _tfWindow.gameObject.SetActive(false);
    }
}
