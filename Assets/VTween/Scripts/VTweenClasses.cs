using UnityEngine;
using System;
using UnityEngine.UIElements;
using VTWeen.Extension;
using System.Collections.Generic;

//Main VTween classes....

namespace VTWeen
{
    ///<summary>Base class of VTween. Shares common properties.</summary>
    public class VTweenClass : VTweenException, IVCommonBase
    {
        //Hide public members with excplicit methods
        public IVCommonBase ivcommon { get; private set; }
        int IVCommonBase.id { get; set; }
        int IVCommonBase.loopAmount { get; set; }
        int IVCommonBase.loopCounter { get; set; }
        int IVCommonBase.pingpongCounter { get; set; }
        Ease IVCommonBase.easeType { get; set; } = Ease.Linear;
        bool IVCommonBase.isLocal { get; set; }
        bool IVCommonBase.unscaledTime { get; set; }
        bool IVCommonBase.pingpong { get; set; }

        float IVCommonBase.duration { get; set; }
        float IVCommonBase.runningTime { get; set; }
        bool IVCommonBase.speedBased { get; set; }
        bool IVCommonBase.oncompleteRepeat { get; set; }
        TweenState IVCommonBase.state { get; set; }

        Action IVCommonBase.exec { get; set; }
        Action IVCommonBase.oncomplete { get; set; }
        Action IVCommonBase.softreset { get; set; }

        List<EventVRegister> IVCommonBase.registers { get; set; }

        ///<summary>Adds/removes invocation of a delegate.</summary>
        private void AddClearEvent(ref EventVRegister register, bool falseClearTrueAdd)
        {
            if (register.id == 2)
            {
                if (falseClearTrueAdd) ivcommon.oncomplete += register.callback;
                else ivcommon.oncomplete -= register.callback;

            }
            else if (register.id == 1)
            {
                if (falseClearTrueAdd) ivcommon.exec += register.callback;
                else ivcommon.exec -= register.callback;

            }
            else if (register.id == 3)
            {
                if (falseClearTrueAdd) ivcommon.softreset += register.callback;
                else ivcommon.softreset -= register.callback;
            }
        }
        ///<summary>Assigns callback to onComplete, onUpdate, exec.</summary>
        void IVCommonBase.AddRegister(ref EventVRegister register)
        {
            ivcommon.registers.Add(register);
            AddClearEvent(ref register, true);
        }
        ///<summary>Registers init.</summary>
        public VTweenClass()
        {
            ivcommon = this;
            ivcommon.registers = new List<EventVRegister>();
            ivcommon.id = VTweenUtil.VtweenGlobalId++;
        }
        ///<summary>Executes scaled/unsclade runningTime.</summary>
        private void ExecRunningTime()
        {
            if (!ivcommon.unscaledTime)
                ivcommon.runningTime += Time.deltaTime;
            else
                ivcommon.runningTime += Time.unscaledDeltaTime;
        }
        ///<summary>Floating point ease impl.</summary>
        float IVCommonBase.RunEaseTimeFloat(float start, float end)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = VEasings.ValEase(ivcommon.easeType, start, end, tm);
            ExecRunningTime();
            return res;
        }
        ///<summary>Vector3 ease impl.</summary>
        Vector3 IVCommonBase.RunEaseTimeVector3(Vector3 startPos, Vector3 endPos)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = new Vector3(VEasings.ValEase(ivcommon.easeType, startPos.x, endPos.x, tm), VEasings.ValEase(ivcommon.easeType, startPos.y, endPos.y, tm), VEasings.ValEase(ivcommon.easeType, startPos.z, endPos.z, tm));
            ExecRunningTime();
            return res;
        }
        ///<summary>Amount of loops a single tween.</summary>
        VTweenClass IVCommonBase.setLoop(int loopCount)
        {
            ivcommon.loopAmount = loopCount;
            return this;
        }
        ///<summary>Repeats callback on each completion of a loop.</summary>
        VTweenClass IVCommonBase.onCompleteRepeat(bool state)
        {
            ivcommon.oncompleteRepeat = state;
            return this;
        }
        ///<summary>Back and forth, pingpong-like animation.</summary>
        VTweenClass IVCommonBase.setPingPong(bool state)
        {
            ivcommon.pingpong = state;
            return this;
        }
        ///<summary>Speed based movement, will ignore duration.</summary>
        VTweenClass IVCommonBase.setSpeed(float speed)
        {
            ivcommon.speedBased = true;
            ivcommon.duration = speed;
            return this;
        }
        ///<summary>Ease function to be used with the tween.</summary>
        VTweenClass IVCommonBase.setEase(Ease ease)
        {
            ivcommon.easeType = ease;
            return this;
        }
        ///<summary>Execute method chain, will be executed every frame.</summary>
        void IVCommonBase.Exec()
        {
            if (ivcommon.state != TweenState.Tweening)
                return;

            if (ivcommon.runningTime >= ivcommon.duration)
            {
                CheckIfFinished();
                return;
            }

            ivcommon.exec.Invoke();
        }
        ///<summary>Will be executed at the very end, the next frame the tween completed</summary>
        VTweenClass IVCommonBase.onComplete(Action callback)
        {
            var t = new EventVRegister { callback = callback, id = 2 };
            ivcommon.AddRegister(ref t);
            return this;
        }
        ///<summary>Callback to execute every frame while tweening.</summary>
        VTweenClass IVCommonBase.onUpdate(Action callback)
        {
            var t = new EventVRegister { callback = callback, id = 1 };
            //Keeps running while tweening.
            ivcommon.AddRegister(ref t);
            return this;
        }
        ///<summary>Cancels the tween, returns to pool.</summary>
        public void Cancel(bool executeOnComplete = false)
        {
            if(ivcommon.state != TweenState.Tweening)
                return;

            if (executeOnComplete)
            {
                ivcommon.oncomplete.Invoke();
            }

            if (ivcommon.state == TweenState.Paused)
            {
                VTweenManager.pausedTweens.Remove(this);
                Clear(false);
            }
            else
            {
                Clear(true);
            }
        }
        ///<summary>Checks if tweening already was done or still tweening</summary>
        public void CheckIfFinished()
        {
            if (ivcommon.loopAmount > 0)
            {
                ivcommon.loopCounter++;

                if (ivcommon.loopAmount > ivcommon.loopCounter)
                {
                    ivcommon.runningTime = 0f;
                    ivcommon.softreset?.Invoke(); //Check her is necessary

                    if (ivcommon.pingpong)
                    {
                        ivcommon.pingpongCounter++;
                    }
                    else
                    {
                        if (ivcommon.oncompleteRepeat)
                        {
                            ivcommon.oncomplete.Invoke();
                        }
                    }

                    return;
                }

                if (ivcommon.pingpong && ivcommon.pingpongCounter != ivcommon.loopAmount)
                {
                    ivcommon.runningTime = 0f;
                    ivcommon.loopCounter = 0;
                    ivcommon.softreset.Invoke();

                    if (ivcommon.oncompleteRepeat)
                    {
                        ivcommon.oncomplete.Invoke();
                    }

                    return;
                }
            }

            try
            {
                if (ivcommon.loopAmount == 0)
                {
                    ivcommon.oncomplete.Invoke();
                    Clear(true);
                }
                else
                {
                    if (ivcommon.loopCounter == ivcommon.loopAmount)
                    {
                        ivcommon.oncomplete.Invoke();
                        Clear(true);
                    }
                }

            }
            catch (Exception e)
            {
                throw new VTweenException(e.Message);
            }
        }

        //int = 1 onEnd //int = 2 onComplete //int = 3 softReset
        ///<summary>Clears invocation lists, prevents leaks.</summary>
        private void ClearInvocations()
        {
            for (int i = 0; i < ivcommon.registers.Count; i++)
            {
                var t = ivcommon.registers[i];
                AddClearEvent(ref t, false);
            }
        }
        ///<summary>Set common properties to default value.</summary>
        private void Clear(bool removeFromActiveList)
        {
            ClearInvocations();
            if (removeFromActiveList)
                VTweenManager.RemoveFromActiveTween(this);
        }
        ///<summary>Checks if tweening.</summary>
        public bool IsTweening() { return ivcommon.state != TweenState.None; }

        ///<summary>Pauses the tweening.</summary>
        public void Pause()
        {
            if (ivcommon.state == TweenState.Tweening)
            {
                VTweenManager.PoolToPaused(this);
            }
        }
        ///<summary>Resumes paused tween instances, if any.</summary>
        public void Resume()
        {
            if (ivcommon.state == TweenState.Paused)
            {
                VTweenManager.UnPoolPaused(this);
            }
        }
    }

    ///<summary>Move class. Moves object to target position.</summary>
    public class VTweenMove : VClass<VTweenMove>
    {
        private Vector3 destination;
        private Transform fromIns;
        private ITransform fromInsUi;
        private Vector3 defaultPosition;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, ITransform itrans, Vector3 dest, Vector3 defPos, float time)
        {
            destination = dest;
            fromIns = trans;
            fromInsUi = itrans;
            defaultPosition = defPos;
            ivcommon.duration = time;
        }
        ///<summary>Shuffling the to/from properties.</summary>
        private void LoopReset()
        {
            if (fromIns != null)
            {
                if (!ivcommon.isLocal)
                {
                    if (!ivcommon.pingpong)
                    {
                        fromIns.transform.position = defaultPosition;
                    }
                }
                else
                {
                    if (!ivcommon.pingpong)
                    {
                        fromIns.transform.localPosition = defaultPosition;
                    }
                }
            }
            else if (fromInsUi != null)
            {
                if (!ivcommon.pingpong)
                    fromInsUi.position = defaultPosition;
            }

            if (ivcommon.pingpong)
            {
                var dest = destination;
                destination = defaultPosition;
                defaultPosition = dest;
            }
        }
        ///<summary>Main even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            Action callback = () =>
            {
                if (fromIns != null)
                {
                    if (!ivcommon.isLocal)
                    {
                        fromIns.position = ivcommon.RunEaseTimeVector3(defaultPosition, destination);
                    }
                    else
                    {
                        fromIns.localPosition = ivcommon.RunEaseTimeVector3(defaultPosition, destination);
                    }
                }
                else if (fromInsUi != null)
                {
                    fromInsUi.position = ivcommon.RunEaseTimeVector3(defaultPosition, destination);
                }
                else
                {
                    throw new VTweenException("GameObject/Transform can't be null");
                }
            };

            var t = new EventVRegister { callback = callback, id = 1 };
            var tx = new EventVRegister { callback = LoopReset, id = 3 };
            ivcommon.AddRegister(ref t);
            ivcommon.AddRegister(ref tx);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Repositioning initial position of object.</summary>
        public VTweenMove setFrom(Vector3 fromPosition)
        {
            if (fromIns != null)
            {
                if (!ivcommon.isLocal)
                    fromIns.position = fromPosition;
                else
                    fromIns.localPosition = fromPosition;
            }
            else if (fromInsUi != null)
            {
                fromInsUi.position = fromPosition;
            }

            defaultPosition = fromPosition;
            return this;
        }
        ///<summary>Sets target to look at while moving.</summary>
        public VTweenMove setLookAt(Vector3 targetToLookAt)
        {
            var evt = new EventVRegister
            {
                callback = () =>
                {
                    fromIns.transform.LookAt(targetToLookAt);
                },
                id = 1
            };
            ivcommon.AddRegister(ref evt);
            return this;
        }
    }

    ///<summary>Rotates object</summary>
    public class VTweenRotate : VClass<VTweenRotate>
    {
        private float degreeAngle;
        private Transform transform;
        private Quaternion defaultRotation;
        private Vector3 direction;
        private Quaternion currentRotation;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, float angle, Vector3 type, float time)
        {
            transform = trans;
            degreeAngle = angle;
            ivcommon.duration = time;
            defaultRotation = trans.rotation;
            direction = type;
        }
        ///<summary>Resets properties shuffle the destination</summary>
        private void LoopReset()
        {
            if (!ivcommon.isLocal)
                transform.transform.rotation = currentRotation;
            else
                transform.transform.localRotation = currentRotation;

            if (ivcommon.pingpong)
            {
                degreeAngle = -degreeAngle;
            }
        }
        ///<summary>Main even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            if (!ivcommon.isLocal)
                currentRotation = transform.rotation * Quaternion.AngleAxis(degreeAngle, direction);
            else
                currentRotation = transform.localRotation * Quaternion.AngleAxis(degreeAngle, direction);

            Action callback = () =>
            {
                if (transform != null)
                {
                    if (!ivcommon.isLocal)
                    {
                        transform.rotation = Quaternion.AngleAxis(ivcommon.RunEaseTimeFloat(0f, degreeAngle), direction);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.AngleAxis(ivcommon.RunEaseTimeFloat(0f, degreeAngle), direction);
                    }
                }
                else
                {
                    throw new VTweenException("GameObject/Transform can't be null");
                }
            };

            var t = new EventVRegister { callback = callback, id = 1 };
            var tx = new EventVRegister { callback = LoopReset, id = 3 };
            ivcommon.AddRegister(ref t);
            ivcommon.AddRegister(ref tx);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Repositioning initial position of object.</summary>
        public VTweenRotate setFrom(float fromAngle, Vector3 direction)
        {
            if (transform != null)
            {
                if (!ivcommon.isLocal)
                {
                    transform.rotation = Quaternion.AngleAxis(fromAngle, direction);
                    defaultRotation = transform.rotation;
                }
                else
                {
                    transform.localRotation = Quaternion.AngleAxis(fromAngle, direction);
                    defaultRotation = transform.localRotation;
                }
            }
            return this;
        }
    }

    ///<summary>Interpolates float value.</summary>
    public class VTweenValueFloat : VClass<VTweenValueFloat>
    {
        private float from;
        private float to;
        private Action<float> incallback;
        private float runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(float fromValue, float toValue, float time, Action<float> callbackEvent)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            incallback = callbackEvent;
        }
        ///<summary>Resets properties shuffle the destination</summary>
        private void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                runningValue = from;
            }
            else if (ivcommon.pingpong)
            {
                var dest = to;
                to = from;
                from = dest;
                runningValue = from;
            }
        }
        ///<summary>Main even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            Action callback = () =>
            {
                runningValue = ivcommon.RunEaseTimeFloat(from, to);

                if (incallback != null)
                {
                    incallback.Invoke(runningValue);
                }
            };

            var t = new EventVRegister { callback = callback, id = 1 };
            var tx = new EventVRegister { callback = LoopReset, id = 3 };
            ivcommon.AddRegister(ref t);
            ivcommon.AddRegister(ref tx);
            VTweenManager.InsertToActiveTween(this);
        }

        ///<summary>Repositioning initial position of object.</summary>
        public VTweenValueFloat setFrom(float value)
        {
            from = value;
            return this;
        }
    }
    ///<summary>Interpolates Vector3 value.</summary>
    public class VTweenValueVector3 : VClass<VTweenValueVector3>
    {
        public Vector3 from;
        public Vector3 to;
        public Action<Vector3> incallback;
        public Vector3 runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Vector3 fromValue, Vector3 toValue, float time, Action<Vector3> callbackEvent)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            incallback = callbackEvent;
        }
        ///<summary>Resets properties shuffle the destination</summary>
        private void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                runningValue = from;
            }
            else if (ivcommon.pingpong)
            {
                var dest = to;
                to = from;
                from = dest;
                runningValue = from;
            }
        }
        ///<summary>Ma even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            Action callback = () =>
            {
                runningValue = ivcommon.RunEaseTimeVector3(from, to);

                if (incallback != null)
                {
                    incallback.Invoke(runningValue);
                }
            };

            var t = new EventVRegister { callback = callback, id = 1 };
            var tx = new EventVRegister { callback = LoopReset, id = 3 };
            ivcommon.AddRegister(ref t);
            ivcommon.AddRegister(ref tx);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Repositioning initial position of object.</summary>
        public VTweenValueVector3 setFrom(Vector3 value)
        {
            from = value;
            return this;
        }
    }
    ///<summary>Scales object. Maclass.</summary>
    public class VTweenScale : VClass<VTweenScale>
    {
        public Vector3 defaultScale;
        public Vector3 targetScale;
        public Transform transform;
        public ITransform itransform;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, ITransform itrans, Vector3 destScale, Vector3 defScale, float time)
        {
            transform = trans;
            itransform = itrans;
            defaultScale = defScale;
            targetScale = destScale;
            ivcommon.duration = time;
        }
        ///<summary>Resets properties shuffle the destination</summary>
        private void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                if (transform != null)
                {
                    transform.localScale = defaultScale;
                }
                else if (itransform != null)
                {
                    itransform.scale = defaultScale;
                }
            }
            else
            {
                var dest = targetScale;
                targetScale = defaultScale;
                defaultScale = dest;
            }
        }
        ///<summary>Main even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            Action callback = () =>
            {
                if (transform != null)
                {
                    transform.localScale = ivcommon.RunEaseTimeVector3(defaultScale, targetScale);
                }
                else if (itransform != null)
                {
                    itransform.scale = ivcommon.RunEaseTimeVector3(defaultScale, targetScale);
                }
                else
                {
                    throw new VTweenException("GameObject/Transform can't be null");
                }
            };

            var t = new EventVRegister { callback = callback, id = 1 };
            var tx = new EventVRegister { callback = LoopReset, id = 3 };
            ivcommon.AddRegister(ref t);
            ivcommon.AddRegister(ref tx);
            VTweenManager.InsertToActiveTween(this);
        }

        ///<summary>Repositioning initial position of object.</summary>
        public VTweenScale setFrom(Vector3 fromScale)
        {
            if (transform != null)
            {
                transform.localScale = fromScale;
            }
            else if (itransform != null)
            {
                itransform.scale = fromScale;
            }

            defaultScale = fromScale;
            return this;
        }
    }
    ///<summary>Interpolates shader values.</summary>
    public class VTweenShaderProperty : VClass<VTweenShaderProperty>
    {

    }
    ///<summary>Follows target object. Custom smoothing can be applied.</summary>
    public class VTweenFollow : VClass<VTweenFollow>
    {

    }
    ///<summary>State of the tweening instance.</summary>
    public enum TweenState
    {
        Paused,
        Tweening,
        None
    }
    ///<summary>Base class common properties. Will be declared explicitly the class.</summary>
    public interface IVCommonBase
    {
        public int id { get; set; }
        public int loopAmount { get; set; }
        public int loopCounter { get; set; }
        public int pingpongCounter { get; set; }
        public Ease easeType { get; set; }

        public float duration { get; set; }
        public float runningTime { get; set; }
        public bool speedBased { get; set; }
        public bool oncompleteRepeat { get; set; }
        public bool isLocal { get; set; }
        public bool unscaledTime { get; set; }
        public bool pingpong { get; set; }
        public TweenState state { get; set; }

        public Action exec { get; set; }
        public Action oncomplete { get; set; }
        public Action softreset { get; set; }
        public List<EventVRegister> registers { get; set; }

        public VTweenClass onComplete(Action callback);
        public VTweenClass onUpdate(Action callback);
        public VTweenClass onCompleteRepeat(bool repeatOnComplete);
        public VTweenClass setPingPong(bool state);

        public VTweenClass setEase(Ease ease);
        public VTweenClass setSpeed(float speed);
        public VTweenClass setLoop(int loopCount);
        public void AddRegister(ref EventVRegister register);
        public float RunEaseTimeFloat(float start, float end);
        public Vector3 RunEaseTimeVector3(Vector3 startPos, Vector3 endPos);
        public void Exec();
    }

    ///<summary>Base abstract class.</summary>
    public abstract class VClass<T> : VTweenClass where T : VTweenClass
    {
        ///<summary>Will be called when tweening was completed</summary>
        public T setOnComplete(Action callback)
        {
            this.ivcommon.onComplete(callback);
            return this as T;
        }
        ///<summary>Will be called every frame while tweening.</summary>
        public virtual T setOnUpdate(Action callback)
        {
            this.ivcommon.onUpdate(callback);
            return this as T;
        }
        ///<summary>Easing type.</summary>
        public T setEase(Ease ease)
        {
            this.ivcommon.setEase(ease);
            return this as T;
        }
        ///<summary>Speed based interpolation rather than time-based</summary>
        public T setSpeed(float speed)
        {
            this.ivcommon.setSpeed(speed);
            return this as T;
        }
        ///<summary>Unique id for tween instance.</summary>
        public T id(int id)
        {
            this.ivcommon.id = id;
            return this as T;
        }
        ///<summary>Loop count for each tween instance.</summary>
        public T setLoop(int loopCount)
        {
            this.ivcommon.setLoop(loopCount);
            return this as T;
        }
        ///<summary>Whether it will be affacted by Time.timeScale or not.</summary>
        public T setUnscaledTime(bool state)
        {
            this.ivcommon.unscaledTime = state;
            return this as T;
        }
        ///<summary>Back and forth or pingpong-like interpolation.</summary>
        public T setPingPong(bool state)
        {
            this.ivcommon.setPingPong(state);
            return this as T;
        }
        ///<summary>Back and forth or pingpong-like interpolation.</summary>
        public T setOnCompleteRepeat(bool state)
        {
            this.ivcommon.oncompleteRepeat = state;
            return this as T;
        }
    }
    ///<summary>Wrapper to VTweenClass events/delegates.</summary>
    public struct EventVRegister
    {
        public Action callback;
        public int id;
    }
}
