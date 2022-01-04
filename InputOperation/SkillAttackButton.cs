using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillAttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    InputManager _inputMNG;

    [SerializeField] string _skillPawn;
    private List<Pawn> _ltPlayerParty;
    private Image _imgSKILL;
    public string SkillPawn { get { return _skillPawn; } set { _skillPawn = value; } }
    private int _pawnIndex;

    private void Start()
    {
        _inputMNG = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
        _imgSKILL = transform.GetComponent<Image>();
        _imgSKILL.color = _inputMNG._colorDefault;
        _ltPlayerParty = IngameManager._instance._ltPartyPawns;
    }

    //[초기화] 각 스킬 버튼에 플레이어, 동료AI 인덱스 번호를 입력
    public void InitButtonInfos(int pawnIndex)
    {
        _pawnIndex = pawnIndex;
    }

    //스킬 사용이 가능할 때에 스킬 버튼을 누르면 → 해당 캐릭터에게 스킬 입력을 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        _imgSKILL.color = _inputMNG._colorPressed;

        if (_ltPlayerParty[_pawnIndex]._skill.PossibleUse)
        {
            if (_ltPlayerParty[_pawnIndex].PawnType == PublicDefines.ePawnType.Player)
            {
                _ltPlayerParty[_pawnIndex].InputState = PublicDefines.NowAction.SKILL;
            }
            else
            {
                _ltPlayerParty[_pawnIndex].GetComponent<PawnFellower>().Input_External(PublicDefines.NowAction.SKILL);
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _imgSKILL.color = _inputMNG._colorDefault;
    }
}
