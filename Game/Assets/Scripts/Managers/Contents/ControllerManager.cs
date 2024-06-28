using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using UnityEngine.Events;

public class ControllerManager
{
    public PlayerController MyController { get; private set; } = null;

    public void Update()
    {
        if (MyController == null)
            return;

        MyController.ControllerUpdate();
    }

    public void Clear()
    {
        _onPossess = new UnityEvent();
        _onUnpossess = new UnityEvent();

        Unpossess();
    }

    public UnityEvent _onPossess = new UnityEvent();
    public UnityEvent _onUnpossess = new UnityEvent();
    public void Possess(GameObject obj)
    {
        if (obj == null)
            return;

        PlayerController controller = obj.GetComponent<PlayerController>();
        if (controller == null)
            return;

        Debug.Log("Possess GameObject");
        MyController = controller;
        MyController.OnPossess();

        _onPossess.Invoke();
    }

    public void Unpossess()
    {
        if (MyController == null)
            return;

        Debug.Log("Unpossess GameObject");
        MyController.OnUnpossess();
        MyController = null;

        _onUnpossess.Invoke();
    }
}
