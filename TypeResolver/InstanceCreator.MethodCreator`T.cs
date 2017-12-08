
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Extensions;


namespace TypeResolver {

    public static partial class InstanceCreator {

        private class MethodCreator<T> : InstanceCreatorBase<T> {

            private readonly MethodInfo method_;


            public MethodCreator( MethodInfo method, params IInstanceCreator[] arguments )
                : base( ( args ) => (T)method.Invoke( null, args ), arguments ) {
                Debug.Assert( method != null );
                Debug.Assert( method.IsStatic );
                Debug.Assert( method.ReturnType.Is<T>( ) );

                Debug.Assert( arguments != null );
                Debug.Assert( arguments.All( ( arg ) => arg != null ) );

                var parameters = method.GetParameters( );
                Debug.Assert( arguments.Length == parameters.Length );
                Debug.Assert( Enumerable.Range( 0, parameters.Length ).All( ( i ) => arguments[i].InstanceType.Is( parameters[i].ParameterType ) ) );


                this.method_ = method;
            }


            protected override string ToStringCore( ) {
                return this.method_.GetDescriptiveName( );
            }

        }

    }

}
