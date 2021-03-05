using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DependencyTreeEnumerator {
    public delegate void EnumerateDependencyTreeDelegate(
        DependencyTreeEnumerator enumerator, 
        Dependency thisDependency, 
        ReferenceInfo referenceInfo, 
        Dependency nextDependency);

    protected readonly Dictionary<string, Dependency> _dependencyLut;
    public Dictionary<string, Dependency> Dependencies => _dependencyLut;
    protected readonly EnumerateDependencyTreeDelegate _traverseDelegate;

    protected string _currentDependencyGuid { get; private set; }

    public string NextDependencyGuid { set => _currentDependencyGuid = value; }

    public DependencyTreeEnumerator(
        Dictionary<string, Dependency> dependencyLut, 
        string startingDependencyGuid,
        EnumerateDependencyTreeDelegate traverseDelegate) {
        _dependencyLut = dependencyLut;
        _currentDependencyGuid = startingDependencyGuid;
        _traverseDelegate = traverseDelegate;
    }
}

public class TraverseUpDependencyTree : DependencyTreeEnumerator {

    private Dependency currentDependency;
    private int currentReferencingIndex = -1;

    private readonly List<TraverseUpDependencyTree> levelUpEnumerators = new List<TraverseUpDependencyTree>(10);
    private int currentEnumeratorIndex = -1;

    private string[] cachedKeys;

    private bool isFinished = false;

    public TraverseUpDependencyTree(
        Dictionary<string, Dependency> dependencyLut,
        string startingDependencyGuid,
        EnumerateDependencyTreeDelegate walkUpDelegate) : base(dependencyLut, startingDependencyGuid, walkUpDelegate) {}

    public bool Start () { 
        if (string.IsNullOrEmpty(_currentDependencyGuid))
            return false;

        if (reconfigureCircularDependencyLookup.Contains(_currentDependencyGuid))
            return false;
        reconfigureCircularDependencyLookup.Add(_currentDependencyGuid);

        if (!_dependencyLut.TryGetValue(_currentDependencyGuid, out currentDependency))
            return false;

        if (
            currentDependency.referencingDependencyGuids == null ||
            currentDependency.referencingDependencyGuids.Count == 0)
            return false;

        currentEnumeratorIndex = 0;
        currentReferencingIndex = 0;

        cachedKeys = currentDependency.referencingDependencyGuids.Keys.ToArray();
        return true;
    }

    private static readonly HashSet<string> reconfigureCircularDependencyLookup = new HashSet<string>();
    public static void Reset() => reconfigureCircularDependencyLookup.Clear();
    public bool Next () {

        if (currentReferencingIndex >= cachedKeys.Length) {

            isFinished = true;

            if (levelUpEnumerators.Count == 0 || currentEnumeratorIndex >= levelUpEnumerators.Count)
                return false;

            if (!levelUpEnumerators[currentEnumeratorIndex].Next())
                currentEnumeratorIndex++;

            return true;
        }

        var referencingGuid = cachedKeys[currentReferencingIndex++];
        var referenceInfo = currentDependency.referencingDependencyGuids[referencingGuid];
        var nextDependency = _dependencyLut[referencingGuid];

        var levelUpEnumerator = new TraverseUpDependencyTree(_dependencyLut, nextDependency.dependencyGuid, _traverseDelegate);
        if (levelUpEnumerator.Start())
            levelUpEnumerators.Add(levelUpEnumerator);

        _traverseDelegate(this, currentDependency, referenceInfo, nextDependency);

        return true;
    }
}
