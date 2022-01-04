using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualPadControl : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler
{
    [SerializeField] PublicDefines.InputDirection _dir;
    InputManager _inputMNG;
    RectTransform _rootPad;

    Vector3 _inputVector;
    bool _nowInput;

    private void Start()
    {
        _rootPad = transform.parent.GetComponent<RectTransform>();
        _inputMNG = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
    }
    private void Update()
    {
        if (_nowInput)
        {
            _inputMNG.SetInputVector(_inputVector);     //이동 입력값을 받으면
            _inputMNG.ActivateMoveButton();             //값에 맞춰 버튼 활성화
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        _nowInput = true;
        Vector2 pos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rootPad, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / _rootPad.sizeDelta.x);
            pos.y = (pos.y / _rootPad.sizeDelta.y);

            _inputVector = new Vector3(pos.x, pos.y, 0);
            _inputVector = (_inputVector.magnitude > 1) ? _inputVector.normalized : _inputVector;
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _nowInput = false;
        _inputVector = Vector3.zero;
        _inputMNG.SetInputVector(_inputVector);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerUp(eventData);
    }
}
