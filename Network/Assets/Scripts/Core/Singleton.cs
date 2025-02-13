using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    [Tooltip("이 싱글톤이 초기화 됐는지 확인")]
    private bool isInitialized = false;
    [Tooltip("종료처리에 들어갔는지 확인")]
    private static bool isShutdown = false;
    [Tooltip("이 싱글톤의 객체(인스턴스)")]
    private static T instance = null;

    [Tooltip("이 싱글톤의 객체를 읽기 위한 프로퍼티")]
    public static T Instance
    {
        get
        {
            // 종료처리에 들어갔으면
            if (isShutdown)
            {
                // 경고 출력하고
                Debug.Log("싱글톤은 이미 삭제 중 이다");

                // 리턴
                return null;
            }

            // 객체가 없으면
            if (instance == null)
            {
                // 다른 게임 오브젝트에 해당 싱글톤이 있는지 확인
                T singleton = FindAnyObjectByType<T>();

                // 다른 게임 오브젝트에도 이 싱글톤이 없으면
                if (singleton == null)
                {
                    // 빈 게임 오브젝트 만들고
                    GameObject obj = new GameObject();

                    // 이름 지정한 다음
                    obj.name = "Singleton";
                    // 싱글톤 컴포넌트 만들어서 추가
                    singleton = obj.AddComponent<T>();
                }

                // 다른 게임 오브젝트에 있는 싱글톤이나 새로 만든 싱글톤ㅇ르 저장
                instance = singleton;

                // 씬이 사라질 때 게임 오브젝트가 삭제 되지 않도록 설정
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    private void Awake()
    {
        // 씬에 이미 배치된 다른 싱글톤이 없다
        if (instance == null)
        {
            // 첫번째를 저장
            instance = this as T;

            // 씬이 사라질 때 게임 오브젝트가 삭제되지 않도록 설정
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            // 이미 씬에 싱글톤이 있다
            if (instance != this)    // 그게 나 자신이 아니면
            {
                // 나 자신을 삭제
                Destroy(this.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        // SceneManager.sceneLoaded는 씬이 로드되면 실행되는 델리게이트
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 로드되었을 때 호출
    /// </summary>
    /// <param name="scene">씬 정보</param>
    /// <param name="mode">로딩 모드</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized)
        {
            OnPreInitialize();
        }

        // additive가 아닐때만 실행
        if (mode != LoadSceneMode.Additive)
        {
            OnInitialize();
        }
    }

    /// <summary>
    /// 싱글톤이 만들어질 때 단 한번만 호출
    /// </summary>
    protected virtual void OnPreInitialize()
    {
        isInitialized = true;
    }

    /// <summary>
    /// 싱글톤이 만들어지고 씬이 변경될 때 마다 호출(additive는 안됨)
    /// </summary>
    protected virtual void OnInitialize()
    {

    }

    private void OnApplicationQuit()
    {
        isShutdown = true;
    }
}

// 싱글톤은 무조건 객체가 1개이어야 한다.
public class TestSingleton
{
    private static TestSingleton instance = null;

    public static TestSingleton Instance
    {
        get
        {
            // 이전에 인스턴스가 만들어진 적이 없으면
            if (instance == null)
            {
                // 인스턴스 생성
                instance = new TestSingleton();
            }

            return instance;
        }
    }

    private TestSingleton()
    {
        // 객체가 중복으로 생성되는 것을 방지하기 위해 생성자를 private으로 한다.(기본 생성자가 만들어지는 것을 방지)
    }
}