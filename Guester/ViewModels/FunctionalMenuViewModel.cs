using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using Guester.Views;
using Mopups.Services;
using Guester.Models;
using Realms;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Guester.ViewModels
{
    public partial class FunctionalMenuViewModel : BaseViewModel
    {
        [ObservableProperty]
        HomePageViewModel homePageViewModel;
        public FunctionalMenuViewModel()
        {
            homePageViewModel=HomePageViewModel.getInstance();
        }


        [RelayCommand]
        internal async Task Close()
        {
            WeakReferenceMessenger.Default.Send(new Helpers.CloseFunctionMenu("Close_"));
            //await MopupService.Instance.PopAsync();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task XReport()
        {
            var html = "<!DOCTYPE html>\n<html>\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Отчёт о смене</title>\n    <style>body {\n        font-family: Arial, sans-serif;\n        text-align: center;\n    }\n    \n    .report {\n        margin: 20px;\n        padding: 20px;\n        border: 1px solid #333;\n        text-align: left; /* Выравнивание содержимого блока влево */\n    }\n    \n    h1 {\n        font-size: 24px;\n    }\n    \n    hr {\n        border: 0;\n        border-top: 1px solid #333;\n        margin: 10px 0;\n    }\n    \n    .section {\n        margin-top: 20px;\n    }\n    \n    h2 {\n        font-size: 18px;\n        margin-bottom: 10px;\n    }\n    \n    ul {\n        list-style: none;\n        padding: 0;\n    }\n    \n    li {\n        margin: 5px 0;\n    }\n    \n    p {\n        font-size: 16px;\n    }\n    \n    /* Выравнивание фигурных скобок влево */\n    .report p {\n        display: flex;\n        justify-content: space-between;\n    }\n    </style>\n</head>\n<body>\n    <div class=\"report\">\n        <h1>Guester</h1>\n        <p>{Devise number}  {Devise name}</p>\n        <hr>\n        <p>X-отчёт № 0001. [type]</p>\n        <hr>\n        <div class=\"section\">\n            <h2>Закрытие смены:</h2>\n            <p>{Дата и время закрытия смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Начало смены:</h2>\n            <p>{Дата и время начала смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Виды оплаты:</h2>\n            <ul>\n                <li>Наличный расчёт: {Сумма наличных}</li>\n                <li>Безналичный расчёт: {Сумма безналичных}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Итоги операций:</h2>\n            <ul>\n                <li>Продажи: {Сумма продаж}</li>\n                <li>Возвраты: {Сумма возвратов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Всего документов:</h2>\n            <ul>\n                <li>Продажных: {Количество продажных документов}</li>\n                <li>Возвратных: {Количество возвратных документов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Наличность в кассе:</h2>\n            <p>{Сумма наличности в кассе}</p>\n        </div>\n        <hr>\n        <div class=\"section\">\n            <h2>Сменный итог:</h2>\n            <p>{Сумма сменного итога}</p>\n        </div>\n    </div>\n</body>\n</html>";
            await DialogService.ShowHtmlAsync(html);
        }

        [RelayCommand]
        private async Task ZReport()
        {
            var html = "<!DOCTYPE html>\n<html lang=\"en\">\n  <head>\n    <meta charset=\"utf-8\" />\n    <title>jsPDF - Create PDFs with HTML5 JavaScript Library</title>\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\n    <meta name=\"description\" content=\"\" />\n    <meta name=\"author\" content=\"\" />\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js\"></script>\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js\"></script>\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js\"></script>\n  </head>\n\n  <body>\n    <h1 style=\"color: green\">Специально для Александра!</h1>\n    <h3>HTML в PDF с помощью библиотеки jsPDF</h3>\n    <div class=\"container\">\n      <input\n        type=\"button\"\n        value=\"Конвертировать HTML в PDF\"\n        onclick=\"convertHTMLtoPDF()\"\n      />\n      <div id=\"divID\">\n        <div class=\"\">\n          <h1>Learning Computer Science</h1>\n\n          <p class=\"\">CPP:<br />My first implementation was in this</p>\n          <p class=\"\">ALGO:<br />Algorithms are fun</p>\n          <p class=\"\">TYPESCRIPT:<br />New technology</p>\n          <p class=\"\">JAVASCRIPT:<br />Client side programming</p>\n        </div>\n      </div>\n    </div>\n\n    <script type=\"text/javascript\">\n      function convertHTMLtoPDF() {\n        const { jsPDF } = window.jspdf;\n\n        let doc = new jsPDF(\"l\", \"mm\", [1500, 1400]);\n        let pdfjs = document.querySelector(\"#divID\");\n\n        doc.html(pdfjs, {\n          callback: function (doc) {\n            doc.save(\"newpdf.pdf\");\n          },\n          x: 12,\n          y: 12,\n        });\n      }\n    </script>\n  </body>\n</html>\n";
            await DialogService.ShowHtmlAsync(html);
        }

        //[RelayCommand]
        //private async Task CloseCashRegistor()
        //{
        //    ;
        //   // await DialogService.ShowCaseRegisterView("Остаток денег в кассе после инкассации");
        //}

        [RelayCommand]
        private void ConnectedToDevice()
        {

        }

    }
}

