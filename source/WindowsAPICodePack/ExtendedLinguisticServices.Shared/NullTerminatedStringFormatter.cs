//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using System;
using System.Text;

namespace Microsoft.WindowsAPICodePack.ExtendedLinguisticServices
{

    /// <summary>
    /// Converts byte arrays containing Unicode null-terminated strings into .NET string objects.
    /// </summary>
    public class NullTerminatedStringFormatter : IMappingFormatter<string>
    {
        /// <summary>
        /// Converts a single <see cref="MappingDataRange">MappingDataRange</see> into a string, stripping the trailing null character.
        /// If the string doesn't contain null characters, the empty string is returned.
        /// </summary>
        /// <param name="dataRange">The <see cref="MappingDataRange">MappingDataRange</see> to convert</param>
        /// <returns>The resulting string</returns>
        public string Format(in MappingDataRange dataRange)
        {       
            byte[] data = (dataRange ?? throw new ArgumentNullException(nameof(dataRange))).GetData();

            if ((data.Length & 1) != 0)
            
                throw new LinguisticException(NativeAPI.Consts.ExtendedLinguisticServices.InvalidArgs);

            int nullIndex = data.Length;

            for (int i = 0; i < data.Length; i += 2)
            
                if (data[i] == 0 && data[i + 1] == 0)
                {
                    nullIndex = i;
                    break;
                }
            
            return Encoding.Unicode.GetString(data, 0, nullIndex);
        }

        /// <summary>
        /// Uses <see cref="Format(MappingDataRange)">Format</see> to format all the ranges of the supplied
        /// <see cref="MappingPropertyBag">MappingPropertyBag</see>.
        /// </summary>
        /// <param name="bag">The property bag to convert.</param>
        /// <returns>An array of strings, one per <see cref="MappingDataRange">MappingDataRange</see>.</returns>
        public string[] FormatAll(in MappingPropertyBag bag)
        {
            if (bag == null)

                throw new ArgumentNullException(nameof(bag));

            MappingDataRange[] dataRanges = bag .GetResultRanges();
            string[] results = new string[dataRanges.Length];

            for (int i = 0; i < results.Length; ++i)
            
                results[i] = Format(dataRanges[i]);
            
            return results;
        }
    }

}
