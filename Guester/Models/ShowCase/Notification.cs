using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models;
public class Notification : RealmObject
{

    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [MapTo("salepoint_id")]
    public string SalesPointId { get; set; }

    [MapTo("emp_id")]
    public Employer CurrentEmp { get; set; }

    [MapTo("title")]
    public string Title { get; set; }


    [MapTo("text")]
    public string Text { get; set; }



    /// <summary>
    /// ?
    /// </summary>
    [MapTo("baground_color")]
    public string BackgroundColor { get; set; } = "#ff0000";
    [MapTo("icon_color")]
    public string IconColor { get; set; } = "#ffffff";

    [MapTo("icon")]
    public string Icon { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }


    [MapTo("is_notify")]
    public bool IsNotify { get; set; } = true;


    [Ignored]
    public DateTime CreationDateTime { get => CreationDate.DateTime; }


    }

