using Core.Health;
using Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Wave
{
    [RequireComponent(typeof(WaveSpawnManager))]
    public class WaveManager: Singleton<WaveManager>
    {
        public WaveIntro intro;

        /// <summary>
		/// The currency that the player starts with
		/// </summary>
		public int startingCurrency;

        public HomeBase[] homeBases;

        public Collider[] environmentColliders;

        /// <summary>
		/// The attached wave spawn manager
		/// </summary>
        public WaveSpawnManager waveSpawnManager { get; protected set; }

        /// <summary>
		/// Number of enemies currently in the Wave
		/// </summary>
		public int numberOfEnemies { get; protected set; }

        /// <summary>
        /// The current state of the Wave
        /// </summary>
        public WaveState waveState { get; protected set; }

        /// <summary>
		/// Number of home bases left
		/// </summary>
		public int numberOfHomeBasesLeft { get; protected set; }

        /// <summary>
        /// Starting number of home bases
        /// </summary>
        public int numberOfHomeBases { get; protected set; }

        /// <summary>
        /// An accessor for the home bases
        /// </summary>
        public HomeBase[] HomeBases
        {
            get { return homeBases; }
        }

        public bool isGameOver
        {
            get { return (waveState == WaveState.Win) || (waveState == WaveState.Lose); }
        }

        /// <summary>
		/// Fired when all the waves are done and there are no more enemies left
		/// </summary>
        public event Action waveCompleted;

        /// <summary>
		/// Fired when all of the home bases are destroyed
		/// </summary>
        public event Action waveFailed;

        /// <summary>
		/// Fired when the wave state is changed - first parameter is the old state, second parameter is the new state
		/// </summary>
		public event Action<WaveState, WaveState> waveStateChanged;

        /// <summary>
        /// Fired when the number of enemies has changed
        /// </summary>
        public event Action<int> numberOfEnemiesChanged;

        /// <summary>
        /// Event for home base being destroyed
        /// </summary>
        public event Action homeBaseDestroyed;


        /// <summary>
        /// Increments the number of enemies. Called on enemy spawn
        /// </summary>
        public virtual void IncrementNumberOfEnemies()
        {
            numberOfEnemies++;
            SafelyCallNumberOfEnemiesChanged();
        }

        /// <summary>
        /// Returns the sum of all HomeBases' health
        /// </summary>
        public float GetAllHomeBasesHealth()
        {
            float health = 0.0f;
            foreach (HomeBase homebase in homeBases)
            {
                health += homebase.configuration.currentHealth;
            }
            return health;
        }

        /// <summary>
        /// Decrements the number of enemies. Called on Agent death
        /// </summary>
        public virtual void DecrementNumberOfEnemies()
        {
            numberOfEnemies--;
            SafelyCallNumberOfEnemiesChanged();
            if (numberOfEnemies < 0)
            {
                Debug.LogError("[WAVE] There should never be a negative number of enemies. Something broke!");
                numberOfEnemies = 0;
            }

            if (numberOfEnemies == 0 && waveState == WaveState.AllEnemiesSpawned)
            {
                ChangeWaveState(WaveState.Win);
            }
        }

        /// <summary>
        /// Completes building phase, setting state to spawn enemies
        /// </summary>
        public virtual void BuildingCompleted()
        {
            ChangeWaveState(WaveState.SpawnEnemy);
        }

        protected override void Awake()
        {
            base.Awake();
            waveSpawnManager = GetComponent<WaveSpawnManager>();
            waveSpawnManager.spawningCompleted += OnSpawningCompleted;

            // Does not use the change state function as we don't need to broadcast the event for this default value
            waveState = WaveState.Intro;
            numberOfEnemies = 0;

            // If there's an intro use it, otherwise fall through to gameplay
            if (intro != null)
            {
                intro.introCompleted += IntroCompleted;
            }
            else
            {
                IntroCompleted();
            }

            // Iterate through home bases and subscribe
            numberOfHomeBases = homeBases.Length;
            numberOfHomeBasesLeft = numberOfHomeBases;
            for (int i = 0; i < numberOfHomeBases; i++)
            {
                homeBases[i].died += OnHomeBaseDestroyed;
            }
        }

        /// <summary>
		/// Unsubscribes from events
		/// </summary>
		protected override void OnDestroy()
        {
            base.OnDestroy();
            if (waveSpawnManager != null)
            {
                waveSpawnManager.spawningCompleted -= OnSpawningCompleted;
            }
            if (intro != null)
            {
                intro.introCompleted -= IntroCompleted;
            }

            // Iterate through home bases and unsubscribe
            for (int i = 0; i < numberOfHomeBases; i++)
            {
                homeBases[i].died -= OnHomeBaseDestroyed;
            }
        }

        /// <summary>
        /// Fired when Intro is completed or immediately, if no intro is specified
        /// </summary>
        protected virtual void IntroCompleted()
        {
            ChangeWaveState(WaveState.Building);
        }

        /// <summary>
        /// Fired when the WaveManager has finished spawning enemies
        /// </summary>
        protected virtual void OnSpawningCompleted()
        {
            ChangeWaveState(WaveState.AllEnemiesSpawned);
        }

        /// <summary>
        /// Changes the state and broadcasts the event
        /// </summary>
        /// <param name="newState">The new state to transitioned to</param>
        protected virtual void ChangeWaveState(WaveState newState)
        {
            // If the state hasn't changed then return
            if (waveState == newState)
            {
                return;
            }

            WaveState oldState = waveState;
            waveState = newState;
            if (waveStateChanged != null)
            {
                waveStateChanged(oldState, newState);
            }

            switch (newState)
            {
                case WaveState.SpawnEnemy:
                    waveSpawnManager.StartWaves();
                    break;
                case WaveState.AllEnemiesSpawned:
                    // Win immediately if all enemies are already dead
                    if (numberOfEnemies == 0)
                    {
                        ChangeWaveState(WaveState.Win);
                    }
                    break;
                case WaveState.Lose:
                    SafelyCallWaveFailed();
                    break;
                case WaveState.Win:
                    SafelyCallWaveCompleted();
                    break;
            }
        }

        /// <summary>
        /// Fired when a home base is destroyed
        /// </summary>
        protected virtual void OnHomeBaseDestroyed(DamageableBehaviour homeBase)
        {
            // Decrement the number of home bases
            numberOfHomeBasesLeft--;

            // Call the destroyed event
            if (homeBaseDestroyed != null)
            {
                homeBaseDestroyed();
            }

            // If there are no home bases left and the wave is not over then set the wave to lost
            if ((numberOfHomeBasesLeft == 0) && !isGameOver)
            {
                ChangeWaveState(WaveState.Lose);
            }
        }

        /// <summary>
        /// Calls the <see cref="waveCompleted"/> event
        /// </summary>
        protected virtual void SafelyCallWaveCompleted()
        {
            if (waveCompleted != null)
            {
                waveCompleted();
            }
        }

        /// <summary>
        /// Calls the <see cref="numberOfEnemiesChanged"/> event
        /// </summary>
        protected virtual void SafelyCallNumberOfEnemiesChanged()
        {
            if (numberOfEnemiesChanged != null)
            {
                numberOfEnemiesChanged(numberOfEnemies);
            }
        }

        /// <summary>
        /// Calls the <see cref="waveFailed"/> event
        /// </summary>
        protected virtual void SafelyCallWaveFailed()
        {
            if (waveFailed != null)
            {
                waveFailed();
            }
        }
    }
}