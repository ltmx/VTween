using System.Collections;
using System.Collections.Generic;
using Breadnone;
using System;
using UnityEngine;
using UnityEngine.UIElements;

//Struct based VTween impl test.
namespace Breadnone.Extension
{
    ///<summary>Fast struct based tweening class with less allocation.</summary>
    public ref struct STStructMove
    {
        public STStructMove(GameObject gameObject, Vector3 destination, float time, Ease ease = Ease.Linear, int loopCount = 0, bool pingpong = false, Action onComplete = null, bool local = false, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[4];
            sevent.unscaledTime = unscaledTime;
            sevent.loopAmount = loopCount;
            sevent.pingPong = pingpong;
            float runningTime = 0f;
            Vector3 defaultPos;
            sevent.sid = gameObject.GetInstanceID();
            Transform transform = gameObject.transform;

            if (!local) defaultPos = transform.position;
            else defaultPos = transform.localPosition;

            if (!local)
            {
                void call()
                {
                    if (sevent.IfCompleted(ref runningTime, ref time))
                        return;

                    var tmval = (runningTime / time);
                    transform.position = VEasings.ValEase(ease, defaultPos, destination, tmval);
                    sevent.CalcRuntime(ref runningTime);
                }

                sevent.onStart(call);
            }
            else
            {
                void call()
                {
                    if (sevent.IfCompleted(ref runningTime, ref time))
                        return;

                    var tmval = (runningTime / time);
                    transform.localPosition = VEasings.ValEase(ease, defaultPos, destination, tmval);
                    sevent.CalcRuntime(ref runningTime);
                }

                sevent.onStart(call);
            }

            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);

            if(loopCount > 0)
            {
                void reset()
                {
                    runningTime = 0f;

                    if(pingpong)
                    {
                        Utilv.SwapRefs<Vector3>(ref defaultPos, ref destination);
                    }
                    else
                    {
                        gameObject.transform.position = destination;
                    }
                }

                sevent.onReset(reset);
            }

            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }

    ///<summary>Struct based object following.</summary>
    public ref struct STStructFollow
    {
        public STStructFollow(GameObject gameObject, Transform target, float speed, Vector3 smoothness)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            Vector3 defaultPos;
            sevent.sid = gameObject.GetInstanceID();
            defaultPos = gameObject.transform.position;
            var transform = gameObject.transform;

            void call()
            {
                transform.position = Vector3.SmoothDamp(transform.position, target.position, ref smoothness, speed);
            }

            sevent.onStart(call);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Struct based tweening.</summary>
    public ref struct STStructMoveUI
    {
        public STStructMoveUI(VisualElement visualElement, Vector3 destination, float time, Ease ease = Ease.Linear, int loopCount = 0, Action onComplete = null, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            Vector3 defaultPos;
            sevent.sid = visualElement.GetHashCode();
            defaultPos = visualElement.transform.position;

            void call()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                var tmval = (runningTime / time);
                var t = VEasings.ValEase(ease, defaultPos, destination, tmval);
                visualElement.style.translate = new Translate(t.x, t.y, t.z);
                sevent.CalcRuntime(ref runningTime);
            }

            sevent.onStart(call);
            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Struct rotate api.</summary>
    public ref struct STStructRotate
    {
        public STStructRotate(GameObject gameObject, float degreeAngle, Vector3 direction, float time, Ease ease = Ease.Linear, Action onComplete = null, bool localSpace = false, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            sevent.sid = gameObject.GetInstanceID();
            Quaternion currentRotation;
            Transform transform = gameObject.transform;

            if (!localSpace)
                currentRotation = transform.rotation * Quaternion.AngleAxis(degreeAngle, direction);
            else
                currentRotation = transform.localRotation * Quaternion.AngleAxis(degreeAngle, direction);

            void callback()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                transform.rotation = Quaternion.AngleAxis(VEasings.ValEase(ease, 0f, degreeAngle, (runningTime / time)), direction);
                sevent.CalcRuntime(ref runningTime);
            }

            void callbackLocal()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                transform.localRotation = Quaternion.AngleAxis(VEasings.ValEase(ease, 0f, degreeAngle, (runningTime / time)), direction);
                sevent.CalcRuntime(ref runningTime);
            }

            if (!localSpace)
                sevent.onStart(callback);
            else
                sevent.onStart(callbackLocal);

            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Struct rotate api.</summary>
    public ref struct STStructRotateUI
    {
        public STStructRotateUI(VisualElement visualElement, float degreeAngle, float time, Ease ease = Ease.Linear, Action onComplete = null, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            sevent.sid = visualElement.GetHashCode();

            void callback()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                visualElement.style.rotate = new Rotate(VEasings.ValEase(ease, 0f, degreeAngle, (runningTime / time)));
                sevent.CalcRuntime(ref runningTime);
            }

            sevent.onStart(callback);
            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Struct scale api.</summary>
    public ref struct STStructScale
    {
        public STStructScale(GameObject gameObject, Vector3 scale, float time, Ease ease = Ease.Linear, Action onComplete = null, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            Vector3 defaultScale = gameObject.transform.localScale;
            sevent.sid = gameObject.GetInstanceID();
            Transform transform = gameObject.transform;

            void call()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                var tmval = (runningTime / time);
                transform.localScale = VEasings.ValEase(ease, defaultScale, scale, tmval);
                sevent.CalcRuntime(ref runningTime);
            }

            sevent.onStart(call);
            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Struct scale api.</summary>
    public ref struct STStructScaleUI
    {
        public STStructScaleUI(VisualElement visualElement, Vector3 scale, float time, Ease ease = Ease.Linear, Action onComplete = null, bool localSpace = false, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            Vector3 defaultScale = visualElement.style.scale.value.value;
            sevent.sid = visualElement.GetHashCode();

            void call()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                var tmval = (runningTime / time);
                visualElement.style.scale = new Scale(VEasings.ValEase(ease, defaultScale, scale, tmval));
                sevent.CalcRuntime(ref runningTime);
            }

            sevent.onStart(call);
            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    public ref struct STStructValue
    {
        public STStructValue(float from, float to, float time, Action callback, Ease ease = Ease.Linear, Action onComplete = null, bool unscaledTime = false)
        {
            var sevent = new VSCore();
            sevent.registers = new StructVRegister[2];
            float runningTime = 0f;
            float runningValue;
            sevent.sid = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            void call()
            {
                if (sevent.IfCompleted(ref runningTime, ref time))
                    return;

                runningValue = VEasings.ValEase(ease, from, to, (runningTime / time));
                sevent.CalcRuntime(ref runningTime);
            }

            sevent.onStart(call);
            sevent.onComplete(onComplete);
            sevent.ChangeState(TweenState.Tweening);
            VTweenManager.FastStructInsertToActive(() => sevent.Exec(), () => sevent.ExecOnComplete(false), sevent.sid);
        }
    }
    ///<summary>Handles events.</summary>
    public struct VSCore
    {
        public int sid { get; set; }
        public Action exec { get; set; }
        public Action oncomplete { get; set; }
        public Action onreset{get;set;}
        public StructVRegister[] registers { get; set; }
        public TweenState state;
        public bool unscaledTime { get; set; }
        public void ChangeState(TweenState vstate) { state = vstate; }
        public int loopAmount { get; set; }
        public int loopCounter { get; set; }
        public bool pingPong { get; set; }
        public int pingPongCounter { get; set; }
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
            if (callback == null)
                return;

            addEvent(callback, 2);
            oncomplete += callback;
        }
        public void onReset(Action callback)
        {
            addEvent(callback, 3);
            onreset += callback;
        }
        private void addEvent(Action callback, int id)
        {
            for (int i = 0; i < registers.Length; i++)
            {
                if (registers[i].callback == null)
                {
                    registers[i] = new StructVRegister { callback = callback, id = id };
                    break;
                }
            }
        }
        public void ExecOnComplete(bool execOnComplete)
        {
            if (state == TweenState.None)
                return;

            ChangeState(TweenState.None);
            VTweenManager.FastStructRemoveFromActive(sid);

            if (execOnComplete)
                oncomplete?.Invoke();

            for (int i = 0; i < registers.Length; i++)
            {
                if (registers[i].callback == null)
                    continue;

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
        ///<summary>Scaled or unscaled time.</summary>
        public void CalcRuntime(ref float runtime)
        {
            if (!unscaledTime)
                runtime += Time.deltaTime;
            else
                runtime += Time.unscaledDeltaTime;
        }
        ///<summary>Checks if tween already finished.</summary>
        public bool IfCompleted(ref float runningTime, ref float time)
        {
            if (runningTime + 0.0001 > time)
            {
                if (loopAmount > 0)
                {
                    onreset.Invoke();
                    loopCounter++;

                    if(!pingPong)
                    {
                        if(loopAmount != loopCounter)
                            return false;
                    }
                    else
                    {                        
                        if(loopCounter != loopAmount * 2)
                        {
                            return false;
                        }
                    }
                }

                ExecOnComplete(true);
                return true;
            }

            return false;
        }
        public VSCore onLoop(int loopCount)
        {
            this.loopAmount = loopCount;
            return this;
        }
        public VSCore onPingPong(bool state)
        {
            this.pingPong = state;
            return this;
        }
    }
}