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

        if (_isAttacking) // �������̶�� 
            return;

        base.AiControllerUpdate();

        if (_enemy == null || _enemy.Health._bDead) // ���� �������� ���� ���ų� �������� ���� �̹� ����ߴٸ�
        {
            // �ֺ��� �ִ� �� ã��
            _enemy = FindEnemy(_enemySearchDistance);
            if(_enemy == null && Vector3.Distance(this.transform.position, _spawnPosition) > 4.5f) // �ֺ��� ���� ���� ���� ��ġ���� �������ִٸ�
            {
                MoveTo(_spawnPosition); // ���� ��ġ�� ���ư���
            }
            else
            {
                _moveDir = Vector3.zero;
            }
        }
        else // �������� ���� �ִٸ�
        {
            Vector3 distanceVec = _enemy.transform.position - this.transform.position;
            float distance = Mathf.Abs(distanceVec.magnitude);

            if(distance > _enemyMaxDistance) // ���� �ִ� �Ÿ����� �ָ� �������ִٸ�
            {
                // ���ο� �ֺ��� �ִ� �� ã��
                _enemy = FindEnemy(_enemySearchDistance);
                if(_enemy == null)
                {
                    _moveDir = Vector3.zero;
                    return;
                }
            }

            if (distance > _enemyAttackDistance) // ������ �Ÿ��� ������ �ʴٸ�
            {
                // �� �ٶ󺸱�
                LookAt(_enemy.gameObject);

                // ������ �ٰ�����
                MoveTo(_enemy.gameObject);
            }
            else // �����ٸ�
            {
                // ���߱�
                _moveDir = Vector3.zero;

                if(_aiCurAttackDelay <= 0.0f)
                {
                    _aiCurAttackDelay = _aiAttackDelay;
                    Attack(); // ����                
                }
            }
        }
    }
    #endregion

    #region Attack
    public override void OnServer_ComboAttackSwing(string attackName) // ���⸦ �ֵθ��� Ÿ�ֿ̹� ȣ��
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

        // ���� ������ �ִ� ������Ʈ ã��
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // ���̾ �� ã���� ���
        {
            Debug.LogWarning("���̾� �̸��� ��ȿ���� �ʽ��ϴ�: " + layerMask);
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
