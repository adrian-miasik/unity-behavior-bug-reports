using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Object), true)]
    public class ButtonAttributeDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Fetch target
            Object obj = target;

            // Fetch methods
            MethodInfo[] methods = obj.GetType().GetMethods(BindingFlags.Instance |
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
                        method.Invoke(obj, null);
                    }
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}