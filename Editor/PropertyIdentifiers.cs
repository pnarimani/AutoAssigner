namespace AutoAssigner
{
    public class PropertyIdentifiers
    {
        public string ObjectName;
        public string ObjectType;
        public string PropertyName;

        public PropertyIdentifiers()
        {
        }

        public PropertyIdentifiers(string objectName, string objectType, string propertyName)
        {
            ObjectName = objectName;
            ObjectType = objectType;
            PropertyName = propertyName;
        }
    }
}