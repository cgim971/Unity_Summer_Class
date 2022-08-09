using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공통적인 데이터 클래스입니다.
/// 1. 데이터 총 갯수
/// 2. 데이터 목록
/// 3. 추가 삭제 복사
/// </summary>
public class DefaultData : ScriptableObject
{
    //ScriptableObject : 대량의 데이터를 저장하는데 사용할 수 있는 데이터 컨테이너

    public const string pathData = "Resources/Data/";
    // 고유 아이디 속성
    public string[] idx = null;
    public DefaultData() { }

    // 총 갯수 가져오기
    public int getDataCnt()
    {
        int _retCnt = 0;

        if (this.idx != null)
        {
            _retCnt = this.idx.Length;
        }

        return _retCnt;
    }

    public string[] getDataIdxList(bool flagID, string strFilter = "")
    {
        string[] retList = new string[0];
        if (this.idx == null)
        {
            return retList;
        }

        retList = new string[this.idx.Length];

        for (int i = 0; i < this.idx.Length; i++)
        {
            if (strFilter != "")
            {
                if (idx[i].ToLower().Contains(strFilter.ToLower()) == false)
                {
                    continue;
                }
            }

            if (flagID)
            {
                retList[i] = "[" + i.ToString() + "]" + this.idx[i];
            }
            else
            {
                retList[i] = this.idx[i];
            }
        }

        return retList;
    }

    public virtual int constructorData(string _dataIdx)
    {
        return getDataCnt();
    }

    public virtual void deleteData(int _pid) { }

    public virtual void defulicateData(int _pid) { }
}
