﻿//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using System.Collections.Generic;

namespace Microsoft.WindowsAPICodePack.Sensors
{
    /// <summary>
    /// Defines a strongly typed list of sensors.
    /// </summary>
    /// <typeparam name="TSensor">The type of sensor in the list.</typeparam>        
    public class SensorList<TSensor> : IList<TSensor> where TSensor : Sensor
    {
        private readonly List<TSensor> sensorList;

        public SensorList() => sensorList = new List<TSensor>();

        public SensorList(in int capacity) => sensorList = new List<TSensor>(capacity);

        public SensorList(in IEnumerable<TSensor> collection) => sensorList = new List<TSensor>(collection);

        #region IList<S> Members

        /// <summary>
        /// Returns a sensor at a particular index.
        /// </summary>
        /// <param name="item">The sensor item.</param>
        /// <returns>The index of the sensor.</returns>
        public int IndexOf(TSensor item) => sensorList.IndexOf(item);

        /// <summary>
        /// Inserts a sensor at a specific location in the list.
        /// </summary>
        /// <param name="index">The index to insert the sensor.</param>
        /// <param name="item">The sensor to insert.</param>
        public void Insert(int index, TSensor item) => sensorList.Insert(index, item);

        /// <summary>
        /// Removes a sensor at a specific location in the list.
        /// </summary>
        /// <param name="index">The index of the sensor to remove.</param>
        public void RemoveAt(int index) => sensorList.RemoveAt(index);

        /// <summary>
        /// Gets or sets a sensor at a specified location in the list.
        /// </summary>
        /// <param name="index">The index of the sensor in the list.</param>
        /// <returns>The sensor.</returns>
        public TSensor this[int index]
        {
            get => sensorList[index];
            set => sensorList[index] = value;
        }

        #endregion

        #region System.Collections.Generic.ICollection<S> Members

        /// <summary>
        /// Adds a sensor to the end of the list.
        /// </summary>
        /// <param name="item">The sensor item.</param>
        public void Add(TSensor item) => sensorList.Add(item);

        /// <summary>
        /// Clears the list of sensors.
        /// </summary>
        public void Clear() => sensorList.Clear();

        /// <summary>
        /// Determines if the list contains a specified sensor.
        /// </summary>
        /// <param name="item">The sensor to locate.</param>
        /// <returns><b>true</b> if the list contains the sensor; otherwise <b>false</b>.</returns>
        public bool Contains(TSensor item) => sensorList.Contains(item);

        /// <summary>
        /// Copies a sensor to a new list.
        /// </summary>
        /// <param name="array">The destination list.</param>
        /// <param name="arrayIndex">The index of the item to copy.</param>
        public void CopyTo(TSensor[] array, int arrayIndex) => sensorList.CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public int Count => sensorList.Count;

        /// <summary>
        /// Gets a value that determines if the list is read-only.
        /// </summary>
        public bool IsReadOnly => (sensorList as System.Collections.Generic.ICollection<TSensor>).IsReadOnly;

        /// <summary>
        /// Removes a specific sensor from the list.
        /// </summary>
        /// <param name="item">The sensor to remove.</param>
        /// <returns><b>true</b> if the sensor was removed from the list; otherwise <b>false</b>.</returns>
        public bool Remove(TSensor item) => sensorList.Remove(item);

        #endregion

        #region IEnumerable<S> Members

        /// <summary>
        /// Returns an enumerator for the list.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<TSensor> GetEnumerator() => sensorList.GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator for the list.
        /// </summary>
        /// <returns>An enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => sensorList.GetEnumerator();

        #endregion
    }
}
