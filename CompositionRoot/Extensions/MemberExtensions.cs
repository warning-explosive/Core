namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    public static class MemberExtensions
    {
        private static readonly IMemberExtensions _memberExtensions = DependencyContainer.Resolve<IMemberExtensions>(); 
        
        public static object GetPropertyValue(this object target, string propertyName)
        {
            return _memberExtensions.GetPropertyValue(target, propertyName);
        }

        public static object GetFieldValue(this object target, string fieldName)
        {
            return _memberExtensions.GetFieldValue(target, fieldName);
        }
    }
}