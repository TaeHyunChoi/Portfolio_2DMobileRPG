using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjUI_SkillButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectPad;

    private Image imgSKILL;
    private PawnBase linkedPawn;
    private float height;
    private float width;

    private void Awake()
    {
        imgSKILL = transform.GetComponent<Image>();
        imgSKILL.color = UIManager.DefaultColor;

        rectPad = this.GetComponent<RectTransform>();
        height = rectPad.rect.height;
        width = rectPad.rect.width;
    }

    //각 스킬 버튼에 플레이어, 동료AI 참조 연결
    public void InitButtonInfos(int _pawnTypeIndex)
    {
        linkedPawn = IngameManager.instance.Fellows.Find(x => x.Stat.TypeIndex == _pawnTypeIndex);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //linked Pawn에게 입력값을 넘기면 > Pawn 쪽에서 스킬 사용 여부를 확인
        linkedPawn.InputAction(Defines.eAct.SKILL);

        //버튼 색상 변경
        imgSKILL.color = UIManager.PressedColor;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        imgSKILL.color = UIManager.DefaultColor;
    }
    public void ChangeSize(int _delta)
    {
        rectPad.sizeDelta = (rectPad.sizeDelta.x < width * 1.5f && rectPad.sizeDelta.x > width * 0.5f) ?
           new Vector2(width * (50 + _delta) * 0.01f, height * (50 + _delta) * 0.01f) : rectPad.sizeDelta;
    }
}
