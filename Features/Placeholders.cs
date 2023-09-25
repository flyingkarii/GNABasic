using System.Text.RegularExpressions;

namespace BattleBitAPI.Features
{
    public class Placeholders
    {
        
        private readonly Regex re = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
        public string text { get; set; }

        public Dictionary<string, object> parameters = new Dictionary<string, object>();

        public Placeholders(string text)
        {
            this.text = text;
        }

        public Placeholders AddParam(string key, object value)
        {
            if (key == null || value == null)
            {
                return this;
            }

            parameters.Add(key, value);
            return this;
        }

        public string Run()
        {
            text = re.Replace(text, delegate (Match match) {
                if (parameters.ContainsKey(match.Groups[1].Value))
                    return parameters[match.Groups[1].Value].ToString();
                return "";
            });

            return text;
        }
    }
}
