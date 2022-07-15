using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EasyAnimation))]
public class EasyAnimationEditor : Editor
{
    static class Styles
    {
        public static GUIContent animation = new GUIContent("Animation", "The clip that will be played if Play() is called, or if \"Play Automatically\" is enabled");
        public static GUIContent animations = new GUIContent("Animations", "These clips will define the States the component will start with");
        public static GUIContent playAutomatically = new GUIContent("Play Automatically", "If checked, the default clip will automatically be played");
        public static GUIContent animatePhysics = new GUIContent("Animate Physics", "If checked, animations will be updated at the same frequency as Fixed Update");

        public static GUIContent cullingMode = new GUIContent("Culling Mode", "Controls what is updated when the object has been culled");
    }

    SerializedProperty _states;
    SerializedProperty _playAutomatically;
    SerializedProperty _animatePhysics;
    SerializedProperty _cullingMode;

    void OnEnable()
    {
        _states = serializedObject.FindProperty("_states");
        _playAutomatically = serializedObject.FindProperty("_playAutomatically");
        _animatePhysics = serializedObject.FindProperty("_animatePhysics");
        _cullingMode = serializedObject.FindProperty("_cullingMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_states, Styles.animations, true);
        EditorGUILayout.PropertyField(_playAutomatically, Styles.playAutomatically);
        EditorGUILayout.PropertyField(_animatePhysics, Styles.animatePhysics);
        EditorGUILayout.PropertyField(_cullingMode, Styles.cullingMode);


        serializedObject.ApplyModifiedProperties();
    }


}

[CustomPropertyDrawer(typeof(EasyAnimation.EditorState))]
class StateDrawer : PropertyDrawer
{
    class Styles
    {
        public static readonly GUIContent disabledTooltip = new GUIContent("", "The Default state cannot be edited, change the Animation clip to change the Default State");
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUILayout.BeginHorizontal();
        // Calculate rects
        var width = position.width / 3;
        Rect clipRect = new Rect(position.x, position.y, width - 5, position.height);
        Rect nameRect = new Rect(position.x + width + 5, position.y, width - 5, position.height);
        Rect wrapRect = new Rect(position.x + width * 2 + 5, position.y, width - 5, position.height);

        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(clipRect, property.FindPropertyRelative("clip"), GUIContent.none);
        EditorGUI.PropertyField(wrapRect, property.FindPropertyRelative("wrapMode"), GUIContent.none);

        EditorGUILayout.EndHorizontal();
        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}