using System;
using System.Collections.Generic;
using UnityEngine;

public enum DownloadMode
{
    Online,
    Offline,
    Hybrid
}

public enum AspectRatios
{
    _3x4,
    _2x3,
    _10x16,
    Default
}

[Serializable]
public class QuestionFormat
{
    public string Question;
    public string Explanation;
    public string Image;
    public bool isToF;
    public bool ToFAnswer;
    public AnswerFormat[] Answers = new AnswerFormat[0];
    public bool isUsed = false;
}

[Serializable]
public class AnswerFormat
{
    public string Text;
    public bool Correct;
}

public struct QuestionsContainer
{
    public List<QuestionFormat> Questions;

    public QuestionsContainer(List<QuestionFormat> data)
    {
        Questions = data;
    }
}

[Serializable]
public class CategoryFormat
{
    public bool Enabled;
    public string CategoryName;
    public Sprite CategoryImage;
    public DownloadMode Mode;
    public string OnlinePath;
    public TextAsset OfflineFile;
    public bool ShuffleQuestions;
    public bool ShuffleAnswers;
    public bool LimitQuestions;
    public int QuestionLimit;
    public bool CustomTimerAmount;
    public int TimerAmount;
    public bool CustomLivesAmount;
    public int LivesCount;
    public LanguageList Language;
    public string HighscorePref;

    public CategoryFormat(LanguageList Lang = 0)
    {
        CategoryName = "New";
        OnlinePath = HighscorePref = string.Empty;
        OfflineFile = null;
        Mode = DownloadMode.Online;
        Language = Lang;
        ShuffleQuestions = ShuffleAnswers = Enabled = true;
        LimitQuestions = CustomTimerAmount = CustomLivesAmount = false;
        QuestionLimit = TimerAmount = LivesCount = 0;
        CategoryImage = null;
    }

    public CategoryFormat(CategoryFormat format)
    {
        Enabled = format.Enabled;
        CategoryName = format.CategoryName;
        CategoryImage = format.CategoryImage;
        Mode = format.Mode;
        OnlinePath = format.OnlinePath;
        OfflineFile = format.OfflineFile;
        ShuffleQuestions = format.ShuffleQuestions;
        ShuffleAnswers = format.ShuffleAnswers;
        LimitQuestions = format.LimitQuestions;
        QuestionLimit = format.QuestionLimit;
        CustomTimerAmount = format.CustomTimerAmount;
        TimerAmount = format.TimerAmount;
        CustomLivesAmount = format.CustomLivesAmount;
        LivesCount = format.LivesCount;
        Language = format.Language;
        HighscorePref = format.HighscorePref;
    }
}

public struct BackupContainer
{
    public List<CategoryFormat> Categories;

    public BackupContainer(List<CategoryFormat> data)
    {
        Categories = data;
    }
}

[Serializable]
public class LocalizationFormat
{
    public string Key;
    public string Value;

    public LocalizationFormat(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public struct LocalizationContainer
{
    public List<LocalizationFormat> Locale;

    public LocalizationContainer(List<LocalizationFormat> data)
    {
        Locale = data;
    }
}