using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Player : MonoBehaviour
{
    [Tooltip("�̵� �ӵ�")]
    public float moveSpeed = 3.5f;
    [Tooltip("ȸ�� �ӵ�")]
    public float rotateSpeed = 90.0f;
    [Tooltip("������ �Է����� ���� �̵� ����(����, ����, ����")]
    private float moveDir = 0.0f;
    [Tooltip("������ �Է����� ���� ȸ�� ����(��ȸ��, ����, ��ȸ��")]
    private float rotate = 0.0f;

    // ������Ʈ
    private CharacterController controller;
    private Animator animator;
    private Player_Input_Actions inputActions;

    [Tooltip("�ִϸ��̼� ����")]
    private enum AnimationState
    {
        Idle,     // ���
        Walk,     // �ȱ�
        BackWalk,     // �ڷ� �ȱ�
        None     // �ʱⰪ
    }

    [Tooltip("���� �ִϸ��̼� ����")]
    private AnimationState state = AnimationState.None;

    [Tooltip("�ִϸ��̼� ���� ���� �� Ȯ�ο� ������Ƽ")]
    private AnimationState State
    {
        get => state;
        set
        {
            if (value != state)
            {
                state = value;

                animator.SetTrigger(state.ToString());
            }
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputActions = new Player_Input_Actions();
    }

    private void Update()
    {
        controller.SimpleMove(moveDir * transform.forward);
        transform.Rotate(0, rotate * Time.deltaTime, 0, Space.World);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move_Forward.performed += OnMoveInput;
        inputActions.Player.Move_Forward.canceled += OnMoveInput;
        inputActions.Player.Rotate.performed += OnRotate;
        inputActions.Player.Rotate.canceled += OnRotate;
    }

    private void OnDisable()
    {
        inputActions.Player.Move_Forward.performed -= OnMoveInput;
        inputActions.Player.Move_Forward.canceled -= OnMoveInput;
        inputActions.Player.Rotate.performed -= OnRotate;
        inputActions.Player.Rotate.canceled -= OnRotate;

        inputActions.Player.Disable();
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Ű����� -1, 0, 1 �� �ϳ�
        float moveInput = context.ReadValue<float>();
        moveDir = moveInput * moveSpeed;

        if (moveDir > 0.001f)
        {
            State = AnimationState.Walk;
        }
        else if (moveDir < -0.00f)
        {
            State = AnimationState.BackWalk;
        }
        else
        {
            State = AnimationState.Idle;
        }
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // Ű����� -1, 0, 1 �� �ϳ�
        float rotateInput = context.ReadValue<float>();
        rotate = rotateInput * rotateSpeed;
    }
}
