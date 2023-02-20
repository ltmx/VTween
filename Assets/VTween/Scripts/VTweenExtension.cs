using UnityEngine;
using VTWeen.Extension;
using System;

namespace VTWeen
{
    public class VTweenExecLater : VClass<VTweenExecLater>
    {
        Action callback;
        bool unscaledTime;
        public void SetBaseValues(float time, Action act, bool unscaled = false)
        {
            ivcommon.duration = time;
            callback = act;
            unscaledTime = unscaled;
            setOnComplete(act);
        }
        public void Init()
        {
            var t = new EventVRegister { callback = () => { 
                
                if (!unscaledTime)
                    ivcommon.runningTime += Time.deltaTime;
                else
                    ivcommon.runningTime += Time.unscaledDeltaTime;

            }, id = 1 };

            ivcommon.AddRegister(ref t);
            VTweenManager.InsertToActiveTween(this);
        }
    }
}