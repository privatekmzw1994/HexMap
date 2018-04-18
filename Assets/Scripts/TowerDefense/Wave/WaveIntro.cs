using System;
using UnityEngine;

namespace TowerDefense.Wave
{
    public abstract class WaveIntro : MonoBehaviour
    {
        public event Action introCompleted;

        protected void safelyCallIntroCompleted()
        {
            if (introCompleted!=null)
            {
                introCompleted();
            }
        }
    }
}
