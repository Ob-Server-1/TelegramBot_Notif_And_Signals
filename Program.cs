using Microsoft.VisualBasic;
using ProjectMeta;
using System.ComponentModel.Design;
using System.Formats.Asn1;
using System.Threading;
using Telegram.Bot; // Подключение библиотеки Telegram.Bot для работы с API Telegram
using Telegram.Bot.Polling; // Подключение пространства имен для опроса сообщений
using Telegram.Bot.Types; // Подключение типов данных, связанных с Telegram API
using Telegram.Bot.Types.Enums; // Подключение перечислений (Enums) для типов обновлений
using Telegram.Bot.Types.ReplyMarkups; // Подключение для работы с разметкой ответов, такой как клавиатуры

// Получаем токен бота из переменных окружения или задаем его вручную. 
// Токен необходим для аутентификации и взаимодействия с API Telegram.
var token = Environment.GetEnvironmentVariable("TOKEN") ??
    "Token"; //Вводим токен сюда,


CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
// Создаем источник отмены, который позволяет отменить асинхронные операции при необходимости.
using var cts = new CancellationTokenSource();

EventUse eventUse = EventUse.NoEvent;
// Инициализируем экземпляр бота с использованием токена.
// Это точка входа для всех операций, которые мы можем выполнять с ботом.
var bot = new TelegramBotClient(token);

// Запрашиваем информацию о боте, чтобы подтвердить, что он запускается правильно.
// Этот метод вернет объект, содержащий информацию о боте (например, его имя).
var me = await bot.GetMeAsync();

// Выводим в консоль имя пользователя бота, чтобы убедиться, что он запущен.
// Это также даст пользователю информацию о том, что бот работает.

Console.WriteLine($"Username : {me.Username}\nId: {me.Id} Бот начал работу");

// Начинаем получать обновления от Telegram.
// Этот метод будет ждать новые сообщения и другие обновления от Telegram.
// HandleUpdateAsync будет методом для обработки обновлений, 
// а HandleErrorAsync для обработки ошибок.
// cts.Token — это токен, который позволит остановить получение обновлений.
bot.StartReceiving(
    HandleUpdateAsync,   // Метод, который будет вызываться при каждом получении обновлений
    HandleErrorAsync,    // Метод, который будет вызываться при возникновении ошибок
    cancellationToken: cts.Token // Токен для отмены
);

// Ожидаем нажатия клавиши Escape, чтобы завершить выполнение программы.
// Это позволяет боту работать до тех пор, пока пользователь не решит остановить его.
Console.ReadKey(true);

// Когда пользователь нажимает Escape, вызываем метод Cancel на cts,
// чтобы остановить получение обновлений и завершить работу бота.
cts.Cancel(); // остановка бота

// Обработчик ошибок, который вызывается при возникновении исключений.
// Используется для логирования ошибок.
async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Выводим информацию об ошибке в консоль для отладки.
    Console.WriteLine(exception);
}

// Обработчик обновлений, который вызывается при получении новых обновлений.
// Это основной метод, который будет обрабатывать все обновления, пришедшие от Telegram.
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    Message message = new Message();
    if (update.Type == UpdateType.Message && update.Message?.Text != null)
    {
        // Если сообщение текстовое и существует, обрабатываем его в методе OnMessage.
        await OnMessage(update.Message, update.Type, botClient);
    }
    // Проверяем, является ли обновление сообщением текста и не является ли оно пустым.
}


// Метод для обработки сообщений, полученных ботом.
async Task OnMessage(Message msg, UpdateType type, ITelegramBotClient botClient)
{
    Update update = new();
    // Проверяем, не является ли текст сообщения null.
    // Если это так, выводим тип сообщения в консоль, не обрабатывая его.
    if (msg.Text is not { } text) Console.WriteLine($"Received a message of type {msg.Type}");
    else if (text.StartsWith('/'))
    {
        // Ищем позицию пробела в тексте, чтобы разделить команду и ее аргументы.
        var space = text.IndexOf(' ');
        if (space < 0) space = text.Length; // Если пробела нет, значит, вся строка - это команда.
        // Извлекаем команду и преобразуем ее в нижний региcтр
        var command = text[..space].ToLower();
        await OnCommand(command, text[space..].TrimStart(), msg);
    }
    // Если сообщение не является командой, обрабатываем его как обычный текст.
    else
    {
        // await OnTextMessage(msg);
        if (eventUse == EventUse.CheckPrice) //Событие что активируется после вписание пользователем checkprice
        {
            string Pair = msg.Text.ToString().ToUpper();
            InterfaseProvader provader = new InterfaseProvader();
            string? lastResult = await provader.PriceUse(Pair);
            await bot.SendTextMessageAsync(msg.Chat, lastResult);
        }
        else if (eventUse == EventUse.Signals)
        {
            string SymbolAndSignals = msg.Text.ToString().ToUpper();
            string[] Use = SymbolAndSignals.Split(" ");
            _ = Task.Run(async () =>
            {
                try
                {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    WebSocketSignals socketSignals = new WebSocketSignals();
                    string result = await socketSignals.Signals(Use[0], cancellationToken, Use[1]);
                    await bot.SendTextMessageAsync(msg.Chat, result);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Служба сигналов была отключена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Было выбрашено иссключение, сообщения ошибки - " + ex.Message);
                }
            });
        }
        else if (eventUse == EventUse.Notif)
        {
            string SymbolAndNotif = msg.Text.ToString().ToUpper();
            string[] Use = SymbolAndNotif.Split(" ");
            _ = Task.Run(async () =>
            {
                try
                {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    WebSocketProvader socket = new WebSocketProvader();
                    string result = await socket.RunWebSocket(Use[0], cancellationToken, Use[1], true);
                    await bot.SendTextMessageAsync(msg.Chat, result);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Служба оповещений была отключена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Было выбрашено иссключение, сообщения ошибки - " + ex.Message);
                }
            });
        }
        else Console.WriteLine($"Текст отправленный пользователем{msg.Text}");
    }
}

// Метод для обработки текстовых сообщений, которые не являются командами.
async Task OnTextMessage(Message msg)
{
    // Выводим в консоль, какой текст было получено.
    Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
    // Перенаправляем текстовое сообщение к команде "/start" для дальнейшей обработки.
    await OnCommand("/start", "", msg);
}

// Метод для обработки команд, которые начинаются с '/'.
async Task OnCommand(string command, string args, Message msg)
{
    // Выводим информацию о полученной команде и ее аргументах для отладки.
    Console.WriteLine($"Received command: {command} {args}");
    // В зависимости от команды выполняем определенные действия.
    switch (command)
    {
        case "/start":
            // Если команда "/start", отправляем приветственное сообщение в чат.
            await bot.SendTextMessageAsync(msg.Chat, "Команды\n /checkprice -Получение фактический цен /checkpriceend -Отключаем эту команду" +
                " /notif -ставим оповещение криптопары и цены /notifend -отключаем эту команду");
            break;
        case "/checkprice":
            await bot.SendTextMessageAsync(msg.Chat,
                "Введите криптовалютные пары, через пробел\nПример ввода - ADAUSDT BTCUSDT APTUSDT");
            eventUse = EventUse.CheckPrice;
            break;
        case "/checkpriceend":
            await bot.SendTextMessageAsync(msg.Chat,
                "Функция вызова цена была отменена");
            eventUse = EventUse.NoEvent;
            break;
        case "/notif":
            await bot.SendTextMessageAsync(msg.Chat,
                "Введите криптовалютную пару и затем цену оповещения через пробел\nПример использования: ADAUSDT 0.3532");
            eventUse = EventUse.Notif;
            break;
        case "/notifend":
            await bot.SendTextMessageAsync(msg.Chat,
                "Функция оповещения была отменена");
            cancellationTokenSource.Cancel(); //Запускаем токен отмены
            eventUse = EventUse.NoEvent;
            break;
        case "/signals":
            await bot.SendTextMessageAsync(msg.Chat, "Введите криптовалютную пару и затем силу рынка через проблел\nПримечание есть" +
                "три силы (активности рынка) 1-3, 1-Рынок активен 3-Очень активный рынок (иногда это просто сильные импульсы\n" +
                "Пример запроса - BTCUSDT 2");
            eventUse = EventUse.Signals;
            break;
        case "/signalsend":
            await bot.SendTextMessageAsync(msg.Chat, "Функция генерации рыночных сигналов была отклчюена");
            eventUse = EventUse.NoEvent;
            break;
        default:
            // Если команда не распознана, отправляем сообщение о неизвестной команде.
            await bot.SendTextMessageAsync(msg.Chat, "Unknown command.");
            break;
    }
}

enum EventUse
{
    CheckPrice,
    Notif,
    Signals,
    NoEvent
}






