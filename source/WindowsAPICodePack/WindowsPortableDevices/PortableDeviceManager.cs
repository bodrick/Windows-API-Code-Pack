﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAPICodePack.PortableDevices
{
    public class PortableDeviceManager : IPortableDeviceManager
    {

        internal Microsoft.WindowsAPICodePack.Win32Native.PortableDevices.IPortableDeviceManager _Manager { get; set; } = null;

        internal List<PortableDevice> _portableDevices = null;

        public IReadOnlyCollection<PortableDevice> PortableDevices { get; }

        internal List<PortableDevice> _privatePortableDevices = null;

        public IReadOnlyCollection<PortableDevice> PrivatePortableDevices { get; }

        public PortableDeviceManager()

        {

            _Manager = new Win32Native.PortableDevices.PortableDeviceManager();

            _portableDevices = new List<PortableDevice>();

            PortableDevices = new ReadOnlyCollection<PortableDevice>(_portableDevices);

            _privatePortableDevices = new List<PortableDevice>();

            PrivatePortableDevices = new ReadOnlyCollection<PortableDevice>(_privatePortableDevices);

        }

        public void RefreshDeviceList() => Marshal.ThrowExceptionForHR((int)_Manager.RefreshDeviceList());

        public void GetDevices()

        {

            uint count = 1;

            Marshal.ThrowExceptionForHR((int)_Manager.GetDevices(null, ref count)); // We get the PortableDevices.

            if (count == 0)

            {

                _portableDevices.Clear(); // We found no devices anymore, so we clear the existing PortableDevices.

                return;

            }

            string[] deviceIDs = new string[count];

            Marshal.ThrowExceptionForHR((int)_Manager.GetDevices(deviceIDs, ref count));

            if (count == 0)

            {

                _portableDevices.Clear(); // We found no devices anymore, so we clear the existing PortableDevices.

                return;

            }

            OnUpdatingPortableDevices(deviceIDs, false);

        }

        public void GetPrivateDevices()

        {

            uint count = 1;

            Marshal.ThrowExceptionForHR((int)_Manager.GetPrivateDevices(null, ref count)); // We get the PortableDevices.

            if (count == 0)

            {

                _privatePortableDevices.Clear(); // We found no devices anymore, so we clear the existing PortableDevices.

                return;

            }

            string[] deviceIDs = new string[count];

            Marshal.ThrowExceptionForHR((int)_Manager.GetPrivateDevices(deviceIDs, ref count));

            if (count == 0)

            {

                _privatePortableDevices.Clear(); // We found no devices anymore, so we clear the existing PortableDevices.

                return;

            }

            OnUpdatingPortableDevices(deviceIDs, true);

        }

        protected virtual void OnUpdatingPortableDevices(string[] deviceIDs, in bool privateDevices)

        {

            List<PortableDevice> portableDevices = privateDevices ? _privatePortableDevices : _portableDevices;

            int i = 0;

            _ = portableDevices.RemoveAll(d => !deviceIDs.Contains(d.DeviceId));

            while (deviceIDs.Length > 0)

            {

                if (portableDevices.Any(d => d.DeviceId == deviceIDs[i]))

                    continue;

                OnAddingPortableDevice(deviceIDs[i], privateDevices);

                deviceIDs[i] = null;

                i++;

            }

        }

        protected virtual void OnAddingPortableDevice(in string deviceId, in bool isPrivateDevice) => OnAddingPortableDevice(new PortableDevice(this, deviceId), isPrivateDevice);

        protected virtual void OnAddingPortableDevice(in PortableDevice portableDevice, in bool isPrivateDevice) => (isPrivateDevice ? _privatePortableDevices : _portableDevices).Add(portableDevice);

        #region IDisposable Support
        public bool IsDisposed { get; private set; } = false;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _portableDevices.Clear();
                _privatePortableDevices.Clear();
            }

            _ = Marshal.ReleaseComObject(_Manager);
            _Manager = null;

            IsDisposed = true;

        }

        ~PortableDeviceManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
