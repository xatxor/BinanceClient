using BinanceCore.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BinanceCore
{
    /// <summary>
    /// Редактор последовательности одного фрактала максимум из 7 звеньев
    /// </summary>
    public partial class FractalConfiguration : UserControl
    {
        public delegate void DeleteRequestedDgt(FractalConfiguration sender);
        /// <summary>
        /// Событие вызывается если пользователь нажал на панели фрактала крестик удаления
        /// </summary>
        public event DeleteRequestedDgt DeleteRequested;
        
        /// <summary>
        /// Конструктор настраивает инерфейс и события
        /// </summary>
        public FractalConfiguration()
        {
            InitializeComponent();
            FractalColor = Colors.White;                            //  Настроим кнопку выбора цвета фрактала
            colorPicker.Picked +=(c)=> FractalColor = c;            //  привяжем событие к выбиралке цвета на случай если выберут другой цвет
            deleteB.Click+=(b,a)=> DeleteRequested?.Invoke(this);   //  привяжем генерацию события об удалении к нажатию на крестик удаления
            ShowColorPickerB.Click+=(b,a) => colorPicker.Visibility = Visibility.Visible;   //  При нажатии на выбор цвета покажем выбиралку цета
        }

        /// <summary>
        /// Определение фрактала можно загрузить в редактор или выгрузить из него
        /// в форме класса FractalDefinition
        /// </summary>
        public FractalDefinition FractalDefinition
        {
            get
            {
                return new FractalDefinition()
                {
                    Title = Title,
                    Code = Code,
                    Color = FractalColor.ToString(),
                    Symbol = Symbol
                };
            }
            set
            {
                Code = value.Code;
                Title = value.Title;
                FractalColor = (Color)ColorConverter.ConvertFromString(value.Color);
                Symbol = value.Symbol;
            }
        }

        #region Связь свойств класса конфигуратора и полей на экране
        /// <summary>
        /// Цвет выбиралки цвета.
        /// Через это свойство цвет выбиралки цвета фрактала делается таким же, как выбранный цвет фрактала.
        /// </summary>
        public Color FractalColor
        {
            get { return (ShowColorPickerB.Background as SolidColorBrush).Color;  }
            set { ShowColorPickerB.Background = new SolidColorBrush(value); }
        }

        /// <summary>
        /// Название фрактала (ассоциировано с текстовым полем ввода)
        /// </summary>
        public string Title
        {
            get => NameTB.Text;
            set => NameTB.Text = value;
        }

        /// <summary>
        /// Символ обозначения фрактала на графике (ассоциирован с текстовым полем ввода)
        /// </summary>
        public string Symbol
        {
            get => SymbolTB.Text; 
            set => SymbolTB.Text = value; 
        }

        /// <summary>
        /// Код фрактала, выражающий последовательность его звеньев и представленный как строка.
        /// Можно выгрузить код фрактала или задать фрактал по текстовому коду
        /// </summary>
        public string Code
        {
            get => Designer.Code; 
            set => Designer.Code = value; 
        }
        #endregion
    }
}
