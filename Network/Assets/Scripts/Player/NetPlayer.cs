using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Collections;

public class NetPlayer : NetworkBehaviour
{
    [Tooltip("�̵� �ӵ�")]
    public float moveSpeed = 3.5f;
    [Tooltip("ȸ�� �ӵ�")]
    public float rotateSpeed = 90.0f;
    [Tooltip("������ �Է����� ���� �̵� ����(����, ����, ����) ��Ʈ��ũ���� �����Ǵ� ����")]
    private NetworkVariable<float> netMoveDir = new NetworkVariable<float>(0.0f);
    [Tooltip("������ �Է����� ���� ȸ�� ����(��ȸ��, ����, ��ȸ��")]
    private NetworkVariable<float> netRotate = new NetworkVariable<float>(0.0f);

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
    [Tooltip("�ִϸ��̼� ���� ó���� ��Ʈ��ũ ����")]
    private NetworkVariable<AnimationState> netAnimState = new NetworkVariable<AnimationState>();
    [Tooltip("ä�ÿ� ��Ʈ��ũ ����")]
    private NetworkVariable<FixedString512Bytes> chatString = new NetworkVariable<FixedString512Bytes>();

    #region ����Ƽ �̺�Ʈ �Լ�
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

    #region �Է� ó���� �Լ���
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Ű����� -1, 0, 1 �� �ϳ�
        float moveInput = context.ReadValue<float>();

        SetMoveInput(moveInput);
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // Ű����� -1, 0, 1 �� �ϳ�
        float rotateInput = context.ReadValue<float>();

        SetRotateInput(rotateInput);
    }
    #endregion

    #region ��Ÿ

    /// <summary>
    /// �̵� �Է� ó����
    /// </summary>
    /// <param name="moveInput">�̵� �Էµ� ����</param>
    private void SetMoveInput(float moveInput)
    {
        // ���� �� ���� �̵� ó��
        if (IsOwner)
        {
            // �̵� ���� ����
            float moveDir = moveInput * moveSpeed;

            if (IsServer)
            {
                // ������ ���� ����
                netMoveDir.Value = moveDir;
            }
            else
            {
                // ������ �ƴϸ� �������� ���� ��û�ϴ� Rpc ����
                MoveRequestServerRpc(moveDir);
            }

            // �ִϸ��̼� ����
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

            // �ִϸ��̼� ���°� ����Ǹ�
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
    /// ȸ�� �Է��� ó��
    /// </summary>
    /// <param name="rotateInput">ȸ�� �Է� ����</param>
    private void SetRotateInput(float rotateInput)
    {
        // ���� �� ���� ó��
        if (IsOwner)
        {
            // ȸ���� ����
            float rotate = rotateInput * rotateSpeed;

            if (IsServer)
            {
                // ������ ���� �ּ�
                netRotate.Value = rotate;
            }
            else
            {
                // ������ �ƴϸ� Rpc ��û
                RotateRequestServerRpc(rotate);
            }
        }
    }

    /// <summary>
    /// �ִϸ��̼� ���°� ����Ǹ� ����Ǵ� �Լ�
    /// </summary>
    /// <param name="previousValue">���� ��</param>
    /// <param name="newValue">�� ��</param>
    private void OnAnimStateChange(AnimationState previousValue, AnimationState newValue)
    {
        // �� ������ ����
        animator.SetTrigger(newValue.ToString());
    }
    #endregion

    #region ä��

    /// <summary>
    /// ä���� ������ �Լ�
    /// </summary>
    /// <param name="message"></param>
    public void SendChat(string message)
    {
        // chatString ����
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
    /// ä���� �޾��� �� ó��(chatString �� ����Ǿ��� = ä���� �޾Ҵ�.)
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    private void OnChatRecieve(FixedString512Bytes previousValue, FixedString512Bytes newValue)
    {
        // ���� ä�� ������ logger�� ���
        GameManager.Instance.Log(newValue.ToString());
    }
    #endregion

    #region ���� Rpc��
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