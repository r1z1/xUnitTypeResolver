
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeResolver.Extensions;


namespace TypeResolver {

    public static partial class TypeCreator {

        private static class WeakInstanceCreator {

            private static readonly MethodInfo DefaultConstructorInstantiatorMethod = typeof( InstanceCreator ).GetMethod( "ForType", Type.EmptyTypes );

            private static readonly MethodInfo DelegateInstantiatorMethod = typeof( InstanceCreator ).GetMethod( "ForDelegate" );

            private static readonly MethodInfo MethodInstantiatorMethod = typeof( InstanceCreator ).GetMethod( "ForMethod", new[] { typeof( MethodBase ), typeof( IInstanceCreator[] ) } );


            public static IEnumerable<IInstanceCreator> ForEnum( Type enumType ) {
                Debug.Assert( enumType != null );
                Debug.Assert( enumType.IsEnum || enumType == typeof( bool ) );

                var concreteMethod = DelegateInstantiatorMethod.MakeGenericMethod( enumType );
                var delegateType = typeof( Func<> ).MakeGenericType( enumType );

                Array values = GetEnumValues( enumType );
                foreach( var value in values ) {
                    var lambda = Expression.Lambda(
                        delegateType,
                        Expression.Constant( value, enumType )
                    );
                    var func = lambda.Compile( );
                    var creator = concreteMethod.Invoke( null, new object[] { func } );

                    yield return (IInstanceCreator)creator;
                }
            }

            public static IInstanceCreator ForType( Type type ) {
                Debug.Assert( type != null );
                Debug.Assert( DefaultConstructorInstantiatorMethod != null );

                var concreteMethod = DefaultConstructorInstantiatorMethod.MakeGenericMethod( type );
                var creator = concreteMethod.Invoke( null, null );

                return (IInstanceCreator)creator;
            }

            public static IInstanceCreator ForDelegate( object func ) {
                Debug.Assert( func != null );
                Debug.Assert( func.GetType( ).GetGenericTypeDefinition( ) == typeof( Func<> ) );
                Debug.Assert( DelegateInstantiatorMethod != null );

                var type = func.GetType( ).GetGenericArguments( )[0];
                var concreteMethod = DelegateInstantiatorMethod.MakeGenericMethod( type );
                var creator = concreteMethod.Invoke( null, new[] { func } );

                return (IInstanceCreator)creator;
            }

            public static IInstanceCreator ForMethod( Type type, MethodBase method, IInstanceCreator[] arguments ) {
                Debug.Assert( type != null );
                Debug.Assert( method != null );
                Debug.Assert( arguments != null );
                Debug.Assert( !method.IsGenericMethodDefinition );
                Debug.Assert( method.GetParameters( ).Length == arguments.Length );
                Debug.Assert( MethodInstantiatorMethod != null );

                var concreteMethod = MethodInstantiatorMethod.MakeGenericMethod( type );
                var creator = concreteMethod.Invoke( null, new object[] { method, arguments } );

                return (IInstanceCreator)creator;
            }


            public static IInstanceCreator ForInstanceCreator( Type targetType, IInstanceCreator creator ) {
                Debug.Assert( creator != null );
                Debug.Assert( targetType != null );
                Debug.Assert( targetType.IsGenericType && targetType.GetGenericTypeDefinition( ) == typeof( IInstanceCreator<> ) );

                IInstanceCreator finalCreator = creator;
                Type creatorType = creator.GetType( );
                Type targetCreatedType = targetType.GetGenericArguments( )[0];

                // Ensure target type is closed.
                Type finalTargetType = targetType;
                if( finalTargetType.ContainsGenericParameters ) {
                    targetCreatedType = GenericTypeResolver.GetCorrespondingBaseTypes( finalCreator.InstanceType, targetCreatedType ).Single( );
                    finalTargetType = typeof( IInstanceCreator<> ).MakeGenericType( targetCreatedType );
                }

                // If the creator does not have the same type as the target, wrap the creator to return instances of the target base type.
                if( !creatorType.Is( finalTargetType ) ) {
                    Type actualCreatedType = creatorType.GetInterfaces( )
                        .Single( i => i.IsGenericType && i.GetGenericTypeDefinition( ) == typeof( IInstanceCreator<> ) )
                        .GetGenericArguments( )[0];

                    Type wrapperType = typeof( InstanceCreatorWrapper<,> ).MakeGenericType( targetCreatedType, actualCreatedType );
                    finalCreator = (IInstanceCreator)Activator.CreateInstance( wrapperType, creator );
                }

                // Get IInstanceCreator to return the given creator instance.
                Type funcType = typeof( Func<> ).MakeGenericType( finalTargetType );
                var lambda = Expression.Lambda( funcType, Expression.Constant( finalCreator, finalTargetType ) );
                var nestingCreator = WeakInstanceCreator.ForDelegate( lambda.Compile( ) );
                return nestingCreator;
            }


            private static Array GetEnumValues( Type enumType ) {
                return enumType == typeof( bool )
                     ? new[] { false, true }
                     : Enum.GetValues( enumType );
            }


            private sealed class InstanceCreatorWrapper<T, U> : IInstanceCreator<T>
                where U : T {

                private readonly IInstanceCreator<U> innerCreator_;

                public InstanceCreatorWrapper( IInstanceCreator<U> innerCreator ) {
                    Debug.Assert( innerCreator != null );
                    this.innerCreator_ = innerCreator;
                }

                #region IInstanceCreator Members

                public Type InstanceType {
                    get { return typeof( T ); }
                }

                object IInstanceCreator.CreateInstance( ) {
                    IInstanceCreator innerCreator = this.innerCreator_;
                    return innerCreator.CreateInstance( );
                }

                public T CreateInstance( ) {
                    return this.innerCreator_.CreateInstance( );
                }

                #endregion

            }

        }

    }

}
