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

    // 근처에 있는 적 찾기
    // searchDistance : 적을 탐지하는 범위
    // return : 찾은 플레이어
    protected virtual PlayerController FindEnemy(float searchDistance)
    {        
        // 탐색 범위에 있는 오브젝트 찾기
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // 레이어를 못 찾았을 경우
        {
            Debug.LogWarning("레이어 이름이 유효하지 않습니다: " + layerMask);
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

    // 객체를 오브젝트 위치로 이동
    // to : to 오브젝트 위치로 이동
    protected virtual void MoveTo(GameObject to)
    {
        MoveTo(to.transform.position);
    }

    // 객체를 특정 위치로 이동
    // to : to 위치로 이동
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
