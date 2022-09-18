using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    public static GameObject prefabMonster;

    //스킬 대상 탐색
    public static List<PawnBase> TargetCount(PawnBase _user)
    {
        List<PawnBase> _targets = new List<PawnBase>();
        IngameManager.instance.RemainPanwns(_user.type, out List<PawnBase> _targetGroup);

        if (_user.Skill.EffectType == eEffectType.Projectile)
        {
            for (int i = 0; i < _targetGroup.Count; i++)
            {
                //월드 좌표 가아닌 'Collider offset' 기준으로 
                Vector2 _offsetUser = _user.GetComponent<Collider2D>().offset;
                Vector2 _offsetTarget = _targetGroup[i].GetComponent<Collider2D>().offset;

                Vector3 _posUser = _user.transform.position + new Vector3(_offsetUser.x, _offsetUser.y, 0);
                Vector3 _posTarget = _targetGroup[i].transform.position + new Vector3(_offsetTarget.x, _offsetTarget.y, 0);

                //스킬 대상 범위를 지정 (y=x, y=-x 기준으로 양옆의 범위만)
                float _tx = (_posTarget - _posUser).x * 0.5f;
                float _ty = (_posTarget - _posUser).y;

                bool _isLeftSide = _user.isLeft && (_ty < -_tx || _ty > _tx);
                bool _isRightSide = !_user.isLeft && (_ty > -_tx || _ty < _tx);

                if (!_isLeftSide && !_isRightSide)
                {
                    continue;
                }

                float _dist = Vector3.Distance(_user.transform.position, _targetGroup[i].transform.position);
                if (_dist < _user.Skill.Range)
                {
                    _targets.Add(_targetGroup[i]);
                }
            }
        }
        else if (_user.Skill.EffectType == eEffectType.Place)
        {
            for (int i = 0; i < _targetGroup.Count; i++)
            {
                float _dist = Vector3.Distance(_user.transform.position, _targetGroup[i].transform.position);
                if (_dist < _user.Skill.Range)
                {
                    _targets.Add(_targetGroup[i]);
                }
            }
        }

        if (_targets.Count <= 0)
        {
            //임시 저장한 몬스터 목록 중에서 가까운 순으로 재정렬
            ArrayCloseTarget(ref _targets, _user.transform.position, 0, _targets.Count - 1);

            //스킬 대상수를 넘어가면 삭제
            for (int i = _user.Skill.TargetNumber; i < _targets.Count; i++)
            {
                _targets.RemoveAt(i);
            }
        }

        return _targets;
    }
    private static void ArrayCloseTarget(ref List<PawnBase> _targets, Vector3 _posUser, int _start, int _end)
    {
        //퀵정렬로 구현 (큰 이유가 있진 않음... 구현해보고 싶어서...)
        float _standard = Vector3.Distance(_posUser, _targets[_start].transform.position);
        int _left = _start;
        int _right = _end;
        PawnBase _temp; //값 스왑용

        //오름차순으로 정렬
        while (_left < _right)
        {
            float _compare = Vector3.Distance(_posUser, _targets[_right].transform.position);
            while (_standard <= _compare)
            {
                --_right;
            }
            if (_left > _right)
            {
                break;
            }

            _compare = Vector3.Distance(_posUser, _targets[_left].transform.position);
            while (_standard >= _compare)
            {
                ++_left;
            }
            if (_left > _right)
            {
                break;
            }

            _temp = _targets[_right];
            _targets[_right] = _targets[_left];
            _targets[_left] = _temp;
        }

        _temp = _targets[_start];
        _targets[_start] = _targets[_left];
        _targets[_left] = _temp;

        if (_start + 1 < _left)
        {
            ArrayCloseTarget(ref _targets, _posUser, _start, _left - 1);
        }
        if (_end > _right)
        {
            ArrayCloseTarget(ref _targets, _posUser, _left + 1, _end);
        }
    }

    //스킬 사용
    public static void UseAtkSkill(PawnBase _user, List<PawnBase> _targets)
    {
        Obj_SkillEffect _skillEffect = null;
        
        //이펙트 오브젝트 생성 및 위치 설정
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_user.Skill.EffectType == eEffectType.Place)
            {
                Vector3 _offsetTarget = new Vector3(_targets[i].transform.GetComponent<Collider2D>().offset.x, _targets[i].transform.GetComponent<Collider2D>().offset.y, 0);
                _skillEffect = UnityEngine.GameObject.Instantiate(_user.Skill.EffectPrefab, _targets[i].transform.position + _offsetTarget, Quaternion.identity).AddComponent<Obj_SkillEffect>();
            }
            else if (_user.Skill.EffectType == eEffectType.Projectile)
            {
                Vector3 _offsetMine = new Vector3(_user.transform.GetComponent<Collider2D>().offset.x, _user.transform.GetComponent<Collider2D>().offset.y, 0);
                _skillEffect = UnityEngine.GameObject.Instantiate(_user.Skill.EffectPrefab, _user.transform.position + _offsetMine, Quaternion.identity).AddComponent<Obj_SkillEffect>();
            }

            //이미지 플립(리소스 > Flip 버전이 없어서 직접 설정)
            Vector3 _dir = (_user.transform.position - _targets[i].transform.position).normalized;
            if(_dir.x <= 0)
            {
                _skillEffect.GetComponentInChildren<SpriteRenderer>().flipX = true;
            }

            //스킬 효과 재생
            _skillEffect.Init(_user, _targets[i], _user.Skill.EffectType);
        }
    }
    public static void UseSpawnSkill(PawnBase _user)
    {
        for (int n = 0; n < Random.Range(2, 4); n++)
        {
            float _range = Random.Range(-_user.Skill.Range, _user.Skill.Range);
            Vector3 _posSpawn = new Vector3(_user.transform.position.x + _range, _user.transform.position.y + _range);

            //(설계 미스) 테이블에서 프리팹 참조값을 넣지 않음. 인스펙터로 직접 추가
            Pwn_Monster _monster = UnityEngine.GameObject.Instantiate(prefabMonster, _posSpawn, Quaternion.identity).AddComponent<Pwn_Monster>();
            IngameManager.instance.Monsters.Add(_monster);

            //직접 초기화
            _monster.Init(Defines.ePawnType.Monster, 9007);
        }
    }
}
