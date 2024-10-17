namespace Slash.Public.Common.Configurations;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigAttribute(string sectionName) : Attribute
{
    public string SectionName { get; } = sectionName;
}
