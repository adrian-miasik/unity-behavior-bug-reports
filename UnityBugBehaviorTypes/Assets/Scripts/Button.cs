using System;

[AttributeUsage(AttributeTargets.Method)]
public class Button : Attribute
{
    private readonly string buttonName;

    // Constructor
    public Button(string name = "")
    {
        buttonName = name;
    }
        
    // Getter
    public string GetButtonName()
    {
        return buttonName;
    }
}