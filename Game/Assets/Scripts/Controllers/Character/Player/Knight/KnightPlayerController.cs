using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;
using Random = UnityEngine.Random;

public class KnightPlayerController : PlayerController
{
	public KnightPlayerController _knightAnim { get; protected set; } = null;

    public KnightPlayerController()
    {
        ObjectType = GameObjectType.KnightPlayer;
    }

    protected override void Start()
	{
		base.Start();

		_knightAnim = GetComponent<KnightPlayerController>();
		if (_knightAnim == null)
			Debug.LogWarning("KnightController is null");

	}

	protected override void Update()
	{
		base.Update();

	}

    #region Controller
    public override void ControllerUpdate()
	{
		if (IsLocallyControlled() == false)
			return;

        base.ControllerUpdate();

		if (Input.GetMouseButtonDown(0))
		{
			Attack();
		}
	}

	public override bool CanRotate()
	{
		if (_isAttacking)
			return false;

		return base.CanRotate();
	}

	public override bool CanMove()
	{
		if (_isAttacking)
			return false;

		return base.CanMove();
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
            case "2":
                break;
            case "3-1":
                break;
            case "3-2":
                break;
            case "4":
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

            gameObject.GiveDamage(cc, Random.Range(15, 30));
        }
    }
    #endregion
}
