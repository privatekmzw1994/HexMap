using System;

namespace Core.Utilities
{
    /// <summary>
    /// 重复计时器
    /// </summary>
    public class RepeatingTimer : Timer
    {
        public RepeatingTimer(float time, Action onElapsed = null):base(time,onElapsed)
        {
        }

        public override bool Tick(float deltaTime)
        {
            if (AssessTime(deltaTime))
            {
                Reset();
            }

            return false;
        }
    }
}