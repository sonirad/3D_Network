using UnityEngine.InputSystem;

public class TestLog : TestBase
{
    public Logger logger;
    private int count = 0;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        logger.Log($"Test - {count++}");
    }
}
