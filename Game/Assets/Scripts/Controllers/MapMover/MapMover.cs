using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMover : ObjectController
{
    public Vector3 boxCenter; // 박스의 중심점
    public Vector3 boxSize; // 박스의 크기
    public Quaternion boxOrientation = Quaternion.identity; // 박스의 방향

    public int moveMapId = 0; // 이동할 맵 아이디
    protected TimerHandler collisionTimer;    

    protected override void Start()
    {
        base.Start();

        if(Managers.Network.IsServer)
            collisionTimer = Managers.Timer.SetTimer(0.25f, CheckCollision, true);
    }

    protected override void Update()
    {
        base.Update();

    }
     
    protected virtual void CheckCollision()
    {
        if (Managers.Network.IsClient)
            return;

        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // 레이어를 못 찾았을 경우
        {
            Debug.LogWarning("레이어 이름이 유효하지 않습니다: " + layerMask);
            return;
        }
        Collider[] colliders = Physics.OverlapBox(boxCenter + transform.position, boxSize / 2, boxOrientation, layerMask);
        foreach (Collider collider in colliders)
        {
            PlayerController pc = collider.GetComponentInParent<PlayerController>();
            if (pc == null)
                continue;

            ClientSession clientSession = pc.Session;
            if (clientSession == null)
                continue;

            clientSession.MoveMap(moveMapId);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter + transform.position, boxOrientation, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
