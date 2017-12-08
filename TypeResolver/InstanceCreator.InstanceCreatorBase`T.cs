
using System;
using System.Diagnostics;
using System.Reflection;
using Xunit.Extensions;


namespace TypeResolver {

    public static partial class InstanceCreator {

        internal static IInstantiateInstanceData InstanceDataInvoker { get; set; }


        private abstract class InstanceCreatorBase<T> : IInstanceCreator<T> {

            private readonly Func<object[], T> creator_;
            private readonly IInstanceCreator[] arguments_;


            protected InstanceCreatorBase( Func<object[], T> creator, params IInstanceCreator[] arguments ) {
                Debug.Assert( creator != null );

                this.creator_ = creator;
                this.arguments_ = arguments;
            }


            public Type InstanceType {
                get { return typeof( T ); }
            }

            public T CreateInstance( ) {
                object[] argumentInstances = new object[this.arguments_.Length];
                for( int i = 0; i < argumentInstances.Length; ++i )
                    argumentInstances[i] = this.arguments_[i].CreateInstance( );

                var invoker = InstanceDataInvoker;
                T instance = invoker == null
                           ? this.creator_( argumentInstances )
                           : invoker.Invoke( this.creator_, argumentInstances );
                return instance;
            }

            object IInstanceCreator.CreateInstance( ) {
                try {
                    return this.CreateInstance( );
                }
                catch( TargetInvocationException ex ) {
                    return ex.InnerException;
                }
            }


            public sealed override string ToString( ) {
                return this.ToStringCore( );
            }

            protected abstract string ToStringCore( );

        }

    }

}
