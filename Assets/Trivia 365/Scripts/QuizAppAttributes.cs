using System;
using UnityEngine;

namespace QuizApp.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class QAHeaderAttribute : PropertyAttribute
    {
        public readonly string header;

        public QAHeaderAttribute(string header)
        {
            this.header = header;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class QASeparatorAttribute : PropertyAttribute
    {
        public readonly string text;

        public QASeparatorAttribute(string text = null)
        {
            this.text = text;
        }
    }
}