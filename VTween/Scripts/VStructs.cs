using System.Collections;
using System.Collections.Generic;
using Breadnone;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Buffers;

//Struct based VTween impl test.
namespace Breadnone.Extension
{
    ///<summary>Fast struct based tweening class with less allocation.</summary>
    public ref struct STStructMove
    {
        SEvent svt;
        public STStructMove(GameObject gameObject, Vector3 destination, float time, float loopCount, Ease ease = Ease.Linear, Action onComplete = null, bool localSpace = false, bool unscaledTime = false)
        {
            var sevent = new SEvent();
            svt = sevent;
            sevent.registers = new List<EventVRegister>(2);

            float runningTime = 0f;
            Vector3 defaultPos;
            sevent.sid = gameObject.GetInstanceID();

            if (!localSpace)
                defaultPos = gameObject.transform.position;
            else
                defaultPos = gameObject.transform.localPosition;

            bool done = false;

            if (!localSpace)
            {
                void call()
                {
                    if (runningTime + 0.0001f > time)
                    {
                        if (done) return;
                        sevent.ExecOnComplete(true);
                        done = true;
                    }

                    gameObject.transform.position = VEasings.ValEase(ease, defaultPos, destination, (runningTime / time));
                    runningTime += Time.deltaTime;
                }

                sevent.onStart(call);
            }
            else
            {
                void call()
                {
                    if (runningTime + 0.0001f > time)
                    {
                        if (done)
                            return;

                        sevent.ExecOnComplete(true);
                        done = true;
                    }

                    gameObject.transform.localPosition = VEasings.ValEase(ease, defaultPos, destination, (runningTime / time));
                    runningTime += Time.deltaTime;
                }

                sevent.onStart(call);
            }

            if (onComplete != null)
                sevent.onComplete(onComplete);

            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(sevent.exec, sevent.sid);
        }

        public void TryCancel(bool execOnComplete = false) { svt.ExecOnComplete(execOnComplete); }
    }
    ///<summary>Struct based object following.</summary>
    public ref struct STStructFollow
    {
        SEvent svt;
        public STStructFollow(GameObject gameObject, Transform target, float speed, Vector3 smoothness)
        {
            var sevent = new SEvent();
            svt = sevent;
            sevent.registers = new List<EventVRegister>(2);

            float runningTime = 0f;
            Vector3 defaultPos;
            sevent.sid = gameObject.GetInstanceID();
            defaultPos = gameObject.transform.position;
            var transform = gameObject.transform;

            void call()
            {
                transform.position = Vector3.SmoothDamp(transform.position, target.position, ref smoothness, speed);
                runningTime += Time.deltaTime;
            }

            sevent.onStart(call);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(sevent.exec, sevent.sid);
        }

        public void TryCancel(bool execOnComplete) { svt.ExecOnComplete(execOnComplete); }
    }
    ///<summary>Struct based tweening.</summary>
    public ref struct STStructMoveUI
    {
        SEvent svt;
        public STStructMoveUI(VisualElement visualElement, Vector3 destination, float time, float loopCount, Ease ease = Ease.Linear, Action onComplete = null, bool unscaledTime = false)
        {
            var sevent = new SEvent();
            svt = sevent;
            sevent.registers = new List<EventVRegister>(2);

            float runningTime = 0f;
            Vector3 defaultPos;
            sevent.sid = visualElement.GetHashCode();
            defaultPos = visualElement.transform.position;
            bool done = false;

            void call()
            {
                if (runningTime + 0.0001f > time)
                {
                    if (done) return;
                    sevent.ExecOnComplete(true);
                    done = true;
                }

                var t = VEasings.ValEase(ease, defaultPos, destination, (runningTime / time));
                visualElement.style.translate = new Translate(t.x, t.y, t.z);
                runningTime += Time.deltaTime;
            }

            sevent.onStart(call);

            if (onComplete != null)
                sevent.onComplete(onComplete);

            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(sevent.exec, sevent.sid);
        }

        public void TryCancel(bool execOnComplete){svt.ExecOnComplete(execOnComplete);}
    }
    public struct SEvent
    {
        public int sid { get; set; }
        public Action exec { get; set; }
        public Action oncomplete { get; set; }
        public List<EventVRegister> registers { get; set; }
        public TweenState state;
        public void ChangeState(TweenState vstate) { state = vstate; }
        public TweenState isTweening { get { return state; } }
        public void Exec()
        {
            if (state != TweenState.Tweening)
                return;

            exec.Invoke();
        }

        public void onStart(Action callback)
        {
            addEvent(callback, 1);
            exec += callback;
        }

        public void onComplete(Action callback)
        {
            addEvent(callback, 2);
            oncomplete += callback;
        }
        private void addEvent(Action callback, int id)
        {
            registers.Add(new EventVRegister { callback = callback, id = id });
        }
        public void ExecOnComplete(bool execOnComplete)
        {
            ChangeState(TweenState.None);
            VTweenManager.FastStructRemoveFromActive(exec, sid);

            if(execOnComplete)
                oncomplete.Invoke();
            
            DisposeEvents();
        }
        public void DisposeEvents()
        {
            for (int i = 0; i < registers.Count; i++)
            {
                if (registers[i].id == 1)
                {
                    exec -= registers[i].callback;
                }
                else if (registers[i].id == 2)
                {
                    oncomplete -= registers[i].callback;
                }
            }
        }
        public void Cancel(bool execOnComplete)
        {
            this.ExecOnComplete(execOnComplete);
        }
        public void Pause()
        {
            this.state = TweenState.Paused;
        }
    }
}