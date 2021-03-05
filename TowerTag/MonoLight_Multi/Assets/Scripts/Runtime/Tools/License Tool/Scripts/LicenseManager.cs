using System;
using System.IO;
using System.Linq;
using System.Text;
#if !UNITY_ANDROID
using Cryptlex;
#endif
using GameManagement;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class LicenseManager : Logger {
#region Serialized Fields

    [Header("General Settings")] [SerializeField, Tooltip("The length of a product key")]
    private int _productKeyLength = 50;

#if !UNITY_ANDROID
    [FormerlySerializedAs("_productGUID")]
    [Space, Header("CryptLex Settings")]
    [Tooltip("The Product Version GUIDs from the CryptLex Dashboard Version Page")]
    public CryptlexProduct[] _cryptlexProducts;


    /// <summary>
    /// The maximum extra activation data string length as defined here:
    /// https://cryptlex.com/help/extra-data
    /// </summary>
    private const int ExtraActivationDataLength = 256;

    [Tooltip("The path where the product files are located _inside_ the StreamingAssets folder")]
    public string productFilesPath = "Product Files";

    public string dllExceptionLogMessage =
        "Couldn't load product versions. The LexActivator.dll has probably an exception because Microsoft Visual C++ 2010 Redistributable Package (x64) is not installed";

    public string dllExceptionOverlayMessage =
        "Couldn't access system library, are you missing Microsoft Visual C++ 2010 Redistributable Package (x64)?";

    private ICryptlexService _cryptlexService;
#endif
    private ISceneService _sceneService;
    private IMessageQueueService _overlayMessageQueue;

#if !UNITY_ANDROID
    [Serializable]
    public struct CryptlexProduct {
        public string FileName;
        public string GUID;
        public CryptlexVersion CryptlexVersion;
    }


    public enum CryptlexVersion {
        V2,
        V3
    }
#endif

#endregion

    private new void Awake() {
        base.Awake();
        Init();
    }

    public void Init() {
#if !UNITY_ANDROID
        _cryptlexService = ServiceProvider.Get<ICryptlexService>();
#endif
        _sceneService = ServiceProvider.Get<ISceneService>();
        _overlayMessageQueue = ServiceProvider.Get<IMessageQueueService>();
    }

    private void Start()
    {
#if !UNITY_ANDROID
        productFilesPath = Path.Combine(Application.streamingAssetsPath, productFilesPath);
#endif
		if (TowerTagSettings.Hologate)
        {
            Log("Hologate Intro playing");
            return;
        }

        if(IsVersionManagedByLicense())
            CheckLicense();
        else
        {
            enabled = false;
        }
	}

    private bool IsVersionManagedByLicense()
    {
        return !TowerTagSettings.Home && !TowerTagSettings.BasicMode && !TowerTagSettings.Hologate;
    }

    public void CheckLicense() {
#if !UNITY_ANDROID
        try {
            if (_cryptlexProducts.Any(IsProductGenuine)) {
                _sceneService.LoadConnectScene(!BalancingConfiguration.Singleton.AutoStart);
            }
        }
        catch (DllNotFoundException e) {
            LogError("Exception: " + e);
            LogError(dllExceptionLogMessage);
            _overlayMessageQueue.AddErrorMessage(dllExceptionOverlayMessage);
        }
#else
        _sceneService.LoadConnectScene(!BalancingConfiguration.Singleton.AutoStart);
#endif
    }

#if !UNITY_ANDROID
    /// <summary>
    /// This method has to be called before every other LexActivator function
    /// </summary>
    /// <param name="cryptlexProduct"></param>
    private bool SetProductFileAndVersionGUIDv2(CryptlexProduct cryptlexProduct) {
        if (cryptlexProduct.CryptlexVersion != CryptlexVersion.V2)
            throw new Exception($"Wrong cryptlex version {cryptlexProduct.CryptlexVersion}");

        // Set the Product File coming from CryptLex Dashboard for this Product
        string productFileName = Path.Combine(productFilesPath, cryptlexProduct.FileName);
        Log($"Setting product file to {productFileName}");
        int status = _cryptlexService.SetProductFileV2(productFileName);
        if (status != StatusCodesV2.LA_OK) {
            LogError("Error setting product file!");
            StatusCodeToDebugLog(status, CryptlexVersion.V2);
            return false;
        }

        // Set the GUID Version coming from the CryptLex Dashboard for this Product
        string guid = cryptlexProduct.GUID;
        Log($"Setting GUID to {guid}");
        status = _cryptlexService.SetVersionGUID(guid, PermissionFlags.LA_USER);
        if (status != StatusCodesV2.LA_OK) {
            LogError("Error setting version GUID!");
            StatusCodeToDebugLog(status, CryptlexVersion.V2);
            return false;
        }

        return true;
    }


    /// <summary>
    /// This method has to be called before every other LexActivator function
    /// </summary>
    /// <param name="cryptlexProduct"></param>
    private bool SetProductFileAndVersionGUIDv3(CryptlexProduct cryptlexProduct) {
        if (cryptlexProduct.CryptlexVersion != CryptlexVersion.V3)
            throw new Exception($"Wrong cryptlex version {cryptlexProduct.CryptlexVersion}");

        // Set the Product File coming from CryptLex Dashboard for this Product
        string productFileName = Path.Combine(productFilesPath, cryptlexProduct.FileName);
        Log($"Setting product file to {productFileName}");
        int status = _cryptlexService.SetProductFile(productFileName);
        if (status != StatusCodes.LA_OK) {
            LogError("Error setting product file!");
            StatusCodeToDebugLog(status, CryptlexVersion.V3);
            return false;
        }

        // Set the GUID Version coming from the CryptLex Dashboard for this Product
        string guid = cryptlexProduct.GUID;
        Log($"Setting GUID to {guid}");
        status = _cryptlexService.SetProductId(guid, PermissionFlags.LA_USER);
        if (status != StatusCodes.LA_OK) {
            LogError("Error setting version GUID!");
            StatusCodeToDebugLog(status, CryptlexVersion.V3);
            return false;
        }

        return true;
    }

    private bool IsProductGenuine(CryptlexProduct cryptlexProduct) {
        switch (cryptlexProduct.CryptlexVersion) {
            case CryptlexVersion.V2:
                return IsProductGenuineV2(cryptlexProduct);
            case CryptlexVersion.V3:
                return IsProductGenuineV3(cryptlexProduct);
            default:
                throw new Exception($"Unknown Cryptlex version {cryptlexProduct.CryptlexVersion}");
        }
    }

    private bool IsProductGenuineV2(CryptlexProduct cryptlexProduct) {
        if (cryptlexProduct.CryptlexVersion != CryptlexVersion.V2)
            throw new Exception($"Wrong cryptlex version {cryptlexProduct.CryptlexVersion}");

        Log("Checking License for Version 2");
        if (!SetProductFileAndVersionGUIDv2(cryptlexProduct))
            return false;

        var keyBuilder = new StringBuilder(_productKeyLength);
        int status = _cryptlexService.IsProductGenuine();
        _cryptlexService.GetProductKey(keyBuilder, keyBuilder.Capacity);

        if (status == StatusCodesV2.LA_OK) {
            Debug.Log($"Validated license key {keyBuilder} for Cryptlex V2");
            MigrateActivation(keyBuilder.ToString()); // todo I think this should not happen within "IsProductGenuine()"
            return true;
        }

        if (status == StatusCodesV2.LA_EXPIRED) {
            StatusCodeToOverlayMessage(status, cryptlexProduct.CryptlexVersion);
            StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
            return false;
        }

        if (status == StatusCodesV2.LA_GP_OVER) {
            StatusCodeToOverlayMessage(status, cryptlexProduct.CryptlexVersion);
            StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
            return false;
        }

        LogWarning($"Can't find a fitting activation for the key: {keyBuilder} " +
                   $"on product: {cryptlexProduct.FileName}");
        StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
        return false;
    }

    private bool IsProductGenuineV3(CryptlexProduct cryptlexProduct) {
        if (cryptlexProduct.CryptlexVersion != CryptlexVersion.V3)
            throw new Exception($"Wrong cryptlex version {cryptlexProduct.CryptlexVersion}");

        Log("Checking License for Version 3");
        if (!SetProductFileAndVersionGUIDv3(cryptlexProduct))
            return false;

//        Debug.Log($"Registering callback on thread {Thread.CurrentThread.ManagedThreadId}");
//        int callbackStatus = _cryptlexService.SetLicenseCallback(LicenseCallback);
//        if(callbackStatus != StatusCodes.LA_OK)
//            StatusCodeToDebugLog(callbackStatus, CryptlexVersion.V3);

        int status = _cryptlexService.IsLicenseGenuine();

        if (status == StatusCodes.LA_OK) {
            // When we can't access the metadata of the license (maybe wrong product id)
            // CheckLicenseMetadataCustomization();

            int daysLeft = GetRemainingDaysForLicenseV3();

            if (daysLeft > 0 && daysLeft <= 3) {
                string text = "License activated, " + daysLeft;
                if (daysLeft == 1) {
                    text += " day left";
                }
                else {
                    text += " days left";
                }

                _overlayMessageQueue.AddVolatileButtonMessage(text);
            }

            Debug.Log("Validated license on Cryptlex V3");
            return true;
        }

        if (status == StatusCodes.LA_EXPIRED) {
            StatusCodeToOverlayMessage(status, cryptlexProduct.CryptlexVersion);
            StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
            return false;
        }

        if (status == StatusCodes.LA_GRACE_PERIOD_OVER) {
            Debug.Log("Grace period is over, try to activate license again");
            return ActivateLicense(cryptlexProduct, GetProductKey(), GetEmailAddress(cryptlexProduct.CryptlexVersion),
                FindObjectOfType<LicenseController>().OnActivationSuccessful);
        }

        LogWarning("Can't find an fitting activation");
        StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
        return false;
    }

    //--------------------Delete when migration is a while ago---------------------
    /// <summary>
    /// check if the old license is already migrated to v3 or not
    /// </summary>
    /// <param name="key">The license key</param>
    private void MigrateActivation(string key) {
        Debug.LogWarning("Trying to migrate Cryptlex V2 activation to Cryptlex V3");
        if (_cryptlexProducts
            .Where(product => product.CryptlexVersion == CryptlexVersion.V3)
            .Any(product => ActivateLicense(product, key,
                GetEmailAddress(CryptlexVersion.V2),
                FindObjectOfType<LicenseController>().OnActivationSuccessful)))
            DeactivateProductV2();
        else {
            _overlayMessageQueue.AddErrorMessage(
                "We changed our license tool! Something went wrong with the migration of your license, please contact as us as soon as possible!");
        }
    }
    //------------------------------------------------------------------------------

    private int GetRemainingDaysForLicenseV3() {
        uint expiryDate = 0;
        _cryptlexService.GetLicenseExpiryDate(ref expiryDate);
        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentEpochTime = (int) (DateTime.UtcNow - epochStart).TotalSeconds;

        return ((int) expiryDate - currentEpochTime) / 86500;
    }

    public bool ActivateLicense(CryptlexProduct cryptlexProduct, string productKey, string email,
        Action successCallback) {
        if (cryptlexProduct.CryptlexVersion != CryptlexVersion.V3) {
            Debug.LogError(
                $"Tried to activate license for cryptlex version {cryptlexProduct.CryptlexVersion}. This is not supported.");
            return false;
        }

        if (!SetProductFileAndVersionGUIDv3(cryptlexProduct) || !SetProductKeyV3(productKey))
            return false;

        // Set Extra Activation Data
        Log("Setting Activation Meta Data, Device Name: " + SystemInfo.deviceName + ", Device Unique Identifier: " +
            SystemInfo.deviceUniqueIdentifier);
        _cryptlexService.SetActivationMetadata(SystemInfo.deviceName, SystemInfo.deviceUniqueIdentifier);

        Log("Trying to activate the product");
        int status = _cryptlexService.ActivateLicense();
        StatusCodeToDebugLog(status, cryptlexProduct.CryptlexVersion);
        if (status == StatusCodes.LA_OK) {
            // License is activated, check if the user has also entered the right Email Address - you just can check the email when you already activated the license!
            bool correctEmail = CheckEmailAddressV3(email);
            if (correctEmail) {
                Log("Activation Successful");
                AnalyticsController.LicenseActivation(email, productKey, cryptlexProduct.FileName);
                successCallback();
                return true;
            }

            LogWarning("Incorrect Email Address entered");
            _overlayMessageQueue.AddErrorMessage("Incorrect Email Address entered");
            DeactivateProductV3();
            return false;
        }

        if (status == StatusCodes.LA_FAIL || status == StatusCodes.LA_E_LICENSE_KEY) {
            Debug.LogWarning("Failed to activate license");
            return false;
        }

        StatusCodeToOverlayMessage(status, cryptlexProduct.CryptlexVersion);
        return false;
    }

    private bool SetProductKeyV3(string productKey) {
        Log("Setting Product Key to: " + productKey);
        int status = _cryptlexService.SetLicenseKey(productKey);
        if (status != StatusCodes.LA_OK) {
            StatusCodeToOverlayMessage(status, CryptlexVersion.V3);
            return false;
        }

        return true;
    }

    /// <summary>
    /// This check is just possible when the license is activated! If false -> Deactivate again!
    /// </summary>
    /// <param name="eMailAddress"></param>
    /// <returns></returns>
    private bool CheckEmailAddressV3(string eMailAddress) {
        var fieldDataBuilder = new StringBuilder(ExtraActivationDataLength);
        _cryptlexService.GetLicenseUserEmail(fieldDataBuilder, fieldDataBuilder.Capacity);
        Debug.Log($"Email {eMailAddress} : fieldData {fieldDataBuilder}");
        if (string.Equals(eMailAddress, fieldDataBuilder.ToString(), StringComparison.CurrentCultureIgnoreCase)) {
            Log("The Email address matches the registrated Email address");
            return true;
        }

        LogError($"The Email address {eMailAddress} does not match the registrated Email address {fieldDataBuilder}.");
        return false;
    }

    public bool DeactivateProduct() {
        bool success = DeactivateProductV2() | DeactivateProductV3();
        _cryptlexService.Reset(); // otherwise the local activation stays until a scheduled refresh is performed
        return success;
    }

    private bool DeactivateProductV3() {
        int status = _cryptlexService.DeactivateLicense();
        Log("Trying to deactivate Cryptlex3 license");
        if (status == StatusCodes.LA_OK) {
            Log("Deactivation successful");
            AnalyticsController.LicenseDeactivation();
            return true;
        }

        StatusCodeToOverlayMessage(status, CryptlexVersion.V3);
        StatusCodeToDebugLog(status, CryptlexVersion.V3);
        return false;
    }

    //--------------------Delete when migration is a while ago---------------------

    private bool DeactivateProductV2() {
        Log("Deactivating the old activation!");
        int status = _cryptlexService.DeactivateProductV2();

        if (status == StatusCodesV2.LA_OK) {
            Log("Deactivation successful");
            AnalyticsController.LicenseDeactivation();
            return true;
        }

        StatusCodeToDebugLog(status, CryptlexVersion.V2);
        return false;
    }

    //--------------------------------------------------------------------------------
    private void StatusCodeToDebugLog(int returnCode, CryptlexVersion cryptlexVersion) {
        Log(ReturnCodeToMessage(returnCode, cryptlexVersion));
    }

    private void StatusCodeToOverlayMessage(int returnCode, CryptlexVersion cryptlexVersion) {
        string message = ReturnCodeToMessage(returnCode, cryptlexVersion);

        if (cryptlexVersion == CryptlexVersion.V3 && returnCode == StatusCodes.LA_OK
            || cryptlexVersion == CryptlexVersion.V2 && returnCode == StatusCodesV2.LA_OK) {
            _overlayMessageQueue.AddVolatileMessage(message);
        }
        else {
            _overlayMessageQueue.AddErrorMessage(message);
        }
    }

    [CanBeNull]
    public string GetProductKey() {
        var productKey = new StringBuilder(_productKeyLength);
        int status = _cryptlexService.GetLicenseKey(productKey, productKey.Capacity);
        if (status != StatusCodes.LA_OK) {
            productKey.Clear();
            status = _cryptlexService.GetProductKey(productKey, productKey.Capacity);
        }

        if (status != StatusCodesV2.LA_OK) {
            Debug.LogWarning($"Failed to get product key: {status}");
            return null;
        }

        return productKey.ToString();
    }

    public string GetEmailAddress(CryptlexVersion version) {
        var emailAddressBuilder = new StringBuilder(ExtraActivationDataLength);
        if (version == CryptlexVersion.V2) {
            int status =
                _cryptlexService.GetCustomLicenseField("301", emailAddressBuilder, emailAddressBuilder.Capacity);
            if (status != StatusCodesV2.LA_OK) {
                Debug.LogError("Failed to get email address");
                return null;
            }
        }
        else if (version == CryptlexVersion.V3) {
            int status = _cryptlexService.GetLicenseUserEmail(emailAddressBuilder, emailAddressBuilder.Capacity);
            if (status != StatusCodes.LA_OK) {
                Debug.LogError("Failed to get email address");
                return null;
            }
        }
        else {
            throw new Exception($"Unknown Cryptlex Version {version}");
        }

        return emailAddressBuilder.ToString();
    }

    //To get the meaning of the error code in text form
    private string ReturnCodeToMessage(int returnCode, CryptlexVersion version) {
        if (version == CryptlexVersion.V2) return ReturnCodeToMessageV2(returnCode);
        if (version == CryptlexVersion.V3) return ReturnCodeToMessageV3(returnCode);
        throw new Exception($"Unknown Cryptlex version {version}");
    }

    private string ReturnCodeToMessageV2(int returnCode)
    {
        var message = "";
        switch (returnCode)
        {
            case StatusCodesV2.LA_OK:
                message = "Success!";
                break;
            case StatusCodesV2.LA_FAIL:
                message = "Product key invalid or wrong product version chosen.";
                break;
            case StatusCodesV2.LA_EXPIRED:
                message = "Your product key expired. Please contact us.";
                break;
            case StatusCodesV2.LA_REVOKED:
                message = "The product key has been revoked.";
                break;
            case StatusCodesV2.LA_GP_OVER:
                message = "Couldn't confirm license validity. Please connect the Computer to the Internet.";
                break;
            case StatusCodesV2.LA_E_INET:
                message = "Failed to connect to the server due to network error.";
                break;
            case StatusCodesV2.LA_E_PKEY:
                message = "Invalid product key.";
                break;
            case StatusCodesV2.LA_E_PFILE:
                message = "Invalid or corrupted product file.";
                break;
            case StatusCodesV2.LA_E_FPATH:
                message = "Invalid product file path.";
                break;
            case StatusCodesV2.LA_E_GUID:
                message = "The version GUID doesn't match that of the product file.";
                break;
            case StatusCodesV2.LA_E_OFILE:
                message = "Invalid offline activation response file.";
                break;
            case StatusCodesV2.LA_E_PERMISSION:
                message = "Insufficient system permissions";
                break;
            case StatusCodesV2.LA_E_EDATA_LEN:
                message = "Extra activation data length is more than 256 characters.";
                break;
            case StatusCodesV2.LA_E_TKEY:
                message = "The trial key doesn't match that of the product file.";
                break;
            case StatusCodesV2.LA_E_TIME:
                message = "The system time has been tampered with. Ensure your date and time settings are correct.";
                break;
            case StatusCodesV2.LA_E_VM:
                message =
                    "Application is being run inside a virtual machine / hyper visor, and activation has been disallowed in the VM.";
                break;
            case StatusCodesV2.LA_E_WMIC:
                message =
                    "Fingerprint couldn't be generated because Windows Management Instrumentation (WMI) service has been disabled. This error is specific to Windows only.";
                break;
            case StatusCodesV2.LA_E_TEXT_KEY:
                message = "Invalid trial extension key";
                break;
            case StatusCodesV2.LA_E_TRIAL_LEN:
                message = "The trial length doesn't match that of the product file.";
                break;
            case StatusCodesV2.LA_T_EXPIRED:
                message =
                    "The trial has expired or system time has been tampered with. Ensure your date and time settings are correct.";
                break;
            case StatusCodesV2.LA_TEXT_EXPIRED:
                message =
                    "The trial extension key being used has already expired or system time has been tampered with. Ensure your date and time settings are correct.";
                break;
            case StatusCodesV2.LA_E_BUFFER_SIZE:
                message = "The buffer size was smaller than required.";
                break;
            case StatusCodesV2.LA_E_CUSTOM_FIELD_ID:
                message = "Invalid custom field id.";
                break;
            case StatusCodesV2.LA_E_NET_PROXY:
                message = "Invalid network proxy.";
                break;
            case StatusCodesV2.LA_E_HOST_URL:
                message = "Invalid Cryptlex host url.";
                break;
            case StatusCodesV2.LA_E_DEACT_LIMIT:
                message = "Deactivation limit for key reached.";
                break;
            case StatusCodesV2.LA_E_ACT_LIMIT:
                message = "Activation limit for key reached.";
                break;
        }

        return message + "\nReturnCode: " + returnCode;
    }

    private string ReturnCodeToMessageV3(int returnCode) {
        var message = "";
        // todo actually non-sense for V2, because these codes are V3 codes. Should have version-dependent code-to-message dictionaries
        switch (returnCode) {
            case StatusCodes.LA_OK:
                message = "Success!";
                break;
            case StatusCodes.LA_FAIL:
                message = "Failure!";
                break;
            case StatusCodes.LA_EXPIRED:
                message = "The license has expired!";
                break;
            case StatusCodes.LA_SUSPENDED:
                message = "The license has been suspended!";
                break;
            case StatusCodes.LA_GRACE_PERIOD_OVER:
                message = "The grace period for server sync is over!";
                break;
            case StatusCodes.LA_TRIAL_EXPIRED:
                message = "The trial has expired or system time has been tampered with!";
                break;
            case StatusCodes.LA_LOCAL_TRIAL_EXPIRED:
                message = "The local trial has expired or system time has been tampered with!";
                break;
            case StatusCodes.LA_E_FILE_PATH:
                message = "Invalid file path! Please contact us";
                break;
            case StatusCodes.LA_E_PRODUCT_FILE:
                message = "Invalid or corrupted product file! Please contact us";
                break;
            case StatusCodes.LA_E_PRODUCT_DATA:
                message = "Invalid product data";
                break;
            case StatusCodes.LA_E_PRODUCT_ID:
                message = "The product id is incorrect";
                break;
            case StatusCodes.LA_E_SYSTEM_PERMISSION:
                message = "Insufficient system permissions";
                break;
            case StatusCodes.LA_E_FILE_PERMISSION:
                message = "No permission to write to file.";
                break;
            case StatusCodes.LA_E_WMIC:
                message =
                    "Fingerprint couldn't be generated because Windows Management Instrumentation(WMI) service has been disabled.";
                break;
            case StatusCodes.LA_E_TIME:
                message =
                    "The difference between the network time and the system time is more than allowed clock offset.";
                break;
            case StatusCodes.LA_E_INET:
                message = "Failed to connect to the server due to network error.";
                break;
            case StatusCodes.LA_E_NET_PROXY:
                message = "Invalid network proxy.";
                break;
            case StatusCodes.LA_E_HOST_URL:
                message = "Invalid Cryptlex host url.";
                break;
            case StatusCodes.LA_E_BUFFER_SIZE:
                message = "The buffer size was smaller than required.";
                break;
            case StatusCodes.LA_E_APP_VERSION_LENGTH:
                message = "App version length is more than 256 characters.";
                break;
            case StatusCodes.LA_E_REVOKED:
                message = "The license has been revoked.";
                break;
            case StatusCodes.LA_E_LICENSE_KEY:
                message = "Invalid license key.";
                break;
            case StatusCodes.LA_E_LICENSE_TYPE:
                message = "Invalid license type.";
                break;
            case StatusCodes.LA_E_OFFLINE_RESPONSE_FILE:
                message = "Invalid offline activation response file.";
                break;
            case StatusCodes.LA_E_OFFLINE_RESPONSE_FILE_EXPIRED:
                message = "The offline activation response has expired.";
                break;
            case StatusCodes.LA_E_ACTIVATION_LIMIT:
                message = "The license has reached it's allowed activations limit.";
                break;
            case StatusCodes.LA_E_ACTIVATION_NOT_FOUND:
                message = "The license activation was deleted on the server.";
                break;
            case StatusCodes.LA_E_DEACTIVATION_LIMIT:
                message = "The license has reached it's allowed deactivation limit.";
                break;
            case StatusCodes.LA_E_TRIAL_NOT_ALLOWED:
                message = "Trial not allowed for the product.";
                break;
            case StatusCodes.LA_E_TRIAL_ACTIVATION_LIMIT:
                message = "Your account has reached it's trial activations limit.";
                break;
            case StatusCodes.LA_E_MACHINE_FINGERPRINT:
                message = "Machine fingerprint has changed since activation.";
                break;
            case StatusCodes.LA_E_METADATA_KEY_LENGTH:
                message = "Metadata key length is more than 256 characters.";
                break;
            case StatusCodes.LA_E_METADATA_VALUE_LENGTH:
                message = "Metadata value length is more than 256 characters.";
                break;
            case StatusCodes.LA_E_ACTIVATION_METADATA_LIMIT:
                message = "The license has reached it's metadata fields limit.";
                break;
            case StatusCodes.LA_E_TRIAL_ACTIVATION_METADATA_LIMIT:
                message = "The trial has reached it's metadata fields limit.";
                break;
            case StatusCodes.LA_E_METADATA_KEY_NOT_FOUND:
                message = "The metadata key does not exist.";
                break;
            case StatusCodes.LA_E_TIME_MODIFIED:
                message = "The system time has been tampered (backdated).";
                break;
            case StatusCodes.LA_E_VM:
                message = "Application is being run inside a virtual machine.";
                break;
            case StatusCodes.LA_E_COUNTRY:
                message = "Country is not allowed.";
                break;
            case StatusCodes.LA_E_IP:
                message = "IP address is not allowed.";
                break;
            case StatusCodes.LA_E_RATE_LIMIT:
                message = "Rate limit for API has reached, try again later.";
                break;
            case StatusCodes.LA_E_SERVER:
                message = "Server error.";
                break;
            case StatusCodes.LA_E_CLIENT:
                message = "Client error.";
                break;
        }

        return message + "\nReturnCode: " + returnCode;
    }
#endif
}