using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class KnightMonsterController : MonsterController
{
    public KnightMonsterController()
    {
        ObjectType = GameObjectType.KnightMonster;
    }

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
    protected float _enemyAttackDistance = 4.0f;
    protected float _enemyMaxDistance = 6.0f;
    protected float _enemySearchDistance = 4.5f;
    
    protected float _aiAttackDelay = 3.25f;
    protected float _aiCurAttackDelay = 0.0f;

    protected override void AiControllerUpdate()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        if(_aiCurAttackDelay > 0.0f)
        {
            _aiCurAttackDelay -= Time.deltaTime;
        }

        if (_isAttacking) // 공격중이라면 
            return;

        base.AiControllerUpdate();

        if (_enemy == null || _enemy.Health._bDead) // 현재 추적중인 적이 없거나 추적중인 적이 이미 사망했다면
        {
            // 주변에 있는 적 찾기
            _enemy = FindEnemy(_enemySearchDistance);
            if(_enemy == null && Vector3.Distance(this.transform.position, _spawnPosition) > 4.5f) // 주변에 적이 없고 스폰 위치에서 떨어져있다면
            {
                MoveTo(_spawnPosition); // 스폰 위치로 돌아가기
            }
            else
            {
                _moveDir = Vector3.zero;
            }
        }
        else // 추적중인 적이 있다면
        {
            Vector3 distanceVec = _enemy.transform.position - this.transform.position;
            float distance = Mathf.Abs(distanceVec.magnitude);

            if(distance > _enemyMaxDistance) // 적이 최대 거리보다 멀리 떨어져있다면
            {
                // 새로운 주변에 있는 적 찾기
                _enemy = FindEnemy(_enemySearchDistance);
                if(_enemy == null)
                {
                    _moveDir = Vector3.zero;
                    return;
                }
            }

            if (distance > _enemyAttackDistance) // 적과의 거리가 가깝지 않다면
            {
                // 적 바라보기
                LookAt(_enemy.gameObject);

                // 적에게 다가가기
                MoveTo(_enemy.gameObject);
            }
            else // 가깝다면
            {
                // 멈추기
                _moveDir = Vector3.zero;

                if(_aiCurAttackDelay <= 0.0f)
                {
                    _aiCurAttackDelay = _aiAttackDelay;
                    Attack(); // 공격                
                }
            }
        }
    }
    #endregion

    #region Attack
    public override void OnServer_ComboAttackSwing(string attackName) // 무기를 휘두르는 타이밍에 호출
    {
        base.OnServer_ComboAttackSwing(attackName);

        if (Util.CheckFuncCalledOnServer() == false)
            return;

        switch (attackName)
        {
            case "1":
                break;
            default:
                Debug.LogWarning("Recv wrong attack name");
                return;
        }

        // 공격 범위에 있는 오브젝트 찾기
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // 레이어를 못 찾았을 경우
        {
            Debug.LogWarning("레이어 이름이 유효하지 않습니다: " + layerMask);
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + new Vector3(0.0f, Capsule.height / 2, 0.0f), 2.0f, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            CharacterController cc = hitCollider.gameObject.GetComponentInParent<CharacterController>();
            if (cc == null || cc == this || cc._characterType == this._characterType)
                continue;

            gameObject.GiveDamage(cc, Random.Range(5, 10));
        }
    }
    #endregion
}
