
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Extensions;
using TypeResolver.Internal;


namespace TypeResolver {
    using CreatorCache = System.Collections.Concurrent.ConcurrentDictionary<Type, ReadOnlyCollection<IInstanceCreator>>;

    public static partial class TypeCreator {

        private static readonly ReadOnlyCollection<IInstanceCreator> TempCreators = new IInstanceCreator[0].ToReadOnlyCollection( );

        private static readonly object creatorCacheLock_ = new object( );
        private static readonly CreatorCache creatorCache_ = new CreatorCache( );


        static TypeCreator( ) {
            LimitInstances( typeof( object ) );
        }

        private static void LimitInstancesCore( Type targetType, IEnumerable<Type> availableTypes ) {
            Debug.Assert( targetType != null );
            Debug.Assert( !creatorCache_.ContainsKey( targetType ) || creatorCache_[targetType].Count == 0, "Creators have already been initialized for type " + targetType.FullName );

            creatorCache_[targetType] = GetInstanceCreators( targetType, availableTypes );
        }

        private static ReadOnlyCollection<IInstanceCreator> GetInstanceCreators( Type targetType ) {
            Debug.Assert( targetType != null );
            Debug.Assert( targetType.IsVisible );

            // If creators have not been cached, or are in the process of being cached, take lock and re-check cache.
            ReadOnlyCollection<IInstanceCreator> cached;
            if( !creatorCache_.TryGetValue( targetType, out cached ) || object.ReferenceEquals( cached, TempCreators ) ) {
                bool isInstanceCreator = targetType.IsGenericType && targetType.GetGenericTypeDefinition( ) == typeof( IInstanceCreator<> );
                Type[] availableTypes = isInstanceCreator ? null : TypeLoader.GetUsableTypes( targetType ).ToArray( );
                lock( creatorCacheLock_ ) {
                    if( !TypeCreator.creatorCache_.ContainsKey( targetType ) ) {
                        TypeCreator.creatorCache_[targetType] = TempCreators;
                        ReadOnlyCollection<IInstanceCreator> creators;

                        // If target type is for IInstanceCreator<T>, wrap creators for T.
                        if( isInstanceCreator ) {
                            Type innerTargetType = targetType.GetGenericArguments( )[0];
                            var innerCreators = TypeCreator.GetCreators( innerTargetType );

                            var outerCreators = new IInstanceCreator[innerCreators.Count];
                            for( int i = 0; i < outerCreators.Length; ++i )
                                outerCreators[i] = WeakInstanceCreator.ForInstanceCreator( targetType, innerCreators[i] );
                            creators = outerCreators.ToReadOnlyCollection( );
                        }
                        // Otherwise, get creators for type directly.
                        else {
                            creators = GetInstanceCreators( targetType, availableTypes );
                        }

                        TypeCreator.creatorCache_[targetType] = creators;
                    }
                }
            }

            return TypeCreator.creatorCache_[targetType];
        }

        private static ReadOnlyCollection<IInstanceCreator> GetInstanceCreators( Type targetType, IEnumerable<Type> availableTypes ) {
            return availableTypes
                .SelectMany( type => GetInstanceCreators( targetType, type ) )
                .Where( creator => creator.InstanceType.Is( targetType ) )
                .ToReadOnlyCollection( );
        }

        private static IEnumerable<IInstanceCreator> GetInstanceCreators( Type targetType, Type availableType ) {
            Debug.Assert( targetType != null );
            Debug.Assert( availableType != null );


            // If type is generic, find all concrete types.
            if( availableType.ContainsGenericParameters )
                return TypeCreator.GetGenericCreators( targetType, availableType );

            // If given type derives from target base type, check constructors.
            if( availableType.Is( targetType ) )
                return GetConstructorCreators( targetType, availableType );

            // If type is a static factory, check for usable factory methods.
            if( availableType.IsAbstract && availableType.IsSealed && availableType.Name.StartsWith( "Factory" ) )
                return GetFactoryCreators( targetType, availableType );

            // Cannot use type to create instances of the target type.
            return Enumerable.Empty<IInstanceCreator>( );
        }


        private static IEnumerable<IInstanceCreator> GetGenericCreators( Type targetType, Type genericType ) {
            Debug.Assert( targetType != null );
            Debug.Assert( genericType != null );
            Debug.Assert( genericType.ContainsGenericParameters );

            // Get all bindings for the generic type's arguments that satisfy the inheritance constraint.
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( genericType );
            Assembly[] referenceAssemblies;
            LinkList<Constraint> inheritanceConstraints =
                targetType.IsGenericParameter
                    ? Constraint.GetConstraints( targetType, out referenceAssemblies )
                    : Constraint.GetInheritanceConstraint( targetType ).MakeLinkList( );
            var argumentBindings = new BindingCollection( Binding.EmptyBindings );
            Constraint.SatisfyConstraints( argumentBindings, genericType, inheritanceConstraints );

            // Construct concrete types for each set of argument bindings,
            //  and return the creators for each concrete type.
            var creators =
                from arguments in argumentBindings
                let concreteType = GenericTypeResolver.CreateConcreteType( genericType, openArguments, arguments )
                where concreteType != null
                from creator in TypeCreator.GetInstanceCreators( targetType, concreteType )
                select creator;

            return creators;
        }

        private static IEnumerable<IInstanceCreator> GetConstructorCreators( Type targetType, Type availableType ) {
            Debug.Assert( targetType != null );
            Debug.Assert( availableType != null );
            Debug.Assert( !availableType.IsAbstract );

            if( targetType.ContainsGenericParameters ) {
                Type definition = targetType.IsGenericParameter ? targetType : targetType.GetGenericTypeDefinition( );
                var bindings = new BindingCollection( Binding.EmptyBindings );
                GenericTypeResolver.GetAssignedGenericArguments( bindings, availableType, definition );

                foreach( var binding in bindings ) {
                    Type resolvedType =
                        definition.IsGenericParameter
                            ? Binding.ForArgument( binding, definition ).Type
                            : GenericTypeResolver.MakeConcreteType( definition, binding );

                    targetType = resolvedType;
                    break;
                }
            }

            var creators = new List<IInstanceCreator>( );

            // If type is an enumeration, get creator for enum values.
            if( availableType.IsEnum || availableType == typeof( bool ) )
                creators.AddRange( WeakInstanceCreator.ForEnum( availableType ) );
            // Otherwise, get creator for any default constructor.
            else if( Constraint.HasDefaultConstructor( availableType ) )
                creators.Add( WeakInstanceCreator.ForType( availableType ) );

            // Get creators for any parameterized constructors.
            var parameterizedConstructors = availableType.GetConstructors( ).Where( ( c ) => c.GetParameters( ).Length > 0 ).ToArray( );
            if( parameterizedConstructors.Length < 3 )
                foreach( var constructor in parameterizedConstructors )
                    creators.AddRange( TypeCreator.GetMethodCreators( targetType, constructor ) );

            return creators;
        }

        private static IEnumerable<IInstanceCreator> GetFactoryCreators( Type targetType, Type availableType ) {
            Debug.Assert( targetType != null );
            Debug.Assert( availableType != null );
            Debug.Assert( availableType.IsAbstract && availableType.IsSealed );

            var factoryMethods =
                from m in availableType.GetMethods( BindingFlags.Public | BindingFlags.Static )
                // Check that method is a "GetInstances" factory method, and returns an IEnumerable<> collection.
                where m.Name.StartsWith( "Get", StringComparison.OrdinalIgnoreCase )
                   && m.Name.EndsWith( "Instances", StringComparison.OrdinalIgnoreCase )
                   && m.ReturnType.IsGenericType
                   && m.ReturnType.GetGenericTypeDefinition( ) == typeof( IEnumerable<> )
                // Check that the enumerable collection is of type Func<>.
                let enumerableArgType = m.ReturnType.GetGenericArguments( )[0]
                where enumerableArgType.IsGenericType
                   && enumerableArgType.GetGenericTypeDefinition( ) == typeof( Func<> )
                // Check that the return type of the Func<> delegate is valid for the target type.
                let funcArgType = enumerableArgType.GetGenericArguments( )[0]
                where targetType.Is( funcArgType )
                select m;

            // Call each factory method, returning creators from the resulting Func<> delegates.
            var factoryCreators =
                from method in factoryMethods
                from methodCreator in TypeCreator.GetMethodCreators( method.ReturnType, method )
                let funcCollection = (System.Collections.IEnumerable)methodCreator.CreateInstance( )
                from func in funcCollection.Cast<object>( )
                let creator = WeakInstanceCreator.ForDelegate( func )
                select creator;

            return factoryCreators;
        }

        private static IEnumerable<IInstanceCreator> GetMethodCreators( Type targetType, MethodBase method ) {
            Debug.Assert( targetType != null );
            Debug.Assert( method != null );
            Debug.Assert( method.IsGenericMethod || !targetType.ContainsGenericParameters );

            // Ignore recursive parameters.
            var parameters = method.GetParameters( );
            if( parameters.Any( p => p.ParameterType.Is( targetType ) || p.ParameterType.Is( method.DeclaringType ) ) )
                yield break;

            // Retrieve creators for each parameter.
            var availableArguments = parameters.Select( ( p ) => TypeCreator.GetInstanceCreators( p.ParameterType ).AsEnumerable( ) );

            // Call constructor with all argument permutations.
            foreach( var arguments in Permuter.Permute( availableArguments ) ) {
                // If method is concrete, use it.
                if( !method.IsGenericMethod ) {
                    yield return WeakInstanceCreator.ForMethod( targetType, method, arguments );
                }
                // Otherwise, try to resolve generic arguments on method.
                else if( method is MethodInfo ) {
                    var methodInfo = (MethodInfo)method;
                    var bindings = new BindingCollection( Binding.EmptyBindings );
                    for( int i = 0; i < parameters.Length; ++i ) {
                        ParameterInfo parameter = parameters[i];
                        IInstanceCreator argument = arguments[i];
                        if( parameter.ParameterType.ContainsGenericParameters )
                            GenericTypeResolver.GetAssignedGenericArguments( bindings, argument.InstanceType, parameter.ParameterType );
                    }

                    foreach( LinkList<Binding> b in bindings ) {
                        var concreteMethod = GenericTypeResolver.MakeConcreteMethod( methodInfo, b );
                        if( concreteMethod != null ) {
                            targetType = concreteMethod.ReturnType;
                            yield return WeakInstanceCreator.ForMethod( targetType, concreteMethod, arguments );
                        }
                    }
                }
            }
        }

    }

}
