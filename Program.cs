
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Npgsql;
using System.Data;
using Telegram.Bot.Types.ReplyMarkups;
using NpgsqlTypes;
using System.Text.RegularExpressions;

namespace MyBot
{
    class Program
    {
        public static string connString = "Host=<ваш хост>;Port=<ваш порт>;username=postgres;Password=<пароль>;Database=<название базы данных>";
        public static int sum_ball=0, ball_dost = 0, prohodn_ball, exam, user_ball, currentPage = 0, currentPage2 = 0, totalPages = 5, totalPages2 = 10;
        private static List<string> viewedExams = new();
        private static List<string> viewedAchiv = new();
        private static int id_achivnment = 0;
        private static List<int> lstExam = new();
        private static bool block = true;
        private static List<long> adminIds = new List<long>() {  };//в фигурных скобках указать ваш тг id, который можно узнать с помощью Get My Id бота в Telegram
        private static string pass;

       
        private static void Main()
        {
            var client = new TelegramBotClient("токен бота");
            client.StartReceiving(Update, Error);
            
            sum_ball = 0;
            ball_dost = 0;
            Console.WriteLine("Бот запущен");
            Console.ReadLine();
        }

        private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception.ToString()));
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            string textError = "Простите, я не понимаю о чем вы. Вот чем я могу вам помочь:\n/list - Переход к спискам поступающих\n/info_sstu - Информация о СГТУ\n" +
                "/ind_dost - Учитываемые индивидуальные достижения\n/priem_com - Информация о приемной кампании" +
                "\n/kurs - Информация о курсах для абитуриентов\n/institut - Информация об институтах\n/calc - Расчет проходного балла, с учетом (без) инд. достижений" +
                "\n/activity - Информация о внеучебной деятельности\n/guide - Как поступить в СГТУ?";
            var message = update.Message;
            
           
            if (message != null)
            {
 
                switch (message.Type)
                {
                    case MessageType.Text:

                        //TODO: продолжение админ части
                        if (getStatusUser(message.From.Id) == "disabled") {
                           await botClient.SendTextMessageAsync(message.Chat.Id, "У вас нет доступа к этому боту.", cancellationToken: token);
                           return;
                        }
                        if (message.Text.StartsWith("/"))
                        {
                            // Вырезаем название команды
                            string commandName = message.Text.Substring(1);

                            
                            switch (commandName)
                            {
                                case "admin":

                                    //TODO: начало админ части
                                    NpgsqlConnection conn1 = new NpgsqlConnection(connString);

                                    conn1.Open();
                                    var cmd = new NpgsqlCommand("select DISTINCT password from users where tg_user_id = @tg", conn1);

                                    cmd.Parameters.AddWithValue("tg", adminIds[0]);


                                    pass = (string)cmd.ExecuteScalar();


                                    if (adminIds.Contains(message.From.Id))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Привет. 🔑 Введи пожалуйста пароль", replyMarkup: new ReplyKeyboardRemove());

                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ты не админ, извини", replyMarkup: new ReplyKeyboardRemove());
                                    }

                                    break;
                                case "start":

                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Добрый день! Я бот, который помогает абитуриентам, которые хотят поступить в Саратовский Государственный Технический Университет имени Гагарина Ю.А.\n" +
                                "С моей помощью вы можете найти ответы на вопросы касательно поступления.\n" +
                                "Для вызова краткой инструкции введите команду /help", replyMarkup: new ReplyKeyboardRemove());

                                    break;
                                case "list":

                                    var currentYear = DateTime.Now.Year;
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Разумеется. Вот ссылка на списки поступивших в " + currentYear + " году\nhttps://abitur.sstu.ru", replyMarkup: new ReplyKeyboardRemove());

                                    break;
                                case "guide":

                                    
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вот что нужно сделать, чтобы поступить в СГТУ:\n" +
                                        "1. Сдать ЕГЭ, если вы поступаете после 11 класса или сдать экзамены внутри ВУЗа, если вы поступаете после СПО;\n" +
                                        "2. Подать документы одним из способов:\n" +
                                        "   a) Лично в приемной комиссии;\n" +
                                        "       Информация для консультации абитуриентов по всем вопросам находится по данной ссылке: https://www.sstu.ru/abiturientu/kontakty/\n" +
                                        "   b) В электронной форме;\n" +
                                        "   c) По Почте России;\n" +
                                        "       Информация о порядке подачи заявления и необходимых документов находится по данной ссылке: https://www.sstu.ru/abiturientu/v-o/2024/podat-dokumenty-pochtoy-rossii/ \n" +
                                        "   d) Через Госуслуги. \n" +
                                        "       Более подробная информация находится здесь: https://www.gosuslugi.ru/vuzonline\n" +
                                        "3. Проследить за своим положением в конкурсных списках;\n" +
                                        "4. Принести оригинал документов;\n" +
                                        "5. Если вы зачислены, то примите мои поздравления и приходите сначала на собрание, о дате которого вам сообщат дополнительно, " +
                                        "а затем 1 сентября на линейку. Удачной учебы!", replyMarkup: new ReplyKeyboardRemove());
                                    
                                    break;
                                case "ind_dost":
                                    string messageText = "";
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вот какие достижения учитываются при поступлении в наш ВУЗ. Обратите пожалуйста внимания, что баллы учитываются только в том случае, если " +
                                     "вместе с пакетом основных документов, вы предоставили оригинал документа, " +
                                     "который подтверждает данный статус и копию данного документа, которая заверяется в приемной комиссии СГТУ им.Гагарина Ю.А.", replyMarkup: new ReplyKeyboardRemove());

                                    var conn = new NpgsqlConnection(connString);
                                    conn.Open();
                                    cmd = new NpgsqlCommand("SELECT * from individ_achivnment", conn);
                                    var reader = cmd.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        messageText = reader.GetString(1) + " " + reader.GetInt32(2) + " балла(ов)";
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: new ReplyKeyboardRemove());
                                    }
                                    break;
                                case "info_sstu":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста, что вам хочется узнать о нашем ВУЗе", replyMarkup: getButtons());
                                break;
                                case "help":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Подсказки по использованию бота:\n" +
                                                                        "1) Для просмотра команд используйте меню с командами;\n" +
                                                                        "2) Для получения ответа вы можете написать в чат сообщение или отправить команду в чат.", replyMarkup: new ReplyKeyboardRemove());
                                break;
                                case "institut":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста интересующий вас институт", replyMarkup: getInstitute());
                               break;
                                case "activity":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "В нашем ВУЗе есть несколько векторов развития, например:\n" +
                                   "1) Студенческий клуб, в котором можно попробовать себя в вокале, игре на различных музыкальных инструментах, " +
                                   "написании сценариев для праздников, создании сценических костюмов, оформлении декораций и т.п. \n" +
                                   "Вот ссылка: https://www.sstu.ru/upravlenie/upravleniya-i-otdely/upmp/centr-tvorchestva-studentov/\n" +
                                   "2) На кафедре 'Физическая культура и спорт' есть спортивные секции, такие как, Баскетбол, Волейбол, Борьба (самбо, дзюдо, греко-римская), Бадминтон, Гандбол, " +
                                   "Гиревой спорт, Дартс, Легкая атлетика, Настольный теннис и др. \n" +
                                   "Вот ссылка на расписание: https://www.sstu.ru/obrazovanie/instituty/sei/struktura/fks/sportivno-masovaya-rabota/sportivnye-sektsii-dlya-studentov.php\n" +
                                   "В нашем ВУЗе каждый найдет себя в чем-то. Так что, дерзай!", replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "priem_com":
                                     messageText = "Центральная приемная комиссия находится по адресу 10054, г. Саратов, ул. Б. Садовая, 127, СГТУ\n\n" +
                                                                        "Телефоны для связи: +7 (8452) 99-86-65, +7 (8452) 99-86-66\n\n" +
                                                                        "E-mail: cpk@sstu.ru\n\n" +
                                                                        "Время работы: Пн-Пт: 9:00-18:00\r\n\r\nСб: 9:00-15:00.\n" +
                                                                        "Также, если Вы хотите узнать больше, то можете перейти по данной ссылке https://www.sstu.ru/abiturientu/kontakty/\n" +
                                                                        "На данной странице сайта находятся контакты институтов для связи по вопросам поступления, заключения договоров на обучение, для поступления в аспирантуру и т.п.\n\n" +
                                                                        "Сроки приемной кампании\n" +
                                                                        "1) 20 июня стартует прием документов\n" +
                                                                        "2) 9 июля заканчивается прием документов на направления 'Архитектура' и 'Дизайн архитектурной среды'\n" +
                                                                        "3) 19 июля завершается прием документов на все формы обучения на бюджетные места по экзаменам СГТУ (Для выпускников колледжей, техникумов и т.д. имеющих СПО)\n" +
                                                                        "4) 25 июля завершается прием на все формы обучения на бюджетные места по результатам ЕГЭ\n" +
                                                                        "5) 27 июля публикация конкурсных списков на бюджетные места всех форм обучения\n" +
                                                                        "6) 28 июля в 12:00 по МСК завершение приема оригиналов документов об образовании на ЕПГУ для поступающих без вступительных испытаний и в пределах квот\n" +
                                                                        "7) 29 июля Формируется и выставляется Приказ о зачислении поступающих без вступительных испытаний и в пределах квот\n" +
                                                                        "8) 3 августа в 12:00 по МСК завершение приема оригиналов документов об образовании и отметок на ЕПГУ на основном этапе на бюджет на все формы\n" +
                                                                        "9) с 4 по 9 августа Формируется и выставляется Приказ о зачислении поступающих на бюджет на основном этапе\n" +
                                                                        "10) 20 августа Завершается прием документов от поступающих на платные места\n" +
                                                                        "11) 24 августа Публикуются списки, завершается прием оригиналов документов об образовании и заключения договоров на платные места на очную форму обучения\n" +
                                                                        "12) 26 августа Формируется и выставляется Приказ о зачислении поступающих на платные места на очное обучение\n" +
                                                                        "13) 27 августа Публикуются конкурсные списки, завершается прием оригиналов документов об образовании и заключения договоров на платные места заочную и очно-заочную формы обучения\n" +
                                                                        "14) 29 августа Формируется и выставляется Приказ о зачислении поступающих на платные места заочную и очно-заочную формы обучения\n" +
                                                                        "15) 1 сентября - начало учебного года\n";
                                    await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "calc":
                                     messageText = "Выберите пожалуйста, есть ли у Вас какие - либо индивидуальные достижения. Узнать какие достижения принимаются в СГТУ можно с помощью команды /ind_dost";
                                    await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: getIndDost());
                                    sum_ball = 0;
                                    ball_dost = 0;
                                    break;
                                case "kurs":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Подготовительные курсы осуществляют комплексную подготовку к прохождению государственной итоговой аттестации (ОГЭ, ЕГЭ) обучающихся 9, 10, 11 классов, техникумов, колледжей, а также подготовку к внутренним вступительным экзаменам. \n" +
                                                                    "Занятия  проводят преподаватели  вуза, знакомые с системой проведения и критериями оценки ЕГЭ.\n" +
                                                                    "По данной ссылке Вы можете узнать всю информацию о данных курсах: https://www.sstu.ru/obrazovanie/instituty/iddo/struktura/tsentr-dovuzovskoy-podgotovki/podgotovitelnye-kursy/", replyMarkup: new ReplyKeyboardRemove());
                                    break;
                            }
                        }
                        else {

                            if (message.Text.ToLower().Contains("привет") || message.Text.ToLower().Contains("здравствуйте") || message.Text.ToLower().Contains("хай"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Добрый день! Я бот, который помогает абитуриентам, которые хотят поступить в Саратовский Государственный Технический Университет имени Гагарина Ю.А.\n" +
                                    "С моей помощью вы можете найти ответы на вопросы касательно поступления.\n" +
                                    "Для вызова краткой инструкции введите команду /help", replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            //TODO:вот тут выводится ссылка на список поступивших
                            if (message.Text.ToLower().Contains("поступили") || message.Text.ToLower().Contains("поступившие"))
                            {
                                var currentYear = DateTime.Now.Year;
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Разумеется. Вот ссылка на списки поступивших в " + currentYear + " году\nhttps://abitur.sstu.ru", replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            //TODO: вот тут гайд как поступить
                            if (message.Text.ToLower().Contains("как поступить") || message.Text.ToLower().Contains("как поступить в ВУЗ?"))
                            {
                                
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Вот что нужно сделать, чтобы поступить в СГТУ:\n" +
                                    "1. Сдать ЕГЭ, если вы поступаете после 11 класса или сдать экзамены внутри ВУЗа, если вы поступаете после СПО;\n" +
                                    "2. Подать документы одним из способов:\n" +
                                    "   a) Лично в приемной комиссии;\n" +
                                    "       Информация для консультации абитуриентов по всем вопросам находится по данной ссылке: https://www.sstu.ru/abiturientu/kontakty/\n" +
                                    "   b) В электронной форме;\n" +
                                    "   c) По Почте России;\n" +
                                    "       Информация о порядке подачи заявления и необходимых документов находится по данной ссылке: https://www.sstu.ru/abiturientu/v-o/2024/podat-dokumenty-pochtoy-rossii/ \n" +
                                    "   d) Через Госуслуги. \n" +
                                    "       Более подробная информация находится здесь: https://www.gosuslugi.ru/vuzonline\n" +
                                    "3. Проследить за своим положением в конкурсных списках;\n" +
                                    "4. Принести оригинал документов;\n" +
                                    "5. Если вы зачислены, то примите мои поздравления и приходите сначала на собрание, о дате которого вам сообщат дополнительно, " +
                                    "а затем 1 сентября на линейку. Удачной учебы!", replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            //TODO: вот тут про то какие индивидуальные достижения принимаются в сгту
                            if (message.Text.ToLower().Contains("какие индивидуальные достижения учитываются в сгту?") || message.Text.ToLower().Contains("сколько баллов я могу получить, если у меня")|| message.Text.ToLower().Contains("индивидуальные достижения"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Вот какие достижения учитываются при поступлении в наш ВУЗ. Обратите пожалуйста внимания, что баллы учитываются только в том случае, если " +
                                    "вместе с пакетом основных документов, вы предоставили оригинал документа, " +
                                    "который подтверждает данный статус и копию данного документа, которая заверяется в приемной комиссии СГТУ им.Гагарина Ю.А.", replyMarkup: new ReplyKeyboardRemove());

                                using var conn = new NpgsqlConnection(connString);
                                conn.Open();
                                using var cmd = new NpgsqlCommand("SELECT * from individ_achivnment", conn);
                                using var reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    string messageText = reader.GetString(1) + " " + reader.GetInt32(2) + " балла(ов)";
                                    await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: new ReplyKeyboardRemove());
                                }
                                return;
                            }
                            //TODO: вот тут про политех и дни открытых дверей 
                            if (message.Text.ToLower().Contains("о политехе") || message.Text.ToLower().Contains("об сгту") || message.Text.ToLower().Contains("о вузе") || message.Text.Contains("о СГТУ"))
                            {

                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста, что вам хочется узнать о нашем ВУЗе", replyMarkup: getButtons());
                                return;

                            }
                            if (message.Text.ToLower().Contains("помоги") || message.Text.ToLower().Contains("помощь"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Подсказки по использованию бота:\n" +
                                    "1) Для просмотра команд используйте меню с командами;\n" +
                                    "2) Для получения ответа вы можете написать в чат сообщение или отправить команду в чат.", replyMarkup: new ReplyKeyboardRemove());
                            }
                            if (message.Text.ToLower() == "про дни открытых дверей" || message.Text.ToLower().Contains("о днях") || message.Text.ToLower().Contains("дни"))
                            {
                                string messageText = "В ближайшее время, дни открытых дверей проводятся в следующих институтах:\n";
                                using var conn = new NpgsqlConnection(connString);
                                conn.Open();
                                using (var cmd = new NpgsqlCommand("SELECT name_institute, date_open, time_open, phone, place_open from day_open_door, institute where date_open>=@TodayDate and day_open_door.id_institute = institute.id_institute", conn))
                                {

                                    cmd.Parameters.AddWithValue("TodayDate", NpgsqlDbType.Date, DateTime.Now.Date);
                                    using var reader = cmd.ExecuteReader();
                                    while (reader.Read())
                                    {

                                        messageText += reader.GetString(0) + "\n" + reader.GetDateTime(1).ToShortDateString() + " " + reader.GetString(2) + "\nТелефон для связи - " + reader.GetString(3) + " \nМесто встречи - " + reader.GetString(4) + "\n\n";
                                    }
                                }

                                await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;

                            }

                            if (message.Text.Contains("Про ВУЗ (контакты)"))
                            {

                                using var conn = new NpgsqlConnection(connString);
                                conn.Open();
                                using var cmd = new NpgsqlCommand("SELECT address from university", conn);
                                using var reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    string messageText = "Наш ВУЗ находится по адресу " + reader.GetString(0) + "\nТелефоны отделов управления каждого института представлены ниже\n" +
                                        "ИнЭН 99-87-58\n" +
                                        "ИММТ 99-88-61\n" +
                                        "ИнЭТиП 99-88-58\n" +
                                        "ИнПИТ 99-87-16\n" +
                                        "ФТИ 99-86-49\n" +
                                        "УРБАС 99-88-94\n" +
                                        "СЭИ 21-17-55\n" +
                                        "Приемная ректора\r\n99-88-11, 99-88-22\r\n\r\n99-88-10 (факс)\r\n\r\nrectorat@sstu.ru\r\n\r\n1 корпус, комната 211\n" +
                                        "Ректор - Сергей Юрьевич Наумов";
                                    await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                }
                                return;
                            }
                            //TODO: вот тут про институты всё
                            if (message.Text.ToLower().Contains("институты"))
                            {

                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста интересующий вас институт", replyMarkup: getInstitute());
                                return;
                            }
                            if (message.Text.Contains("ИнЭН"))
                            {

                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "Институт энергетики (ИнЭН) обучает на следующие специальности: \n\n";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n  " +
                                    "  SELECT \r\n\t\r\n        i.name_institute,\r\n    " +
                                    "    s.name_spec AS speciality,\r\n     " +
                                    "   s.form_education AS form_of_education,\r\n    " +
                                    "    s.count_places AS places,\r\n    " +
                                    "    s.stage_education,\r\n     " +
                                    "   s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n     " +
                                    "   ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n  " +
                                    "  FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n        " +
                                    "    *\r\n        FROM speciality s\r\n        WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n    places,\r\n    stage_education,\r\n  " +
                                    "  obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n    institute_speciality_program\r\n\r\nwhere name_institute ilike '%ИнЭН%' and prohodn_ball>0", conn);
                                using var reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);

                                return;
                            }

                            if (message.Text.Contains("ИММТ"))
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "Институт машиностроения, материаловедения и транспорта (ИММТ) обучает на следующие специальности: \n\n";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n  " +
                                    "  SELECT \r\n\t\r\n        i.name_institute,\r\n    " +
                                    "    s.name_spec AS speciality,\r\n     " +
                                    "   s.form_education AS form_of_education,\r\n   " +
                                    "     s.count_places AS places,\r\n        s.stage_education,\r\n  " +
                                    "      s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n     " +
                                    "   ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n   " +
                                    " FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n       " +
                                    "     *\r\n        FROM speciality s\r\n     " +
                                    "   WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n " +
                                    "   name_institute,\r\n    speciality,\r\n    form_of_education,\r\n  " +
                                    "  places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n " +
                                    "   institute_speciality_program\r\n\r\nwhere name_institute ilike '%ИММТ%' and prohodn_ball>0", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";

                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text.Contains("ИнЭТиП"))
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "Институт электронной техники и приборостроения (ИнЭТиП) обучает на следующие специальности: \n\n";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n  " +
                                    "  SELECT \r\n\t\r\n        i.name_institute,\r\n    " +
                                    "    s.name_spec AS speciality,\r\n    " +
                                    "    s.form_education AS form_of_education,\r\n    " +
                                    "    s.count_places AS places,\r\n        s.stage_education,\r\n   " +
                                    "     s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n    FROM \r\n    " +
                                    "    institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n            *\r\n        FROM speciality s\r\n  " +
                                    "      WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n    places,\r\n  " +
                                    "  stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n " +
                                    "   institute_speciality_program\r\n\r\nwhere name_institute ilike '%ИнЭТиП%' and prohodn_ball>0", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text.Contains("ИнПИТ"))
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "Институт прикладных информационных технологий и коммуникаций (ИнПИТ) обучает на следующие специальности: \n\n";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n  " +
                                    "  SELECT \r\n\t\r\n        i.name_institute,\r\n    " +
                                    "    s.name_spec AS speciality,\r\n      " +
                                    "  s.form_education AS form_of_education,\r\n    " +
                                    "    s.count_places AS places,\r\n        s.stage_education,\r\n  " +
                                    "      s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n   " +
                                    " FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n    " +
                                    "    SELECT \r\n            *\r\n        FROM speciality s\r\n   " +
                                    "     WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n  " +
                                    "  places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n " +
                                    "   institute_speciality_program\r\n\r\nwhere name_institute ilike '%ИнПИТ%' and prohodn_ball>0", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text.Contains("ФТИ"))
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "Физико-технический институт (ФТИ) обучает на следующие специальности: \n\n";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n    " +
                                    "    i.name_institute,\r\n        s.name_spec AS speciality,\r\n    " +
                                    "    s.form_education AS form_of_education,\r\n    " +
                                    "    s.count_places AS places,\r\n        s.stage_education,\r\n    " +
                                    "    s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n  " +
                                    "  FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n     " +
                                    "   SELECT \r\n            *\r\n        FROM speciality s\r\n     " +
                                    "   WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n  " +
                                    "  places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n  " +
                                    "  institute_speciality_program\r\n\r\nwhere name_institute ilike '%ФТИ%' and prohodn_ball>0", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                          "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                          "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text == "СЭИ")
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста удобную для Вас форму обучения в институте СЭИ", replyMarkup: getFormEducationButtonS());
                                return;
                            }

                            if (message.Text == "УРБАС")
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста удобную для Вас форму обучения в институте УРБАС", replyMarkup: getFromEducationButtonUrb());
                                return;
                            }

                            if (message.Text == "очная СЭИ")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n    " +
                                    "    i.name_institute,\r\n        s.name_spec AS speciality,\r\n     " +
                                    "   s.form_education AS form_of_education,\r\n     " +
                                    "   s.count_places AS places,\r\n        s.stage_education,\r\n    " +
                                    "    s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n  " +
                                    "  FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n     " +
                                    "       *\r\n        FROM speciality s\r\n     " +
                                    "   WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n  " +
                                    "  places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n  " +
                                    "  institute_speciality_program\r\n\r\nwhere name_institute ilike '%СЭИ%' and form_of_education = 'очная' and and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                          "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                          "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;

                            }

                            if (message.Text == "заочная СЭИ")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n    " +
                                    "    i.name_institute,\r\n        s.name_spec AS speciality,\r\n    " +
                                    "    s.form_education AS form_of_education,\r\n     " +
                                    "   s.count_places AS places,\r\n     " +
                                    "   s.stage_education,\r\n     " +
                                    "   s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n  " +
                                    "  FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n      " +
                                    "      *\r\n        FROM speciality s\r\n   " +
                                    "     WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n  " +
                                    "  name_institute,\r\n    speciality,\r\n    form_of_education,\r\n    places,\r\n   " +
                                    " stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n    institute_speciality_program\r\n\r\nwhere name_institute ilike '%СЭИ%' and form_of_education = 'заочная' and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text == "очно-заочная СЭИ")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n " +
                                    "       i.name_institute,\r\n        s.name_spec AS speciality,\r\n    " +
                                    "    s.form_education AS form_of_education,\r\n        s.count_places AS places,\r\n     " +
                                    "   s.stage_education,\r\n        s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n " +
                                    "   FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n       " +
                                    "     *\r\n        FROM speciality s\r\n        WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n " +
                                    "   name_institute,\r\n    speciality,\r\n    form_of_education,\r\n    places,\r\n " +
                                    "   stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n   " +
                                    " institute_speciality_program\r\n\r\nwhere name_institute ilike '%СЭИ%' and form_of_education = 'очно-заочная' and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text == "очная УРБАС")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n        i.name_institute,\r\n   " +
                                    "     s.name_spec AS speciality,\r\n        s.form_education AS form_of_education,\r\n        s.count_places AS places,\r\n   " +
                                    "     s.stage_education,\r\n        s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n    " +
                                    "    ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n    FROM \r\n     " +
                                    "   institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n            *\r\n        FROM speciality s\r\n     " +
                                    "   WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n    name_institute,\r\n   " +
                                    " speciality,\r\n    form_of_education,\r\n    places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n  " +
                                    "  institute_speciality_program\r\n\r\nwhere name_institute ilike '%УРБАС%' and form_of_education = 'очная' and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4065)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4065)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text == "заочная УРБАС")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n   " +
                                    "     i.name_institute,\r\n        s.name_spec AS speciality,\r\n        s.form_education AS form_of_education,\r\n  " +
                                    "      s.count_places AS places,\r\n        s.stage_education,\r\n        s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n   " +
                                    "     ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n    FROM \r\n    " +
                                    "    institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n    " +
                                    "        *\r\n        FROM speciality s\r\n     " +
                                    "   WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n " +
                                    "   name_institute,\r\n    speciality,\r\n    form_of_education,\r\n  " +
                                    "  places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n  " +
                                    "  institute_speciality_program\r\n\r\nwhere name_institute ilike '%УРБАС%' and form_of_education = 'заочная' and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {
                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                if (messageText.Length > 4096)
                                {
                                    for (int i = 0; i < messageText.Length; i += 4096)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }

                            if (message.Text == "очно-заочная УРБАС")
                            {
                                using var conn = new NpgsqlConnection(connString);
                                string messageText = "";
                                conn.Open();
                                using var cmd = new NpgsqlCommand("WITH institute_speciality_program AS (\r\n    SELECT \r\n\t\r\n        i.name_institute,\r\n        s.name_spec AS speciality,\r\n        s.form_education AS form_of_education,\r\n        s.count_places AS places,\r\n        s.stage_education,\r\n        s.obrazov_program,\r\n\t\ts.prohodn_ball,\r\n        ROW_NUMBER() OVER(PARTITION BY i.name_institute ORDER BY s.obrazov_program) AS rn\r\n    FROM \r\n        institute i\r\n    CROSS JOIN LATERAL (\r\n        SELECT \r\n            *\r\n        FROM speciality s\r\n        WHERE s.id_spec = ANY(i.specialities)\r\n    ) s\r\n)\r\nSELECT \r\n    name_institute,\r\n    speciality,\r\n    form_of_education,\r\n    places,\r\n    stage_education,\r\n    obrazov_program,\r\n\tprohodn_ball\r\nFROM \r\n " +
                                    "   institute_speciality_program\r\n\r\nwhere name_institute ilike '%УРБАС%' and form_of_education = 'очно-заочная' and prohodn_ball>0\r\ngroup by name_institute,stage_education, speciality,form_of_education,places, obrazov_program, prohodn_ball", conn);
                                using var reader = cmd.ExecuteReader();

                                while (reader.Read())
                                {

                                    messageText += reader.GetString(1) + "\nФорма обучения - " + reader.GetString(2) +
                                         "\nКоличество лет обучения - " + reader.GetDouble(3) + "\nПо программе: " + reader.GetString(4) + "\nНа профиль - " + reader.GetString(5) + "\n" +
                                         "Проходной балл - " + reader.GetInt32(6) + "\n\n";
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                return;
                            }
                            if (message.Text.ToLower().Contains("чем можно заняться вне учебы?") || message.Text.ToLower().Contains("что есть интересного?")
                            || message.Text.ToLower().Contains("внеучебная деятельность"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "В нашем ВУЗе есть несколько векторов развития, например:\n" +
                                    "1) Студенческий клуб, в котором можно попробовать себя в вокале, игре на различных музыкальных инструментах, " +
                                    "написании сценариев для праздников, создании сценических костюмов, оформлении декораций и т.п. \n" +
                                    "Вот ссылка: https://www.sstu.ru/upravlenie/upravleniya-i-otdely/upmp/centr-tvorchestva-studentov/\n" +
                                    "2) На кафедре 'Физическая культура и спорт' есть спортивные секции, такие как, Баскетбол, Волейбол, Борьба (самбо, дзюдо, греко-римская), Бадминтон, Гандбол, " +
                                    "Гиревой спорт, Дартс, Легкая атлетика, Настольный теннис и др. \n" +
                                    "Вот ссылка на расписание: https://www.sstu.ru/obrazovanie/instituty/sei/struktura/fks/sportivno-masovaya-rabota/sportivnye-sektsii-dlya-studentov.php\n" +
                                    "В нашем ВУЗе каждый найдет себя в чем-то. Так что, дерзай!", replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            //TODO: вот тут про приемную кампанию
                            if (message.Text.ToLower().Contains("приемная кампания") || message.Text.ToLower().Contains("о приемной кампании"))
                            {

                                string messageText = "Центральная приемная комиссия находится по адресу 10054, г. Саратов, ул. Б. Садовая, 127, СГТУ\n\n" +
                                    "Телефоны для связи: +7 (8452) 99-86-65, +7 (8452) 99-86-66\n\n" +
                                    "E-mail: cpk@sstu.ru\n\n" +
                                    "Время работы: Пн-Пт: 9:00-18:00\r\n\r\nСб: 9:00-15:00.\n" +
                                    "Также, если Вы хотите узнать больше, то можете перейти по данной ссылке https://www.sstu.ru/abiturientu/kontakty/\n" +
                                    "На данной странице сайта находятся контакты институтов для связи по вопросам поступления, заключения договоров на обучение, для поступления в аспирантуру и т.п.\n\n" +
                                    "Сроки приемной кампании\n" +
                                    "1) 20 июня стартует прием документов\n" +
                                    "2) 9 июля заканчивается прием документов на направления 'Архитектура' и 'Дизайн архитектурной среды'\n" +
                                    "3) 19 июля завершается прием документов на все формы обучения на бюджетные места по экзаменам СГТУ (Для выпускников колледжей, техникумов и т.д. имеющих СПО)\n" +
                                    "4) 25 июля завершается прием на все формы обучения на бюджетные места по результатам ЕГЭ\n" +
                                    "5) 27 июля публикация конкурсных списков на бюджетные места всех форм обучения\n" +
                                    "6) 28 июля в 12:00 по МСК завершение приема оригиналов документов об образовании на ЕПГУ для поступающих без вступительных испытаний и в пределах квот\n" +
                                    "7) 29 июля Формируется и выставляется Приказ о зачислении поступающих без вступительных испытаний и в пределах квот\n" +
                                    "8) 3 августа в 12:00 по МСК завершение приема оригиналов документов об образовании и отметок на ЕПГУ на основном этапе на бюджет на все формы\n" +
                                    "9) с 4 по 9 августа Формируется и выставляется Приказ о зачислении поступающих на бюджет на основном этапе\n" +
                                    "10) 20 августа Завершается прием документов от поступающих на платные места\n" +
                                    "11) 24 августа Публикуются списки, завершается прием оригиналов документов об образовании и заключения договоров на платные места на очную форму обучения\n" +
                                    "12) 26 августа Формируется и выставляется Приказ о зачислении поступающих на платные места на очное обучение\n" +
                                    "13) 27 августа Публикуются конкурсные списки, завершается прием оригиналов документов об образовании и заключения договоров на платные места заочную и очно-заочную формы обучения\n" +
                                    "14) 29 августа Формируется и выставляется Приказ о зачислении поступающих на платные места заочную и очно-заочную формы обучения\n" +
                                    "15) 1 сентября - начало учебного года\n";
                                await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            if (message.Text.ToLower().Contains("рассчитать проходной") || message.Text.ToLower().Contains("как посчитать мой проходной") || message.Text.ToLower().Contains("проходной балл")
                            || message.Text.ToLower().Contains("расчет проходного балла")   || message.Text.ToLower().Contains("расчет") || message.Text.ToLower().Contains("проходной"))
                            {
                                string messageText = "Выберите пожалуйста, есть ли у Вас какие - либо индивидуальные достижения. Узнать какие достижения принимаются в СГТУ можно с помощью команды /ind_dost";
                                await botClient.SendTextMessageAsync(message.Chat.Id, messageText, replyMarkup: getIndDost());
                                sum_ball = 0;
                                ball_dost = 0;
                                return;
                            }

                            //TODO: Проверка на наличие индивидуальных достижений
                            if (message.Text == "У меня есть индивидуальные достижения")
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                            }

                            if (message.Text == "У меня нет индивидуальных достижений")
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста экзамен, который вы сдавали", replyMarkup: getExam(currentPage));
                                return;
                            }
                            //Тут про экзамены, достижения и прочее
                            switch (message.Text)
                            {
                                case "физика":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'физика'", conn);

                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                       
                                    }
                                 break;

                                case "математика":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'математика'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "информатика":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'информатика'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "история":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'история'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "литература":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'литература'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "русский язык":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'русский язык'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "обществознание":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'обществознание'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "химия":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'химия'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "биология":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'биология'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "география":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'география'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "рис. головы":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'рис. головы'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "рис. натюрморта":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'рис. натюрморта'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "профессиональное испытание":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'профессиональное испытание'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "иностранный язык":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "введите пожалуйста количество баллов, на которое вы написали данный экзамен", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_exam from exam where name_exam = 'иностранный язык'", conn);
                                        exam = (int)cmd.ExecuteScalar();
                                        lstExam.Add(exam);
                                    }
                                    break;
                                case "больше не сдавал(а)":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball, replyMarkup: new ReplyKeyboardRemove());
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы можете поступить на следующие специальности: ", replyMarkup: new ReplyKeyboardRemove());
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        string messageText = "";
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select name_spec, stage_education, obrazov_program, prohodn_ball  from speciality where @my_ball>=prohodn_ball and exams @> @my_exams and prohodn_ball>0", conn);
                                        cmd.Parameters.AddWithValue("my_ball", sum_ball);
                                        cmd.Parameters.AddWithValue("my_exams", lstExam.ToArray());

                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                messageText += "Специальность " + reader.GetString(0) +
                                                                                              "\nПрофиль - " + reader.GetString(2) + "\n" + "Проходной балл - " + reader.GetInt32(3) + "\n\n";
                                            }

                                        }
                                        if (messageText.Length > 4096)
                                        {
                                            for (int i = 0; i < messageText.Length; i += 4096)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, messageText.Substring(i, Math.Min(4096, messageText.Length - i)));
                                            }
                                        }
                                        else await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                                    }
                                    break;

                                case "Предыдущая страница":
                                    if (currentPage > 0)
                                    {
                                        currentPage--;
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста экзамен, который вы сдавали", replyMarkup: getExam(currentPage));

                                    }
                                    break;
                                case "Следующая страница":
                                    if (currentPage < totalPages - 1 || currentPage == 0)
                                    {
                                        currentPage++;
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста экзамен, который вы сдавали", replyMarkup: getExam(currentPage));

                                    }
                                    break;
                                case "Предыд. страница":
                                    if (currentPage > 0)
                                    {
                                        currentPage--;
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage));

                                    }
                                    break;
                                case "След. страница":
                                    if (currentPage2 < totalPages2 - 1 || currentPage2 == 0)
                                    {
                                        currentPage2++;
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                    }
                                    break;

                                case var text when text.Contains("Статус чемпиона, призера"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Статус чемпиона, призера%'", conn);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {

                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                break;

                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage));
                                            }
                                        }
                                    }
                                    break;

                                case var text when text.Contains("Статус чемпиона мира, Европы"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Статус чемпиона мира, Европы%'", conn);
                                        using var reader = cmd.ExecuteReader();
                                        while (reader.Read())
                                        {
                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                            }

                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }

                                    break;

                                case var text when text.Contains("Статус победителя (призера)"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Статус победителя (призера)%'", conn);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {

                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }

                                    break;

                                case var text when text.Contains("Аттестат или диплом с отличием"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn);
                                        cmd.Parameters.AddWithValue("achiv", text);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {

                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }
                                    break;

                                case var text when text.Contains("Победитель заключительного этапа всероссийской олимпиады школьников"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Победитель заключительного этапа всероссийской олимпиады школьников%'", conn);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {

                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }

                                    break;

                                case var text when text.Contains("Призер заключительного этапа всероссийской олимпиады школьников"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Призер заключительного этапа всероссийской олимпиады школьников%'", conn);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {
                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }

                                    break;

                                case var text when text.Contains("Победитель интеллектуальных и (или) творческих конкурсов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Победитель интеллектуальных и (или) творческих конкурсов%'", conn);
                                        using var reader = cmd.ExecuteReader();

                                        while (reader.Read())
                                        {

                                            id_achivnment = reader.GetInt32(0);
                                            ball_dost = reader.GetInt32(1);
                                            if ((sum_ball + ball_dost) <= 10)
                                            {
                                                sum_ball += ball_dost;
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                            }
                                        }
                                    }

                                    break;

                                case var text when text.Contains("Призер интеллектуальных и (или) творческих конкурсов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Призер интеллектуальных и (или) творческих конкурсов%'", conn))
                                        {
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("Победитель регионального этапа всероссийской олимпиады школьников"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));


                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("Призер регионального этапа всероссийской олимпиады школьников"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case var text when text.Contains("Лауреаты, победители и призеры"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Лауреаты, победители и призеры%'", conn))
                                        {
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case "заслуженный мастер спорта":
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", message.Text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case "мастер спорта международного класса":
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", message.Text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case "мастер спорта":
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", message.Text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case "кандидат в мастера спорта":
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", message.Text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));


                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case "1 спортивный разряд":
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", message.Text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));


                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    break;

                                case var text when text.Contains("25-99 часов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("100-199 часов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("200-299 часов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("более 300 часов"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like @achiv", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("Прохождение военной службы по призыву"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Прохождение военной службы по призыву%'", conn))
                                        {
                                            cmd.Parameters.AddWithValue("achiv", text);
                                            using (var reader = cmd.ExecuteReader())
                                            {

                                                while (reader.Read())
                                                {

                                                    id_achivnment = reader.GetInt32(0);
                                                    ball_dost = reader.GetInt32(1);
                                                    if ((sum_ball + ball_dost) <= 10)
                                                    {
                                                        sum_ball += ball_dost;
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    break;

                                case var text when text.Contains("Значок ГТО"):
                                    using (var conn = new NpgsqlConnection(connString))
                                    {
                                        conn.Open();
                                        using var cmd = new NpgsqlCommand("select id_type, ball_achiv from individ_achivnment where name_type like '%Значок ГТО%'", conn);

                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                id_achivnment = reader.GetInt32(0);
                                                ball_dost = reader.GetInt32(1);
                                                if ((sum_ball + ball_dost) <= 10)
                                                {
                                                    sum_ball += ball_dost;
                                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста какие индивидуальные достижения у Вас есть", replyMarkup: getIndivDostUser(currentPage2));

                                                }
                                                else
                                                {
                                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста другое достижение, если оно у вас есть", replyMarkup: getIndivDostUser(currentPage2));
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case "Больше нет достижений":
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста экзамен, который вы сдавали", replyMarkup: getExam(0));
                                    break;
                                //TODO: продолжение админ части
                                case "Заблокировать пользователя":
                                case "Разблокировать пользователя":

                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вставь в чат файл с настройками пользователя пожалуйста");
                                    break;

                                case var text when text.Contains("Добавить/Обновить данные"):
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста, что именно вы хотите изменить", replyMarkup: getInsertUpdateData());
                                    break;

                                case "Данные о специальностях":
                                case "Данные об институтах":
                                case "Данные о днях открытых дверей":

                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте пожалуйста текстовый файл с данными", replyMarkup: new ReplyKeyboardRemove());
                                    break;
                            }
                            //TODO: Тут описано про курсы
                            if (message.Text.ToLower().Contains("курсы при поступлении") || message.Text.ToLower().Contains("курсы") || message.Text.ToLower().Contains("подготовка"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Подготовительные курсы осуществляют комплексную подготовку к прохождению государственной итоговой аттестации (ОГЭ, ЕГЭ) обучающихся 9, 10, 11 классов, техникумов, колледжей, а также подготовку к внутренним вступительным экзаменам. \n" +
                                    "Занятия  проводят преподаватели  вуза, знакомые с системой проведения и критериями оценки ЕГЭ.\n" +
                                    "По данной ссылке Вы можете узнать всю информацию о данных курсах: https://www.sstu.ru/obrazovanie/instituty/iddo/struktura/tsentr-dovuzovskoy-podgotovki/podgotovitelnye-kursy/", replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            if (message.Text == pass)
                            {

                                await botClient.SendTextMessageAsync(message.Chat.Id, "👋 Привет, Админ. \n ⚙️Что будем настраивать сегодня?", replyMarkup: getOptions());

                                return;
                            }

                            if (checkInputExam(message.Text) == true)
                            {


                                user_ball = Convert.ToInt32(message.Text);
                                sum_ball += user_ball;
                                var conn2 = new NpgsqlConnection(connString);
                                conn2.Open();
                                //TODO: переписать, ибо он будет заполнять только последнее достижение, а надо, чтобы были все, наверное. Либо забить, хз пока

                                var cmd2 = new NpgsqlCommand("INSERT INTO users (tg_user_id, id_exam, ball_exam) values (@tg_user, @exam, @ball_exam)", conn2);
                                cmd2.Parameters.AddWithValue("tg_user", message.Chat.Id);
                                cmd2.Parameters.AddWithValue("exam", exam);
                                cmd2.Parameters.AddWithValue("ball_exam", user_ball);

                                //cmd2.Parameters.AddWithValue("id_achivnm", id_achivnment);
                                cmd2.ExecuteNonQuery();

                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш балл = " + sum_ball);
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пожалуйста экзамен, который вы сдавали", replyMarkup: getExam(currentPage));

                            }
                        }
                        
                     break;

                    case MessageType.Document:
                        if (getStatusUser(message.From.Id) == "disabled")
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "У вас нет доступа к этому боту.", cancellationToken: token);
                            return;
                        }
                        var document = message.Document;
                        string[] values;
                        List<string> lst = new();
                       
                        if (document.FileName.ToLower().Contains("специальности")|| document.FileName.ToLower().Contains("специальностях")) {
                            var fileInfo = await botClient.GetFileAsync(document.FileId);
                            var file = await botClient.GetFileAsync(message.Document.FileId);
                            using (var stream = new MemoryStream())
                            {
                                await botClient.DownloadFileAsync(file.FilePath, stream);
                                stream.Position = 0;
                                using var reader = new StreamReader(stream);
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    values = GetValues(line);
                                    lst.Add(values[0]);
                                }
                            }

                            if (lst.Count >= 5 && lst[0] is string specialtyName && lst[1] is string form_education && lst[2] is string stage_education &&
                               Convert.ToInt32( lst[3]) is int cost && Convert.ToInt32(lst[4]) is int ball)
                            {
                               
                                using (var conn = new NpgsqlConnection(connString))
                                {
                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand("UPDATE speciality SET cost_of_year = @cost, prohodn_ball = @ball WHERE name_spec ilike @name_spec and form_education = @form_education", conn))
                                    {
                                        cmd.Parameters.AddWithValue("form_education", form_education);
                                        cmd.Parameters.AddWithValue("name_spec", "%" + specialtyName + "%");
                                        cmd.Parameters.AddWithValue("stage_education", stage_education);
                                        cmd.Parameters.AddWithValue("@cost", cost);
                                        cmd.Parameters.AddWithValue("@ball", ball);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Данные об специальности {specialtyName}были успешно изменены");
                            }
                            
                        }

                        if (document.FileName.ToLower().Contains("институт"))
                        {
                            var fileInfo = await botClient.GetFileAsync(document.FileId);
                            var file = await botClient.GetFileAsync(message.Document.FileId);
                            using (var stream = new MemoryStream())
                            {
                                await botClient.DownloadFileAsync(file.FilePath, stream);
                                stream.Position = 0;
                                using var reader = new StreamReader(stream);
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    values = GetValues(line);
                                    lst.Add(values[0]);
                                }
                            }

                            if (lst.Count >= 3 && lst[0] is string name_institute && lst[1] is string address && Convert.ToInt32(lst[2]) is int number_building)
                            {
                                using (var conn = new NpgsqlConnection(connString))
                                {
                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand("UPDATE institute SET address = @address, number_building = @number WHERE name_institute ilike @name_institute", conn))
                                    {
                                        cmd.Parameters.AddWithValue("address", address);
                                        cmd.Parameters.AddWithValue("name_institute", "%" + name_institute + "%");
                                        cmd.Parameters.AddWithValue("number", number_building);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Данные об институте {name_institute}были успешно изменены");
                            }
                            
                        }

                        if (document.FileName.ToLower().Contains("открытых дверей"))
                        {
                            var fileInfo = await botClient.GetFileAsync(document.FileId);
                            var file = await botClient.GetFileAsync(message.Document.FileId);
                            using (var stream = new MemoryStream())
                            {
                                await botClient.DownloadFileAsync(file.FilePath, stream);
                                stream.Position = 0;
                                using var reader = new StreamReader(stream);
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    values = GetValues(line);
                                    lst.Add(values[0]);
                                }
                            }

                            if (lst.Count >= 5 && lst[0] is string name_institute && lst[1] is string date_open && lst[2] is string time_open && lst[3] is string phone && lst[4] is string place)
                            {
                                using (var conn = new NpgsqlConnection(connString))
                                {
                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand("WITH institute_id AS (\r\n    SELECT id_institute\r\n    FROM institute\r\n    WHERE name_institute ilike @name_institute" +
                                        "\r\n)\r\nINSERT INTO day_open_door (id_institute, date_open, time_open, phone, place_open)\r\nVALUES " +
                                        "((SELECT id_institute FROM institute_id), @date, @time_open, @phone, @place)\r\nON CONFLICT " +
                                        "(id_institute, date_open) \r\nDO UPDATE SET time_open = EXCLUDED.time_open, phone = EXCLUDED.phone, place_open = EXCLUDED.place_open;", conn))
                                    {
                                        cmd.Parameters.AddWithValue("date", Convert.ToDateTime(date_open));
                                        cmd.Parameters.AddWithValue("name_institute", "%" + name_institute + "%");
                                        cmd.Parameters.AddWithValue("time_open", time_open);
                                        cmd.Parameters.AddWithValue("phone", phone);
                                        cmd.Parameters.AddWithValue("place", place);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Данные о дне открытых дверей в институте {name_institute} были успешно изменены");
                            }

                        }

                        if (document.FileName.ToLower().Contains("настройки")) {
                            var fileInfo = await botClient.GetFileAsync(document.FileId);
                            var file = await botClient.GetFileAsync(message.Document.FileId);
                            using (var stream = new MemoryStream())
                            {
                                await botClient.DownloadFileAsync(file.FilePath, stream);
                                stream.Position = 0;
                                using var reader = new StreamReader(stream);
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    values = GetValues(line);
                                    lst.Add(values[0]);
                                }
                            }

                            if (lst.Count >= 2 && Convert.ToInt64(lst[0]) is long id_user  && lst[1] is string status_user)
                            {

                                using (var conn = new NpgsqlConnection(connString))
                                {
                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand("UPDATE users SET status = @status WHERE tg_user_id = @id_user", conn))
                                    {
                                        cmd.Parameters.AddWithValue("status", status_user);
                                        cmd.Parameters.AddWithValue("id_user", id_user);
                                        
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Настройки пользователя {id_user} были успешно изменены");
                            }
                        }
                        break;
                    default:
                        await botClient.SendTextMessageAsync(message.Chat.Id, textError);
                        break;
                }
            }
        }
        private static string[] GetValues(string line)
        {
            string textPattern = @"'(.*?)'"; // Регулярное выражение для поиска текстовых значений в одинарных кавычках
            string numberPattern = @"\d+"; // Регулярное выражение для поиска чисел

            MatchCollection textMatches = Regex.Matches(line, textPattern);
            MatchCollection numberMatches = Regex.Matches(line, numberPattern);

            List<string> values = new List<string>();

            foreach (Match textMatch in textMatches)
            {
                values.Add(textMatch.Groups[1].Value); // Добавляем текстовые значения в список
            }

            foreach (Match numberMatch in numberMatches)
            {
                values.Add(numberMatch.Value); // Добавляем числовые значения в список
            }

            return values.ToArray(); // Возвращаем пустой массив, если значение не найдено
        }
        private static string getStatusUser(long id_user) {

            string status = "";
            var conn = new NpgsqlConnection(connString);
            conn.Open();
            var cmd = new NpgsqlCommand("select status from users where tg_user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", id_user);
            status = (string)cmd.ExecuteScalar();
            return status;


        }
        private static IReplyMarkup? getInsertUpdateData()
        {
            var lstButton = new List<KeyboardButton>();
            lstButton.Add(new KeyboardButton("Данные о специальностях"));
            lstButton.Add(new KeyboardButton("Данные об институтах"));
            lstButton.Add(new KeyboardButton("Данные о днях открытых дверей"));

            return new ReplyKeyboardMarkup(lstButton);
        }
        private static IReplyMarkup? getOptions()
        {
            var lstOption = new List<KeyboardButton>();
            lstOption.Add(new KeyboardButton("🔒 Заблокировать пользователя"));
            lstOption.Add(new KeyboardButton("🔒 Разблокировать пользователя"));
            lstOption.Add(new KeyboardButton("📝 Добавить/Обновить данные"));
            return new ReplyKeyboardMarkup(lstOption);
        }

        private static IReplyMarkup getIndivDostUser(int currentPage, int pageSize = 2)
        {
            var allAchiv = new[] {
                "Статус чемпиона, призера Олимпийских, Паралимпийских, Сурдлимпийских игр, чемпиона мира, Европы (1 место)","Статус чемпиона мира, Европы, победителя первенства мира, Европы","Статус победителя (призера) национального и (или) международного чемпионата по профессиональному мастерству среди инвалидов и лиц с ограниченными возможностями здоровья \"Абилимпикс\"","Аттестат или диплом с отличием",
                "Победитель заключительного этапа всероссийской олимпиады школьников, олимпиад школьников, включенных в " +
                "перечни олимпиад школьников",
                "Призер заключительного этапа всероссийской олимпиады школьников, " +
                "олимпиад школьников, включенных в перечни олимпиад школьников и их уровней, " +
                "утвержденных приказами Минобрнауки РФ" +
                " (не используемые для получения особых прав и (или) особого преимущества при поступлении" +
                " на обучение по конкретным условиям поступления)",
                "Победитель интеллектуальных и (или) творческих конкурсов, физкультурных мероприятий и спортивных мероприятий " +
                "по специальностям и (или) направлениям подготовки СГТУ, соответствующим профилю мероприятия",
                "Призер интеллектуальных и (или) творческих конкурсов, физкультурных мероприятий и спортивных мероприятий " +
                "по специальностям и (или) направлениям подготовки СГТУ, соответствующим профилю мероприятия",
                "Победитель регионального этапа всероссийской олимпиады школьников в 2022/2023 и 2023/2024 учебных годах","Призер регионального этапа всероссийской олимпиады школьников в 2022/2023 и 2023/2024 учебных годах",
                "Лауреаты, победители и призеры Международной детско-юношеской премии «Экология – дело каждого» проводимой Росприроднадзором на 2022/2023 и 2023/2024 годы",
                "заслуженный мастер спорта","мастер спорта международного класса","мастер спорта","кандидат в мастера спорта", "1 спортивный разряд",
                "Волонтер 25-99 часов","Волонтер 100-199 часов","Волонтер 200-299 часов","Волонтер более 300 часов",
                "Прохождение военной службы по призыву, военной службы по контракту, " +
                "военной службы по мобилизации в ВС РФ, а также пребывание в добровольческих формированиях" +
                " в соответствии с контрактом о добровольном содействии в выполнении задач, возложенных на ВС РФ," +
                " в ходе СВО на территориях Украины, ДНР, ЛНР, Запорожской области и Херсонской области",
                "Значок ГТО, у поступающего в текущем году и (или) предшествующем году, если он относится (относился) к этой возрастной группе","Больше нет достижений"
            };
            var totalPages = (int)Math.Ceiling(allAchiv.Length / (double)pageSize);
            var start = currentPage * pageSize;
            var end = Math.Min(start + pageSize, allAchiv.Length);
            var currentAchiv = allAchiv.Skip(start).Take(end).Where(achiv => !viewedAchiv.Contains(achiv)).ToArray();
            var lstAchiv = new List<KeyboardButton[]>();
            for (int i = 0; i < currentAchiv.Length; i++)
            {
                lstAchiv.Add(new KeyboardButton[1] { new KeyboardButton(currentAchiv[i]) });
            }

            if (currentPage > 0)
            {
                lstAchiv.Add(new KeyboardButton[1] { new KeyboardButton("Предыд. страница") });
            }

            if (start + currentAchiv.Length < allAchiv.Length)
            {
                lstAchiv.Add(new KeyboardButton[1] { new KeyboardButton("След. страница") });
            }

            return new ReplyKeyboardMarkup(lstAchiv.ToArray());
        }
        
        private static IReplyMarkup getExam(int currentPage, int pageSize = 2)
        {
            var allExams = new[]
             {
                "математика", "физика", "информатика", "история", "литература",
                "русский язык", "обществознание", "химия", "биология", "география",
                 "рис. головы", "рис. натюрморта", "профессиональное испытание", "иностранный язык", "больше не сдавал(а)",
             };
            var totalPages = (int)Math.Ceiling(allExams.Length / (double)pageSize);
            var start = currentPage * pageSize;
            var end = Math.Min(start + pageSize, allExams.Length);
            var currentExams = allExams.Skip(start).Take(end).Where(exam => !viewedExams.Contains(exam)).ToArray();
            var lstExam = new List<KeyboardButton[]>();
            for (int i = 0; i < currentExams.Length; i++)
            {
                lstExam.Add(new KeyboardButton[1] { new KeyboardButton(currentExams[i]) });
            }

            if (currentPage > 0)
            {
                lstExam.Add(new KeyboardButton[1] { new KeyboardButton("Предыдущая страница") });
            }

            if (start + currentExams.Length < allExams.Length)
            {
                lstExam.Add(new KeyboardButton[1] { new KeyboardButton("Следующая страница") });
            }

            return new ReplyKeyboardMarkup(lstExam.ToArray());

        }
        private static bool checkInputExam(object message) {

            bool flag = false;
            if (int.TryParse(message.ToString(), out int number)) { 
            
                flag = true;
            }
            else flag = false;
            return flag;

        }

        
        private static IReplyMarkup? getIndDost()
        {
            var lst = new List<KeyboardButton>();
            lst.Add(new KeyboardButton("У меня есть индивидуальные достижения"));
            lst.Add(new KeyboardButton("У меня нет индивидуальных достижений"));
            return new ReplyKeyboardMarkup(lst);
        }

        private static IReplyMarkup? getFromEducationButtonUrb()
        {
            var lst = new List<KeyboardButton>();
            lst.Add(new KeyboardButton("очная УРБАС"));
            lst.Add(new KeyboardButton("заочная УРБАС"));
            lst.Add(new KeyboardButton("очно-заочная УРБАС"));
            return new ReplyKeyboardMarkup(lst);
        }

        private static IReplyMarkup? getFormEducationButtonS()
        {
            var lst = new List<KeyboardButton>();
            lst.Add(new KeyboardButton("очная СЭИ"));
            lst.Add(new KeyboardButton("заочная СЭИ"));
            lst.Add(new KeyboardButton("очно-заочная СЭИ"));
            return new ReplyKeyboardMarkup(lst);
        }

        private static IReplyMarkup? getInstitute()
        {
            var lst = new List<KeyboardButton>();
            lst.Add(new KeyboardButton("ИнЭН"));
            lst.Add(new KeyboardButton("ИММТ"));
            lst.Add(new KeyboardButton("ИнЭТиП"));
            lst.Add(new KeyboardButton("ИнПИТ"));
            lst.Add(new KeyboardButton("ФТИ"));
            lst.Add(new KeyboardButton("УРБАС"));
            lst.Add(new KeyboardButton("СЭИ"));
            return new ReplyKeyboardMarkup(lst);
        }

        private static IReplyMarkup? getButtons()
        {
            var lst = new List<KeyboardButton>();

            lst.Add(new KeyboardButton("Про дни открытых дверей"));
            lst.Add(new KeyboardButton("Про ВУЗ (контакты)"));
            return new ReplyKeyboardMarkup(lst);
        }
        
    }
}
