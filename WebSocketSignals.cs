using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectMeta;
public class WebSocketSignals
{
    public double control;
    public string? answer;
    ConvertAll? convertAll;
    private readonly ClientWebSocket? clientSocket; //Клиент вебСокета
    private string UriWebServer = "wss://stream.bybit.com/v5/public/spot"; //Uri сервера
    public WebSocketSignals()
    {
        //Инициализация поля
        clientSocket = new ClientWebSocket();
    }
    public async Task<string> Signals(string? symbol, CancellationToken cancellationToken, string  TradePower) // Конект, отправка, получение ответа
    {
        double? indicator;
        double? marker;
        // Готовый индикатор, тут определяем наскольк Volume должен превосходить SMA чтобы
      indicator = TradePower switch
            { 
            "1" => 1.7,
            "2" => 2,
            "3" => 2.4,
            _ =>1.7
        };
        string? Volume = null;
        bool Bag = false;
        try
        {
            await clientSocket!.ConnectAsync(new Uri(UriWebServer), CancellationToken.None); //Конектимся на свервем
            byte[] byffer = new byte[512]; //Буффер для приемки сообщений
                                           // преобразовавываем ответ (запрос) в Json формат
            string MessageJson = JsonSerializer.Serialize(new
            {
                op = "subscribe",
                args = new[] { $"kline.5.{symbol}" }
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
                    return "Ошикба";
                }
                else
                {
                    SmaVolume sma = new SmaVolume();
                    double? morok = await sma.GetSmaVolume(ServerPart.httpClient, symbol);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Можно выбросить исключение, чтобы обработать его выше
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    //Console.WriteLine(Encoding.UTF8.GetString(byffer, 0, result.Count)); //Получая данных с севера  
                    answer = Encoding.UTF8.GetString(byffer); //Данное сообщение
                    convertAll = new ConvertAll(answer);
                    Volume = convertAll.OnlyVolumeReturn();
                    Bag = double.TryParse(Volume, out control);
                    if (Bag==true)
                    {
                        marker =control/morok;
                        Console.WriteLine($"Значение объема: {control}");
                        Console.WriteLine($"Значение SMA: {morok}");
                        Console.WriteLine($"Значение индикатора: {marker}");
                        if (marker > indicator)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Индикатор успешно сработал, проверьте данные");
                            return $"Рыночное оповещение! Активность рынка возрасла\nВыбранная криптопара {symbol}";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Вызвано исключеие\n{ex.Message}");
            Console.ResetColor();
            return "";
        }
        finally
        {
            if (clientSocket!.State == WebSocketState.Open)
            {
                // Закрытие соединения, если оно все еще открыто
                await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            Console.WriteLine("Сработал finally");
        }
        return string.Empty;
    }
}

