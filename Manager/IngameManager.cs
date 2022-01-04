using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    #region 필드
    private static IngameManager _uniqueInstance;
    public static IngameManager _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    //플레이어 캐릭터 정보
    public PawnPlayer _player { get; set; }
    
    //플레이어, 동료 캐릭터 정보를 저장한 List
    public List<Pawn> _ltPartyPawns { get; set; }
    
    //현재 전투 지역에 있는 몬스터 정보 List
    public List<Pawn> _ltMonstersInRoom { get; set; }

    //현재 전투 상태 여부
    public bool NowBattle { get; set; }
    #endregion

    //[전투 관련] 전투 지역 정보
    [SerializeField] private Transform _tfBttlRooms;

    //[전투 관련] 드롭 아이템 프리팹
    [SerializeField] public GameObject _prfDropItem;

    //[전투 관련] 전투 매니저 스크립트
    private BattleManager _battleMNG;

    //[UI] 체력창 표시 델리게이트
    public delegate void DisplayNowHP(PawnStats stats);
    public DisplayNowHP _uiDisplayNowHP;

    //[사운드] 
    SoundManager _soundMNG;

    //[초기화] 인게임 진입 시 객체 초기화의 순서를 여기서 제어하고자 함.
    private void Awake()
    {
        //Ingame Manager > static하게 설정
        _uniqueInstance = this;

        //인게임에서 관리할 list 생성
        _ltPartyPawns = new List<Pawn>();
        _ltMonstersInRoom = new List<Pawn>();

        //json 데이터 테이블 불러오기
        DataTableManager._instance.Init_LoadSheet();

        //Battle Manager 생성
        _battleMNG = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        _battleMNG.Init_BattleManager();

        //Sound 연결 
        _soundMNG = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        _soundMNG.Init_SoundManager();

        //ui 연결
        UIManager _uiMNG = GameObject.FindGameObjectWithTag("BattleUI").GetComponent<UIManager>();
        _uiDisplayNowHP = _uiMNG.DisplayPartyHP;

        //Player Party 정보 생성 (Monster는 방에 입장할 때마다 생성함)
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PawnPlayer>();
        _player.Init_Player();
        GameObject.FindGameObjectWithTag("Friend1").GetComponent<PawnFellower>().Init_Fellow();
        GameObject.FindGameObjectWithTag("Friend2").GetComponent<PawnFellower>().Init_Fellow();
    }

    //스크립트 정보 생성+연결은 Awake()에서, 행동 개시는 Start()부터!
    private void Start()
    {
        //BGM 재생
        _soundMNG.PlaySound_BGM(BgmIndex.DevilsVelly);

        //매 프레임마다 스킬 사용 조건 확인 (ex. 스킬 쿨타임)
        StartCoroutine(_battleMNG.CheckSkillCondition());
    }

    //(전투 중이라면) 현재 전투 상황의 종료를 확인
    private void Update()
    {
        if (NowBattle)
        {
            if (_ltMonstersInRoom.Count <= 0 || _player.Stats.NowHp <= 0)
            {
                BattleManager._instance.SetBattle(false);
            }
        }
    }
}
