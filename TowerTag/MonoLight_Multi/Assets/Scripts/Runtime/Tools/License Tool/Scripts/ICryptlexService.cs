// ReSharper disable InconsistentNaming : using Cryptlex naming conventions for consistency
#if !UNITY_ANDROID
using System.Text;

namespace Cryptlex {
    public enum PermissionFlags : uint {
        LA_USER = 1,
        LA_SYSTEM = 2,
    }

    public delegate void CallbackType(uint status);

    public static class StatusCodes {
        /*
            CODE: LA_OK
            MESSAGE: Success code.
        */
        public const int LA_OK = 0;

        /*
            CODE: LA_FAIL
            MESSAGE: Failure code.
        */
        public const int LA_FAIL = 1;

        /*
            CODE: LA_EXPIRED
            MESSAGE: The license has expired or system time has been tampered
            with. Ensure your date and time settings are correct.
        */
        public const int LA_EXPIRED = 20;

        /*
            CODE: LA_SUSPENDED
            MESSAGE: The license has been suspended.
        */
        public const int LA_SUSPENDED = 21;

        /*
            CODE: LA_GRACE_PERIOD_OVER
            MESSAGE: The grace period for server sync is over.
        */
        public const int LA_GRACE_PERIOD_OVER = 22;

        /*
            CODE: LA_TRIAL_EXPIRED
            MESSAGE: The trial has expired or system time has been tampered
            with. Ensure your date and time settings are correct.
        */
        public const int LA_TRIAL_EXPIRED = 25;

        /*
            CODE: LA_LOCAL_TRIAL_EXPIRED
            MESSAGE: The local trial has expired or system time has been tampered
            with. Ensure your date and time settings are correct.
        */
        public const int LA_LOCAL_TRIAL_EXPIRED = 26;

        /*
            CODE: LA_E_FILE_PATH
            MESSAGE: Invalid file path.
        */
        public const int LA_E_FILE_PATH = 40;

        /*
            CODE: LA_E_PRODUCT_FILE
            MESSAGE: Invalid or corrupted product file.
        */
        public const int LA_E_PRODUCT_FILE = 41;

        /*
            CODE: LA_E_PRODUCT_DATA
            MESSAGE: Invalid product data.
        */
        public const int LA_E_PRODUCT_DATA = 42;

        /*
            CODE: LA_E_PRODUCT_ID
            MESSAGE: The product id is incorrect.
        */
        public const int LA_E_PRODUCT_ID = 43;

        /*
            CODE: LA_E_SYSTEM_PERMISSION
            MESSAGE: Insufficient system permissions. Occurs when LA_SYSTEM flag is used
            but application is not run with admin privileges.
        */
        public const int LA_E_SYSTEM_PERMISSION = 44;

        /*
            CODE: LA_E_FILE_PERMISSION
            MESSAGE: No permission to write to file.
        */
        public const int LA_E_FILE_PERMISSION = 45;

        /*
            CODE: LA_E_WMIC
            MESSAGE: Fingerprint couldn't be generated because Windows Management
            Instrumentation (WMI) service has been disabled. This error is specific
            to Windows only.
        */
        public const int LA_E_WMIC = 46;

        /*
            CODE: LA_E_TIME
            MESSAGE: The difference between the network time and the system time is
            more than allowed clock offset.
        */
        public const int LA_E_TIME = 47;

        /*
            CODE: LA_E_INET
            MESSAGE: Failed to connect to the server due to network error.
        */
        public const int LA_E_INET = 48;

        /*
            CODE: LA_E_NET_PROXY
            MESSAGE: Invalid network proxy.
        */
        public const int LA_E_NET_PROXY = 49;

        /*
            CODE: LA_E_HOST_URL
            MESSAGE: Invalid Cryptlex host url.
        */
        public const int LA_E_HOST_URL = 50;

        /*
            CODE: LA_E_BUFFER_SIZE
            MESSAGE: The buffer size was smaller than required.
        */
        public const int LA_E_BUFFER_SIZE = 51;

        /*
            CODE: LA_E_APP_VERSION_LENGTH
            MESSAGE: App version length is more than 256 characters.
        */
        public const int LA_E_APP_VERSION_LENGTH = 52;

        /*
            CODE: LA_E_REVOKED
            MESSAGE: The license has been revoked.
        */
        public const int LA_E_REVOKED = 53;

        /*
            CODE: LA_E_LICENSE_KEY
            MESSAGE: Invalid license key.
        */
        public const int LA_E_LICENSE_KEY = 54;

        /*
            CODE: LA_E_LICENSE_TYPE
            MESSAGE: Invalid license type. Make sure floating license
            is not being used.
        */
        public const int LA_E_LICENSE_TYPE = 55;

        /*
            CODE: LA_E_OFFLINE_RESPONSE_FILE
            MESSAGE: Invalid offline activation response file.
        */
        public const int LA_E_OFFLINE_RESPONSE_FILE = 56;

        /*
            CODE: LA_E_OFFLINE_RESPONSE_FILE_EXPIRED
            MESSAGE: The offline activation response has expired.
        */
        public const int LA_E_OFFLINE_RESPONSE_FILE_EXPIRED = 57;

        /*
            CODE: LA_E_ACTIVATION_LIMIT
            MESSAGE: The license has reached it's allowed activations limit.
        */
        public const int LA_E_ACTIVATION_LIMIT = 58;

        /*
            CODE: LA_E_ACTIVATION_NOT_FOUND
            MESSAGE: The license activation was deleted on the server.
        */
        public const int LA_E_ACTIVATION_NOT_FOUND = 59;

        /*
            CODE: LA_E_DEACTIVATION_LIMIT
            MESSAGE: The license has reached it's allowed deactivations limit.
        */
        public const int LA_E_DEACTIVATION_LIMIT = 60;

        /*
            CODE: LA_E_TRIAL_NOT_ALLOWED
            MESSAGE: Trial not allowed for the product.
        */
        public const int LA_E_TRIAL_NOT_ALLOWED = 61;

        /*
            CODE: LA_E_TRIAL_ACTIVATION_LIMIT
            MESSAGE: Your account has reached it's trial activations limit.
        */
        public const int LA_E_TRIAL_ACTIVATION_LIMIT = 62;

        /*
            CODE: LA_E_MACHINE_FINGERPRINT
            MESSAGE: Machine fingerprint has changed since activation.
        */
        public const int LA_E_MACHINE_FINGERPRINT = 63;

        /*
            CODE: LA_E_METADATA_KEY_LENGTH
            MESSAGE: Metadata key length is more than 256 characters.
        */
        public const int LA_E_METADATA_KEY_LENGTH = 64;

        /*
            CODE: LA_E_METADATA_VALUE_LENGTH
            MESSAGE: Metadata value length is more than 256 characters.
        */
        public const int LA_E_METADATA_VALUE_LENGTH = 65;

        /*
            CODE: LA_E_ACTIVATION_METADATA_LIMIT
            MESSAGE: The license has reached it's metadata fields limit.
        */
        public const int LA_E_ACTIVATION_METADATA_LIMIT = 66;

        /*
            CODE: LA_E_TRIAL_ACTIVATION_METADATA_LIMIT
            MESSAGE: The trial has reached it's metadata fields limit.
        */
        public const int LA_E_TRIAL_ACTIVATION_METADATA_LIMIT = 67;

        /*
            CODE: LA_E_METADATA_KEY_NOT_FOUND
            MESSAGE: The metadata key does not exist.
        */
        public const int LA_E_METADATA_KEY_NOT_FOUND = 68;

        /*
            CODE: LA_E_TIME_MODIFIED
            MESSAGE: The system time has been tampered (backdated).
        */
        public const int LA_E_TIME_MODIFIED = 69;

        /*
            CODE: LA_E_VM
            MESSAGE: Application is being run inside a virtual machine / hypervisor,
            and activation has been disallowed in the VM.
        */
        public const int LA_E_VM = 80;

        /*
            CODE: LA_E_COUNTRY
            MESSAGE: Country is not allowed.
        */
        public const int LA_E_COUNTRY = 81;

        /*
            CODE: LA_E_IP
            MESSAGE: IP address is not allowed.
        */
        public const int LA_E_IP = 82;

        /*
            CODE: LA_E_RATE_LIMIT
            MESSAGE: Rate limit for API has reached, try again later.
        */
        public const int LA_E_RATE_LIMIT = 90;

        /*
            CODE: LA_E_SERVER
            MESSAGE: Server error.
        */
        public const int LA_E_SERVER = 91;

        /*
            CODE: LA_E_CLIENT
            MESSAGE: Client error.
        */
        public const int LA_E_CLIENT = 92;
    }

    public class StatusCodesV2 {
        public const int LA_OK = 0x00;

        public const int LA_FAIL = 0x01;

        /*
            CODE: LA_EXPIRED
    
            MESSAGE: The product key has expired or system time has been tampered
            with. Ensure your date and time settings are correct.
        */

        public const int LA_EXPIRED = 0x02;

        /*
            CODE: LA_REVOKED
    
            MESSAGE: The product key has been revoked.
        */

        public const int LA_REVOKED = 0x03;

        /*
            CODE: LA_GP_OVER
    
            MESSAGE: The grace period is over.
        */

        public const int LA_GP_OVER = 0x04;

        /*
            CODE: LA_E_INET
    
            MESSAGE: Failed to connect to the server due to network error.
        */

        public const int LA_E_INET = 0x05;

        /*
            CODE: LA_E_PKEY
    
            MESSAGE: Invalid product key.
        */

        public const int LA_E_PKEY = 0x06;

        /*
            CODE: LA_E_PFILE
    
            MESSAGE: Invalid or corrupted product file.
        */

        public const int LA_E_PFILE = 0x07;

        /*
            CODE: LA_E_FPATH
    
            MESSAGE: Invalid product file path.
        */

        public const int LA_E_FPATH = 0x08;

        /*
            CODE: LA_E_GUID
    
            MESSAGE: The version GUID doesn't match that of the product file.
        */

        public const int LA_E_GUID = 0x09;

        /*
            CODE: LA_E_OFILE
    
            MESSAGE: Invalid offline activation response file.
        */

        public const int LA_E_OFILE = 0x0A;

        /*
            CODE: LA_E_PERMISSION
    
            MESSAGE: Insufficent system permissions. Occurs when LA_SYSTEM flag is used
            but application is not run with admin privileges.
        */

        public const int LA_E_PERMISSION = 0x0B;

        /*
            CODE: LA_E_EDATA_LEN
    
            MESSAGE: Extra activation data length is more than 256 characters.
        */


        public const int LA_E_EDATA_LEN = 0x0C;

        /*
            CODE: LA_E_TKEY
    
            MESSAGE: The trial key doesn't match that of the product file.
        */

        public const int LA_E_TKEY = 0x0D;

        /*
            CODE: LA_E_TIME
    
            MESSAGE: The system time has been tampered with. Ensure your date
            and time settings are correct.
        */

        public const int LA_E_TIME = 0x0E;

        /*
            CODE: LA_E_VM
    
            MESSAGE: Application is being run inside a virtual machine / hypervisor,
            and activation has been disallowed in the VM.
            but
        */

        public const int LA_E_VM = 0x0F;

        /*
            CODE: LA_E_WMIC
    
            MESSAGE: Fingerprint couldn't be generated because Windows Management 
            Instrumentation (WMI) service has been disabled. This error is specific
            to Windows only.
        */

        public const int LA_E_WMIC = 0x10;

        /*
            CODE: LA_E_TEXT_KEY
    
            MESSAGE: Invalid trial extension key.
        */

        public const int LA_E_TEXT_KEY = 0x11;

        /*
            CODE: LA_E_TRIAL_LEN
    
            MESSAGE: The trial length doesn't match that of the product file.
        */

        public const int LA_E_TRIAL_LEN = 0x12;

        /*
            CODE: LA_T_EXPIRED
    
            MESSAGE: The trial has expired or system time has been tampered
            with. Ensure your date and time settings are correct.
        */

        public const int LA_T_EXPIRED = 0x13;

        /*
            CODE: LA_TEXT_EXPIRED
    
            MESSAGE: The trial extension key being used has already expired or system
            time has been tampered with. Ensure your date and time settings are correct.
        */

        public const int LA_TEXT_EXPIRED = 0x14;

        /*
            CODE: LA_E_BUFFER_SIZE
    
            MESSAGE: The buffer size was smaller than required.
        */

        public const int LA_E_BUFFER_SIZE = 0x15;

        /*
            CODE: LA_E_CUSTOM_FIELD_ID
    
            MESSAGE: Invalid custom field id.
        */

        public const int LA_E_CUSTOM_FIELD_ID = 0x16;

        /*
            CODE: LA_E_NET_PROXY
    
            MESSAGE: Invalid network proxy.
        */

        public const int LA_E_NET_PROXY = 0x17;

        /*
            CODE: LA_E_HOST_URL
    
            MESSAGE: Invalid Cryptlex host url.
        */

        public const int LA_E_HOST_URL = 0x18;

        /*
            CODE: LA_E_DEACT_LIMIT
    
            MESSAGE: Deactivation limit for key has reached.
        */

        public const int LA_E_DEACT_LIMIT = 0x19;

        /*
            CODE: LA_E_ACT_LIMIT
    
            MESSAGE: Activation limit for key has reached.
        */

        public const int LA_E_ACT_LIMIT = 0x1A;
    }

    public interface ICryptlexService {
        int SetProductFile(string filePath);
        int SetProductData(string productData);
        int SetProductId(string productId, PermissionFlags flags);
        int SetLicenseKey(string licenseKey);
        int SetLicenseCallback(CallbackType callback);
        int SetActivationMetadata(string key, string value);
        int SetTrialActivationMetadata(string key, string value);
        int SetAppVersion(string appVersion);
        int SetNetworkProxy(string proxy);
        int GetProductMetadata(string key, StringBuilder value, int length);
        int GetLicenseMetadata(string key, StringBuilder value, int length);
        int GetLicenseKey(StringBuilder licenseKey, int length);
        int GetLicenseExpiryDate(ref uint expiryDate);
        int GetLicenseUserEmail(StringBuilder email, int length);
        int GetLicenseUserName(StringBuilder name, int length);
        int GetLicenseType(StringBuilder licenseType, int length);
        int GetActivationMetadata(string key, StringBuilder value, int length);
        int GetTrialActivationMetadata(string key, StringBuilder value, int length);
        int GetTrialExpiryDate(ref uint trialExpiryDate);
        int GetTrialId(StringBuilder trialId, int length);
        int GetLocalTrialExpiryDate(ref uint trialExpiryDate);
        int ActivateLicense();
        int ActivateLicenseOffline(string filePath);
        int GenerateOfflineActivationRequest(string filePath);
        int DeactivateLicense();
        int GenerateOfflineDeactivationRequest(string filePath);
        int IsLicenseGenuine();
        int IsLicenseValid();
        int ActivateTrial();
        int ActivateTrialOffline(string filePath);
        int GenerateOfflineTrialActivationRequest(string filePath);
        int IsTrialGenuine();
        int ActivateLocalTrial(uint trialLength);
        int IsLocalTrialGenuine();
        int ExtendLocalTrial(uint trialExtensionLength);
        int Reset();
        int SetVersionGUID(string versionGUID, PermissionFlags flags);
        int IsProductGenuine();
        int GetCustomLicenseField(string fieldId, StringBuilder fieldValue, int length);
        int GetProductKey(StringBuilder productKey, int length);
        int SetProductFileV2(string filePath);
        int DeactivateProductV2();
    }

    public class CryptlexService : ICryptlexService {
        public int SetProductFile(string filePath) {
            return LexActivator.SetProductFile(filePath);
        }

        public int SetProductData(string productData) {
            return LexActivator.SetProductData(productData);
        }

        public int SetProductId(string productId, PermissionFlags flags) {
            return LexActivator.SetProductId(productId, (LexActivator.PermissionFlags) flags);
        }

        public int SetLicenseKey(string licenseKey) {
            return LexActivator.SetLicenseKey(licenseKey);
        }

        public int SetLicenseCallback(CallbackType callback) {
            return LexActivator.SetLicenseCallback(status => callback(status));
        }

        public int SetActivationMetadata(string key, string value) {
            return LexActivator.SetActivationMetadata(key, value);
        }

        public int SetTrialActivationMetadata(string key, string value) {
            return LexActivator.SetTrialActivationMetadata(key, value);
        }

        public int SetAppVersion(string appVersion) {
            return LexActivator.SetAppVersion(appVersion);
        }

        public int SetNetworkProxy(string proxy) {
            return LexActivator.SetNetworkProxy(proxy);
        }

        public int GetProductMetadata(string key, StringBuilder value, int length) {
            return LexActivator.GetProductMetadata(key, value, length);
        }

        public int GetLicenseMetadata(string key, StringBuilder value, int length) {
            return LexActivator.GetLicenseMetadata(key, value, length);
        }

        public int GetLicenseKey(StringBuilder licenseKey, int length) {
            return LexActivator.GetLicenseKey(licenseKey, length);
        }

        public int GetLicenseExpiryDate(ref uint expiryDate) {
            return LexActivator.GetLicenseExpiryDate(ref expiryDate);
        }

        public int GetLicenseUserEmail(StringBuilder email, int length) {
            return LexActivator.GetLicenseUserEmail(email, length);
        }

        public int GetLicenseUserName(StringBuilder name, int length) {
            return LexActivator.GetLicenseUserName(name, length);
        }

        public int GetLicenseType(StringBuilder licenseType, int length) {
            return LexActivator.GetLicenseType(licenseType, length);
        }

        public int GetActivationMetadata(string key, StringBuilder value, int length) {
            return LexActivator.GetActivationMetadata(key, value, length);
        }

        public int GetTrialActivationMetadata(string key, StringBuilder value, int length) {
            return LexActivator.GetTrialActivationMetadata(key, value, length);
        }

        public int GetTrialExpiryDate(ref uint trialExpiryDate) {
            return LexActivator.GetTrialExpiryDate(ref trialExpiryDate);
        }

        public int GetTrialId(StringBuilder trialId, int length) {
            return LexActivator.GetTrialId(trialId, length);
        }

        public int GetLocalTrialExpiryDate(ref uint trialExpiryDate) {
            return LexActivator.GetLocalTrialExpiryDate(ref trialExpiryDate);
        }

        public int ActivateLicense() {
            return LexActivator.ActivateLicense();
        }

        public int ActivateLicenseOffline(string filePath) {
            return LexActivator.ActivateLicenseOffline(filePath);
        }

        public int GenerateOfflineActivationRequest(string filePath) {
            return LexActivator.GenerateOfflineActivationRequest(filePath);
        }

        public int DeactivateLicense() {
            return LexActivator.DeactivateLicense();
        }

        public int GenerateOfflineDeactivationRequest(string filePath) {
            return LexActivator.GenerateOfflineDeactivationRequest(filePath);
        }

        public int IsLicenseGenuine() {
            return LexActivator.IsLicenseGenuine();
        }

        public int IsLicenseValid() {
            return LexActivator.IsLicenseValid();
        }

        public int ActivateTrial() {
            return LexActivator.ActivateTrial();
        }

        public int ActivateTrialOffline(string filePath) {
            return LexActivator.ActivateTrialOffline(filePath);
        }

        public int GenerateOfflineTrialActivationRequest(string filePath) {
            return LexActivator.GenerateOfflineTrialActivationRequest(filePath);
        }

        public int IsTrialGenuine() {
            return LexActivator.IsTrialGenuine();
        }

        public int ActivateLocalTrial(uint trialLength) {
            return LexActivator.ActivateLocalTrial(trialLength);
        }

        public int IsLocalTrialGenuine() {
            return LexActivator.IsLocalTrialGenuine();
        }

        public int ExtendLocalTrial(uint trialExtensionLength) {
            return LexActivator.ExtendLocalTrial(trialExtensionLength);
        }

        public int Reset() {
            return LexActivator.Reset();
        }

        public int SetVersionGUID(string versionGUID, PermissionFlags flags) {
            return LexActivator.SetVersionGUID(versionGUID, (LexActivator.PermissionFlags) flags);
        }

        public int IsProductGenuine() {
            return LexActivator.IsProductGenuine();
        }

        public int GetCustomLicenseField(string fieldId, StringBuilder fieldValue, int length) {
            return LexActivator.GetCustomLicenseField(fieldId, fieldValue, length);
        }

        public int GetProductKey(StringBuilder productKey, int length) {
            return LexActivator.GetProductKey(productKey, length);
        }

        public int SetProductFileV2(string filePath) {
            return LexActivator.SetProductFileV2(filePath);
        }

        public int DeactivateProductV2() {
            return LexActivator.DeactivateProductV2();
        }
    }
}
#endif