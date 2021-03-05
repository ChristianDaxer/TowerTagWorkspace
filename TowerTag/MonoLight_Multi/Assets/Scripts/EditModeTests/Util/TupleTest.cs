using NUnit.Framework;
using UnityEngine;

public class TupleTest {
    [Test]
    public void TupleShouldBeEditable() {
        var namedTuple = (index: 2, type: "sexy", isAwesome: true);
        namedTuple.type = "super sexy";
        Assert.AreEqual("super sexy", namedTuple.type);
    }

    [Test]
    public void TupleShouldPrintPretty() {
        var namedTuple = (index: 2, type: "sexy", isAwesome: true);
        Assert.AreEqual("(2, sexy, True)", namedTuple.ToString());
    }

    [Test]
    public void TuplesShouldBeEqual() {
        var tuple1 = (a: 1, b: "yeah");
        var tuple2 = (a: 1, b: "yeah");
        Assert.AreEqual(tuple1, tuple2);
    }

    [Test]
    public void TupleEqualityIsNameIndependent() {
        var tuple1 = (a: 1, b: "yeah");
        var tuple2 = (c: 1, "yeah");
        Assert.AreEqual(tuple1, tuple2);
    }

    [Test]
    public void UnnamedTuplesShouldNotBeEqual() {
        var tuple1 = (1, "yeah");
        var tuple2 = ("yeah", 1);
        Assert.AreNotEqual(tuple1, tuple2);
    }
}