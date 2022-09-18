using UnityEngine;
using UnityEngine.EventSystems;

public class ObjUI_VirtualPad : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler
{
    [SerializeField] Defines.InputDirection dir;
    private RectTransform rootPad;

    private Vector3 inputVector;
    private bool nowInput;

    private float height;
    private float width;

    private RectTransform rect;

    private void Awake()
    {
        rootPad = transform.GetComponentInParent<RectTransform>();

        height = rootPad.rect.height;
        width = rootPad.rect.width;

        rect = this.GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (nowInput)
        {
            InputManager.SetInputVector(inputVector);                  //이동 입력값 저장
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        nowInput = true;
        Vector2 pos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootPad, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / rootPad.sizeDelta.x);
            pos.y = (pos.y / rootPad.sizeDelta.y);

            inputVector = new Vector3(pos.x, pos.y, 0);
            inputVector = (inputVector.magnitude > 1) ? inputVector.normalized : inputVector;
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        nowInput = false;
        InputManager.SetInputVector(Vector3.zero);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerUp(eventData);
    }

    public void ChangeSize(int _delta)
    {
        rootPad.sizeDelta = (rootPad.sizeDelta.x < width * 1.5f && rootPad.sizeDelta.x > width * 0.5f) ?
           new Vector2(width * (50 + _delta) * 0.01f, height * (50 + _delta) * 0.01f) : rootPad.sizeDelta;
    }
    public void MovePosition(bool _moveXaxis, int _delta)
    {
        rect.localPosition = (_moveXaxis) ?
                              new Vector2(rect.localPosition.x + _delta, rect.localPosition.y):
                              new Vector2(rect.localPosition.x, rect.localPosition.y + _delta);
    }
}
