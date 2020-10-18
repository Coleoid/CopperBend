using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace CopperBend.Creation
{
    public class YamlVisitor_OmitEmptyCollections : ChainedObjectGraphVisitor
    {
        public YamlVisitor_OmitEmptyCollections(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        { }

        public override bool Enter(IObjectDescriptor value, IEmitter context)
        {
            // if it's not a collection, serialize as normal
            if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(value.Value.GetType()))
                return base.Enter(value, context);

            // if it's a collection with entries, serialize as normal
            var enumerableObject = (System.Collections.IEnumerable)value.Value;
            if (enumerableObject.GetEnumerator().MoveNext())
                return base.Enter(value, context);

            // otherwise, it's an empty collection, don't serialize it
            return false;
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            if (value.Value == null) return false;

            // if it's not a collection, serialize as normal
            if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(value.Value.GetType()))
            return base.EnterMapping(key, value, context);

            // if it's a collection with entries, serialize as normal
            var enumerableObject = (System.Collections.IEnumerable)value.Value;
            if (enumerableObject.GetEnumerator().MoveNext())
                return base.EnterMapping(key, value, context);

            // otherwise, it's an empty collection, don't serialize it
            return false;
        }
    }
}
