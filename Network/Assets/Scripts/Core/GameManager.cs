using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetSingleton<GameManager>
{
    [Tooltip("�ΰ�(�ؽ�Ʈ ��� �� ä�ÿ�")]
    private Logger logger;
    private NetPlayer player;
    [Tooltip("���� ������ ��")]
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);

    public NetPlayer Player => player;

    [Tooltip("���������� ���� ����Ǿ����� �˸��� ��������Ʈ")]
    public Action<int> onPlayersInGameChange;

    protected override void OnInitialize()
    {
        logger = FindAnyObjectByType<Logger>();

        // � Ŭ���̾�Ʈ�� ���� / ���� ���� �� ����(�������� �׻� ����, Ŭ���̾�Ʈ�� �ڱ�͸� ����)
        NetworkManager.OnClientConnectedCallback += OnClientConnect;
        // � Ŭ���̾�Ʈ�� �������� �� ������ ����
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        // ������ ���� �����ϱ�
        playersInGame.OnValueChanged += (_, newValue) => onPlayersInGameChange?.Invoke(newValue);
    }

    /// <summary>
    /// � Ŭ���̾�Ʈ�� �������� �� ó��
    /// </summary>
    /// <param name="id">������ Ŭ���̾�Ʈ�� id</param>
    private void OnClientConnect(ulong id)
    {
        NetworkObject netObj = NetworkManager.SpawnManager.GetPlayerNetworkObject(id);

        if (netObj.IsOwner)
        {
            player = netObj.GetComponent<NetPlayer>();
            player.gameObject.name = $"Player_{id}";

            foreach (var other in NetworkManager.SpawnManager.SpawnedObjectsList)
            {
                NetPlayer otherPlayer = other.GetComponent<NetPlayer>();

                if (otherPlayer != null && otherPlayer != player)
                {
                    otherPlayer.gameObject.name = $"OtherPlayer_{other.OwnerClientId}";
                }
            }
        }
        else
        {
            NetPlayer other = netObj.GetComponent<NetPlayer>();

            if (other != null && other != player)
            {
                netObj.gameObject.name = $"OtherPlayer_{id}";
            }
        }

        if (IsServer)
        {
            // ���������� ����
            playersInGame.Value++;
        }
    }

    /// <summary>
    /// � Ŭ���̾�Ʈ�� ���� �������� �� ó��
    /// </summary>
    /// <param name="id">���� ������ Ŭ���̾�Ʈ�� id</param>
    private void OnClientDisconnect(ulong id)
    {
        NetworkObject netObj = NetworkManager.SpawnManager.GetPlayerNetworkObject(id);

        if (netObj.IsOwner)
        {
            player = null;
        }

        if (IsServer)
        {
            // ���������� ����
            playersInGame.Value--;
        }
    }

    /// <summary>
    /// �ΰſ� ���ڿ��� �߰�
    /// </summary>
    /// <param name="message">�߰��� ���ڿ�</param>
    public void Log(string message)
    {
        logger.Log(message);
    }
}