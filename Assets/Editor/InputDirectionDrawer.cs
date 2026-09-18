using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputDirection))]
public class InputDirectionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Dibujar la etiqueta de la propiedad
        position = EditorGUI.PrefixLabel(position, label);

        int currentMask = property.intValue;
        string buttonText = GetFormattedText((InputDirection)currentMask);

        // Botón desplegable personalizado
        if (GUI.Button(position, buttonText, EditorStyles.popup))
        {
            GenericMenu menu = new GenericMenu();

            AddMenuItem(menu, property, "None (Neutral)", InputDirection.None);
            AddMenuItem(menu, property, "Forward", InputDirection.Forward);
            AddMenuItem(menu, property, "Back", InputDirection.Back);
            AddMenuItem(menu, property, "Left", InputDirection.Left);
            AddMenuItem(menu, property, "Right", InputDirection.Right);

            menu.AddSeparator("");
            AddSelectAllItem(menu, property, "Any");

            menu.DropDown(position);
        }

        EditorGUI.EndProperty();
    }

    private void AddMenuItem(GenericMenu menu, SerializedProperty property, string name, InputDirection flag)
    {
        int mask = property.intValue;
        int flagValue = (int)flag;
        bool isChecked = (mask & flagValue) != 0;

        menu.AddItem(new GUIContent(name), isChecked, () =>
        {
            if (isChecked)
                property.intValue &= ~flagValue; // Desmarcar bit
            else
                property.intValue |= flagValue;  // Marcar bit

            property.serializedObject.ApplyModifiedProperties();
        });
    }

    private void AddSelectAllItem(GenericMenu menu, SerializedProperty property, string name)
    {
        int allMask = (int)InputDirection.Any;
        bool isAllChecked = (property.intValue & allMask) == allMask;

        menu.AddItem(new GUIContent(name), isAllChecked, () =>
        {
            if (isAllChecked)
                property.intValue = (int)InputDirection.None; // Si se desmarca "Any", regresa a Neutral
            else
                property.intValue = allMask;

            property.serializedObject.ApplyModifiedProperties();
        });
    }

    private string GetFormattedText(InputDirection direction)
    {
        if (direction == InputDirection.Any) return "Any";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (direction.HasFlag(InputDirection.None)) sb.Append("None, ");
        if (direction.HasFlag(InputDirection.Forward)) sb.Append("Forward, ");
        if (direction.HasFlag(InputDirection.Back)) sb.Append("Back, ");
        if (direction.HasFlag(InputDirection.Left)) sb.Append("Left, ");
        if (direction.HasFlag(InputDirection.Right)) sb.Append("Right, ");

        string result = sb.ToString();
        if (result.EndsWith(", "))
            result = result.Substring(0, result.Length - 2);

        return string.IsNullOrEmpty(result) ? "None (Neutral)" : result;
    }
}