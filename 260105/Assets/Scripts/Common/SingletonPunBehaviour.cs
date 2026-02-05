using Photon.Pun;

public class SingletonPunBehaviour<T> : MonoBehaviourPunCallbacks where T : SingletonPunBehaviour<T>
{
    protected bool _isDestroyOnLoad = false;

    protected static T _instance = null;

    public static T Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            if (_isDestroyOnLoad)
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
        _instance = null;
    }
}

