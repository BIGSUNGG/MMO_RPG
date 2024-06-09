using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class KnightMonsterController : MonsterController
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    #region Ai
    protected PlayerController _enemy = null;
    protected float _enemyMinDistance = 1.5f;
    protected float _enemyMaxDistance = 5.0f;
    protected float _enemySearchLength = 3.0f;

    protected override void AiControllerUpdate()
    {
        base.AiControllerUpdate();

        if (_enemy == null) // 현재 추적중인 적이 없으면
        {
            // 주변에 있는 적 찾기
            _enemy = FindEnemy(_enemySearchLength);
        }
        else // 추적중인 적이 있다면
        {
            Vector3 distanceVec = _enemy.transform.position - this.transform.position;
            float distance = Mathf.Abs(distanceVec.magnitude);

            if(distance > _enemyMaxDistance) // 적이 최대 거리보다 멀리 떨어져있다면
            {
                // 새로운 주변에 있는 적 찾기
                _enemy = FindEnemy(_enemySearchLength);
                if(_enemy == null)
                {
                    _moveDir = Vector3.zero;
                    return;
                }
            }

            // 적 바라보기
            LookAt(_enemy.gameObject);

            if (distance > _enemyMinDistance) // 적과의 거리가 가깝지 않다면
            {
                // 적에게 다가가기
                MoveTo(_enemy.gameObject);
            }
            else // 가깝다면
            {
                // 멈추기
                _moveDir = Vector3.zero;
            }

            Debug.Log($"{distance}, {_moveDir}");
        }
    }
    #endregion
}
