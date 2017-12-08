
using System;
using TypeResolver.Extensions;


namespace TypeResolver {

    public static partial class InstanceCreator {

        private sealed class DelegateCreator<T> : InstanceCreatorBase<T> {

            public DelegateCreator( Func<T> creator )
                : base( ( _ ) => creator( ) ) {
            }


            protected override string ToStringCore( ) {
                return typeof( Func<T> ).GetDescriptiveName( );
            }

        }
    }

}
