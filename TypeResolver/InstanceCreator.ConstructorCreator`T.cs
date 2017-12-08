
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Extensions;


namespace TypeResolver {

    public static partial class InstanceCreator {

        private class ConstructorCreator<T> : InstanceCreatorBase<T> {

            private readonly ConstructorInfo constructor_;


            public ConstructorCreator( ConstructorInfo constructor, params IInstanceCreator[] arguments )
                : base( ( args ) => (T)constructor.Invoke( args ), arguments ) {
                Debug.Assert( constructor != null );
                Debug.Assert( constructor.DeclaringType.Is<T>( ) );

                Debug.Assert( arguments != null );
                Debug.Assert( arguments.All( ( arg ) => arg != null ) );

                var parameters = constructor.GetParameters( );
                Debug.Assert( arguments.Length == parameters.Length );
                Debug.Assert( Enumerable.Range( 0, parameters.Length ).All( ( i ) => arguments[i].InstanceType.Is( parameters[i].ParameterType ) ) );


                this.constructor_ = constructor;
            }


            protected override string ToStringCore( ) {
                return this.constructor_.GetDescriptiveName( );
            }

        }

    }

}
