using Photon.Pun;

namespace Network.Synchronization.ExtrapolatedValues {
    public class ExtrapolatedFloat : ExtrapolatedValue<float> {
        protected override float CalculateSpeed(float oldValue, float newValue, int oldTimestamp, int newTimestamp) {
            return (newValue - oldValue) / HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                       oldTimestamp, newTimestamp);
        }

        public override float GetExtrapolatedValue() {
            return LastReceivedValue + Speed *
                   HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(LastTimestamp,
                       PhotonNetwork.ServerTimestamp);
        }
    }
}