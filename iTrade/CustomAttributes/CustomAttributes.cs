using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iTrade.CustomAttributes
{
    public class CustomAttributes
    {

    }

    /// <summary>
    /// For a boolean field, set the display text for <c>true</c> and
    /// <c>false</c> values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
                    Inherited = false, AllowMultiple = false)]
    public class BooleanDisplayValuesAttribute : Attribute, IMetadataAware
    {
        public const string TrueTitleAdditionalValueName = "BooleanTrueValueTitle";
        public const string FalseTitleAdditionalValueName = "BooleanFalseValueTitle";

        private readonly string _trueValueTitle;
        private readonly string _falseValueTitle;

        public BooleanDisplayValuesAttribute(string trueValueTitle,
                                             string falseValueTitle)
        {
            _trueValueTitle = trueValueTitle;
            _falseValueTitle = falseValueTitle;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues[TrueTitleAdditionalValueName] = _trueValueTitle;
            metadata.AdditionalValues[FalseTitleAdditionalValueName] = _falseValueTitle;
        }
    }

    /// <summary>
    /// For a boolean field, set the display text for <c>true</c> and
    /// <c>false</c> values to "Show" and "Hide".
    /// </summary>
    public class BooleanDisplayValuesAsShowHideAttribute :
       BooleanDisplayValuesAttribute
    {
        public BooleanDisplayValuesAsShowHideAttribute()
            : base("Show", "Hide")
        {
        }
    }

    /// <summary>
    /// For a boolean field, set the display text for <c>true</c> and
    /// <c>false</c> values to "Enable" and "Disable".
    /// </summary>
    public class BooleanDisplayValuesAsEnableDisableAttribute :
        BooleanDisplayValuesAttribute
    {
        public BooleanDisplayValuesAsEnableDisableAttribute()
            : base("Enable", "Disable")
        {
        }
    }

    /// <summary>
    /// For a boolean field, set the display text for <c>true</c> and
    /// <c>false</c> values to "Yes" and "No".
    /// </summary>
    public class BooleanDisplayValuesAsYesNoAttribute :
        BooleanDisplayValuesAttribute
    {
        public BooleanDisplayValuesAsYesNoAttribute()
            : base("Yes", "No")
        {
        }
    }


}


