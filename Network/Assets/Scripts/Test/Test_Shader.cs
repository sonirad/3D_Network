using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_Shader : TestBase
{
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        // �� ĳ������ �󱼺κ��� �� ������ �����
        GameManager.Instance.PlayerDeco.IsEffectOn = false;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        // �� ĳ������ �� �κ��� ������ �����
        GameManager.Instance.PlayerDeco.IsEffectOn = true;
    }
}
