
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestPermuter {

        public static IEnumerable<object[]> Permutations {
            get {
                yield return new object[] {
                    new[] { new[] { 1 } },
                    new[] {
                        new[] { 1 }
                    }
                };
                yield return new object[] {
                    new[] { new[] { 1 }, new int[] { }, new[] { 3 } },
                    new int[][] { }
                };
                yield return new object[] {
                    new[] { new[] { 1, 2, 3 } },
                    new[] {
                        new[] { 1 },
                        new[] { 2 },
                        new[] { 3 }
                    }
                };
                yield return new object[] {
                    new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } },
                    new[] {
                        new[] { 1, 4 }, new[] { 1, 5 }, new[] { 1, 6 },
                        new[] { 2, 4 }, new[] { 2, 5 }, new[] { 2, 6 },
                        new[] { 3, 4 }, new[] { 3, 5 }, new[] { 3, 6 },
                    }
                };
            }
        }


        [Theory]
        [MemberData( "Permutations" )]
        public void Permute_returns_expected_permutation( int[][] items, int[][] expectedPermutations ) {
            var permutations = Permuter.Permute( items );

            Assert.NotNull( permutations );
            Assert.Equal( expectedPermutations.Length, permutations.Length );
            foreach( var permutation in permutations )
                Assert.Contains( permutation, expectedPermutations, ArrayComparer<int>.Instance );
        }

    }

}
