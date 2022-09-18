using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum OptionMenu
{
    BGM = 0,
    Effect,
    DPadSize = 10,
    APadSize,
    DPadPositionX = 20,
    DPadPositinoY,
    APadPositionX = 30,
    APadPositinoY
}
public class OptionWindow : MonoBehaviour
{
    [SerializeField] Transform _tfSound;
    [SerializeField] Transform _tfCtrlSize;
    [SerializeField] Transform _tfCtrlPosition;

    SoundManager _soundMNG;

    //액션 버튼 정보를 모두 갖고 있어야 하나? 흠...
    ObjUI_VirtualPad[] _dPad;
    ObjUI_NormalAtkButton _norAtkPad;
    ObjUI_SkillButton[] _skillAtkPad;

    Text _valBGM;
    Slider _sldBGM;

    private void Awake()
    {
        _soundMNG = SoundManager.instance;
        //_sldBGM = 
        //전체구조 한 번 더 잡고 가야겠는데 + 옵션만 하루종일 걸릴 가능성도 있겠군...
        //이런 식의 UI나 코드는 짜본 적이 없긴 했구나 ㅇㅋㅇㅋ... 침착쓰...

        Init_PadInfos();
    }
    private void Init_PadInfos()
    {
        //Virtual Direction Pad
        _dPad = new ObjUI_VirtualPad[4];
        Transform _tfDPad = GameObject.FindGameObjectWithTag("VirtualDPad").transform;
        for (int n = 0; n < _dPad.Length; n++)
        {
            _dPad[n] = _tfDPad.GetChild(n).GetComponent<ObjUI_VirtualPad>();
        }

        //Normal Attack
        _norAtkPad = GameObject.FindGameObjectWithTag("NormalAttackPad").GetComponent<ObjUI_NormalAtkButton>();

        //Skill Attack
        _skillAtkPad = new ObjUI_SkillButton[3];
        Transform _tfSkillPad = GameObject.FindGameObjectWithTag("SkillAttackPad").transform;
        for (int s = 0; s < _skillAtkPad.Length; s++)
        {
            _skillAtkPad[s] = _tfSkillPad.GetChild(s).GetComponent<ObjUI_SkillButton>();
        }
    }

    public void Button_ValueUp(int optionIndex)
    {
        switch ((OptionMenu)optionIndex)
        {
            case OptionMenu.BGM:
                _soundMNG.UpButton_BGM();
                break;
            case OptionMenu.Effect:
                _soundMNG.UpButton_Effect();
                break;
            case OptionMenu.DPadSize:
                _dPad[0].ChangeSize(1);
                _dPad[1].ChangeSize(1);
                _dPad[2].ChangeSize(1);
                _dPad[3].ChangeSize(1);
                break;
            case OptionMenu.APadSize:
                _norAtkPad.ChangeSize(1);
                _skillAtkPad[0].ChangeSize(1);
                _skillAtkPad[1].ChangeSize(1);
                _skillAtkPad[2].ChangeSize(1);
                break;
            case OptionMenu.DPadPositionX:
                _dPad[0].MovePosition(true, 1);
                _dPad[1].MovePosition(true, 1);
                _dPad[2].MovePosition(true, 1);
                _dPad[3].MovePosition(true, 1);
                break;
            case OptionMenu.DPadPositinoY:
                _dPad[0].MovePosition(false, 1);
                _dPad[1].MovePosition(false, 1);
                _dPad[2].MovePosition(false, 1);
                _dPad[3].MovePosition(false, 1);
                break;
            case OptionMenu.APadPositionX:
                break;
            case OptionMenu.APadPositinoY:
                break;
        }
    }
    public void Slider_Value(int optionIndex)
    {
        switch ((OptionMenu)optionIndex)
        {
            case OptionMenu.BGM:
                if(_soundMNG != null) //옵션 쪽이 뭔가 짜치네...? 깔끔하게 안 떨어진다 뭔가...뭔가...
                { 
                    _soundMNG.Slider_BGM((int)_sldBGM.value);
                }
                break;
            case OptionMenu.Effect:
                _soundMNG.UpButton_Effect();
                break;
            case OptionMenu.DPadSize:
                _dPad[0].ChangeSize(1);
                _dPad[1].ChangeSize(1);
                _dPad[2].ChangeSize(1);
                _dPad[3].ChangeSize(1);
                break;
            case OptionMenu.APadSize:
                _norAtkPad.ChangeSize(1);
                _skillAtkPad[0].ChangeSize(1);
                _skillAtkPad[1].ChangeSize(1);
                _skillAtkPad[2].ChangeSize(1);
                break;
            case OptionMenu.DPadPositionX:
                _dPad[0].MovePosition(true, 1);
                _dPad[1].MovePosition(true, 1);
                _dPad[2].MovePosition(true, 1);
                _dPad[3].MovePosition(true, 1);
                break;
            case OptionMenu.DPadPositinoY:
                _dPad[0].MovePosition(false, 1);
                _dPad[1].MovePosition(false, 1);
                _dPad[2].MovePosition(false, 1);
                _dPad[3].MovePosition(false, 1);
                break;
            case OptionMenu.APadPositionX:
                break;
            case OptionMenu.APadPositinoY:
                break;
        }
    }
    public void Button_ValueDown(int optionIndex)
    {

    }

    //슬라이드도 만들어야 하는거쥐? 이거는 ChangeSize(slider value) 넣으면 되지 않나?

    public void CloseOptionWindow()
    {
        //System.GC.Collect();
        UIManager.instance.CloseWindow(this.transform);
    }
}
