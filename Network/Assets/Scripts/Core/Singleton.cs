using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    [Tooltip("�� �̱����� �ʱ�ȭ �ƴ��� Ȯ��")]
    private bool isInitialized = false;
    [Tooltip("����ó���� ������ Ȯ��")]
    private static bool isShutdown = false;
    [Tooltip("�� �̱����� ��ü(�ν��Ͻ�)")]
    private static T instance = null;

    [Tooltip("�� �̱����� ��ü�� �б� ���� ������Ƽ")]
    public static T Instance
    {
        get
        {
            // ����ó���� ������
            if (isShutdown)
            {
                // ��� ����ϰ�
                Debug.Log("�̱����� �̹� ���� �� �̴�");

                // ����
                return null;
            }

            // ��ü�� ������
            if (instance == null)
            {
                // �ٸ� ���� ������Ʈ�� �ش� �̱����� �ִ��� Ȯ��
                T singleton = FindAnyObjectByType<T>();

                // �ٸ� ���� ������Ʈ���� �� �̱����� ������
                if (singleton == null)
                {
                    // �� ���� ������Ʈ �����
                    GameObject obj = new GameObject();

                    // �̸� ������ ����
                    obj.name = "Singleton";
                    // �̱��� ������Ʈ ���� �߰�
                    singleton = obj.AddComponent<T>();
                }

                // �ٸ� ���� ������Ʈ�� �ִ� �̱����̳� ���� ���� �̱��椷�� ����
                instance = singleton;

                // ���� ����� �� ���� ������Ʈ�� ���� ���� �ʵ��� ����
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    private void Awake()
    {
        // ���� �̹� ��ġ�� �ٸ� �̱����� ����
        if (instance == null)
        {
            // ù��°�� ����
            instance = this as T;

            // ���� ����� �� ���� ������Ʈ�� �������� �ʵ��� ����
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            // �̹� ���� �̱����� �ִ�
            if (instance != this)    // �װ� �� �ڽ��� �ƴϸ�
            {
                // �� �ڽ��� ����
                Destroy(this.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        // SceneManager.sceneLoaded�� ���� �ε�Ǹ� ����Ǵ� ��������Ʈ
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// ���� �ε�Ǿ��� �� ȣ��
    /// </summary>
    /// <param name="scene">�� ����</param>
    /// <param name="mode">�ε� ���</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized)
        {
            OnPreInitialize();
        }

        // additive�� �ƴҶ��� ����
        if (mode != LoadSceneMode.Additive)
        {
            OnInitialize();
        }
    }

    /// <summary>
    /// �̱����� ������� �� �� �ѹ��� ȣ��
    /// </summary>
    protected virtual void OnPreInitialize()
    {
        isInitialized = true;
    }

    /// <summary>
    /// �̱����� ��������� ���� ����� �� ���� ȣ��(additive�� �ȵ�)
    /// </summary>
    protected virtual void OnInitialize()
    {

    }

    private void OnApplicationQuit()
    {
        isShutdown = true;
    }
}

// �̱����� ������ ��ü�� 1���̾�� �Ѵ�.
public class TestSingleton
{
    private static TestSingleton instance = null;

    public static TestSingleton Instance
    {
        get
        {
            // ������ �ν��Ͻ��� ������� ���� ������
            if (instance == null)
            {
                // �ν��Ͻ� ����
                instance = new TestSingleton();
            }

            return instance;
        }
    }

    private TestSingleton()
    {
        // ��ü�� �ߺ����� �����Ǵ� ���� �����ϱ� ���� �����ڸ� private���� �Ѵ�.(�⺻ �����ڰ� ��������� ���� ����)
    }
}