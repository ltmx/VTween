/*
MIT License

Copyright 2023 Stevphanie Ricardo

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify,
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using VTWeen.Extension;
using System;

namespace VTWeen
{
    ///<summary>Delayed execution. Respects both scaled/unscaled time</summary>
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

            ivcommon.AddRegister(t);
            VTweenManager.InsertToActiveTween(this);
        }
    }
    public class VTweenQueue
    {

    }
}