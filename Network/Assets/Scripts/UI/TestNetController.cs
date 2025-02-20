using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class TestNetController : MonoBehaviour
{
    private TextMeshProUGUI playerInGame;

    private void Start()
    {
        Transform child = transform.GetChild(0);
        Button startHost = child.GetComponent<Button>();

        startHost.onClick.AddListener(() =>
        {
            // ȣ��Ʈ�� ���� �õ�
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("ȣ��Ʈ�� ���� ����");
            }
            else
            {
                Debug.Log("ȣ��Ʈ�� ���� ����");
            }
        });

        child = transform.GetChild(1);
        Button startClient = child.GetComponent<Button>();

        startClient.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Ŭ���̾�Ʈ�� ���� ����");
            }
            else
            {
                Debug.Log("Ŭ���̾�Ʈ�� ���� ����");
            }
        });

        child = transform.GetChild(2);
        Button disconnect = child.GetComponent<Button>();

        disconnect.onClick.AddListener(() =>
        {
            // �� ���� ����
            NetworkManager.Singleton.Shutdown();
        });

        // ���������� ��
        child = transform.GetChild(3);
        child = child.GetChild(1);
        playerInGame = child.GetComponent<TextMeshProUGUI>();
    }
}
