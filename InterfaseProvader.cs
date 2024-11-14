using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Schema;
using ProjectMeta;

/*Данный файл нужен как связующее звено между ConvertPrice ServerPart, и самим интерфейсом телеги) */

public class InterfaseProvader()
{
    ConvertAll? ConvertAllPrice; // Класс преобразований, Файл - ConvertPrice -> ProjectMeta
    ServerPart serverPart = new ServerPart(); // Создаем класс Сервера
    public async Task<string> PriceAndSymbol(HttpClient? httpClient, string? pair) //Ассинхронный метод для передачи цены и криптопары
    {
        string? GoUrl = serverPart.urlTickers + pair; // Итоговоя строка запроса
        HttpResponseMessage? response = await httpClient!.GetAsync(GoUrl); //Получаем ответ от запроса

        if (response.IsSuccessStatusCode) //Проверяем что нам пришло, если код 200 то if выполняется
        {
            string responseBody = await response.Content.ReadAsStringAsync(); //Преобразование ответа в строку
            /*Пояснение - мы получаем ответ в json формате, метод ReadAsStringAsync переводит джейсон элемент
             в строку и после этого его можно преобразовать*/
            ConvertAllPrice = new ConvertAll(responseBody);
            double? lastPrice = ConvertAllPrice.OnlyPriceReturn(); //Задействуем метод класса и получим только цену
            string? lastSymbol = ConvertAllPrice.OnlySymbolReturn(); //Задействуем метод класс и получаем криптопару
            if (lastPrice == 0 || lastPrice == null) // Проверка есть цена или нет
            {
                string message = "Криптовалютная пара не найдена\n";
                Console.Write(message);
                return message;
            }
            else
            {
                string LastSumString = $"Пара: {lastSymbol} Фактическая цена: {lastPrice}\n\r";
                Console.Write(LastSumString);
                return LastSumString;
            }
        }
        else
        {
            Console.WriteLine("Ошибка при получении данных: " + response.StatusCode);
            return "Ошибка при получении данных";
        }
    }
    public async Task<string> PriceUse(string PricePair) //Этот метод позволяет вызвать метод запроса несколько раз в зависимости от пар
    {
        int temp = 1; //Счетчик
        string OneAllMessage = "";
        string?[] PricePairList = PricePair?.Split(' ')!; //Делем строку на несколько строк, разделитель является пробелом
        foreach (string? PriceBeta in PricePairList) //PriceBeta - используем как объкт  для перебора
        {
            OneAllMessage += $"Запрос № {temp} "+await PriceAndSymbol(ServerPart.httpClient, PriceBeta); 
            //Тут обращаемся к методу несколько раз и формируем сообщение
            temp++;
        }
        return OneAllMessage; //Итоговый ответ
    }

}
