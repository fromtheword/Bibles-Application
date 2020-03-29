using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace WPF.Tools.ModelViewer.ValidationRules
{
    public class IsEitherOrValudationRule : ValidationRule
    {
        public bool IsRequired
        {
            get;
            set;
        }

        public List<ModelViewItem> GroupItems
        {
            get;
            set;
        }
        
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            StringBuilder resultMessage = new StringBuilder();

            resultMessage.Append("Either ");

            List<bool> itemHaveValues = new List<bool>();

            foreach (ModelViewItem item in this.GroupItems)
            {
                resultMessage.Append($"{item.Caption} or ");

                ModelItemTypeEnum itemType = item.ObjectType;

                object itemValue = item.GetValue();

                switch (itemType)
                {
                    case ModelItemTypeEnum.DatePicker:

                        if (itemValue == null && this.IsRequired)
                        {
                            itemHaveValues.Add(false);
                        }
                        else
                        {
                            itemHaveValues.Add(true);
                        }

                        break;

                    case ModelItemTypeEnum.CheckBox:
                    case ModelItemTypeEnum.ComboBox:
                    case ModelItemTypeEnum.EnumBox:
                    case ModelItemTypeEnum.SecureString:
                    case ModelItemTypeEnum.TextBox:
                    default:

                        string content = itemValue as String;

                        if (content.IsNullEmptyOrWhiteSpace() && this.IsRequired)
                        {
                            itemHaveValues.Add(false);
                        }
                        else
                        {
                            itemHaveValues.Add(true);
                        }

                        break;
                }
            }

            resultMessage.Remove(resultMessage.Length - 3, 3);

            resultMessage.Append("is required.");

            if (!itemHaveValues.Any(a => a))
            {
                return new ValidationResult(false, resultMessage.ToString());
            }

            return ValidationResult.ValidResult;
        }
    }
}
