using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.InputSystem.iOS;
using System.Net.NetworkInformation;

public class NetPlayerDecorator : NetworkBehaviour
{
    #region 몸 색

    NetworkVariable<Color> bodyColor = new NetworkVariable<Color>();
    private Renderer playerRenderer;
    private Material bodyMaterial;

    readonly int BaseColor_Hash = Shader.PropertyToID("_BaseColor");

    #endregion

    #region 이름

    private NetworkVariable<FixedString32Bytes> userName = new NetworkVariable<FixedString32Bytes>();
    private NamePlate namePlate;

    #endregion

    #region 이펙트

    private NetworkVariable<bool> netEffectState = new NetworkVariable<bool>(false);

    public bool IsEffectOn
    {
        get => netEffectState.Value;
        set
        {
            if (netEffectState.Value != value)
            {
                if (IsServer)
                {
                    netEffectState.Value = value;
                }
                else
                {
                    UpdateEffectStateServerRpc(value);
                }
            }
        }
    }

    readonly int EmissionIntensity_Hash = Shader.PropertyToID("_EmissionIntensity");

    #endregion

    private void Awake()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        bodyMaterial = playerRenderer.material;
        bodyColor.OnValueChanged += OnBodyColorChange;
        namePlate = GetComponentInChildren<NamePlate>();

        userName.OnValueChanged += onNameSet;
        netEffectState.OnValueChanged += OnEffectStateChange;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            bodyColor.Value = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
        }

        bodyMaterial.SetColor(BaseColor_Hash, bodyColor.Value);
    }

    #region 이름 설정용

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

    #region 색상 설정용

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

    #region 이펙트용

    private void OnEffectStateChange(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            bodyMaterial.SetFloat(EmissionIntensity_Hash, 1.0f);
        }
        else
        {
            bodyMaterial.SetFloat(EmissionIntensity_Hash, 0.0f);
        }
    }

    [ServerRpc]
    private void UpdateEffectStateServerRpc(bool isOn)
    {
        netEffectState.Value = isOn;
    }

    #endregion
}