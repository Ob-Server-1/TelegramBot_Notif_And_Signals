using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ProjectMeta
{
    class ConvertAll //Класс конвертации
    {
        string? OnlyPrice; //Строка для получения цены
        string? OnlySymbol; //Строка для получения криптопары
        string? OnlyVolume; //Строка для получения объёма

        double resultPrice; //Окончательная цена
        string patternDeleteLiteral = "[A-Za-z,\",:, ]"; //Патерн для удаления букав
        string pattrernSymbol = "[\",:,symbol, ]"; //Паттерн для удаления ненужны символов для получения пары
        // Регулярное выражение для цены
        Regex regexPrice = new Regex(@".lastprice.:..............", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); //Поиск цены
        //Патерн для получение криптоПары
        Regex regexSymbol = new Regex(@".symbol.:...........", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); //Поиск криптопары
        Regex regexVolume = new Regex(@".volume.:.............",RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); //Поиск объёма
        public ConvertAll(string Price) //Конструктор
        {
            //Регулярные коллекции
            MatchCollection match1 = regexPrice.Matches(Price);
            MatchCollection match2 = regexSymbol.Matches(Price);
            MatchCollection match3 = regexVolume.Matches(Price);
            //
            if (match1.Count > 0)
            {
                foreach (Match match in match1)
                {
                    OnlyPrice = match.Value.ToString();
                }
            }
            if (match2.Count > 0)
            {
                foreach (Match match in match2)
                {
                    OnlySymbol = match.Value.ToString();
                }
            }
            if (match3.Count>0)
            {
                foreach (Match match in match3)
                {
                    OnlyVolume = match.Value.ToString();
                }
            }
        }
        public double OnlyPriceReturn() //Получаем на выход только цену
        {
            //Регулярное выражение которое позволяет заменить некоторые символ на ничто! в [] написан объект для изменения
            try
            { //Используем для обработки исключений

                OnlyPrice = Regex.Replace(OnlyPrice!, patternDeleteLiteral, "");
                resultPrice = double.Parse(OnlyPrice, CultureInfo.InvariantCulture);
                return resultPrice;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return resultPrice;
            }
        }
        public string OnlySymbolReturn() //Получаем криптоапару
        {
            try
            {
                OnlySymbol = Regex.Replace(OnlySymbol!, pattrernSymbol, ""); //Используем регульрку
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return OnlySymbol!;
            }
            return OnlySymbol;
        }
        public string OnlyVolumeReturn()
        {
            try
            {
                OnlyVolume = Regex.Replace(OnlyVolume!, patternDeleteLiteral, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return OnlyVolume!;
            }
            return OnlyVolume.Replace(".",",") ;
        }
    }
}
