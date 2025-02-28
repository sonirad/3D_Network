using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Collections;

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
        Idle,     // 대기
        Walk,     // 걷기
        BackWalk,     // 뒤로 걷기
        None     // 초기값
    }

    [Tooltip("현재 애니메이션 상태")]
    private AnimationState state = AnimationState.None;
    [Tooltip("애니메이션 상태 처리용 네트워크 변수")]
    private NetworkVariable<AnimationState> netAnimState = new NetworkVariable<AnimationState>();
    [Tooltip("채팅용 네트워크 변수")]
    private NetworkVariable<FixedString512Bytes> chatString = new NetworkVariable<FixedString512Bytes>();

    #region 유니티 이벤트 함수
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputActions = new Player_Input_Actions();

        netAnimState.OnValueChanged += OnAnimStateChange;
        chatString.OnValueChanged += OnChatRecieve;
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {

        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameManager.Instance.onPlayerDisconnected?.Invoke();
        }
    }

    #endregion

    #region 입력 처리용 함수들
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
    #endregion

    #region 기타

    /// <summary>
    /// 이동 입력 처리용
    /// </summary>
    /// <param name="moveInput">이동 입력된 정도</param>
    private void SetMoveInput(float moveInput)
    {
        // 오너 일 때만 이동 처리
        if (IsOwner)
        {
            // 이동 정도 결정
            float moveDir = moveInput * moveSpeed;

            if (IsServer)
            {
                // 서버면 직접 수정
                netMoveDir.Value = moveDir;
            }
            else
            {
                // 서버가 아니면 서버에게 수정 요청하는 Rpc 실행
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
                state = AnimationState.Idle;
            }

            // 애니메이션 상태가 변경되면
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
    }

    /// <summary>
    /// 회전 입력을 처리
    /// </summary>
    /// <param name="rotateInput">회전 입력 정도</param>
    private void SetRotateInput(float rotateInput)
    {
        // 오너 일 때만 처리
        if (IsOwner)
        {
            // 회전량 결정
            float rotate = rotateInput * rotateSpeed;

            if (IsServer)
            {
                // 서버면 직접 주성
                netRotate.Value = rotate;
            }
            else
            {
                // 서버가 아니면 Rpc 요청
                RotateRequestServerRpc(rotate);
            }
        }
    }

    /// <summary>
    /// 애니메이션 상태가 변경되면 실행되는 함수
    /// </summary>
    /// <param name="previousValue">이전 값</param>
    /// <param name="newValue">새 값</param>
    private void OnAnimStateChange(AnimationState previousValue, AnimationState newValue)
    {
        // 새 값으로 변경
        animator.SetTrigger(newValue.ToString());
    }
    #endregion

    #region 채팅

    /// <summary>
    /// 채팅을 보내는 함수
    /// </summary>
    /// <param name="message"></param>
    public void SendChat(string message)
    {
        // chatString 변경
        if (IsServer)
        {
            chatString.Value = message;
        }
        else
        {
            RequestChatServerRpc(message);
        }
    }

    /// <summary>
    /// 채팅을 받았을 때 처리(chatString 이 변경되었다 = 채팅을 받았다.)
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    private void OnChatRecieve(FixedString512Bytes previousValue, FixedString512Bytes newValue)
    {
        // 받은 채팅 내용을 logger에 찍기
        GameManager.Instance.Log(newValue.ToString());
    }
    #endregion

    #region 서버 Rpc들
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

    [ServerRpc]
    private void RequestChatServerRpc(FixedString512Bytes message)
    {
        chatString.Value = message;
    }
    #endregion
}