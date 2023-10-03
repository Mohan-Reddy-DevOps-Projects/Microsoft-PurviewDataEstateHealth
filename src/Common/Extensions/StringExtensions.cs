// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System.Security;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Extension methods for the <see cref="string"/> type.
/// </summary>
public static class StringExtensions
{
    private static readonly char[] passwordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
    private static readonly char[] specialChars = "!@#$%^&*()_+".ToCharArray();

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

    /// <summary>
    /// Generate a random string
    /// </summary>
    /// <param name="length">The length of the desired string.</param>
    /// <param name="specialChars">The number of permitted special characters.</param>
    /// <returns></returns>
    public static string RandomString(int length, uint specialChars)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "length must be positive");
        }
        if (specialChars > length)
        {
            throw new ArgumentOutOfRangeException(nameof(specialChars), "The number of special characters must be less than or equal to length");
        }
        byte[] buffer = RandomNumberGenerator.GetBytes(length);
        StringBuilder sb = new(length);
        foreach (byte b in buffer)
        {
            sb.Append(passwordChars[b % passwordChars.Length]);
        }
        // Insert special characters at random positions
        byte[] specialCharPositions = RandomNumberGenerator.GetBytes((int)specialChars);
        for (int i = 0; i < specialChars; i++)
        {
            int position = specialCharPositions[i] % length;
            sb[position] = StringExtensions.specialChars[specialCharPositions[i] % StringExtensions.specialChars.Length];
        }
        return sb.ToString();
    }
    /// <summary>
    /// Extension to turn a string into a secure string
    /// </summary>
    public static SecureString ToSecureString(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        SecureString secureString = new();
        foreach (char s in str)
        {
            secureString.AppendChar(s);
        }
        secureString.MakeReadOnly();
        return secureString;
    }
}
