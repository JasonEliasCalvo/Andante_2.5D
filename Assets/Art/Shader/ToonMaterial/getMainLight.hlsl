#ifndef MAINLIGHT_INCLUDE
#define MAINLIGHT_INCLUDE


void GetMainLightData_float(out float3 Direction, out float3 Color)
{
#ifdef SHADERGRAPH_PREVIEW
    // Valores "falsos" para que el nodo funcione en la vista previa.
    // Si falta esta asignación, te dará el error "not initialized".
    Direction = float3(0.5, 0.5, 0.0);
    Color = float3(1.0, 1.0, 1.0);
#else
    // Obtenemos la luz real del Universal Render Pipeline (URP).
    Light mainLight = GetMainLight();
    
    // Aquí asignamos los valores de la luz real a nuestras variables de salida.
    Direction = mainLight.direction;
    Color = mainLight.color;
#endif
}
#endif