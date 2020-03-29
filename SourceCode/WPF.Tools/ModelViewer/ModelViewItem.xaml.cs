using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPF.Tools.Attributes;
using WPF.Tools.CommonControls;
using WPF.Tools.Dictionaries;
using WPF.Tools.ModelViewer.ValidationRules;
using WPF.Tools.Specialized;
using WPF.Tools.ToolModels;

namespace WPF.Tools.ModelViewer
{
    /// <summary>
    /// Interaction logic for ModelViewItem.xaml
    /// </summary>
    public partial class ModelViewItem : UserControl
    {
        public delegate void ModelViewItemGotFocusEvent(ModelViewItem sender, object focusedbject);

        public delegate void ModelViewItemBrowseEvent(object sender, string buttonKey);

        public event ModelViewItemGotFocusEvent ModelViewItemGotFocus;

        public event ModelViewItemBrowseEvent ModelViewItemBrowse;

        private bool isRequired;

        private bool isSpellCheck;

        private bool isComboBoxEdit;

        private object contentObject;

        private object parent;

        private string eitherOrGroupName;

        private ValuesSourceAttribute valuesSource;

        public ModelViewItem(string parentTypeName, object parentObject, PropertyInfo property)
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.ParentObjectTypeName = parentTypeName;

            this.PropertyName = property.Name;

            this.PropertyInfo = property;

            this.parent = parentObject;

            this.CreateObject(parentObject);
        }

        public int Sort
        {
            get;
            set;
        }

        public bool HasFieldInformation
        {
            get;
            set;
        }

        public bool IsReadOnly
        {
            set
            {
                this.SetIsReadOnly(value);

                switch (this.ObjectType)
                {
                    case ModelItemTypeEnum.CheckBox:

                        ((CheckBoxItem) this.contentObject).IsEnabled = !value;

                        break;

                    case ModelItemTypeEnum.ComboBox:
                    case ModelItemTypeEnum.EnumBox:

                        ((ComboBoxTool) this.contentObject).IsEnabled = !value;

                        break;

                    case ModelItemTypeEnum.DatePicker:

                        ((DatePicker) this.contentObject).IsEnabled = !value;

                        break;

                    case ModelItemTypeEnum.TextBox:
                    default:

                        ((TextBoxItem) this.contentObject).IsReadOnly = value;

                        break;
                }
            }
        }

        public bool IsRequired
        {
            get
            {
                return this.isRequired;
            }

            set
            {
                this.isRequired = value;

                this.uxRequired.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

                if (this.Binding != null)
                {
                    ((IsRequiredValidationRule) this.Binding.ValidationRules.First(r => r.GetType() == typeof(IsRequiredValidationRule))).IsRequired = value;
                }
            }
        }

        public new bool Focus()
        {
            try
            {
                FocusManager.SetFocusedElement(this, (UIElement) this.contentObject);

                Keyboard.Focus((UIElement) this.contentObject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public object Caption
        {
            get
            {
                return this.uxCaption.Content;
            }

            set
            {
                this.uxCaption.Content = value;
            }
        }

        public double CaptionTextWidth
        {
            get
            {
                if (this.IsRequired)
                {
                    return this.uxCaption.TextLenght + 15;
                }

                return this.uxCaption.TextLenght;
            }
        }

        public double CaptionWidth
        {
            get
            {
                return this.uxCaption.Width;
            }

            set
            {
                value += 15;

                this.uxCaption.Width = this.IsRequired ? (value - 15) : value;

                this.InvalidateVisual();
            }
        }

        public string ParentObjectTypeName
        {
            get;
            private set;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public string EitherOrGroupName
        { 
            get
            {
                return this.eitherOrGroupName;
            }

            set
            {
                this.eitherOrGroupName = value;

                this.uxRequired.Foreground = value.IsNullEmptyOrWhiteSpace() ? Brushes.Red : Brushes.Blue;
            }
        }

        public UIElement ContentObject
        {
            get
            {
                return (UIElement) this.contentObject;
            }
        }

        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }

        public ModelItemTypeEnum ObjectType
        {
            get;
            private set;
        }

        public DependencyProperty DependencyProperty
        {
            get;
            private set;
        }

        public BindingExpression BindingExpression
        {
            get;
            private set;
        }

        public Binding Binding { get; set; }

        public object GetSelectedItem()
        {
            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.CheckBox:

                    return ((CheckBoxItem) this.contentObject).IsChecked;


                case ModelItemTypeEnum.ComboBox:
                case ModelItemTypeEnum.EnumBox:

                    return ((ComboBoxTool) this.contentObject).SelectedItem;

                case ModelItemTypeEnum.DatePicker:

                    return ((DatePicker) this.contentObject).SelectedDate;

                case ModelItemTypeEnum.SecureString:

                    return ((PasswordBoxBindable) this.contentObject).Password;

                case ModelItemTypeEnum.TextBox:
                default:

                    return ((TextBoxItem) this.contentObject).Text;
            }
        }

        public object GetValue()
        {
            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.CheckBox:

                    return ((CheckBoxItem) this.contentObject).IsChecked;


                case ModelItemTypeEnum.ComboBox:
                case ModelItemTypeEnum.EnumBox:

                    return ((ComboBoxTool) this.contentObject).SelectedValue;

                case ModelItemTypeEnum.DatePicker:

                    return ((DatePicker) this.contentObject).SelectedDate;

                case ModelItemTypeEnum.SecureString:

                    return ((PasswordBoxBindable) this.contentObject).Password;

                case ModelItemTypeEnum.TextBox:
                default:

                    return ((TextBoxItem) this.contentObject).Text;
            }
        }

        public void SetValue(object value)
        {
            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.CheckBox:

                    ((CheckBoxItem) this.contentObject).IsChecked = value.TryToBool();

                    break;

                case ModelItemTypeEnum.ComboBox:
                case ModelItemTypeEnum.EnumBox:

                    ((ComboBoxTool) this.contentObject).SelectedValue = value;

                    break;

                case ModelItemTypeEnum.DatePicker:

                    ((DatePicker) this.contentObject).SelectedDate = value.TryToDate();

                    break;

                case ModelItemTypeEnum.SecureString:

                    ((PasswordBoxBindable) this.contentObject).Password = value.ParseToString();

                    break;

                case ModelItemTypeEnum.TextBox:
                default:

                    ((TextBoxItem) this.contentObject).Text = value.ParseToString();

                    break;
            }
        }

        public void SetComboBoxItems(DataItemModel[] sourceItems)
        {
            ComboBoxTool comboBox = (ComboBoxTool)this.contentObject;

            comboBox.Items.Clear();

            List<string> comboBoxKeys = new List<string>();

            foreach (DataItemModel valueItem in sourceItems)
            {
                comboBoxKeys.Add(valueItem.ItemKey.ParseToString());

                comboBox.Items.Add(valueItem);
            }

            if (this.Binding != null)
            {
                ((IsRequiredValidationRule)this.Binding.ValidationRules.First(r => r.GetType() == typeof(IsRequiredValidationRule))).ComboBoxKeys = comboBoxKeys.ToArray();
            }
        }

        public void AddComboBoxItem(DataItemModel sourceItem)
        {
            ComboBoxTool comboBox = (ComboBoxTool)this.contentObject;
            
            List<string> comboBoxKeys = new List<string>();
            
            foreach (DataItemModel valueItem in comboBox.Items)
            {
                comboBoxKeys.Add(valueItem.ItemKey.ParseToString());
            }

            comboBoxKeys.Add(sourceItem.ItemKey.ParseToString());

            comboBox.Items.Add(sourceItem);

            if (this.Binding != null)
            {
                ((IsRequiredValidationRule)this.Binding.ValidationRules.First(r => r.GetType() == typeof(IsRequiredValidationRule))).ComboBoxKeys = comboBoxKeys.ToArray();
            }
        }

        public void ChangeType(ModelItemTypeEnum newType)
        {
            this.ObjectType = newType;

            this.uxContent.Content = null;

            FieldInformationAttribute fieldValues = (FieldInformationAttribute) this.PropertyInfo.GetCustomAttribute(typeof(FieldInformationAttribute));

            this.CreateContent(this.parent, fieldValues);
        }

        private void Item_Focuesd(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ModelViewItemGotFocus != null)
                {
                    this.ModelViewItemGotFocus(this, this.contentObject);
                }
            }
            catch
            {
                // DO NOTHING
            }
        }

        private void BrowseButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.ModelViewItemBrowse != null)
            {
                this.ModelViewItemBrowse(sender, ((ActionButton) sender).Tag.ToString());
            }
        }

        private void CreateObject(object parentObject)
        {
            FieldInformationAttribute fieldValues = (FieldInformationAttribute) this.PropertyInfo.GetCustomAttribute(typeof(FieldInformationAttribute));

            ItemTypeAttribute itemType = (ItemTypeAttribute) this.PropertyInfo.GetCustomAttribute(typeof(ItemTypeAttribute));

            this.valuesSource = (ValuesSourceAttribute) this.PropertyInfo.GetCustomAttribute(typeof(ValuesSourceAttribute));

            this.HasFieldInformation = true;

            if (fieldValues == null)
            {
                fieldValues = new FieldInformationAttribute(this.PropertyInfo.Name);

                this.HasFieldInformation = false;
            }

            this.ObjectType = itemType == null ? ModelItemType.GetItemType(this.PropertyInfo.PropertyType) : itemType.ModelType;

            this.isComboBoxEdit = itemType == null ? false : itemType.IsComboboxEditable;

            this.Visibility = fieldValues.IsVisible ? Visibility.Visible : Visibility.Collapsed;

            this.EitherOrGroupName = fieldValues.EitherOrGroupName;

            this.IsRequired = fieldValues.IsRequired;
            
            this.isSpellCheck = !fieldValues.DisableSpellChecker;

            this.Caption = fieldValues.FieldCaption;

            this.Sort = fieldValues.Sort;

            this.FontWeight = FontWeights.Normal;

            this.CreateContent(parentObject, fieldValues);

            this.AddBrowsable();
        }

        private void AddBrowsable()
        {
            foreach (BrowseButtonAttribute buttonAttribute in this.PropertyInfo.GetCustomAttributes(typeof(BrowseButtonAttribute)))
            {
                ActionButton buttonItem = new ActionButton
                {
                    Tag = buttonAttribute.ButtonKey,
                    Content = buttonAttribute.ButtonText,
                    ToolTip = buttonAttribute.ButtonText,
                    DefaultSize = 26
                };

                if (!buttonAttribute.ResourceImage.IsNullEmptyOrWhiteSpace())
                {
                    buttonItem.ResourceImageName = buttonAttribute.ResourceImage;
                }

                buttonItem.Click += this.BrowseButton_Clicked;

                this.uxButtonStack.Children.Add(buttonItem);
            }
        }

        private void CreateContent(object parentObject, FieldInformationAttribute fieldValues)
        {
            BindingMode bindingMode = this.PropertyInfo.CanRead && this.PropertyInfo.CanWrite ? BindingMode.TwoWay :
                this.PropertyInfo.CanWrite ? BindingMode.OneWayToSource : BindingMode.OneWay;

            this.Binding = new Binding(this.PropertyInfo.Name)
            {
                Path = new PropertyPath(this.PropertyInfo.Name),
                Source = parentObject,
                Mode = bindingMode,
                BindsDirectlyToSource = true,
            };

            this.Binding.ValidationRules.Add(new IsRequiredValidationRule 
            {
                    IsRequired = (fieldValues.IsRequired && fieldValues.EitherOrGroupName.IsNullEmptyOrWhiteSpace()), 
                    ObjectType = this.ObjectType
            });

            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.CheckBox:

                    #region CHECK BOX

                    CheckBoxItem check = new CheckBoxItem {IsEnabled = !fieldValues.IsReadOnly};

                    check.GotFocus += this.Item_Focuesd;

                    this.DependencyProperty = CheckBoxItem.IsCheckedProperty;

                    check.SetBinding(CheckBoxItem.IsCheckedProperty, this.Binding);

                    this.BindingExpression = check.GetBindingExpression(CheckBoxItem.IsCheckedProperty);

                    this.contentObject = check;

                    break;

                #endregion

                case ModelItemTypeEnum.ComboBox:
                case ModelItemTypeEnum.EnumBox:

                    #region COMBO BOX

                    ComboBoxTool comboBox = new ComboBoxTool {IsEnabled = !fieldValues.IsReadOnly, IsEditable = this.isComboBoxEdit};

                    comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;

                    comboBox.GotFocus += this.Item_Focuesd;

                    this.LoadContentValues(parentObject, comboBox);

                    this.DependencyProperty = this.isComboBoxEdit ? ComboBoxTool.TextProperty : ComboBoxTool.ItemKeyProperty;

                    comboBox.SetBinding(this.isComboBoxEdit ? ComboBoxTool.TextProperty : ComboBoxTool.ItemKeyProperty, this.Binding);

                    this.BindingExpression = comboBox.GetBindingExpression(this.isComboBoxEdit ? ComboBoxTool.TextProperty : ComboBoxTool.ItemKeyProperty);

                    this.contentObject = comboBox;

                    break;

                #endregion

                case ModelItemTypeEnum.DatePicker:

                    #region DATE PICKER

                    DatePicker date = new DatePicker {IsEnabled = !fieldValues.IsReadOnly};

                    date.HorizontalAlignment = HorizontalAlignment.Stretch;

                    date.GotFocus += this.Item_Focuesd;

                    this.DependencyProperty = DatePicker.SelectedDateProperty;

                    date.SetBinding(DatePicker.SelectedDateProperty, this.Binding);

                    this.BindingExpression = date.GetBindingExpression(DatePicker.SelectedDateProperty);

                    this.contentObject = date;

                    break;

                #endregion

                case ModelItemTypeEnum.SecureString:

                    #region SECURE STRING

                    PasswordBoxBindable pass = new PasswordBoxBindable(parentObject) {IsEnabled = !fieldValues.IsReadOnly};

                    pass.HorizontalAlignment = HorizontalAlignment.Stretch;

                    pass.HorizontalContentAlignment = HorizontalAlignment.Stretch;

                    pass.LostFocus += this.Item_Focuesd;

                    this.DependencyProperty = PasswordBoxBindable.PasswordTextProperty;

                    pass.SetBinding(PasswordBoxBindable.PasswordTextProperty, this.Binding);

                    this.BindingExpression = pass.GetBindingExpression(PasswordBoxBindable.PasswordTextProperty);

                    this.contentObject = pass;

                    break;

                #endregion

                case ModelItemTypeEnum.TextBox:
                default:

                    #region TEXT BOX (DEFAULT)

                    TextBoxItem text = new TextBoxItem {IsReadOnly = fieldValues.IsReadOnly, TextWrapping = TextWrapping.WrapWithOverflow};

                    text.MaxHeight = 250;

                    text.MaxLength = fieldValues.MaxTextLength;

                    text.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    text.HorizontalAlignment = HorizontalAlignment.Stretch;

                    text.HorizontalContentAlignment = HorizontalAlignment.Stretch;

                    text.SpellCheck.IsEnabled = this.isSpellCheck;

                    text.LostFocus += this.Item_Focuesd;

                    this.DependencyProperty = TextBoxItem.TextProperty;

                    text.SetBinding(TextBoxItem.TextProperty, this.Binding);

                    this.BindingExpression = text.GetBindingExpression(TextBoxItem.TextProperty);

                    this.contentObject = text;

                    break;

                #endregion
            }

            this.SetIsReadOnly(fieldValues.IsReadOnly);

            this.uxContent.Content = this.contentObject;
        }

        private void LoadContentValues(object parentObject, object tool)
        {
            switch (this.ObjectType)
            {
                case ModelItemTypeEnum.CheckBox:

                    break;

                case ModelItemTypeEnum.ComboBox:

                    if (this.valuesSource == null)
                    {
                        return;
                    }

                    ComboBoxTool comboBox = (ComboBoxTool) tool;

                    PropertyInfo sourceProp = parentObject.GetType().GetProperty(this.valuesSource.PropertyName);

                    DataItemModel[] sourceItems = (DataItemModel[]) sourceProp.GetValue(parentObject);

                    List<string> comboBoxKeys = new List<string>();

                    foreach (DataItemModel valueItem in sourceItems)
                    {
                        comboBoxKeys.Add(valueItem.ItemKey.ParseToString());

                        comboBox.Items.Add(valueItem);
                    }

                    if (this.Binding != null)
                    {
                        ((IsRequiredValidationRule)this.Binding.ValidationRules.First(r => r.GetType() == typeof(IsRequiredValidationRule))).ComboBoxKeys = comboBoxKeys.ToArray();
                    }

                    break;

                case ModelItemTypeEnum.EnumBox:

                    ComboBoxTool enumBox = (ComboBoxTool) tool;

                    List<string> enumBoxKeys = new List<string>();

                    foreach (var item in Enum.GetValues(this.PropertyInfo.PropertyType))
                    {
                        enumBoxKeys.Add(item.ParseToString());

                        enumBox.Items.Add(new DataItemModel {DisplayValue = item.GetDescriptionAttribute(), ItemKey = item});
                    }

                    if (this.Binding != null)
                    {
                        ((IsRequiredValidationRule)this.Binding.ValidationRules.First(r => r.GetType() == typeof(IsRequiredValidationRule))).ComboBoxKeys = enumBoxKeys.ToArray();
                    }

                    break;

                case ModelItemTypeEnum.DatePicker:

                    break;

                case ModelItemTypeEnum.TextBox:
                default:
                    break;
            }
        }
    
        private void SetIsReadOnly(bool value)
        {
            this.uxCaption.Foreground = value ? Brushes.DarkGray : Brushes.Black;

            this.IsTabStop = !value;

            if (this.contentObject != null)
            {
                switch (this.ObjectType)
                {
                    case ModelItemTypeEnum.CheckBox:

                        ((CheckBoxItem)this.contentObject).IsTabStop = !value;
                        
                        break;
                        
                    case ModelItemTypeEnum.ComboBox:
                    case ModelItemTypeEnum.EnumBox:

                        ((ComboBoxTool)this.contentObject).IsTabStop = !value;

                        break;


                    case ModelItemTypeEnum.DatePicker:
                        
                        ((DatePicker)this.contentObject).IsTabStop = !value;

                        break;
                        
                    case ModelItemTypeEnum.SecureString:

                        ((PasswordBoxBindable)this.contentObject).IsTabStop = !value;
                        
                        break;

                    case ModelItemTypeEnum.TextBox:
                    default:

                        ((TextBoxItem)this.contentObject).IsTabStop = !value;

                        break;
                }
            }
        }
    }
}
