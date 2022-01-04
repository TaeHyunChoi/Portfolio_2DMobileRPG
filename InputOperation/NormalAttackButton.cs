using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NormalAttackButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    InputManager _inputMNG;
    Image _imgATK;

    private void Awake()
    {
        _inputMNG = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
        _imgATK = transform.GetComponent<Image>();
        _imgATK.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _imgATK.color = _inputMNG._colorPressed;
        PawnPlayer._instance.InputAttack(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _imgATK.color = _inputMNG._colorDefault;
        PawnPlayer._instance.InputAttack(false);
    }
}
