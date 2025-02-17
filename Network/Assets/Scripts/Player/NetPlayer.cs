using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

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
        Idel,     // ���
        Walk,     // �ȱ�
        BackWalk,     // �ڷ� �ȱ�
        None     // �ʱⰪ
    }

    [Tooltip("���� �ִϸ��̼� ����")]
    private AnimationState state = AnimationState.None;
    [Tooltip("�ִϸ��̼� ���� ó���� ��Ʈ��ũ ����")]
    private NetworkVariable<AnimationState> netAnimState = new NetworkVariable<AnimationState>();

    // ����Ƽ �̺�Ʈ �Լ���
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

    // �Է� ó���� �Լ���
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

    // ��Ÿ ----------------------------------------------------------------------------------------------------------
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

    // ���� Rpc�� --------------------------------------------------------------------------------------------------------------
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
