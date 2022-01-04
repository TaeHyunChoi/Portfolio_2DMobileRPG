using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowBase : MonoBehaviour
{
    protected Dictionary<string, Dictionary<string, string>> _dicNODE = new Dictionary<string, Dictionary<string, string>>();
    public Dictionary<string, Dictionary<string, string>> NodeDic { get { return _dicNODE; } }

    public int MaxCount()
    {
        return _dicNODE.Count;
    }
    public virtual void Load(string jsonData) //(개별) 시트 자료 불러오기
    {
        //상속 받은 클래스에서 재정의 : 그래서 내용 비움.
    }
    public void Add(string index, string column, string value) //각 데이터 테이블에 추가하기
    {
        if (!_dicNODE.ContainsKey(index))
        { 
            _dicNODE.Add(index, new Dictionary<string, string>());
        }
        if (!_dicNODE[index].ContainsKey(column))
        {                                         
            _dicNODE[index].Add(column, value);
        }
    }

    //자료형 변환
    public string ToString(int index, string colName)
    {
        string findValue = string.Empty;
        string sIndex = index.ToString();

        if (_dicNODE.ContainsKey(sIndex))
            _dicNODE[sIndex].TryGetValue(colName, out findValue);

        return findValue;
    }
    public int ToInteger(int index, string colName)
    {
        int value = 0;
        string findValue = ToString(index, colName);
        int.TryParse(findValue, out value);

        return value;
    }
    public float ToFloat(int index, string colName)
    {
        float value = 0;
        string findValue = ToString(index, colName);
        float.TryParse(findValue, out value);

        return value;
    }
    public bool ToBool(int index, string colName)
    {
        int value = 0;
        string findValue = ToString(index, colName);
        int.TryParse(findValue, out value);

        if (value == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
