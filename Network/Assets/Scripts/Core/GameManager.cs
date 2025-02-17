using Unity.Netcode;
using UnityEngine;

public class GameManager : NetSingleton<GameManager>
{
    [Tooltip("�ΰ�(�ؽ�Ʈ ��� �� ä�ÿ�")]
    private Logger logger;
    private NetPlayer player;

    public NetPlayer Player => player;

    protected override void OnInitialize()
    {
        logger = FindAnyObjectByType<Logger>();

        // � Ŭ���̾�Ʈ�� ���� / ���� ���� �� ����(�������� �׻� ����, Ŭ���̾�Ʈ�� �ڱ�͸� ����)
        NetworkManager.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;         // � Ŭ���̾�Ʈ�� �������� �� ������ ����
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