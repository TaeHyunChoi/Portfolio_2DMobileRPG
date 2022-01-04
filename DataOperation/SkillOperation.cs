using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillOperation : MonoBehaviour
{
    private static SkillOperation _uniqueInstance;
    public static SkillOperation _instance { get { return _uniqueInstance; } set { _uniqueInstance = value; } }

    //[스킬 이펙트] 스킬 이펙트 생성 델리게이트
    private static SkillEffectPrefab skEffect;
    //[소환] 소환할 몬스터 프리팹
    [SerializeField] GameObject _prfSpawnMonster;

    private void Awake()
    {
        _uniqueInstance = this;
    }

    //[타겟팅] 공격 스킬의 대상이 되는 상대 캐릭터 리스트 반환
    public static List<Pawn> _ltTargetingAttacked(Pawn skillUser, SkillInfos usedSkill, List<Pawn> ltTypicalPawns)
    {
        Dictionary<float, GameObject> dicInnerRangePawns = new Dictionary<float, GameObject>(); //스킬 범위 내에 있는 대상 리스트
        List<Pawn> ltTargets = new List<Pawn>();     //최종으로 반환할 스킬 타겟 리스트

        switch (usedSkill.EffectType)
        {
            case eEffectType.Projectile:
                for (int n = 0; n < ltTypicalPawns.Count; n++)
                {
                    //월드 좌표가 아니라 "skillUserPos"를 기준으로 타겟팅 (부모 오브젝트가 있으므로)
                    Vector3 posSkillUser = skillUser.transform.position + new Vector3(skillUser.GetComponent<Collider2D>().offset.x, skillUser.GetComponent<Collider2D>().offset.y);
                    Vector3 posTarget = ltTypicalPawns[n].transform.position + new Vector3(ltTypicalPawns[n].GetComponent<Collider2D>().offset.x, ltTypicalPawns[n].GetComponent<Collider2D>().offset.y);

                    float tx = (posTarget - posSkillUser).x * 0.5f; //스킬 범위의 기울기는 임의로 0.5f로 설정
                    float ty = (posTarget - posSkillUser).y;
                    bool leftSide = skillUser.IsLeft && (ty < -tx || ty > tx);
                    bool rightSide = !skillUser.IsLeft && (ty > -tx || ty < tx);

                    if (!leftSide && !rightSide)
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(skillUser.transform.position, ltTypicalPawns[n].transform.position);
                    if (dist < usedSkill.fRange && (leftSide || rightSide))
                    {
                        dicInnerRangePawns.Add(dist, ltTypicalPawns[n].gameObject);
                    }
                }
                break;
            case eEffectType.Place:
                //Place는 좌우 상관없이 전부 해당 >> Projectile처럼 좌표값 기준으로 좌우 구역 구분 필요X
                for (int n = 0; n < ltTypicalPawns.Count; n++)
                {
                    float dist = Vector3.Distance(skillUser.transform.position, ltTypicalPawns[n].transform.position);
                    if (dist < usedSkill.fRange)
                    {
                        dicInnerRangePawns.Add(dist, ltTypicalPawns[n].gameObject);
                    }
                }
                break;
        }

        //스킬 범위 내에 대상이 될 수 있는 캐릭터가 있을 경우
        if (dicInnerRangePawns.Count > 0)
        {
            List<float> keyDist = new List<float>();
            foreach (float dist in dicInnerRangePawns.Keys)
            {
                keyDist.Add(dist);
            }
            ArrayTargetDist(ref keyDist, 0, keyDist.Count - 1);

            for (int n = 0; n < keyDist.Count; n++)
            {
                if (ltTargets.Count >= usedSkill.TargetNumber)
                {
                    return ltTargets;
                }
                else
                {
                    ltTargets.Add(dicInnerRangePawns[keyDist[n]].GetComponent<Pawn>());
                }
            }
        }
        return ltTargets;
    }

    //[타겟팅] 스킬 범위 내에서 가장 가까운 타겟 탐색
    private static void ArrayTargetDist(ref List<float> keyDist, int start, int end, bool ascend = true)
    {
        float standard = keyDist[start];
        int left = start, right = end;
        float temp;

        if (ascend)
            while (left < right)
            {
                while (left < right && standard <= keyDist[right])
                    right--;

                if (left > right)
                    break;

                while (left < right && standard >= keyDist[left])
                    left++;
                if (left > right)
                    break;

                temp = keyDist[right];
                keyDist[right] = keyDist[left];
                keyDist[left] = temp;
            }
        else
            while (left < right)
            {
                while (left < right && standard >= keyDist[right])
                    right--;

                if (left > right)
                    break;

                while (left < right && standard <= keyDist[left])
                    left++;
                if (left > right)
                    break;

                temp = keyDist[right];
                keyDist[right] = keyDist[left];
                keyDist[left] = temp;
            }

        temp = keyDist[start];
        keyDist[start] = keyDist[left];
        keyDist[left] = temp;

        if (start + 1 < left)
            ArrayTargetDist(ref keyDist, start, left - 1, ascend);
        if (end > right)
            ArrayTargetDist(ref keyDist, left + 1, end, ascend);
    }

    //[소환] 몬스터 소환 및 스킬을 사용하는 캐릭터에게 '스킬 사용' 입력값 전달
    public static void CallOtherPawns(Pawn skillUser, SkillInfos usedSkill)
    {
        for(int n = 0; n < Random.Range(2,4); n++)
        {
            float distGap = Random.Range(-usedSkill.fRange, usedSkill.fRange);
            Vector3 spawnPos = new Vector3(skillUser.transform.position.x + distGap, skillUser.transform.position.y + distGap);

            PawnMonster monster = Instantiate(_instance._prfSpawnMonster, spawnPos, Quaternion.identity).GetComponent<PawnMonster>();
            IngameManager._instance._ltMonstersInRoom.Add(monster);
            monster.Init_CalledMonster(90,7);
        }

        if (skillUser.GetComponent<PawnMonster>() != null)
        {
            skillUser.GetComponent<PawnMonster>().Input_External(PublicDefines.NowAction.SKILL);
        }
    }

    //[스킬 이펙트] 스킬 이펙트 생성 및 이펙트가 생성될(날아갈) 위치 설정
    public static void PlaySkillEffect(Pawn skillUser, List<Pawn> targetParty, SkillInfos usedSkill)
    {
        //Determine Skill Effect`s Init Location
        foreach (Pawn target in targetParty)
        {
            switch (usedSkill.EffectType)
            {
                case eEffectType.Place:
                    Vector3 offsetTarget = new Vector3(target.transform.GetComponent<Collider2D>().offset.x, target.transform.GetComponent<Collider2D>().offset.y);
                    skEffect = Instantiate(usedSkill.PrbEffect, target.transform.position + offsetTarget, Quaternion.identity).GetComponent<SkillEffectPrefab>();
                    break;
                case eEffectType.Projectile:
                    Vector3 offsetMine = new Vector3(skillUser.transform.GetComponent<Collider2D>().offset.x, skillUser.transform.GetComponent<Collider2D>().offset.y);
                    skEffect = Instantiate(usedSkill.PrbEffect, skillUser.transform.position + offsetMine, Quaternion.identity).GetComponent<SkillEffectPrefab>();
                    break;
                case eEffectType.Front:
                    //필요 없음
                    break;
            }

            Vector3 dir = (skillUser.transform.position - target.transform.position).normalized;


            if (dir.x <= 0) //스킬이펙트의 경우 Sprite를 Left, Right로 따로 저장하지 않았음 → flip 필요
            {
                //이거 잘 안 먹는거 같은데?
                skEffect.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
            }

            //Play Effect Animation On All Target
            skEffect.PlayEffect(skillUser, target, usedSkill.EffectType);
        }
    }


}
