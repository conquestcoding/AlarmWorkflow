﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using AlarmWorkflow.Shared.Core;

namespace AlarmWorkflow.Windows.IlsAnsbachOperationViewer
{
    /// <summary>
    /// Represents the configuration for the Windows UI.
    /// </summary>
    internal sealed class UIConfigurationNea
    {
        #region Properties

        /// <summary>
        /// Gets the abbreviations that must be contained within the resource name.
        /// </summary>
        public string[] VehicleMustContainAbbreviations { get; private set; }
        /// <summary>
        /// Gets the configuration of each vehicle.
        /// </summary>
        public IList<Vehicle> Vehicles { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UIConfigurationNea"/> class.
        /// </summary>
        public UIConfigurationNea()
        {
            Vehicles = new List<Vehicle>();
        }

        #endregion

        #region Methods

        public Vehicle ResourceMatches(string resourceName)
        {
            return Vehicles.FirstOrDefault(v => resourceName.ToUpperInvariant().Contains(v.Identifier));
        }

        /// <summary>
        /// Loads the UIConfigurationNea from its default path.
        /// </summary>
        /// <returns></returns>
        public static UIConfigurationNea Load()
        {
            string configFile = Path.Combine(Utilities.GetWorkingDirectory(Assembly.GetExecutingAssembly()), "Config\\IlsAnsbachNeaOperationViewerConfig.xml");
            if (configFile == null)
            {
                return null;
            }

            UIConfigurationNea configuration = new UIConfigurationNea();

            XDocument doc = XDocument.Load(configFile);

            XElement vehicleE = doc.Root.Element("Vehicles");
            configuration.VehicleMustContainAbbreviations = vehicleE.Attribute("MustContainAbbreviations").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (XElement resE in vehicleE.Elements("Vehicle"))
            {
                Vehicle vehicle = new Vehicle();
                vehicle.Identifier = resE.Attribute("Identifier").Value;
                vehicle.Name = resE.Attribute("Name").Value;

                // Try to parse shortkey
                string shortkeyS = resE.TryGetAttributeValue("Shortkey", null);
                if (!string.IsNullOrWhiteSpace(shortkeyS))
                {
                    Key shortkey = Key.None;
                    Enum.TryParse<Key>(shortkeyS, true, out shortkey);
                    vehicle.Shortkey = shortkey;
                }

                FileInfo imageFile = new FileInfo(Path.Combine(Utilities.GetWorkingDirectory(Assembly.GetExecutingAssembly()), resE.Attribute("Image").Value));
                vehicle.Image = imageFile.FullName;

                configuration.Vehicles.Add(vehicle);
            }

            return configuration;
        }

        #endregion

        #region Nested types

        /// <summary>
        /// Provides detail for each of the fire department's vehicles.
        /// </summary>
        public class Vehicle
        {
            /// <summary>
            /// Gets/sets the identifier of this vehicle as it appears in the alarmfax.
            /// </summary>
            public string Identifier { get; set; }
            /// <summary>
            /// Gets/sets the display name of the vehicle, if it differs from the <see cref="Identifier"/>-name.
            /// If this is null or empty, the name from <see cref="Identifier"/> is used in the UI.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Gets/sets the path to the image that is used for this vehicle in the UI.
            /// </summary>
            public string Image { get; set; }
            /// <summary>
            /// Gets/sets the shortkey on the keyboard associated with this vehicle.
            /// </summary>
            public Key Shortkey { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Vehicle"/> class.
            /// </summary>
            public Vehicle()
            {

            }
        }

        #endregion
    }
}