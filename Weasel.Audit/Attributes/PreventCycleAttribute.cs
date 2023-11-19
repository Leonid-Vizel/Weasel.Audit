namespace Weasel.Audit.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PreventCycleAttribute : Attribute { }
