﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class StringResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StringResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Purview.DataEstateHealth.DHModels.StringResources", typeof(StringResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of array &quot;{0}&quot; must be greater than {1}.
        /// </summary>
        internal static string ErrorMessageArrayLengthBelowMin {
            get {
                return ResourceManager.GetString("ErrorMessageArrayLengthBelowMin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of array &quot;{0}&quot; must be less than {1}.
        /// </summary>
        internal static string ErrorMessageArrayLengthExceedMax {
            get {
                return ResourceManager.GetString("ErrorMessageArrayLengthExceedMax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot set rules when targetEntityType is not set or targetQualityType is not set..
        /// </summary>
        internal static string ErrorMessageAssessmentRuleShouldBeEmpty {
            get {
                return ResourceManager.GetString("ErrorMessageAssessmentRuleShouldBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The name connot be just composed of spaces..
        /// </summary>
        internal static string ErrorMessageEmptyEntityName {
            get {
                return ResourceManager.GetString("ErrorMessageEmptyEntityName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to parse the entity payload. The input entity is invalid..
        /// </summary>
        internal static string ErrorMessageEntityParseFailed {
            get {
                return ResourceManager.GetString("ErrorMessageEntityParseFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value &quot;{0}&quot; for the &quot;{1}&quot; property is not valid. Valid values: {2}.
        /// </summary>
        internal static string ErrorMessageEnumPropertyValueNotValid {
            get {
                return ResourceManager.GetString("ErrorMessageEnumPropertyValueNotValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Id can only contain alphanumeric characters, dash and underscore(_)..
        /// </summary>
        internal static string ErrorMessageInvalidEntityId {
            get {
                return ResourceManager.GetString("ErrorMessageInvalidEntityId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The name contains invalid character(s). Try another name..
        /// </summary>
        internal static string ErrorMessageInvalidEntityName {
            get {
                return ResourceManager.GetString("ErrorMessageInvalidEntityName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &quot;{0}&quot; can not be null or empty..
        /// </summary>
        internal static string ErrorMessageNullOrEmpty {
            get {
                return ResourceManager.GetString("ErrorMessageNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;{0}&quot; must be greater than {1}.
        /// </summary>
        internal static string ErrorMessageNumberBelowMin {
            get {
                return ResourceManager.GetString("ErrorMessageNumberBelowMin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;{0}&quot; must be less than {1}.
        /// </summary>
        internal static string ErrorMessageNumberExceedMax {
            get {
                return ResourceManager.GetString("ErrorMessageNumberExceedMax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;{0}&quot; is not a valid value for {1}..
        /// </summary>
        internal static string ErrorMessageReferenceTypeInvalid {
            get {
                return ResourceManager.GetString("ErrorMessageReferenceTypeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reference type must be &quot;{0}&quot;..
        /// </summary>
        internal static string ErrorMessageReferenceTypeRestriction {
            get {
                return ResourceManager.GetString("ErrorMessageReferenceTypeRestriction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The field {0} must match the regular expression &quot;{1}&quot;..
        /// </summary>
        internal static string ErrorMessageRegularExpressionNotMatch {
            get {
                return ResourceManager.GetString("ErrorMessageRegularExpressionNotMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Length of &quot;{0}&quot; must be less than {1}. Current length: {2}..
        /// </summary>
        internal static string ErrorMessageStringLengthMax {
            get {
                return ResourceManager.GetString("ErrorMessageStringLengthMax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The minimum length of &quot;{0}&quot; is {1}. Current length: {2}..
        /// </summary>
        internal static string ErrorMessageStringLengthMin {
            get {
                return ResourceManager.GetString("ErrorMessageStringLengthMin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type of property &quot;{0}&quot; must be &quot;{1}&quot;..
        /// </summary>
        internal static string ErrorMessageTypeNotMatch {
            get {
                return ResourceManager.GetString("ErrorMessageTypeNotMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &quot;{0}&quot; must be a string..
        /// </summary>
        internal static string ErrorMessageTypeNotString {
            get {
                return ResourceManager.GetString("ErrorMessageTypeNotString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TargetEntityType &quot;{0}&quot; is not supported for targetQualityType &quot;{1}&quot;. Supported TargetEntityTypes are &quot;{2}&quot;..
        /// </summary>
        internal static string ErrorMessageUnsupportedTargetEntityType {
            get {
                return ResourceManager.GetString("ErrorMessageUnsupportedTargetEntityType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &quot;{0}&quot; is required..
        /// </summary>
        internal static string ErrorMessageValueRequired {
            get {
                return ResourceManager.GetString("ErrorMessageValueRequired", resourceCulture);
            }
        }
    }
}
