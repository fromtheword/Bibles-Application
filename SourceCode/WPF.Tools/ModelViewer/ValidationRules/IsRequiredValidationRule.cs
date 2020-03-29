using GeneralExtensions;
using System;
using System.Linq;
using System.Windows.Controls;

namespace WPF.Tools.ModelViewer.ValidationRules
{
    public class IsRequiredValidationRule : ValidationRule
    {
        private string requiredMessage = "Required Field";

        public bool IsRequired
        {
            get;
            set;
        }

        public string[] ComboBoxKeys { get; set; }

        public ModelItemTypeEnum ObjectType
        {
            get;
            set;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.DatePicker:

                    if (value == null && this.IsRequired)
                    {
                        return new ValidationResult(false, requiredMessage);
                    }

                    return ValidationResult.ValidResult;

                case ModelItemTypeEnum.ComboBox:

                    string selectedItem = value as String;

                    if (selectedItem.IsNullEmptyOrWhiteSpace() && this.IsRequired)
                    {
                        return new ValidationResult(false, requiredMessage);
                    }

                    if (this.ComboBoxKeys.HasElements())
                    {
                        if (!this.ComboBoxKeys.Contains(selectedItem) && this.IsRequired)
                        {
                            return new ValidationResult(false, requiredMessage);
                        }
                    }

                    return ValidationResult.ValidResult;

                case ModelItemTypeEnum.CheckBox:
                case ModelItemTypeEnum.EnumBox:
                case ModelItemTypeEnum.SecureString:
                case ModelItemTypeEnum.TextBox:
                default:

                    string content = value as String;

                    if (content.IsNullEmptyOrWhiteSpace() && this.IsRequired)
                    {
                        return new ValidationResult(false, requiredMessage);
                    }

                    return ValidationResult.ValidResult;
            }
        }
    }
}
