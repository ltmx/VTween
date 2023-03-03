using UnityEngine;

namespace Breadnone.Extension
{
    public static class Utilv
    {
        public static void SwapRefs<T>(ref T source, ref T target) where T : struct
        {
            var a = source;
            var b = target;
            source = b;
            target = a;
        }
        public static float FloatLerp(Ease ease, ref float start, ref float end, float time)
        {
            var res = VEasings.ValEase(ease, start, end, time);
            return res;
        }
        public static Vector3 Vec3Lerp(Ease ease, ref Vector3 start, ref Vector3 end, float time)
        {
            var res = VEasings.ValEase(ease, start, end, time);
            return res;
        }
        public static Vector2 Vec2Lerp(Ease ease, ref Vector2 start, ref Vector2 end, float time)
        {
            Vector3 from = new Vector2(start.x, start.y);
            Vector3 to = new Vector2(end.x, end.y);
            var res = VEasings.ValEase(ease, from, to, time);
            return res;
        }
        public static Vector4 Vec4Lerp(Ease easeType, ref Vector4 startPos, ref Vector4 endPos, float time)
        {
            var res = new Vector4(VEasings.ValEase(easeType, startPos.x, endPos.x, time), VEasings.ValEase(easeType, startPos.y, endPos.y, time), VEasings.ValEase(easeType, startPos.z, endPos.z, time), VEasings.ValEase(easeType, startPos.w, endPos.w, time));
            return res;
        }
    }

}