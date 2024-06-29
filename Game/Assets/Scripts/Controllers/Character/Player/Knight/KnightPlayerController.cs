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
    public override void OnServer_ComboAttackSwing(string attackName) // ���⸦ �ֵθ��� Ÿ�ֿ̹� ȣ��
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

            gameObject.GiveDamage(cc, Random.Range(15, 30));
        }
    }
    #endregion
}
