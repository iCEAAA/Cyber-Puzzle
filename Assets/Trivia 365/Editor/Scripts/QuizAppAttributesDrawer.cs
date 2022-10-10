using UnityEditor;
using UnityEngine;
using QuizApp.Attributes;
using QuizApp.Editor;

[CustomPropertyDrawer(typeof(QAHeaderAttribute))]
public sealed class QAHeaderAttributeDrawer : DecoratorDrawer
{
    QAHeaderAttribute headerAttribute { get { return ((QAHeaderAttribute)attribute); } }

    public override void OnGUI(Rect position)
    {
        position.y += 5;

        EditorGUI.LabelField(position, headerAttribute.header, CustomStyles.headerAttribute);
    }

    public override float GetHeight()
    {
        return 33.0f;
    }
}

[CustomPropertyDrawer(typeof(QASeparatorAttribute))]
public sealed class QASeparatorDrawer : DecoratorDrawer
{
    QASeparatorAttribute separatorAttribute { get { return ((QASeparatorAttribute)attribute); } }

    public override void OnGUI(Rect position)
    {
        if (string.IsNullOrEmpty(separatorAttribute.text))
        {
            position.height = 1;
            position.y += 15;
            GUI.Box(position, "");
        }
        else
        {
            position.y += 15;

            Vector2 textSize = EditorStyles.boldLabel.CalcSize(new GUIContent(separatorAttribute.text));
            float separatorWidth = (position.width - textSize.x) / 2f - 5f;

            GUI.Box(new Rect(position.xMin, position.yMin, separatorWidth, 1), "");
            EditorGUI.LabelField(new Rect(position.xMin + separatorWidth + 5.0f, position.yMin - 8.0f, textSize.x, 20), separatorAttribute.text, EditorStyles.boldLabel);
            GUI.Box(new Rect(position.xMin + separatorWidth + 10.0f + textSize.x, position.yMin, separatorWidth, 1), "");
        }
    }

    public override float GetHeight()
    {
        return 35f;
    }
}