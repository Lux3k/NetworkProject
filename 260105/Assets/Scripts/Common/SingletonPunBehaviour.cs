using Photon.Pun;

public class SingletonPunBehaviour<T> : MonoBehaviourPunCallbacks where T : SingletonPunBehaviour<T>
{
    protected bool isDestroyOnLoad = false;

    protected static T instance = null;

    public static T Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        if (instance == null)
        {
            instance = (T)this;
            if (isDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //삭제시 호출
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    //삭제시 추가로 처리할 내용이 있으면 오버라이드해서 사용
    protected virtual void Dispose()
    {
        instance = null;
    }
}

