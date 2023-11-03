// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System.Security.Cryptography;

/// <summary>
/// Generates a random string.
/// </summary>
public class RandomStringBuilder
{
    private const string LowerCaseLetters = "abcdefghjklmnopqrstuvwxyz";
    private const string CapitalLetters = "ABCDEFGHIJKLMNPQRSTUVWXY";
    private const string Numbers = "0123456789";
    private const string Alphabet = $"{LowerCaseLetters}{CapitalLetters}";
    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    /// <summary>
    /// The type of characters to use when generating a random string.
    /// </summary>
    public enum StringType
    {
        /// <summary>
        /// Only use alphabetical characters.
        /// </summary>
        Alphabetic,

        /// <summary>
        /// Only use numeric characters.
        /// </summary>
        Numeric,

        /// <summary>
        /// Use both alphabetical and numeric characters.
        /// </summary>
        Alphanumeric
    }

    /// <summary>
    /// Generates a random string.
    /// </summary>
    /// <param name="length">Length of the random string to generate.</param>
    /// <param name="type">Optional type of characters to use. Defaults to alphabetical.</param>
    /// <returns>Randomly generated string.</returns>
    public static string Generate(int length, StringType type = StringType.Alphabetic)
    {
        string characterPool = type switch
        {
            StringType.Numeric => Numbers,
            StringType.Alphanumeric => Alphabet + Numbers,
            _ => Alphabet
        };

        char[] output = new char[length];

        for (int i = 0; i < length; i++)
        {
            output[i] = characterPool[Next(0, characterPool.Length)];
        }

        return new string(output);
    }

    /// <summary>
    /// Generates a random integer within the specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number to be generated.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to minValue.</param>
    /// <returns>A random integer between minValue and maxValue; minValue is inclusive and maxValue is exclusive.</returns>
    private static int Next(int minValue, int maxValue)
    {
        // Generates a random integer within the given range
        byte[] randomBuffer = new byte[4];
        rng.GetBytes(randomBuffer);
        int result = BitConverter.ToInt32(randomBuffer, 0);

        return new Random(result).Next(minValue, maxValue);
    }
}
