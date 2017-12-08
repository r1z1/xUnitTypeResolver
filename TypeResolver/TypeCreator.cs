
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace TypeResolver {

    /// <summary>
    /// Finds all instantiable <see cref="Type"/>s for a given set of type constraints.
    /// </summary>
    public static partial class TypeCreator {

        /// <summary>
        /// Prevents creators being constructor for objects of the specified <paramref name="type"/>,
        /// except for the sources defined by <paramref name="availableTypes"/>.
        /// </summary>
        [DebuggerHidden]
        public static void LimitInstances( Type type, params Type[] availableTypes ) {
            LimitInstancesCore( type, availableTypes );
        }

        /// <summary>
        /// Returns <see cref="IInstanceCreator"/>s for all instantiable <see cref="Type"/>s that derive from the specified <paramref name="baseType"/>.
        /// </summary>
        [DebuggerHidden]
        public static ReadOnlyCollection<IInstanceCreator> GetCreators( Type baseType ) {
            return TypeCreator.GetInstanceCreators( baseType );
        }

    }

}
