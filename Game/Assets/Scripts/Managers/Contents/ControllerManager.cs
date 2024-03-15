using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public class ControllerManager
{
    PlayerController MyController = null;

    public void Update()
    {
        if (MyController == null)
            return;

        MyController.ControllerUpdate();
    }
}
