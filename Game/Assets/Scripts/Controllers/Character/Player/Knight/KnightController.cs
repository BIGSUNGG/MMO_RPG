using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KnightController : PlayerController
{
    protected KnightController _knightAnim = null;

    protected override void Start()
    {
        base.Start();

        _knightAnim = GetComponent<KnightController>();
        if (_knightAnim == null)
            Debug.Log("KnightController is null");

        _playerMovement._onDodgeStartEvent.AddListener(ComboEnd);
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Attack
    public bool _isAttacking { get; protected set; } = false;
    public bool _bDoNextCombo { get; protected set; } = false;
    int _curCombo = 0;
    const int _maxCombo = 3;
    public UnityEvent _onComboStartEvent = new UnityEvent();
    public UnityEvent _onComboEndEvent = new UnityEvent();

    public virtual void Attack()
    {
        if (CanAttack() == false)
            return;

        if (_isAttacking == false)
        {
            ComboStart();
        }
        else
        {
            _bDoNextCombo = true;
        }

    }

    // 콤보 공격 시작 시 호출
    protected virtual void ComboStart() 
    {
        Debug.Log("ComboStart");

        _curCombo = 0;
        _isAttacking = true;
        _bDoNextCombo = false;

        _onComboStartEvent.Invoke();
    }

    // 콤보 공격 종료 시 호출
    public virtual void ComboEnd() 
    {
        Debug.Log("ComboEnd");

        _curCombo = 0;
        _isAttacking = false;
        _onComboEndEvent.Invoke();

    }

    public virtual void OnComboAttackSwing(string attackName)
    {
        Debug.Log($"{attackName}");

    }

    public virtual void OnStartComboAttack()
    {
        Debug.Log("OnStartComboAttack");
        _bDoNextCombo = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 dir = hit.point - transform.position;
            transform.eulerAngles = new Vector3(0.0f, Util.GetAngleY(dir), 0.0f);
        }
    }

    #endregion

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        if (IsLocallyControlled() == false)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    public override bool CanDodgeInput()
    {
        //if (_isAttacking)
        //    return false;

        return true;
    }

    public override bool CanRotationInput()
    {
        if (_isAttacking)
            return false;

        return base.CanRotationInput();
    }

    public override bool CanMovementInput()
    {
        if (_isAttacking)
            return false;

        return base.CanMovementInput();
    }
    #endregion

}
