using System;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace PostClient.Helpers.Converters
{
    public sealed class RemoveUnnecessaryCharsFromStringAccountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var text = value as string;
                if (text != null)
                {
                    if (text.Contains("Service") && !text.Contains("@"))
                    {
                        value = text.Replace("Service", "");
                    }
                    else if (text.Contains("@"))
                    {
                        var tempValue = string.Empty;

                        foreach (var letter in text.TakeWhile(c => c != '@'))
                        {
                            tempValue += letter;
                        }

                        value = tempValue;
                    }
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
