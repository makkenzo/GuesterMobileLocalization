using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    public class Booking :RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [MapTo("sale_point_id")]
        public string SalesPointId { get; set; } = string.Empty;

        [MapTo("guest_count")]
        public int GuestCount { get; set; }

        [MapTo("table_id")]
        public Table Table { get; set; }

        [MapTo("premises_id")]
        public Premises Premises { get; set; }

        [MapTo("date_booking")]
        public DateTimeOffset DateBooking { get; set; }= DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.TotalOffsetMinutes);

        //[MapTo("booking_duration")]
        //public float BookingDuration { get; set; } // in secound need to int

        [MapTo("comment")]
        public string Comment { get; set; }


        [MapTo("booking_client")]
        public BookingClient BookingClient { get; set; } = new();

        [MapTo("booking_durations")]
        public string Duration { get; set; }


        [Ignored]
        public string TimeBookingToVisual{ get => DateBooking.ToString("HH:mm"); }


        [Ignored]
        public string DateBookingToVisual { get => DateBooking.ToString("dd MMM yyyy"); }


        [Ignored]
        public DateTime SelectedDate { get; set; }=DateTime.Now;


        //[Ignored]
        //public string DurationVisual { get => $"{((BookingDuration / 60) / 60):0}ч {(BookingDuration / 60):0}мин"; }



    }

    public class BookingClient : EmbeddedObject
    {
        [MapTo("name")]
        public string FullName { get; set; }

        [MapTo("number")]
        public string Number { get; set; }


        [MapTo("country_code")]
        public string CountryCode { get; set; }


        [Ignored]
        public string NumberVisual { get => $"{CountryCode} {Number}"; }
    }
}
