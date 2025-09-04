using System;
using UnityEngine;
using BridgeItTogether.Gameplay.Carriles;
using BridgeItTogether.Gameplay.Spawning;

namespace BridgeItTogether.Gameplay.Rondas
{
    /// <summary>
    /// Configuración para una ronda específica de spawneo
    /// </summary>
    [Serializable]
    public class RondaConfig
    {
        [Header("Configuración de la Ronda")]
        public string nombreRonda = "Ronda";
        public int cantidadAutos = 5;
        public float tiempoEntreAutos = 10f;

        [Header("Configuración de Carril (Opcional - sobrescribe configuración global)")]
        public bool sobrescribirTipoCarril = false;
        public TipoCarril tipoCarrilRonda = TipoCarril.DobleCarril;

        [Header("Configuración Individual de Carriles (Solo una dirección)")]
        [Tooltip("Solo aplica cuando tipoCarrilRonda es SoloIzquierda o SoloDerecha. Define el carril (inferior/superior) para cada auto.")]
        public PosicionCarril[] posicionesCarrilPorAuto = new PosicionCarril[0];

        [Header("Configuración Individual de Vehículos")]
        [Tooltip("Define el tipo de vehículo (Auto1..Auto5/Random) para cada auto en esta ronda.")]
        public TipoVehiculo[] tiposVehiculoPorAuto = new TipoVehiculo[0];

        public TipoVehiculo ObtenerTipoVehiculoParaAuto(int indiceAuto)
        {
            var tipoConfigurado = TipoVehiculo.Auto1;
            if (tiposVehiculoPorAuto != null && indiceAuto < tiposVehiculoPorAuto.Length)
                tipoConfigurado = tiposVehiculoPorAuto[indiceAuto];
            if (tipoConfigurado == TipoVehiculo.Random)
            {
                // Elegir uniformemente entre Auto1..Auto5
                int idx = UnityEngine.Random.Range(0, 5);
                return (TipoVehiculo)idx;
            }
            return tipoConfigurado;
        }

        public PosicionCarril ObtenerPosicionCarrilParaAuto(int indiceAuto, PosicionCarril ultimoCarrilUsado)
        {
            var posicionConfigurada = PosicionCarril.Inferior;
            if (posicionesCarrilPorAuto != null && indiceAuto < posicionesCarrilPorAuto.Length)
                posicionConfigurada = posicionesCarrilPorAuto[indiceAuto];

            if (posicionConfigurada == PosicionCarril.Random)
            {
                var carrilSeleccionado = (ultimoCarrilUsado == PosicionCarril.Inferior)
                    ? PosicionCarril.Superior : PosicionCarril.Inferior;
                Debug.Log($"[AutoGenerator] Carril Random para auto {indiceAuto}: Último usado={ultimoCarrilUsado}, Seleccionado={carrilSeleccionado}");
                return carrilSeleccionado;
            }
            return posicionConfigurada;
        }

        public void ValidarConfiguracionCarriles()
        {
            // Posiciones de carril
            if (posicionesCarrilPorAuto == null || posicionesCarrilPorAuto.Length != cantidadAutos)
            {
                var nuevas = new PosicionCarril[cantidadAutos];
                if (posicionesCarrilPorAuto != null)
                {
                    for (int i = 0; i < Mathf.Min(posicionesCarrilPorAuto.Length, cantidadAutos); i++)
                        nuevas[i] = posicionesCarrilPorAuto[i];
                }
                for (int i = (posicionesCarrilPorAuto?.Length ?? 0); i < cantidadAutos; i++)
                    nuevas[i] = (i % 3 == 2) ? PosicionCarril.Random : (i % 2 == 0 ? PosicionCarril.Inferior : PosicionCarril.Superior);
                posicionesCarrilPorAuto = nuevas;
            }

            // Tipos de vehículo
            if (tiposVehiculoPorAuto == null || tiposVehiculoPorAuto.Length != cantidadAutos)
            {
                var nuevas = new TipoVehiculo[cantidadAutos];
                if (tiposVehiculoPorAuto != null)
                {
                    for (int i = 0; i < Mathf.Min(tiposVehiculoPorAuto.Length, cantidadAutos); i++)
                        nuevas[i] = tiposVehiculoPorAuto[i];
                }
                for (int i = (tiposVehiculoPorAuto?.Length ?? 0); i < cantidadAutos; i++)
                {
                    // Semilla por defecto alternando Auto1..Auto5 y Random cada 6
                    int mod = i % 6;
                    nuevas[i] = mod == 5 ? TipoVehiculo.Random : (TipoVehiculo)(mod % 5);
                }
                tiposVehiculoPorAuto = nuevas;
            }
        }
    }
}
