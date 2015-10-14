using System;

namespace ProductiveRage.RuleAttributes
{
    // TODO: Note about how it can be used on property getters/setter (though why would you on a setter) but not on the property itself
    // TODO: Note about it only applying directly to methods (not automatically to derived class' methods if attribute on interface method)
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ReturnValueMustNotBeIgnoredAttribute : Attribute { }
}
