using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CharacterController
{
    public MonsterController()
    {
        _characterType = CharacterType.Monster;
    }

    protected override void Start()
    {
        base.Start();   

        if(Managers.Network.IsServer)
        {
            Managers.Object.Register(this);
        }
    }

    protected override void Update()
    {
        base.Update();

        if(Managers.Network.IsServer)
            AiControllerUpdate();
    }

    #region Controller
    public override bool IsLocallyControlled()
    {
        if (Managers.Network.IsServer)
            return true;

        return base.IsLocallyControlled();
    }
    #endregion

    #region Ai
    protected virtual void AiControllerUpdate()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;
    }

    // ��ó�� �ִ� �� ã��
    // searchDistance : ���� Ž���ϴ� ����
    // return : ã�� �÷��̾�
    protected virtual PlayerController FindEnemy(float searchDistance)
    {        
        // Ž�� ������ �ִ� ������Ʈ ã��
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // ���̾ �� ã���� ���
        {
            Debug.LogWarning("���̾� �̸��� ��ȿ���� �ʽ��ϴ�: " + layerMask);
            return null;
        }

        PlayerController findPlayer = null;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + new Vector3(0.0f, _capsule.height / 2, 0.0f), searchDistance, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            PlayerController pc = hitCollider.gameObject.GetComponentInParent<PlayerController>();
            if (pc == null || pc._health._bDead)
                continue;            

            findPlayer = pc;
        }

        return findPlayer;
    }

    // ��ü�� ������Ʈ ��ġ�� �̵�
    // to : to ������Ʈ ��ġ�� �̵�
    protected virtual void MoveTo(GameObject to)
    {
        MoveTo(to.transform.position);
    }

    // ��ü�� Ư�� ��ġ�� �̵�
    // to : to ��ġ�� �̵�
    protected virtual void MoveTo(Vector3 to)
    {
        if (CanMove() == false)
            return;

        Vector3 dir = to - transform.position;
        _moveDir.x = dir.x;
        _moveDir.y = dir.z;
        _moveDir.Normalize();
    }

    protected virtual void LookAt(GameObject to)
    {
        LookAt(to.transform.position);
    }

    protected virtual void LookAt(Vector3 to)
    {
        if (CanRotate() == false)
            return;

        transform.LookAt(to);
        transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
    }
    #endregion
}
