using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.IO;

public enum eTableIndex_Pawn
{
    Index,
    Name,
    Level,
    MaxHp,
    Atk,
    Def,
    SpecAtk,
    SpecDef,
    Speed,
    ProvideExp,
    MaxExp,
    max_cnt
}
public class PawnDataTable : LowBase
{
    public string mainKey = "Index";
    public override void Load(string _jsonData) 
    {
        //데이터를 읽어서 + 상속받은 LowBase > Dic에 저장
        JSONNode _node = JSONNode.Parse(_jsonData);

        for (int n = 0; n < (int)eTableIndex_Pawn.max_cnt; n++)
        {
            eTableIndex_Pawn _subKey = (eTableIndex_Pawn)n;
            if (string.Compare(mainKey, _subKey.ToString()) != 0)
            {
                for (int m = 0; m < _node[0].AsArray.Count; m++) //(엑셀 기준) 한 행씩 저장한다.
                { 
                    Add(_node[0][m][mainKey], _subKey.ToString(), _node[0][m][_subKey.ToString()].Value);
                }
            }
        }
    }
}