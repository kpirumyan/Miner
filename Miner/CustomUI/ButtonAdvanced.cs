using Miner.Classes;
using System.Windows;
using System.Windows.Controls;

namespace Miner.CustomUI
{
  // Create custom button class which has a Coordinates property
  class ButtonAdvanced : Button
  {
    public static readonly DependencyProperty IndexProperty;
    public static readonly DependencyProperty LabelProperty;
    public static readonly DependencyProperty ValueProperty;
    public static readonly DependencyProperty ContentTypeProperty;

    static ButtonAdvanced()
    {
      IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(ButtonAdvanced));
      LabelProperty = DependencyProperty.Register("Label", typeof(MarkedField), typeof(ButtonAdvanced));
      ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ButtonAdvanced));
      ContentTypeProperty = DependencyProperty.Register("ContentType", typeof(FieldContentType), typeof(ButtonAdvanced));
    }

    public int Index
    {
      get { return (int)GetValue(IndexProperty); }
      set { SetValue(IndexProperty, value); }
    }

    public MarkedField Label
    {
      get { return (MarkedField)GetValue(LabelProperty); }
      set { SetValue(LabelProperty, value); }
    }

    public object Value
    {
      get { return GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    public FieldContentType ContentType
    {
      get { return (FieldContentType)GetValue(ContentTypeProperty); }
      set { SetValue(ContentTypeProperty, value); }
    }
  }
}
