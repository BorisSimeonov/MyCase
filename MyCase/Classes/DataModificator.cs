using System.Text.RegularExpressions;
namespace MyCase.Classes
{
    public static class DataModificator
    {
        public static string TextCleaner(string inputText)
        {
            inputText = inputText.Trim();
            inputText = Regex.Replace(inputText, @"[\n\t]", " ");
            inputText = Regex.Replace(inputText, @"\s{2,}", " ");
            return inputText;
        }
    }
}
