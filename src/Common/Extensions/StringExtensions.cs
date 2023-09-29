// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System.Security;

/// <summary>
/// Extension methods for the <see cref="string"/> type.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Changes the first character in the provided <see cref="string"/> to lowercase.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string UncapitalizeFirstChar(this string value)
    {
        if(string.IsNullOrWhiteSpace(value) || char.IsLower(value, 0))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    /// <summary>
    /// Convert a secure string to a plain string
    /// </summary>
    /// <param name="sstr"></param>
    /// <returns></returns>
    public static string ToPlainString(this SecureString sstr)
    {
        IntPtr valuePtr = IntPtr.Zero;
        try
        {
            valuePtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(sstr);
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(valuePtr);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
        }
    }
}
