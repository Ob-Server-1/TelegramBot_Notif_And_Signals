using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMeta;

public class SmaVolume //Технический индикатор в каком то плане (Вычисляем SMA)
{
    ServerPart serverPart = new ServerPart(); // Создаем класс Сервера
    public async Task<double?> GetSmaVolume(HttpClient? httpClient,string? pair) //Ассинхронный метод для передачи цены и криптопары
    {
        double? lastReturn = 0;
        int temp = 0;
        string? GoUrl = serverPart.urlVolume + pair; // Итоговоя строка запроса
        HttpResponseMessage? response = await httpClient!.GetAsync(GoUrl); //Получаем ответ от запроса
        if (response.IsSuccessStatusCode) //Проверяем что нам пришло, если код 200 то if выполняется
        {
            string? responseBody = await response.Content.ReadAsStringAsync(); //Преобразуем ответ json=>string
            string[]? lastArray = new string[15];
            double[]? lastArrayDouble = new double[15];
            JObject? jsonObject = JObject.Parse(responseBody);
            // Извлечение массива list
            var listArray = jsonObject?["result"]!["list"];
            // Преобразование в массив
            var array = listArray?.ToObject<string[][]>();
            foreach (var item in array!)
            {
                lastArray[temp] = string.Join(", ", item[5]).Replace(".", ",");
                temp++;
            }
            temp = 0;
            foreach (var item in lastArray)
            {
                try {
                    lastArrayDouble[temp] = double.Parse(lastArray[temp]);
                    temp++;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            temp = 0;
            for (int i = 0; i < lastArrayDouble.Length;  i++)
            {
                lastReturn += lastArrayDouble[i];
                temp++;
            }
            lastReturn = lastReturn / temp;
            return lastReturn;
        }
        else
        {
            Console.WriteLine("Ошибка при получении данных: " + response.StatusCode);
            return lastReturn;
        }
    }
}
