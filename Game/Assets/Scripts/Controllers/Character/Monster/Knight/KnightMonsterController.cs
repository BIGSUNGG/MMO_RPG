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

        if (_isAttacking) // �������̶�� 
            return;

            base.AiControllerUpdate();

        if (_enemy == null || _enemy._health._bDead) // ���� �������� ���� ���ų� �������� ���� �̹� ����ߴٸ�
        {
            // �ֺ��� �ִ� �� ã��
            _enemy = FindEnemy(_enemySearchDistance);
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
                Attack(); // ����
            }
        }
    }
    #endregion

    #region Attack
    public override void OnComboAttackSwing(string attackName) // ���⸦ �ֵθ��� Ÿ�ֿ̹� ȣ��
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

        // ���� ������ �ִ� ������Ʈ ã��
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // ���̾ �� ã���� ���
        {
            Debug.LogWarning("���̾� �̸��� ��ȿ���� �ʽ��ϴ�: " + layerMask);
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

    // attackName : ���� �̸�
    // objectIdArr : ������ ������Ʈ���� ���̵� �迭
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
