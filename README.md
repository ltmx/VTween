# VTween
 A compact tweening library for Unity3D with only just 30kb of size. Inspired by the legendary LeanTween.  
 
 <br>**Requirement**</br>
 Unity3D 2022.2.x and above.  
 
 <br>**Installation**</br>
Download the .zip and unpack it to your Assets folder in your project.  

 <br>**UIToolkit ~Experimental**</br>
 UIToolkit should work as long as you're using Unity editor 2022.2.x and above due to style translate api.  

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
                
                //Color
                var legacyImage = gameObject.GetComponent<Image>();
                VTween.color(legacyImage, new Color(0.2f, 0.1f, 0.2f, 1), 5);
                
                //Follow
                var target = someTarget.GetComponent<Transform>();
                VTween.follow(gameObject, target, new Vector3(0f, 0f, 0.1f), 5f);
                
                //ShaderProperty //Lerps or interpolates values (apis : shaderFloat, shaderVector2, shaderVector3)
                var myMaterials = gameObject.GetComponent<Renderer>().materials;
                VTween.shaderFloat(myMaterials[0], "_myFloatRef", 0, 2, 5); 
                
                //Value //Interpolates float value(supported types: float, Vector2, Vector3, Vector4)
                VTween.value(0f, 5f, 3f, (x)=> {Debug.Log("running value : " + x)});
                
```
 
 **ToDo:**  
 - CustomYieldInstruction. Currently uses it's own timing. This the highest priority!  
 - Port more LeanTween apis.    
 - Support for UIToolkit is on the way. 

https://user-images.githubusercontent.com/64100867/220118744-85f4dee1-a35b-4772-ae41-83688e9b810a.mp4

