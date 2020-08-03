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
    /// Самоисчезающая выбиралка цвета (исчезает после нажатия на кнопку цвета,  генерирует эвент с цветом)
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public delegate void PickedDgt(Color c);
        public event PickedDgt Picked;

        public ColorPicker()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            Picked?.Invoke(((sender as Button).Background as SolidColorBrush).Color);
        }
    }
}
