
namespace HotFix_Project
{
    public class MonoBehaviourController
    {
        private static bool m_entryGame = false;
        #region 生命周期函数
        public static void Awake()
        {
            
        }

        public static void OnEnable()
        {
            
        }

        public static void Start()
        {
            EntryGame();
        }

        public static void Update()
        {
            if (!m_entryGame) return;
            UIManager.Instance.Update(UnityEngine.Time.deltaTime);
            TimerManager.Instance.Update(UnityEngine.Time.deltaTime);
        }

        public static void FixedUpdate()
        {
            if (!m_entryGame) return;
        }

        public static void LateUpdate()
        {
            if (!m_entryGame) return;
        }

        public static void OnDisable()
        {
            m_entryGame = false;
        }

        public static void OnDestory()
        {
            ExistGame();
        }

        #endregion

        public static void EntryGame()
        {
            GameController.Instance.Initialization();
            TimerManager.Instance.Initialization();

            m_entryGame = true;
        }

        public static void ExistGame()
        {
            TimerManager.Instance.Release();
            GameController.Instance.Release();
        }

    }
}
