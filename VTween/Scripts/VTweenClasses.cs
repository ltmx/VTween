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

///TODO: NOTE: The purpose of local functions replacing delegates here is to avoid creating new objects. It's pretty much free :)
///TODO: The goal here to minimize unnecessary checks as much as we can.
///No checking for position nor final value needed, thus the 0.001 magic number.

using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

//Main VTween classes....
namespace Breadnone.Extension
{
    ///<summary>Base class of VTween. Shares common properties.</summary>
    public class VTweenClass : IVCommonBase
    {
        public int id { get; set; }
        //Hide public members with excplicit methods
        public IVCommonBase ivcommon { get; private set; }
        int IVCommonBase.loopAmount { get; set; }
        int IVCommonBase.loopCounter { get; set; }
        int IVCommonBase.pingpongCounter { get; set; }
        Ease IVCommonBase.easeType { get; set; } = Ease.Linear;
        bool IVCommonBase.isLocal { get; set; }
        bool IVCommonBase.pingpong { get; set; }
        float IVCommonBase.duration { get; set; }
        float IVCommonBase.runningTime { get; set; }
        float? IVCommonBase.delayedTime { get; set; }
        bool IVCommonBase.oncompleteRepeat { get; set; }
        TweenState IVCommonBase.state { get; set; }
        Action IVCommonBase.exec { get; set; }
        Action IVCommonBase.oncomplete { get; set; }
        Action IVCommonBase.runTime { get; set; }
        EventVRegister[] IVCommonBase.registers { get; set; }

        public void renewRegister(bool rent)
        {
            if (rent)
                ivcommon.registers = VTweenManager.RentRegister(VTweenManager.RegisterLength);
            else
            {
                ivcommon.runTime -= ivcommon.ExecRunningTimeScaled;
                ivcommon.runTime -= ivcommon.ExecRunningTimeUnscaled;
                VTweenManager.ReturnRegister(ivcommon.registers);
            }
        }

        ///<summary>Adds/removes invocation of a delegate.</summary>
        void IVCommonBase.AddClearEvent(in EventVRegister register, bool falseClearTrueAdd)
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
        }
        ///<summary>Assigns callback to onComplete, onUpdate, exec.</summary>
        void IVCommonBase.AddRegister(in EventVRegister register)
        {
            for (int i = 0; i < ivcommon.registers.Length; i++)
            {
                if (ivcommon.registers[i].callback == null)
                {
                    ivcommon.registers[i] = register;
                    break;
                }
            }

            ivcommon.AddClearEvent(register, true);
        }
        ///<summary>Registers init.</summary>
        public VTweenClass()
        {
            ivcommon = this;
            ivcommon.runTime = ivcommon.ExecRunningTimeScaled;
            renewRegister(true);
        }
        public virtual void LoopReset() { }

        ///<summary>Executes scaled/unsclade runningTime.</summary>
        void IVCommonBase.ExecRunningTimeScaled() { ivcommon.runningTime += Time.deltaTime; }
        void IVCommonBase.ExecRunningTimeUnscaled() { ivcommon.runningTime += Time.unscaledDeltaTime; }

        ///<summary>Floating point ease impl.</summary>
        float IVCommonBase.RunEaseTimeFloat(in float start, in float end)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = VEasings.ValEase(ivcommon.easeType, start, end, tm);
            ivcommon.runTime.Invoke();
            return res;
        }
        ///<summary>Vector3 ease impl.</summary>
        Vector3 IVCommonBase.RunEaseTimeVector3(in Vector3 startPos, in Vector3 endPos)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = new Vector3(VEasings.ValEase(ivcommon.easeType, startPos.x, endPos.x, tm), VEasings.ValEase(ivcommon.easeType, startPos.y, endPos.y, tm), VEasings.ValEase(ivcommon.easeType, startPos.z, endPos.z, tm));
            ivcommon.runTime.Invoke();
            return res;
        }
        ///<summary>Vector4 ease impl.</summary>
        Vector4 IVCommonBase.RunEaseTimeVector4(in Vector4 startPos, in Vector4 endPos)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = new Vector4(VEasings.ValEase(ivcommon.easeType, startPos.x, endPos.x, tm), VEasings.ValEase(ivcommon.easeType, startPos.y, endPos.y, tm), VEasings.ValEase(ivcommon.easeType, startPos.z, endPos.z, tm), VEasings.ValEase(ivcommon.easeType, startPos.w, endPos.w, tm));
            ivcommon.runTime.Invoke();
            return res;
        }
        ///<summary>Vector2 ease impl</summary>
        Vector2 IVCommonBase.RunEaseTimeVector2(in Vector2 startPos, in Vector2 endPos)
        {
            var tm = ivcommon.runningTime / ivcommon.duration;
            var res = new Vector2(VEasings.ValEase(ivcommon.easeType, startPos.x, endPos.x, tm), VEasings.ValEase(ivcommon.easeType, startPos.y, endPos.y, tm));
            ivcommon.runTime.Invoke();
            return res;
        }
        ///<summary>Amount of loops a single tween.</summary>
        VTweenClass IVCommonBase.setLoop(in int loopCount)
        {
            ivcommon.loopAmount = loopCount;
            return this;
        }

        ///<summary>Back and forth, pingpong-like animation.</summary>
        VTweenClass IVCommonBase.setPingPong(bool state)
        {
            ivcommon.pingpong = state;
            return this;
        }

        ///<summary>Ease function to be used with the tween.</summary>
        VTweenClass IVCommonBase.setEase(in Ease ease)
        {
            ivcommon.easeType = ease;
            return this;
        }
        ///<summary>Execute method chain, will be executed every frame.</summary>
        public void Exec()
        {
            if (ivcommon.state != TweenState.Tweening)
                return;

            if (ivcommon.runningTime + 0.001f > ivcommon.duration)
            {
                CheckIfFinished();
                return;
            }

            ivcommon.exec.Invoke();
        }
        ///<summary>Will be executed at the very end, the next frame the tween completed</summary>
        VTweenClass IVCommonBase.onComplete(Action callback)
        {
            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            return this;
        }
        ///<summary>Callback to execute every frame while tweening.</summary>
        VTweenClass IVCommonBase.onUpdate(Action callback)
        {
            //Keeps running while tweening.
            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            return this;
        }
        ///<summary>Cancels the tween, returns to pool.</summary>
        public void Cancel(bool executeOnComplete = false)
        {
            if (ivcommon.state == TweenState.None)
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
                    LoopReset();

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
                    LoopReset();

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

        ///<summary>Set common properties to default value.</summary>
        public void Clear(bool removeFromActiveList)
        {
            for (int i = 0; i < ivcommon.registers.Length; i++)
            {
                ivcommon.AddClearEvent(ivcommon.registers[i], false);
            }

            if (removeFromActiveList)
            {
                VTweenManager.RemoveFromActiveTween(this);
            }
        }
        ///<summary>Sets to default for re-use purposes.</summary>
        public void DefaultProperties()
        {
            ivcommon.loopAmount = 0;
            ivcommon.loopCounter = 0;
            ivcommon.pingpongCounter = 0;
            ivcommon.easeType = Ease.Linear;
            ivcommon.isLocal = false;
            ivcommon.pingpong = false;
            //ivcommon.duration = 0f;
            ivcommon.runningTime = 0;
            ivcommon.delayedTime = null;
            ivcommon.oncompleteRepeat = false;
            //ivcommon.exec = null;
            //ivcommon.oncomplete = null;
            //ivcommon.softreset = null;
        }
        ///<summary>Checks if tweening.</summary>
        public bool IsTweening() { return ivcommon.state != TweenState.None; }

        ///<summary>Pauses the tweening.</summary>
        public void Pause()
        {
            if (ivcommon.state != TweenState.Tweening)
                return;

            VTweenManager.PoolToPaused(this);
        }
        ///<summary>Resumes paused tween instances, if any.</summary>
        public void Resume()
        {
            if (ivcommon.state != TweenState.Paused)
                return;

            VTweenManager.UnPoolPaused(this);
        }
    }

    ///<summary>Move class. Moves object to target position.</summary>
    public class VTweenMove : VClass<VTweenMove>
    {
        private Vector3 destination;
        private Transform fromIns;
        private IStyle fromInsUi;
        private Vector3 defaultPosition;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, IStyle istyle, Vector3 dest, Vector3 defPos, in float time)
        {
            destination = dest;
            fromIns = trans;
            fromInsUi = istyle;
            defaultPosition = defPos;
            ivcommon.duration = time;

            void callback() { fromIns.position = ivcommon.RunEaseTimeVector3(defaultPosition, destination); }
            void callbackLocal() { fromIns.localPosition = ivcommon.RunEaseTimeVector3(defaultPosition, destination); }

            void callbackUi()
            {
                var tmp = ivcommon.RunEaseTimeVector3(defaultPosition, destination);
                fromInsUi.translate = new Translate(tmp.x, tmp.y, tmp.z);
            }

            if (fromIns is object)
            {
                if (!ivcommon.isLocal)
                    ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
                else
                    ivcommon.AddRegister(new EventVRegister { callback = callbackLocal, id = 1 });
            }
            else
            {
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });
            }

            VTweenManager.InsertToActiveTween(this);
        }


        ///<summary>Shuffling the to/from properties.</summary>
        public override void LoopReset()
        {
            if (fromIns is object)
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
            else if (fromInsUi is object)
            {
                if (!ivcommon.pingpong)
                    fromInsUi.translate = new Translate(defaultPosition.x, defaultPosition.y, defaultPosition.z);
            }

            if (ivcommon.pingpong)
            {
                var dest = destination;
                destination = defaultPosition;
                defaultPosition = dest;
            }
        }
        public void UpdatePos()
        {
            for (int i = 0; i < ivcommon.registers.Length; i++)
            {
                if (ivcommon.registers[i].id == 1)
                {
                    ivcommon.AddClearEvent(ivcommon.registers[i], false);
                    break;
                }
            }

            ivcommon.runningTime = 0;
            VTweenManager.activeTweens.Remove(this);
            SetBaseValues(fromIns, fromInsUi, destination, fromIns.transform.position, ivcommon.duration);
        }
        ///<summary>Repositioning initial position of object.</summary>
        public VTweenMove setFrom(Vector3 fromPosition)
        {
            if (fromIns is object)
            {
                if (!ivcommon.isLocal)
                    fromIns.position = fromPosition;
                else
                    fromIns.localPosition = fromPosition;
            }
            else if (fromInsUi is object)
            {
                fromInsUi.translate = new Translate(fromPosition.x, fromPosition.y, fromPosition.z);
            }

            defaultPosition = fromPosition;
            return this;
        }
        ///<summary>Sets target to look at while moving.</summary>
        public VTweenMove setLookAt(Vector3 targetToLookAt)
        {
            void evt() { fromIns.transform.LookAt(targetToLookAt); }
            ivcommon.AddRegister(new EventVRegister { callback = evt, id = 1 });
            return this;
        }
        ///<summary>Destroys gameObject when completed. VisualElements will be removed from parents hierarchy.</summary>
        public override VTweenMove setDestroy(bool state)
        {
            if (!state)
                return this;

            if (fromIns is object)
            {
                void callback() { GameObject.Destroy(fromIns.gameObject); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }
            else
            {
                void callback() { (fromInsUi as VisualElement).RemoveFromHierarchy(); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }

            return this;
        }
    }

    ///<summary>Rotates object</summary>
    public class VTweenRotate : VClass<VTweenRotate>
    {
        private float degreeAngle;
        private Transform transform;
        private ITransform itransform;
        private Quaternion defaultRotation;
        private Quaternion currentRotation;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, ITransform itrans, in float angle, Vector3 direction, in float time)
        {
            transform = trans;
            degreeAngle = angle;
            ivcommon.duration = time;
            defaultRotation = trans.rotation;
            itransform = itrans;

            if (transform is object)
            {
                if (!ivcommon.isLocal)
                    currentRotation = trans.rotation * Quaternion.AngleAxis(degreeAngle, direction);
                else
                    currentRotation = trans.localRotation * Quaternion.AngleAxis(degreeAngle, direction);
            }
            else if (itransform is object)
            {
                currentRotation = itrans.rotation * Quaternion.AngleAxis(degreeAngle, direction);
            }

            void callback() { trans.rotation = Quaternion.AngleAxis(ivcommon.RunEaseTimeFloat(0f, degreeAngle), direction); }
            void callbackLocal() { trans.localRotation = Quaternion.AngleAxis(ivcommon.RunEaseTimeFloat(0f, degreeAngle), direction); }
            void callbackUi() { itrans.rotation = Quaternion.AngleAxis(ivcommon.RunEaseTimeFloat(0f, degreeAngle), direction); }

            if (trans is object)
            {
                if (!ivcommon.isLocal)
                    ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
                else
                    ivcommon.AddRegister(new EventVRegister { callback = callbackLocal, id = 1 });
            }
            else
            {
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });
            }

            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (transform is object)
            {
                if (!ivcommon.isLocal)
                    transform.transform.rotation = currentRotation;
                else
                    transform.transform.localRotation = currentRotation;
            }
            else if (itransform is object)
            {
                itransform.rotation = currentRotation;
            }

            if (ivcommon.pingpong)
            {
                degreeAngle = -degreeAngle;
            }
        }
        ///<summary>Sets target transform to look at while rotating.</summary>
        public VTweenRotate setLookAt(Transform target, Vector3 direction)
        {
            void Upd()
            {
                if (transform is object)
                {
                    var relativePos = target.position - transform.position;
                    transform.rotation = Quaternion.LookRotation(relativePos, direction);
                }
                else if (itransform is object)
                {
                    var relativePos = target.position - itransform.position;
                    itransform.rotation = Quaternion.LookRotation(relativePos, direction);
                }
            }

            ivcommon.onUpdate(Upd);
            return this;
        }
        ///<summary>Repositioning initial position of object.</summary>
        public VTweenRotate setFrom(float fromAngle, Vector3 direction)
        {
            if (transform is object)
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
            else if (itransform is object)
            {
                defaultRotation = itransform.rotation;
            }

            return this;
        }
        ///<summary>Destroys gameObject/VisualElement on completion.</summary>
        public override VTweenRotate setDestroy(bool state)
        {
            if (!state)
                return this;

            if (transform is object)
            {
                void callback() { GameObject.Destroy(transform.gameObject); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }
            else
            {
                void callback() { (itransform as VisualElement).RemoveFromHierarchy(); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }
            return this;
        }
    }
    ///<summary>Interpolates float value of a shader property.</summary>
    public class VTweenShaderFloat : VClass<VTweenShaderFloat>
    {
        private float from;
        private float to;
        private Material mat;
        private string refName;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Material material, string shaderReferenceName, in float fromValue, in float toValue, in float time)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            mat = material;
            refName = shaderReferenceName;

            if (!material.HasFloat(shaderReferenceName))
            {
                throw new VTweenException("No reference named " + shaderReferenceName + " in the material/shader.");
            }

            void callback()
            {
                material.SetFloat(shaderReferenceName, ivcommon.RunEaseTimeFloat(from, to));
            }

            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                mat.SetFloat(refName, from);
            }
            else
            {
                var dest = to;
                to = from;
                from = dest;
            }
        }
    }
    ///<summary>Interpolates integer value of a shader property.</summary>
    public class VTweenShaderInt : VClass<VTweenShaderInt>
    {
        private int from;
        private int to;
        private Material mat;
        private string refName;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Material material, string shaderReferenceName, in int fromValue, in int toValue, in float time)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            mat = material;
            refName = shaderReferenceName;

            if (!material.HasInt(shaderReferenceName))
            {
                throw new VTweenException("No reference named " + shaderReferenceName + " in the material/shader.");
            }

            void callback()
            {
                material.SetInt(shaderReferenceName, (int)ivcommon.RunEaseTimeFloat(from, to));
            }

            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                mat.SetFloat(refName, from);
            }
            else if (ivcommon.pingpong)
            {
                var dest = to;
                to = from;
                from = dest;
            }
        }
    }
    ///<summary>Interpolates Vector3 value.</summary>
    public class VTweenShaderVector3 : VClass<VTweenShaderVector3>
    {
        private Vector3 from;
        private Vector3 to;
        private Material mat;
        private string refName;

        ///<summary>Sets Vector3 reference in the shader.</summary>
        public void SetBaseValues(Material material, string shaderReferenceName, Vector3 fromValue, Vector3 toValue, in float time)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            mat = material;
            refName = shaderReferenceName;

            if (!material.HasVector(shaderReferenceName))
            {
                throw new VTweenException("No reference named " + shaderReferenceName + " in the material/shader.");
            }

            void callback()
            {
                material.SetVector(shaderReferenceName, ivcommon.RunEaseTimeVector3(from, to));
            }

            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Interpolates Vector3 reference in the shader/material</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                mat.SetVector(refName, from);
            }
            else if (ivcommon.pingpong)
            {
                var dest = to;
                to = from;
                from = dest;
            }
        }
    }
    ///<summary>Interpolates Vector2 value.</summary>
    public class VTweenShaderVector2 : VClass<VTweenShaderVector2>
    {
        private Vector2 from;
        private Vector2 to;
        private Material mat;
        private string refName;

        ///<summary>Sets Vector3 reference in the shader.</summary>
        public void SetBaseValues(Material material, string shaderReferenceName, Vector2 fromValue, Vector2 toValue, in float time)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;
            mat = material;
            refName = shaderReferenceName;

            if (!material.HasVector(shaderReferenceName))
            {
                throw new VTweenException("No reference named " + shaderReferenceName + " in the material/shader.");
            }

            void callback()
            {
                material.SetVector(shaderReferenceName, ivcommon.RunEaseTimeVector2(from, to));
            }

            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Interpolates Vector3 reference in the shader/material</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                mat.SetVector(refName, from);
            }
            else if (ivcommon.pingpong)
            {
                var dest = to;
                to = from;
                from = dest;
            }
        }
    }
    ///<summary>Interpolates float reference in the shader/material.</summary>
    public class VTweenValueFloat : VClass<VTweenValueFloat>
    {
        private float from;
        private float to;
        private float runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(in float fromValue, in float toValue, in float time, Action<float> callbackEvent)
        {
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;

            void callback()
            {
                runningValue = ivcommon.RunEaseTimeFloat(from, to);

                if (callbackEvent is object)
                {
                    callbackEvent.Invoke(runningValue);
                }
            }

            var t = new EventVRegister { callback = callback, id = 1 };
            ivcommon.AddRegister(t);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
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
    }
    ///<summary>Interpolates Vector3 value.</summary>
    public class VTweenValueVector2 : VClass<VTweenValueVector2>
    {
        private Vector2 from;
        private Vector2 to;
        private Vector2 runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Vector2 fromValue, Vector2 toValue, in float time, Action<Vector2> callbackEvent)
        {
            runningValue = Vector2.zero;
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;

            void callback()
            {
                runningValue = ivcommon.RunEaseTimeVector2(from, to);

                if (callbackEvent is object)
                {
                    callbackEvent.Invoke(runningValue);
                }
            }

            var t = new EventVRegister { callback = callback, id = 1 };
            ivcommon.AddRegister(t);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
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
    }
    ///<summary>Interpolates Vector3 value.</summary>
    public class VTweenValueVector3 : VClass<VTweenValueVector3>
    {
        private Vector3 from;
        private Vector3 to;
        private Vector3 runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Vector3 fromValue, Vector3 toValue, in float time, Action<Vector3> callbackEvent)
        {
            runningValue = Vector3.zero;
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;

            void callback()
            {
                runningValue = ivcommon.RunEaseTimeVector3(from, to);

                if (callbackEvent is object)
                {
                    callbackEvent.Invoke(runningValue);
                }
            }

            var t = new EventVRegister { callback = callback, id = 1 };
            ivcommon.AddRegister(t);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
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
    }
    ///<summary>Interpolates Vector4 value.</summary>
    public class VTweenValueVector4 : VClass<VTweenValueVector4>
    {
        private Vector4 from;
        private Vector4 to;
        private Vector4 runningValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Vector4 fromValue, Vector4 toValue, in float time, Action<Vector4> callbackEvent)
        {
            runningValue = Vector4.zero;
            ivcommon.duration = time;
            from = fromValue;
            to = toValue;

            void callback()
            {
                runningValue = ivcommon.RunEaseTimeVector4(from, to);

                if (callbackEvent is object)
                {
                    callbackEvent.Invoke(runningValue);
                }
            }

            var t = new EventVRegister { callback = callback, id = 1 };
            ivcommon.AddRegister(t);
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
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
    }
    ///<summary>Scales object. Maclass.</summary>
    public class VTweenScale : VClass<VTweenScale>
    {
        private Vector3 defaultScale;
        private Vector3 targetScale;
        private Transform transform;
        private IStyle itransform;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, IStyle itrans, Vector3 destScale, Vector3 defScale, in float time)
        {
            transform = trans;
            itransform = itrans;
            defaultScale = defScale;
            targetScale = destScale;
            ivcommon.duration = time;

            void callback() { transform.localScale = ivcommon.RunEaseTimeVector3(defaultScale, targetScale); }
            void callbackUi() { itransform.scale = new Scale(ivcommon.RunEaseTimeVector3(defaultScale, targetScale)); }

            if (transform is object)
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            else
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });

            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                if (transform is object)
                {
                    transform.localScale = defaultScale;
                }
                else if (itransform is object)
                {
                    itransform.scale = new Scale(defaultScale);
                }
            }
            else
            {
                var dest = targetScale;
                targetScale = defaultScale;
                defaultScale = dest;
            }
        }
        ///<summary>Destroys gameObject/VisualElement on completion.</summary>
        public override VTweenScale setDestroy(bool state)
        {
            if (!state)
                return this;

            if (transform is object)
            {
                void callback() { GameObject.Destroy(transform.gameObject); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }
            else
            {
                void callback() { (itransform as VisualElement).RemoveFromHierarchy(); }
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 2 });
            }

            return this;
        }
    }
    ///<summary>Sets alpha value of a Canvas or the opacity of a VisualeElement.</summary>
    public class VTweenAlpha : VClass<VTweenAlpha>
    {
        private CanvasGroup canvyg;
        private VisualElement visualElement;
        private float fromValue;
        private float toValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(CanvasGroup canvas, VisualElement visualelement, float from, float to, in float time)
        {
            canvyg = canvas;
            visualElement = visualelement;
            ivcommon.duration = time;

            if (from < 0)
                from = 0;

            if (to > 1)
                to = 1;

            fromValue = from;
            toValue = to;

            void callback()
            {
                canvyg.alpha = ivcommon.RunEaseTimeFloat(fromValue, toValue);
            }

            void callbackUi()
            {
                visualElement.style.opacity = ivcommon.RunEaseTimeFloat(fromValue, toValue);
            }

            if (canvyg is object)
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            else
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });

            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                if (canvyg is object)
                {
                    canvyg.alpha = fromValue;
                }
                else if (visualElement is object)
                {
                    visualElement.style.opacity = fromValue;
                }
            }
            else
            {
                var dest = toValue;
                toValue = fromValue;
                fromValue = dest;
            }
        }
    }
    ///<summary>Sets alpha value of a Canvas or the opacity of a VisualeElement.</summary>
    public class VTweenColor : VClass<VTweenColor>
    {
        private UnityEngine.UI.Image image;
        private VisualElement visualElement;
        private Color fromValue;
        private Color toValue;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(UnityEngine.UI.Image uiimage, VisualElement visualelement, Color from, Color to, in float time)
        {
            image = uiimage;
            visualElement = visualelement;
            ivcommon.duration = time;
            fromValue = from;
            toValue = to;

            void callback()
            {
                Vector4 vecFrom = fromValue;
                Vector4 vecTo = toValue;
                image.color = ivcommon.RunEaseTimeVector4(fromValue, toValue);
            }

            void callbackUi()
            {
                visualElement.style.backgroundColor = new StyleColor(ivcommon.RunEaseTimeVector4(fromValue, toValue));
            }

            if (image is object)
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            else
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (!ivcommon.pingpong)
            {
                if (image is object)
                {
                    image.color = fromValue;
                }
                else if (visualElement is object)
                {
                    visualElement.style.backgroundColor = fromValue;
                }
            }
            else
            {
                var dest = toValue;
                toValue = fromValue;
                fromValue = dest;
            }
        }
    }
    //TODO Needs more testing!
    ///<summary>Frame-by-frame animation of array of images for both legacy and UIElements.Image.</summary>
    public class VTweenAnimation : VClass<VTweenAnimation>
    {
        private UnityEngine.UI.Image[] images;
        private UnityEngine.UIElements.Image[] uiImages;
        private int fps = 12;
        private int runningIndex;
        private int prevFrame;

        ///<summary>Sets base values that aren't common properties of the base class. Default sets to 12 frame/second. Use setFps for custom frame per second.</summary>
        public void SetBaseValues(UnityEngine.UI.Image[] legacyimages, UnityEngine.UIElements.Image[] uiimages, in int? framePerSecond, in float time)
        {
            ivcommon.duration = time;
            images = legacyimages;
            uiImages = uiimages;

            if (framePerSecond.HasValue)
            {
                fps = framePerSecond.Value;
            }

            ivcommon.duration = time;
        }
        ///<summary>Resets properties shuffle the destination</summary>
        public override void LoopReset()
        {
            if (runningIndex == images.Length)
            {
                if (!ivcommon.pingpong)
                {
                    Array.Reverse(images);
                }
                runningIndex = 0;
            }
        }
        private void SetColor(bool show)
        {
            if (images is object)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null)
                        continue;

                    images[i].gameObject.SetActive(show);
                }
            }
            else if (uiImages is object)
            {
                for (int i = 0; i < uiImages.Length; i++)
                {
                    if (uiImages[i] == null)
                        continue;

                    if (show)
                        uiImages[i].style.display = DisplayStyle.Flex;
                    else
                        uiImages[i].style.display = DisplayStyle.None;
                }
            }
        }
        ///<summary>Main even assignment of Exec method, refers to base class.</summary>
        public void AssignMainEvent()
        {
            SetColor(false);

            void callback()
            {
                if ((prevFrame + fps) > Time.frameCount)
                    return;
                else
                {
                    prevFrame = Time.frameCount;
                }

                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null)
                        continue;

                    if (i == runningIndex)
                    {
                        images[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        images[i].gameObject.SetActive(false);
                    }
                }

                runningIndex++;
            }

            void callbackUi()
            {
                if ((prevFrame + fps) > Time.frameCount)
                    return;
                else
                {
                    prevFrame = Time.frameCount;
                }

                for (int i = 0; i < uiImages.Length; i++)
                {
                    if (i == runningIndex)
                    {
                        uiImages[i].style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        uiImages[i].style.display = DisplayStyle.None;
                    }
                }

                runningIndex++;
            }

            if (images is object)
                ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            else
                ivcommon.AddRegister(new EventVRegister { callback = callbackUi, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
        ///<summary>Sets frame-per-second used for the timing.</summary>
        public VTweenAnimation setFps(int framePerSecond)
        {
            fps = framePerSecond;
            return this;
        }
        public VTweenAnimation setActiveFrame(int index)
        {
            runningIndex = index;
            return this;
        }
        public VTweenAnimation setDisableOnComplete(bool state)
        {
            if (state)
            {
                void act() { SetColor(false); };
                var tx = new EventVRegister { callback = act, id = 2 };
                ivcommon.AddRegister(tx);
            }

            return this;
        }
    }
    ///<summary>Follows target object. Custom dampening can be applied.</summary>
    public class VTweenFollow : VClass<VTweenFollow>
    {
        private Vector3 smoothTime;
        private float speedTime;

        ///<summary>Sets base values that aren't common properties of the base class.</summary>
        public void SetBaseValues(Transform trans, Transform targetTransform, Vector3 smoothness, in float speed)
        {
            smoothTime = smoothness;
            speedTime = speed;

            void callback()
            {
                trans.position = Vector3.SmoothDamp(trans.position, targetTransform.position, ref smoothTime, speedTime);
            }

            ivcommon.AddRegister(new EventVRegister { callback = callback, id = 1 });
            VTweenManager.InsertToActiveTween(this);
        }
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
        public float? delayedTime { get; set; }
        public bool oncompleteRepeat { get; set; }
        public bool isLocal { get; set; }
        public bool pingpong { get; set; }
        public TweenState state { get; set; }
        public Action exec { get; set; }
        public Action oncomplete { get; set; }
        public Action runTime { get; set; }
        public VTweenClass onComplete(Action callback);
        public VTweenClass onUpdate(Action callback);
        public VTweenClass setPingPong(bool state);
        public VTweenClass setEase(in Ease ease);
        public VTweenClass setLoop(in int loopCount);
        public void AddRegister(in EventVRegister register);
        public float RunEaseTimeFloat(in float start, in float end);
        public Vector3 RunEaseTimeVector3(in Vector3 startPos, in Vector3 endPos);
        public Vector4 RunEaseTimeVector4(in Vector4 startPos, in Vector4 endPos);
        public Vector2 RunEaseTimeVector2(in Vector2 startPos, in Vector2 endPos);
        public EventVRegister[] registers { get; set; }
        public void AddClearEvent(in EventVRegister register, bool falseClearTrueAdd);
        public void ExecRunningTimeScaled();
        public void ExecRunningTimeUnscaled();
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
        public T setOnUpdate(Action callback)
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
        ///<summary>Loop count for each tween instance.</summary>
        public T setLoop(in int loopCount)
        {
            this.ivcommon.setLoop(loopCount);
            return this as T;
        }
        ///<summary>Whether it will be affacted by Time.timeScale or not.</summary>
        public T setUnscaledTime(bool state)
        {
            if (state)
                this.ivcommon.runTime = this.ivcommon.ExecRunningTimeUnscaled;
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
        ///<summary>Delays startup execution.</summary>
        public T setDelay(in float delayTime)
        {
            if (delayTime > 0f)
            {
                this.ivcommon.delayedTime = delayTime;
            }

            for (int i = 0; i < ivcommon.registers.Length; i++)
            {
                if (ivcommon.registers[i].id == 1)
                {
                    ivcommon.AddClearEvent(ivcommon.registers[i], false);

                    void delayed()
                    {
                        //Wait for delayed time to be 0.
                        if (ivcommon.delayedTime.HasValue && ivcommon.delayedTime.Value > 0)
                        {
                            ivcommon.delayedTime -= Time.deltaTime;
                            return;
                        }

                        ivcommon.registers[i].callback.Invoke();
                    };

                    ivcommon.AddRegister(new EventVRegister { callback = delayed, id = 1 });
                    break;
                }
            }
            return this as T;
        }
        ///<summary>Destroys gameObject when completed (if it's a visualElement, it will be removed from the hierarchy).</summary>
        public virtual T setDestroy(bool state)
        {
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