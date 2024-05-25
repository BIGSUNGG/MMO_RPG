using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : ObjectComponent
{
    protected enum RpcFunctionId
    {

    }

    protected override void Start()
    {
        base.Start();

        _curHp = _maxHp;
    }

    protected override void Update()
    {
        base.Update();
    }

    #region Health
    public bool _bDead = false; // 오브젝트가 죽어있는지
    public int _curHp { get; protected set; } // 현재 체력
    public int _maxHp { get; protected set; } = 100; // 최대 체력

    UnityEvent _onTakeDamageEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트
    UnityEvent _onDeathEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트

    // 서버에서 오브젝트가 대미지를 줄 때 호출
    // damage : 줄 대미지
    // victim : 대미지를 받을 오브젝트
    // return : 최종적으로 준 대미지
    public float OnServer_GiveDamage(GameObject victim, float damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return 0.0f;

        HealthComponent health = victim.GetComponentInChildren<HealthComponent>();
        if (health == null)
            return 0.0f;

        return health.OnServer_TakeDamage(gameObject, damage);
    }

    // 서버에서 오브젝트가 대미지를 받을 때 호출
    // damage : 받은 대미지
    // damageCauser : 대미지를 주는 오브젝트
    // return : 최종적으로 받은 대미지
    public float OnServer_TakeDamage(GameObject attacker, float damage) 
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return 0.0f;

        if (_bDead) // 이미 죽어있다면
            return 0.0f;

        _curHp -= (int)damage;
        _onTakeDamageEvent.Invoke();

        if(_curHp <= 0) // 체력이 0이하로 떨어졌다면
        {
            _curHp = 0;
            OnServer_Death(damage, attacker);
        }

        Debug.Log($"{_curHp}");

        return damage;
    }

    // 서버에서 오브젝트의 체력이 0이하로 떨어졌을 때 호출
    // damage : 받은 대미지, damageCauser : 대미지를 주는 오브젝트
    public void OnServer_Death(float damage, GameObject attacker)
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return;

        _bDead = true;
        _onDeathEvent.Invoke();
        Debug.Log("Dead");
    }

    #endregion

}
