namespace GNA.Core.CommandSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Alias : Attribute
    {
        public string alias { get; set; }

        public Alias(string alias)
        {
            this.alias = alias;
        }
    }
}
