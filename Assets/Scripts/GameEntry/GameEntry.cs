using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        yield return HotFixMgr.Instance.LoadHotFixAssembly();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
