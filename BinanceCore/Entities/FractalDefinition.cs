using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.Entities
{
    /// <summary>
    /// Описание фрактала
    /// </summary>
    public class FractalDefinition
    {
        /// <summary>
        /// Название фрактала
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Код фрактала
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Символ отображения фрактала на графике
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Цвет отображения фрактала на графике
        /// </summary>
        public string Color { get; set; }

    }
}
