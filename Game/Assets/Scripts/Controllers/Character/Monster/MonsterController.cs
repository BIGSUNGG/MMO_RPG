using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class MonsterController : CharacterController
{
    protected override void Start()
    {
        base.Start();   
    }

    protected override void Update()
    {
        base.Update();

        AiControllerUpdate();
    }

    #region Ai
    protected virtual void AiControllerUpdate()
    {
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
            if (pc && pc == this)
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
        transform.LookAt(to);
    }
    #endregion
}
