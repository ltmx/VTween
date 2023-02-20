using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VTWeen
{
    public enum VectorDirection
    {
        Vector3Up,
        Vector3Down,
        Vector3Left,
        Vector3Right,
        Vector3Forward,
        Vector3Back
    }
    public class VTweenTests : MonoBehaviour
    {
        [SerializeField] private int loopCount = 0;
        [SerializeField] private bool pingPong = false;
        [SerializeField] private TMP_Text textVal;
        [SerializeField] private Vector3 floatVecJoinTest = new Vector3(90, 90, 0);
        [SerializeField] private GameObject obj;
        [SerializeField] private GameObject ThreeDObject;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private float rotationInVec3 = 90;
        [SerializeField] private VectorDirection direction = VectorDirection.Vector3Forward;
        [SerializeField] private Canvas parent;
        [SerializeField] private int instanceAmount = 5;
        [SerializeField] private int distanceAmount = 5;
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool enableStopwatch;
        [SerializeField] private Ease easeTest = Ease.Linear;
        [SerializeField] private Transform target;
        [SerializeField] private Transform fromTarget;
        private List<GameObject> objs = new List<GameObject>();
        private int defaultDistance;
        private Vector3 defaultPos;
        private Vector3 defaultScale;
        private Quaternion defaultRot;
        void Start()
        {
            defaultDistance = distanceAmount;
            defaultPos = obj.transform.position;
            defaultRot = ThreeDObject.transform.rotation;
            defaultScale = obj.transform.localScale;
        }

        public void TestMoveSingle()
        {
            if(obj != null)
            {
                var t = new Stopwatch();

                if(enableStopwatch)
                t.Start();

                obj.transform.SetParent(parent.transform, false);
                VTween.move(obj, obj.transform.position * 2f, duration).setOnComplete(()=>
                {
                    if(enableStopwatch)
                    {
                        UnityEngine.Debug.Log(t.Elapsed.TotalSeconds);
                        t.Stop();
                    }
                }).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }
        public void TestMoveToTarget()
        {
            if(obj != null)
            {
                obj.transform.position = defaultPos;
                var t = new Stopwatch();

                if(enableStopwatch)
                t.Start();

                obj.transform.SetParent(parent.transform, false);
                VTween.move(obj, target, duration).setOnComplete(()=>
                {
                    if(enableStopwatch)
                    {
                        UnityEngine.Debug.Log(t.Elapsed.TotalSeconds);
                        t.Stop();
                    }

                    UnityEngine.Debug.Log("TEST OnCOmplete REPEAT!");
                }).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong).setOnCompleteRepeat(true);
            }
        }
        public void TestRotate()
        {
            if(obj != null)
            {
                Vector3 vecDir = Vector3.left;

                if(direction == VectorDirection.Vector3Up)
                {
                    vecDir = Vector3.up;
                }
                else if(direction == VectorDirection.Vector3Down)
                {
                    vecDir = Vector3.down;
                }
                else if(direction == VectorDirection.Vector3Left)
                {
                    vecDir = Vector3.left;
                }
                else if(direction == VectorDirection.Vector3Right)
                {
                    vecDir = Vector3.right;
                }
                else if(direction == VectorDirection.Vector3Forward)
                {
                    vecDir = Vector3.forward;
                }
                else if(direction == VectorDirection.Vector3Back)
                {
                    vecDir = Vector3.back;
                }

                var t = new Stopwatch();

                if(enableStopwatch)
                    t.Start();

                VTween.rotate(ThreeDObject, rotationInVec3, vecDir, duration).setOnComplete(()=>
                {
                    if(enableStopwatch)
                    {
                        UnityEngine.Debug.Log(t.Elapsed.TotalSeconds);
                        t.Stop();
                    }
                }).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }
        public void TestScale()
        {
            if(obj != null)
            {
                obj.transform.localScale = defaultScale;
                var t = new Stopwatch();

                if(enableStopwatch)
                t.Start();

                obj.transform.SetParent(parent.transform, false);

                VTween.scale(obj, obj.transform.localScale*2, duration).setOnComplete(()=>
                {
                    if(enableStopwatch)
                    {
                        UnityEngine.Debug.Log(t.Elapsed.TotalSeconds);
                        t.Stop();
                    }
                }).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }
        public void TestMoveFromTarget()
        {
            if(obj != null)
            {
                obj.transform.position = defaultPos;
                var t = new Stopwatch();

                if(enableStopwatch)
                t.Start();

                obj.transform.SetParent(parent.transform, false);
                VTween.move(obj, target, duration).setFrom(fromTarget.position).setOnComplete(()=>
                {
                    if(enableStopwatch)
                    {
                        UnityEngine.Debug.Log(t.Elapsed.TotalSeconds);
                        t.Stop();
                    }
                }).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }

        public void GetActiveTweenCount()
        {
            textVal.SetText(VTween.ActiveTweenCount.ToString());
            UnityEngine.Debug.Log(textVal.text);
        }
        public void GetPausedTweenCount()
        {
            textVal.SetText(VTween.PausedTweenCount.ToString());
            UnityEngine.Debug.Log(textVal.text);
        }
        public void TestMoveArray()
        {
            if(obj == null)
                return;

            DestroyInstances();

            for(int i = 0; i < instanceAmount; i++)
            {
                if(i != 0)
                {
                    distanceAmount *= 3;
                }

                var tr = obj.transform.position;
                var go = Instantiate(obj, obj.transform.position, Quaternion.identity);
                go.name = i.ToString();
                go.transform.SetParent(parent.transform, false);
                objs.Add(go);
                go.transform.position = new Vector3(tr.x, tr.y + (float)distanceAmount, tr.z);
                
                VTween.move(go, new Vector3(tr.x * 8, tr.y, tr.z), duration).setOnComplete(() =>
                {
                    UnityEngine.Debug.Log(go.name + " _was moved! Called from onComplete event trigger!");
                }).setOnUpdate(()=>
                {
                    int t = 0;
                    UnityEngine.Debug.Log("OnUpdate"  + t++);
                }).setLoop(loopCount).setPingPong(pingPong);
            }
        }
        private void DestroyInstances()
        {
            if(objs != null && objs.Count > 0)
            {
                for(int i = 0; i < objs.Count; i++)
                {
                    if(objs[i] == null)
                        continue;

                    Destroy(objs[i]);
                }
            }

            objs = new List<GameObject>();
            distanceAmount = defaultDistance;
        }
        public void TestValueFloat()
        {
            if(textVal != null)
            {
                textVal.SetText(string.Empty);

                VTween.value(floatVecJoinTest.x, floatVecJoinTest.y, duration, new System.Action<float>(x =>
                {
                    textVal.SetText("Float value test : " + x.ToString("0.00"));

                })).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }

        public void TestValueVector()
        {
            if(textVal != null)
            {
                textVal.SetText(string.Empty);

                var vec = floatVecJoinTest * 2;

                VTween.value(floatVecJoinTest, vec, duration, new System.Action<Vector3>(x =>
                {
                    textVal.SetText("Vector3 value test : " + x.ToString("X = " + x.x + " Y = " + x.y + " Z = " + x.z));

                })).setEase(easeTest).setLoop(loopCount).setPingPong(pingPong);
            }
        }
        public void TestExecLater()
        {
            VTween.execLater(5, ()=> {UnityEngine.Debug.Log("Done waiting!");});
        }
        public void Cancel()
        {
            VTween.CancelAll();
        }
        public void Pause()
        {
            VTween.PauseAll();
        }
        public void Resume()
        {
            VTween.ResumeAll();
        }
    }
}