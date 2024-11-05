using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMeta;


public class ServerPart //Класс для серверной части
{ 
    public static HttpClient httpClient = new HttpClient(); //Создаём клиент для взаимодейсвтия с сервером
    public static string host = "https://api.bybit.com"; //Ссылка на сам сервер api 
    
    public string urlTickers = $"{host}/v5/market/tickers?category=spot&symbol=";
    public string urlVolume = $"{host}/v5/market/kline?category=spot&interval=5&limit=15&symbol="; //Используем 5 минутный график,ставим SMA в 10 свечей
}

