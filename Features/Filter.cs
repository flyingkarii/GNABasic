namespace BattleBitAPI.Features
{
    public class Filter
    {
        private static List<string> words = new List<string>()
        { "faggot", "faggots", "fag", "fags", "retard", "tranny", "nigger", "nig", "nigs", "dyke", "troon", "retards", "niggers", "kneegers", "kneeger", "dykes", "trannys", "trannies" };

        public static bool IsFiltered(string text)
        {
            string[] split = text.Split(" ");
            string[] found = split.Where((w) => words.Contains(w.ToLower())).ToArray();

            if (found.Length > 0)
                return true;

            return false;
        }
    }
}
