using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.Spawning
{
    /// <summary>
    /// Implementación de IPlatformProvider que resuelve plataformas por tag y cachea resultados.
    /// Cumple SRP y permite reemplazar la fuente de plataformas (DIP).
    /// </summary>
    public class PlatformProviderByTag : MonoBehaviour, IPlatformProvider
    {
        [SerializeField] private string platformTag = "Platform";
        private Transform izquierda;
        private Transform derecha;

        public bool TryGetPlatforms(out Transform outIzquierda, out Transform outDerecha)
        {
            if (izquierda == null || derecha == null)
            {
                var plataformas = GameObject.FindGameObjectsWithTag(platformTag);
                if (plataformas.Length >= 2)
                {
                    System.Array.Sort(plataformas, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
                    izquierda = plataformas[0].transform;
                    derecha = plataformas[plataformas.Length - 1].transform;
                }
            }
            outIzquierda = izquierda;
            outDerecha = derecha;
            return izquierda != null && derecha != null;
        }
    }
}
