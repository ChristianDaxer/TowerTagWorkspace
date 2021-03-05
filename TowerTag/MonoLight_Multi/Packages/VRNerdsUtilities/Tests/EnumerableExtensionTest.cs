using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VRNerdsUtilities.Tests {
    public class EnumerableExtensionTest {
        [Test]
        public void ShouldExecuteActionWithForEach() {
            // GIVEN
            IEnumerable<int> sevenToEleven = Enumerable.Range(7, 5);
            var cumsum = 0;

            // WHEN
            sevenToEleven.ForEach(number => cumsum += number);

            // THEN
            Assert.AreEqual(45, cumsum);
        }

        [Test]
        public void ShouldApplyLazily() {
            // GIVEN
            IEnumerable<int> sevenToEleven = Enumerable.Range(7, 5);
            var cumsum = 0;

            // WHEN
            IEnumerable<int> applied = sevenToEleven.Apply(number => cumsum += number);

            // THEN
            Assert.AreEqual(0, cumsum);
            Assert.AreEqual(sevenToEleven, applied);
            Assert.AreEqual(45, cumsum);
        }
    }
}