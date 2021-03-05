using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class RegionDropdown : MonoBehaviour {
    private Dropdown _dropdown;
    private Configuration _configuration;
    private readonly string[] _regionCodes =
        {"best", "asia", "au", "cae", "cn", "eu", "in", "jp", "ru", "rue", "sa", "kr", "us", "usw"};

    private void Awake() {
        _configuration = ConfigurationManager.Configuration;
        _dropdown = GetComponent<Dropdown>();
        _dropdown.options = _regionCodes.Select(code => new Dropdown.OptionData(code)).ToList();
        _dropdown.value = _regionCodes.Contains(_configuration.PreferredRegion)
            ? Array.IndexOf(_regionCodes, _configuration.PreferredRegion)
            : 0;
    }

    private void OnEnable() {
        _dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable() {
        _dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(int index) {
        _configuration.PreferredRegion = _regionCodes[index];
        ConfigurationManager.WriteConfigToFile();
    }
}