using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetSingleton<GameManager>
{
    [Tooltip("로거(텍스트 출력 및 채팅용")]
    private Logger logger;
    private NetPlayer player;
    [Tooltip("현재 접속자 수")]
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);

    public NetPlayer Player => player;

    [Tooltip("동시접속자 수가 변경되었음을 알리는 델리게이트")]
    public Action<int> onPlayersInGameChange;

    protected override void OnInitialize()
    {
        logger = FindAnyObjectByType<Logger>();

        // 어떤 클라이언트가 접속 / 해제 했을 때 실행(서버에는 항상 실행, 클라이언트는 자기것만 실행)
        NetworkManager.OnClientConnectedCallback += OnClientConnect;
        // 어떤 클라이언트가 접속해제 할 때마다 실행
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        // 동접자 숫자 변경하기
        playersInGame.OnValueChanged += (_, newValue) => onPlayersInGameChange?.Invoke(newValue);
    }

    /// <summary>
    /// 어떤 클라이언트가 접속했을 때 처리
    /// </summary>
    /// <param name="id">접속한 클라이언트의 id</param>
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
            // 서버에서만 증가
            playersInGame.Value++;
        }
    }

    /// <summary>
    /// 어떤 클라이언트가 접속 해제했을 때 처리
    /// </summary>
    /// <param name="id">접속 해제한 클라이언트의 id</param>
    private void OnClientDisconnect(ulong id)
    {
        NetworkObject netObj = NetworkManager.SpawnManager.GetPlayerNetworkObject(id);

        if (netObj.IsOwner)
        {
            player = null;
        }

        if (IsServer)
        {
            // 서버에서만 감소
            playersInGame.Value--;
        }
    }

    /// <summary>
    /// 로거에 문자열을 추가
    /// </summary>
    /// <param name="message">추가할 문자열</param>
    public void Log(string message)
    {
        logger.Log(message);
    }
}