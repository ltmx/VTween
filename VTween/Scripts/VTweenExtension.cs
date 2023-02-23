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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace VTWeen.Extension
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
            void evt()
            {
                if (!unscaledTime)
                    ivcommon.runningTime += Time.deltaTime;
                else
                    ivcommon.runningTime += Time.unscaledDeltaTime;
            }

            ivcommon.AddRegister(new EventVRegister{callback = evt, id = 1});
            VTweenManager.InsertToActiveTween(this);
        }
    }
    public class VTweenQueue
    {
        private List<VTweenClass> vtweens = new List<VTweenClass>();
        private Action oncomplete;
        private Ease? ease;

        public VTweenQueue add(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.None;
            vtweens.Add(vtween);
            return this;
        }
        public void start()
        {
            for (int i = 0; i < vtweens.Count; i++)
            {
                if (i < vtweens.Count - 1)
                {
                    int idx = i + 1;

                    vtweens[i].ivcommon.onComplete(() =>
                    {
                        var vts = vtweens[idx] as IVDefaultIntern;

                        if (vts  is object)
                        {
                            vts.defaultsetter();
                        }
                        else
                        {
                            vtweens[idx].ivcommon.state = TweenState.Tweening;
                        }
                    });
                }
            }
            
            vtweens[vtweens.Count - 1].ivcommon.onComplete(oncomplete);
            vtweens[0].ivcommon.state = TweenState.Tweening;
        }
        public VTweenQueue stop()
        {
            for (int i = vtweens.Count; i-- > 0;)
            {
                vtweens[i].Cancel();
            }
            return this;
        }
        public VTweenQueue setOnComplete(Action callback)
        {
            oncomplete = callback;
            return this;
        }
    }
}