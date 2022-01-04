using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private static InputManager _uniqueInstance;
    public static InputManager _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    PawnPlayer _player;
    public float _playerMoveSpeed { get; set; }
    Vector3 _inputVector;

    //(구글 검색) EventSystem의 Threshold() 조절
    private const float inchToCm = 2.54f;
    [SerializeField] private EventSystem eventSystem = null;
    [SerializeField] private float dragThresholdCM = 0.5f;
    private void SetDragThreshold()
    {
        if (eventSystem != null)
        {
            eventSystem.pixelDragThreshold = (int)(dragThresholdCM * Screen.dpi / inchToCm);
        }
    }

    [SerializeField] Image[] _bttnsMOVE;
    [SerializeField] Transform _tfSkillButton;
    SkillAttackButton[] _bttnSkill;

    public bool _isLeft { get; set; }
    public bool _isRight { get; set; }

    public Color _colorDefault;
    public Color _colorPressed;

    private void Start()
    {
        _uniqueInstance = this;
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PawnPlayer>();
        _playerMoveSpeed = _player.MoveSpeed;

        _colorDefault = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        _colorPressed = new Color(1, 1, 1, 0.5f);

        //스킬 버튼 정보 저장
        _bttnSkill = new SkillAttackButton[_tfSkillButton.childCount];
        for (int n = 0; n < _bttnSkill.Length; n++)
        {
            _bttnSkill[n] = _tfSkillButton.GetChild(n).GetComponent<SkillAttackButton>();
        }

        SetSkillButtons();
        SetDragThreshold();
    }
    private void Update()
    {
        Touch[] touches = Input.touches;
        if (Input.touchCount > 0)
        {
            foreach (Touch t in touches)
            {
                switch (t.phase)
                {
                    case TouchPhase.Began:
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        RaycastHit2D hitInfo = Physics2D.Raycast(t.position, Vector2.zero);
                        break;
                }
            }
        }
        else
        {
            DeactivateMoveButtons();
        }
    }

    //[스킬] 각 스킬 버튼에 연결된 스킬 정보, 사용 캐릭터 정보 등록
    public void SetSkillButtons()
    {
        //Inspector 상에서 Player Party 순번 == 스킬 버튼 순번이므로 별도 조치 없이 n으로 순번 통일
        for (int n = 0; n < IngameManager._instance._ltPartyPawns.Count; n++)
        {
            _bttnSkill[n].InitButtonInfos(n);
        }
    }

    //[이동] 이동 입력값을 받아 Vector3값을 생성/전달
    public void SetInputVector(Vector3 move)
    {
        _inputVector = move;
    }
    public Vector3 GetInputVector()
    {
        return _inputVector;
    }

    //[UI] 이동 키패드의 UI. 활성화되는 조건 및 활성될 때의 색상 결정
    public void ActivateMoveButton()
    {
        float x = _inputVector.x;
        float y = _inputVector.y;

        bool up = (x < 0 && y > 0 && y > 0.5f * x) || (x > 0 && y > 0 && y > -0.5f * x);
        bool down = (x < 0 && y < 0 && y < 0.5f * x) || (x > 0 && y < 0 && y < -0.5f * x);
        bool left = (x < 0 && y < 0 && y > 2 * x) || (x < 0 && y > 0 && y < -2 * x);
        bool right = (x > 0 && y < 0 && y > -2 * x) || (x > 0 && y > 0 && y < 2 * x);

        _bttnsMOVE[(int)PublicDefines.InputDirection.UP].color = up ? _colorPressed : _colorDefault;
        _bttnsMOVE[(int)PublicDefines.InputDirection.DOWN].color = down ? _colorPressed : _colorDefault;
        _bttnsMOVE[(int)PublicDefines.InputDirection.LEFT].color = left ? _colorPressed : _colorDefault;
        _bttnsMOVE[(int)PublicDefines.InputDirection.RIGHT].color = right ? _colorPressed : _colorDefault;
    }

    //[UI] 이동 키패드의 UI. 비활성화가 될 때의 색상 결정
    public void DeactivateMoveButtons()
    {
        for (int n = 0; n < _bttnsMOVE.Length; n++)
        {
            _bttnsMOVE[n].color = _colorDefault;
        }
    }
}
