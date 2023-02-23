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

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Buffers;
using Collections.Pooled;

namespace VTWeen
{
    public class VTweenManager
    {
        public static PooledList<VTweenClass> activeTweens { get; private set; } = new PooledList<VTweenClass>();
        public static VTweenClass[] unusedTweens;
        public static PooledList<VTweenClass> pausedTweens { get; private set; } = new PooledList<VTweenClass>();
        public static ArrayPool<EventVRegister> regs = ArrayPool<EventVRegister>.Shared;
        public static ReadOnlySpan<VTweenClass> unused => unusedTweens;
        private static bool VWorkerIsRunning { get; set; }
        public static VTweenMono vtmono { get; set; }
        public static void InitPool(int len){unusedTweens = new VTweenClass[len];}
        public static EventVRegister[] RentRegister(int len){return regs.Rent(len);}
        public static void ReturnRegister(EventVRegister[] events){regs.Return(events, true);}
        public static int RegisterLength{get;set;} = 10;
        public static void InsertToActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.Tweening;
            activeTweens.Add(vtween);
            VTweenWorker();
        }

        public static void RemoveFromActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.None;
            InsertRemoveUnused(vtween);
            activeTweens.Remove(vtween);
        }
        public static void InsertRemoveUnused(VTweenClass vtween)
        {
            for (int i = 0; i < unused.Length; i++)
            {
                if (unused[i] is null)
                {
                    unusedTweens[i] = vtween;
                    return;
                }
            }

            vtween.renewRegister(false);
        }
        private static async void VTweenWorker()
        {
            if (VWorkerIsRunning)
                return;

            VWorkerIsRunning = true;
            var tsk = Task.Yield();

            while (activeTweens.Count > 0)
            {
                await tsk;

                for (int i = activeTweens.Count; i-- > 0;)
                {
                    activeTweens[i].ivcommon.Exec();
                }
            }

            VWorkerIsRunning = false;
        }
        public static void AbortVTweenWorker()
        {
            VWorkerIsRunning = false;

            if (activeTweens.Count > 0)
            {
                for (int i = activeTweens.Count; i-- > 0;)
                {
                    activeTweens[i].Cancel();
                }
            }

            if (pausedTweens.Count == 0)
                return;

            for (int i = pausedTweens.Count; i-- > 0;)
            {
                pausedTweens[i].Cancel();
            }
        }

        public static void PoolToPaused(VTweenClass vtween)
        {
            activeTweens.Remove(vtween);
            vtween.ivcommon.state = TweenState.Paused;
            pausedTweens.Add(vtween);
        }
        public static void UnPoolPaused(VTweenClass vtween)
        {
            pausedTweens.Remove(vtween);
            vtween.ivcommon.state = TweenState.Tweening;

            if (!activeTweens.Contains(vtween))
                activeTweens.Add(vtween);

            VTweenWorker();
        }
    }
}

namespace VTWeen.Extension
{
    public static class VTweenUtil
    {
        public static int VtweenGlobalId { get; set; } = 1;
    }

    public static class VExtension
    {
        public static float RunEaseTimeFloat(Ease easeType, float start, float end, float value)
        {
            return VEasings.ValEase(easeType, start, end, value);
        }
        public static Vector2 RunEaseTimeVector2(Ease easeType, Vector2 startPos, Vector2 endPos, float value)
        {
            return new Vector2(VEasings.ValEase(easeType, startPos.x, endPos.x, value), VEasings.ValEase(easeType, startPos.y, endPos.y, value));
        }
        public static Vector3 RunEaseTimeVector3(Ease easeType, Vector3 startPos, Vector3 endPos, float value)
        {
            return new Vector3(VEasings.ValEase(easeType, startPos.x, endPos.x, value), VEasings.ValEase(easeType, startPos.y, endPos.y, value), VEasings.ValEase(easeType, startPos.z, endPos.z, value));
        }
        public static Vector4 RunEaseTimeVector3(Ease easeType, Vector4 startPos, Vector4 endPos, float value)
        {
            return new Vector4(VEasings.ValEase(easeType, startPos.x, endPos.x, value), VEasings.ValEase(easeType, startPos.y, endPos.y, value), VEasings.ValEase(easeType, startPos.z, endPos.z, value), VEasings.ValEase(easeType, startPos.w, endPos.w, value));
        }
        public static void Pause(VTweenClass vtween, bool all = false)
        {
            if (!all)
            {
                if (vtween.ivcommon.state == TweenState.None || vtween.ivcommon.state == TweenState.Paused)
                    return;

                vtween.Pause();
            }
            else
            {
                for (int i = VTweenManager.activeTweens.Count; i-- > 0;)
                {
                    var t = VTweenManager.activeTweens[i];

                    if (t == null || t.ivcommon.state == TweenState.Paused || t.ivcommon.state == TweenState.None)
                        return;

                    t.Pause();
                }
            }
        }

        public static void Resume(VTweenClass vtween, bool all = false)
        {
            if (!all)
            {
                if (vtween.ivcommon.state != TweenState.Paused)
                    return;

                vtween.Resume();
            }
            else
            {
                if (VTweenManager.pausedTweens.Count == 0)
                    return;

                for (int i = VTweenManager.pausedTweens.Count; i-- > 0;)
                {
                    var t = VTweenManager.pausedTweens[i];
                    t.Resume();
                }
            }
        }
        public static void Cancel(int vid, bool state)
        {
            for (int i = VTweenManager.activeTweens.Count; i-- > 0;)
            {
                if(VTweenManager.activeTweens[i].ivcommon.id == vid)
                {
                    var t = VTweenManager.activeTweens[i];
                    t.Cancel(state);
                    return;
                }
            }

            if(VTweenManager.pausedTweens.Count == 0)
                return;
            
            for(int i = 1; i < VTweenManager.pausedTweens.Count; i++)
            {
                if(VTweenManager.unused[i].ivcommon.id == vid)
                {
                    VTweenManager.unused[i].Resume();
                    break;
                }
            }
        }
        public static void Cancel(VTweenClass vtween, bool all = false)
        {
            if (!all)
            {
                if (vtween.ivcommon.state != TweenState.None)
                    vtween.Cancel();
            }
            else
            {
                if (VTweenManager.activeTweens.Count > 0)
                {
                    for (int i = VTweenManager.activeTweens.Count; i-- > 0;)
                    {
                        var t = VTweenManager.activeTweens[i];
                        t.Cancel();
                    }
                }

                if (VTweenManager.pausedTweens.Count == 0)
                    return;

                for (int i = VTweenManager.unused.Length; i-- > 0;)
                {
                    var t = VTweenManager.unused[i];
                    t.Cancel();
                }

            }
        }
        public static VTweenClass[] GetActiveTweens()
        {
            return VTweenManager.activeTweens.ToArray();
        }
        public static VTweenClass[] GetPausedTweens()
        {
            return VTweenManager.pausedTweens.ToArray();
        }
        private static bool CheckTypes<T>(VTweenClass targetToCheck) where T : VTweenClass
        {
            return targetToCheck is T;
        }
        ///<summary>VTween object pooling. Default is 5.</summary>
        public static T GetInstance<T>(int vid) where T : VTweenClass, new()
        {
            for (int i = VTweenManager.unused.Length; i-- > 0;)
            {
                if (VTweenManager.unused[i] is null)
                    continue;

                if (VTweenManager.unused[i] is T validT)
                {
                    validT.ivcommon.id = vid;
                    VTweenManager.unusedTweens[i] = null;
                    return validT;
                }
            }

            T nut = new T();
            nut.ivcommon.id = vid;
            return nut;
        }
    }
}