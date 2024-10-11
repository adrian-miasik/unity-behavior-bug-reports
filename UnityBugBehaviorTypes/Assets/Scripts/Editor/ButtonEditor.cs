using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Fetch target
            MonoBehaviour mono = (MonoBehaviour)target;

            // Fetch methods
            MethodInfo[] methods = mono.GetType().GetMethods(BindingFlags.Instance |
                                                             BindingFlags.Static | 
                                                             BindingFlags.Public |
                                                             BindingFlags.NonPublic);

            // Iterate through each method...
            foreach (MethodInfo method in methods)
            {
                // Fetch button attribute
                Button buttonAttribute = (Button)System.Attribute.GetCustomAttribute(method, typeof(Button));

                // Verify if button attribute exists...
                if (buttonAttribute != null)
                {
                    // Determine button name
                    string buttonName = string.IsNullOrEmpty(buttonAttribute.GetButtonName()) 
                        ? method.Name : buttonAttribute.GetButtonName();

                    // Render button
                    if (GUILayout.Button(buttonName))
                    {
                        // Invoke method on clicks
                        method.Invoke(mono, null);
                    }
                }
            }
        }
    }
}