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
public struct PawnStats
{
    public int Index;
    public string Name;
    public int Level;
    public int NowHp;
    public int MaxHp;
    public int Atk;
    public int Def;
    public int SpecAtk;
    public int SpecDef;
    public int Speed;
    public int ProvideExp;
    public int NowExp;
    public int MaxExp;

    public PawnStats(int idx, string nm, int lv, int hp, int atk, int def, int spAtk, int spDef, int spd, int exp, int maxExp)
    {
        Index = idx;
        Name = nm;
        Level = lv;
        MaxHp = NowHp = hp;
        Atk = atk;
        Def = def;
        SpecAtk = spAtk;
        SpecDef = spDef;
        Speed = spd;
        ProvideExp = exp;
        NowExp = 0;
        MaxExp = maxExp;
    }
    public int CalcDamage(PawnStats attacker, int power)
    {
        //데미지 계산
        float CalcStatValue = ((float)((Level * 2) + 10) / 250) * ((float)attacker.Atk / Def);
        int damage = Mathf.RoundToInt((CalcStatValue) * (power + 2) * Random.Range(0.85f, 1f));

        //HP 반영
        NowHp = (NowHp - damage) <= 0 ? 0 : (NowHp - damage);
        return damage;
    }
}

public class PawnDataTable : LowBase
{
    public string _mainKey = "Index";
    public override void Load(string jsonData) 
    {
        //데이터를 읽어서 + 상속받은 LowBase > Dic에 저장
        JSONNode node = JSONNode.Parse(jsonData);

        for (int n = 0; n < (int)eTableIndex_Pawn.max_cnt; n++)
        {
            eTableIndex_Pawn subKey = (eTableIndex_Pawn)n;
            if (string.Compare(_mainKey, subKey.ToString()) != 0)
            {
                for (int m = 0; m < node[0].AsArray.Count; m++) //(엑셀 기준) 한 행씩 저장한다.
                { 
                    Add(node[0][m][_mainKey], subKey.ToString(), node[0][m][subKey.ToString()].Value);
                }
            }
        }
    }
}
