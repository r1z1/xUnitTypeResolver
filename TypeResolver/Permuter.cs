
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TypeResolver.Internal;


namespace TypeResolver {

    /// <summary>
    /// Permutes a collection of objects.
    /// </summary>
    public static class Permuter {

        /// <summary>
        /// Returns all permutations in the collections of available items.
        /// </summary>
        public static T[][] Permute<T>( IEnumerable<IEnumerable<T>> availableItems ) {
            Debug.Assert( availableItems != null );

            var listedItems = availableItems
                .Select( ( itemCollection ) => itemCollection.ToLinkList( ) )
                .ToLinkList( );

            return Permuter.Permute( listedItems, LinkList<T>.Empty ).ToArray( );
        }

        private static IEnumerable<T[]> Permute<T>( LinkList<LinkList<T>> availableItems, LinkList<T> currentPermutation ) {
            // If there are no remaining available items, return current permutation.
            if( availableItems.IsEmpty )
                return currentPermutation.ToArray( ).MakeEnumerable( );

            // Otherwise, loop through all of the current available items,
            //  returning permutations of the remaining items.
            var currentAvailableItems = availableItems.Value;
            var remainingAvailableItems = availableItems.Tail;
            Debug.Assert( currentAvailableItems != null );
            return
                from item in currentAvailableItems
                let nextPermutation = currentPermutation.Add( item )
                from permutation in Permuter.Permute( remainingAvailableItems, nextPermutation )
                select permutation;
        }

    }

}
