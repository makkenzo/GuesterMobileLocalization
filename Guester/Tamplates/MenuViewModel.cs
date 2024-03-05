using System;
namespace Guester.Tamplates
{
	public class MenuViewModel: BaseViewModel
	{
		string x_report = "<!DOCTYPE html>\n<html>\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Отчёт о смене</title>\n    <style>body {\n        font-family: Arial, sans-serif;\n        text-align: center;\n    }\n    \n    .report {\n        margin: 20px;\n        padding: 20px;\n        border: 1px solid #333;\n        text-align: left; /* Выравнивание содержимого блока влево */\n    }\n    \n    h1 {\n        font-size: 24px;\n    }\n    \n    hr {\n        border: 0;\n        border-top: 1px solid #333;\n        margin: 10px 0;\n    }\n    \n    .section {\n        margin-top: 20px;\n    }\n    \n    h2 {\n        font-size: 18px;\n        margin-bottom: 10px;\n    }\n    \n    ul {\n        list-style: none;\n        padding: 0;\n    }\n    \n    li {\n        margin: 5px 0;\n    }\n    \n    p {\n        font-size: 16px;\n    }\n    \n    /* Выравнивание фигурных скобок влево */\n    .report p {\n        display: flex;\n        justify-content: space-between;\n    }\n    </style>\n</head>\n<body>\n    <div class=\"report\">\n        <h1>Guester</h1>\n        <p>{Devise number}  {Devise name}</p>\n        <hr>\n        <p>X-отчёт № 0001. [type]</p>\n        <hr>\n        <div class=\"section\">\n            <h2>Закрытие смены:</h2>\n            <p>{Дата и время закрытия смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Начало смены:</h2>\n            <p>{Дата и время начала смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Виды оплаты:</h2>\n            <ul>\n                <li>Наличный расчёт: {Сумма наличных}</li>\n                <li>Безналичный расчёт: {Сумма безналичных}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Итоги операций:</h2>\n            <ul>\n                <li>Продажи: {Сумма продаж}</li>\n                <li>Возвраты: {Сумма возвратов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Всего документов:</h2>\n            <ul>\n                <li>Продажных: {Количество продажных документов}</li>\n                <li>Возвратных: {Количество возвратных документов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Наличность в кассе:</h2>\n            <p>{Сумма наличности в кассе}</p>\n        </div>\n        <hr>\n        <div class=\"section\">\n            <h2>Сменный итог:</h2>\n            <p>{Сумма сменного итога}</p>\n        </div>\n    </div>\n</body>\n</html>";

        public MenuViewModel()
		{

			x_report = string.Empty;
		}
	}
}

