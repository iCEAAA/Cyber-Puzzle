//This file is auto-generated from the editor. DO NOT MODIFY.

using System.Collections.Generic;

public enum LanguageList
{
	English
}

[System.Serializable]
public class LocaleFormat
{
	public string Key;
	public List<string> Languages = new List<string>();

	public static LocaleFormat FromCsv(string Line)
	{
		string[] values = Line.Split(',');
		LocaleFormat value = new LocaleFormat();
		value.Key = values[0];

		for (int x = 1; x < values.Length; x++)
			value.Languages.Add(values[x]);

		return value;
	}
}

public struct LocaleContainer
{
	public List<LocaleFormat> Locale;

	public LocaleContainer(List<LocaleFormat> data) { Locale = data; }
}