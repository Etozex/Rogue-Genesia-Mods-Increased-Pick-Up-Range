using ModGenesia;
using RogueGenesia.Data;
using RogueGenesia.GameManager;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace MaxLootRange
{
    public class MaxLootRangeMod : RogueGenesiaMod
    {
        private const string MOD_ID = "MaxLootRange";
        private const string OPTION_NAME = "pickup_range";
        private const string PREFS_KEY = "MaxLootRange_PickupValue";
        private float currentRange = 2.5f; // Default normal range
        private const float DEFAULT_RANGE = 2.5f;

        public override void OnModLoaded(ModData modData)
        {
            // Load settings from PlayerPrefs
            LoadSettings();

            // Create the slider option
            AddRangeSlider();

            // Add event listeners
            RegisterEventHandlers();
        }

        private void AddRangeSlider()
        {
            var nameLocalization = new LocalizationDataList() 
            { 
                localization = new List<LocalizationData>() 
                { 
                    new LocalizationData() { Key = "en", Value = "Pickup Range" } 
                } 
            };

            var tooltipLocalization = new LocalizationDataList() 
            { 
                localization = new List<LocalizationData>() 
                { 
                    new LocalizationData() { Key = "en", Value = "Adjust how far you can pick up items (2.5 = normal, higher = further)" } 
                } 
            };

            var sliderOption = ModOption.MakeSliderDisplayValueOption(
                OPTION_NAME,
                nameLocalization,
                DEFAULT_RANGE,  // min value (normal range)
                100f,  // max value
                currentRange,  // Use loaded value
                65,   // steps
                false, // don't show as percentage
                tooltipLocalization
            );

            ModOption.AddModOption(sliderOption, "Gameplay Options", "Pickup Range");
        }

        private void RegisterEventHandlers()
        {
            GameEventManager.OnGameStart.AddListener(OnGameStart);
            GameEventManager.OnPostStatsUpdate.AddListener(OnPostStatsUpdate);
            GameEventManager.OnOptionChanged.AddListener(OnOptionChanged);
            GameEventManager.OnRunLoad.AddListener(OnRunLoad);
        }

        private void LoadSettings()
        {
            try
            {
                if (PlayerPrefs.HasKey(PREFS_KEY))
                {
                    currentRange = PlayerPrefs.GetFloat(PREFS_KEY);
                }
                else
                {
                    currentRange = DEFAULT_RANGE;
                }
            }
            catch
            {
                currentRange = DEFAULT_RANGE;
            }
        }

        private void SaveSettings()
        {
            try
            {
                PlayerPrefs.SetFloat(PREFS_KEY, currentRange);
                PlayerPrefs.Save();
            }
            catch (Exception) { /* Silent fail, not critical */ }
        }

        private void OnOptionChanged(string optionName, float value)
        {
            if (optionName == OPTION_NAME)
            {
                currentRange = value;
                SaveSettings();
                ApplyPickupRange();
            }
        }

        private void OnGameStart()
        {
            ApplyPickupRange();
        }
        
        private void OnRunLoad()
        {
            ApplyPickupRange();
        }

        private void OnPostStatsUpdate(AvatarData avatarData, PlayerStats playerStats, bool isBaseStats)
        {
            ApplyPickupRange();
        }

        private void ApplyPickupRange()
        {
            if (GameData.PlayerDatabase != null && GameData.PlayerDatabase.Count > 0)
            {
                var player = GameData.PlayerDatabase[0];
                if (player != null)
                {
                    if (player._basePlayerStats?.PickUpDistance != null)
                    {
                        player._basePlayerStats.PickUpDistance.SetDefaultBaseStat((double)currentRange);
                    }
                    
                    if (player._playerStats?.PickUpDistance != null)
                    {
                        player._playerStats.PickUpDistance.SetDefaultBaseStat((double)currentRange);
                    }
                }
            }
        }

        public override void OnModUnloaded()
        {
            // Save settings
            SaveSettings();
            
            // Remove event listeners
            GameEventManager.OnGameStart.RemoveListener(OnGameStart);
            GameEventManager.OnPostStatsUpdate.RemoveListener(OnPostStatsUpdate);
            GameEventManager.OnOptionChanged.RemoveListener(OnOptionChanged);
            GameEventManager.OnRunLoad.RemoveListener(OnRunLoad);
            
            // Reset pickup range
            if (GameData.PlayerDatabase != null && GameData.PlayerDatabase.Count > 0)
            {
                var player = GameData.PlayerDatabase[0];
                if (player != null)
                {
                    if (player._basePlayerStats?.PickUpDistance != null)
                    {
                        player._basePlayerStats.PickUpDistance.SetDefaultBaseStat((double)DEFAULT_RANGE);
                    }
                    
                    if (player._playerStats?.PickUpDistance != null)
                    {
                        player._playerStats.PickUpDistance.SetDefaultBaseStat((double)DEFAULT_RANGE);
                    }
                }
            }
        }
    }
} 