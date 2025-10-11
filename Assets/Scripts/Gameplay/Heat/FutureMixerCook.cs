using UnityEngine;
using System.Collections;

namespace Gameplay.Heat
{
    public class FutureMixerCook : GenericObject2
    {
        [SerializeField] private BatterySystem batterySystem;
        [SerializeField] private WorkingSpawner workingSpawner;
        [SerializeField] private float tiempoCoccionFuturista = 1.5f;
        
        private bool procesandoMaterial = false;

        protected override bool CanStartProcess()
        {
            // Verificar que la batería esté cargada (isCharged = true)
            bool hasCharge = batterySystem != null && batterySystem.IsCharged;
            
            if (!hasCharge)
            {
                Debug.Log("La mezcladora necesita energía. Carga la batería primero.");
            }
            
            return hasCharge && base.CanStartProcess();
        }

        protected override bool ShouldAutoStart()
        {
            // NO auto-iniciar aquí, lo manejamos en nuestro Update
            return false;
        }

        protected override void Update()
        {
            // NO llamar a base.Update() para evitar que GenericObject2 inicie el proceso automáticamente
            
            // Implementar nuestra propia lógica de auto-inicio
            if (!procesandoMaterial && CanStartProcess())
            {
                // Usar reflexión para verificar si ambos slots están ocupados
                var slot1Field = typeof(GenericObject2).GetField("slotTipo1Ocupado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var slot2Field = typeof(GenericObject2).GetField("slotTipo2Ocupado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (slot1Field != null && slot2Field != null)
                {
                    bool slot1 = (bool)slot1Field.GetValue(this);
                    bool slot2 = (bool)slot2Field.GetValue(this);
                    
                    if (slot1 && slot2)
                    {
                        Debug.Log("Auto-arrancando proceso de mezcla futurista.");
                        StartCoroutine(ProcesarMaterialFuturista());
                    }
                }
            }
        }

        private IEnumerator ProcesarMaterialFuturista()
        {
            procesandoMaterial = true;
            
            // Marcar en proceso en GenericObject2 usando reflexión
            var procesoField = typeof(GenericObject2).GetField("enProcesoCoccion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (procesoField != null) procesoField.SetValue(this, true);
            
            Debug.Log("Comenzando proceso de mezcla futurista...");
            
            yield return new WaitForSeconds(tiempoCoccionFuturista);
            
            // En lugar de instanciar el objeto, activar el WorkingSpawner
            if (workingSpawner != null)
            {
                workingSpawner.ActivateSpawner();
                Debug.Log("Material tipo 3 listo en el spawner.");
            }
            else
            {
                Debug.LogError("No se ha asignado el WorkingSpawner en FutureMixerCook.");
            }
            
            // Limpiar los slots usando reflexión
            var slot1Field = typeof(GenericObject2).GetField("slotTipo1Ocupado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var slot2Field = typeof(GenericObject2).GetField("slotTipo2Ocupado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (slot1Field != null) slot1Field.SetValue(this, false);
            if (slot2Field != null) slot2Field.SetValue(this, false);
            if (procesoField != null) procesoField.SetValue(this, false);
            
            // Actualizar visuales
            var method = typeof(GenericObject2).GetMethod("ActualizarVisualesSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(this, null);
            
            procesandoMaterial = false;
        }
    }
}
