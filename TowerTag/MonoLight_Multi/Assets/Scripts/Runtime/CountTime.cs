using UnityEngine;

public interface ITimeService {

    float DeltaTime { get;}
}

public class TimeService : ITimeService{
    public float DeltaTime => Time.deltaTime;
}

public class CountTime : MonoBehaviour {
    public float TimeTest { get; private set; }
    private ITimeService _timeService;

    private void Awake() {
        _timeService = new TimeService();
    }

    private void Update() {
        TimeTest += _timeService.DeltaTime;
    }
}
