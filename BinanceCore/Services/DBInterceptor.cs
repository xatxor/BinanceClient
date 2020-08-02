using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace BinanceCore.Services
{
    public class HintCommandInterceptor : DbCommandInterceptor
    {
        /// <summary>
        /// Перехватчик запросов Entity к СУБД - тут можем проверять команды запросов.
        /// Иногда энтити делает очень суровые запросы, и их приходится оптимизировать.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            // Manipulate the command text, etc. here...
            return result;
        }
    }
}
