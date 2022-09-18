using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjUI_NormalAtkButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    RectTransform _rectPad;
    Image _imgATK;

    private float _height;
    private float _width;

    private void Awake()
    {
        _imgATK = transform.GetComponent<Image>();
        _imgATK.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        _rectPad = this.GetComponent<RectTransform>();
        _height = _rectPad.rect.height;
        _width = _rectPad.rect.width;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _imgATK.color = UIManager.PressedColor;
        InputManager.InputAttack(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _imgATK.color = UIManager.DefaultColor;
        InputManager.InputAttack(false);
    }

    public void ChangeSize(int _delta)
    {
        _rectPad.sizeDelta = (_rectPad.sizeDelta.x < _width * 1.5f && _rectPad.sizeDelta.x > _width * 0.5f) ?
           new Vector2(_width * (50 + _delta) * 0.01f, _height * (50 + _delta) * 0.01f) : _rectPad.sizeDelta;
    }
}
