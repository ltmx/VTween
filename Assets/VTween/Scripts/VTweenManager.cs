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

namespace VTWeen
{
    public class VTweenManager
    {
        public static List<VTweenClass> activeTweens { get; private set; } = new List<VTweenClass>();
        public static List<VTweenClass> pausedTweens { get; private set; } = new List<VTweenClass>();
        private static bool VWorkerIsRunning { get; set; }
        public static VTweenMono vtmono { get; set; }
        public static void InsertToActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.Tweening;
            activeTweens.Add(vtween);
            VTweenWorker();
        }

        public static void RemoveFromActiveTween(VTweenClass vtween)
        {
            vtween.ivcommon.state = TweenState.None;
            activeTweens.Remove(vtween);
        }

        private static async void VTweenWorker()
        {
            if (VWorkerIsRunning)
                return;

            VWorkerIsRunning = true;

            while (activeTweens.Count > 0)
            {
                await Task.Yield();

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

                for (int i = VTweenManager.pausedTweens.Count; i-- > 0;)
                {
                    var t = VTweenManager.pausedTweens[i];
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
        public static T GetFromPoolSlot<T>() where T : VTweenClass, new()
        {
            var instance = new T();
            return instance;
        }
        private static bool CheckTypes<T>(VTweenClass targetToCheck) where T : VTweenClass
        {
            return targetToCheck is T;
        }
    }
}