using BinanceCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinanceCore
{
    /// <summary>
    /// Логика взаимодействия для FractalConfiguration.xaml
    /// </summary>
    public partial class FractalConfiguration : UserControl
    {
        public delegate void DeleteRequestedDgt(FractalConfiguration sender);
        public event DeleteRequestedDgt DeleteRequested;
        public FractalConfiguration()
        {
            InitializeComponent();
            Color = Colors.White;   //  Настроим кнопку выбора цвета фрактала
            colorPicker.Picked +=(c)=> Color = c;   //  привяжем событие к выбиралке цвета на случай если выберут другой цвет
        }

        public FractalDefinition Fractal
        {
            get
            {
                return new FractalDefinition()
                {
                    Title = Title,
                    Code = Code,
                    Color = Color.ToString(),
                    Symbol = Symbol
                };
            }
            set
            {
                Code = value.Code;
                Title = value.Title;
                Color = (Color)ColorConverter.ConvertFromString(value.Color);
                Symbol = value.Symbol;
            }
        }
        public System.Windows.Media.Color Color
        {
            get { return (ShowColorPickerB.Background as SolidColorBrush).Color;  }
            set { ShowColorPickerB.Background = new SolidColorBrush(value); }
        }
        public string Title
        {
            get { return NameTB.Text; }
            set { NameTB.Text = value; }
        }

        public string Symbol
        {
            get { return SymbolTB.Text; }
            set { SymbolTB.Text = value; }
        }

        public string Code
        {
            get { return Designer.Code; }
            set { Designer.Code = value; }
        }

        private void deleteB_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this);
        }

        private void ShowColorPicker_Click(object sender, RoutedEventArgs e)
        {
            colorPicker.Visibility = Visibility.Visible;
        }
    }
}
