using UnityEngine;
using Unity.Netcode;

public class NetPlayerDecorator : NetworkBehaviour
{
    NetworkVariable<Color> bodyColor = new NetworkVariable<Color>();
    private Renderer playerRenderer;
    private Material bodyMaterial;

    readonly int BaseColor_Hash = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        bodyMaterial = playerRenderer.material;
        bodyColor.OnValueChanged += OnBodyColorChange;
    }

    private void OnBodyColorChange(Color previousValue, Color newValue)
    {
        bodyMaterial.SetColor(BaseColor_Hash, newValue);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            bodyColor.Value = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
        }

        bodyMaterial.SetColor(BaseColor_Hash, bodyColor.Value);
    }
}