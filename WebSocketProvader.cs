using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace ProjectMeta; // Подключаем пространство имен нашего каталога

public class WebSocketProvader //Класс вебсокера
{
    bool tempBool = true;
    byte a = 0;
    ConvertAll convertAll;
    string? Price;
    private readonly ClientWebSocket? clientSocket; //Клиент вебСокета
    private string UriWebServer = "wss://stream.bybit.com/v5/public/spot"; //Uri сервера
    public WebSocketProvader()
    {
        //Инициализация поля
        clientSocket = new ClientWebSocket();
    }

    public async Task<string> RunWebSocket(string? symbol, CancellationToken cancellationToken,string SoundPrice,bool Notif = false) // Конект, отправка, получение ответа
    {
        bool LogSoundPrice = double.TryParse(SoundPrice, out double LastSoundPrice);
        if (LogSoundPrice == false)
        {
            return "Цена оповещение введена не верно, введите повторно";
        }
        try
        {
            await clientSocket!.ConnectAsync(new Uri(UriWebServer), CancellationToken.None); //Конектимся на свервем
            byte[] byffer = new byte[512]; //Буффер для приемки сообщений
                                           // преобразовавываем ответ (запрос) в Json формат
            string MessageJson = JsonSerializer.Serialize(new
            {
                op = "subscribe",
                args = new[] { $"tickers.{symbol}" }
            });
            //Отправляем сообщение и подписываемся на поток
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(MessageJson), WebSocketMessageType.Text, true, CancellationToken.None);
            while (clientSocket.State == WebSocketState.Open) //Открыто ли соединение (подписка) ?
            {
                var result = await clientSocket.ReceiveAsync(byffer, CancellationToken.None); // Получаем ответ который помещается в буфер
                if (result.MessageType == WebSocketMessageType.Close) //Является ли сообщение закрытием потока
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None); //Закрываем соеднение
                    Console.WriteLine("Соединениt разорвано, поток закрыт");
                    Console.WriteLine($"Статус ответа - {result.CloseStatusDescription}");
                }
                else
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Можно выбросить исключение, чтобы обработать его выше
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    //Console.WriteLine(Encoding.UTF8.GetString(byffer, 0, result.Count)); //Получая данных с севера  
                        Price = Encoding.UTF8.GetString(byffer); //Данное сообщение
                        convertAll = new ConvertAll(Price);
                        double lastPrice = convertAll.OnlyPriceReturn();
                        string lastSymbol = convertAll.OnlySymbolReturn();
                        string LastString = $"Криптовалютная пара: {lastSymbol} Фактическая цена: {lastPrice}";
                        if (lastPrice != 0 && tempBool == true && Notif == true && LastSoundPrice != 0)
                        {
                            // Разыне сценарии
                            if (lastPrice > LastSoundPrice)
                            {
                                a = 1;
                                tempBool = false;
                            }
                            else
                            {
                                a = 2;
                                tempBool = false;
                            }
                        }
                        if (a == 1)
                        {
                            Console.WriteLine($"Цена факта - {lastPrice}\tЦена оповещения - {LastSoundPrice}");
                            if (LastSoundPrice > lastPrice || lastPrice == LastSoundPrice)
                            {
                                Console.WriteLine("Сработал сценарий № 1");
                                break; //Останавливаем цикл while
                            }
                        }
                        if (a == 2)
                        {
                            Console.WriteLine($"Цена факта - {lastPrice}\tЦена оповещения - {SoundPrice}");
                            if (LastSoundPrice < lastPrice || lastPrice == LastSoundPrice)
                            {
                                Console.WriteLine("Сработал сценарий № 2");
                                break;
                            }
                        }
                    }

                }
                return $"Оповещение пользователя\nКриптопара: {symbol}\tЦена оповещения {SoundPrice} ";
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Вызвано исключеие\n{ex.Message}");
                Console.ResetColor();
                return $"";
            }
    } //Скобка конца метода RunWebSocket
 
    public async Task CloseFlow() // Закрываем поток
    {
        if (clientSocket!.State== WebSocketState.Open)
        {
            // Закрытие соединения, если оно все еще открыто
            await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
    }
 

}

