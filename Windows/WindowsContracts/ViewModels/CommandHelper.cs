﻿// This file is part of AlarmWorkflow.
// 
// AlarmWorkflow is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AlarmWorkflow is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AlarmWorkflow.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using AlarmWorkflow.Shared.Core;

namespace AlarmWorkflow.Windows.UIContracts.ViewModels
{
    /// <summary>
    /// Provides helper methods for working with commands.
    /// </summary>
    public static class CommandHelper
    {
        #region Constants

        private const string MethodTemplateCanExecute = "{0}_CanExecute";
        private const string MethodTemplateExecute = "{0}_Execute";

        #endregion

        #region Methods

        /// <summary>
        /// Automatically wires up relay commands.
        /// Performs this operation by searching for public properties of type "ICommand" and for methods which have "PROPERTYNAME_Execute" or "PROPERTYNAME_CanExecute".
        /// </summary>
        /// <param name="instance">The instance to wire up.</param>
        public static void WireupRelayCommands(object instance)
        {
            Assertions.AssertNotNull(instance, "instance");

            WireupRelayCommands(instance.GetType(), instance);
        }

        /// <summary>
        /// Automatically wires up relay commands.
        /// Performs this operation by searching for public properties of type "ICommand" and for methods which have "PROPERTYNAME_Execute" or "PROPERTYNAME_CanExecute".
        /// </summary>
        /// <param name="type">The type to retrieve the methods and properties of. This is usually the type from <paramref name="instance"/>.</param>
        /// <param name="instance">The instance to wire up.</param>
        public static void WireupRelayCommands(Type type, object instance)
        {
            Assertions.AssertNotNull(type, "type");
            Assertions.AssertNotNull(instance, "instance");

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(ICommand)))
            {
                Action<object> execute = null;
                Func<object, bool> canExecute = null;

                {
                    MethodInfo methodInfo = type.GetMethod(string.Format(MethodTemplateExecute, property.Name), BindingFlags.Instance | BindingFlags.NonPublic);
                    if (methodInfo == null || methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length != 1)
                    {
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, Properties.Resources.WireupCommandPropertyFailed, property.Name));
                        continue;
                    }

                    execute = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), instance, methodInfo.Name);
                }

                {
                    MethodInfo methodInfo = type.GetMethod(string.Format(MethodTemplateCanExecute, property.Name), BindingFlags.Instance | BindingFlags.NonPublic);
                    if (methodInfo != null && methodInfo.ReturnType == typeof(bool) && methodInfo.GetParameters().Length == 1)
                    {
                        canExecute = (Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), instance, methodInfo.Name);
                    }
                }

                property.SetValue(instance, new RelayCommand(execute, canExecute), null);
            }
        }

        /// <summary>
        /// Un-wires all previously wired up commands (by <see cref="WireupRelayCommands(object)"/>) so they can be garbage-collected.
        /// </summary>
        /// <param name="instance">The instance with wired-up commands.</param>
        public static void UnwireRelayCommands(object instance)
        {
            Assertions.AssertNotNull(instance, "instance");

            foreach (PropertyInfo property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(ICommand)))
            {
                RelayCommand value = property.GetValue(instance, null) as RelayCommand;
                if (value == null)
                {
                    continue;
                }

                property.SetValue(instance, null, null);
            }
        }

        #endregion
    }
}