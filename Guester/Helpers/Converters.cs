using Realms.Sync;
using System.Globalization;
namespace Guester.Helpers
{
    public class ImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return "noimage.png";
            var imageUri = value?.ToString();
            if (imageUri.Contains("http"))
            {
                UriImageSource uriImageSource = new UriImageSource();
                uriImageSource.Uri = new Uri(imageUri);
                uriImageSource.CachingEnabled = true;

                return uriImageSource;
            }
            else
            {
                return imageUri;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class UserToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            try
            {
                if (value is Employer user && (user.Post == null || user.Post.IsAdministrator))
                {
                    return true;
                }
                return false;
            }
            catch
            {

                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
    
    
    public class CashRegisterTypToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            try
            {
                if (value is CashRegisterType type && type==CashRegisterType.CashRegister)
                {
                    return true;
                }
              return false;
            }
            catch(Exception ex) 
            {

                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is CashRegisterType type && type == CashRegisterType.CashRegister)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }

    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return Colors.Transparent;


                if (value is OrderSale type && type.GuestIndex == (int)parameter)
                {
                    return Colors.Red; // Возвращаем цвет красный
                }
                return Colors.Blue; // Возвращаем цвет синий
            }
            catch (Exception ex)
            {
                // Обработка ошибок, возвращаем цвет синий
                return Colors.Blue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;
                if (value is Color color && color == Colors.Red)
                {
                    return true; // Если цвет красный, возвращаем true
                }
                return false; // Если цвет не красный, возвращаем false
            }
            catch (Exception ex)
            {
                // Обработка ошибок, возвращаем false
                return false;
            }
        }
    }



    public class StringNotNullOrWhiteSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            try
            {
                if (!string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return true;
                }
                return false;
            }
            catch
            {

                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class CompactCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long || value is int ||value is decimal)
            {
                decimal number = Math.Truncate( System.Convert.ToDecimal(value));

                

                if (number >= 1_000_000_000_000_000_000_000_000_000m)
                {
                    return $"{Math.Truncate(number / 1_000_000_000_000_000_000_000_000_000m)}O {Math.Truncate(number % 1_000_000_000_000_000_000_000_000_000m/1_000_000_000_000_000_000_000_000m)}" +
                        $"S2 {Math.Truncate(number % 1_000_000_000_000_000_000_000_000m / 1_000_000_000_000_000_000_000m)}S {Math.Truncate(number % 1_000_000_000_000_000_000_000m / 1_000_000_000_000_000_000m)}Q2 " +
                        $"{Math.Truncate(number % 1_000_000_000_000_000_000 / 1_000_000_000_000_000m)}Q {Math.Truncate(number % 1_000_000_000_000_000m / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                        $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000_000_000_000_000_000m)
                {
                    return $" {Math.Truncate(number / 1_000_000_000_000_000_000_000_000m)}" +
                        $"S2 {Math.Truncate(number % 1_000_000_000_000_000_000_000_000m / 1_000_000_000_000_000_000_000m)}S {Math.Truncate(number % 1_000_000_000_000_000_000_000m / 1_000_000_000_000_000_000m)}Q2 " +
                        $"{Math.Truncate(number % 1_000_000_000_000_000_000 / 1_000_000_000_000_000m)}Q {Math.Truncate(number % 1_000_000_000_000_000m / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                        $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000_000_000_000_000m)
                {
                    return 
                        $"{Math.Truncate(number / 1_000_000_000_000_000_000_000m)}S {Math.Truncate(number % 1_000_000_000_000_000_000_000m / 1_000_000_000_000_000_000m)}Q2 " +
                        $"{Math.Truncate(number % 1_000_000_000_000_000_000 / 1_000_000_000_000_000m)}Q {Math.Truncate(number % 1_000_000_000_000_000m / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                        $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000_000_000_000)
                {
                    return
                        $" {Math.Truncate(number / 1_000_000_000_000_000_000m)}Q2 " +
                        $"{Math.Truncate(number % 1_000_000_000_000_000_000 / 1_000_000_000_000_000m)}Q {Math.Truncate(number % 1_000_000_000_000_000m / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                        $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000_000_000)
                {
                    return  $"{Math.Truncate(number  / 1_000_000_000_000_000m)}Q {Math.Truncate(number % 1_000_000_000_000_000m / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                        $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000_000)
                {
                    return $"{Math.Truncate(number  / 1_000_000_000_000m)}T {Math.Truncate(number % 1_000_000_000_000 / 1_000_000_000m)}B " +
                       $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000_000)
                {
                    return $" {Math.Truncate(number  / 1_000_000_000m)}B " +
                     $" {Math.Truncate(number % 1_000_000_000 / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}";
                }
                else if (number >= 1_000_000)
                {
                    return $"{Math.Truncate(number / 1_000_000)}M {((number % 1_000_000) / 1_000 != 0 ? $"{(number % 1_000_000) / 1_000}k" : "")}"
;
                }
                else if (number >= 1_000)
                {
                    return $"{Math.Truncate(number / 1_000)}k {(number % 1_000 != 0 ? $"{(number % 1_000):000}" : "")}";
                }
                else
                {
                    return $"{number}";
                }

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
