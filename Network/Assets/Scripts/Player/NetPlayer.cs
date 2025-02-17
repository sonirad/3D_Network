using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class NetPlayer : NetworkBehaviour
{
    [Tooltip("이동 속도")]
    public float moveSpeed = 3.5f;
    [Tooltip("회전 속도")]
    public float rotateSpeed = 90.0f;
    [Tooltip("마지막 입력으로 인한 이동 방향(전진, 정지, 후진) 네트워크에서 공유되는 변수")]
    private NetworkVariable<float> netMoveDir = new NetworkVariable<float>(0.0f);
    [Tooltip("마지막 입력으로 인한 회전 방향(좌회전, 정지, 우회전")]
    private NetworkVariable<float> netRotate = new NetworkVariable<float>(0.0f);

    // 컴포넌트
    private CharacterController controller;
    private Animator animator;
    private Player_Input_Actions inputActions;

    [Tooltip("애니메이션 상태")]
    private enum AnimationState
    {
        Idel,     // 대기
        Walk,     // 걷기
        BackWalk,     // 뒤로 걷기
        None     // 초기값
    }

    [Tooltip("현재 애니메이션 상태")]
    private AnimationState state = AnimationState.None;
    [Tooltip("애니메이션 상태 처리용 네트워크 변수")]
    private NetworkVariable<AnimationState> netAnimState = new NetworkVariable<AnimationState>();

    // 유니티 이벤트 함수들
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputActions = new Player_Input_Actions();
        netAnimState.OnValueChanged += OnAnimStateChange;
    }

    private void Update()
    {
        if (netMoveDir.Value != 0.0f)
        {
            controller.SimpleMove(netMoveDir.Value * transform.forward);
        }

        transform.Rotate(0, netRotate.Value * Time.deltaTime, 0, Space.World);
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

    // 입력 처리용 함수들
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // 키보드라 -1, 0, 1 중 하나
        float moveInput = context.ReadValue<float>();

        SetMoveInput(moveInput);
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // 키보드라 -1, 0, 1 중 하나
        float rotateInput = context.ReadValue<float>();

        SetRotateInput(rotateInput);
    }

    // 기타 ----------------------------------------------------------------------------------------------------------
    private void SetMoveInput(float moveInput)
    {
        float moveDir = moveInput * moveSpeed;

        if (NetworkManager.Singleton.IsServer)
        {
            netMoveDir.Value = moveDir;
        }
        else if (IsOwner)
        {
            MoveRequestServerRpc(moveDir);
        }

        // 애니메이션 변경
        if (moveDir > 0.001f)
        {
            state = AnimationState.Walk;
        }
        else if (moveDir < -0.001f)
        {
            state = AnimationState.BackWalk;
        }
        else
        {
            state = AnimationState.Idel;
        }

        if (state != netAnimState.Value)
        {
            if (IsServer)
            {
                netAnimState.Value = state;
            }
            else if (IsOwner)
            {
                UpdateAnimStateServerRpc(state);
            }
        }
    }

    private void SetRotateInput(float rotateInput)
    {
        float rotate = rotateInput * rotateSpeed;
        
        if (NetworkManager.Singleton.IsServer)
        {
            netRotate.Value = rotate;
        }
        else if (IsOwner)
        {
            RotateRequestServerRpc(rotate);
        }
    }

    private void OnAnimStateChange(AnimationState previousValue, AnimationState newValue)
    {
        animator.SetTrigger(newValue.ToString());
    }

    // 서버 Rpc들 --------------------------------------------------------------------------------------------------------------
    [ServerRpc]
    private void MoveRequestServerRpc(float move)
    {
        netMoveDir.Value = move;
    }

    [ServerRpc]
    private void RotateRequestServerRpc(float rotate)
    {
        netRotate.Value = rotate;
    }

    [ServerRpc]
    private void UpdateAnimStateServerRpc(AnimationState state)
    {
        netAnimState.Value = state;
    }
}
