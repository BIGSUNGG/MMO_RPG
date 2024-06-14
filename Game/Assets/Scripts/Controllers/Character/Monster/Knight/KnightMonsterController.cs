using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

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
    protected float _enemyAttackDistance = 1.5f;
    protected float _enemyMaxDistance = 5.0f;
    protected float _enemySearchDistance = 3.0f;

    protected override void AiControllerUpdate()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        if (_isAttacking) // 공격중이라면 
            return;

            base.AiControllerUpdate();

        if (_enemy == null || _enemy._health._bDead) // 현재 추적중인 적이 없거나 추적중인 적이 이미 사망했다면
        {
            // 주변에 있는 적 찾기
            _enemy = FindEnemy(_enemySearchDistance);
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
                Attack(); // 공격
            }
        }
    }
    #endregion

    #region Attack
    public override void OnComboAttackSwing(string attackName) // 무기를 휘두르는 타이밍에 호출
    {
        base.OnComboAttackSwing(attackName);

        if (IsLocallyControlled() == false)
            return;

        if (_isAttacking == false)
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

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + new Vector3(0.0f, _capsule.height / 2, 0.0f), 2.0f, layerMask);
        List<int> objectIdList = new List<int>();

        foreach (var hitCollider in hitColliders)
        {
            ObjectController oc = hitCollider.gameObject.GetComponentInParent<ObjectController>();
            if (oc && oc == this)
                continue;

            objectIdList.Add(oc.ObjectId);
        }

        if (objectIdList.Count > 0)
            Server_ComboAttackResult(attackName, objectIdList);
    }

    // attackName : 공격 이름
    // objectIdArr : 공격할 오브젝트들의 아이디 배열
    protected override void Server_ComboAttackResult_Implementation(string attackName, List<int> objectIdArr)
    {
        base.Server_ComboAttackResult_Implementation(attackName, objectIdArr);

        List<ObjectController> objects = new List<ObjectController>();
        foreach (int id in objectIdArr)
        {
            GameObject go = Managers.Object.FindById(id);
            if (go == null)
                continue;

            ObjectController oc = go.GetComponent<ObjectController>();
            if (oc == null)
                continue;

            gameObject.GiveDamage(oc, Random.Range(5, 10));
        }
    }
    #endregion
}
