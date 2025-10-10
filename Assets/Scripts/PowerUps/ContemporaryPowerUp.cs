using UnityEngine;

namespace PowerUps
{
    public class ContemporaryPowerUp : MonoBehaviour
    {
        [Header("Botones a monitorear")]
        [SerializeField] private ContemporaryButtonPowerUp[] botones;
        [Header("Referencia visual para el shader")]
        [SerializeField] private GameObject objetoVisual;
        [Header("Prefab de shader de humo")]
        [SerializeField] private GameObject shaderHumoPrefab;
        [Header("Duración del power up (segundos)")]
        [SerializeField] private float cooldown = 10f;

        private bool _powerUpActivo;
        private float _tiempoRestante;
        private bool _velocidadDuplicada;

        private void Update()
        {
            if (!_powerUpActivo)
            {
                if (TodosBotonesPresionados())
                {
                    ActivarPowerUp();
                }
            }
            else
            {
                _tiempoRestante -= Time.deltaTime;
                if (_tiempoRestante <= 0f)
                {
                    FinalizarPowerUp();
                }
            }
        }

        private bool TodosBotonesPresionados()
        {
            if (botones == null || botones.Length == 0) return false;
            foreach (var boton in botones)
            {
                if (boton == null || !boton.isPressed)
                    return false;
            }
            return true;
        }

        private void ActivarPowerUp()
        {
            _powerUpActivo = true;
            _tiempoRestante = cooldown;
            // Duplicar velocidad de todos los ConveyorBelt
            var cintas = FindObjectsOfType<ConveyorBelt>();
            foreach (var cinta in cintas)
            {
                cinta.velocidad *= 2f;
            }
            _velocidadDuplicada = true;
        }

        private void FinalizarPowerUp()
        {
            // Restaurar velocidad original si lo necesitas (no solicitado, pero recomendable)
            if (_velocidadDuplicada)
            {
                var cintas = FindObjectsOfType<ConveyorBelt>();
                foreach (var cinta in cintas)
                {
                    cinta.velocidad /= 2f;
                }
            }
            // Instanciar shader de humo
            if (objetoVisual != null && shaderHumoPrefab != null)
            {
                Instantiate(shaderHumoPrefab, objetoVisual.transform.position, objetoVisual.transform.rotation);
            }
            
            Destroy(gameObject);
        }
    }
}
