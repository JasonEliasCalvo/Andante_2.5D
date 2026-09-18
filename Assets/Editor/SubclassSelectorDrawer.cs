using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Dibujar el campo base de Unity
        EditorGUI.PropertyField(position, property, label, true);

        // Superponer un botón desplegable en la barra de cabecera del elemento
        Rect buttonRect = new Rect(
            position.x + EditorGUIUtility.labelWidth,
            position.y,
            position.width - EditorGUIUtility.labelWidth,
            EditorGUIUtility.singleLineHeight
        );

        string fullTypeName = property.managedReferenceFullTypename;
        string displayTypeName = string.IsNullOrEmpty(fullTypeName)
            ? "<Seleccionar Tipo>"
            : fullTypeName.Split(' ').Last().Split('.').Last();

        if (GUI.Button(buttonRect, displayTypeName, EditorStyles.popup))
        {
            Type baseType = GetFieldType(property);
            var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface);

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Null (Ninguno)"), string.IsNullOrEmpty(fullTypeName), () =>
            {
                property.serializedObject.Update();
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            foreach (var type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), fullTypeName.EndsWith(type.Name), () =>
                {
                    property.serializedObject.Update();
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.DropDown(buttonRect);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private Type GetFieldType(SerializedProperty property)
    {
        string[] parts = property.managedReferenceFieldTypename.Split(' ');
        if (parts.Length == 2)
        {
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }
        return typeof(object);
    }
}