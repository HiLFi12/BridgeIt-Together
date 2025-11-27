using System;
using UnityEngine;

namespace Tutorials
{
    // Minimal manager: counter increases when referenced players build successfully; deactivates a wall at threshold
    public class TutorialRepairManager : MonoBehaviour
    {
        [Header("Simple Settings")]
        [Tooltip("Número objetivo de builds necesarios para desactivar la pared")]
        [SerializeField] private int targetCount = 3;

        [Tooltip("Referencias a los jugadores (asignar en el inspector)")]
        [SerializeField] private Player[] players;

        [Tooltip("GameObject que se desactivará cuando se alcance el contador")]
        [SerializeField] private GameObject wall;

        private int _currentCount;

        public int Counter => _currentCount;
        public Player[] Players => players;

        private void Start()
        {
            if (players == null || players.Length == 0) return;

            foreach (var p in players)
            {
                if (p == null) continue;
                var bridge = p.GetComponent<PlayerBridgeInteraction>();
                if (bridge == null) continue;

                bridge.OnRepairResult += OnPlayerBuildResult;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Solo activar la pared si detecta un AutoController
            if (other.GetComponent<BridgeItTogether.Gameplay.AutoControllers.AutoController>() != null)
            {
                wall.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (players == null || players.Length == 0) return;
            foreach (var p in players)
            {
                if (p == null) continue;
                var bridge = p.GetComponent<PlayerBridgeInteraction>();
                if (bridge == null) continue;

                bridge.OnRepairResult -= OnPlayerBuildResult;
            }
        }

        private void OnPlayerBuildResult(bool success)
        {
            if (!success) return;

            _currentCount++;
            Debug.Log($"[TutorialRepairManager] Build successful. Count={_currentCount}/{targetCount}");

            if (_currentCount >= targetCount)
            {
                if (wall != null)
                {
                    wall.SetActive(false);
                    Debug.Log("[TutorialRepairManager] Target reached - wall deactivated.");
                }
                else
                {
                    Debug.LogWarning("[TutorialRepairManager] Target reached but 'wall' is not assigned.");
                }
            }
        }
    }
}
