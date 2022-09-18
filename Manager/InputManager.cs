using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }

    //이동 입력
    [SerializeField] public Transform tfSkillButton;
    private ObjUI_SkillButton[] btnSkill;
    private Vector3 inputVector;

    //공격 입력
    public bool inputAtk { get; private set; }

    //(구글 검색) EventSystem의 Threshold() 조절
    private const float inchToCm = 2.54f;
    [SerializeField] private EventSystem eventSystem = null;
    [SerializeField] private float dragThresholdCM = 0.5f;


    private void Awake()
    {
        instance = this;

        //스킬 버튼 정보 저장
        btnSkill = new ObjUI_SkillButton[tfSkillButton.childCount];
        for (int n = 0; n < btnSkill.Length; n++)
        {
            btnSkill[n] = tfSkillButton.GetChild(n).GetComponent<ObjUI_SkillButton>();
            btnSkill[n].InitButtonInfos(10 + n);
        }

        SetDragThreshold();
    }
    private void SetDragThreshold()
    {
        if (eventSystem != null)
        {
            eventSystem.pixelDragThreshold = (int)(dragThresholdCM * Screen.dpi / inchToCm);
        }
    }



    private void Update()
    {
        Touch[] touches = Input.touches;
        if (Input.touchCount > 0)
        {
            RaycastHit2D _hitInfo;
            foreach (Touch t in touches)
            {
                switch (t.phase)
                {
                    case TouchPhase.Began:
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        _hitInfo = Physics2D.Raycast(t.position, Vector2.zero);
                        break;
                }
            }
        }
        else
        {
            UpdateUI_MoveButton(Vector3.zero);
        }
    }

    public static void SetInputVector(Vector3 _vec)
    {
        instance.inputVector = _vec;
        instance.UpdateUI_MoveButton(_vec);
    }
    public static Vector3 GetInputVector()
    {
        return instance.inputVector;
    }
    public void UpdateUI_MoveButton(Vector3 _vec)
    {
        float x = _vec.x;
        float y = _vec.y;
        bool[] _isMoving = new bool[4];
        /*Up*/    _isMoving[0] = (x < 0 && y > 0 && y > 0.5f * x) || (x > 0 && y > 0 && y > -0.5f * x);
        /*Down*/  _isMoving[1] = (x < 0 && y < 0 && y < 0.5f * x) || (x > 0 && y < 0 && y < -0.5f * x);
        /*Left*/  _isMoving[2] = (x < 0 && y < 0 && y > 2 * x) || (x < 0 && y > 0 && y < -2 * x);
        /*Right*/ _isMoving[3] = (x > 0 && y < 0 && y > -2 * x) || (x > 0 && y > 0 && y < 2 * x);

        UIManager.instance.DisplayMoveButton(_isMoving);
    }



    public static void InputAttack(bool _input)
    {
        instance.inputAtk = _input;
    }
}
