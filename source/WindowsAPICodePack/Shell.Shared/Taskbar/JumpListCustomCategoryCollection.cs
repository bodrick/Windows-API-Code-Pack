﻿//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.WindowsAPICodePack.Taskbar
{
    /// <summary>
    /// Represents a collection of custom categories
    /// </summary>
    internal class JumpListCustomCategoryCollection : ICollection<JumpListCustomCategory>, INotifyCollectionChanged
    {
        private readonly List<JumpListCustomCategory> categories = new
#if !CS9
            List<JumpListCustomCategory>
#endif
            ();

        /// <summary>
        /// Event to trigger anytime this collection is modified
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        /// <summary>
        /// Determines if this collection is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// The number of items in this collection
        /// </summary>
        public int Count => categories.Count;

        /// <summary>
        /// Add the specified category to this collection
        /// </summary>
        /// <param name="category">Category to add</param>
        public void Add(JumpListCustomCategory category)
        {
            categories.Add(category ?? throw new ArgumentNullException(nameof(category)));

            // Trigger CollectionChanged event
            CollectionChanged(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    category));

            // Make sure that a collection changed event is fire if this category
            // or it's corresponding jumplist is modified
            category.CollectionChanged += CollectionChanged;
            category.JumpListItems.CollectionChanged += CollectionChanged;
        }

        /// <summary>
        /// Remove the specified category from this collection
        /// </summary>
        /// <param name="category">Category item to remove</param>
        /// <returns>True if item was removed.</returns>
        public bool Remove(JumpListCustomCategory category)
        {
            bool removed = categories.Remove(category);

            if (removed == true)
            
                // Trigger CollectionChanged event
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        0));
            
            return removed;
        }

        /// <summary>
        /// Clear all items from the collection
        /// </summary>
        public void Clear()
        {
            categories.Clear();

            CollectionChanged(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determine if this collection contains the specified item
        /// </summary>
        /// <param name="category">Category to search for</param>
        /// <returns>True if category was found</returns>
        public bool Contains(JumpListCustomCategory category) => categories.Contains(category);

        /// <summary>
        /// Copy this collection to a compatible one-dimensional array,
        /// starting at the specified index of the target array
        /// </summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="index">Index of target array to start copy</param>
        public void CopyTo(JumpListCustomCategory[] array, int index) => categories.CopyTo(array, index);

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>Enumerator to iterate through this collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => categories.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>Enumerator to iterate through this collection.</returns>
        IEnumerator<JumpListCustomCategory> IEnumerable<JumpListCustomCategory>.GetEnumerator() => categories.GetEnumerator();
    }
}
