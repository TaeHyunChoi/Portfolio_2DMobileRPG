using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.IO;

//데이터 테이블 인덱스
public enum eSheet_Pawn
{
    Pawn,
    Skill
}
public class DataTableManager : TSingleton<DataTableManager>
{
    //테이블 : 캐릭터별, 레벨별 능력치
    Dictionary<eSheet_Pawn, LowBase> _dicPawnSheet;

    //테이블 : 캐릭터별 스킬 정보
    Dictionary<eSheet_Pawn, LowBase> _dicSkillSheet;

    private void Awake()
    {
        base.Init();
    }

    //[불러오기] json 파일에서 정보 불러오기. 개별 sheet의 내용도 함께 저장.
    public void Init_LoadSheet()
    {
        _dicPawnSheet = new Dictionary<eSheet_Pawn, LowBase>();
        LoadSheet_Pawn<PawnDataTable>(eSheet_Pawn.Pawn);

        _dicSkillSheet = new Dictionary<eSheet_Pawn, LowBase>();
        LoadSheet_Skill<SkillDataTable>(eSheet_Pawn.Skill);
    }
    private LowBase LoadSheet_Pawn<T>(eSheet_Pawn info) where T : LowBase, new()
    {
        if (_dicPawnSheet.ContainsKey(info))
        {
            LowBase lBase = _dicPawnSheet[info];
            return lBase;
        }
        TextAsset TextAsset = Resources.Load("Json/" + info.ToString()) as TextAsset;
        if (TextAsset != null)
        {
            T tLow = new T();
            tLow.Load(TextAsset.text);
            _dicPawnSheet.Add(info, tLow);
        }
        return _dicPawnSheet[info];
    }
    private LowBase LoadSheet_Skill<T>(eSheet_Pawn info) where T : LowBase, new()
    {
        if (_dicSkillSheet.ContainsKey(info))
        {
            LowBase lBase = _dicSkillSheet[info];
            return lBase;
        }
        TextAsset TextAsset = Resources.Load("Json/" + info.ToString()) as TextAsset;
        if (TextAsset != null)
        {
            T tLow = new T();
            tLow.Load(TextAsset.text);
            _dicSkillSheet.Add(info, tLow);
        }
        return _dicSkillSheet[info];
    }

    //[가져오기] json에서 저장한 정보를 게임으로 가져오기
    public LowBase GetTableInfo(eSheet_Pawn info)
    {
        if (_dicPawnSheet.ContainsKey(info))
        {
            return _dicPawnSheet[info];
        }
        else if (_dicSkillSheet.ContainsKey(info))
        {
            return _dicSkillSheet[info];
        }

        return null;
    }
}
