namespace GNA.Core.CommandSystem
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Permission : Attribute
    {
        public DefaultGroup defaultAccess;
        public string permission;
        
        public Permission(string permission)
        {
            this.permission = permission;
        }

        public Permission(DefaultGroup defaultAccess)
        {
            this.defaultAccess = defaultAccess;
        }
    }
}
