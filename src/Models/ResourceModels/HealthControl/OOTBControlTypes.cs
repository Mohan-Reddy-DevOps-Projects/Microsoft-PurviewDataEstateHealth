// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

/// <summary>
/// Out of the box control types
/// </summary>
public class OOTBControlTypes
{
    private readonly OOTBControlTypesEnum value;

    private OOTBControlTypes(OOTBControlTypesEnum value)
    {
        this.value = value;
    }

    /// <summary>
    /// Data governance control
    /// </summary>
    public static OOTBControlTypes DataGovernanceScore => new OOTBControlTypes(OOTBControlTypesEnum.DataGovernanceScore);

    /// <summary>
    /// Metadata completeness control
    /// </summary>
    public static OOTBControlTypes MetadataCompleteness => new OOTBControlTypes(OOTBControlTypesEnum.MetadataCompleteness);

    /// <summary>
    /// Ownership control
    /// </summary>
    public static OOTBControlTypes Ownership => new OOTBControlTypes(OOTBControlTypesEnum.Ownership);

    /// <summary>
    /// Cataloging control
    /// </summary>
    public static OOTBControlTypes Cataloging => new OOTBControlTypes(OOTBControlTypesEnum.Cataloging);

    /// <summary>
    /// Classification control
    /// </summary>
    public static OOTBControlTypes Classification => new OOTBControlTypes(OOTBControlTypesEnum.Classification);

    /// <summary>
    /// Use control
    /// </summary>
    public static OOTBControlTypes Use => new OOTBControlTypes(OOTBControlTypesEnum.Use);

    /// <summary>
    /// Data consumption purpose control
    /// </summary>
    public static OOTBControlTypes DataConsumptionPurpose => new OOTBControlTypes(OOTBControlTypesEnum.DataConsumptionPurpose);

    /// <summary>
    /// Access entitlement control
    /// </summary>
    public static OOTBControlTypes AccessEntitlement => new OOTBControlTypes(OOTBControlTypesEnum.AccessEntitlement);

    /// <summary>
    /// Quality control 
    /// </summary>
    public static OOTBControlTypes Quality => new OOTBControlTypes(OOTBControlTypesEnum.Quality);

    /// <summary>
    /// Data quality control
    /// </summary>
    public static OOTBControlTypes DataQuality => new OOTBControlTypes(OOTBControlTypesEnum.DataQuality);

    /// <summary>
    /// Authoritatize data source control
    /// </summary>
    public static OOTBControlTypes AuthoritativeDataSource => new OOTBControlTypes(OOTBControlTypesEnum.AuthoritativeDataSource);

    /// <summary>
    /// Get the full name of the control
    /// </summary>
    public string Name => this.value.GetDescription();

    private static List<string> names = null;

    /// <summary>
    /// Temporary method to parse the name and return an instance of OOTBControlTypes.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static OOTBControlTypes Parse(string name)
    {
        if (name == "Data governance score")
        {
            return new OOTBControlTypes(OOTBControlTypesEnum.DataGovernanceScore);
        }

        foreach (OOTBControlTypesEnum enumValue in Enum.GetValues(typeof(OOTBControlTypesEnum)))
        {
            if (name == enumValue.GetDescription())
            {
                return new OOTBControlTypes(enumValue);
            }
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all the names of the OOTB controls as a list of string
    /// </summary>
    /// <returns></returns>
    public static List<string> GetListOfNames()
    {
        if (names == null)
        {
            names = new List<string>();

            foreach (OOTBControlTypesEnum enumValue in Enum.GetValues(typeof(OOTBControlTypesEnum)))
            {
                names.Add(enumValue.GetDescription());
            }
        }

        return names;
    }

    private enum OOTBControlTypesEnum
    {
        [Description("Governance score")]
        DataGovernanceScore,
        [Description("Metadata completeness")]
        MetadataCompleteness,
        [Description("Ownership")]
        Ownership,
        [Description("Cataloging")]
        Cataloging,
        [Description("Classification")]
        Classification,
        [Description("Use")]
        Use,
        [Description("Data consumption purpose")]
        DataConsumptionPurpose,
        [Description("Access entitlement")]
        AccessEntitlement,
        [Description("Quality")]
        Quality,
        [Description("Data quality")]
        DataQuality,
        [Description("Authoritative data source")]
        AuthoritativeDataSource
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var castObject = (OOTBControlTypes)obj;

        return castObject.value == value;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.value.GetHashCode();
    }
}
