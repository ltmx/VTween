# VTween
 Fully featured tweening library for Unity3D with only just 30kb of size. Inspired by the legendary LeanTween.  
 <br>**Why another tweening library?**</br>
 Well, LeanTween isn't maintained anymore and it doesn't work with UIToolkit(still the best tweening library in my heart!)  
 
 <br>**Installation**</br>
Download the .zip and unpack it to your Assets folder in your project.

 <br>Syntax</br>
```
                //Move
                VTween.move(obj, target, duration).setOnComplete(()=>
                {
                    UnityEngine.Debug.Log("Was completed!");
                    
                }).setEase(Ease.Linear).setLoop(3).setPingPong(true).setOnCompleteRepeat(true);

                //Rotate
                VTween.rotate(ThreeDObject, rotationInVec3, Vector3.forward, duration).setEase(Ease.Linear).setLoop(2).setPingPong(true);
                
                //Scale
                VTween.scale(obj, new Vector3(2, 2, 2), duration).setEase(Ease.Linear).setLoop(3);

                //ExecuteLater (Similar to LeanTween.delayedCall)
                VTween.execLater(5, ()=> {UnityEngine.Debug.Log("Done waiting!");});
                
                //Frame-by-frame animation(VTween.animation)
                Image[] arr = new Image[11];
                VTween.animation(arr, duration, 60).setDisableOnComplete(true).setLoop(loopCount).setPingPong(true);
                
                //Alpha
                VTween.alpha(canvasGroup, 0f, 1f, 5f); //for legacy UI
                //OR
                VTween.alpha(visualElement, 0f, 1f, 5f); //for UIToolkit
                
```
 
 **ToDo:**  
 - CustomYieldInstruction. Currently uses it's own timing. This the highest priority!  
 - Port more LeanTween apis.    
 - Support for UIToolkit is on the way. 

https://user-images.githubusercontent.com/64100867/220118744-85f4dee1-a35b-4772-ae41-83688e9b810a.mp4

