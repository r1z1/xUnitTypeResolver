
using TypeResolver.Extensions;


namespace TypeResolver {

    public static partial class InstanceCreator {

        private sealed class DefaultConstructorCreator<T> : InstanceCreatorBase<T>
            where T : new( ) {

            public DefaultConstructorCreator( )
                : base( ( _ ) => new T( ) ) {
            }


            protected override string ToStringCore( ) {
                return "new " + typeof( T ).GetDescriptiveName( ) + "( )";
            }

        }
    }

}
