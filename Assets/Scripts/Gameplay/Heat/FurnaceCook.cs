using UnityEngine;

namespace Gameplay.Heat
{
    public class FurnaceCook : GenericObject2
    {
        [SerializeField] private GameObject heatSphere;

        protected override bool CanStartProcess()
        {
            // Verificar que haya calor activo
            bool hasHeat = heatSphere != null && heatSphere.activeInHierarchy;
            return hasHeat && base.CanStartProcess();
        }

        protected override bool ShouldAutoStart()
        {
            // Permitir auto-inicio si hay calor
            bool hasHeat = heatSphere != null && heatSphere.activeInHierarchy;
            return hasHeat;
        }
    }
}