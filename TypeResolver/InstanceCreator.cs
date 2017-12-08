
using System;
using System.Diagnostics;
using System.Reflection;


namespace TypeResolver {

    /// <summary>
    /// Returns appropriate <see cref="IInstanceCreator{T}"/> objects for different constructors.
    /// </summary>
    public static partial class InstanceCreator {

        /// <summary>
        /// Returns an <see cref="IInstanceCreator{T}"/> for a type with a default constructor.
        /// </summary>
        public static IInstanceCreator<T> ForType<T>( )
            where T : new( ) {
            return new DefaultConstructorCreator<T>( );
        }

        /// <summary>
        /// Returns an <see cref="IInstanceCreator{T}"/> for a type using the specified <paramref name="creator"/> delegate.
        /// </summary>
        public static IInstanceCreator<T> ForDelegate<T>( Func<T> creator ) {
            return new DelegateCreator<T>( creator );
        }

        /// <summary>
        /// Returns an <see cref="IInstanceCreator{T}"/> for a method that returns a <typeparamref name="T"/> instance.
        /// </summary>
        public static IInstanceCreator<T> ForMethod<T>( MethodBase method, params IInstanceCreator[] arguments ) {
            var methodInfo = method as MethodInfo;
            if( methodInfo != null )
                return new MethodCreator<T>( methodInfo, arguments );

            var constructorInfo = method as ConstructorInfo;
            Debug.Assert( constructorInfo != null );
            return new ConstructorCreator<T>( constructorInfo, arguments );
        }

    }

}
