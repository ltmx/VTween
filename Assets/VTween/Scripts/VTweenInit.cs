using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace VTWeen
{
    class TweeInitClass
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            AttachVComponent();
        } 

        private static void AttachVComponent()
        {
            var go = new GameObject();
            go.name = "~VTween-" + UnityEngine.Random.Range(100, int.MaxValue);
            go.AddComponent<VTweenMono>();
            VTweenManager.vtmono = go.GetComponent<VTweenMono>();
        }
    }
}