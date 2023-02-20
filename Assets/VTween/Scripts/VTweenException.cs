using System;

namespace VTWeen
{
    public class VTweenException : Exception
    {
        public VTweenException() {  }

        public VTweenException(string content): base(String.Format("VTween : {0}", content))
        {
            VTweenManager.AbortVTweenWorker();
        }
    }
}