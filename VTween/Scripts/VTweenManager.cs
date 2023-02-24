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

namespace Breadnone.Extension
{
    public class VTweenManager
    {
        public static List<VTweenClass> activeTweens { get; private set; } = new List<VTweenClass>();
        public static VTweenClass[] unusedTweens;
        public static List<VTweenClass> pausedTweens { get; private set; } = new List<VTweenClass>();
        public static ArrayPool<EventVRegister> regs = ArrayPool<EventVRegister>.Shared;
        private static bool VWorkerIsRunning;
        public static VTweenMono vtmono { get; set; }
        public static void InitPool(int len){unusedTweens = new VTweenClass[len];}
        ///<summary>Gets registers.</summary>
        public static EventVRegister[] RentRegister(int len){return regs.Rent(len);}
        ///<summary>Returns the registers</summary>
        public static void ReturnRegister(EventVRegister[] events){regs.Return(events, true);}
        public static int RegisterLength{get;set;} = 10;
        ///<summary>Resizes the size of pool. Default is 10.</summary>
        public static void FlushPools(int poolSize){InitPool(poolSize);}
        ///<summary>Adds to active list.</summary>
        public static void InsertToActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.Tweening;
            activeTweens.Add(vtween);
            VTweenWorker();
        }
        ///<summary>Removes from active list.</summary>
        public static void RemoveFromActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.None;
            InsertRemoveUnused(vtween);
            activeTweens.Remove(vtween);
        }
        ///<summary>Removes from active list and returns back to the pool.</summary>
        public static void InsertRemoveUnused(VTweenClass vtween)
        {
            for (int i = 0; i < unusedTweens.Length; i++)
            {
                if (unusedTweens[i] is null)
                {
                    vtween.DefaultProperties();
                    unusedTweens[i] = vtween;
                    return;
                }
            }

            vtween.renewRegister(false);
        }
        ///<summary>Acts as background worker.</summary>
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
                    activeTweens[i].Exec();
                }
            }

            VWorkerIsRunning = false;
        }
        ///<summary>Cancels background worker, resets the lists.</summary>
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
        ///<summary>Adds to the pause list.</summary>
        public static void PoolToPaused(VTweenClass vtween)
        {
            activeTweens.Remove(vtween);
            vtween.ivcommon.state = TweenState.Paused;
            pausedTweens.Add(vtween);
        }
        ///<summary>Removes from the pause list</summary>
        public static void UnPoolPaused(VTweenClass vtween)
        {
            pausedTweens.Remove(vtween);
            vtween.ivcommon.state = TweenState.Tweening;

            if (!activeTweens.Contains(vtween))
                activeTweens.Add(vtween);

            VTweenWorker();
        }
    }
    ///<summary>VTween global Ids.</summary>
    public static class VTweenUtil{public static int VtweenGlobalId { get; set; } = 1;}
    ///<summary>Collection of VTween helper functions.</summary>
    public static class VExtension
    {
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
        ///<summary>Resumes the tween</summary>
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
        ///<summary>Resumes the tween.</summary>
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
                if(VTweenManager.unusedTweens[i].ivcommon.id == vid)
                {
                    VTweenManager.unusedTweens[i].Resume();
                    break;
                }
            }
        }
        ///<summary>Cancels the tween.</summary>
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

                for (int i = VTweenManager.unusedTweens.Length; i-- > 0;)
                {
                    var t = VTweenManager.unusedTweens[i];
                    t.Cancel();
                }
            }
        }
        ///<summary>Returns array of all active tweens.</summary>
        public static VTweenClass[] GetActiveTweens(){return VTweenManager.activeTweens.ToArray();}
        ///<summary>Returns array of all paused tweens.</summary>
        public static VTweenClass[] GetPausedTweens(){return VTweenManager.pausedTweens.ToArray();}
        ///<summary>VTween object pooling. Default is 5.</summary>
        public static T GetInstance<T>(int vid) where T : VTweenClass, new()
        {
            for (int i = VTweenManager.unusedTweens.Length; i-- > 0;)
            {
                if (VTweenManager.unusedTweens[i] is T validT)
                {
                    validT.ivcommon.id = vid;
                    VTweenManager.unusedTweens[i] = null;                    
                    return validT;
                }
            }
            // No need to be fancy here, null-ing the last and first slots is effective enough
            VTweenManager.unusedTweens[VTweenManager.unusedTweens.Length - 1] = null;
            VTweenManager.unusedTweens[0] = null;
            
            T nut = new T();
            nut.ivcommon.id = vid;
            return nut;
        }
    }
}