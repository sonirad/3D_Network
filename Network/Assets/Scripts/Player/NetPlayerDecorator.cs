using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.InputSystem.iOS;

public class NetPlayerDecorator : NetworkBehaviour
{
    #region �� ��

    NetworkVariable<Color> bodyColor = new NetworkVariable<Color>();
    private Renderer playerRenderer;
    private Material bodyMaterial;

    readonly int BaseColor_Hash = Shader.PropertyToID("_BaseColor");

    #endregion

    #region �̸�

    private NetworkVariable<FixedString32Bytes> userName = new NetworkVariable<FixedString32Bytes>();
    private NamePlate namePlate;

    #endregion

    private void Awake()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        bodyMaterial = playerRenderer.material;
        bodyColor.OnValueChanged += OnBodyColorChange;
        namePlate = GetComponentInChildren<NamePlate>();
        userName.OnValueChanged += onNameSet;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            bodyColor.Value = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
        }

        bodyMaterial.SetColor(BaseColor_Hash, bodyColor.Value);
    }

    #region �̸� ������

    public void SetName(string name)
    {
        if (IsOwner)
        {
            if (IsServer)
            {
                userName.Value = name;
            }
            else
            {
                RequestUserNameChangeServerRpc(name);
            }
        }
    }

    [ServerRpc]
    private void RequestUserNameChangeServerRpc(string name)
    {
        userName.Value = name;
    }

    private void onNameSet(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        namePlate.SetName(newValue.ToString());
    }

    public void RefreshNamePlate()
    {
        namePlate.SetName(userName.Value.ToString());
    }

    #endregion

    #region ���� ������

    public void SetColor(Color color)
    {
        if (IsOwner)
        {
            if (IsServer)
            {
                bodyColor.Value = color;
            }
            else
            {
                RequestBodyColorChangeServerRpc(color);
            }
        }
    }

    [ServerRpc]
    private void RequestBodyColorChangeServerRpc(Color color)
    {
        bodyColor.Value = color;
    }

    private void OnBodyColorChange(Color previousValue, Color newValue)
    {
        bodyMaterial.SetColor(BaseColor_Hash, newValue);
    }

    #endregion
}