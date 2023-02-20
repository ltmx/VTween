using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using VTWeen;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

public class VStressTests : MonoBehaviour
{
    [SerializeField] private TMP_Text labelCounter;
    [SerializeField] private List<GameObject> xTopObjects = new List<GameObject>();
    [SerializeField] private Transform parent;
    [SerializeField] private List<GameObject> xBottomObjects = new List<GameObject>();
    [SerializeField] private int loopCount = 20;
    [SerializeField] private double waitTime = 1;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private TMP_Text btnText;
    [SerializeField] private bool pingPong;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private int spawnCounter = 0;
    private bool cancelled = false;
    public async void StartStressTesting()
    {
        bool exited = false;
        
        while (!cancelled)
        {
            for (int i = 0; i < xTopObjects.Count; i++)
            {
                if (cts.IsCancellationRequested)
                {
                    exited = true;
                    break;
                }

                await Task.Yield();
                var go = Instantiate(xTopObjects[i], xTopObjects[i].transform.position, xTopObjects[i].transform.rotation);
                go.transform.SetParent(parent, true);
                go.GetComponent<Image>().color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                VTween.move(go, xBottomObjects[UnityEngine.Random.Range(0, xBottomObjects.Count - 1)].transform, UnityEngine.Random.Range(1f, 3f)).setLoop(loopCount).setEase(ease).setPingPong(pingPong).setOnComplete(() =>
                    {
                        Debug.Log(go.name + " was DONE -->");
                    });

                spawnCounter++;
                labelCounter.SetText("TOTAL TWEENS : " + spawnCounter);

                for (int j = 0; j < xBottomObjects.Count; j++)
                {
                    if (cts.IsCancellationRequested)
                    {
                        exited = true;
                        break;
                    }
      
                    await Task.Yield();
                    var ggo = Instantiate(xBottomObjects[j], xBottomObjects[j].transform.position, xBottomObjects[j].transform.rotation);
                    ggo.transform.SetParent(parent, true);
                    ggo.GetComponent<Image>().color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    VTween.move(ggo, xTopObjects[UnityEngine.Random.Range(0, xTopObjects.Count - 1)].transform, UnityEngine.Random.Range(1.5f, 3f)).setLoop(loopCount).setEase(ease).setPingPong(pingPong).setOnComplete(() =>
                    {
                        Debug.Log(ggo.name + " was DONE -->");
                    });
                    spawnCounter++;
                    labelCounter.SetText("TOTAL TWEENS : " + spawnCounter);
                }

                if(exited)
                    break;
            }
            
            if (cts.IsCancellationRequested)
            {
                exited = true;
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(waitTime), cts.Token);

            if(exited)
                break;
        }

        VTween.CancelAll();
        
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            cancelled = true;
        }
    }

    public void Cancel()
    {
        cancelled = true;
        UnityEngine.Debug.Log("HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH" + VTweenManager.activeTweens.Count);
    }
    void OnDisable()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            cancelled = true;
        }
    }
}