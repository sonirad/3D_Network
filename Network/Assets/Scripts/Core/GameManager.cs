using System;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class GameManager : NetSingleton<GameManager>
{
    [Tooltip("로거(텍스트 출력 및 채팅용")]
    private Logger logger;
    [Tooltip("내 플레이어(접속 안했으면 null)")]
    private NetPlayer player;
    [Tooltip("현재 접속자 수")]
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);
    [Tooltip("현재 사용자 이름")]
    private string userName = DefaultName;
    [Tooltip("현재 사용자 색상")]
    private Color userColor = Color.clear;
    [Tooltip("플레이어의 이름과 색상을 컨트롤하기 위한 컴포넌트")]
    private NetPlayerDecorator deco;
    const string DefaultName = "플레이어";
    private CinemachineVirtualCamera virtualCamera;

    public CinemachineVirtualCamera VCam => virtualCamera;
    public NetPlayer Player => player;
    [Tooltip("현재 사용자 이름을 확인하고 설정하기 위한 프로퍼티")]
    public string UserName
    {
        get => userName;
        set
        {
            userName = value;

            // 변경되었음을 알림
            onUserNameChange?.Invoke(userName);
        }
    }
    [Tooltip("사용자의 색상을 변경하기 위한 프로퍼티")]
    public Color UserColor
    {
        get => userColor;
        set
        {
            userColor = value;

            // 변경되었음을 알림
            onUserColorChange?.Invoke(userColor);
        }
    }
    [Tooltip("플레이어의 이름과 색상을 컨트롤하는 컴포넌트 확이용 프로퍼티")]
    public NetPlayerDecorator PlayerDeco => deco;

    [Tooltip("동시접속자 수가 변경되었음을 알리는 델리게이트")]
    public Action<int> onPlayersInGameChange;
    [Tooltip("유저 이름이 변경되었음을 알리는 델리게이트")]
    public Action<string> onUserNameChange;
    [Tooltip("유저 색상이 변경되었음을 알리는 델리게이트")]
    public Action<Color> onUserColorChange;
    [Tooltip("유저(자기 자신) 연결이 해제되었음을 알리는 델리게이트")]
    public Action onPlayerDisconnected;

    protected override void OnInitialize()
    {
        logger = FindAnyObjectByType<Logger>();
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

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
            deco = netObj.GetComponent<NetPlayerDecorator>();

            if (UserName != DefaultName)
            {
                deco.SetName($"{UserName}_{id}");
                UserName = UserName;
            }
            else
            {
                deco.SetName($"{DefaultName}_{id}");
                UserName = DefaultName;
            }

            if (UserColor != Color.clear)
            {
                deco.SetColor(UserColor);
            }

            foreach (var other in NetworkManager.SpawnManager.SpawnedObjectsList)
            {
                NetPlayer otherPlayer = other.GetComponent<NetPlayer>();

                if (otherPlayer != null && otherPlayer != player)
                {
                    otherPlayer.gameObject.name = $"OtherPlayer_{other.OwnerClientId}";
                }

                NetPlayerDecorator netDeco = other.GetComponent<NetPlayerDecorator>();

                if (netDeco != null && netDeco != deco)
                {
                    netDeco.RefreshNamePlate();
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