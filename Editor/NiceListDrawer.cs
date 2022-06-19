using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace sapra.HelperMethods
{ 
    public abstract class NiceListDrawer : PropertyDrawer
    {
        ReorderableList currentList;
        protected abstract string ListName{get;}
        protected abstract string parameterName{get;}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.IndentedRect(position);
            position.y += EditorGUIUtility.standardVerticalSpacing*2;
            EditorGUI.BeginChangeCheck();
            if(currentList == null)
                MakeList(property);
            currentList.DoList(position);
            if (EditorGUI.EndChangeCheck()) 
                property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
        void MakeList(SerializedProperty property)
        {
            var list = property.FindPropertyRelative(parameterName);
            currentList = new ReorderableList(property.serializedObject, list, false, true, true, true);
            currentList.drawHeaderCallback = (Rect rect) => {EditorGUI.LabelField(rect, ListName);};
            currentList.elementHeightCallback = (int index) => {
                if(index > list.arraySize-1)
                    return EditorGUIUtility.singleLineHeight;
                else
                    return EditorGUI.GetPropertyHeight(list.GetArrayElementAtIndex(index));
                };
            currentList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                rect = new Rect(rect.x, rect.y+4, rect.width, rect.height);
                EditorGUI.PropertyField(rect, list.GetArrayElementAtIndex(index));
                };
            currentList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>{
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
                AddCallback(element, property);
                property.serializedObject.ApplyModifiedProperties();
            };
            currentList.onRemoveCallback = (ReorderableList l) =>{
                RemoveCallback(l);
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
                property.serializedObject.ApplyModifiedProperties();
            };
        }
        protected abstract void AddCallback(SerializedProperty newElement, SerializedProperty property);
        protected virtual void RemoveCallback(ReorderableList l){}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(currentList != null)
                return currentList.GetHeight();
            else
                return base.GetPropertyHeight(property, label);
        }
    }
}