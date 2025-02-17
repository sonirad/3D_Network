using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Player : MonoBehaviour
{
    [Tooltip("이동 속도")]
    public float moveSpeed = 3.5f;
    [Tooltip("회전 속도")]
    public float rotateSpeed = 90.0f;
    [Tooltip("마지막 입력으로 인한 이동 방향(전진, 정지, 후진")]
    private float moveDir = 0.0f;
    [Tooltip("마지막 입력으로 인한 회전 방향(좌회전, 정지, 우회전")]
    private float rotate = 0.0f;

    // 컴포넌트
    private CharacterController controller;
    private Animator animator;
    private Player_Input_Actions inputActions;

    [Tooltip("애니메이션 상태")]
    private enum AnimationState
    {
        Idle,     // 대기
        Walk,     // 걷기
        BackWalk,     // 뒤로 걷기
        None     // 초기값
    }

    [Tooltip("현재 애니메이션 상태")]
    private AnimationState state = AnimationState.None;

    [Tooltip("애니메이션 상태 설정 및 확인용 프로퍼티")]
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
        // 키보드라 -1, 0, 1 중 하나
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
        // 키보드라 -1, 0, 1 중 하나
        float rotateInput = context.ReadValue<float>();
        rotate = rotateInput * rotateSpeed;
    }
}
