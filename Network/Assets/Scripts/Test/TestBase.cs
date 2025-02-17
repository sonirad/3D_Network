using UnityEngine;
using UnityEngine.InputSystem;

public class TestBase : MonoBehaviour
{
    public int seed = -1;
    const int allRandom = -1;
    protected Test_Input_Actions inputActions;

    private void Awake()
    {
        inputActions = new Test_Input_Actions();

        // -1 일때는 완전 랜덤. 그 외에 값은 시드로 설정
        if (seed != allRandom)
        {
            UnityEngine.Random.InitState(seed);
        }
    }

    protected virtual void OnEnable()
    {
        inputActions.Test.Enable();

        inputActions.Test.Test1.performed += OnTest1;
        inputActions.Test.Test1.performed += OnTest2;
        inputActions.Test.Test1.performed += OnTest3;
        inputActions.Test.Test1.performed += OnTest4;
        inputActions.Test.Test1.performed += OnTest5;
        inputActions.Test.Test1.performed += OnTestRClick;
        inputActions.Test.Test1.performed += OnTestLClick;
    }

    protected virtual void OnDisable()
    {
        inputActions.Test.Test1.performed -= OnTest1;
        inputActions.Test.Test1.performed -= OnTest2;
        inputActions.Test.Test1.performed -= OnTest3;
        inputActions.Test.Test1.performed -= OnTest4;
        inputActions.Test.Test1.performed -= OnTest5;
        inputActions.Test.Test1.performed -= OnTestRClick;
        inputActions.Test.Test1.performed -= OnTestLClick;

        inputActions.Test.Disable();
    }

    protected virtual void OnTestRClick(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTestLClick(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTest5(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTest4(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTest3(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTest2(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnTest1(InputAction.CallbackContext context)
    {

    }
}
