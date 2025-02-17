using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestChat : TestBase
{
    public Logger logger;

    private void Start()
    {
        logger = FindAnyObjectByType<Logger>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        inputActions.Test.Enter.performed += OnEnter;
    }

    protected override void OnDisable()
    {
        inputActions.Test.Enter.performed -= OnEnter;

        base.OnDisable();
    }

    private void OnEnter(InputAction.CallbackContext _)
    {
        logger.InputFieldFocusOn();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Color color = Color.red;
        string colorText = ColorUtility.ToHtmlStringRGB(color);

        Debug.Log(colorText);
        logger.Log($"<#{colorText}>색이 있다. </color> 여기부터는 검정색");
        logger.Log($"<#FF0000>색이 있다. </color> 여기부터는 검정색");
    }
}
