using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class TestNetController : MonoBehaviour
{
    private TextMeshProUGUI playerInGame;
    private TextMeshProUGUI userName;

    private void Start()
    {
        Transform child = transform.GetChild(0);
        Button startHost = child.GetComponent<Button>();

        startHost.onClick.AddListener(() =>
        {
            // 호스트로 시작 시도
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("호스트로 시작 성공");
            }
            else
            {
                Debug.Log("호스트로 시작 실패");
            }
        });

        child = transform.GetChild(1);
        Button startClient = child.GetComponent<Button>();

        startClient.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("클라이언트로 연결 성공");
            }
            else
            {
                Debug.Log("클라이언트로 연결 실패");
            }
        });

        child = transform.GetChild(2);
        Button disconnect = child.GetComponent<Button>();

        disconnect.onClick.AddListener(() =>
        {
            // 내 연결 끊기
            NetworkManager.Singleton.Shutdown();
        });

        // 동시접속자 수
        child = transform.GetChild(3);
        child = child.GetChild(1);
        playerInGame = child.GetComponent<TextMeshProUGUI>();
        GameManager gameManager = GameManager.Instance;

        // 동시 접속자 숫자 변경 델리게이트가 실행되면 UI 갱신
        gameManager.onPlayersInGameChange += (count) => playerInGame.text = count.ToString();

        child = transform.GetChild(4);
        child = child.GetChild(1);
        userName = child.GetComponent<TextMeshProUGUI>();

        gameManager.onUserNameChange += (name) =>
        {
            userName.text = gameManager.UserName;
        };
    }
}
