using UnityEngine;
using Puerts;
using System;
using System.Collections;
using System.Threading;

namespace PuertsTest
{
    public delegate void ModuleInit(JsBehaviour monoBehaviour);

    //只是演示纯用js实现MonoBehaviour逻辑的可能，
    //但从性能角度这并不是最佳实践，会导致过多的跨语言调用
    public class JsBehaviour : MonoBehaviour
    {
        public string ModuleName;//可配置加载的js模块

        public Action JsStart;
        public Action JsUpdate;
        public Action JsOnDestroy;

        static JsEnv jsEnv;
        public bool isRunning;


        void Awake()
        {
            isRunning = true;
            if (jsEnv == null) jsEnv = new JsEnv(new DefaultLoader(), 9229);
            jsEnv.UsingAction<float>();
            var init = jsEnv.Eval<ModuleInit>("const m = require('" + ModuleName + "'); m.init;");

            if (init != null) init(this);
        }

        void Start()
        {
            if (JsStart != null) JsStart();
          
            Thread t = new Thread(new ThreadStart(()=> {
                while (isRunning) {
                    if (JsUpdate != null)
                        JsUpdate();
                    Thread.Sleep(100);
                }
            }));
            t.Start();
            t.IsBackground = true;
            
        }

        void Update()
        {
            jsEnv.Tick();
        }

        void OnDestroy()
        {
            isRunning = false;
            if (JsOnDestroy != null) JsOnDestroy();
            JsStart = null;
            JsUpdate = null;
            JsOnDestroy = null;
        }

        public IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(1);
            UnityEngine.Debug.Log("coroutine done");
        }
    }
}