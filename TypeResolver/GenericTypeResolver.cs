
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Extensions;
using TypeResolver.Internal;


namespace TypeResolver {

    /// <summary>
    /// Finds all instantiable <see cref="Type"/>s for a given set of type constraints.
    /// </summary>
    public static partial class GenericTypeResolver {

        /// <summary>
        /// Returns all unassigned generic arguments on the <paramref name="genericType"/>.
        /// </summary>
        public static LinkList<Type> GetOpenGenericArguments( Type genericType ) {
            return GenericTypeResolver.GetOpenGenericArguments( genericType.GetGenericArguments( ) );
        }

        /// <summary>
        /// Creates an instance of the <paramref name="genericType"/> by assigning each of the <paramref name="openArguments"/>
        ///  to the concrete type specified by the <paramref name="bindings"/>.
        /// </summary>
        public static Type CreateConcreteType( Type genericType, LinkList<Type> openArguments, LinkList<Binding> bindings ) {
            return GenericTypeResolver.MakeConcreteType( genericType, openArguments, bindings );
        }


        /// <summary>
        /// Returns all concrete <see cref="Type"/>s that satisfy the <see cref="Constraint"/>s of the specified <paramref name="genericType"/>.
        /// </summary>
        [DebuggerHidden]
        public static ReadOnlyCollection<Type> GetConcreteTypes( Type genericType ) {
            return GenericTypeResolver.GetConcreteTypes( genericType, Binding.EmptyBindings );
        }

        /// <summary>
        /// Returns all concrete <see cref="Type"/>s that satisfy the <see cref="Constraint"/>s of the specified <paramref name="genericType"/> and type <paramref name="bindings"/>.
        /// </summary>
        [DebuggerHidden]
        public static ReadOnlyCollection<Type> GetConcreteTypes( Type genericType, LinkList<Binding> bindings ) {
            return GenericTypeResolver.GetConcreteTypesCore( genericType, bindings );
        }

        /// <summary>
        /// Returns a concrete <see cref="Type"/> for the given generic argument bindings, or null if the type could not be created.
        /// </summary>
        [DebuggerHidden]
        public static Type MakeConcreteType( Type genericType, LinkList<Binding> bindings ) {
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( genericType );
            return GenericTypeResolver.MakeConcreteType( genericType, openArguments, bindings );
        }


        /// <summary>
        /// Returns all concrete <see cref="MethodInfo"/> objects that satisfy the <see cref="Constraint"/>s of the method's generic arguments.
        /// </summary>
        [DebuggerHidden]
        public static ICollection<MethodInfo> GetConcreteMethods( MethodInfo genericMethod ) {
            return GenericTypeResolver.GetConcreteMethods( genericMethod, Binding.EmptyBindings );
        }

        /// <summary>
        /// Returns a concrete <see cref="MethodInfo"/> for the given generic argument bindings, or null if the method could not be created.
        /// </summary>
        [DebuggerHidden]
        public static MethodInfo MakeConcreteMethod( MethodInfo method, LinkList<Binding> bindings ) {
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( method.GetGenericArguments( ) );
            return GenericTypeResolver.MakeConcreteMethod( method, openArguments, bindings );
        }


        /// <summary>
        /// Returns bindings for all assigned generic arguments in the specified list of bindings.
        /// </summary>
        public static void GetAssignedGenericArguments( BindingCollection bindings, Type concreteType, Type genericType ) {
            Debug.Assert( bindings != null );
            Debug.Assert( concreteType != null );
            Debug.Assert( genericType != null );
            Debug.Assert( concreteType.Is( genericType ) );

            IEnumerable<Tuple<Type, Type>> concreteTypes = GetConcreteTypes( concreteType, genericType );

            bindings.Expand( concreteTypes, ( currentBindings, tuple ) => {
                Type type = tuple.Item1;
                Type generic = tuple.Item2;
                Type[] assignedArguments = type.GetGenericArguments( );
                Type[] unassignedArguments = generic.GetGenericArguments( );
                Debug.Assert( assignedArguments.Length == unassignedArguments.Length );

                for( int i = 0; currentBindings.Count > 0 && i < unassignedArguments.Length; ++i ) {
                    Type unassignedArgument = unassignedArguments[i];
                    Type assignedArgument = assignedArguments[i];

                    if( assignedArgument.IsGenericParameter )
                        continue;

                    if( !unassignedArgument.IsGenericParameter ) {
                        if( unassignedArgument.ContainsGenericParameters )
                            GenericTypeResolver.GetAssignedGenericArguments( currentBindings, assignedArgument, unassignedArgument );
                        continue;
                    }

                    if( !assignedArgument.Is( unassignedArgument ) ) {
                        currentBindings.Clear( );
                        break;
                    }

                    currentBindings.Reduce( b => {
                        Binding existingBinding = Binding.ForArgument( b, unassignedArgument );
                        return existingBinding == null
                             ? b.Add( new Binding( unassignedArgument, assignedArgument ) )
                             : (existingBinding.Type == assignedArgument ? b : null);
                    } );
                }

                if( genericType.IsGenericParameter )
                    currentBindings.Reduce( b => b.Add( new Binding( genericType, concreteType ) ) );
            } );
        }

        /// <summary>
        /// Returns the appropriate pairs of concrete and generic types, based on the kind of generic base type given.
        /// </summary>
        private static IEnumerable<Tuple<Type, Type>> GetConcreteTypes( Type concreteType, Type genericType ) {
            IEnumerable<Tuple<Type, Type>> concreteTypes;
            if( genericType.IsGenericType )
                concreteTypes = GetCorrespondingBaseTypes( concreteType, genericType ).Select( c => Tuple.Create( c, genericType ) );
            else if( genericType.IsGenericParameter )
                concreteTypes = genericType.GetGenericParameterConstraints( )
                    .Where( t => t.IsGenericType )
                    .SelectMany( t => GetCorrespondingBaseTypes( concreteType, t ).Select( c => Tuple.Create( c, t ) ) );
            else
                concreteTypes = Tuple.Create( concreteType, genericType ).MakeEnumerable( );

            return concreteTypes;
        }

        /// <summary>
        /// Returns the concrete class or interface <see cref="Type"/>s corresponding to the specified generic base <see cref="Type"/>.
        /// </summary>
        public static IEnumerable<Type> GetCorrespondingBaseTypes( Type concreteType, Type genericBaseType ) {
            Debug.Assert( genericBaseType.IsGenericType );
            Debug.Assert( concreteType.Is( genericBaseType ) );

            Type genericBaseTypeDefinition = genericBaseType.GetGenericTypeDefinition( );
            for( Type type = concreteType; type != null; type = type.BaseType ) {
                if( type.IsGenericType && genericBaseTypeDefinition == type.GetGenericTypeDefinition( ) )
                    yield return type;
            }

            if( genericBaseType.IsInterface ) {
                foreach( Type i in concreteType.GetInterfaces( ) ) {
                    bool isMatchingInterface =
                           i.IsGenericType
                        && i.GetGenericTypeDefinition( ) == genericBaseTypeDefinition
                        && (i.ContainsGenericParameters || i.Is( genericBaseType ));
                    if( isMatchingInterface )
                        yield return i;
                }
            }
        }

    }

}
