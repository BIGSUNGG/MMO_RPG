using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : CharacterController
{
    public PlayerController()
    {
        _characterType = CharacterType.Player;
    }

    protected override void Start()
    {
        base.Start();

        PlayerAnim = GetComponent<PlayerAnimParameter>();
        if (PlayerAnim == null)
            Debug.LogWarning("PlayerAnimParameter is null");

        _playerMovement = GetComponent<PlayerMovementComponent>();
        if (_playerMovement == null)
            Debug.LogWarning("PlayerMovementComponent is null");

        _playerMovement._onDodgeStartEvent.AddListener(() =>
        {
            if (IsLocallyControlled())
                Multicast_ComboEnd();
        });

        Inventory = GetComponent<InventoryComponent>();
        if (Inventory == null)
            Debug.LogWarning("InventoryComponent is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Player
    public ClientSession Session = null;
    protected NpcController _curCanInteractNcp = null;

    #endregion

    #region Controller
    public override void ControllerUpdate()
    {
        if (IsLocallyControlled() == false)
            return;

        base.ControllerUpdate();

        if (_playerMovement)
        {
            if (Input.GetKeyDown(KeyCode.Space) && CanDodgeInput())
                _playerMovement.DodgeRollStart();
        }

        if (_camera)
        {
            _camera.transform.position = new UnityEngine.Vector3(0.0f, 10.0f, -3.0f) + transform.position;
            _camera.transform.eulerAngles = new UnityEngine.Vector3(70, 0, 0);
        }

        // 마우스 방향으로 회전
        if (CanRotate())
        {
            LookMousePos();
        }

        // 근처 상호작용 가능한 Npc 찾기
        {
            _curCanInteractNcp = null;

            int targetLayer = LayerMask.NameToLayer("Npc");
            int layerMask = 1 << targetLayer;
            if (targetLayer == -1) // 레이어를 못 찾았을 경우
            {
                Debug.LogWarning("레이어 이름이 유효하지 않습니다: " + layerMask);
                return;
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position + new Vector3(0.0f, Capsule.height / 2, 0.0f), 3.0f, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                NpcController npc = hitCollider.gameObject.GetComponentInParent<NpcController>();
                if (npc == null)
                    continue;

                _curCanInteractNcp = npc;
            }

            if (_curCanInteractNcp == null)
            {
                CloseShopPopup();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if(_itemShopUI == null)
                    ShowShopPopup(_curCanInteractNcp);
                else
                    CloseShopPopup();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Inventory.UseItem(0);
    }

    protected virtual void LookMousePos()
    {
        if (IsLocallyControlled() == false)
        {
            Debug.LogError("This function must called on locally controller");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 dir = hit.point - transform.position;
            transform.eulerAngles = new Vector3(0.0f, Util.GetAngleY(dir), 0.0f);
        }
    }

    public override void OnPossess()
    {
        base.OnPossess();

        transform.position = new UnityEngine.Vector3(0.0f, 3.0f, 0.0f);

        _camera = GameObject.Find("Main Camera");

    }

    public virtual bool CanDodgeInput()
    {
        if (CanInput() == false)
            return false;
            
        return true;
    }

    public override bool CanAttack()
    {
        if (CanInput() == false)
            return false;

        if (_playerMovement._bIsdodging)
            return false;

        return base.CanAttack();
    }

    public override bool CanInput()
    {
        if (_itemShopUI != null)
            return false;

        return base.CanInput();
    }

    public override bool IsPlayerControlled()
    {
        return true;
    }
    #endregion

    #region Component
    public InventoryComponent Inventory
    {
        get
        {
            if (_inventory == null)
                _inventory = GetComponent<InventoryComponent>();

            return _inventory;
        }
        set { _inventory = value; }
    }
    private InventoryComponent _inventory;

    public PlayerMovementComponent PlayerMovement
    {
        get
        {
            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovementComponent>();

            return _playerMovement;
        }
        set { _playerMovement = value; }
    }
    private PlayerMovementComponent _playerMovement;

    public PlayerAnimParameter PlayerAnim
    {
        get
        {
            if (_playerAnim == null)
                _playerAnim = GetComponent<PlayerAnimParameter>();

            return _playerAnim;
        }
        set { _playerAnim = value; }
    }
    private PlayerAnimParameter _playerAnim;
    #endregion

    #region Object
    GameObject _camera = null;
    #endregion

    #region Attack
    protected override void Multicast_ComboAttack_Implementation(int combo)
    {
        base.Multicast_ComboAttack_Implementation(combo);

        if(IsLocallyControlled())
        {
            LookMousePos();
        }
    }
    #endregion

    #region Shop
    protected UI_ItemShop _itemShopUI = null;

    protected virtual void ShowShopPopup(NpcController dealer)
    {
        if (_itemShopUI != null)
            return;

        _itemShopUI = Managers.UI.ShowPopupUI<UI_ItemShop>();
        _itemShopUI.SetDealer(dealer);
    }

    protected virtual void CloseShopPopup()
    {
        if (_itemShopUI == null)
            return;

        Managers.UI.ClosePopupUI(_itemShopUI);
        _itemShopUI = null;
    }
    #endregion

    #region Sync
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class PlayerSyncInfo : CharacterSyncInfo
    {
    }

    public override void ObjectSync(ByteString syncInfo)
    {
        PlayerSyncInfo info = Util.BytesToObject<PlayerSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(PlayerSyncInfo info)
    {
        base.ObjectSync(info);
    }

    public override ByteString GetObjectSyncInfo()
    {
        PlayerSyncInfo info = new PlayerSyncInfo();
        GetObjectSyncInfo(info);
        return ByteString.CopyFrom(Util.ObjectToBytes<PlayerSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(PlayerSyncInfo info)
    {
        base.GetObjectSyncInfo(info);
    }
    #endregion
}
