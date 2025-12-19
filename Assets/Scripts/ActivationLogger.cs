using UnityEngine;
using System.Diagnostics;

public class ActivationLogger : MonoBehaviour
{
    private void OnEnable()
    {
        string logMessage = $"[ActivationLogger] {gameObject.name} se activó.";

        // Intentar obtener más información de quién lo activó
        StackTrace stackTrace = new StackTrace(true);
        logMessage += "\nTraza de llamada:\n";

        for (int i = 2; i < Mathf.Min(stackTrace.FrameCount, 10); i++) // Empieza en 2 para evitar este método mismo
        {
            var frame = stackTrace.GetFrame(i);
            logMessage += $"{frame.GetMethod().DeclaringType}.{frame.GetMethod().Name} (línea {frame.GetFileLineNumber()})\n";
        }

        UnityEngine.Debug.Log(logMessage);
    }
}
