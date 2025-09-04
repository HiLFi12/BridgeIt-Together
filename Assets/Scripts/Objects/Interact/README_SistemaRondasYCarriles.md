# Sistema de Rondas y Configuración de Carriles - AutoGenerator

## 📋 Resumen de Cambios

El sistema `AutoGenerator` ha sido extendido con dos nuevas funcionalidades principales:

### 1. 🔄 Sistema de Rondas
- Permite configurar múltiples rondas con diferentes parámetros
- Cada ronda puede tener diferente cantidad de autos, velocidad y tiempo entre spawns
- Control automático de progreso entre rondas
- Opción de loop infinito o finalización al completar todas las rondas

### 2. 🛣️ Configuración de Tipos de Carril
- **Doble Carril**: Spawneo desde ambos lados (comportamiento original)
- **Solo Izquierda**: Únicamente spawneo desde el lado izquierdo
- **Solo Derecha**: Únicamente spawneo desde el lado derecho
- Cada ronda puede sobrescribir el tipo de carril globalmente configurado

---

## 🎛️ Configuración en el Inspector

### Configuración de Carriles
- **Tipo Carril**: Selecciona entre DobleCarril, SoloIzquierda, o SoloDerecha
- **Separación Carriles**: Distancia entre carriles en modo doble carril

### Configuración de Rondas
- **Usar Sistema Rondas**: Habilita/deshabilita el sistema de rondas
- **Configuración Rondas**: Array de configuraciones para cada ronda
- **Loopear Rondas**: Si repetir las rondas al terminar

### Configuración por Ronda
Cada ronda incluye:
- **Nombre Ronda**: Identificador descriptivo
- **Cantidad Autos**: Cuántos autos spawnear en esta ronda
- **Tiempo Entre Autos**: Intervalo entre spawns (segundos)
- **Velocidad Autos**: Velocidad de movimiento para esta ronda
- **Sobrescribir Tipo Carril**: Si usar un tipo específico para esta ronda
- **Tipo Carril Ronda**: Tipo de carril específico para esta ronda

---

## 🎮 Métodos de Control (Context Menu)

### Control de Rondas
- **🔄 Iniciar Sistema Rondas**: Inicia manualmente el sistema de rondas
- **⏹️ Detener Sistema Rondas**: Detiene las rondas y vuelve al modo continuo
- **⏭️ Forzar Siguiente Ronda**: Salta inmediatamente a la siguiente ronda
- **ℹ️ Debug Sistema Rondas**: Muestra información detallada del estado actual

### Control General
- **Debug Sistema Spawn**: Información sobre la configuración actual de spawn

---

## 💻 API Pública

### Control de Rondas
```csharp
// Iniciar/detener sistema de rondas
void IniciarSistemaRondas()
void DetenerSistemaRondas()
void ForzarSiguienteRonda()

// Configuración dinámica
void ConfigurarRondas(RondaConfig[] nuevasRondas, bool iniciarInmediatamente = false)
void SetSistemaRondasActivo(bool activo)

// Información del estado
string GetInfoRondaActual()
bool IsUsandoSistemaRondas()
int GetRondaActual()
int GetTotalRondas()
```

### Configuración de Rondas por Código
```csharp
// Ejemplo de configuración de rondas
RondaConfig[] misRondas = new RondaConfig[]
{
    new RondaConfig 
    { 
        nombreRonda = "Fácil", 
        cantidadAutos = 3, 
        tiempoEntreAutos = 8f, 
        velocidadAutos = 3f 
    },
    new RondaConfig 
    { 
        nombreRonda = "Difícil", 
        cantidadAutos = 10, 
        tiempoEntreAutos = 2f, 
        velocidadAutos = 8f,
        sobrescribirTipoCarril = true,
        tipoCarrilRonda = TipoCarril.SoloIzquierda
    }
};

autoGenerator.ConfigurarRondas(misRondas, true);
```

---

## 🔧 Funcionamiento Interno

### Sistema de Rondas
1. Se spawnea la cantidad especificada de autos para la ronda actual
2. El sistema espera a que todos los autos vuelvan al pool (via triggers)
3. Automáticamente avanza a la siguiente ronda
4. Si está habilitado el loop, reinicia desde la primera ronda al terminar

### Tipos de Carril
- **DobleCarril**: Alterna entre spawns izquierdo y derecho
- **SoloIzquierda**: Siempre spawea desde el punto izquierdo hacia la derecha
- **SoloDerecha**: Siempre spawea desde el punto derecho hacia la izquierda

### Integración con Triggers
- El sistema se integra automáticamente con los triggers de retorno
- Cuando un auto toca un trigger y vuelve al pool, se notifica al sistema de rondas
- No requiere configuración adicional de triggers

---

## 🎯 Casos de Uso

### Juego por Niveles
```csharp
// Configurar rondas progresivamente más difíciles
RondaConfig[] niveles = new RondaConfig[]
{
    new RondaConfig { nombreRonda = "Nivel 1", cantidadAutos = 3, tiempoEntreAutos = 10f, velocidadAutos = 2f },
    new RondaConfig { nombreRonda = "Nivel 2", cantidadAutos = 5, tiempoEntreAutos = 8f, velocidadAutos = 4f },
    new RondaConfig { nombreRonda = "Nivel 3", cantidadAutos = 8, tiempoEntreAutos = 5f, velocidadAutos = 6f },
    new RondaConfig { nombreRonda = "Jefe Final", cantidadAutos = 15, tiempoEntreAutos = 1f, velocidadAutos = 10f, 
                     sobrescribirTipoCarril = true, tipoCarrilRonda = TipoCarril.SoloIzquierda }
};
```

### Diferentes Configuraciones de Tráfico
```csharp
// Cambiar dinámicamente el tipo de carril
autoGenerator.tipoCarril = TipoCarril.SoloIzquierda; // Solo tráfico de izquierda a derecha
autoGenerator.tipoCarril = TipoCarril.SoloDerecha;   // Solo tráfico de derecha a izquierda
autoGenerator.tipoCarril = TipoCarril.DobleCarril;   // Tráfico bidireccional
```

---

## ⚠️ Consideraciones Importantes

1. **Triggers Requeridos**: El sistema de rondas requiere triggers configurados para detectar cuándo los autos salen del área de juego
2. **Compatibilidad**: Mantiene 100% compatibilidad con el sistema anterior
3. **Performance**: No hay impacto significativo en el rendimiento
4. **Configuración por Defecto**: Viene con 3 rondas de ejemplo pre-configuradas

---

## 🐛 Debugging

### Logs del Sistema
El sistema proporciona logs detallados:
- Inicio y fin de cada ronda
- Progreso de spawning
- Estado de retorno de vehículos
- Información de configuración

### Comandos de Debug
- Use los métodos de Context Menu para inspeccionar el estado
- `GetInfoRondaActual()` proporciona información en tiempo real
- Los Gizmos visuales muestran el tipo de carril activo

### Troubleshooting
- Si las rondas no avanzan: Verificar que los triggers estén configurados
- Si los autos no spawean: Revisar la configuración de la ronda actual
- Para forzar el avance: Usar "Forzar Siguiente Ronda" en el Context Menu
