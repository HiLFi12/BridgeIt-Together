using UnityEngine;

namespace Gameplay.Heat
{
    [RequireComponent(typeof(Collider))]
    public class FurnaceCook : GenericObject2, ITurnable
    {
        public bool isTurned { get; private set; }

        // Permite que la base dispare la mezcla automáticamente cuando haya calor.
        // Mantiene el comportamiento por defecto (Prehistórica/Medieval) y lo amplía.
        protected override bool ShouldAutoStart()
        {
            // Si tu GenericObject2 no tiene este hook, omite este override.
            bool result = base.ShouldAutoStart() || isTurned;
            Debug.Log($"FurnaceCook - ShouldAutoStart: {result} (isTurned={isTurned})");
            return result;
        }

        // Exige calor y, además, que las condiciones de la base estén OK (slots llenos, etc.)
        protected override bool CanStartProcess()
        {
            bool result = isTurned && base.CanStartProcess();
            Debug.Log($"FurnaceCook - CanStartProcess: {result} (isTurned={isTurned})");
            return result;
        }

        public void TurnOn()
        {
            isTurned = true;
        }

        public void TurnOff()
        {
            isTurned = false;
        }

        private void OnDisable()
        {
            isTurned = false;
        }
    }
}