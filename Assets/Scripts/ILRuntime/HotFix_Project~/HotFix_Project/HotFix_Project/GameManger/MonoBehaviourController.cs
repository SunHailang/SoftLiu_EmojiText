using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project
{
    public class MonoBehaviourController
    {

        public static void Awake()
        {
            //Debug.Log("Awake");
        }

        public static void OnEnable()
        {
            //Debug.Log("OnEnable");
        }

        public static void Start()
        {
            //Debug.Log("Start");
            var proxy = ResourceLoaderCore.ResourceLoaderProxy.GetInstance();
            Debug.Log(proxy.ToString());
        }

        public static void Update()
        {
            //Debug.Log($"Update -> deltaTime:{UnityEngine.Time.deltaTime}");
            TimerManager.Instance.Update(UnityEngine.Time.deltaTime);
        }

        public static void FixedUpdate()
        {
            //Debug.Log($"FixedUpdate -> fixedDeltaTime:{UnityEngine.Time.fixedDeltaTime}");
        }

        public static void LateUpdate()
        {
            //Debug.Log("LateUpdate");
        }

        public static void OnDisable()
        {
            //Debug.Log("OnDisable");
        }

        public static void OnDestory()
        {
            //Debug.Log("OnDestory");
        }

    }
}
