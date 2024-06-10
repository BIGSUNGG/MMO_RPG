using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    protected float _enemyMinDistance = 1.5f;
    protected float _enemyMaxDistance = 5.0f;
    protected float _enemySearchLength = 3.0f;

    protected override void AiControllerUpdate()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        base.AiControllerUpdate();

        if (_enemy == null) // ���� �������� ���� ������
        {
            // �ֺ��� �ִ� �� ã��
            _enemy = FindEnemy(_enemySearchLength);
        }
        else // �������� ���� �ִٸ�
        {
            Vector3 distanceVec = _enemy.transform.position - this.transform.position;
            float distance = Mathf.Abs(distanceVec.magnitude);

            if(distance > _enemyMaxDistance) // ���� �ִ� �Ÿ����� �ָ� �������ִٸ�
            {
                // ���ο� �ֺ��� �ִ� �� ã��
                _enemy = FindEnemy(_enemySearchLength);
                if(_enemy == null)
                {
                    _moveDir = Vector3.zero;
                    return;
                }
            }

            // �� �ٶ󺸱�
            LookAt(_enemy.gameObject);

            if (distance > _enemyMinDistance) // ������ �Ÿ��� ������ �ʴٸ�
            {
                // ������ �ٰ�����
                MoveTo(_enemy.gameObject);
            }
            else // �����ٸ�
            {
                // ���߱�
                _moveDir = Vector3.zero;
            }

            Debug.Log($"{distance}, {_moveDir}");
        }
    }
    #endregion
}
